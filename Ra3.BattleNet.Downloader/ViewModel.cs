using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Ra3.BattleNet.Downloader;

public class ViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private readonly Dictionary<string, string> _values = new()
    {
        ["en.Caption"] = "RA3BattleNet Downloader",
        ["zh.Caption"] = "红警3战网客户端 下载程序",
        ["en.ErrorCaption"] = "RA3BattleNet Download Error",
        ["zh.ErrorCaption"] = "红警3战网下载错误",
        ["en.DownloadTextPart1"] = "Downloading {0} to ",
        ["zh.DownloadTextPart1"] = "正在下载 {0} 到 ",
        ["en.DownloadFolderText"] = "Downloads",
        ["zh.DownloadFolderText"] = "下载",
        ["en.FolderText"] = " folder",
        ["zh.FolderText"] = " 文件夹",
        ["en.DownloadedSize"] = "{0} / {1}",
        ["zh.DownloadedSize"] = "{0} / {1}",
        ["en.Progress"] = "{0:P2} Completed",
        ["zh.Progress"] = "已完成 {0:P2}",
        ["en.DownloadSpeed"] = "{0}/s",
        ["zh.DownloadSpeed"] = "{0}/s",
        ["en.Description"] = "After the download is completed, the installer will be executed automatically. You can visit https://ra3battle.net for more information.",
        ["zh.Description"] = "下载完毕之后安装程序将自动执行。您可以访问 https://ra3battle.net 或者加入 QQ 群 818026401 以了解关于红警3战网的更多信息。",
        ["en.CannotRunInstaller"] = "Cannot run the installer.",
        ["zh.CannotRunInstaller"] = "无法执行安装程序。"
    };
    private readonly HashSet<string> _properties = new();
    private string _id;

    public ViewModel() : this("zh") { }

    public ViewModel(string id)
    {
        ChangeLanguage(id);
        SetDownloadText("METADATA");
        SetDownloadedSize(0, 0);
        SetProgress(0);
        SetDownloadSpeed(0);
    }

    [MemberNotNull(nameof(_id))]
    public void ChangeLanguage(string id)
    {
        _id = id switch { "en" => "en", _ => "zh" };
        foreach (var key in _properties)
        {
            PropertyChanged?.Invoke(this, new(key));
        }
    }
    public string Caption => Get(nameof(Caption));
    public string ErrorCaption => Get(nameof(ErrorCaption));
    public string CannotRunInstaller => Get(nameof(CannotRunInstaller));
    public string DownloadTextPart1 => string.Format(Get(nameof(DownloadTextPart1)), _fileName);
    public string DownloadFolderText => Get(nameof(DownloadFolderText));
    public string FolderText => Get(nameof(FolderText));

    private string _fileName = "?";
    public void SetDownloadText(string fileName)
    {
        _fileName = fileName;
        OnSet(nameof(DownloadTextPart1));
    }

    public string DownloadedSize => string.Format(Get(nameof(DownloadedSize)), FormatBytes(_downloadedSize), FormatBytes(_totalSize));
    private long _downloadedSize;
    private long _totalSize;
    public void SetDownloadedSize(long downloadedSize, long totalSize)
    {
        _downloadedSize = downloadedSize;
        _totalSize = totalSize;
        OnSet(nameof(DownloadedSize));
    }

    public string Progress => string.Format(Get(nameof(Progress)), _progress);
    public double ProgressBarValue => _progress * 100;
    public double ProgressTaskbarValue => _progress;

    private double _progress;
    public void SetProgress(double progress)
    {
        _progress = progress;
        OnSet(nameof(Progress));
        OnSet(nameof(ProgressBarValue));
        OnSet(nameof(ProgressTaskbarValue));
    }

    public string DownloadSpeed => string.Format(Get(nameof(DownloadSpeed)), FormatBytes(_bytePerSecond));
    private double _bytePerSecond;
    public void SetDownloadSpeed(double bytePerSecond)
    {
        _bytePerSecond = bytePerSecond;
        OnSet(nameof(DownloadSpeed));
    }

    public string Description => Get(nameof(Description));

    private string Get(string property)
    {
        _properties.Add(property);
        return _values.TryGetValue($"{_id}.{property}", out var value) ? value : string.Empty;
    }
    private void OnSet(string property)
    {
        _properties.Add(property);
        PropertyChanged?.Invoke(this, new(property));
    }

    private static string FormatBytes(double bytes)
    {
        return bytes switch
        {
            var x when x > 1024 * 1024 => $"{x / 1024 / 1024:F2}MB",
            var x when x > 1024 => $"{x / 1024:F2}KB",
            _ => $"{bytes:F2}B",
        };
    }
}