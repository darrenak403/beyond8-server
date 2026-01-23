using Beyond8.Identity.Domain.Entities;
using Beyond8.Identity.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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
            new("00000000-0000-0000-0000-000000000001"),
            new("00000000-0000-0000-0000-000000000002"),
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
                UserRoles = [
                    new() {
                        RoleId = roleIds[0],
                        UserId = userIds[0],
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
                UserRoles = [
                    new() {
                        UserId = userIds[1],
                        RoleId = roleIds[1],
                    }
                ],
            }
        };
        await context.Roles.AddRangeAsync(roles);
        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();
    }
}
