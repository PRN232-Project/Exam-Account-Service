using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRN232.ExamAccount.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPlagiarismWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PlagiarismCheckedAtUtc",
                schema: "exam",
                table: "GradingItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlagiarismErrorMessage",
                schema: "exam",
                table: "GradingItems",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "PlagiarismMaxSimilarity",
                schema: "exam",
                table: "GradingItems",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlagiarismReportJson",
                schema: "exam",
                table: "GradingItems",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'{}'::jsonb");

            migrationBuilder.AddColumn<string>(
                name: "PlagiarismStatus",
                schema: "exam",
                table: "GradingItems",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.AddColumn<int>(
                name: "PlagiarismViolationCount",
                schema: "exam",
                table: "GradingItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlagiarismCheckedAtUtc",
                schema: "exam",
                table: "GradingItems");

            migrationBuilder.DropColumn(
                name: "PlagiarismErrorMessage",
                schema: "exam",
                table: "GradingItems");

            migrationBuilder.DropColumn(
                name: "PlagiarismMaxSimilarity",
                schema: "exam",
                table: "GradingItems");

            migrationBuilder.DropColumn(
                name: "PlagiarismReportJson",
                schema: "exam",
                table: "GradingItems");

            migrationBuilder.DropColumn(
                name: "PlagiarismStatus",
                schema: "exam",
                table: "GradingItems");

            migrationBuilder.DropColumn(
                name: "PlagiarismViolationCount",
                schema: "exam",
                table: "GradingItems");
        }
    }
}
