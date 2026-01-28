namespace Beyond8.Integration.Application.Dtos.MediaFiles
{
    public class MediaFileInfoDto
    {
        public Guid FileId { get; set; }
        public Guid UserId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileExtension { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long Size { get; set; }
        public string SizeFormatted { get; set; } = string.Empty;
        public string Folder { get; set; } = string.Empty;
        public string? SubFolder { get; set; }
        public string S3Key { get; set; } = string.Empty;
        public string CloudFrontUrl { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? UploadedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
