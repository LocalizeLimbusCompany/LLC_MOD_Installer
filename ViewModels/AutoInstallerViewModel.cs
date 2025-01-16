﻿using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LLC_MOD_Toolbox.Helpers;
using LLC_MOD_Toolbox.Models;
using LLC_MOD_Toolbox.Services;
using Microsoft.Extensions.Logging;

namespace LLC_MOD_Toolbox.ViewModels;

public partial class AutoInstallerViewModel(
    ILogger<AutoInstallerViewModel> logger,
    IFileDownloadService fileDownloadServiceProxy,
    SettingsViewModel settingsViewModel
) : ObservableObject
{
    [ObservableProperty]
    Progress<double> installationProgress = new();

    [RelayCommand]
    private async Task ModInstallation()
    {
        logger.LogInformation("开始安装 BepInEx。");
        NodeInformation selectedEndPoint = settingsViewModel.DownloadNode;
        string limbusCompanyPath = settingsViewModel.LimbusCompanyPath;
        logger.LogInformation("选择的下载节点为：{selectedEndPoint}", selectedEndPoint);
        logger.LogInformation("边狱公司路径为：{limbusCompanyPath}", limbusCompanyPath);
        MessageBoxResult result = MessageBox.Show(
            "安装前请确保游戏已经关闭。\n确定继续吗？",
            "警告",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning
        );
        if (result != MessageBoxResult.Yes)
        {
            logger.LogInformation("用户取消了安装 BepInEx。");
            return;
        }
        try
        {
            var stream = await fileDownloadServiceProxy.DownloadFileAsync(
                selectedEndPoint.Endpoint,
                limbusCompanyPath,
                InstallationProgress
            );
            var onlineHash = await fileDownloadServiceProxy.GetHashAsync(selectedEndPoint.Endpoint);
            if (!await ValidateHelper.CheckHashAsync(stream, onlineHash))
                throw new Exception("文件校验失败。");
            FileHelper.InstallBepInEx(stream, limbusCompanyPath);
            logger.LogInformation("BepInEx 安装完成。");
        }
        catch (IOException)
        {
            MessageBox.Show("Limbus Company正在运行中，请先关闭游戏。", "警告");
            logger.LogError("Limbus Company正在运行中，请先关闭游戏。");
        }
        catch (ArgumentNullException)
        {
            MessageBox.Show("注册表内无数据，可能被恶意修改了！", "警告");
            logger.LogError("注册表内无数据，可能被恶意修改了！");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"安装过程中出现了一些问题：\n{ex}", "警告");
            logger.LogError(ex, "安装过程中出现了一些问题。");
        }
    }
}
