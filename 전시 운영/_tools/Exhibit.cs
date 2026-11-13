using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

internal static class Exhibit
{
    private static readonly string Profile = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        @"Interactive1113\ExhibitChrome");
    private static readonly string StateFile = Path.Combine(Profile, "exhibit-process.txt");

    [STAThread]
    private static int Main()
    {
        try
        {
            using (var operation = new Mutex(false, @"Local\Interactive1113Exhibit"))
            {
                try
                {
                    if (!operation.WaitOne(TimeSpan.FromSeconds(10)))
                        throw new InvalidOperationException("전시 시작 또는 종료가 이미 진행 중입니다.");
                }
                catch (AbandonedMutexException) { }

                try
                {
                    using (var browser = FindExhibitBrowser())
                    {
#if EXHIBIT_STOP
                        Stop(browser);
#else
                        if (browser == null) Start();
#endif
                    }
                }
                finally { operation.ReleaseMutex(); }
            }
            return 0;
        }
        catch (Exception error)
        {
            MessageBox.Show(error.Message, "전시 운영", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return 1;
        }
    }

    private static Process FindExhibitBrowser()
    {
        if (!File.Exists(StateFile)) return null;
        var state = File.ReadAllLines(StateFile);
        int id;
        long started;
        if (state.Length != 2 || !int.TryParse(state[0], out id) ||
            !long.TryParse(state[1], out started)) return null;

        Process browser;
        try { browser = Process.GetProcessById(id); }
        catch (ArgumentException) { return null; }
        try
        {
            if (browser.ProcessName.Equals("chrome", StringComparison.OrdinalIgnoreCase) &&
                browser.StartTime.ToFileTimeUtc() == started) return browser;
        }
        catch (InvalidOperationException) { }
        browser.Dispose();
        return null;
    }

    private static void Start()
    {
        const string page = "https://doeonon.github.io/interactive1113/redesign/";

        var locations = new[] { Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) };
        var chrome = Array.Find(Array.ConvertAll(locations,
            folder => Path.Combine(folder, @"Google\Chrome\Application\chrome.exe")), File.Exists);
        if (chrome == null) throw new FileNotFoundException("Google Chrome을 찾을 수 없습니다.");

        Directory.CreateDirectory(Profile);
        var arguments = "--user-data-dir=\"" + Profile + "\" " +
            "--no-first-run --no-default-browser-check --disable-background-mode --start-fullscreen " +
            page;
        using (var browser = Process.Start(new ProcessStartInfo(chrome, arguments)))
        {
            File.WriteAllLines(StateFile, new[] { browser.Id.ToString(),
                browser.StartTime.ToFileTimeUtc().ToString() });
        }
    }

    private static void Stop(Process browser)
    {
        if (browser != null && !browser.HasExited &&
            (!browser.CloseMainWindow() || !browser.WaitForExit(5000)))
        {
            if (!browser.HasExited) browser.Kill();
            if (!browser.WaitForExit(3000)) throw new InvalidOperationException("전시 창을 닫지 못했습니다.");
        }
        if (File.Exists(StateFile)) File.Delete(StateFile);
    }
}
