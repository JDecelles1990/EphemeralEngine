using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RunbookForge.Modules.RunbookEngine.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial_Runbooks_Schema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "runbooks");

            migrationBuilder.CreateTable(
                name: "documents",
                schema: "runbooks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Slug = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    RawMarkdown = table.Column<string>(type: "text", nullable: false),
                    AstJson = table.Column<string>(type: "jsonb", nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RiskLevel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Tags = table.Column<string[]>(type: "text[]", nullable: false),
                    CurrentVersionNumber = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_documents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "lint_results",
                schema: "runbooks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    RuleId = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LineNumber = table.Column<int>(type: "integer", nullable: false),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    IsValid = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorCount = table.Column<int>(type: "integer", nullable: false),
                    WarningCount = table.Column<int>(type: "integer", nullable: false),
                    Diagnostics = table.Column<string>(type: "jsonb", nullable: false),
                    EvaluatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lint_results", x => x.Id);
                    table.ForeignKey(
                        name: "FK_lint_results_documents_DocumentId",
                        column: x => x.DocumentId,
                        principalSchema: "runbooks",
                        principalTable: "documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "versions",
                schema: "runbooks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    RawMarkdown = table.Column<string>(type: "text", nullable: false),
                    ParsedAst = table.Column<string>(type: "jsonb", nullable: false),
                    ChangeSummary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    PublishedByUserId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_versions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_versions_documents_DocumentId",
                        column: x => x.DocumentId,
                        principalSchema: "runbooks",
                        principalTable: "documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "workflows",
                schema: "runbooks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    NodesJson = table.Column<string>(type: "jsonb", nullable: false),
                    EdgesJson = table.Column<string>(type: "jsonb", nullable: false),
                    CompiledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workflows_documents_DocumentId",
                        column: x => x.DocumentId,
                        principalSchema: "runbooks",
                        principalTable: "documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "gin_documents_ast_json",
                schema: "runbooks",
                table: "documents",
                column: "AstJson")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "gin_documents_tags",
                schema: "runbooks",
                table: "documents",
                column: "Tags")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "ix_documents_slug",
                schema: "runbooks",
                table: "documents",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_documents_status",
                schema: "runbooks",
                table: "documents",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "ix_lint_results_document_id",
                schema: "runbooks",
                table: "lint_results",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "ix_lint_results_document_severity",
                schema: "runbooks",
                table: "lint_results",
                columns: new[] { "DocumentId", "Severity" });

            migrationBuilder.CreateIndex(
                name: "gin_versions_parsed_ast",
                schema: "runbooks",
                table: "versions",
                column: "ParsedAst")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "ix_versions_document_id",
                schema: "runbooks",
                table: "versions",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "uq_versions_document_version",
                schema: "runbooks",
                table: "versions",
                columns: new[] { "DocumentId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "gin_workflows_edges",
                schema: "runbooks",
                table: "workflows",
                column: "EdgesJson")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "gin_workflows_nodes",
                schema: "runbooks",
                table: "workflows",
                column: "NodesJson")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "IX_workflows_DocumentId",
                schema: "runbooks",
                table: "workflows",
                column: "DocumentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_workflows_document_version",
                schema: "runbooks",
                table: "workflows",
                columns: new[] { "DocumentId", "VersionNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "lint_results",
                schema: "runbooks");

            migrationBuilder.DropTable(
                name: "versions",
                schema: "runbooks");

            migrationBuilder.DropTable(
                name: "workflows",
                schema: "runbooks");

            migrationBuilder.DropTable(
                name: "documents",
                schema: "runbooks");
        }
    }
}
