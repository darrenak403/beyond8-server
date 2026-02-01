using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace Beyond8.Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFullTextSearch_Courses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:unaccent", ",,");

            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "Courses",
                type: "tsvector",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Courses_SearchVector",
                table: "Courses",
                column: "SearchVector")
                .Annotation("Npgsql:IndexMethod", "GIN");

            // Create function to update SearchVector with ALL searchable fields
            // Includes: Title, Slug, ShortDescription, CategoryName (via JOIN), Description,
            // Outcomes, Requirements, TargetAudience, Status, Level, Language, InstructorName
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION courses_search_vector_update() RETURNS trigger AS $$
                DECLARE
                    category_name TEXT := '';
                    status_text TEXT := '';
                    level_text TEXT := '';
                BEGIN
                    -- Get category name from Categories table
                    SELECT ""Name"" INTO category_name FROM ""Categories"" WHERE ""Id"" = NEW.""CategoryId"";
                    
                    -- Convert Status enum to searchable text (both English and Vietnamese)
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
                    
                    -- Convert Level enum to searchable text (both English and Vietnamese)
                    level_text := CASE NEW.""Level""
                        WHEN 0 THEN 'All Tất cả'
                        WHEN 1 THEN 'Beginner Cơ bản Người mới'
                        WHEN 2 THEN 'Intermediate Trung cấp Trung bình'
                        WHEN 3 THEN 'Advanced Nâng cao Chuyên sâu'
                        ELSE ''
                    END;
                    
                    NEW.""SearchVector"" :=
                        -- Weight A: Title (highest priority)
                        setweight(to_tsvector('simple', unaccent(coalesce(NEW.""Title"", ''))), 'A') ||
                        -- Weight B: Slug, ShortDescription, CategoryName
                        setweight(to_tsvector('simple', unaccent(coalesce(NEW.""Slug"", ''))), 'B') ||
                        setweight(to_tsvector('simple', unaccent(coalesce(NEW.""ShortDescription"", ''))), 'B') ||
                        setweight(to_tsvector('simple', unaccent(coalesce(category_name, ''))), 'B') ||
                        -- Weight C: Description, Outcomes, Requirements, TargetAudience, Status, Level, Language
                        setweight(to_tsvector('simple', unaccent(coalesce(NEW.""Description"", ''))), 'C') ||
                        setweight(to_tsvector('simple', unaccent(regexp_replace(coalesce(NEW.""Outcomes""::TEXT, '[]'), '[\[\]"",]', ' ', 'g'))), 'C') ||
                        setweight(to_tsvector('simple', unaccent(regexp_replace(coalesce(NEW.""Requirements""::TEXT, '[]'), '[\[\]"",]', ' ', 'g'))), 'C') ||
                        setweight(to_tsvector('simple', unaccent(regexp_replace(coalesce(NEW.""TargetAudience""::TEXT, '[]'), '[\[\]"",]', ' ', 'g'))), 'C') ||
                        setweight(to_tsvector('simple', unaccent(coalesce(status_text, ''))), 'C') ||
                        setweight(to_tsvector('simple', unaccent(coalesce(level_text, ''))), 'C') ||
                        setweight(to_tsvector('simple', unaccent(coalesce(NEW.""Language"", ''))), 'C') ||
                        -- Weight D: InstructorName (lowest priority)
                        setweight(to_tsvector('simple', unaccent(coalesce(NEW.""InstructorName"", ''))), 'D');
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");

            // Create trigger to auto-update SearchVector on INSERT or UPDATE of any searchable field
            migrationBuilder.Sql(@"
                CREATE TRIGGER courses_search_vector_trigger
                BEFORE INSERT OR UPDATE OF ""Title"", ""Slug"", ""ShortDescription"", ""Description"", ""Outcomes"", ""Requirements"", ""TargetAudience"", ""InstructorName"", ""CategoryId"", ""Status"", ""Level"", ""Language""
                ON ""Courses""
                FOR EACH ROW
                EXECUTE FUNCTION courses_search_vector_update();
            ");

            // Update existing data to populate SearchVector with all fields
            migrationBuilder.Sql(@"
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
                WHERE c.""CategoryId"" = cat.""Id"";
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop trigger first
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS courses_search_vector_trigger ON ""Courses"";");

            // Drop function
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS courses_search_vector_update();");

            migrationBuilder.DropIndex(
                name: "IX_Courses_SearchVector",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "SearchVector",
                table: "Courses");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:unaccent", ",,");
        }
    }
}
