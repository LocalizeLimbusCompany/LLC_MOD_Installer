﻿using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using Downloader;

namespace LLC_MOD_Toolbox.Helpers;

public class HttpHelper
{
    private static readonly HttpClient httpClient = new();
    public static readonly DownloadConfiguration downloadOpt = new()
    {
        // file parts to download
        ChunkCount = 8,
        // download speed limited to 2MB/s, default values is zero or unlimited
        MaximumBytesPerSecond = 1024 * 1024 * 2,
        // the maximum number of times to fail
        MaxTryAgainOnFailover = 3,
        // release memory buffer after each 50 MB
        MaximumMemoryBufferBytes = 1024 * 1024 * 50,
        // download parts of the file as parallel or not. The default value is false
        ParallelDownload = true,
        // clear package chunks data when download completed with failure, default value is false
        ClearPackageOnCompletionWithFailure = true,
        // minimum size of chunking to download a file in multiple parts, the default value is 512
        MinimumSizeOfChunking = 1024,
        // Before starting the download, reserve the storage space of the file as file size, the default value is false
        ReserveStorageSpaceBeforeStartingDownload = true,
        // Get on demand downloaded data with ReceivedBytes on downloadProgressChanged event 
        EnableLiveStreaming = false,
        // config and customize request headers
        RequestConfiguration =
        {
        UserAgent = $"LLC_MOD_Toolbox/{Assembly.GetExecutingAssembly().GetName().Version}",
        Proxy = new WebProxy() {
           Address = default,
           UseDefaultCredentials = false,
           Credentials = System.Net.CredentialCache.DefaultNetworkCredentials,
           BypassProxyOnLocal = true
        }
    }
    };
    public static readonly DownloadService downloader = new(downloadOpt);

    static HttpHelper()
    {
        httpClient.DefaultRequestHeaders
            .Add("User-Agent", $"LLC_MOD_Toolbox/{Assembly.GetExecutingAssembly().GetName().Version}");
    }

    /// <summary>
    /// Get an http response from sending request to a specific url
    /// 获取从发送请求到特定url的http响应
    /// </summary>
    /// <param name="url"></param>
    /// <param name="method">Http method being used (default: <see cref="HttpMethod.Get"/>)</param>
    /// <exception cref="HttpRequestException">当网络连接不良时直接断言</exception>
    /// <returns></returns>
    public static async Task<HttpResponseMessage> GetResponseAsync(Uri url, [Optional] HttpMethod method)
    {
        using var request = new HttpRequestMessage
        {
            RequestUri = url,
            Method = method ?? HttpMethod.Get
        };
        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return response;
    }

    public static async Task<Stream> GetAppAsync(Uri url)
    {
        Stream stream = await downloader.DownloadFileTaskAsync(url.AbsolutePath);

        return stream;
    }

    /// <summary>
    /// 获取json数据
    /// </summary>
    /// <exception cref="HttpRequestException">当网络连接不良时直接断言</exception>
    /// <param name="url"></param>
    /// <returns></returns>
    public static async Task<string> GetJsonAsync(Uri url)
    {
        var stream = await GetResponseAsync(url);
        return await stream.Content.ReadAsStringAsync();
    }

    public static async Task<string> GetHashAsync(Uri url)
    {
        var response = await GetResponseAsync(url);
        return await response.Content.ReadAsStringAsync();
    }

    public static async Task<BitmapImage> GetImageAsync(Uri url)
    {
        using var response = await GetResponseAsync(url);
        response.EnsureSuccessStatusCode();
        using var stream = await response.Content.ReadAsStreamAsync();
        var image = new BitmapImage();
        image.BeginInit();
        image.StreamSource = stream;
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.EndInit();
        return image;
    }

    /// <summary>
    /// 老实说或许它不应该在这个类里……
    /// </summary>
    /// <param name="url">要打开的文件</param>
    public static void LaunchUrl(string url)
    {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url)
        {
            UseShellExecute = true
        });
    }
}
