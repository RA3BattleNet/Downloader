# RA3BattleNet Downloader

一个基于 WPF 的小工具，用于下载并启动 RA3BattleNet 客户端的安装包。程序使用内置的 Torrent 文件，通过 MonoTorrent 下载到用户的“下载”目录并在完成后自动运行安装程序。

## 开发环境
- Windows（WPF 应用）
- .NET SDK，支持构建 .NET Framework 4.6.1 目标

## 构建
```bash
dotnet restore
dotnet build Downloader.sln
```

构建产物位于 `Ra3.BattleNet.Downloader/bin/{Debug|Release}`。

## 许可
本项目基于 [MIT License](LICENSE) 发布。***
