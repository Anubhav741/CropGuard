-- Prisma's String and Int fields do not encode PostgreSQL CHECK constraints.
-- Add the rules missing from 0_init; skip names already present on SQL-created databases.
DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conrelid = 'scenarios'::regclass AND conname = 'scenarios_difficulty_check') THEN
    ALTER TABLE scenarios ADD CONSTRAINT scenarios_difficulty_check CHECK (difficulty IN ('Easy', 'Medium', 'Hard'));
  END IF;
  IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conrelid = 'game_sessions'::regclass AND conname = 'game_sessions_status_check') THEN
    ALTER TABLE game_sessions ADD CONSTRAINT game_sessions_status_check CHECK (status IN ('ongoing', 'completed', 'abandoned'));
  END IF;
  IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conrelid = 'game_results'::regclass AND conname = 'game_results_score_check') THEN
    ALTER TABLE game_results ADD CONSTRAINT game_results_score_check CHECK (score >= 0);
  END IF;
  IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conrelid = 'game_results'::regclass AND conname = 'game_results_groundwater_before_check') THEN
    ALTER TABLE game_results ADD CONSTRAINT game_results_groundwater_before_check CHECK (groundwater_before BETWEEN 0 AND 100);
  END IF;
  IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conrelid = 'game_results'::regclass AND conname = 'game_results_groundwater_after_check') THEN
    ALTER TABLE game_results ADD CONSTRAINT game_results_groundwater_after_check CHECK (groundwater_after BETWEEN 0 AND 100);
  END IF;
  IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conrelid = 'game_results'::regclass AND conname = 'game_results_water_saved_check') THEN
    ALTER TABLE game_results ADD CONSTRAINT game_results_water_saved_check CHECK (water_saved >= 0);
  END IF;
END $$;
