using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRN232.ExamAccount.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitCentralService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "exam");

            migrationBuilder.CreateTable(
                name: "Students",
                schema: "exam",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    StudentCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FullName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Role = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExamRooms",
                schema: "exam",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    LecturerId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamRooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamRooms_Students_LecturerId",
                        column: x => x.LecturerId,
                        principalSchema: "exam",
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                schema: "exam",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipientUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmissionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExamId = table.Column<Guid>(type: "uuid", nullable: true),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: true),
                    Type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Message = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Students_RecipientUserId",
                        column: x => x.RecipientUserId,
                        principalSchema: "exam",
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Exams",
                schema: "exam",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    MaxScore = table.Column<decimal>(type: "numeric", nullable: false),
                    SolutionPattern = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    RequireAppSettings = table.Column<bool>(type: "boolean", nullable: false),
                    ForbidHardcodedConnectionString = table.Column<bool>(type: "boolean", nullable: false),
                    TimeoutSeconds = table.Column<int>(type: "integer", nullable: false),
                    PlagiarismKeywords = table.Column<string[]>(type: "text[]", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Exams_ExamRooms_RoomId",
                        column: x => x.RoomId,
                        principalSchema: "exam",
                        principalTable: "ExamRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExamSections",
                schema: "exam",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Weight = table.Column<decimal>(type: "numeric", nullable: false),
                    TestFilter = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamSections_Exams_ExamId",
                        column: x => x.ExamId,
                        principalSchema: "exam",
                        principalTable: "Exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Submissions",
                schema: "exam",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspacePath = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TotalScore = table.Column<decimal>(type: "numeric", nullable: true),
                    RawJsonReport = table.Column<string>(type: "text", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    SubmittedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GradedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Submissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Submissions_Exams_ExamId",
                        column: x => x.ExamId,
                        principalSchema: "exam",
                        principalTable: "Exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Submissions_Students_StudentAccountId",
                        column: x => x.StudentAccountId,
                        principalSchema: "exam",
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubmissionSectionResults",
                schema: "exam",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SectionName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Score = table.Column<decimal>(type: "numeric", nullable: false),
                    MaxScore = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Feedback = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubmissionSectionResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubmissionSectionResults_Submissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalSchema: "exam",
                        principalTable: "Submissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExamRooms_LecturerId",
                schema: "exam",
                table: "ExamRooms",
                column: "LecturerId");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_RoomId",
                schema: "exam",
                table: "Exams",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSections_ExamId",
                schema: "exam",
                table: "ExamSections",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientUserId",
                schema: "exam",
                table: "Notifications",
                column: "RecipientUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_ExamId",
                schema: "exam",
                table: "Submissions",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_StudentAccountId",
                schema: "exam",
                table: "Submissions",
                column: "StudentAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionSectionResults_SubmissionId",
                schema: "exam",
                table: "SubmissionSectionResults",
                column: "SubmissionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExamSections",
                schema: "exam");

            migrationBuilder.DropTable(
                name: "Notifications",
                schema: "exam");

            migrationBuilder.DropTable(
                name: "SubmissionSectionResults",
                schema: "exam");

            migrationBuilder.DropTable(
                name: "Submissions",
                schema: "exam");

            migrationBuilder.DropTable(
                name: "Exams",
                schema: "exam");

            migrationBuilder.DropTable(
                name: "ExamRooms",
                schema: "exam");

            migrationBuilder.DropTable(
                name: "Students",
                schema: "exam");
        }
    }
}
