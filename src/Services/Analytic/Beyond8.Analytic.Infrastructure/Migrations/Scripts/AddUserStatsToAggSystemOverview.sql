-- Add user stats columns to AggSystemOverviews (matches migration AddUserStatsToAggSystemOverview)
-- Run this if migration was not applied (e.g. column "NewUsersToday" does not exist)

ALTER TABLE "AggSystemOverviews"
ADD COLUMN IF NOT EXISTS "TotalUsers" integer NOT NULL DEFAULT 0,
ADD COLUMN IF NOT EXISTS "TotalActiveUsers" integer NOT NULL DEFAULT 0,
ADD COLUMN IF NOT EXISTS "NewUsersToday" integer NOT NULL DEFAULT 0,
ADD COLUMN IF NOT EXISTS "TotalInstructors" integer NOT NULL DEFAULT 0,
ADD COLUMN IF NOT EXISTS "TotalStudents" integer NOT NULL DEFAULT 0;

-- Optional: record that this migration was applied (EF Core tracks in __EFMigrationsHistory)
-- INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion") VALUES ('20260207120000_AddUserStatsToAggSystemOverview', '10.0.0');
