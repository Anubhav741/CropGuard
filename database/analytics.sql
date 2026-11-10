-- =============================================================
-- AquaGuardian – Analytics SQL (Final, Validated)
-- Task:    DE Task 3 – Complete Syllabus Coverage
-- Author:  Yash Kumar
-- Purpose: Analytical queries demonstrating every required SQL
--          concept against the AquaGuardian game database.
-- Pre-req: Run schema.sql then seed.sql before this file.
-- Tested:  PostgreSQL 18 on database "aquaguardian"
-- =============================================================
-- SCHEMA REFERENCE (confirmed from live database):
--   users        : user_id PK, name, email UK, password_hash, created_at
--   scenarios    : scenario_id PK, scenario_name, description, difficulty
--   game_sessions: session_id PK, user_id FK, scenario_id FK,
--                  started_at, ended_at, status
--   game_results : result_id PK, session_id FK UK, score,
--                  groundwater_before, groundwater_after,
--                  water_saved, completed_at
-- =============================================================


-- =============================================================
-- SECTION A: USER ANALYTICS
-- =============================================================

-- A.1  Total sessions per user (LEFT JOIN, GROUP BY, COUNT)
-- Which users are the most active overall?
SELECT
    u.user_id,
    u.name         AS player_name,
    COUNT(gs.session_id) AS total_sessions
FROM users u
LEFT JOIN game_sessions gs ON u.user_id = gs.user_id
GROUP BY u.user_id, u.name
ORDER BY total_sessions DESC, u.user_id;


-- A.2  Completed sessions per user (Conditional COUNT with CASE)
-- How many missions did each player successfully finish?
SELECT
    u.user_id,
    u.name         AS player_name,
    COUNT(CASE WHEN gs.status = 'completed' THEN 1 END) AS completed_sessions
FROM users u
LEFT JOIN game_sessions gs ON u.user_id = gs.user_id
GROUP BY u.user_id, u.name
ORDER BY completed_sessions DESC, u.user_id;


-- A.3  Average score per user (AVG, COALESCE, ROUND)
-- Who is the best performing player on average?
SELECT
    u.user_id,
    u.name         AS player_name,
    COALESCE(ROUND(AVG(gr.score), 2), 0.00) AS avg_score
FROM users u
LEFT JOIN game_sessions gs ON u.user_id = gs.user_id
LEFT JOIN game_results  gr ON gs.session_id = gr.session_id
GROUP BY u.user_id, u.name
ORDER BY avg_score DESC;


-- A.4  Total water saved per user (SUM, COALESCE)
-- Which player has contributed the most to groundwater conservation?
SELECT
    u.user_id,
    u.name         AS player_name,
    COALESCE(SUM(gr.water_saved), 0) AS total_water_saved
FROM users u
LEFT JOIN game_sessions gs ON u.user_id = gs.user_id
LEFT JOIN game_results  gr ON gs.session_id = gr.session_id
GROUP BY u.user_id, u.name
ORDER BY total_water_saved DESC;


-- A.5  Comprehensive user performance report (DENSE_RANK window function)
-- Rank all players by total cumulative score with detailed stats.
SELECT
    u.user_id,
    u.name         AS player_name,
    COUNT(gr.result_id) AS games_completed,
    COALESCE(SUM(gr.score),      0)    AS total_score,
    COALESCE(ROUND(AVG(gr.score), 2), 0.00) AS avg_score,
    COALESCE(SUM(gr.water_saved), 0)   AS total_water_saved,
    DENSE_RANK() OVER (ORDER BY COALESCE(SUM(gr.score), 0) DESC) AS rank_by_score
FROM users u
LEFT JOIN game_sessions gs ON u.user_id = gs.user_id
LEFT JOIN game_results  gr ON gs.session_id = gr.session_id
GROUP BY u.user_id, u.name
ORDER BY rank_by_score;


-- A.6  Dedicated players filter (GROUP BY + HAVING)
-- Find players who have finished more than one mission.
-- HAVING filters the aggregated groups, unlike WHERE which acts on raw rows.
SELECT
    u.user_id,
    u.name         AS player_name,
    COUNT(gr.result_id) AS completed_sessions,
    SUM(gr.score)  AS total_score,
    SUM(gr.water_saved) AS total_water_saved
FROM users u
JOIN game_sessions gs ON u.user_id = gs.user_id
JOIN game_results  gr ON gs.session_id = gr.session_id
WHERE gs.status = 'completed'
GROUP BY u.user_id, u.name
HAVING COUNT(gr.result_id) > 1
ORDER BY total_score DESC;


-- A.7  High scorers using a FROM-clause subquery (derived table)
-- Which players have a cumulative score above 1500?
-- The inner SELECT computes totals first; the outer SELECT filters the groups.
SELECT sub.user_id, sub.player_name, sub.total_score
FROM (
    SELECT
        u.user_id, u.name AS player_name,
        SUM(gr.score) AS total_score
    FROM users u
    JOIN game_sessions gs ON u.user_id = gs.user_id
    JOIN game_results  gr ON gs.session_id = gr.session_id
    GROUP BY u.user_id, u.name
) AS sub
WHERE sub.total_score > 1500
ORDER BY sub.total_score DESC;


-- A.8  Player name and email formatted as a display label (String functions)
-- Demonstrates UPPER(), LOWER(), LENGTH(), CONCAT().
SELECT
    u.user_id,
    UPPER(u.name)                             AS name_upper,
    LOWER(u.email)                            AS email_lower,
    LENGTH(u.name)                            AS name_length,
    CONCAT(u.name, ' <', u.email, '>')        AS display_label
FROM users u
ORDER BY u.user_id;



-- =============================================================
-- SECTION B: SCENARIO ANALYTICS
-- =============================================================

-- B.1  Total sessions per scenario (LEFT JOIN, GROUP BY, COUNT)
-- Which scenario attracts the most players?
SELECT
    s.scenario_id,
    s.scenario_name,
    s.difficulty,
    COUNT(gs.session_id) AS total_sessions
FROM scenarios s
LEFT JOIN game_sessions gs ON s.scenario_id = gs.scenario_id
GROUP BY s.scenario_id, s.scenario_name, s.difficulty
ORDER BY total_sessions DESC, s.scenario_id;


-- B.2  Average score per scenario (AVG, COALESCE)
-- Which scenario generates the highest scores on average?
SELECT
    s.scenario_id,
    s.scenario_name,
    s.difficulty,
    COUNT(gr.result_id) AS completed_plays,
    COALESCE(ROUND(AVG(gr.score), 2), 0.00) AS avg_score
FROM scenarios s
LEFT JOIN game_sessions gs ON s.scenario_id = gs.scenario_id
LEFT JOIN game_results  gr ON gs.session_id = gr.session_id
GROUP BY s.scenario_id, s.scenario_name, s.difficulty
ORDER BY avg_score DESC;


-- B.3  Average groundwater improvement per scenario (AVG, arithmetic)
-- Which scenario restores aquifer levels the most on average?
-- Formula: groundwater_after - groundwater_before
SELECT
    s.scenario_id,
    s.scenario_name,
    s.difficulty,
    COUNT(gr.result_id) AS recorded_results,
    COALESCE(ROUND(AVG(gr.groundwater_after - gr.groundwater_before), 2), 0.00)
        AS avg_groundwater_improvement
FROM scenarios s
LEFT JOIN game_sessions gs ON s.scenario_id = gs.scenario_id
LEFT JOIN game_results  gr ON gs.session_id = gr.session_id
GROUP BY s.scenario_id, s.scenario_name, s.difficulty
ORDER BY avg_groundwater_improvement DESC;


-- B.4  Score distribution per scenario (MIN, MAX, AVG in one query)
-- What is the spread of scores within each scenario?
SELECT
    s.scenario_id,
    s.scenario_name,
    s.difficulty,
    COUNT(gr.result_id)      AS completed_count,
    MIN(gr.score)            AS lowest_score,
    MAX(gr.score)            AS highest_score,
    ROUND(AVG(gr.score), 2)  AS average_score
FROM scenarios s
JOIN game_sessions gs ON s.scenario_id = gs.scenario_id
JOIN game_results  gr ON gs.session_id = gr.session_id
GROUP BY s.scenario_id, s.scenario_name, s.difficulty
ORDER BY s.scenario_id;


-- B.5  Most popular scenario (single row result with LIMIT)
SELECT
    s.scenario_id,
    s.scenario_name,
    s.difficulty,
    COUNT(gs.session_id) AS session_count
FROM scenarios s
JOIN game_sessions gs ON s.scenario_id = gs.scenario_id
GROUP BY s.scenario_id, s.scenario_name, s.difficulty
ORDER BY session_count DESC, s.scenario_id
LIMIT 1;


-- B.6  Scenario completion statistics with CASE-based pivot
-- (Attempts, completed, abandoned, ongoing, and completion rate %)
-- NULLIF prevents division-by-zero if a scenario has 0 sessions.
SELECT
    s.scenario_id,
    s.scenario_name,
    s.difficulty,
    COUNT(gs.session_id)                                             AS total_attempts,
    COUNT(CASE WHEN gs.status = 'completed' THEN 1 END)             AS completed_count,
    COUNT(CASE WHEN gs.status = 'abandoned' THEN 1 END)             AS abandoned_count,
    COUNT(CASE WHEN gs.status = 'ongoing'   THEN 1 END)             AS ongoing_count,
    ROUND(
        100.0 * COUNT(CASE WHEN gs.status = 'completed' THEN 1 END)
              / NULLIF(COUNT(gs.session_id), 0),
        2
    ) AS completion_rate_pct
FROM scenarios s
LEFT JOIN game_sessions gs ON s.scenario_id = gs.scenario_id
GROUP BY s.scenario_id, s.scenario_name, s.difficulty
ORDER BY total_attempts DESC, s.scenario_id;


-- B.7  High-impact scenarios filter (GROUP BY + HAVING on avg water_saved)
-- Only show scenarios where players save more than 150 water units on average.
SELECT
    s.scenario_id,
    s.scenario_name,
    s.difficulty,
    COUNT(gr.result_id)         AS completed_plays,
    ROUND(AVG(gr.score),      2) AS avg_score,
    ROUND(AVG(gr.water_saved), 2) AS avg_water_saved
FROM scenarios s
JOIN game_sessions gs ON s.scenario_id = gs.scenario_id
JOIN game_results  gr ON gs.session_id = gr.session_id
GROUP BY s.scenario_id, s.scenario_name, s.difficulty
HAVING AVG(gr.water_saved) >= 150
ORDER BY avg_water_saved DESC;



-- =============================================================
-- SECTION C: LEADERBOARD  (RANK, DENSE_RANK, ROW_NUMBER)
-- =============================================================

-- C.1  Official leaderboard with RANK(), DENSE_RANK(), and ROW_NUMBER()
-- ROW_NUMBER  → always unique, no ties (1, 2, 3, 4 ...)
-- RANK        → tied scores share a rank; next rank SKIPS (1, 2, 2, 4)
-- DENSE_RANK  → tied scores share a rank; next rank does NOT skip (1, 2, 2, 3)
SELECT
    ROW_NUMBER()  OVER (ORDER BY SUM(gr.score) DESC, u.user_id) AS row_num,
    RANK()        OVER (ORDER BY SUM(gr.score) DESC) AS rank,
    DENSE_RANK()  OVER (ORDER BY SUM(gr.score) DESC) AS dense_rank,
    u.name           AS player_name,
    SUM(gr.score)    AS total_score,
    SUM(gr.water_saved) AS total_water_saved,
    COUNT(gr.result_id) AS completed_sessions
FROM users u
INNER JOIN game_sessions gs ON u.user_id = gs.user_id
INNER JOIN game_results  gr ON gs.session_id = gr.session_id
GROUP BY u.user_id, u.name
ORDER BY dense_rank;



-- =============================================================
-- SECTION D: GROUNDWATER ANALYTICS
-- =============================================================

-- D.1  Global conservation summary (COUNT, SUM, AVG, MIN, MAX)
-- What is the game-wide environmental impact?
SELECT
    COUNT(result_id)                AS total_results_evaluated,
    SUM(water_saved)                AS total_water_saved_overall,
    ROUND(AVG(water_saved),   2)    AS avg_water_saved_per_game,
    MIN(water_saved)                AS min_water_saved,
    MAX(water_saved)                AS max_water_saved,
    MIN(score)                      AS lowest_score_recorded,
    MAX(score)                      AS highest_score_recorded
FROM game_results;


-- D.2  Average groundwater levels before and after all gameplay (AVG)
-- How much did the aquifer recover on average across all missions?
SELECT
    ROUND(AVG(groundwater_before),               2) AS avg_groundwater_before,
    ROUND(AVG(groundwater_after),                2) AS avg_groundwater_after,
    ROUND(AVG(groundwater_after - groundwater_before), 2) AS overall_avg_improvement
FROM game_results;


-- D.3  Session-level groundwater improvement with CASE classification
-- Classify each mission result as Substantial, Moderate, or Minimal recovery.
SELECT
    gr.result_id,
    u.name            AS player_name,
    s.scenario_name,
    gr.groundwater_before,
    gr.groundwater_after,
    (gr.groundwater_after - gr.groundwater_before) AS improvement,
    CASE
        WHEN (gr.groundwater_after - gr.groundwater_before) >= 25 THEN 'Substantial Recovery'
        WHEN (gr.groundwater_after - gr.groundwater_before) >= 15 THEN 'Moderate Recovery'
        ELSE                                                            'Minimal Recovery'
    END AS recovery_category
FROM game_results gr
JOIN game_sessions gs ON gr.session_id = gs.session_id
JOIN users         u  ON gs.user_id    = u.user_id
JOIN scenarios     s  ON gs.scenario_id = s.scenario_id
ORDER BY improvement DESC;


-- D.4  User-wise water conservation performance (SUM, AVG, COALESCE)
-- How much groundwater has each player helped save overall?
SELECT
    u.user_id,
    u.name            AS player_name,
    COUNT(gr.result_id)                                         AS sessions_contributed,
    COALESCE(SUM(gr.water_saved),                          0)   AS total_water_saved,
    COALESCE(ROUND(AVG(gr.water_saved),                   2), 0.00) AS avg_water_saved_per_session,
    COALESCE(ROUND(AVG(gr.groundwater_after - gr.groundwater_before), 2), 0.00)
                                                                AS avg_gw_level_gain
FROM users u
LEFT JOIN game_sessions gs ON u.user_id    = gs.user_id
LEFT JOIN game_results  gr ON gs.session_id = gr.session_id
GROUP BY u.user_id, u.name
ORDER BY total_water_saved DESC;



-- =============================================================
-- SECTION E: SESSION & TEMPORAL ANALYTICS
-- =============================================================

-- E.1  Session status breakdown (GROUP BY, COUNT, scalar subquery for %)
-- What proportion of all sessions are completed vs abandoned vs ongoing?
SELECT
    status,
    COUNT(*) AS session_count,
    ROUND(100.0 * COUNT(*) / (SELECT COUNT(*) FROM game_sessions), 2) AS percentage
FROM game_sessions
GROUP BY status
ORDER BY session_count DESC;


-- E.2  Session duration using EXTRACT and CASE for NULL safety
-- How long did each mission take? EXTRACT(EPOCH …) converts to seconds.
SELECT
    gs.session_id,
    u.name            AS player_name,
    s.scenario_name,
    gs.status,
    gs.started_at,
    gs.ended_at,
    CASE WHEN gs.ended_at IS NOT NULL
         THEN ROUND(EXTRACT(EPOCH FROM (gs.ended_at - gs.started_at)) / 60.0, 1)
         ELSE NULL
    END AS duration_minutes,
    COALESCE(
        CAST(ROUND(EXTRACT(EPOCH FROM (gs.ended_at - gs.started_at)) / 60.0, 0) AS VARCHAR) || ' min',
        'Still in progress'
    ) AS duration_display
FROM game_sessions gs
JOIN users     u ON gs.user_id     = u.user_id
JOIN scenarios s ON gs.scenario_id = s.scenario_id
ORDER BY gs.session_id;


-- E.3  Recent game history – last 5 sessions (ORDER BY timestamp DESC, LIMIT)
SELECT
    gs.session_id,
    gs.started_at,
    u.name            AS player_name,
    s.scenario_name,
    s.difficulty,
    gs.status,
    COALESCE(gr.score,       0) AS score,
    COALESCE(gr.water_saved, 0) AS water_saved
FROM game_sessions gs
JOIN users     u  ON gs.user_id     = u.user_id
JOIN scenarios s  ON gs.scenario_id = s.scenario_id
LEFT JOIN game_results gr ON gs.session_id = gr.session_id
ORDER BY gs.started_at DESC
LIMIT 5;


-- E.4  Users with no completed sessions (Correlated NOT EXISTS subquery)
-- Which registered players have never finished a mission?
SELECT u.user_id, u.name, u.email, u.created_at
FROM users u
WHERE NOT EXISTS (
    SELECT 1
    FROM game_sessions gs
    WHERE gs.user_id = u.user_id AND gs.status = 'completed'
);


-- E.5  Scenario-wise completion rate (CASE-based pivot + NULLIF)
SELECT
    s.scenario_id,
    s.scenario_name,
    s.difficulty,
    COUNT(gs.session_id)                                              AS total_played,
    COUNT(CASE WHEN gs.status = 'completed' THEN 1 END)              AS completed,
    COUNT(CASE WHEN gs.status = 'abandoned' THEN 1 END)              AS abandoned,
    COUNT(CASE WHEN gs.status = 'ongoing'   THEN 1 END)              AS ongoing,
    ROUND(
        100.0 * COUNT(CASE WHEN gs.status = 'completed' THEN 1 END)
              / NULLIF(COUNT(gs.session_id), 0),
        2
    ) AS completion_pct
FROM scenarios s
LEFT JOIN game_sessions gs ON s.scenario_id = gs.scenario_id
GROUP BY s.scenario_id, s.scenario_name, s.difficulty
ORDER BY completion_pct DESC, total_played DESC;


-- E.6  Score progression per player (LAG and LEAD window functions)
-- Did players improve between sessions?
-- LAG  → score from the PREVIOUS session for this user
-- LEAD → score from the NEXT session for this user
SELECT
    gs.user_id,
    u.name            AS player_name,
    gs.session_id,
    s.scenario_name,
    gs.started_at,
    gr.score          AS current_score,
    LAG(gr.score)  OVER (PARTITION BY gs.user_id ORDER BY gs.started_at) AS previous_score,
    gr.score - LAG(gr.score) OVER (PARTITION BY gs.user_id ORDER BY gs.started_at) AS score_growth,
    LEAD(gr.score) OVER (PARTITION BY gs.user_id ORDER BY gs.started_at) AS next_score
FROM game_results  gr
JOIN game_sessions gs ON gr.session_id  = gs.session_id
JOIN users         u  ON gs.user_id     = u.user_id
JOIN scenarios     s  ON gs.scenario_id = s.scenario_id
ORDER BY gs.user_id, gs.started_at;


-- E.7  Monthly activity trends (DATE_TRUNC, TO_CHAR, NOW)
-- How many sessions and how much water was saved each calendar month?
SELECT
    DATE_TRUNC('month', gs.started_at)                                  AS month_start,
    TO_CHAR(DATE_TRUNC('month', gs.started_at), 'FMMonth YYYY')         AS display_month,
    COUNT(gs.session_id)                                                 AS total_sessions,
    COUNT(CASE WHEN gs.status = 'completed' THEN 1 END)                 AS completed_sessions,
    COALESCE(SUM(gr.water_saved), 0)                                     AS total_water_saved,
    TO_CHAR(NOW(), 'YYYY-MM-DD')                                         AS report_generated_on
FROM game_sessions gs
LEFT JOIN game_results gr ON gs.session_id = gr.session_id
GROUP BY DATE_TRUNC('month', gs.started_at)
ORDER BY month_start;


-- E.8  EXTRACT individual date parts from session start times
-- Demonstrates EXTRACT(YEAR …), EXTRACT(MONTH …), EXTRACT(DOW …).
SELECT
    gs.session_id,
    gs.started_at,
    EXTRACT(YEAR  FROM gs.started_at)::INT AS session_year,
    EXTRACT(MONTH FROM gs.started_at)::INT AS session_month,
    EXTRACT(DOW   FROM gs.started_at)::INT AS day_of_week_0sun
FROM game_sessions gs
ORDER BY gs.session_id;



-- =============================================================
-- SECTION F: SAFE TRANSACTION DEMONSTRATION
-- Temporary data remains local to this connection; no project rows are changed.
-- =============================================================
CREATE TEMP TABLE aquaguard_transaction_demo (step TEXT NOT NULL);
BEGIN;
INSERT INTO aquaguard_transaction_demo VALUES ('session prepared');
SAVEPOINT after_session;
INSERT INTO aquaguard_transaction_demo VALUES ('discarded result');
ROLLBACK TO SAVEPOINT after_session;
INSERT INTO aquaguard_transaction_demo VALUES ('validated result');
COMMIT;
-- Expected two rows: the committed session and validated result.
SELECT step FROM aquaguard_transaction_demo ORDER BY step;
DROP TABLE aquaguard_transaction_demo;
