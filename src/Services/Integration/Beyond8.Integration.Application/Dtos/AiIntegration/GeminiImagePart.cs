namespace Beyond8.Integration.Application.Dtos.AiIntegration;

/// <summary>Ảnh hoặc tài liệu (PDF) gửi kèm cho Gemini dạng inline base64.</summary>
public record GeminiImagePart(byte[] Data, string MimeType);
