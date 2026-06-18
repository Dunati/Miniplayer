namespace MiniPlayer
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            // Stale DOM selectors in the async-void station commands can throw after a
            // page reload; keep those from tearing down the whole app.
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) => System.Diagnostics.Debug.WriteLine(e.Exception);

            Application.Run(new MiniPlayer());
        }
    }
}