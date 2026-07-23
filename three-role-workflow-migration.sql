DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'exam') THEN
        CREATE SCHEMA exam;
    END IF;
END $EF$;
CREATE TABLE IF NOT EXISTS exam."__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM exam."__EFMigrationsHistory" WHERE "MigrationId" = '20260721154446_InitThreeRoleWorkflow') THEN
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'exam') THEN
            CREATE SCHEMA exam;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM exam."__EFMigrationsHistory" WHERE "MigrationId" = '20260721154446_InitThreeRoleWorkflow') THEN
    CREATE TABLE exam."AuditLogs" (
        "Id" uuid NOT NULL,
        "ActorUserId" uuid NOT NULL,
        "Action" character varying(128) NOT NULL,
        "ResourceType" character varying(128) NOT NULL,
        "ResourceId" uuid,
        "DetailsJson" jsonb NOT NULL,
        "CreatedAtUtc" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_AuditLogs" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM exam."__EFMigrationsHistory" WHERE "MigrationId" = '20260721154446_InitThreeRoleWorkflow') THEN
    CREATE TABLE exam."ExamPapers" (
        "Id" uuid NOT NULL,
        "Code" character varying(64) NOT NULL,
        "Title" character varying(256) NOT NULL,
        "RubricVersion" character varying(64) NOT NULL,
        "MaxScore" numeric NOT NULL,
        "SolutionPattern" character varying(256) NOT NULL,
        "RequireAppSettings" boolean NOT NULL,
        "ForbidHardcodedConnectionString" boolean NOT NULL,
        "TimeoutSeconds" integer NOT NULL,
        "PlagiarismKeywords" text[] NOT NULL,
        "IsActive" boolean NOT NULL,
        CONSTRAINT "PK_ExamPapers" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM exam."__EFMigrationsHistory" WHERE "MigrationId" = '20260721154446_InitThreeRoleWorkflow') THEN
    CREATE TABLE exam."ExamRooms" (
        "Id" uuid NOT NULL,
        "Code" character varying(64) NOT NULL,
        "Name" character varying(128) NOT NULL,
        "Location" character varying(256) NOT NULL,
        "IsActive" boolean NOT NULL,
        CONSTRAINT "PK_ExamRooms" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM exam."__EFMigrationsHistory" WHERE "MigrationId" = '20260721154446_InitThreeRoleWorkflow') THEN
    CREATE TABLE exam."Students" (
        "Id" uuid NOT NULL,
        "StudentCode" character varying(64) NOT NULL,
        "FullName" character varying(256) NOT NULL,
        "Email" character varying(256) NOT NULL,
        "ClassName" character varying(128) NOT NULL,
        "IsActive" boolean NOT NULL,
        CONSTRAINT "PK_Students" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM exam."__EFMigrationsHistory" WHERE "MigrationId" = '20260721154446_InitThreeRoleWorkflow') THEN
    CREATE TABLE exam."Users" (
        "Id" uuid NOT NULL,
        "UserName" character varying(64) NOT NULL,
        "PasswordHash" character varying(512) NOT NULL,
        "FullName" character varying(256) NOT NULL,
        "Email" character varying(256) NOT NULL,
        "Role" character varying(32) NOT NULL,
        "IsActive" boolean NOT NULL,
        "CreatedAtUtc" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM exam."__EFMigrationsHistory" WHERE "MigrationId" = '20260721154446_InitThreeRoleWorkflow') THEN
    CREATE TABLE exam."ExamSections" (
        "Id" uuid NOT NULL,
        "ExamPaperId" uuid NOT NULL,
        "Name" character varying(128) NOT NULL,
        "Weight" numeric NOT NULL,
        "TestFilter" character varying(256) NOT NULL,
        CONSTRAINT "PK_ExamSections" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ExamSections_ExamPapers_ExamPaperId" FOREIGN KEY ("ExamPaperId") REFERENCES exam."ExamPapers" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM exam."__EFMigrationsHistory" WHERE "MigrationId" = '20260721154446_InitThreeRoleWorkflow') THEN
    CREATE TABLE exam."ExamSessions" (
        "Id" uuid NOT NULL,
        "Code" character varying(64) NOT NULL,
        "Title" character varying(256) NOT NULL,
        "RoomId" uuid NOT NULL,
        "ExamPaperId" uuid NOT NULL,
        "ScheduledAtUtc" timestamp with time zone NOT NULL,
        "Status" character varying(32) NOT NULL,
        CONSTRAINT "PK_ExamSessions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ExamSessions_ExamPapers_ExamPaperId" FOREIGN KEY ("ExamPaperId") REFERENCES exam."ExamPapers" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_ExamSessions_ExamRooms_RoomId" FOREIGN KEY ("RoomId") REFERENCES exam."ExamRooms" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM exam."__EFMigrationsHistory" WHERE "MigrationId" = '20260721154446_InitThreeRoleWorkflow') THEN
    CREATE TABLE exam."Notifications" (
        "Id" uuid NOT NULL,
        "RecipientUserId" uuid NOT NULL,
        "GradingBatchId" uuid,
        "GradingItemId" uuid,
        "Type" character varying(64) NOT NULL,
        "Title" character varying(256) NOT NULL,
        "Message" character varying(2048) NOT NULL,
        "IsRead" boolean NOT NULL,
        "CreatedAtUtc" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Notifications" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Notifications_Users_RecipientUserId" FOREIGN KEY ("RecipientUserId") REFERENCES exam."Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM exam."__EFMigrationsHistory" WHERE "MigrationId" = '20260721154446_InitThreeRoleWorkflow') THEN
    CREATE TABLE exam."RefreshTokens" (
        "Id" uuid NOT NULL,
        "UserAccountId" uuid NOT NULL,
        "TokenHash" character varying(128) NOT NULL,
        "ExpiresAtUtc" timestamp with time zone NOT NULL,
        "RevokedAtUtc" timestamp with time zone,
        "CreatedAtUtc" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_RefreshTokens" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_RefreshTokens_Users_UserAccountId" FOREIGN KEY ("UserAccountId") REFERENCES exam."Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM exam."__EFMigrationsHistory" WHERE "MigrationId" = '20260721154446_InitThreeRoleWorkflow') THEN
    CREATE TABLE exam."ExamCandidates" (
        "Id" uuid NOT NULL,
        "ExamSessionId" uuid NOT NULL,
        "StudentId" uuid NOT NULL,
        "PaperCode" character varying(64) NOT NULL,
        "IsAbsent" boolean NOT NULL,
        CONSTRAINT "PK_ExamCandidates" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ExamCandidates_ExamSessions_ExamSessionId" FOREIGN KEY ("ExamSessionId") REFERENCES exam."ExamSessions" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_ExamCandidates_Students_StudentId" FOREIGN KEY ("StudentId") REFERENCES exam."Students" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM exam."__EFMigrationsHistory" WHERE "MigrationId" = '20260721154446_InitThreeRoleWorkflow') THEN
    CREATE TABLE exam."GradingBatches" (
        "Id" uuid NOT NULL,
        "Code" character varying(64) NOT NULL,
        "ExamSessionId" uuid NOT NULL,
        "LecturerId" uuid NOT NULL,
        "Status" character varying(32) NOT NULL,
        "AssignedAtUtc" timestamp with time zone NOT NULL,
        "SubmittedAtUtc" timestamp with time zone,
        "AcceptedAtUtc" timestamp with time zone,
        CONSTRAINT "PK_GradingBatches" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_GradingBatches_ExamSessions_ExamSessionId" FOREIGN KEY ("ExamSessionId") REFERENCES exam."ExamSessions" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_GradingBatches_Users_LecturerId" FOREIGN KEY ("LecturerId") REFERENCES exam."Users" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM exam."__EFMigrationsHistory" WHERE "MigrationId" = '20260721154446_InitThreeRoleWorkflow') THEN
    CREATE TABLE exam."GradingItems" (
        "Id" uuid NOT NULL,
        "GradingBatchId" uuid NOT NULL,
        "ExamCandidateId" uuid NOT NULL,
        "Status" character varying(64) NOT NULL,
        "LatestScore" numeric,
        "LatestAttemptNumber" integer NOT NULL,
        "LastErrorCode" character varying(64) NOT NULL,
        "LastErrorMessage" character varying(2048) NOT NULL,
        CONSTRAINT "PK_GradingItems" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_GradingItems_ExamCandidates_ExamCandidateId" FOREIGN KEY ("ExamCandidateId") REFERENCES exam."ExamCandidates" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_GradingItems_GradingBatches_GradingBatchId" FOREIGN KEY ("GradingBatchId") REFERENCES exam."GradingBatches" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM exam."__EFMigrationsHistory" WHERE "MigrationId" = '20260721154446_InitThreeRoleWorkflow') THEN
    CREATE TABLE exam."GradingAttempts" (
        "Id" uuid NOT NULL,
        "GradingItemId" uuid NOT NULL,
        "AttemptNumber" integer NOT NULL,
        "ClientRequestId" character varying(128) NOT NULL,
        "TotalScore" numeric NOT NULL,
        "RawJsonReport" text NOT NULL,
        "HasTechnicalError" boolean NOT NULL,
        "ErrorCode" character varying(64) NOT NULL,
        "ErrorMessage" character varying(2048) NOT NULL,
        "RubricVersion" character varying(64) NOT NULL,
        "CompletedAtUtc" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_GradingAttempts" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_GradingAttempts_GradingItems_GradingItemId" FOREIGN KEY ("GradingItemId") REFERENCES exam."GradingItems" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM exam."__EFMigrationsHistory" WHERE "MigrationId" = '20260721154446_InitThreeRoleWorkflow') THEN
    CREATE TABLE exam."ReviewRequests" (
        "Id" uuid NOT NULL,
        "GradingItemId" uuid NOT NULL,
        "RequestedByUserId" uuid NOT NULL,
        "Reason" character varying(2048) NOT NULL,
        "IsResolved" boolean NOT NULL,
        "CreatedAtUtc" timestamp with time zone NOT NULL,
        "ResolvedAtUtc" timestamp with time zone,
        CONSTRAINT "PK_ReviewRequests" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ReviewRequests_GradingItems_GradingItemId" FOREIGN KEY ("GradingItemId") REFERENCES exam."GradingItems" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM exam."__EFMigrationsHistory" WHERE "MigrationId" = '20260721154446_InitThreeRoleWorkflow') THEN
    CREATE UNIQUE INDEX "IX_ExamCandidates_ExamSessionId_StudentId" ON exam."ExamCandidates" ("ExamSessionId", "StudentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM exam."__EFMigrationsHistory" WHERE "MigrationId" = '20260721154446_InitThreeRoleWorkflow') THEN
    CREATE INDEX "IX_ExamCandidates_StudentId" ON exam."ExamCandidates" ("StudentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM exam."__EFMigrationsHistory" WHERE "MigrationId" = '20260721154446_InitThreeRoleWorkflow') THEN
    CREATE UNIQUE INDEX "IX_ExamPapers_Code" ON exam."ExamPapers" ("Code");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM exam."__EFMigrationsHistory" WHERE "MigrationId" = '20260721154446_InitThreeRoleWorkflow') THEN
    CREATE UNIQUE INDEX "IX_ExamRooms_Code" ON exam."ExamRooms" ("Code");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM exam."__EFMigrationsHistory" WHERE "MigrationId" = '20260721154446_InitThreeRoleWorkflow') THEN
    CREATE UNIQUE INDEX "IX_ExamSections_ExamPaperId_Name" ON exam."ExamSections" ("ExamPaperId", "Name");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM exam."__EFMigrationsHistory" WHERE "MigrationId" = '20260721154446_InitThreeRoleWorkflow') THEN
    CREATE UNIQUE INDEX "IX_ExamSessions_Code" ON exam."ExamSessions" ("Code");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM exam."__EFMigrationsHistory" WHERE "MigrationId" = '20260721154446_InitThreeRoleWorkflow') THEN
    CREATE INDEX "IX_ExamSessions_ExamPaperId" ON exam."ExamSessions" ("ExamPaperId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM exam."__EFMigrationsHistory" WHERE "MigrationId" = '20260721154446_InitThreeRoleWorkflow') THEN
    CREATE INDEX "IX_ExamSessions_RoomId" ON exam."ExamSessions" ("RoomId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM exam."__EFMigrationsHistory" WHERE "MigrationId" = '20260721154446_InitThreeRoleWorkflow') THEN
    CREATE UNIQUE INDEX "IX_GradingAttempts_ClientRequestId" ON exam."GradingAttempts" ("ClientRequestId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM exam."__EFMigrationsHistory" WHERE "MigrationId" = '20260721154446_InitThreeRoleWorkflow') THEN
    CREATE UNIQUE INDEX "IX_GradingAttempts_GradingItemId_AttemptNumber" ON exam."GradingAttempts" ("GradingItemId", "AttemptNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM exam."__EFMigrationsHistory" WHERE "MigrationId" = '20260721154446_InitThreeRoleWorkflow') THEN
    CREATE UNIQUE INDEX "IX_GradingBatches_Code" ON exam."GradingBatches" ("Code");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM exam."__EFMigrationsHistory" WHERE "MigrationId" = '20260721154446_InitThreeRoleWorkflow') THEN
    CREATE INDEX "IX_GradingBatches_ExamSessionId" ON exam."GradingBatches" ("ExamSessionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM exam."__EFMigrationsHistory" WHERE "MigrationId" = '20260721154446_InitThreeRoleWorkflow') THEN
    CREATE INDEX "IX_GradingBatches_LecturerId" ON exam."GradingBatches" ("LecturerId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM exam."__EFMigrationsHistory" WHERE "MigrationId" = '20260721154446_InitThreeRoleWorkflow') THEN
    CREATE UNIQUE INDEX "IX_GradingItems_ExamCandidateId" ON exam."GradingItems" ("ExamCandidateId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM exam."__EFMigrationsHistory" WHERE "MigrationId" = '20260721154446_InitThreeRoleWorkflow') THEN
    CREATE INDEX "IX_GradingItems_GradingBatchId" ON exam."GradingItems" ("GradingBatchId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM exam."__EFMigrationsHistory" WHERE "MigrationId" = '20260721154446_InitThreeRoleWorkflow') THEN
    CREATE INDEX "IX_Notifications_RecipientUserId" ON exam."Notifications" ("RecipientUserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM exam."__EFMigrationsHistory" WHERE "MigrationId" = '20260721154446_InitThreeRoleWorkflow') THEN
    CREATE UNIQUE INDEX "IX_RefreshTokens_TokenHash" ON exam."RefreshTokens" ("TokenHash");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM exam."__EFMigrationsHistory" WHERE "MigrationId" = '20260721154446_InitThreeRoleWorkflow') THEN
    CREATE INDEX "IX_RefreshTokens_UserAccountId" ON exam."RefreshTokens" ("UserAccountId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM exam."__EFMigrationsHistory" WHERE "MigrationId" = '20260721154446_InitThreeRoleWorkflow') THEN
    CREATE INDEX "IX_ReviewRequests_GradingItemId" ON exam."ReviewRequests" ("GradingItemId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM exam."__EFMigrationsHistory" WHERE "MigrationId" = '20260721154446_InitThreeRoleWorkflow') THEN
    CREATE UNIQUE INDEX "IX_Students_StudentCode" ON exam."Students" ("StudentCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM exam."__EFMigrationsHistory" WHERE "MigrationId" = '20260721154446_InitThreeRoleWorkflow') THEN
    CREATE UNIQUE INDEX "IX_Users_UserName" ON exam."Users" ("UserName");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM exam."__EFMigrationsHistory" WHERE "MigrationId" = '20260721154446_InitThreeRoleWorkflow') THEN
    INSERT INTO exam."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260721154446_InitThreeRoleWorkflow', '9.0.2');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM exam."__EFMigrationsHistory" WHERE "MigrationId" = '20260722235258_AddEngineExecutionBridge') THEN
    ALTER TABLE exam."ExamSections" ADD "ApiProjectPath" character varying(512) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM exam."__EFMigrationsHistory" WHERE "MigrationId" = '20260722235258_AddEngineExecutionBridge') THEN
    ALTER TABLE exam."ExamSections" ADD "TestCasesJson" jsonb NOT NULL DEFAULT ('[]'::jsonb);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM exam."__EFMigrationsHistory" WHERE "MigrationId" = '20260722235258_AddEngineExecutionBridge') THEN
    CREATE TABLE exam."BatchExecutionTokens" (
        "Id" uuid NOT NULL,
        "GradingBatchId" uuid NOT NULL,
        "IssuedByUserId" uuid NOT NULL,
        "TokenHash" character varying(128) NOT NULL,
        "ExpiresAtUtc" timestamp with time zone NOT NULL,
        "RevokedAtUtc" timestamp with time zone,
        "CreatedAtUtc" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_BatchExecutionTokens" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_BatchExecutionTokens_GradingBatches_GradingBatchId" FOREIGN KEY ("GradingBatchId") REFERENCES exam."GradingBatches" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM exam."__EFMigrationsHistory" WHERE "MigrationId" = '20260722235258_AddEngineExecutionBridge') THEN
    CREATE INDEX "IX_BatchExecutionTokens_GradingBatchId" ON exam."BatchExecutionTokens" ("GradingBatchId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM exam."__EFMigrationsHistory" WHERE "MigrationId" = '20260722235258_AddEngineExecutionBridge') THEN
    CREATE UNIQUE INDEX "IX_BatchExecutionTokens_TokenHash" ON exam."BatchExecutionTokens" ("TokenHash");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM exam."__EFMigrationsHistory" WHERE "MigrationId" = '20260722235258_AddEngineExecutionBridge') THEN
    INSERT INTO exam."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260722235258_AddEngineExecutionBridge', '9.0.2');
    END IF;
END $EF$;
COMMIT;

