namespace Beyond8.Integration.Application.Dtos.MediaFiles;

public class UploadFileRequest
{
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long Size { get; set; }
    public string? Metadata { get; set; }
}
