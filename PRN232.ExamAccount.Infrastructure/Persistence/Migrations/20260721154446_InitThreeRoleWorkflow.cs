using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRN232.ExamAccount.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitThreeRoleWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "exam");

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                schema: "exam",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ResourceType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ResourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    DetailsJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExamPapers",
                schema: "exam",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    RubricVersion = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    MaxScore = table.Column<decimal>(type: "numeric", nullable: false),
                    SolutionPattern = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    RequireAppSettings = table.Column<bool>(type: "boolean", nullable: false),
                    ForbidHardcodedConnectionString = table.Column<bool>(type: "boolean", nullable: false),
                    TimeoutSeconds = table.Column<int>(type: "integer", nullable: false),
                    PlagiarismKeywords = table.Column<string[]>(type: "text[]", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamPapers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExamRooms",
                schema: "exam",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Location = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamRooms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                schema: "exam",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FullName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ClassName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "exam",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    FullName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Role = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExamSections",
                schema: "exam",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamPaperId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Weight = table.Column<decimal>(type: "numeric", nullable: false),
                    TestFilter = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamSections_ExamPapers_ExamPaperId",
                        column: x => x.ExamPaperId,
                        principalSchema: "exam",
                        principalTable: "ExamPapers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamSessions",
                schema: "exam",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamPaperId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduledAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamSessions_ExamPapers_ExamPaperId",
                        column: x => x.ExamPaperId,
                        principalSchema: "exam",
                        principalTable: "ExamPapers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamSessions_ExamRooms_RoomId",
                        column: x => x.RoomId,
                        principalSchema: "exam",
                        principalTable: "ExamRooms",
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
                    GradingBatchId = table.Column<Guid>(type: "uuid", nullable: true),
                    GradingItemId = table.Column<Guid>(type: "uuid", nullable: true),
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
                        name: "FK_Notifications_Users_RecipientUserId",
                        column: x => x.RecipientUserId,
                        principalSchema: "exam",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                schema: "exam",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserAccountId",
                        column: x => x.UserAccountId,
                        principalSchema: "exam",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamCandidates",
                schema: "exam",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaperCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IsAbsent = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamCandidates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamCandidates_ExamSessions_ExamSessionId",
                        column: x => x.ExamSessionId,
                        principalSchema: "exam",
                        principalTable: "ExamSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamCandidates_Students_StudentId",
                        column: x => x.StudentId,
                        principalSchema: "exam",
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GradingBatches",
                schema: "exam",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExamSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    LecturerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AssignedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SubmittedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AcceptedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GradingBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GradingBatches_ExamSessions_ExamSessionId",
                        column: x => x.ExamSessionId,
                        principalSchema: "exam",
                        principalTable: "ExamSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GradingBatches_Users_LecturerId",
                        column: x => x.LecturerId,
                        principalSchema: "exam",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GradingItems",
                schema: "exam",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GradingBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamCandidateId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    LatestScore = table.Column<decimal>(type: "numeric", nullable: true),
                    LatestAttemptNumber = table.Column<int>(type: "integer", nullable: false),
                    LastErrorCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    LastErrorMessage = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GradingItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GradingItems_ExamCandidates_ExamCandidateId",
                        column: x => x.ExamCandidateId,
                        principalSchema: "exam",
                        principalTable: "ExamCandidates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GradingItems_GradingBatches_GradingBatchId",
                        column: x => x.GradingBatchId,
                        principalSchema: "exam",
                        principalTable: "GradingBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GradingAttempts",
                schema: "exam",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GradingItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttemptNumber = table.Column<int>(type: "integer", nullable: false),
                    ClientRequestId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    TotalScore = table.Column<decimal>(type: "numeric", nullable: false),
                    RawJsonReport = table.Column<string>(type: "text", nullable: false),
                    HasTechnicalError = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    RubricVersion = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GradingAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GradingAttempts_GradingItems_GradingItemId",
                        column: x => x.GradingItemId,
                        principalSchema: "exam",
                        principalTable: "GradingItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReviewRequests",
                schema: "exam",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GradingItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    IsResolved = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResolvedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReviewRequests_GradingItems_GradingItemId",
                        column: x => x.GradingItemId,
                        principalSchema: "exam",
                        principalTable: "GradingItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExamCandidates_ExamSessionId_StudentId",
                schema: "exam",
                table: "ExamCandidates",
                columns: new[] { "ExamSessionId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExamCandidates_StudentId",
                schema: "exam",
                table: "ExamCandidates",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamPapers_Code",
                schema: "exam",
                table: "ExamPapers",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExamRooms_Code",
                schema: "exam",
                table: "ExamRooms",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExamSections_ExamPaperId_Name",
                schema: "exam",
                table: "ExamSections",
                columns: new[] { "ExamPaperId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExamSessions_Code",
                schema: "exam",
                table: "ExamSessions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExamSessions_ExamPaperId",
                schema: "exam",
                table: "ExamSessions",
                column: "ExamPaperId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSessions_RoomId",
                schema: "exam",
                table: "ExamSessions",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_GradingAttempts_ClientRequestId",
                schema: "exam",
                table: "GradingAttempts",
                column: "ClientRequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GradingAttempts_GradingItemId_AttemptNumber",
                schema: "exam",
                table: "GradingAttempts",
                columns: new[] { "GradingItemId", "AttemptNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GradingBatches_Code",
                schema: "exam",
                table: "GradingBatches",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GradingBatches_ExamSessionId",
                schema: "exam",
                table: "GradingBatches",
                column: "ExamSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_GradingBatches_LecturerId",
                schema: "exam",
                table: "GradingBatches",
                column: "LecturerId");

            migrationBuilder.CreateIndex(
                name: "IX_GradingItems_ExamCandidateId",
                schema: "exam",
                table: "GradingItems",
                column: "ExamCandidateId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GradingItems_GradingBatchId",
                schema: "exam",
                table: "GradingItems",
                column: "GradingBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientUserId",
                schema: "exam",
                table: "Notifications",
                column: "RecipientUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_TokenHash",
                schema: "exam",
                table: "RefreshTokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserAccountId",
                schema: "exam",
                table: "RefreshTokens",
                column: "UserAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewRequests_GradingItemId",
                schema: "exam",
                table: "ReviewRequests",
                column: "GradingItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_StudentCode",
                schema: "exam",
                table: "Students",
                column: "StudentCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserName",
                schema: "exam",
                table: "Users",
                column: "UserName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs",
                schema: "exam");

            migrationBuilder.DropTable(
                name: "ExamSections",
                schema: "exam");

            migrationBuilder.DropTable(
                name: "GradingAttempts",
                schema: "exam");

            migrationBuilder.DropTable(
                name: "Notifications",
                schema: "exam");

            migrationBuilder.DropTable(
                name: "RefreshTokens",
                schema: "exam");

            migrationBuilder.DropTable(
                name: "ReviewRequests",
                schema: "exam");

            migrationBuilder.DropTable(
                name: "GradingItems",
                schema: "exam");

            migrationBuilder.DropTable(
                name: "ExamCandidates",
                schema: "exam");

            migrationBuilder.DropTable(
                name: "GradingBatches",
                schema: "exam");

            migrationBuilder.DropTable(
                name: "Students",
                schema: "exam");

            migrationBuilder.DropTable(
                name: "ExamSessions",
                schema: "exam");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "exam");

            migrationBuilder.DropTable(
                name: "ExamPapers",
                schema: "exam");

            migrationBuilder.DropTable(
                name: "ExamRooms",
                schema: "exam");
        }
    }
}
