using System.Collections.Generic;
using System.ComponentModel;

namespace Ra3.BattleNet.Downloader;

public class ViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private readonly Dictionary<string, string> _values = new()
    {
        ["en.Caption"] = "RA3BattleNet Downloader",
        ["zh.Caption"] = "红警3战网客户端下载程序",
        ["en.ErrorCaption"] = "RA3BattleNet Download Error",
        ["zh.ErrorCaption"] = "红警3战网下载错误",
        ["en.DownloadText"] = "Downloading {0} to {1}",
        ["zh.DownloadText"] = "正在下载 {0} 到 {1}",
        ["en.DownloadedSize"] = "已下载 {0} / {1}",
        ["zh.DownloadedSize"] = "已下载 {0} / {1}",
        ["en.Progress"] = "{0:P2}",
        ["zh.Progress"] = "{0:P2}",
        ["en.DownloadSpeed"] = "{0}",
        ["zh.DownloadSpeed"] = "{0}",
        ["en.Description"] = "After the download is complete, the installer will be executed automatically. You can visit https://ra3battle.net or join QQ group 604807102 for more information about RA3BattleNet.",
        ["zh.Description"] = "下载完毕之后安装程序将自动执行。您可以访问 https://ra3battle.net 或者加入 QQ 群 604807102 以了解关于红警3战网的更多信息。"
    };
    private readonly string _id;

    public ViewModel() : this("zh") { }

    public ViewModel(string id)
    {
        _id = id switch { "en" => "en", _ => "zh" };
        SetDownloadText("?", "?");
        SetDownloadedSize(0, 0);
        SetProgress(0);
        SetDownloadSpeed(0);
    }

    public string Caption => GetLocalized(nameof(Caption));
    public string ErrorCaption => GetLocalized(nameof(ErrorCaption));

    public string DownloadText => Get(nameof(DownloadText));
    public string SetDownloadText(string fileName, string downloadFolder)
    {
        var text = string.Format(GetLocalized(nameof(DownloadText)), fileName, downloadFolder);
        Set(nameof(DownloadText), text);
        return text;
    }

    public string DownloadedSize => Get(nameof(DownloadedSize));
    public string SetDownloadedSize(long downloadedSize, long totalSize)
    {
        var text = string.Format(GetLocalized(nameof(DownloadedSize)), FormatBytes(downloadedSize), FormatBytes(totalSize));
        Set(nameof(DownloadedSize), text);
        return text;
    }

    public string Progress => Get(nameof(Progress));
    public string SetProgress(double progress)
    {
        var text = string.Format(GetLocalized(nameof(Progress)), progress);
        Set(nameof(Progress), text);
        return text;
    }

    public string DownloadSpeed => Get(nameof(DownloadSpeed));
    public string SetDownloadSpeed(double bytePerSecond)
    {
        var speed = FormatBytes(bytePerSecond) + "/s";
        var text = string.Format(GetLocalized(nameof(DownloadSpeed)), speed);
        Set(nameof(DownloadSpeed), text);
        return text;
    }

    public string Description => GetLocalized(nameof(Description));


    private string Get(string key) => _values.TryGetValue(key, out var value) ? value : string.Empty;
    private string GetLocalized(string key) => Get($"{_id}.{key}");
    private void Set(string key, string value)
    {
        _values[key] = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(key));
    }

    private static string FormatBytes(double bytes)
    {
        return bytes switch
        {
            var x when x > 1024 * 1024 => $"{x / 1024 / 1024:F2} MB",
            var x when x > 1024 => $"{x / 1024:F2} KB",
            _ => $"{bytes:F2} B",
        };
    }
}

