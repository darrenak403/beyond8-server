using System;

namespace Beyond8.Integration.Application.Dtos.Ai;

public class ProfileReviewRequest
{
    public string Bio { get; set; } = string.Empty;
    public string Headline { get; set; } = string.Empty;

    public List<string> ExpertiseAreas { get; set; } = [];

    public List<EducationInfo> Education { get; set; } = [];

    public List<WorkInfo> WorkExperience { get; set; } = [];

    public List<CertificateInfo> Certificates { get; set; } = [];

    public List<string> TeachingLanguages { get; set; } = [];
}

public class EducationInfo
{
    public string School { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public string FieldOfStudy { get; set; } = string.Empty;
    public int Start { get; set; }
    public int End { get; set; }
}

public class WorkInfo
{
    public string Company { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime From { get; set; } = DateTime.MinValue;
    public DateTime To { get; set; } = DateTime.MinValue;
    public bool IsCurrentJob { get; set; } = false;
    public string? Description { get; set; }
}

public class CertificateInfo
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public int Year { get; set; }
}
