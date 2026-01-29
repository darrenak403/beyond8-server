using Beyond8.Integration.Domain.Enums;

namespace Beyond8.Integration.Application.Dtos.MediaFiles
{
    public class MediaFileDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public StorageProvider Provider { get; set; }
        public string FilePath { get; set; } = null!;
        public string FileUrl { get; set; } = null!;
        public string OriginalFileName { get; set; } = null!;
        public string ContentType { get; set; } = null!;
        public string Extension { get; set; } = null!;
        public long Size { get; set; }
        public FileStatus Status { get; set; }
        public string? Metadata { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
