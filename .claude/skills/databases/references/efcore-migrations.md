# EF Core Migrations Guide

## Migration Commands

### Create Migration
```bash
# From solution root, specify project paths
dotnet ef migrations add InitialCreate \
    --project src/Services/Catalog/Beyond8.Catalog.Infrastructure \
    --startup-project src/Services/Catalog/Beyond8.Catalog.Api

# With context specification (if multiple DbContexts)
dotnet ef migrations add AddCourseStatus \
    --context CatalogDbContext \
    --project src/Services/Catalog/Beyond8.Catalog.Infrastructure \
    --startup-project src/Services/Catalog/Beyond8.Catalog.Api
```

### Apply Migrations
```bash
# Update to latest
dotnet ef database update \
    --project src/Services/Catalog/Beyond8.Catalog.Infrastructure \
    --startup-project src/Services/Catalog/Beyond8.Catalog.Api

# Update to specific migration
dotnet ef database update AddCourseStatus \
    --project src/Services/Catalog/Beyond8.Catalog.Infrastructure \
    --startup-project src/Services/Catalog/Beyond8.Catalog.Api
```

### Remove Last Migration
```bash
# Only if not applied to database
dotnet ef migrations remove \
    --project src/Services/Catalog/Beyond8.Catalog.Infrastructure \
    --startup-project src/Services/Catalog/Beyond8.Catalog.Api
```

### Generate SQL Script
```bash
# Full script
dotnet ef migrations script \
    --project src/Services/Catalog/Beyond8.Catalog.Infrastructure \
    --startup-project src/Services/Catalog/Beyond8.Catalog.Api \
    --output migration.sql

# From specific migration
dotnet ef migrations script FromMigration ToMigration \
    --output migration.sql
```

## Auto-Migration on Startup

Beyond8 uses `MigrateDbContextAsync` helper:

```csharp
// Program.cs
await app.MigrateDbContextAsync<CatalogDbContext>(async (database, cancellationToken) =>
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    await CatalogSeedData.SeedCategoriesAsync(context);
    await CatalogSeedData.SeedCoursesAsync(context);
});
```

## Migration File Structure

```csharp
public partial class AddCourseDocuments : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "CourseDocuments",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CourseId = table.Column<Guid>(type: "uuid", nullable: false),
                Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                FileUrl = table.Column<string>(type: "text", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CourseDocuments", x => x.Id);
                table.ForeignKey(
                    name: "FK_CourseDocuments_Courses_CourseId",
                    column: x => x.CourseId,
                    principalTable: "Courses",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_CourseDocuments_CourseId",
            table: "CourseDocuments",
            column: "CourseId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "CourseDocuments");
    }
}
```

## Common Migration Operations

### Add Column
```csharp
migrationBuilder.AddColumn<string>(
    name: "ThumbnailUrl",
    table: "Courses",
    type: "text",
    nullable: true);
```

### Add Column with Default
```csharp
migrationBuilder.AddColumn<bool>(
    name: "IsActive",
    table: "Courses",
    type: "boolean",
    nullable: false,
    defaultValue: true);
```

### Rename Column
```csharp
migrationBuilder.RenameColumn(
    name: "OldName",
    table: "Courses",
    newName: "NewName");
```

### Drop Column
```csharp
migrationBuilder.DropColumn(
    name: "ObsoleteColumn",
    table: "Courses");
```

### Add Index
```csharp
migrationBuilder.CreateIndex(
    name: "IX_Courses_Slug",
    table: "Courses",
    column: "Slug",
    unique: true);

// Composite index
migrationBuilder.CreateIndex(
    name: "IX_Courses_Status_CreatedAt",
    table: "Courses",
    columns: new[] { "Status", "CreatedAt" });
```

### Add Foreign Key
```csharp
migrationBuilder.AddForeignKey(
    name: "FK_Sections_Courses_CourseId",
    table: "Sections",
    column: "CourseId",
    principalTable: "Courses",
    principalColumn: "Id",
    onDelete: ReferentialAction.Cascade);
```

## Data Migrations

### Insert Seed Data
```csharp
migrationBuilder.InsertData(
    table: "Categories",
    columns: new[] { "Id", "Name", "Slug", "IsActive", "CreatedAt" },
    values: new object[]
    {
        Guid.Parse("..."),
        "Lập trình",
        "lap-trinh",
        true,
        DateTime.UtcNow
    });
```

### Update Existing Data
```csharp
migrationBuilder.Sql(
    "UPDATE \"Courses\" SET \"Status\" = 4 WHERE \"Status\" = 2 AND \"ApprovedAt\" IS NOT NULL");
```

### Data Migration with Logic
```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // Add new column
    migrationBuilder.AddColumn<int>(
        name: "TotalDurationMinutes",
        table: "Courses",
        type: "integer",
        nullable: false,
        defaultValue: 0);

    // Migrate data
    migrationBuilder.Sql(@"
        UPDATE ""Courses"" c
        SET ""TotalDurationMinutes"" = (
            SELECT COALESCE(SUM(s.""TotalDurationMinutes""), 0)
            FROM ""Sections"" s
            WHERE s.""CourseId"" = c.""Id""
        )
    ");
}
```

## Best Practices

### Naming Conventions
```
AddCourseStatus          # Adding new feature
RemoveObsoleteField      # Removing deprecated field
UpdateUserEmailIndex     # Modifying index
FixSectionOrderColumn    # Bug fix
RefactorLessonTypes      # Structural change
```

### Rules
1. Never modify migrations already applied to production
2. Always test migrations on a copy of production data
3. Create Down() method for rollback capability
4. Keep migrations small and focused
5. Use meaningful names describing the change
6. Generate SQL script before production deployment

### Rollback Strategy
```bash
# Rollback to previous migration
dotnet ef database update PreviousMigrationName

# Rollback all (dangerous!)
dotnet ef database update 0
```
