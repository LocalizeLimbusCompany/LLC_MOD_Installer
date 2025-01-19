﻿using System.Windows;
using LLC_MOD_Toolbox.Helpers;
using LLC_MOD_Toolbox.Models;
using LLC_MOD_Toolbox.Services;
using LLC_MOD_Toolbox.ViewModels;
using LLC_MOD_Toolbox.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using SevenZip;

namespace LLC_MOD_Toolbox
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Gets the current instance of the application.
        /// </summary>
        public static new App Current => (App)Application.Current;
        public IServiceProvider Services { get; }

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Models
            services.AddSingleton<PrimaryNodeList>();

            // Services
            services.AddSingleton<IFileDownloadService, GrayFileDownloadService>();
            services.AddSingleton<IFileDownloadService, RegularFileDownloadService>();

            // Views
            services.AddTransient<MainWindow>();
            services.AddTransient(sp => new AutoInstaller
            {
                DataContext = sp.GetRequiredService<AutoInstallerViewModel>()
            });
            services.AddTransient(sp => new Settings
            {
                DataContext = sp.GetRequiredService<SettingsViewModel>()
            });

            // ViewModels
            services.AddSingleton<SettingsViewModel>();
            services.AddTransient<AutoInstallerViewModel>();
            services.AddTransient<GachaViewModel>();

            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddNLog("Nlog.config");
            });
            return services.BuildServiceProvider();
        }

        public App()
        {
            Services = ConfigureServices();
        }

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            var _logger = Services.GetRequiredService<ILogger<App>>();
            _logger.LogInformation("—————新日志分割线—————");
            _logger.LogInformation("工具箱已进入加载流程。");
            _logger.LogInformation("We have a lift off.");

            SevenZipBase.SetLibraryPath("7z.dll");
            if (e.Args.Length > 0)
            {
                _logger.LogInformation("检测到启动参数。");
                throw new NotImplementedException("暂不支持启动参数。");
            }
            try
            {
                PrimaryNodeList primaryNodeList = Services.GetRequiredService<PrimaryNodeList>();
                primaryNodeList = await PrimaryNodeList.CreateAsync("NodeList.json");
                _logger.LogInformation("节点初始化完成。");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "节点初始化失败。");
            }

            _logger.LogInformation("当前版本：{version}", GetType().Assembly.GetName().Version);
            // 检查更新
            try
            {
                var http = Services.GetRequiredService<IFileDownloadService>();
                var NodeList = Services.GetRequiredService<PrimaryNodeList>();
                // TODO: 优化节点选择
                NodeInformation nodeInformation = NodeList.ApiNode[0];
                var jsonPayload = await http.GetJsonAsync(nodeInformation.Endpoint);
                _logger.LogInformation("API 节点连接成功。");
                var latestVersion = JsonHelper.DeserializeTagName(jsonPayload);
                _logger.LogInformation("检测到版本：{}", latestVersion);
                if (VersionHelper.CheckForUpdate(latestVersion))
                {
                    _logger.LogInformation("检测到新版本。打开链接");
                    FileHelper.LaunchUrl(nodeInformation.Endpoint.ToString());
                    throw new NotImplementedException("暂不支持自动更新。");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "网络不通畅");
            }
            var mainWindow = Services.GetRequiredService<MainWindow>();
            mainWindow?.Show();
        }
    }
}
