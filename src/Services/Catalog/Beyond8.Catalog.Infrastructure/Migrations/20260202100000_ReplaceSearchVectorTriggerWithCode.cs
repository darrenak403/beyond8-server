using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beyond8.Catalog.Infrastructure.Migrations
{
    /// <summary>
    /// Replaces SearchVector trigger with a function called from application code (CatalogDbContext.SaveChangesAsync).
    /// Same SearchVector logic; no trigger on Courses table.
    /// </summary>
    public partial class ReplaceSearchVectorTriggerWithCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop trigger first
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS courses_search_vector_trigger ON ""Courses"";");

            // Drop old trigger function
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS courses_search_vector_update();");

            // Create function that updates SearchVector for given course IDs (called from C# after SaveChanges)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop function used by code
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS courses_search_vector_update_for_ids(UUID[]);");

            // Restore trigger function
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION courses_search_vector_update() RETURNS trigger AS $$
                DECLARE
                    category_name TEXT := '';
                    status_text TEXT := '';
                    level_text TEXT := '';
                BEGIN
                    SELECT ""Name"" INTO category_name FROM ""Categories"" WHERE ""Id"" = NEW.""CategoryId"";
                    status_text := CASE NEW.""Status""
                        WHEN 0 THEN 'Draft Bản nháp'
                        WHEN 1 THEN 'PendingApproval Chờ duyệt Pending'
                        WHEN 2 THEN 'Approved Đã duyệt'
                        WHEN 3 THEN 'Rejected Từ chối'
                        WHEN 4 THEN 'Published Công khai Đã xuất bản'
                        WHEN 5 THEN 'Archived Lưu trữ'
                        WHEN 6 THEN 'Suspended Tạm ngưng'
                        ELSE ''
                    END;
                    level_text := CASE NEW.""Level""
                        WHEN 0 THEN 'All Tất cả'
                        WHEN 1 THEN 'Beginner Cơ bản Người mới'
                        WHEN 2 THEN 'Intermediate Trung cấp Trung bình'
                        WHEN 3 THEN 'Advanced Nâng cao Chuyên sâu'
                        ELSE ''
                    END;
                    NEW.""SearchVector"" :=
                        setweight(to_tsvector('simple', unaccent(coalesce(NEW.""Title"", ''))), 'A') ||
                        setweight(to_tsvector('simple', unaccent(coalesce(NEW.""Slug"", ''))), 'B') ||
                        setweight(to_tsvector('simple', unaccent(coalesce(NEW.""ShortDescription"", ''))), 'B') ||
                        setweight(to_tsvector('simple', unaccent(coalesce(category_name, ''))), 'B') ||
                        setweight(to_tsvector('simple', unaccent(coalesce(NEW.""Description"", ''))), 'C') ||
                        setweight(to_tsvector('simple', unaccent(regexp_replace(coalesce(NEW.""Outcomes""::TEXT, '[]'), '[\[\]"",]', ' ', 'g'))), 'C') ||
                        setweight(to_tsvector('simple', unaccent(regexp_replace(coalesce(NEW.""Requirements""::TEXT, '[]'), '[\[\]"",]', ' ', 'g'))), 'C') ||
                        setweight(to_tsvector('simple', unaccent(regexp_replace(coalesce(NEW.""TargetAudience""::TEXT, '[]'), '[\[\]"",]', ' ', 'g'))), 'C') ||
                        setweight(to_tsvector('simple', unaccent(coalesce(status_text, ''))), 'C') ||
                        setweight(to_tsvector('simple', unaccent(coalesce(level_text, ''))), 'C') ||
                        setweight(to_tsvector('simple', unaccent(coalesce(NEW.""Language"", ''))), 'C') ||
                        setweight(to_tsvector('simple', unaccent(coalesce(NEW.""InstructorName"", ''))), 'D');
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");

            // Restore trigger
            migrationBuilder.Sql(@"
                CREATE TRIGGER courses_search_vector_trigger
                BEFORE INSERT OR UPDATE OF ""Title"", ""Slug"", ""ShortDescription"", ""Description"", ""Outcomes"", ""Requirements"", ""TargetAudience"", ""InstructorName"", ""CategoryId"", ""Status"", ""Level"", ""Language""
                ON ""Courses""
                FOR EACH ROW
                EXECUTE FUNCTION courses_search_vector_update();
            ");
        }
    }
}
