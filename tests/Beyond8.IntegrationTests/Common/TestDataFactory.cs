using Beyond8.Identity.Application.Dtos.Instructors;
using Beyond8.Identity.Domain.JSONFields;

namespace Beyond8.IntegrationTests.Common;

internal static class TestDataFactory
{
    public static CreateInstructorProfileRequest BuildValidCreateInstructorRequest()
    {
        return new CreateInstructorProfileRequest
        {
            Bio = "Backend instructor profile for flow test.",
            Headline = "Senior Backend Instructor",
            ExpertiseAreas = ["ASP.NET Core", "Microservices"],
            Education =
            [
                new EducationInfo
                {
                    School = "FPT University",
                    Degree = "Bachelor",
                    FieldOfStudy = "Software Engineering",
                    Start = 2014,
                    End = 2018
                }
            ],
            WorkExperience =
            [
                new WorkInfo
                {
                    Company = "Beyond8",
                    Role = "Tech Lead",
                    From = new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    IsCurrentJob = true,
                    Description = "Leading backend architecture."
                }
            ],
            BankInfo = new BankInfo
            {
                BankName = "Vietcombank",
                AccountNumber = "1234567890",
                AccountHolderName = "NGUYEN VAN A"
            },
            TeachingLanguages = ["vi-VN"],
            IntroVideoUrl = "https://cdn.example.com/intro.mp4",
            IdentityDocuments =
            [
                new IdentityInfo
                {
                    Type = "CCCD",
                    Number = "079201000999",
                    IssuerDate = new DateTime(2018, 5, 20, 0, 0, 0, DateTimeKind.Utc),
                    FrontImg = "https://cdn.example.com/id-front.jpg",
                    BackImg = "https://cdn.example.com/id-back.jpg"
                }
            ],
            Certificates =
            [
                new CertificateInfo
                {
                    Name = "AWS Certified Developer",
                    Url = "https://example.com/aws-dev",
                    Issuer = "AWS",
                    Year = 2023
                }
            ]
        };
    }
}
