namespace Beyond8.Integration.Application.Dtos.MediaFiles
{
    public class UploadFileResponse
    {
        public Guid FileId { get; set; }
        public string PresignedUrl { get; set; } = null!;
        public string FileKey { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public string FileName { get; set; } = null!;
        public string ContentType { get; set; } = null!;
    }
}
