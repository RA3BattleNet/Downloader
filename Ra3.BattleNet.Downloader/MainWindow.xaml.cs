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
        ChangeLanguage(chinese: true);
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        return;
        var assembly = Assembly.GetEntryAssembly();
        // Replace 'yourMagnetLink' with the actual magnetic link.
        string downloadFolder = Path.GetDirectoryName(assembly.Location);

        // Create a Torrent object from the magnetic link.
        Torrent torrent = Torrent.Load(assembly.GetManifestResourceStream("Client.torrent"));

        var engine = new ClientEngine();
        engine.CriticalException += (o, e) => 
        {
            App.NoReturnFatalErrorMessageBox(e.Exception.ToString());
        };

        var torrentManager = await engine.AddAsync(torrent, downloadFolder);
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

    private void ChineseCheckbox_Checked(object sender, RoutedEventArgs e) => ChangeLanguage(chinese: true);
    private void EnglishCheckbox_Checked(object sender, RoutedEventArgs e) => ChangeLanguage(chinese: false);

    private void ChangeLanguage(bool chinese)
    {
        var viewModel = new ViewModel(chinese ? "zh" : "en");
        App.ErrorCaption = viewModel.ErrorCaption;
        DataContext = viewModel;

        ChineseCheckbox.IsChecked = chinese;
        EnglishCheckbox.IsChecked = !chinese;
    }
}
