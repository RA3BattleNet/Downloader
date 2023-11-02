using MonoTorrent;
using MonoTorrent.Client;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;

namespace Ra3.BattleNet.Downloader;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        // Replace 'yourMagnetLink' with the actual magnetic link.
        string magnetLink = "yourMagnetLink";
        string downloadFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        // Create a Torrent object from the magnetic link.
        MagnetLink magnet = MagnetLink.Parse(magnetLink);

        var engine = new ClientEngine();
        engine.CriticalException += (o, e) => Failure(e.Exception.Message);

        var torrentManager = await engine.AddAsync(magnet, downloadFolder);
        await engine.StartAllAsync();

        var dispatcherTimer = new DispatcherTimer();
        dispatcherTimer.Tick += new EventHandler((o, e) =>
        {
            if (!torrentManager.Complete)
            {
                return;
            }
            dispatcherTimer.Stop();

            Process.Start(new ProcessStartInfo
            {
                FileName = torrentManager.Files[0].Path,
                UseShellExecute = true
            });
        });
        dispatcherTimer.Interval = new TimeSpan(0, 1, 0);
        dispatcherTimer.Start();
    }

    private void Failure(string reason)
    {
        MessageBox.Show(reason, "RA3BattleNet Download Error", MessageBoxButton.OK, MessageBoxImage.Error);
        Application.Current.Shutdown();
    }
}
