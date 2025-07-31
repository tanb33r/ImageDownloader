using ImageDownloader.Models;

namespace ImageDownloader.Services
{
    public interface IImageDownloadService
    {
        Task<ResponseDownload> DownloadImagesAsync(RequestDownload request);
        Task<string?> GetImageAsBase64Async(string imageName);
    }
}