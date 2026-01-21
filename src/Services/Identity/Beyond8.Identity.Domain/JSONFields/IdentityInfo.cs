using System;

namespace Beyond8.Identity.Domain.JSONFields;

public class IdentityInfo
{
    public string Type { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string IssuedDate { get; set; } = string.Empty;
    public string FrontImg { get; set; } = string.Empty;
    public string BackImg { get; set; } = string.Empty;
}
