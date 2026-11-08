-- CreateSchema
CREATE SCHEMA IF NOT EXISTS "public";

-- CreateTable
CREATE TABLE "users" (
    "user_id" SERIAL NOT NULL,
    "name" VARCHAR(100) NOT NULL,
    "email" VARCHAR(255) NOT NULL,
    "password_hash" VARCHAR(255) NOT NULL,
    "created_at" TIMESTAMP(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT "users_pkey" PRIMARY KEY ("user_id")
);

-- CreateTable
CREATE TABLE "scenarios" (
    "scenario_id" SERIAL NOT NULL,
    "scenario_name" VARCHAR(150) NOT NULL,
    "description" TEXT NOT NULL,
    "difficulty" VARCHAR(20) NOT NULL,

    CONSTRAINT "scenarios_pkey" PRIMARY KEY ("scenario_id")
);

-- CreateTable
CREATE TABLE "game_sessions" (
    "session_id" SERIAL NOT NULL,
    "user_id" INTEGER NOT NULL,
    "scenario_id" INTEGER NOT NULL,
    "started_at" TIMESTAMP(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "ended_at" TIMESTAMP(6),
    "status" VARCHAR(20) NOT NULL DEFAULT 'ongoing',

    CONSTRAINT "game_sessions_pkey" PRIMARY KEY ("session_id")
);

-- CreateTable
CREATE TABLE "game_results" (
    "result_id" SERIAL NOT NULL,
    "session_id" INTEGER NOT NULL,
    "score" INTEGER NOT NULL,
    "groundwater_before" INTEGER NOT NULL,
    "groundwater_after" INTEGER NOT NULL,
    "water_saved" INTEGER NOT NULL,
    "completed_at" TIMESTAMP(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT "game_results_pkey" PRIMARY KEY ("result_id")
);

-- CreateIndex
CREATE UNIQUE INDEX "users_email_key" ON "users"("email");

-- CreateIndex
CREATE UNIQUE INDEX "game_results_session_id_key" ON "game_results"("session_id");

-- AddForeignKey
ALTER TABLE "game_sessions" ADD CONSTRAINT "game_sessions_scenario_id_fkey" FOREIGN KEY ("scenario_id") REFERENCES "scenarios"("scenario_id") ON DELETE NO ACTION ON UPDATE NO ACTION;

-- AddForeignKey
ALTER TABLE "game_sessions" ADD CONSTRAINT "game_sessions_user_id_fkey" FOREIGN KEY ("user_id") REFERENCES "users"("user_id") ON DELETE NO ACTION ON UPDATE NO ACTION;

-- AddForeignKey
ALTER TABLE "game_results" ADD CONSTRAINT "game_results_session_id_fkey" FOREIGN KEY ("session_id") REFERENCES "game_sessions"("session_id") ON DELETE NO ACTION ON UPDATE NO ACTION;

