using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;

namespace Ra3.BattleNet.Downloader;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        ApplySystemTheme();
        ChangeLanguage(chinese: true);
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        await Download.BitTorrentDownload((ViewModel)DataContext);
    }

    private void ChineseCheckbox_Checked(object sender, RoutedEventArgs e) => ChangeLanguage(chinese: true);
    private void EnglishCheckbox_Checked(object sender, RoutedEventArgs e) => ChangeLanguage(chinese: false);

    private void ChangeLanguage(bool chinese)
    {
        var viewModel = (ViewModel)DataContext;
        viewModel.ChangeLanguage(chinese ? "zh" : "en");
        App.ErrorCaption = viewModel.ErrorCaption;

        ChineseCheckbox.IsChecked = chinese;
        EnglishCheckbox.IsChecked = !chinese;
    }

    private void Hyperlink_Click(object sender, RoutedEventArgs e)
    {
        Download.OpenAndLocateFile(Download.DownloadPath);
    }

    private const string PersonalizeRegistryKeyPath = @"Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize";

    private void ApplySystemTheme()
    {
        if (!IsSystemInDarkMode())
        {
            return;
        }

        Background = Brushes.Black;
        Foreground = Brushes.White;
        DownloadProgressBar.Background = Brushes.Black;
    }

    private static bool IsSystemInDarkMode()
    {
        try
        {
            using var personalize = Registry.CurrentUser.OpenSubKey(PersonalizeRegistryKeyPath);
            var appsUseLightTheme = personalize?.GetValue("AppsUseLightTheme");
            return appsUseLightTheme is int value && value == 0;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            return false;
        }
    }
}
