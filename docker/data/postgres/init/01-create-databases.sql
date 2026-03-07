-- Beyond8: Create all application and Hangfire databases on first Postgres startup.
-- Only runs when the data volume is empty (first container start).

-- Service databases
CREATE DATABASE "Identities";
CREATE DATABASE "Integrations";
CREATE DATABASE "Assessments";
CREATE DATABASE "Catalogs";
CREATE DATABASE "Learnings";
CREATE DATABASE "Analytics";
CREATE DATABASE "Sales";

-- Hangfire databases (Identity, Integration, Sale)
CREATE DATABASE "HangfiresIdentity";
CREATE DATABASE "HangfiresIntegration";
CREATE DATABASE "HangfiresSale";
