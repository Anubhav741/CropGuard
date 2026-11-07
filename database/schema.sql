-- =============================================================
-- AquaGuardian Database Schema
-- Task: DE Task 1 – Checkpoint 1
-- Author: Yash Kumar
-- Description: Initial PostgreSQL schema for the AquaGuardian
--              groundwater conservation educational game.
-- =============================================================


-- =============================================================
-- TABLE 1: users
-- Stores one row per registered player.
-- 'email' is UNIQUE so no two accounts share the same address.
-- =============================================================
CREATE TABLE users (
    user_id       SERIAL        PRIMARY KEY,                -- Auto-incrementing unique ID for each user
    name          VARCHAR(100)  NOT NULL,                   -- Player's display name (required)
    email         VARCHAR(255)  NOT NULL UNIQUE,            -- Login email, must be unique across all users
    password_hash VARCHAR(255)  NOT NULL,                   -- Hashed password (never store plain text)
    created_at    TIMESTAMP     NOT NULL DEFAULT NOW()      -- Account creation time, defaults to now
);


-- =============================================================
-- TABLE 2: scenarios
-- Stores one row per game scenario (e.g., "Drought Crisis").
-- Scenario data lives here once; many sessions can reference it.
-- =============================================================
CREATE TABLE scenarios (
    scenario_id   SERIAL        PRIMARY KEY,                -- Auto-incrementing unique ID for each scenario
    scenario_name VARCHAR(150)  NOT NULL,                   -- Short name shown in the game menu (required)
    description   TEXT          NOT NULL,                   -- Longer description of the scenario's story/goal
    difficulty    VARCHAR(20)   NOT NULL                    -- Difficulty level for this scenario (required)
        CHECK (difficulty IN ('Easy', 'Medium', 'Hard'))    -- Only these three values are allowed
);


-- =============================================================
-- TABLE 3: game_sessions
-- Stores one row per play-through.
-- Links a user to the scenario they chose and tracks timing.
-- =============================================================
CREATE TABLE game_sessions (
    session_id    SERIAL        PRIMARY KEY,                -- Auto-incrementing unique ID for each session
    user_id       INTEGER       NOT NULL                    -- Which user started this session
        REFERENCES users(user_id),                         -- Must match an existing user
    scenario_id   INTEGER       NOT NULL                    -- Which scenario is being played
        REFERENCES scenarios(scenario_id),                 -- Must match an existing scenario
    started_at    TIMESTAMP     NOT NULL DEFAULT NOW(),     -- When the session began
    ended_at      TIMESTAMP,                               -- When the session ended (NULL if still in progress)
    status        VARCHAR(20)   NOT NULL DEFAULT 'ongoing'  -- Current state of the session
        CHECK (status IN ('ongoing', 'completed', 'abandoned'))
        -- 'ongoing'   – session is in progress
        -- 'completed' – player finished the scenario
        -- 'abandoned' – player quit before finishing
);


-- =============================================================
-- TABLE 4: game_results
-- Stores the outcome of a completed session.
-- 'session_id' is UNIQUE → one session can have at most one result.
-- =============================================================
CREATE TABLE game_results (
    result_id           SERIAL    PRIMARY KEY,              -- Auto-incrementing unique ID for each result
    session_id          INTEGER   NOT NULL UNIQUE           -- Which session this result belongs to
        REFERENCES game_sessions(session_id),              -- Must match an existing session
        -- UNIQUE ensures at most one result per session (one-to-one rule)
    score               INTEGER   NOT NULL                  -- Player's final score (required)
        CHECK (score >= 0),                                -- Score cannot be negative
    groundwater_before  INTEGER   NOT NULL                  -- Groundwater level at the start (percentage)
        CHECK (groundwater_before BETWEEN 0 AND 100),      -- Must be a valid percentage (0–100)
    groundwater_after   INTEGER   NOT NULL                  -- Groundwater level at the end (percentage)
        CHECK (groundwater_after BETWEEN 0 AND 100),       -- Must be a valid percentage (0–100)
    water_saved         INTEGER   NOT NULL                  -- Total units of water saved during the session
        CHECK (water_saved >= 0),                          -- Cannot be negative
    completed_at        TIMESTAMP NOT NULL DEFAULT NOW()    -- When the result was recorded
);
