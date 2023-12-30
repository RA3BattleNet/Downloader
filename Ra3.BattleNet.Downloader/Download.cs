using MonoTorrent;
using MonoTorrent.Client;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Ra3.BattleNet.Downloader;

public static class Download
{
    public static async Task BitTorrentDownload(ViewModel viewModel)
    {
        // Set download folder to Current file path.
        //string downloadFolder = Path.Combine(Environment.GetEnvironmentVariable("appdata"), "RA3BattleNet", "temp");
        string downloadFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        if (!Directory.Exists(downloadFolder))
        {
            Directory.CreateDirectory(downloadFolder);
        }

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
        viewModel.SetDownloadText(torrent.Files[0].Path);

        var dispatcherTimer = new DispatcherTimer();
        dispatcherTimer.Tick += new EventHandler((o, e) =>
        {
            if (!torrentManager.Complete)
            {
                var monitor = torrentManager.Monitor;
                viewModel.SetDownloadedSize(monitor.DataBytesDownloaded, torrent.Size);
                var progress = (double)monitor.DataBytesDownloaded / torrent.Size;
                viewModel.SetProgress(progress);
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
                FileName = Path.Combine(downloadFolder, torrentManager.Files[0].Path),
                UseShellExecute = true
            });
            // TODO: 按理来说应该可以设置Torrent库的cache位置。得防止用户在当前目录下有cache文件夹
            // （我的cache就被删了）
            //DeleteFolder(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),"cache"));
            Environment.Exit(0);
        }
        catch (Exception e)
        {
            App.NoReturnFatalErrorMessageBox($"{viewModel.CannotRunInstaller}\r\n\r\n{e}");
        }
    }

    /// <summary>
    /// FileSystem Operations
    /// </summary>
    public static void DeleteFolder(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            return;
        }

        ClearFolderContent(directoryPath, path => false);
        Directory.Delete(directoryPath);
    }
    public static void ClearFolderContent(string directoryPath, Func<FileSystemInfo, bool> keep)
    {
        var directory = new DirectoryInfo(directoryPath);
        foreach (var info in directory.EnumerateFileSystemInfos())
        {
            if (info.Attributes.HasFlag(FileAttributes.ReparsePoint))
            {
                throw new NotSupportedException($"Attempting to delete a reparse point: {info.FullName}");
            }
            else if (info is DirectoryInfo directoryInfo)
            {
                ClearFolderContent(info.FullName, keep);
                if (!directoryInfo.EnumerateFileSystemInfos().Any())
                {
                    // delete directory if it's empty
                    directoryInfo.Delete();
                }
            }
            else if (info is FileInfo fileInfo)
            {
                if (keep(info))
                {
                    continue;
                }

                fileInfo.IsReadOnly = false;
                fileInfo.Delete();
            }
            else
            {
                throw new NotImplementedException($"Unexpected FileSystemInfo type {info.GetType()}");
            }
        }
    }


}
