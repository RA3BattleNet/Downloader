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
        ["zh.Caption"] = "红色警戒3战网客户端 下载程序",
        ["en.ErrorCaption"] = "RA3BattleNet Download Error",
        ["zh.ErrorCaption"] = "红警3战网下载错误",
        ["en.Welcome"] = "Welcome",
        ["zh.Welcome"] = "欢迎使用",
        ["en.Headline"] = "RA3BattleNet Desktop",
        ["zh.Headline"] = "红色警戒3战网客户端",
        ["en.Description"] = "The new BattleNet desktop client is now a one-stop platform for downloading and updating mainstream mods and maps. It also brings 8-player matches, automatic desync recovery and more, for more varied and more stable gameplay.",
        ["zh.Description"] = "战网全新客户端已成为玩家的一站式服务平台，支持各大主流模组与地图的下载与更新。同时也带来8人联机、不同步自动恢复等多项能力，让玩家有更多样，更稳定的游戏体验！",
        ["en.DescriptionMore"] = "You can visit https://ra3battle.net or join QQ group 881583099 to learn more about RA3BattleNet.",
        ["zh.DescriptionMore"] = "您可以访问 https://ra3battle.net 或者加入 QQ 群 881583099 以了解关于红警3战网的更多信息。",
        ["en.Start"] = "Install",
        ["zh.Start"] = "开始下载",
        ["en.HintIdle"] = "*The installer runs automatically",
        ["zh.HintIdle"] = "*下载完成后将自动安装",
        ["en.OpenFolder"] = "Open download folder",
        ["zh.OpenFolder"] = "打开下载文件夹",
        ["en.DownloadedSize"] = "{0} / {1}",
        ["zh.DownloadedSize"] = "{0} / {1}",
        ["en.DownloadSpeed"] = "{0}/s",
        ["zh.DownloadSpeed"] = "{0}/s",
        ["en.CannotRunInstaller"] = "Cannot run the installer.",
        ["zh.CannotRunInstaller"] = "无法执行安装程序。"
    };
    private readonly HashSet<string> _properties = new();
    private string _id;

    public ViewModel() : this("zh") { }

    public ViewModel(string id)
    {
        ChangeLanguage(id);
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
    public string Welcome => Get(nameof(Welcome));
    public string Headline => Get(nameof(Headline));
    public string Description => Get(nameof(Description));
    public string DescriptionMore => Get(nameof(DescriptionMore));
    public string Start => Get(nameof(Start));
    public string HintIdle => Get(nameof(HintIdle));
    public string OpenFolder => Get(nameof(OpenFolder));

    public string DownloadedSize => string.Format(Get(nameof(DownloadedSize)), FormatBytes(_downloadedSize), FormatBytes(_totalSize));
    private long _downloadedSize;
    private long _totalSize;
    public void SetDownloadedSize(long downloadedSize, long totalSize)
    {
        _downloadedSize = downloadedSize;
        _totalSize = totalSize;
        OnSet(nameof(DownloadedSize));
    }

    public double ProgressBarValue => _progress * 100;
    public double ProgressTaskbarValue => _progress;

    private double _progress;
    public void SetProgress(double progress)
    {
        _progress = Math.Min(progress, 1.0);
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
