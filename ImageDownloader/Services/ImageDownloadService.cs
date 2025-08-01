using ImageDownloader.Models;
using System.Collections.Concurrent;

namespace ImageDownloader.Services
{
    public class ImageDownloadService : IImageDownloadService
    {
        private readonly HttpClient _httpClient;
        private readonly IWebHostEnvironment _environment;
        private readonly string _downloadPath;

        public ImageDownloadService(
            HttpClient httpClient,
            IWebHostEnvironment environment,
            ILogger<ImageDownloadService> logger)
        {
            _httpClient = httpClient;
            _environment = environment;
            _downloadPath = Path.Combine(_environment.WebRootPath, "images", "downloaded");

            if (!Directory.Exists(_downloadPath))
            {
                Directory.CreateDirectory(_downloadPath);
            }
        }

        public async Task<ResponseDownload> DownloadImagesAsync(RequestDownload request)
        {
            var response = new ResponseDownload { Success = true };
            var urlAndNames = new ConcurrentDictionary<string, string>();
            var errors = new List<string>();
            var totalImages = request.ImageUrls.Count();
            var completedCount = 0;

            try
            {
                using var semaphore = new SemaphoreSlim(request.MaxDownloadAtOnce, request.MaxDownloadAtOnce);

                var downloadTasks = request.ImageUrls.Select(async (url, index) =>
                {
                    await semaphore.WaitAsync();

                    var position = index + 1;
                    // for testing
                    Console.WriteLine($"-------------------- [{position}/{totalImages}] Starting dowload: {url}");
                    try
                    {
                        var result = await DownloadSingleImageAsync(url);
                        var completed = Interlocked.Increment(ref completedCount);

                        if (result.Success)
                        {
                            urlAndNames.TryAdd(url, result.FileName);
                            // for testing
                            Console.WriteLine($"------------------  [{position}/{totalImages}] Downloaded : {result.FileName}");
                        }
                        else
                        {
                            lock (errors)
                            {
                                errors.Add($"Failed to download {url}: {result.Error}");

                                // for testing
                                Console.WriteLine($"------------------ Failed to download {url}: {result.Error}");
                            }
                            // for testing
                            Console.WriteLine($"------------------ [{position}/{totalImages}] failed: {result.Error}");
                        }
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                await Task.WhenAll(downloadTasks);

                response.UrlAndNames = urlAndNames.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                if (errors.Any())
                {
                    response.Success = urlAndNames.Any();
                    response.Message = $"Completed with {errors.Count} errors: {string.Join("; ", errors)}";

                    Console.WriteLine($"############## {errors.Count} errors.");
                    Console.WriteLine($"############## {urlAndNames.Count} successful downloads.");
                }
                else
                {
                    response.Message = $"Successfully downloaded {urlAndNames.Count} images";
                    Console.WriteLine($"##############  {totalImages} images downloaded successfully!");
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Batch download failed: {ex.Message}";
            }

            return response;
        }

        public async Task<string?> GetImageAsBase64Async(string imageName)
        {
            var filePath = Path.Combine(_downloadPath, imageName);

            if (!File.Exists(filePath))
                return null;

            var imageBytes = await File.ReadAllBytesAsync(filePath);
            return Convert.ToBase64String(imageBytes);
        }

        private async Task<(bool Success, string FileName, string Error)> DownloadSingleImageAsync(string url)
        {
            try
            {
                using var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"############# {response.StatusCode}: {url}");
                    return (false, string.Empty, $"{response.StatusCode}");
                }

                var contentType = response.Content.Headers.ContentType?.MediaType;

                if (!IsValidImageContentType(contentType))
                {
                    Console.WriteLine($"Invalid type {contentType}: {url}");
                    return (false, string.Empty, $"Invalid type: {contentType}");
                }

                var imageBytes = await response.Content.ReadAsByteArrayAsync();
                var fileName = GenerateFileName(url, contentType);
                var filePath = Path.Combine(_downloadPath, fileName);

                //Console.WriteLine($"#################### Saving {fileName}");
                await File.WriteAllBytesAsync(filePath, imageBytes);

                return (true, fileName, string.Empty);
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Error  {url}");
                return (false, string.Empty, ex.Message);
            }
        }

        private static bool IsValidImageContentType(string? contentType)
        {
            if (string.IsNullOrEmpty(contentType))
                return false;

            var validTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp", "image/bmp" };
            return validTypes.Contains(contentType.ToLowerInvariant());
        }

        private static string GenerateFileName(string url, string? contentType)
        {
            var guid = Guid.NewGuid().ToString();
            var extension = GetExtensionFromContentType(contentType) ?? GetExtensionFromUrl(url) ?? ".jpg";
            return $"{guid}{extension}";
        }

        private static string? GetExtensionFromContentType(string? contentType)
        {
            return contentType?.ToLowerInvariant() switch
            {
                "image/jpeg" => ".jpg",
                "image/jpg" => ".jpg",
                "image/png" => ".png",
                "image/gif" => ".gif",
                "image/webp" => ".webp",
                "image/bmp" => ".bmp",
                _ => null
            };
        }

        private static string? GetExtensionFromUrl(string url)
        {
            var uri = new Uri(url);
            var extension = Path.GetExtension(uri.AbsolutePath);
            return extension;
        }
    }
}
