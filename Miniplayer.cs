using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows.Forms;

namespace MiniPlayer
{
    public partial class MiniPlayer : Form
    {
        StationSettings stationSettings;
        private readonly HttpClient httpClient = new HttpClient();


        private WebView2 webView;
        private bool dragging;
        private Point dragCursorPoint;
        private Point dragFormPoint;

        private bool resizing;
        private Size resizeStart;
        public MiniPlayer()
        {
            _proc = HookCallback;
            _hookID = SetHook(_proc);

            stationSettings = new();

            InitializeComponent(); 
            this.MouseWheel += MainForm_MouseWheel;
            this.ResizeRedraw = true;
            webView = new WebView2();
            InitializeWebView();
            webView.WebMessageReceived += WebView_WebMessageReceived;
            TopMost = true;

            Padding = new Padding(5);

            webView.Focus();


            // Load saved settings
            this.Load += Form1_Load;
            this.FormClosing += Form1_FormClosing;

        }

        private async void WebView_WebMessageReceived(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
        {
            if (commands == null)
                return;

            string rawMsg = e.TryGetWebMessageAsString();

            if (double.TryParse(rawMsg, out double deltaY))
            {
                if (deltaY < 0)
                {
                    await commands.ZoomIn();
                }
                else
                {
                    await commands.ZoomOut();
                }
            }
        }

        private async void MainForm_MouseWheel(object? sender, MouseEventArgs e)
        {
            if ((Control.ModifierKeys & Keys.Control) == Keys.Control && commands != null)
            {
                if (e.Delta > 0)
                {
                    await commands.ZoomIn();
                }
                else
                {
                    await commands.ZoomOut();
                }
            }
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);
        [DllImport("user32.dll")]
        private static extern short GetKeyState(int nVirtKey);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private LowLevelKeyboardProc _proc;
        private IntPtr _hookID = IntPtr.Zero;


        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int VK_CONTROL = 0x11;

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (var cur = Process.GetCurrentProcess())
            using (var curAssembly = cur.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curAssembly!.ModuleName), 0);
            }
        }

        public WebView2 GetWebView()
        {
            return webView;
        }


        public async Task HandleHotkey(string key)
        {
            /*
            switch (key)
            {
            case "F1":
                await InjectionFunctions.ClickElementAsync(
GetWebView(), "[data-qa='thumbs_up_button']",
                    "[aria-label='Like']"
                );
                break;
            case "F2":
                await InjectionFunctions.ClickElementAsync(
GetWebView(), "#contextMenuOption1",
    "music-list-item[primary-text='Dislike']"
);
                break;

            }
            */
        }

        StationCommands? commands;


        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                Keys vkCode = (Keys)Marshal.ReadInt32(lParam);

                switch (vkCode)
                {
                case Keys.MediaPlayPause:
                    commands?.Play();
                    break;
                case Keys.MediaNextTrack:
                    commands?.Next();
                    break;
                case Keys.MediaPreviousTrack:
                    commands?.Previous();
                    break;
                case Keys.F22:
                    commands?.Dislike();
                    break;
                case Keys.F23:
                    webView.Reload();
                    break;
                case Keys.F24:
                    commands?.Like();
                    break;
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            SaveSettings();
            UnhookWindowsHookEx(_hookID);
        }

        private void SaveSettings()
        {
            stationSettings.Current.location = this.Location;
            stationSettings.Current.size = this.Size;
            stationSettings.Current.zoom = commands?.Zoom ?? 1.0f;

            Settings.Default.StationSettings = JsonSerializer.Serialize(stationSettings);

            Settings.Default.Save();
        }

        private bool IsOnScreen(Point location, Size size)
        {
            Rectangle rect = new Rectangle(location, size);
            return Screen.AllScreens.Any(s => s.WorkingArea.IntersectsWith(rect));
        }

        private void Form1_Load(object? sender, EventArgs e)
        {
            string json = Settings.Default.StationSettings;

            if (!string.IsNullOrWhiteSpace(json))
            {
                try
                {
                    var state = JsonSerializer.Deserialize<StationSettings>(json);

                    if (state != null)
                    {
                        stationSettings = state;

                        if (IsOnScreen(state.Current.location, state.Current.size))
                        {
                            this.Location = state.Current.location;
                            this.Size = state.Current.size;

                            if (commands != null)
                            {
                                commands.Zoom = stationSettings.Current.zoom;
                            }
                        }
                    }
                }
                catch
                {
                }
            }
            if (File.Exists("DefaultStations.json"))
            {
                StationSettings defaults = JsonSerializer.Deserialize<StationSettings>(File.ReadAllText("DefaultStations.json"))!;

                var existing = stationSettings.GetStationIndices().Keys.ToHashSet();


                foreach ((string uri, int index) in defaults.GetStationIndices())
                {
                    if (!existing.Contains(uri))
                    {
                        stationSettings.Add(defaults[index]);
                    }
                }
            }
        }

        ActiveBorder ActiveBorder;

        int resizeBorderThickness => Padding.All;

        private async void InitializeWebView()
        {
            webView.Dock = DockStyle.Fill;

            this.Controls.Add(webView);

            var options = new CoreWebView2EnvironmentOptions
            {
                AreBrowserExtensionsEnabled = true,
                AdditionalBrowserArguments = "--autoplay-policy=no-user-gesture-required"
            };

            var env = await CoreWebView2Environment.CreateAsync(null, null, options);
            await webView.EnsureCoreWebView2Async(env);

            string injected = await File.ReadAllTextAsync("injector.js");
            await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(injected);
            try
            {

                string extensionPath = Path.GetFullPath(@".\uBlock0.chromium");
                await webView.CoreWebView2.Profile.AddBrowserExtensionAsync(extensionPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load extension: {ex.Message}");
            }

            webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            webView.CoreWebView2.Settings.IsZoomControlEnabled = false;

            webView.NavigationCompleted += WebView_NavigationCompleted;
            webView.CoreWebView2.FaviconChanged += CoreWebView2_FaviconChanged;


            MouseDown += PaddingPanel_MouseDown;
            MouseMove += PaddingPanel_MouseMove;
            MouseUp += PaddingPanel_MouseUp;

            //webView.CoreWebView2.OpenDevToolsWindow();

            StationCommands.RegisterCommands();
            var new_commands = StationCommands.Get(stationSettings.Current.uri, webView);
            if (new_commands != null)
            {
                commands = new_commands;
                webView.CoreWebView2.Navigate(stationSettings.Current.uri);
                return;
            }
            NextStation();

        }

        private async void WebView_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            await commands!.AdjustStyle();
        }

        private void Form1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.ActiveBorder == ActiveBorder.Top)
            {
                ToggleMaximize();
            }
            else if (ActiveBorder == ActiveBorder.Right)
            {

                SaveSettings();
                NextStation();
            }
            else if (ActiveBorder == ActiveBorder.Left)
            {
                SaveSettings();
                PreviousStation();
            }
        }

        private void PreviousStation()
        {
            string original = stationSettings.Current.uri;
            Station station = stationSettings.Current;
            do
            {
                station = stationSettings.PrevStation;
                var next_command = StationCommands.Get(station.uri, webView);
                if (next_command != null)
                {
                    commands = next_command;
                    webView.CoreWebView2.Navigate(station.uri);

                    this.Size = station.size;
                    this.Location = station.location;
                    commands.Zoom = stationSettings.Current.zoom;
                    break;
                }
            } while (station.uri != original);

        }

        private void NextStation()
        {
            string original = stationSettings.Current.uri;
            Station station = stationSettings.Current;
            do
            {
                station = stationSettings.NextStation;
                var next_command = StationCommands.Get(station.uri, webView);
                if (next_command != null)
                {
                    commands = next_command;
                    webView.CoreWebView2.Navigate(station.uri);

                    this.Size = station.size;
                    this.Location = station.location;
                    commands.Zoom = stationSettings.Current.zoom;
                    break;
                }
            } while (station.uri != original);

        }

        private void ToggleMaximize()
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
            }
            else
            {
                this.WindowState = FormWindowState.Maximized;
            }
        }

        private void PaddingPanel_MouseDown(object? sender, MouseEventArgs e)
        {
            // Start dragging if left mouse button is pressed
            if (e.Button == MouseButtons.Left)
            {
                if (Cursor == Cursors.SizeAll)
                {
                    BeginMove();
                }
                else
                {
                    BeginResize();
                }
            }

        }

        private void BeginResize()
        {
            resizing = true;
            dragCursorPoint = Cursor.Position;
            dragFormPoint = this.Location;
            resizeStart = this.Size;
        }

        private void BeginMove()
        {
            dragging = true;
            dragCursorPoint = Cursor.Position;
            dragFormPoint = this.Location;
        }

        private void SetCursor(MouseEventArgs e)
        {
            SetCursor(e.Location);
        }

        private void SetCursor(Point mouseLocation)
        {
            if (mouseLocation.Y <= resizeBorderThickness && (mouseLocation.X > resizeBorderThickness && mouseLocation.X < Width - resizeBorderThickness))
            {
                ActiveBorder = ActiveBorder.Top;
                this.Cursor = Cursors.SizeAll;
            }
            // Determine if the mouse is near the edges for resizing
            else if (mouseLocation.X <= resizeBorderThickness && mouseLocation.Y <= resizeBorderThickness) // Top-left corner
            {
                ActiveBorder = ActiveBorder.TopLeft;
                this.Cursor = Cursors.SizeNWSE; // Diagonal resize cursor
            }
            else if (mouseLocation.X >= this.Width - resizeBorderThickness && mouseLocation.Y <= resizeBorderThickness) // Top-right corner
            {
                ActiveBorder = ActiveBorder.TopRight;
                this.Cursor = Cursors.SizeNESW; // Diagonal resize cursor
            }
            else if (mouseLocation.X <= resizeBorderThickness && mouseLocation.Y >= this.Height - resizeBorderThickness) // Bottom-left corner
            {
                ActiveBorder = ActiveBorder.BottomLeft;
                this.Cursor = Cursors.SizeNESW; // Diagonal resize cursor
            }
            else if (mouseLocation.X >= this.Width - resizeBorderThickness && mouseLocation.Y >= this.Height - resizeBorderThickness) // Bottom-right corner
            {
                ActiveBorder = ActiveBorder.BottomRight;
                this.Cursor = Cursors.SizeNWSE; // Diagonal resize cursor
            }
            else if (mouseLocation.X <= resizeBorderThickness) // Left edge
            {
                ActiveBorder = ActiveBorder.Left;
                this.Cursor = Cursors.SizeWE; // Horizontal resize cursor
            }
            else if (mouseLocation.X >= this.Width - resizeBorderThickness) // Right edge
            {
                ActiveBorder = ActiveBorder.Right;
                this.Cursor = Cursors.SizeWE; // Horizontal resize cursor
            }
            else if (mouseLocation.Y >= this.Height - resizeBorderThickness) // Bottom edge
            {

                ActiveBorder = ActiveBorder.Bottom;
                this.Cursor = Cursors.SizeNS; // Vertical resize cursor
            }
            else
            {
                ActiveBorder = ActiveBorder.None;
                this.Cursor = Cursors.Default; // Default cursor
            }
        }

        private void PaddingPanel_MouseMove(object? sender, MouseEventArgs e)
        {
            if (dragging)
            {
                DoMove();
            }
            else if (resizing)
            {
                DoResize();
            }
            else
            {
                SetCursor(e);
            }
        }

        private void DoMove()
        {
            Point dif = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
            this.Location = Point.Add(dragFormPoint, new Size(dif.X, dif.Y));
        }

        private void DoResize()
        {
            Point dif = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));


            switch (ActiveBorder)
            {
            case ActiveBorder.None:
                break;
            case ActiveBorder.Top:
                break;
            case ActiveBorder.TopRight:
                this.Location = new Point(dragFormPoint.X, dragFormPoint.Y + dif.Y);
                this.Size = new Size(resizeStart.Width + dif.X, resizeStart.Height - dif.Y);
                break;
            case ActiveBorder.Right:
                this.Size = new Size(resizeStart.Width + dif.X, resizeStart.Height);
                break;
            case ActiveBorder.BottomRight:
                this.Size = new Size(resizeStart.Width + dif.X, resizeStart.Height + dif.Y);
                break;
            case ActiveBorder.Bottom:
                this.Size = new Size(resizeStart.Width, resizeStart.Height + dif.Y);
                break;
            case ActiveBorder.BottomLeft:
                this.Location = new Point(dragFormPoint.X + dif.X, dragFormPoint.Y);
                this.Size = new Size(resizeStart.Width - dif.X, resizeStart.Height + dif.Y);
                break;
            case ActiveBorder.Left:
                this.Location = new Point(dragFormPoint.X + dif.X, dragFormPoint.Y);
                this.Size = new Size(resizeStart.Width - dif.X, resizeStart.Height);
                break;
            case ActiveBorder.TopLeft:
                this.Location = new Point(dragFormPoint.X + dif.X, dragFormPoint.Y + dif.Y);
                this.Size = new Size(resizeStart.Width - dif.X, resizeStart.Height - dif.Y);
                break;
            }
        }

        private void PaddingPanel_MouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                dragging = false;
                resizing = false;
            }
        }
        private async void CoreWebView2_FaviconChanged(object? sender, object e)
        {
            string uri = webView.CoreWebView2.FaviconUri;

            if (string.IsNullOrEmpty(uri)) return;

            try
            {
                var stream = await httpClient.GetStreamAsync(uri);
                using (var bitmap = new Bitmap(stream))
                {

                    IntPtr hIcon = bitmap.GetHicon();
                    using (var tempIcon = Icon.FromHandle(hIcon))
                    {
                        Icon = (Icon)tempIcon.Clone();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Favicon error: {ex.Message}");
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            int thickness = this.Padding.All;

            Color color = commands?.Color ?? Color.DarkGray;

            Color darkTint = ControlPaint.Dark(color, 0.5f);
            Color lightTint = ControlPaint.Light(color, 0.5f);

            ControlPaint.DrawBorder(
                e.Graphics,
                ClientRectangle,
                darkTint, thickness, ButtonBorderStyle.Solid,
                darkTint, thickness, ButtonBorderStyle.Solid,
                lightTint, thickness, ButtonBorderStyle.Solid,
                lightTint, thickness, ButtonBorderStyle.Solid
            );
        }

    }
}
