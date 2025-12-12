using System.Diagnostics;
using System.IO;
using System.Security;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;

namespace Ra3.BattleNet.Downloader;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private const string PersonalizeRegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
    private const int DarkModeRegistryValue = 0; // 0 indicates dark mode is enabled (AppsUseLightTheme registry value is 0)
    private static readonly Brush DarkBackgroundBrush = Brushes.Black;
    private static readonly Brush DarkForegroundBrush = Brushes.White;

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

    private void ApplySystemTheme()
    {
        if (!IsSystemInDarkMode())
        {
            return;
        }

        Background = DarkBackgroundBrush;
        Foreground = DarkForegroundBrush;
        DownloadProgressBar.Background = DarkBackgroundBrush;
    }

    private static bool IsSystemInDarkMode()
    {
        try
        {
            using var personalize = Registry.CurrentUser.OpenSubKey(PersonalizeRegistryKeyPath);
            if (personalize is null)
            {
                return false;
            }

            var appsUseLightTheme = personalize.GetValue("AppsUseLightTheme");
            return appsUseLightTheme is int value && value == DarkModeRegistryValue;
        }
        catch (Exception ex) when (ex is SecurityException
                                   or UnauthorizedAccessException
                                   or ArgumentException
                                   or ObjectDisposedException
                                   or IOException)
        {
#if DEBUG
            Debug.WriteLine($"Failed to read dark mode registry setting from {PersonalizeRegistryKeyPath}, falling back to light mode: {ex}");
#endif
            return false;
        }
    }
}
