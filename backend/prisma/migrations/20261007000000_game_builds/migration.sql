CREATE TABLE "game_builds" (
  "id" UUID NOT NULL,
  "version" VARCHAR(64) NOT NULL,
  "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT "game_builds_pkey" PRIMARY KEY ("id")
);
