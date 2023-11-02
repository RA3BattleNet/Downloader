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
    public static string ErrorCaption { get; set; }

    static App()
    {
        AppDomain.CurrentDomain.UnhandledException += (o, e) =>
        {
            NoReturnFatalErrorMessageBox(e.ExceptionObject.ToString());
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
