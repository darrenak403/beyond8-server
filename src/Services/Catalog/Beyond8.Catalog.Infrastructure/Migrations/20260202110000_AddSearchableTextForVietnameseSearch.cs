using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beyond8.Catalog.Infrastructure.Migrations
{
    /// <summary>
    /// Adds SearchableText column (normalized in C# with RemoveDiacritics) so full-text search
    /// matches Vietnamese without diacritics (e.g. "lap trinh" matches "Lập trình").
    /// </summary>
    public partial class AddSearchableTextForVietnameseSearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SearchableText",
                table: "Courses",
                type: "character varying(10000)",
                maxLength: 10000,
                nullable: true);

            // Replace function: when SearchableText is set, use it for SearchVector; else fall back to unaccent
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION courses_search_vector_update_for_ids(p_ids UUID[])
                RETURNS void
                LANGUAGE plpgsql
                AS $$
                BEGIN
                    UPDATE ""Courses"" c SET ""SearchVector"" =
                        CASE
                            WHEN c.""SearchableText"" IS NOT NULL AND trim(c.""SearchableText"") != '' THEN
                                to_tsvector('simple', c.""SearchableText"")
                            ELSE
                                setweight(to_tsvector('simple', unaccent(coalesce(c.""Title"", ''))), 'A') ||
                                setweight(to_tsvector('simple', unaccent(coalesce(c.""Slug"", ''))), 'B') ||
                                setweight(to_tsvector('simple', unaccent(coalesce(c.""ShortDescription"", ''))), 'B') ||
                                setweight(to_tsvector('simple', unaccent(coalesce(cat.""Name"", ''))), 'B') ||
                                setweight(to_tsvector('simple', unaccent(coalesce(c.""Description"", ''))), 'C') ||
                                setweight(to_tsvector('simple', unaccent(regexp_replace(coalesce(c.""Outcomes""::TEXT, '[]'), '[\[\]"",]', ' ', 'g'))), 'C') ||
                                setweight(to_tsvector('simple', unaccent(regexp_replace(coalesce(c.""Requirements""::TEXT, '[]'), '[\[\]"",]', ' ', 'g'))), 'C') ||
                                setweight(to_tsvector('simple', unaccent(regexp_replace(coalesce(c.""TargetAudience""::TEXT, '[]'), '[\[\]"",]', ' ', 'g'))), 'C') ||
                                setweight(to_tsvector('simple', unaccent(
                                    CASE c.""Status""
                                        WHEN 0 THEN 'Draft Bản nháp'
                                        WHEN 1 THEN 'PendingApproval Chờ duyệt Pending'
                                        WHEN 2 THEN 'Approved Đã duyệt'
                                        WHEN 3 THEN 'Rejected Từ chối'
                                        WHEN 4 THEN 'Published Công khai Đã xuất bản'
                                        WHEN 5 THEN 'Archived Lưu trữ'
                                        WHEN 6 THEN 'Suspended Tạm ngưng'
                                        ELSE ''
                                    END
                                )), 'C') ||
                                setweight(to_tsvector('simple', unaccent(
                                    CASE c.""Level""
                                        WHEN 0 THEN 'All Tất cả'
                                        WHEN 1 THEN 'Beginner Cơ bản Người mới'
                                        WHEN 2 THEN 'Intermediate Trung cấp Trung bình'
                                        WHEN 3 THEN 'Advanced Nâng cao Chuyên sâu'
                                        ELSE ''
                                    END
                                )), 'C') ||
                                setweight(to_tsvector('simple', unaccent(coalesce(c.""Language"", ''))), 'C') ||
                                setweight(to_tsvector('simple', unaccent(coalesce(c.""InstructorName"", ''))), 'D')
                        END
                    FROM ""Categories"" cat
                    WHERE c.""CategoryId"" = cat.""Id"" AND c.""Id"" = ANY(p_ids);
                END;
                $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SearchableText",
                table: "Courses");

            // Restore original function (no SearchableText)
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION courses_search_vector_update_for_ids(p_ids UUID[])
                RETURNS void
                LANGUAGE plpgsql
                AS $$
                BEGIN
                    UPDATE ""Courses"" c SET ""SearchVector"" =
                        setweight(to_tsvector('simple', unaccent(coalesce(c.""Title"", ''))), 'A') ||
                        setweight(to_tsvector('simple', unaccent(coalesce(c.""Slug"", ''))), 'B') ||
                        setweight(to_tsvector('simple', unaccent(coalesce(c.""ShortDescription"", ''))), 'B') ||
                        setweight(to_tsvector('simple', unaccent(coalesce(cat.""Name"", ''))), 'B') ||
                        setweight(to_tsvector('simple', unaccent(coalesce(c.""Description"", ''))), 'C') ||
                        setweight(to_tsvector('simple', unaccent(regexp_replace(coalesce(c.""Outcomes""::TEXT, '[]'), '[\[\]"",]', ' ', 'g'))), 'C') ||
                        setweight(to_tsvector('simple', unaccent(regexp_replace(coalesce(c.""Requirements""::TEXT, '[]'), '[\[\]"",]', ' ', 'g'))), 'C') ||
                        setweight(to_tsvector('simple', unaccent(regexp_replace(coalesce(c.""TargetAudience""::TEXT, '[]'), '[\[\]"",]', ' ', 'g'))), 'C') ||
                        setweight(to_tsvector('simple', unaccent(
                            CASE c.""Status""
                                WHEN 0 THEN 'Draft Bản nháp'
                                WHEN 1 THEN 'PendingApproval Chờ duyệt Pending'
                                WHEN 2 THEN 'Approved Đã duyệt'
                                WHEN 3 THEN 'Rejected Từ chối'
                                WHEN 4 THEN 'Published Công khai Đã xuất bản'
                                WHEN 5 THEN 'Archived Lưu trữ'
                                WHEN 6 THEN 'Suspended Tạm ngưng'
                                ELSE ''
                            END
                        )), 'C') ||
                        setweight(to_tsvector('simple', unaccent(
                            CASE c.""Level""
                                WHEN 0 THEN 'All Tất cả'
                                WHEN 1 THEN 'Beginner Cơ bản Người mới'
                                WHEN 2 THEN 'Intermediate Trung cấp Trung bình'
                                WHEN 3 THEN 'Advanced Nâng cao Chuyên sâu'
                                ELSE ''
                            END
                        )), 'C') ||
                        setweight(to_tsvector('simple', unaccent(coalesce(c.""Language"", ''))), 'C') ||
                        setweight(to_tsvector('simple', unaccent(coalesce(c.""InstructorName"", ''))), 'D')
                    FROM ""Categories"" cat
                    WHERE c.""CategoryId"" = cat.""Id"" AND c.""Id"" = ANY(p_ids);
                END;
                $$;
            ");
        }
    }
}
