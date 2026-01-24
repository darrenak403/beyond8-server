using System;
using Beyond8.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Beyond8.Catalog.Infrastructure.Data.Seeders;

public static class CatalogSeedData
{
    public static async Task SeedCategoriesAsync(CatalogDbContext context)
    {
        if (await context.Categories.AnyAsync())
        {
            return;
        }

        var categories = new List<Category>();

        // --- 1. LẬP TRÌNH ---
        var devRoot = CreateCategory("Lập trình", "lap-trinh", "Các khóa học về tư duy và kỹ thuật phần mềm.");
        categories.Add(devRoot);
        categories.AddRange(CreateSubCategories(devRoot, new[]
        {
            ("Lập trình Web", "lap-trinh-web", "Frontend, Backend, Fullstack."),
            ("Lập trình Mobile", "lap-trinh-mobile", "iOS, Android, React Native, Flutter.")
        }));

        // --- 2. THIẾT KẾ ---
        var designRoot = CreateCategory("Thiết kế", "thiet-ke", "Tư duy thẩm mỹ và công cụ sáng tạo.");
        categories.Add(designRoot);
        categories.AddRange(CreateSubCategories(designRoot, new[]
        {
            ("Thiết kế UI/UX", "thiet-ke-ui-ux", "Trải nghiệm và giao diện người dùng."),
            ("Thiết kế Đồ họa", "thiet-ke-do-hoa", "Photoshop, AI, Branding.")
        }));

        // --- 3. NGÔN NGỮ ---
        var langRoot = CreateCategory("Ngôn ngữ", "ngon-ngu", "Đào tạo ngoại ngữ các cấp độ.");
        categories.Add(langRoot);
        categories.AddRange(CreateSubCategories(langRoot, new[]
        {
            ("Tiếng Anh", "tieng-anh", "Giao tiếp, TOEIC, IELTS."),
            ("Tiếng Nhật", "tieng-nhat", "Sơ cấp N5 đến Cao cấp N1.")
        }));

        // --- 4. KINH DOANH ---
        var bizRoot = CreateCategory("Kinh doanh", "kinh-doanh", "Kiến thức quản trị và khởi nghiệp.");
        categories.Add(bizRoot);
        categories.AddRange(CreateSubCategories(bizRoot, new[]
        {
            ("Khởi nghiệp", "khoi-nghiep", "Xây dựng mô hình kinh doanh."),
            ("Quản trị nhân sự", "quan-tri-nhan-su", "Tuyển dụng và đào tạo nhân tài.")
        }));

        // --- 5. MARKETING ---
        var mktRoot = CreateCategory("Marketing", "marketing", "Tiếp thị số và thương hiệu.");
        categories.Add(mktRoot);
        categories.AddRange(CreateSubCategories(mktRoot, new[]
        {
            ("Digital Marketing", "digital-marketing", "SEO, Google Ads, Social Media."),
            ("Content Marketing", "content-marketing", "Copywriting và sáng tạo nội dung.")
        }));

        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();
    }


    private static Category CreateCategory(string name, string slug, string description)
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
        };
    }

    private static IEnumerable<Category> CreateSubCategories(Category parent, (string Name, string Slug, string Desc)[] subs)
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
            });
        }
        return list;
    }
}
