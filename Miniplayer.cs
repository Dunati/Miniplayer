using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MiniPlayer
{
    enum ActiveBorder
    {
        None,
        Top,
        TopRight,
        Right,
        BottomRight,
        Bottom,
        BottomLeft,
        Left,
        TopLeft,
    }




    public partial class MiniPlayer : Form
    {

        class Station
        {
            public string uri { get; set; }
            public Point location { get; set; }
            public Size size { get; set; }

            public Station(string uri, Point location, Size size)
            {
                this.uri = uri;
                this.location = location;
                this.size = size;
            }
        };


        class StationSettings
        {
            public List<Station> stations { get; set; } = new();
            public int current_station { get; set; }
            public void Add(Station s)
            {
                stations.Add(s);
            }

            [JsonIgnore]
            public Station Current
            {
                get
                {
                    return stations[current_station];
                }
                set
                {
                    stations[current_station] = value;
                }
            }

            public Station this[int index]
            {
                get { return stations[index]; }
            }
            public Dictionary<string, int> GetStationIndices()
            {
                Dictionary<string, int> s = new Dictionary<string, int>();
                for (int i = 0; i < stations.Count; i++)
                {
                    s[stations[i].uri] = i;
                }
                return s;
            }
            [JsonIgnore]
            public Station NextStation
            {
                get
                {
                    current_station = (current_station + 1) % stations.Count;
                    return stations[current_station];
                }
            }
            [JsonIgnore]
            public Station PrevStation
            {
                get
                {
                    current_station = (current_station + stations.Count - 1) % stations.Count;
                    return stations[current_station];
                }
            }
        }


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
            this.ResizeRedraw = true;
            webView = new WebView2();
            InitializeWebView();

            Padding = new Padding(10);

            webView.Focus();


            // Load saved settings
            this.Load += Form1_Load;
            this.FormClosing += Form1_FormClosing;
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
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curAssembly.ModuleName), 0);
            }
        }

        private void MiniPlayer_KeyDown(object? sender, KeyEventArgs e)
        {
            if (webView.CoreWebView2 != null)
            {
                if (e.KeyCode == Keys.MediaPlayPause)
                {
                    webView.CoreWebView2.ExecuteScriptAsync("document.dispatchEvent(new KeyboardEvent('keydown', {key: 'MediaPlayPause'}));");
                }
                else if (e.KeyCode == Keys.MediaNextTrack)
                {
                    webView.CoreWebView2.ExecuteScriptAsync("document.dispatchEvent(new KeyboardEvent('keydown', {key: 'MediaNextTrack'}));");
                }
                else if (e.KeyCode == Keys.MediaPreviousTrack)
                {
                    webView.CoreWebView2.ExecuteScriptAsync("document.dispatchEvent(new KeyboardEvent('keydown', {key: 'MediaPreviousTrack'}));");
                }
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                bool isCtrlDown = (GetKeyState(VK_CONTROL) & 0x8000) != 0;
                if (vkCode == (int)Keys.MediaPlayPause)
                {
                    if (isCtrlDown)
                    {
                        webView.Reload();
                    }
                    else
                    {
                        webView.CoreWebView2.ExecuteScriptAsync("clickButton('pause_button', 'play_button');");
                    }
                }
                else if (vkCode == (int)Keys.MediaNextTrack)
                {
                    if (isCtrlDown)
                    {
                        webView.CoreWebView2.ExecuteScriptAsync("clickButton('thumbs_up_button');");
                    }
                    else
                    {
                        webView.CoreWebView2.ExecuteScriptAsync("clickButton('skip_button');");
                    }
                }
                else if (vkCode == (int)Keys.MediaPreviousTrack)
                {
                    if (isCtrlDown)
                    {
                        webView.CoreWebView2.ExecuteScriptAsync("clickButton('thumbs_down_button');");
                    }
                    else
                    {
                        webView.CoreWebView2.ExecuteScriptAsync("clickButton('replay_button');");
                    }
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            stationSettings.Current.location = this.Location;
            stationSettings.Current.size = this.Size;

            Settings.Default.StationSettings = JsonSerializer.Serialize(stationSettings);

            Settings.Default.Save();
            UnhookWindowsHookEx(_hookID);
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
                        }
                    }
                }
                catch
                {
                }
            }
            try
            {
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
            catch { }
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

            // This script defines the style, then keeps trying to add it until it succeeds.

            string robustHideScript = @"
        function clickButton(dataQa, dataQa2) {
            var button = document.querySelector('[data-qa=""' + dataQa + '""]');
            if (button) {
                button.click(); // Simulate a click on the button
            } else if (dataQa2 !== undefined) {
                button = document.querySelector('[data-qa=""' + dataQa2 + '""]');
                if (button) {
                    button.click(); // Simulate a click on the button
                } else {
                    console.error(dataQa+' not found.');
                }
            }
        }
    (function() {
        const css = `
            /* 1. Hide the scrollbar visual parts (Chromium/WebView2) */
            ::-webkit-scrollbar { 
                display: none !important; 
                width: 0px !important; 
                height: 0px !important; 
            }
            
            /* 2. Hide visuals for standard compliance (Firefox/other) */
            html, body { 
                scrollbar-width: none !important; 
                -ms-overflow-style: none !important; 
                
                /* 3. IMPORTANT: Ensure scrolling is ENABLED */
                overflow: auto !important; 
            }
        `;

        function inject() {
            const target = document.head || document.documentElement;
            if (target) {
                const style = document.createElement('style');
                style.innerHTML = css;
                target.appendChild(style);
            } else {
                requestAnimationFrame(inject);
            }
        }
        inject();
    })();
";

            await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(robustHideScript);
            try
            {
                string extensionPath = @"C:\source\MiniPlayer\uBlock0.chromium";
                await webView.CoreWebView2.Profile.AddBrowserExtensionAsync(extensionPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load extension: {ex.Message}");
            }


            webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            webView.CoreWebView2.Settings.IsZoomControlEnabled = false;

            webView.CoreWebView2.FaviconChanged += CoreWebView2_FaviconChanged;

            webView.CoreWebView2.Navigate(stationSettings.Current.uri);

            MouseDown += PaddingPanel_MouseDown;
            MouseMove += PaddingPanel_MouseMove;
            MouseUp += PaddingPanel_MouseUp;
        }

        private void Form1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.ActiveBorder == ActiveBorder.Top)
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
            else if (ActiveBorder == ActiveBorder.Right)
            {
                stationSettings.Current.location = this.Location;
                stationSettings.Current.size = this.Size;

                var station = stationSettings.NextStation;
                webView.CoreWebView2.Navigate(station.uri);

                this.Size = station.size;
                this.Location = station.location;
            }
            else if (ActiveBorder == ActiveBorder.Left)
            {
                stationSettings.Current.location = this.Location;
                stationSettings.Current.size = this.Size;

                var station = stationSettings.PrevStation;
                webView.CoreWebView2.Navigate(station.uri);

                this.Size = station.size;
                this.Location = station.location;
            }
        }

        private void PaddingPanel_MouseDown(object? sender, MouseEventArgs e)
        {
            // Start dragging if left mouse button is pressed
            if (e.Button == MouseButtons.Left)
            {
                if (Cursor == Cursors.SizeAll)
                {
                    dragging = true;
                    dragCursorPoint = Cursor.Position;
                    dragFormPoint = this.Location;
                }
                else
                {
                    resizing = true;
                    dragCursorPoint = Cursor.Position;
                    dragFormPoint = this.Location;
                    resizeStart = this.Size;
                }
            }

        }

        private void SetCursor(MouseEventArgs e)
        {
            if (e.Location.Y <= resizeBorderThickness && (e.Location.X > resizeBorderThickness && e.Location.X < Width - resizeBorderThickness))
            {
                ActiveBorder = ActiveBorder.Top;
                this.Cursor = Cursors.SizeAll;
            }
            // Determine if the mouse is near the edges for resizing
            else if (e.Location.X <= resizeBorderThickness && e.Location.Y <= resizeBorderThickness) // Top-left corner
            {
                ActiveBorder = ActiveBorder.TopLeft;
                this.Cursor = Cursors.SizeNWSE; // Diagonal resize cursor
            }
            else if (e.Location.X >= this.Width - resizeBorderThickness && e.Location.Y <= resizeBorderThickness) // Top-right corner
            {
                ActiveBorder = ActiveBorder.TopRight;
                this.Cursor = Cursors.SizeNESW; // Diagonal resize cursor
            }
            else if (e.Location.X <= resizeBorderThickness && e.Location.Y >= this.Height - resizeBorderThickness) // Bottom-left corner
            {
                ActiveBorder = ActiveBorder.BottomLeft;
                this.Cursor = Cursors.SizeNESW; // Diagonal resize cursor
            }
            else if (e.Location.X >= this.Width - resizeBorderThickness && e.Location.Y >= this.Height - resizeBorderThickness) // Bottom-right corner
            {
                ActiveBorder = ActiveBorder.BottomRight;
                this.Cursor = Cursors.SizeNWSE; // Diagonal resize cursor
            }
            else if (e.Location.X <= resizeBorderThickness) // Left edge
            {
                ActiveBorder = ActiveBorder.Left;
                this.Cursor = Cursors.SizeWE; // Horizontal resize cursor
            }
            else if (e.Location.X >= this.Width - resizeBorderThickness) // Right edge
            {
                ActiveBorder = ActiveBorder.Right;
                this.Cursor = Cursors.SizeWE; // Horizontal resize cursor
            }
            else if (e.Location.Y >= this.Height - resizeBorderThickness) // Bottom edge
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
            // Drag the form if dragging is true
            if (dragging)
            {
                Point dif = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                this.Location = Point.Add(dragFormPoint, new Size(dif.X, dif.Y));
            }
            // Check if resizing is happening
            else if (resizing)
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
            else
            {
                SetCursor(e);
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
    }
}
