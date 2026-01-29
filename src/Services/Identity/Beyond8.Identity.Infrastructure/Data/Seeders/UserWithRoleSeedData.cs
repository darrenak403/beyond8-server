using Beyond8.Identity.Domain.Entities;
using Beyond8.Identity.Domain.Enums;
using Beyond8.Identity.Domain.JSONFields;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Beyond8.Identity.Infrastructure.Data.Seeders;

public static class UserWithRoleSeedData
{
    public static async Task SeedUserWithRoleAsync(IdentityDbContext context)
    {
        if (await context.Users.AnyAsync() && await context.Roles.AnyAsync())
        {
            return; // Users and Roles already seeded
        }

        var roleIds = new List<Guid>()
        {
            new("00000000-0000-0000-0000-000000000001"),
            new("00000000-0000-0000-0000-000000000002"),
            new("00000000-0000-0000-0000-000000000003"),
            new("00000000-0000-0000-0000-000000000004")
        };

        var roles = new List<Role>()
        {
            new() {
                Id = roleIds[0],
                Code = "ROLE_ADMIN",
                Name = "Admin",
                Description = "Quản trị viên hệ thống với quyền truy cập đầy đủ",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = Guid.Empty
            },
            new() {
                Id = roleIds[1],
                Code = "ROLE_STAFF",
                Name = "Staff",
                Description = "Nhân viên hỗ trợ quản lý hệ thống",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = Guid.Empty
            },
            new() {
                Id = roleIds[2],
                Code = "ROLE_INSTRUCTOR",
                Name = "Instructor",
                Description = "Giảng viên có quyền tạo và quản lý khóa học",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = Guid.Empty
            },
            new() {
                Id = roleIds[3],
                Code = "ROLE_STUDENT",
                Name = "Student",
                Description = "Học viên có quyền tham gia các khóa học",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = Guid.Empty
            }
        };


        var userIds = new List<Guid>()
        {
            new("00000000-0000-0000-0000-000000000001"), // Admin
            new("00000000-0000-0000-0000-000000000002"), // Staff
            new("00000000-0000-0000-0000-000000000005"), // Student
            new("00000000-0000-0000-0000-000000000006"), // Instructor
        };
        var defaultPassword = "12345@Abc";
        var hashedPassword = new PasswordHasher<User>().HashPassword(new User(), defaultPassword);
        var users = new List<User>()
        {
            new() {
                Id = userIds[0],
                Email = "admin@gmail.com",
                PasswordHash = hashedPassword,
                FullName = "AdminSeed",
                IsEmailVerified = true,
                Status = UserStatus.Active,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = Guid.Empty,
                UserRoles = [
                    new() {
                        RoleId = roleIds[0],
                        UserId = userIds[0],
                        AssignedAt = DateTime.UtcNow
                    }
                ],
            },
            new() {
                Id = userIds[1],
                Email = "staff@gmail.com",
                PasswordHash = hashedPassword,
                FullName = "StaffSeed",
                IsEmailVerified = true,
                Status = UserStatus.Active,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = Guid.Empty,
                UserRoles = [
                    new() {
                        UserId = userIds[1],
                        RoleId = roleIds[1],
                        AssignedAt = DateTime.UtcNow
                    }
                ],
            },
            // Student user với full profile
            new() {
                Id = userIds[2],
                Email = "student@gmail.com",
                PasswordHash = hashedPassword,
                FullName = "Nguyễn Văn Học",
                IsEmailVerified = true,
                Status = UserStatus.Active,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = Guid.Empty,
                DateOfBirth = DateTime.SpecifyKind(new DateTime(1995, 5, 15), DateTimeKind.Utc),
                PhoneNumber = "0912345678",
                Address = "123 Đường ABC, Quận 1, TP.HCM",
                Bio = "Sinh viên đam mê học tập và phát triển bản thân",
                AvatarUrl = "user/avatars/00000000-0000-0000-0000-000000000005/avatar.jpg",
                CoverUrl = "user/covers/00000000-0000-0000-0000-000000000005/cover.jpg",
                Specialization = "Công nghệ thông tin",
                UserRoles = [
                    new() {
                        UserId = userIds[2],
                        RoleId = roleIds[3],
                        AssignedAt = DateTime.UtcNow
                    }
                ],
            },
            // Instructor user với full profile
            new() {
                Id = userIds[3],
                Email = "instructor@gmail.com",
                PasswordHash = hashedPassword,
                FullName = "Trần Thị Giảng Viên",
                IsEmailVerified = true,
                Status = UserStatus.Active,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = Guid.Empty,
                DateOfBirth = DateTime.SpecifyKind(new DateTime(1985, 8, 20), DateTimeKind.Utc),
                PhoneNumber = "0987654321",
                Address = "456 Đường XYZ, Quận 3, TP.HCM",
                Bio = "Giảng viên với hơn 10 năm kinh nghiệm trong lĩnh vực công nghệ thông tin",
                AvatarUrl = "user/avatars/00000000-0000-0000-0000-000000000006/avatar.jpg",
                CoverUrl = "user/covers/00000000-0000-0000-0000-000000000006/cover.jpg",
                Specialization = "Full Stack Development",
                UserRoles = [
                    new() {
                        UserId = userIds[3],
                        RoleId = roleIds[2],
                        AssignedAt = DateTime.UtcNow
                    }
                ],
            }
        };

        var instructorProfileId = new Guid("00000000-0000-0000-0000-000000000007");
        var adminId = userIds[0];

        var expertiseAreas = new List<string> { "Python", "C#", "Rust", "Tailwind CSS", "Java", "React", "C++", "PHP", "Ruby", "Go", "Node.js" };
        var education = new List<EducationInfo>
        {
            new()
            {
                School = "Đại học FPT",
                Degree = "Cử nhân kỹ thuật phần mềm",
                FieldOfStudy = "Công nghệ thông tin",
                Start = 2021,
                End = 2024
            }
        };
        var workExperience = new List<WorkInfo>
        {
            new()
            {
                Company = "Tech Company",
                Role = "Software Engineer",
                From = new DateTime(2023, 1, 8),
                To = new DateTime(2026, 1, 6),
                IsCurrentJob = true,
                Description = "Phát triển và xây dựng hệ thống backend cho doanh nghiệp"
            }
        };
        var socialLinks = new SocialInfo
        {
            Facebook = "https://www.facebook.com/profile.php?id=100000000000000",
            LinkedIn = "https://www.linkedin.com/in/software-engineer-solution-architect",
            Website = "https://portfolio.beyond8.edu.vn/instructor"
        };
        var bankInfo = new BankInfo
        {
            BankName = "Vietcombank",
            AccountNumber = "1234567890",
            AccountHolderName = "Trần Thị Giảng Viên"
        };
        var identityDocuments = new List<IdentityInfo>
        {
            new()
            {
                Type = "Căn cước công dân",
                Number = "094204016524",
                IssuerDate = new DateTime(2010, 1, 15),
                FrontImg = "https://d30z0qh7rhzgt8.cloudfront.net/instructor/profile/identity-cards/019bf413-737c-73b4-a422-df0bcd8af038/back/53866b95f8fe4048990725e319f9cd57_5111e7c1-a26e-4266-b642-ea9299aed2be.jpg",
                BackImg = "https://d30z0qh7rhzgt8.cloudfront.net/instructor/profile/identity-cards/019bf413-737c-73b4-a422-df0bcd8af038/front/15075121a6514b11a2c5afee9635b269_11bfca86-d31a-4748-a913-e1961b37e5c4.jpg"
            }
        };
        var certificates = new List<CertificateInfo>
        {
            new()
            {
                Name = "Project Management Principles and Practices",
                Url = "https://d30z0qh7rhzgt8.cloudfront.net/instructor/profile/certificates/019bf413-737c-73b4-a422-df0bcd8af038/012781eb6612484f800d0b7539760271_CERTIFICATE_LANDING_PAGE~5EUNXZHOYNNC.jpeg",
                Issuer = "University of California, Irvine",
                Year = 2026
            }
        };

        var instructorProfile = new InstructorProfile
        {
            Id = instructorProfileId,
            UserId = userIds[3],
            Bio = "Giảng viên CNTT với hơn 5 năm kinh nghiệm trong giảng dạy, phát triển web và xây dựng hệ thống backend cho doanh nghiệp.",
            Headline = "Software Engineer | Solution Architecture | Prompt Engineer",
            TaxId = "123456789012",
            TeachingLanguages = new List<string> { "vi-VN", "en-US" },
            IntroVideoUrl = "https://d30z0qh7rhzgt8.cloudfront.net/instructor/profile/intro-videos/019bf413-737c-73b4-a422-df0bcd8af038/6067f6a0ece94105b336f1daee328212_meo_con_lon_ton.mp4",
            ExpertiseAreas = JsonSerializer.Serialize(expertiseAreas),
            Education = JsonSerializer.Serialize(education),
            WorkExperience = JsonSerializer.Serialize(workExperience),
            SocialLinks = JsonSerializer.Serialize(socialLinks),
            BankInfo = JsonSerializer.Serialize(bankInfo),
            IdentityDocuments = JsonSerializer.Serialize(identityDocuments),
            Certificates = JsonSerializer.Serialize(certificates),
            VerificationStatus = VerificationStatus.Verified,
            VerifiedBy = adminId,
            VerifiedAt = DateTime.UtcNow,
            TotalStudents = 150,
            TotalCourses = 5,
            AvgRating = 4.8m,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = Guid.Empty
        };

        await context.Roles.AddRangeAsync(roles);
        await context.Users.AddRangeAsync(users);
        await context.InstructorProfiles.AddAsync(instructorProfile);
        await context.SaveChangesAsync();
    }
}
