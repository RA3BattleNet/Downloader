using System;
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

    private async void StartButton_Click(object sender, RoutedEventArgs e)
    {
        StartButton.Visibility = Visibility.Collapsed;
        IdleBottomPanel.Visibility = Visibility.Collapsed;
        DownloadPanel.Visibility = Visibility.Visible;

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

    // 界面默认是浅色（XAML 里的颜色值），系统深色时换成深色那套。
    private void ApplySystemTheme()
    {
        if (!IsSystemInDarkMode())
        {
            return;
        }

        SetBrush("DialogBgBrush", Color.FromRgb(0x0A, 0x0A, 0x0A));
        SetBrush("DialogFgBrush", Color.FromRgb(0xFA, 0xFA, 0xFA));
        SetBrush("DialogSecondaryFgBrush", Color.FromRgb(0xA3, 0xA3, 0xA3));
        SetBrush("DialogTertiaryFgBrush", Color.FromRgb(0x73, 0x73, 0x73));
        SetBrush("DialogLineBrush", Color.FromArgb(0x1A, 0xFF, 0xFF, 0xFF));
        SetBrush("DialogPrimaryBrush", Color.FromRgb(0xE5, 0xE5, 0xE5));
        SetBrush("DialogPrimaryHoverBrush", Color.FromRgb(0xD4, 0xD4, 0xD8));
        SetBrush("DialogPrimaryPressedBrush", Color.FromRgb(0xBD, 0xBD, 0xBD));
        SetBrush("DialogPrimaryFgBrush", Color.FromRgb(0x17, 0x17, 0x17));
        SetBrush("DialogTrackBrush", Color.FromRgb(0x2A, 0x2A, 0x2A));
    }

    private void SetBrush(string key, Color color)
    {
        Resources[key] = new SolidColorBrush(color);
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

            // AppsUseLightTheme 为 0 表示系统处于深色模式
            return personalize.GetValue("AppsUseLightTheme") is int value && value == 0;
        }
        catch
        {
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
