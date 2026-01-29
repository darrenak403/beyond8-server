namespace Beyond8.Integration.Application.Dtos.AiIntegration;

/// <summary>Ảnh hoặc tài liệu (PDF) gửi kèm cho AI dạng inline base64.</summary>
public record GenerativeAiImagePart(byte[] Data, string MimeType);
