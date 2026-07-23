START TRANSACTION;
ALTER TABLE exam."ExamSections" ADD "ApiProjectPath" character varying(512) NOT NULL DEFAULT '';

ALTER TABLE exam."ExamSections" ADD "TestCasesJson" jsonb NOT NULL DEFAULT ('[]'::jsonb);

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

CREATE INDEX "IX_BatchExecutionTokens_GradingBatchId" ON exam."BatchExecutionTokens" ("GradingBatchId");

CREATE UNIQUE INDEX "IX_BatchExecutionTokens_TokenHash" ON exam."BatchExecutionTokens" ("TokenHash");

INSERT INTO exam."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260722235258_AddEngineExecutionBridge', '9.0.2');

COMMIT;

