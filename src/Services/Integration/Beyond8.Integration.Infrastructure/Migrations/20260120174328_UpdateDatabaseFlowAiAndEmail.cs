using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beyond8.Integration.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDatabaseFlowAiAndEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AiPrompts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    Template = table.Column<string>(type: "text", nullable: false),
                    Version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Variables = table.Column<string>(type: "jsonb", nullable: true),
                    DefaultParameters = table.Column<string>(type: "jsonb", nullable: true),
                    SystemPrompt = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    MaxTokens = table.Column<int>(type: "integer", nullable: false),
                    Temperature = table.Column<decimal>(type: "numeric(3,2)", nullable: false),
                    TopP = table.Column<decimal>(type: "numeric(3,2)", nullable: false),
                    Tags = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiPrompts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AiUsages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<int>(type: "integer", nullable: false),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Operation = table.Column<int>(type: "integer", nullable: false),
                    InputTokens = table.Column<int>(type: "integer", nullable: false),
                    OutputTokens = table.Column<int>(type: "integer", nullable: false),
                    TotalTokens = table.Column<int>(type: "integer", nullable: false),
                    InputCost = table.Column<decimal>(type: "numeric(10,4)", nullable: false),
                    OutputCost = table.Column<decimal>(type: "numeric(10,4)", nullable: false),
                    TotalCost = table.Column<decimal>(type: "numeric(10,4)", nullable: false),
                    PromptId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestSummary = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ResponseTimeMs = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Metadata = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiUsages_AiPrompts_PromptId",
                        column: x => x.PromptId,
                        principalTable: "AiPrompts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiPrompts_Category_IsActive",
                table: "AiPrompts",
                columns: new[] { "Category", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_AiPrompts_Name_Version",
                table: "AiPrompts",
                columns: new[] { "Name", "Version" });

            migrationBuilder.CreateIndex(
                name: "IX_AiUsages_PromptId",
                table: "AiUsages",
                column: "PromptId");

            migrationBuilder.CreateIndex(
                name: "IX_AiUsages_Provider_CreatedAt",
                table: "AiUsages",
                columns: new[] { "Provider", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AiUsages_UserId_CreatedAt",
                table: "AiUsages",
                columns: new[] { "UserId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiUsages");

            migrationBuilder.DropTable(
                name: "AiPrompts");
        }
    }
}
