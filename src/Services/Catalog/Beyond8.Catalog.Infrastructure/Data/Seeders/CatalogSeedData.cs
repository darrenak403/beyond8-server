using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beyond8.Catalog.Domain.Entities;
using Beyond8.Catalog.Domain.Enums; // Đảm bảo namespace này tồn tại chứa Enum CategoryType
using Microsoft.EntityFrameworkCore;

namespace Beyond8.Catalog.Infrastructure.Data.Seeders;

public static class CatalogSeedData
{
    public static async Task SeedCategoriesAsync(CatalogDbContext context)
    {
        // Kiểm tra xem đã có dữ liệu chưa
        if (await context.Categories.AnyAsync())
        {
            return;
        }

        var categories = new List<Category>();

        // --- 1. LẬP TRÌNH ---
        var devRoot = CreateCategory("Lập trình", "lap-trinh", "Các khóa học về tư duy và kỹ thuật phần mềm.", CategoryType.Technology);
        categories.Add(devRoot);
        categories.AddRange(CreateSubCategories(devRoot,
        [
            ("Lập trình Web", "lap-trinh-web", "Frontend, Backend, Fullstack."),
            ("Lập trình Mobile", "lap-trinh-mobile", "iOS, Android, React Native, Flutter.")
        ]));

        // --- 2. THIẾT KẾ ---
        var designRoot = CreateCategory("Thiết kế", "thiet-ke", "Tư duy thẩm mỹ và công cụ sáng tạo.", CategoryType.Design);
        categories.Add(designRoot);
        categories.AddRange(CreateSubCategories(designRoot,
        [
            ("Thiết kế UI/UX", "thiet-ke-ui-ux", "Trải nghiệm và giao diện người dùng."),
            ("Thiết kế Đồ họa", "thiet-ke-do-hoa", "Photoshop, AI, Branding.")
        ]));

        // --- 3. NGÔN NGỮ ---
        var langRoot = CreateCategory("Ngôn ngữ", "ngon-ngu", "Đào tạo ngoại ngữ các cấp độ.", CategoryType.Language);
        categories.Add(langRoot);
        categories.AddRange(CreateSubCategories(langRoot,
        [
            ("Tiếng Anh", "tieng-anh", "Giao tiếp, TOEIC, IELTS."),
            ("Tiếng Nhật", "tieng-nhat", "Sơ cấp N5 đến Cao cấp N1.")
        ]));

        // --- 4. KINH DOANH ---
        var bizRoot = CreateCategory("Kinh doanh", "kinh-doanh", "Kiến thức quản trị và khởi nghiệp.", CategoryType.Business);
        categories.Add(bizRoot);
        categories.AddRange(CreateSubCategories(bizRoot,
        [
            ("Khởi nghiệp", "khoi-nghiep", "Xây dựng mô hình kinh doanh."),
            ("Quản trị nhân sự", "quan-tri-nhan-su", "Tuyển dụng và đào tạo nhân tài.")
        ]));

        // --- 5. MARKETING ---
        var mktRoot = CreateCategory("Marketing", "marketing", "Tiếp thị số và thương hiệu.", CategoryType.Marketing);
        categories.Add(mktRoot);
        categories.AddRange(CreateSubCategories(mktRoot,
        [
            ("Digital Marketing", "digital-marketing", "SEO, Google Ads, Social Media."),
            ("Content Marketing", "content-marketing", "Copywriting và sáng tạo nội dung.")
        ]));

        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();
    }

    private static Category CreateCategory(
        string name,
        string slug,
        string description,
        CategoryType type = CategoryType.Other)
    {
        var id = Guid.NewGuid();
        return new Category
        {
            Id = id,
            Name = name,
            Slug = slug,
            Description = description,
            ParentId = null,
            Level = 0,
            Path = id.ToString(),
            IsActive = true,
            TotalCourses = 0,
            IsRoot = true,
            Type = type
        };
    }

    private static IEnumerable<Category> CreateSubCategories(
        Category parent,
        (string Name, string Slug, string Desc)[] subs)
    {
        var list = new List<Category>();
        foreach (var sub in subs)
        {
            var id = Guid.NewGuid();
            list.Add(new Category
            {
                Id = id,
                Name = sub.Name,
                Slug = sub.Slug,
                Description = sub.Desc,
                ParentId = parent.Id,
                Level = parent.Level + 1,
                Path = $"{parent.Path}/{id}",
                IsActive = true,
                TotalCourses = 0,
                IsRoot = false,
                Type = parent.Type
            });
        }
        return list;
    }
}