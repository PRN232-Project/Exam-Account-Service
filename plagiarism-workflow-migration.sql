START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM exam."__EFMigrationsHistory" WHERE "MigrationId" = '20260723004343_AddPlagiarismWorkflow') THEN
    ALTER TABLE exam."GradingItems" ADD "PlagiarismCheckedAtUtc" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM exam."__EFMigrationsHistory" WHERE "MigrationId" = '20260723004343_AddPlagiarismWorkflow') THEN
    ALTER TABLE exam."GradingItems" ADD "PlagiarismErrorMessage" character varying(2048) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM exam."__EFMigrationsHistory" WHERE "MigrationId" = '20260723004343_AddPlagiarismWorkflow') THEN
    ALTER TABLE exam."GradingItems" ADD "PlagiarismMaxSimilarity" numeric;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM exam."__EFMigrationsHistory" WHERE "MigrationId" = '20260723004343_AddPlagiarismWorkflow') THEN
    ALTER TABLE exam."GradingItems" ADD "PlagiarismReportJson" jsonb NOT NULL DEFAULT ('{}'::jsonb);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM exam."__EFMigrationsHistory" WHERE "MigrationId" = '20260723004343_AddPlagiarismWorkflow') THEN
    ALTER TABLE exam."GradingItems" ADD "PlagiarismStatus" character varying(32) NOT NULL DEFAULT 'Pending';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM exam."__EFMigrationsHistory" WHERE "MigrationId" = '20260723004343_AddPlagiarismWorkflow') THEN
    ALTER TABLE exam."GradingItems" ADD "PlagiarismViolationCount" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM exam."__EFMigrationsHistory" WHERE "MigrationId" = '20260723004343_AddPlagiarismWorkflow') THEN
    INSERT INTO exam."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260723004343_AddPlagiarismWorkflow', '9.0.2');
    END IF;
END $EF$;
COMMIT;

