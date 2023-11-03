using MonoTorrent;
using MonoTorrent.Client;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Ra3.BattleNet.Downloader;

public static class Download
{
    public static async Task BitTorrentDownload(ViewModel viewModel)
    {
        string downloadFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        viewModel.SetDownloadText("?", downloadFolder);

        var torrent = await Task.Run(() => Torrent.Load(Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(Download).Namespace + ".Client.torrent")));

        var settingsBuilder = new EngineSettingsBuilder
        {
            ListenPort = -1,
            DhtPort = -1,
            AllowPortForwarding = false,
        };
        var engine = new ClientEngine(settingsBuilder.ToSettings());

        var taskSource = new TaskCompletionSource<string>();
        engine.CriticalException += (o, e) => taskSource.TrySetException(e.Exception);

        var torrentManager = await engine.AddAsync(torrent, downloadFolder);

        await engine.StartAllAsync();
        viewModel.SetDownloadText(torrent.Files[0].Path, downloadFolder);

        var dispatcherTimer = new DispatcherTimer();
        dispatcherTimer.Tick += new EventHandler((o, e) =>
        {
            if (!torrentManager.Complete)
            {
                var monitor = torrentManager.Monitor;
                viewModel.SetDownloadedSize(monitor.DataBytesDownloaded, torrent.Size);
                viewModel.SetProgress((double)monitor.DataBytesDownloaded / torrent.Size);
                viewModel.SetDownloadSpeed(monitor.DownloadSpeed);
                return;
            }

            taskSource.TrySetResult(torrentManager.Files[0].Path);
            dispatcherTimer.Stop();
        });
        dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
        dispatcherTimer.Start();

        var downloadedFile = await taskSource.Task;
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = torrentManager.Files[0].Path,
                UseShellExecute = true
            });
        }
        catch (Exception e)
        {
            App.NoReturnFatalErrorMessageBox($"{viewModel.CannotRunInstaller}\r\n\r\n{e}");
        }
    }
}
