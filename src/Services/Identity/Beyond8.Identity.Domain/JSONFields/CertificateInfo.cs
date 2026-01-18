using System;

namespace Beyond8.Identity.Domain.JSONFields;

public class CertificateInfo
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public int Year { get; set; } = 0;
}