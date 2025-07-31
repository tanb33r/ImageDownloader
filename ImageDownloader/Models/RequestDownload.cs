using System.ComponentModel.DataAnnotations;

namespace ImageDownloader.Models
{
    public class RequestDownload
    {
        [Required]
        public IEnumerable<string> ImageUrls { get; set; } = new List<string>();
        
        [Range(1, 50, ErrorMessage = "MaxDownloadAtOnce must be between 1 and 50")]
        public int MaxDownloadAtOnce { get; set; } = 5;
    }
}