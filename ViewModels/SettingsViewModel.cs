﻿using System.Configuration;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LLC_MOD_Toolbox.Helpers;
using LLC_MOD_Toolbox.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace LLC_MOD_Toolbox.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    public static PrimaryNodeList PrimaryNodeList { get; set; } = new();

    private string? limbusCompanyPath;

    private readonly ILogger<SettingsViewModel> logger;

    /// <summary>
    /// 仅在 Windows 下有效，不过这个项目也只在 Windows 下有效
    /// </summary>
    public string LimbusCompanyPath
    {
        get
        {
            var path =
                ConfigurationManager.AppSettings["GamePath"]
                ?? Registry.GetValue(
                    @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 1973530",
                    "InstallLocation",
                    null
                ) as string
                ?? throw new ArgumentNullException("未找到边狱公司路径。可能是注册表被恶意修改了！");
            if (Directory.Exists(path))
            {
                limbusCompanyPath = path;
                return path;
            }
            throw new DirectoryNotFoundException("未找到边狱公司路径。可能是注册表被恶意修改了！");
        }
        set
        {
            logger.LogInformation("设置边狱公司路径为：{value}", value);
            ConfigurationManager.AppSettings["GamePath"] = value;
            SetProperty(ref limbusCompanyPath, value);
        }
    }

    [ObservableProperty]
    private List<NodeInformation> downloadNodeList;

    [ObservableProperty]
    private NodeInformation downloadNode;

    [ObservableProperty]
    private List<NodeInformation> apiNodeList;

    [ObservableProperty]
    private NodeInformation apiNode;

    [RelayCommand]
    private Task ModUnistallation()
    {
        logger.LogInformation("开始卸载 BepInEx。");
        MessageBoxResult result = MessageBox.Show(
            "删除后你需要重新安装汉化补丁。\n确定继续吗？",
            "警告",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning
        );
        if (result != MessageBoxResult.Yes)
        {
            logger.LogInformation("取消卸载 BepInEx。");
            return Task.CompletedTask;
        }
        try
        {
            FileHelper.DeleteBepInEx(LimbusCompanyPath);
        }
        catch (IOException)
        {
            MessageBox.Show("Limbus Company正在运行中，请先关闭游戏。", "警告");
        }
        catch (ArgumentNullException)
        {
            MessageBox.Show("注册表内无数据，可能被恶意修改了！", "警告");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"删除过程中出现了一些问题：\n{ex}", "警告");
        }

        return Task.CompletedTask;
    }

    public SettingsViewModel(ILogger<SettingsViewModel> logger)
    {
        this.logger = logger;
        DownloadNodeList = PrimaryNodeList.DownloadNode;
        ApiNodeList = PrimaryNodeList.ApiNode;
        downloadNode = DownloadNodeList.Last(n => n.IsDefault);
        apiNode = ApiNodeList.Last(n => n.IsDefault);
    }
}
