using System;

namespace Beyond8.Integration.Application.Dtos.VnptEkyc;

public class CompareFaceRequest
{
    public string ImgFront { get; set; } = string.Empty;
    public string ImgFace { get; set; } = string.Empty;
    public string ClientSession { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}
