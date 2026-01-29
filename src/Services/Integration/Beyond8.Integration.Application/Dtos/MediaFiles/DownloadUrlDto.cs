namespace Beyond8.Integration.Application.Dtos.MediaFiles
{
    public class DownloadUrlDto
    {
        public string DownloadUrl { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public string ExpiresIn { get; set; } = string.Empty;
    }
}
