using System;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
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
    private static readonly Brush DarkBackgroundBrush = new SolidColorBrush(Color.FromRgb(0x19, 0x19, 0x19));
    private static readonly Brush DarkForegroundBrush = Brushes.White;

    public MainWindow()
    {
        InitializeComponent();
        ApplySystemTheme();
        ChangeLanguage(chinese: true);
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        if (IsSystemInDarkMode())
        {
            TrySetDarkTitleBar();
        }
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

        try
        {
            Background = DarkBackgroundBrush;
            Foreground = DarkForegroundBrush;
            DownloadProgressBar.Background = DarkBackgroundBrush;
            ChineseCheckbox.Foreground = DarkForegroundBrush;
            EnglishCheckbox.Foreground = DarkForegroundBrush;
        }
        catch
        {
            // If dark mode application fails, just continue with default colors
        }
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

    // Enable dark title bar (non-client area) for Windows 10 1809+ when dark mode is active.
    // Returns true if dark title bar was successfully applied, false otherwise.
    private bool TrySetDarkTitleBar()
    {
        try
        {
            var handle = new WindowInteropHelper(this).Handle;
            if (handle == IntPtr.Zero)
            {
                return false;
            }

            // Get real Windows build from registry (Environment.OSVersion.Build may return compatibility version)
            int build = GetWindowsBuild();
            if (build == -1)
            {
                return false;
            }

            // Detect Windows build: 17763 (1809) supports attribute 19, 18362+ (1903) supports 20
            var attribute = build >= 18362 ? DWMWA_USE_IMMERSIVE_DARK_MODE : (build >= 17763 ? DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_1903 : -1);
            if (attribute == -1)
            {
                return false;
            }

            int useDark = 1;
            int result = DwmSetWindowAttribute(handle, attribute, ref useDark, sizeof(int));

            // S_OK = 0, anything else is failure
            return result == 0;
        }
        catch
        {
            return false;
        }
    }

    private static int GetWindowsBuild()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            if (key is null)
            {
                return -1;
            }

            var buildValue = key.GetValue("CurrentBuildNumber");
            if (buildValue is string buildStr && int.TryParse(buildStr, out int build))
            {
                return build;
            }

            return -1;
        }
        catch
        {
            return -1;
        }
    }

    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_1903 = 19;

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int dwAttribute, ref int pvAttribute, int cbAttribute);
}