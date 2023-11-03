using System;
using System.Diagnostics;
using System.Windows;

namespace Ra3.BattleNet.Downloader;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
/// 
public partial class App : Application
{
    public static string? ErrorCaption { get; set; }
    public static string? ErrorDetailsFormat { get; set; }

    static App()
    {
        AppDomain.CurrentDomain.UnhandledException += (o, e) =>
        {
            var reason = "Unknown Error";
            try
            {
                reason = e.ExceptionObject?.ToString() ?? "Unknown Error";
            }
            catch { }
            NoReturnFatalErrorMessageBox(reason);
        };
    }

    public static void NoReturnFatalErrorMessageBox(string reason)
    {
        var caption = string.IsNullOrWhiteSpace(ErrorCaption) ? "RA3BattleNet Download Error" : ErrorCaption;
        MessageBox.Show(reason, caption, MessageBoxButton.OK, MessageBoxImage.Error);
        if (!Debugger.IsAttached)
        {
            Environment.Exit(1);
        }
    }
}
