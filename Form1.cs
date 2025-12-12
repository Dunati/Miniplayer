using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System.Net.Http;
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
    public partial class Form1 : Form
    {
        private IntPtr CreateThemedIconHandle()
        {
            // 64x64 is a good size for high-DPI scaling
            using (var bmp = new Bitmap(64, 64))
            using (var g = Graphics.FromImage(bmp))
            using (var font = new Font("Segoe MDL2 Assets", 40, FontStyle.Regular))
            {
                // Settings for smooth rendering
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                // \uE7F6 is the standard "Headset/Audio" icon in Windows
                // We use 'Black' here, but you could use SystemBrushes.WindowText for theme awareness
                g.DrawString("\uE7F6", font, Brushes.Black, new PointF(4, 8));

                return bmp.GetHicon();
            }
        }


        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        extern static bool DestroyIcon(IntPtr handle);
        private IntPtr currentIconHandle = IntPtr.Zero; // Track the handle to destroy it later
        private readonly HttpClient httpClient = new HttpClient();


        private WebView2 webView;
        private bool dragging;
        private Point dragCursorPoint;
        private Point dragFormPoint;

        private bool resizing;
        private Size resizeStart;
        public Form1()
        {
            


            InitializeComponent();
            currentIconHandle = CreateThemedIconHandle();
            webView = new WebView2();
            InitializeWebView();

            Padding = new Padding(10);

            // Load saved settings
            this.Load += Form1_Load;
            this.FormClosing += Form1_FormClosing;
        }

        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            Settings.Default.Location = this.Location;
            Settings.Default.Size = this.Size;

            Settings.Default.Save();
        }

        private void Form1_Load(object? sender, EventArgs e)
        {
          // Load the saved location and size
          if (Settings.Default.Location != Point.Empty)
          {
              this.Location = Settings.Default.Location;
          }
          if (Settings.Default.Size != Size.Empty)
          {
              this.Size = Settings.Default.Size;
          }

        }

        ActiveBorder ActiveBorder;

        int resizeBorderThickness => Padding.All;

        private async void InitializeWebView()
        {
            webView.Dock = DockStyle.Fill;

            this.Controls.Add(webView);
            //webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            //webView.CoreWebView2.Settings.IsZoomControlEnabled = false;

            var options = new CoreWebView2EnvironmentOptions
            {
                AreBrowserExtensionsEnabled = true
            };

            var env = await CoreWebView2Environment.CreateAsync(null, null, options);
            await webView.EnsureCoreWebView2Async(env);

            // This script defines the style, then keeps trying to add it until it succeeds.
            string robustHideScript = @"
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



            await webView.EnsureCoreWebView2Async(env);

            webView.CoreWebView2.FaviconChanged += CoreWebView2_FaviconChanged;

            webView.CoreWebView2.Navigate("https://www.pandora.com/");

            MouseDown += PaddingPanel_MouseDown;
            MouseMove += PaddingPanel_MouseMove;
            MouseUp += PaddingPanel_MouseUp;
        }

        private void Form1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            // Toggle between maximized and normal state
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
                        this.Location = new Point(dragFormPoint.X , dragFormPoint.Y+dif.Y);
                        this.Size = new Size(resizeStart.Width + dif.X, resizeStart.Height-dif.Y);
                        break;
                    case ActiveBorder.Right:
                        this.Size = new Size(resizeStart.Width + dif.X, resizeStart.Height);
                        break;
                    case ActiveBorder.BottomRight:
                        this.Size = new Size(resizeStart.Width + dif.X, resizeStart.Height + dif.Y);
                        break;
                    case ActiveBorder.Bottom:
                        this.Size = new Size(resizeStart.Width, resizeStart.Height+dif.Y);
                        break;
                    case ActiveBorder.BottomLeft:
                        this.Location = new Point(dragFormPoint.X + dif.X, dragFormPoint.Y);
                        this.Size = new Size(resizeStart.Width - dif.X, resizeStart.Height + dif.Y);
                        break;
                    case ActiveBorder.Left:
                        this.Location = new Point(dragFormPoint.X+dif.X, dragFormPoint.Y);
                        this.Size = new Size(resizeStart.Width - dif.X, resizeStart.Height);
                        break;
                    case ActiveBorder.TopLeft:
                        this.Location = new Point(dragFormPoint.X+dif.X, dragFormPoint.Y + dif.Y);
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
            if (currentIconHandle != IntPtr.Zero)
            {
                DestroyIcon(currentIconHandle);
            }
            base.OnFormClosed(e);
        }
    }
}
