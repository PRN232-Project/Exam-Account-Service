using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRN232.ExamAccount.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEngineExecutionBridge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApiProjectPath",
                schema: "exam",
                table: "ExamSections",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TestCasesJson",
                schema: "exam",
                table: "ExamSections",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'[]'::jsonb");

            migrationBuilder.CreateTable(
                name: "BatchExecutionTokens",
                schema: "exam",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GradingBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    IssuedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BatchExecutionTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BatchExecutionTokens_GradingBatches_GradingBatchId",
                        column: x => x.GradingBatchId,
                        principalSchema: "exam",
                        principalTable: "GradingBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BatchExecutionTokens_GradingBatchId",
                schema: "exam",
                table: "BatchExecutionTokens",
                column: "GradingBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_BatchExecutionTokens_TokenHash",
                schema: "exam",
                table: "BatchExecutionTokens",
                column: "TokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BatchExecutionTokens",
                schema: "exam");

            migrationBuilder.DropColumn(
                name: "ApiProjectPath",
                schema: "exam",
                table: "ExamSections");

            migrationBuilder.DropColumn(
                name: "TestCasesJson",
                schema: "exam",
                table: "ExamSections");
        }
    }
}
