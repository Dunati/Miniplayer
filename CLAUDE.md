# MiniPlayer

Personal Windows tool: a small, borderless, always-on-top window that hosts a WebView2
browser running web music services (Pandora, Amazon Music, Spotify). Global media keys
control playback; per-station code injects CSS/JS to crop each site down to its player.

## Code style
- Do not add comments. No narrative/explanatory comments, and never comments describing how a
  solution was reached or what changed. Only add a comment when the code is genuinely non-obvious,
  and keep it to a terse note of the fact — not a story.

## Stack
- .NET 9 (`net9.0-windows`), WinForms, `Microsoft.Web.WebView2`. Entry point: `Program.cs` → `MiniPlayer` form.
- uBlock Origin is loaded as a browser extension from `exe/data/uBlock0.chromium` (not in the repo; lives next to the built exe). The WebView2 user-data folder (login/cookies) is `exe/data/WebView2`; both sit under `exe/data/`.

## Build / Run
- Build: `dotnet build MiniPlayer.sln` (from Git Bash, use `-p:` not `/p:` switches — Git Bash mangles `/p`).
- Run/debug: F5 in VSCode (`.vscode/launch.json`) runs `bin/Debug/net9.0-windows/MiniPlayer.exe` with `cwd=exe`
  (so data files and `data/` resolve from `exe/`). Debug stays normal multi-file in `bin/Debug`; its
  PostBuild step only refreshes `exe/DefaultStations.json`. DevTools auto-opens when a debugger is attached.
- Publish (the lean standalone in `exe/`): `dotnet publish MiniPlayer.csproj -c Release -r win-x64`. Produces a
  framework-dependent single-file `MiniPlayer.exe` (all managed DLLs embedded; needs the .NET 9 desktop runtime).
  The PostPublish target mirrors the publish output into `exe/` via `copy_files.cmd`, pruning old files but
  preserving `exe/data/` (uBlock + the WebView2 login profile).
- `injector.js` and `amazon.js` are embedded resources (read via `ResourceLoader.ReadText`), not loose files.

## Architecture
- **`Miniplayer.cs`** — the form. Owns the WebView2, custom window chrome (drag/resize via
  border hit-testing in `SetCursor`/`DoResize`, double-click edges to switch station or maximize),
  favicon→window-icon, theme border in `OnPaint`, and the global keyboard hook.
- **`StationCommands` (abstract)** — one subclass per service in `Stations/`. Maps a site URI to
  `Play/Next/Previous/Like/Dislike`, a theme `Color`, and `AdjustStyle()` (CSS/JS to reshape the page).
  Subclasses self-register by reflection: `RegisterCommands()` finds every subclass and calls its
  static `Register()`, which calls `Register(uri, factory)`. Lookup is `StationCommands.Get(uri, webView)`.
- **`injector.js`** — runs on every document (`AddScriptToExecuteOnDocumentCreatedAsync`). Exposes
  `window.miniplayer.{find_element, click_element, set_properties, inject}` with shadow-DOM-piercing
  `deepQuery`, hides scrollbars, and forwards Ctrl+wheel as a postMessage for zoom.
- **`InjectionFunctions.cs`** — C# side of the bridge; serializes selectors and calls the JS via `ExecuteScriptAsync`.
- **`StationSettings` / `Station`** — the station list + current index, persisted as JSON in app Settings
  (window location/size/zoom per station). `DefaultStations.json` seeds any stations the user doesn't already have.

## Controls
- Media keys (play/pause, next, prev) and F22/F23/F24 (dislike/reload/like) via a low-level keyboard
  hook in `HookCallback`.
- Ctrl+mouse-wheel = zoom (handled both natively and via the injected wheel listener).
- Drag top edge to move; drag other edges/corners to resize; double-click right/left edge = next/prev station; double-click top = maximize.

## Adding a station
1. Add `Stations/FooCommands.cs`: subclass `StationCommands`, with `private const string uri = "https://…"`,
   a static `Register()` calling `Register(uri, v => new FooCommands(v))`, override `Uri`, set `Color` in the ctor,
   and implement the command/`AdjustStyle` methods.
2. Add the same `uri` to `DefaultStations.json` so it's seeded into the rotation.
3. **The registered URI must exactly match the navigation URI** — `Get` is a dictionary lookup by string,
   and stations without a registered command are skipped during Next/Previous cycling.

## Working on stations (selectors break often)
Sites change their DOM, breaking the CSS selectors in `AdjustStyle`/Like/Dislike. With a debugger attached,
DevTools opens automatically — inspect the live page, fix the selector, and remember `deepQuery` pierces
shadow DOM and accepts an array of selectors (each step queried inside the previous match).
