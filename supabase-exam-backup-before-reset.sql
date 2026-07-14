--
-- PostgreSQL database dump
--

\restrict Z6krxLxaBJ3Vohlt4JTc6JEKjRoUEuRPa40WLk3d36h7whEcxrX0I0zEb683xXf

-- Dumped from database version 17.6
-- Dumped by pg_dump version 18.4

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- Name: exam; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA exam;


ALTER SCHEMA exam OWNER TO postgres;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: ExamSections; Type: TABLE; Schema: exam; Owner: postgres
--

CREATE TABLE exam."ExamSections" (
    "Id" uuid NOT NULL,
    "ExamId" uuid NOT NULL,
    "Name" character varying(128) NOT NULL,
    "Weight" numeric NOT NULL,
    "TestFilter" character varying(256) NOT NULL
);


ALTER TABLE exam."ExamSections" OWNER TO postgres;

--
-- Name: Exams; Type: TABLE; Schema: exam; Owner: postgres
--

CREATE TABLE exam."Exams" (
    "Id" uuid NOT NULL,
    "Code" character varying(64) NOT NULL,
    "Title" character varying(256) NOT NULL,
    "MaxScore" numeric NOT NULL,
    "SolutionPattern" character varying(256) NOT NULL,
    "RequireAppSettings" boolean NOT NULL,
    "ForbidHardcodedConnectionString" boolean NOT NULL,
    "TimeoutSeconds" integer NOT NULL,
    "PlagiarismKeywords" text[] NOT NULL,
    "CreatedAtUtc" timestamp with time zone NOT NULL,
    "Description" character varying(2048) DEFAULT ''::character varying NOT NULL,
    "RoomCode" character varying(64) DEFAULT ''::character varying NOT NULL,
    "StartsAtUtc" timestamp with time zone DEFAULT now() NOT NULL,
    "EndsAtUtc" timestamp with time zone DEFAULT now() NOT NULL,
    "CreatedByUserId" uuid,
    "LecturerId" uuid
);


ALTER TABLE exam."Exams" OWNER TO postgres;

--
-- Name: Students; Type: TABLE; Schema: exam; Owner: postgres
--

CREATE TABLE exam."Students" (
    "Id" uuid NOT NULL,
    "StudentCode" character varying(64) NOT NULL,
    "FullName" character varying(256) NOT NULL,
    "Email" character varying(256) NOT NULL
);


ALTER TABLE exam."Students" OWNER TO postgres;

--
-- Name: SubmissionSectionResults; Type: TABLE; Schema: exam; Owner: postgres
--

CREATE TABLE exam."SubmissionSectionResults" (
    "Id" uuid NOT NULL,
    "SubmissionId" uuid NOT NULL,
    "SectionName" character varying(128) NOT NULL,
    "Score" numeric NOT NULL,
    "MaxScore" numeric NOT NULL,
    "Status" character varying(64) NOT NULL,
    "Feedback" text NOT NULL
);


ALTER TABLE exam."SubmissionSectionResults" OWNER TO postgres;

--
-- Name: Submissions; Type: TABLE; Schema: exam; Owner: postgres
--

CREATE TABLE exam."Submissions" (
    "Id" uuid NOT NULL,
    "ExamId" uuid NOT NULL,
    "StudentId" uuid NOT NULL,
    "WorkspacePath" character varying(1024) NOT NULL,
    "Status" character varying(64) NOT NULL,
    "TotalScore" numeric,
    "RawJsonReport" text NOT NULL,
    "SubmittedAtUtc" timestamp with time zone NOT NULL,
    "GradedAtUtc" timestamp with time zone,
    "FailureReason" character varying(2048) DEFAULT ''::character varying NOT NULL
);


ALTER TABLE exam."Submissions" OWNER TO postgres;

--
-- Name: Users; Type: TABLE; Schema: exam; Owner: postgres
--

CREATE TABLE exam."Users" (
    "Id" uuid NOT NULL,
    "Email" character varying(256) NOT NULL,
    "FullName" character varying(256) NOT NULL,
    "PasswordHash" character varying(1024) NOT NULL,
    "Role" character varying(32) NOT NULL,
    "StudentCode" character varying(64),
    "IsActive" boolean DEFAULT true NOT NULL,
    "CreatedAtUtc" timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE exam."Users" OWNER TO postgres;

--
-- Data for Name: ExamSections; Type: TABLE DATA; Schema: exam; Owner: postgres
--

COPY exam."ExamSections" ("Id", "ExamId", "Name", "Weight", "TestFilter") FROM stdin;
880e8400-e29b-41d4-a716-446655440000	550e8400-e29b-41d4-a716-446655440000	CRUD_Test	50.0	Category=CRUD
\.


--
-- Data for Name: Exams; Type: TABLE DATA; Schema: exam; Owner: postgres
--

COPY exam."Exams" ("Id", "Code", "Title", "MaxScore", "SolutionPattern", "RequireAppSettings", "ForbidHardcodedConnectionString", "TimeoutSeconds", "PlagiarismKeywords", "CreatedAtUtc", "Description", "RoomCode", "StartsAtUtc", "EndsAtUtc", "CreatedByUserId", "LecturerId") FROM stdin;
550e8400-e29b-41d4-a716-446655440000	PRN231_PE_SU25	Practical Exam Summer 2025	10.0	PRN231_SU25_*.sln	t	t	30	{Process.Start,Registry}	2026-07-08 06:35:25.697058+00			2026-07-14 09:04:58.760188+00	2026-07-14 09:04:58.797834+00	\N	\N
\.


--
-- Data for Name: Students; Type: TABLE DATA; Schema: exam; Owner: postgres
--

COPY exam."Students" ("Id", "StudentCode", "FullName", "Email") FROM stdin;
770e8400-e29b-41d4-a716-446655440000	SE182004	Nguyen Van A	se182004@fpt.edu.vn
\.


--
-- Data for Name: SubmissionSectionResults; Type: TABLE DATA; Schema: exam; Owner: postgres
--

COPY exam."SubmissionSectionResults" ("Id", "SubmissionId", "SectionName", "Score", "MaxScore", "Status", "Feedback") FROM stdin;
3a4124a8-4e84-4707-b7c2-b4fcb1de136c	6a9dd983-da9b-444c-979e-eb22e66c54f4	Auth	2.25	3.0	Passed	Auth ok
95ada6da-5c47-454a-a60a-7b490a4b1e56	6a9dd983-da9b-444c-979e-eb22e66c54f4	Validation	2.5	3.0	Passed	Validation ok
dd712841-31cb-4207-9723-017a15541334	6a9dd983-da9b-444c-979e-eb22e66c54f4	CRUD	3.0	4.0	Passed	CRUD ok
\.


--
-- Data for Name: Submissions; Type: TABLE DATA; Schema: exam; Owner: postgres
--

COPY exam."Submissions" ("Id", "ExamId", "StudentId", "WorkspacePath", "Status", "TotalScore", "RawJsonReport", "SubmittedAtUtc", "GradedAtUtc", "FailureReason") FROM stdin;
6a9dd983-da9b-444c-979e-eb22e66c54f4	550e8400-e29b-41d4-a716-446655440000	770e8400-e29b-41d4-a716-446655440000	D:\\Project_PRN\\System_Repo\\All Engine\\Engine_Service\\sample-student-submission\\d48590cb-2292-4d7a-8f1d-8cb5d5a712e3	GradedPendingPublication	7.75	{"sectionResults":[{"name":"CRUD","score":3.0,"maxScore":4.0,"status":"Passed","feedback":"CRUD ok"},{"name":"Auth","score":2.25,"maxScore":3.0,"status":"Passed","feedback":"Auth ok"},{"name":"Validation","score":2.5,"maxScore":3.0,"status":"Passed","feedback":"Validation ok"}]}	2026-07-08 16:37:43.485084+00	2026-07-08 16:45:00+00	
\.


--
-- Data for Name: Users; Type: TABLE DATA; Schema: exam; Owner: postgres
--

COPY exam."Users" ("Id", "Email", "FullName", "PasswordHash", "Role", "StudentCode", "IsActive", "CreatedAtUtc") FROM stdin;
0ca3e212-81c2-49a9-a34d-d1aed54d2b5d	admin@prn232.local	System Admin	100000.7UuqiAlUJa5/do8koeFufg==.F46XnR5ZTkmR+aXVaSKgYOPgSbnMK7jXnL8CCkFvBO0=	Admin		t	2026-07-14 08:58:49.43177+00
\.


--
-- Name: ExamSections PK_ExamSections; Type: CONSTRAINT; Schema: exam; Owner: postgres
--

ALTER TABLE ONLY exam."ExamSections"
    ADD CONSTRAINT "PK_ExamSections" PRIMARY KEY ("Id");


--
-- Name: Exams PK_Exams; Type: CONSTRAINT; Schema: exam; Owner: postgres
--

ALTER TABLE ONLY exam."Exams"
    ADD CONSTRAINT "PK_Exams" PRIMARY KEY ("Id");


--
-- Name: Students PK_Students; Type: CONSTRAINT; Schema: exam; Owner: postgres
--

ALTER TABLE ONLY exam."Students"
    ADD CONSTRAINT "PK_Students" PRIMARY KEY ("Id");


--
-- Name: SubmissionSectionResults PK_SubmissionSectionResults; Type: CONSTRAINT; Schema: exam; Owner: postgres
--

ALTER TABLE ONLY exam."SubmissionSectionResults"
    ADD CONSTRAINT "PK_SubmissionSectionResults" PRIMARY KEY ("Id");


--
-- Name: Submissions PK_Submissions; Type: CONSTRAINT; Schema: exam; Owner: postgres
--

ALTER TABLE ONLY exam."Submissions"
    ADD CONSTRAINT "PK_Submissions" PRIMARY KEY ("Id");


--
-- Name: Users Users_pkey; Type: CONSTRAINT; Schema: exam; Owner: postgres
--

ALTER TABLE ONLY exam."Users"
    ADD CONSTRAINT "Users_pkey" PRIMARY KEY ("Id");


--
-- Name: IX_ExamSections_ExamId; Type: INDEX; Schema: exam; Owner: postgres
--

CREATE INDEX "IX_ExamSections_ExamId" ON exam."ExamSections" USING btree ("ExamId");


--
-- Name: IX_Exams_Code; Type: INDEX; Schema: exam; Owner: postgres
--

CREATE UNIQUE INDEX "IX_Exams_Code" ON exam."Exams" USING btree ("Code");


--
-- Name: IX_Exams_RoomCode; Type: INDEX; Schema: exam; Owner: postgres
--

CREATE INDEX "IX_Exams_RoomCode" ON exam."Exams" USING btree ("RoomCode");


--
-- Name: IX_SubmissionSectionResults_SubmissionId; Type: INDEX; Schema: exam; Owner: postgres
--

CREATE INDEX "IX_SubmissionSectionResults_SubmissionId" ON exam."SubmissionSectionResults" USING btree ("SubmissionId");


--
-- Name: IX_Submissions_ExamId; Type: INDEX; Schema: exam; Owner: postgres
--

CREATE INDEX "IX_Submissions_ExamId" ON exam."Submissions" USING btree ("ExamId");


--
-- Name: IX_Submissions_ExamId_StudentId; Type: INDEX; Schema: exam; Owner: postgres
--

CREATE INDEX "IX_Submissions_ExamId_StudentId" ON exam."Submissions" USING btree ("ExamId", "StudentId");


--
-- Name: IX_Submissions_StudentAccountId; Type: INDEX; Schema: exam; Owner: postgres
--

CREATE INDEX "IX_Submissions_StudentAccountId" ON exam."Submissions" USING btree ("StudentId");


--
-- Name: IX_Users_Email; Type: INDEX; Schema: exam; Owner: postgres
--

CREATE UNIQUE INDEX "IX_Users_Email" ON exam."Users" USING btree ("Email");


--
-- Name: ExamSections FK_ExamSections_Exams_ExamId; Type: FK CONSTRAINT; Schema: exam; Owner: postgres
--

ALTER TABLE ONLY exam."ExamSections"
    ADD CONSTRAINT "FK_ExamSections_Exams_ExamId" FOREIGN KEY ("ExamId") REFERENCES exam."Exams"("Id") ON DELETE CASCADE;


--
-- Name: SubmissionSectionResults FK_SubmissionSectionResults_Submissions_SubmissionId; Type: FK CONSTRAINT; Schema: exam; Owner: postgres
--

ALTER TABLE ONLY exam."SubmissionSectionResults"
    ADD CONSTRAINT "FK_SubmissionSectionResults_Submissions_SubmissionId" FOREIGN KEY ("SubmissionId") REFERENCES exam."Submissions"("Id") ON DELETE CASCADE;


--
-- Name: Submissions FK_Submissions_Exams_ExamId; Type: FK CONSTRAINT; Schema: exam; Owner: postgres
--

ALTER TABLE ONLY exam."Submissions"
    ADD CONSTRAINT "FK_Submissions_Exams_ExamId" FOREIGN KEY ("ExamId") REFERENCES exam."Exams"("Id") ON DELETE CASCADE;


--
-- Name: Submissions FK_Submissions_Students_StudentAccountId; Type: FK CONSTRAINT; Schema: exam; Owner: postgres
--

ALTER TABLE ONLY exam."Submissions"
    ADD CONSTRAINT "FK_Submissions_Students_StudentAccountId" FOREIGN KEY ("StudentId") REFERENCES exam."Students"("Id") ON DELETE CASCADE;


--
-- PostgreSQL database dump complete
--

\unrestrict Z6krxLxaBJ3Vohlt4JTc6JEKjRoUEuRPa40WLk3d36h7whEcxrX0I0zEb683xXf

