-- =============================================================
-- AquaGuardian – Analytics and SQL Validation
-- Task: DE Task 3 – Checkpoint 3 (Reviewed & Corrected)
-- Author: Yash Kumar
-- Description: Analytical SQL queries for the AquaGuardian
--              groundwater conservation educational game.
--              Run schema.sql, then seed.sql before running this.
-- =============================================================
-- DATABASE SCHEMA REFERENCE (4 Tables):
--   users        : user_id, name, email, password_hash, created_at
--   scenarios    : scenario_id, scenario_name, description, difficulty
--   game_sessions: session_id, user_id, scenario_id, started_at, ended_at, status
--   game_results : result_id, session_id, score, groundwater_before,
--                  groundwater_after, water_saved, completed_at
-- =============================================================


-- =============================================================
-- SECTION A: USER ANALYTICS
-- =============================================================

-- A.1 Total sessions per user
-- Purpose: Count how many times each registered user started a game session.
-- Concept: LEFT JOIN preserves users who have not played any session.
SELECT 
    u.user_id, 
    u.name AS player_name, 
    COUNT(gs.session_id) AS total_sessions
FROM users u
LEFT JOIN game_sessions gs ON u.user_id = gs.user_id
GROUP BY u.user_id, u.name
ORDER BY total_sessions DESC, u.user_id;


-- A.2 Completed sessions per user
-- Purpose: Count only the sessions that each player successfully finished.
-- Concept: Conditional aggregation using COUNT(CASE WHEN ... THEN 1 END).
--          Users with 0 completed sessions return 0 instead of being omitted.
SELECT 
    u.user_id, 
    u.name AS player_name, 
    COUNT(CASE WHEN gs.status = 'completed' THEN 1 END) AS completed_sessions
FROM users u
LEFT JOIN game_sessions gs ON u.user_id = gs.user_id
GROUP BY u.user_id, u.name
ORDER BY completed_sessions DESC, u.user_id;


-- A.3 Average score per user
-- Purpose: Calculate the mean score achieved across completed games for each player.
-- Concept: AVG() ignores NULLs; COALESCE() ensures players without results display 0.00.
SELECT 
    u.user_id, 
    u.name AS player_name, 
    COALESCE(ROUND(AVG(gr.score), 2), 0.00) AS avg_score
FROM users u
LEFT JOIN game_sessions gs ON u.user_id = gs.user_id
LEFT JOIN game_results gr ON gs.session_id = gr.session_id
GROUP BY u.user_id, u.name
ORDER BY avg_score DESC;


-- A.4 Total water saved per user
-- Purpose: Sum the total units of water saved by each player in completed missions.
-- Concept: SUM() aggregate with COALESCE() to gracefully convert NULL to 0.
SELECT 
    u.user_id, 
    u.name AS player_name, 
    COALESCE(SUM(gr.water_saved), 0) AS total_water_saved
FROM users u
LEFT JOIN game_sessions gs ON u.user_id = gs.user_id
LEFT JOIN game_results gr ON gs.session_id = gr.session_id
GROUP BY u.user_id, u.name
ORDER BY total_water_saved DESC;


-- A.5 Best-performing users (Comprehensive Ranking)
-- Purpose: Overview ranking players by their cumulative score using window functions.
-- Concept: DENSE_RANK() window function over total score with tie-handling.
SELECT 
    u.user_id,
    u.name AS player_name,
    COUNT(gr.result_id) AS games_completed,
    COALESCE(SUM(gr.score), 0) AS total_score,
    COALESCE(ROUND(AVG(gr.score), 2), 0.00) AS avg_score,
    COALESCE(SUM(gr.water_saved), 0) AS total_water_saved,
    DENSE_RANK() OVER (ORDER BY COALESCE(SUM(gr.score), 0) DESC) AS rank_by_score
FROM users u
LEFT JOIN game_sessions gs ON u.user_id = gs.user_id
LEFT JOIN game_results gr ON gs.session_id = gr.session_id
GROUP BY u.user_id, u.name
ORDER BY rank_by_score;


-- A.6 Frequent players filter (GROUP BY + HAVING)
-- Purpose: Find committed players who have completed more than 1 session.
-- Concept: HAVING filters aggregated groups (unlike WHERE which filters raw rows).
SELECT 
    u.user_id,
    u.name AS player_name,
    COUNT(gr.result_id) AS completed_sessions,
    SUM(gr.score) AS total_score,
    SUM(gr.water_saved) AS total_water_saved
FROM users u
JOIN game_sessions gs ON u.user_id = gs.user_id
JOIN game_results gr ON gs.session_id = gr.session_id
GROUP BY u.user_id, u.name
HAVING COUNT(gr.result_id) > 1
ORDER BY total_score DESC;



-- =============================================================
-- SECTION B: SCENARIO ANALYTICS
-- =============================================================

-- B.1 Total sessions per scenario
-- Purpose: Measure engagement across all scenarios regardless of status.
-- Concept: LEFT JOIN from scenarios to game_sessions ensures unused scenarios show 0.
SELECT 
    s.scenario_id,
    s.scenario_name,
    s.difficulty,
    COUNT(gs.session_id) AS total_sessions
FROM scenarios s
LEFT JOIN game_sessions gs ON s.scenario_id = gs.scenario_id
GROUP BY s.scenario_id, s.scenario_name, s.difficulty
ORDER BY total_sessions DESC, s.scenario_id;


-- B.2 Average score per scenario
-- Purpose: Compare average scoring difficulty across different conservation scenarios.
-- Concept: Multi-table JOIN linking scenarios, sessions, and outcomes.
SELECT 
    s.scenario_id,
    s.scenario_name,
    s.difficulty,
    COUNT(gr.result_id) AS completed_plays,
    COALESCE(ROUND(AVG(gr.score), 2), 0.00) AS avg_score
FROM scenarios s
LEFT JOIN game_sessions gs ON s.scenario_id = gs.scenario_id
LEFT JOIN game_results gr ON gs.session_id = gr.session_id
GROUP BY s.scenario_id, s.scenario_name, s.difficulty
ORDER BY avg_score DESC;


-- B.3 Average groundwater improvement per scenario
-- Purpose: Calculate average percentage points of aquifer recharge per scenario.
-- Formula: groundwater_after - groundwater_before
SELECT 
    s.scenario_id,
    s.scenario_name,
    s.difficulty,
    COUNT(gr.result_id) AS recorded_results,
    COALESCE(ROUND(AVG(gr.groundwater_after - gr.groundwater_before), 2), 0.00) AS avg_groundwater_improvement
FROM scenarios s
LEFT JOIN game_sessions gs ON s.scenario_id = gs.scenario_id
LEFT JOIN game_results gr ON gs.session_id = gr.session_id
GROUP BY s.scenario_id, s.scenario_name, s.difficulty
ORDER BY avg_groundwater_improvement DESC;


-- B.4 Score distribution per scenario (MIN, MAX, AVG)
-- Purpose: Demonstrate MIN() and MAX() to inspect score spread per scenario.
-- Concept: MIN() finds lowest score, MAX() finds highest score, AVG() gives mean.
SELECT 
    s.scenario_id,
    s.scenario_name,
    s.difficulty,
    COUNT(gr.result_id) AS completed_count,
    MIN(gr.score) AS lowest_score,
    MAX(gr.score) AS highest_score,
    ROUND(AVG(gr.score), 2) AS average_score
FROM scenarios s
JOIN game_sessions gs ON s.scenario_id = gs.scenario_id
JOIN game_results gr ON gs.session_id = gr.session_id
GROUP BY s.scenario_id, s.scenario_name, s.difficulty
ORDER BY s.scenario_id;


-- B.5 Most popular scenario
-- Purpose: Identify the scenario with the highest player attempts.
-- Concept: ORDER BY count DESC with LIMIT 1.
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


-- B.6 Scenario completion statistics
-- Purpose: Complete breakdown of attempts, completions, abandonments, and success rates.
-- Concept: Conditional counting with NULLIF to prevent division by zero.
SELECT 
    s.scenario_id,
    s.scenario_name,
    s.difficulty,
    COUNT(gs.session_id) AS total_attempts,
    COUNT(CASE WHEN gs.status = 'completed' THEN 1 END) AS completed_count,
    COUNT(CASE WHEN gs.status = 'abandoned' THEN 1 END) AS abandoned_count,
    COUNT(CASE WHEN gs.status = 'ongoing' THEN 1 END) AS ongoing_count,
    ROUND(
        100.0 * COUNT(CASE WHEN gs.status = 'completed' THEN 1 END) / 
        NULLIF(COUNT(gs.session_id), 0), 
        2
    ) AS completion_rate_pct
FROM scenarios s
LEFT JOIN game_sessions gs ON s.scenario_id = gs.scenario_id
GROUP BY s.scenario_id, s.scenario_name, s.difficulty
ORDER BY total_attempts DESC, s.scenario_id;


-- B.7 High-impact conservation scenarios (GROUP BY + HAVING)
-- Purpose: Filter for scenarios achieving an average water savings of 150+ units.
-- Concept: HAVING clause applied after aggregation to filter scenario groups.
SELECT 
    s.scenario_id,
    s.scenario_name,
    s.difficulty,
    COUNT(gr.result_id) AS completed_plays,
    ROUND(AVG(gr.score), 2) AS avg_score,
    ROUND(AVG(gr.water_saved), 2) AS avg_water_saved
FROM scenarios s
JOIN game_sessions gs ON s.scenario_id = gs.scenario_id
JOIN game_results gr ON gs.session_id = gr.session_id
GROUP BY s.scenario_id, s.scenario_name, s.difficulty
HAVING AVG(gr.water_saved) >= 150
ORDER BY avg_water_saved DESC;



-- =============================================================
-- SECTION C: LEADERBOARD (RANK vs DENSE_RANK)
-- =============================================================

-- C.1 AquaGuardian Official Leaderboard
-- Purpose: Display rankings of players based on cumulative score.
-- Concept: Side-by-side demonstration of RANK() and DENSE_RANK() window functions.
--          RANK() leaves gaps when ties occur (e.g., 1, 2, 2, 4).
--          DENSE_RANK() never skips numbers (e.g., 1, 2, 2, 3).
SELECT 
    RANK() OVER (ORDER BY SUM(gr.score) DESC) AS rank,
    DENSE_RANK() OVER (ORDER BY SUM(gr.score) DESC) AS dense_rank,
    u.name AS player_name,
    SUM(gr.score) AS total_score,
    SUM(gr.water_saved) AS total_water_saved,
    COUNT(gr.result_id) AS completed_sessions
FROM users u
INNER JOIN game_sessions gs ON u.user_id = gs.user_id
INNER JOIN game_results gr ON gs.session_id = gr.session_id
GROUP BY u.user_id, u.name
ORDER BY dense_rank, total_score DESC;



-- =============================================================
-- SECTION D: GROUNDWATER ANALYTICS
-- =============================================================

-- D.1 Global conservation summary (COUNT, SUM, AVG, MIN, MAX)
-- Purpose: Global game-wide metrics showcasing all 5 standard aggregate functions.
SELECT 
    COUNT(result_id) AS total_results_evaluated,
    SUM(water_saved) AS total_water_saved_overall,
    ROUND(AVG(water_saved), 2) AS avg_water_saved_per_game,
    MIN(water_saved) AS min_water_saved_single_game,
    MAX(water_saved) AS max_water_saved_single_game,
    MIN(score) AS lowest_score_recorded,
    MAX(score) AS highest_score_recorded
FROM game_results;


-- D.2 Average groundwater before and after gameplay
-- Purpose: Compare mean pre-game vs post-game groundwater percentages.
SELECT 
    ROUND(AVG(groundwater_before), 2) AS avg_groundwater_before,
    ROUND(AVG(groundwater_after), 2) AS avg_groundwater_after,
    ROUND(AVG(groundwater_after - groundwater_before), 2) AS overall_avg_improvement
FROM game_results;


-- D.3 Groundwater improvement per session with recovery category
-- Purpose: Evaluate every individual conservation run with qualitative classification.
-- Concept: Arithmetic column difference plus CASE WHEN categorization.
SELECT 
    gr.result_id,
    gs.session_id,
    u.name AS player_name,
    s.scenario_name,
    gr.groundwater_before,
    gr.groundwater_after,
    (gr.groundwater_after - gr.groundwater_before) AS groundwater_improvement,
    CASE 
        WHEN (gr.groundwater_after - gr.groundwater_before) >= 25 THEN 'Substantial Recovery'
        WHEN (gr.groundwater_after - gr.groundwater_before) >= 15 THEN 'Moderate Recovery'
        ELSE 'Minimal Recovery'
    END AS conservation_category
FROM game_results gr
JOIN game_sessions gs ON gr.session_id = gs.session_id
JOIN users u ON gs.user_id = u.user_id
JOIN scenarios s ON gs.scenario_id = s.scenario_id
ORDER BY groundwater_improvement DESC;


-- D.4 User-wise water conservation performance
-- Purpose: Assess individual ecological impact for environmental awareness education.
SELECT 
    u.user_id,
    u.name AS player_name,
    COUNT(gr.result_id) AS sessions_contributed,
    COALESCE(SUM(gr.water_saved), 0) AS total_water_saved,
    COALESCE(ROUND(AVG(gr.water_saved), 2), 0.00) AS avg_water_saved_per_session,
    COALESCE(ROUND(AVG(gr.groundwater_after - gr.groundwater_before), 2), 0.00) AS avg_groundwater_level_gain
FROM users u
LEFT JOIN game_sessions gs ON u.user_id = gs.user_id
LEFT JOIN game_results gr ON gs.session_id = gr.session_id
GROUP BY u.user_id, u.name
ORDER BY total_water_saved DESC, avg_groundwater_level_gain DESC;



-- =============================================================
-- SECTION E: SESSION & TEMPORAL ANALYTICS
-- =============================================================

-- E.1 Completed versus ongoing sessions
-- Purpose: Analyze session state breakdown across the game.
-- Concept: GROUP BY status with dynamic subquery calculation for overall percentage.
SELECT 
    status,
    COUNT(*) AS session_count,
    ROUND(100.0 * COUNT(*) / (SELECT COUNT(*) FROM game_sessions), 2) AS percentage
FROM game_sessions
GROUP BY status
ORDER BY session_count DESC;


-- E.2 Session duration
-- Purpose: Calculate how long each session took, handling in-progress sessions gracefully.
-- Concept: Date/Time subtraction (interval) and EXTRACT(EPOCH ...) for minutes.
SELECT 
    gs.session_id,
    u.name AS player_name,
    s.scenario_name,
    gs.status,
    gs.started_at,
    gs.ended_at,
    CASE 
        WHEN gs.ended_at IS NOT NULL THEN (gs.ended_at - gs.started_at)
        ELSE NULL 
    END AS duration_interval,
    CASE 
        WHEN gs.ended_at IS NOT NULL THEN 
            ROUND(EXTRACT(EPOCH FROM (gs.ended_at - gs.started_at)) / 60.0, 2)
        ELSE NULL 
    END AS duration_minutes,
    COALESCE(
        CAST(ROUND(EXTRACT(EPOCH FROM (gs.ended_at - gs.started_at)) / 60.0, 0) AS VARCHAR) || ' mins',
        'Still in progress'
    ) AS duration_display
FROM game_sessions gs
JOIN users u ON gs.user_id = u.user_id
JOIN scenarios s ON gs.scenario_id = s.scenario_id
ORDER BY gs.session_id;


-- E.3 Recent game history
-- Purpose: Display the 5 most recent gaming activities across all users.
-- Concept: Multi-table JOIN with ORDER BY timestamp DESC and LIMIT 5.
SELECT 
    gs.session_id,
    gs.started_at,
    u.name AS player_name,
    s.scenario_name,
    s.difficulty,
    gs.status,
    COALESCE(gr.score, 0) AS score,
    COALESCE(gr.water_saved, 0) AS water_saved
FROM game_sessions gs
JOIN users u ON gs.user_id = u.user_id
JOIN scenarios s ON gs.scenario_id = s.scenario_id
LEFT JOIN game_results gr ON gs.session_id = gr.session_id
ORDER BY gs.started_at DESC
LIMIT 5;


-- E.4 Users with no completed sessions
-- Purpose: Identify inactive or struggling players who registered but haven't finished a session.
-- Concept: Correlated NOT EXISTS subquery — ideal for anti-filtering.
SELECT 
    u.user_id,
    u.name,
    u.email,
    u.created_at
FROM users u
WHERE NOT EXISTS (
    SELECT 1 
    FROM game_sessions gs 
    WHERE gs.user_id = u.user_id AND gs.status = 'completed'
);


-- E.5 Scenario-wise completion rate
-- Purpose: Detect whether certain scenarios are too difficult or abandoned frequently.
SELECT 
    s.scenario_id,
    s.scenario_name,
    s.difficulty,
    COUNT(gs.session_id) AS total_played,
    COUNT(CASE WHEN gs.status = 'completed' THEN 1 END) AS completed_sessions,
    COUNT(CASE WHEN gs.status = 'abandoned' THEN 1 END) AS abandoned_sessions,
    COUNT(CASE WHEN gs.status = 'ongoing' THEN 1 END) AS ongoing_sessions,
    ROUND(
        100.0 * COUNT(CASE WHEN gs.status = 'completed' THEN 1 END) / 
        NULLIF(COUNT(gs.session_id), 0), 
        2
    ) AS completion_rate_percentage
FROM scenarios s
LEFT JOIN game_sessions gs ON s.scenario_id = gs.scenario_id
GROUP BY s.scenario_id, s.scenario_name, s.difficulty
ORDER BY completion_rate_percentage DESC, total_played DESC;


-- E.6 Session score progression per player (LAG and LEAD window functions)
-- Purpose: Track player learning curve and score changes across consecutive games.
-- Concept: LAG() fetches the preceding row's value; LEAD() fetches the succeeding row's value.
--          PARTITION BY user_id isolates history to each individual player.
SELECT 
    gs.user_id,
    u.name AS player_name,
    gs.session_id,
    s.scenario_name,
    gs.started_at,
    gr.score AS current_score,
    LAG(gr.score) OVER (PARTITION BY gs.user_id ORDER BY gs.started_at) AS previous_score,
    gr.score - LAG(gr.score) OVER (PARTITION BY gs.user_id ORDER BY gs.started_at) AS score_growth,
    LEAD(gr.score) OVER (PARTITION BY gs.user_id ORDER BY gs.started_at) AS next_score
FROM game_results gr
JOIN game_sessions gs ON gr.session_id = gs.session_id
JOIN users u ON gs.user_id = u.user_id
JOIN scenarios s ON gs.scenario_id = s.scenario_id
ORDER BY gs.user_id, gs.started_at;


-- E.7 Monthly gameplay activity trends (DATE_TRUNC, TO_CHAR, NOW)
-- Purpose: Aggregate sessions and water saved by calendar month for temporal analysis.
-- Concept: DATE_TRUNC('month', ...) groups timestamps into monthly buckets.
--          TO_CHAR(...) formats dates into human-readable strings.
--          NOW() provides the current execution timestamp for reporting.
SELECT 
    DATE_TRUNC('month', gs.started_at) AS month_start,
    TO_CHAR(DATE_TRUNC('month', gs.started_at), 'FMMonth YYYY') AS display_month,
    COUNT(gs.session_id) AS total_sessions_started,
    COUNT(CASE WHEN gs.status = 'completed' THEN 1 END) AS completed_sessions,
    COALESCE(SUM(gr.water_saved), 0) AS total_water_saved,
    TO_CHAR(NOW(), 'YYYY-MM-DD') AS report_generated_on
FROM game_sessions gs
LEFT JOIN game_results gr ON gs.session_id = gr.session_id
GROUP BY DATE_TRUNC('month', gs.started_at)
ORDER BY month_start;
