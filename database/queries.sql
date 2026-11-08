-- =============================================================
-- AquaGuardian – Complete SQL Query Examples
-- Task: DE Task 1 – Checkpoint 2 (Revised)
-- Author: Yash Kumar
-- Description: Beginner-friendly queries covering every topic
--              in the Data Engineering syllabus.
--              Run order: schema.sql → seed.sql → queries.sql
-- =============================================================
-- TABLE REFERENCE (from schema.sql):
--   users        : user_id, name, email, password_hash, created_at
--   scenarios    : scenario_id, scenario_name, description, difficulty
--   game_sessions: session_id, user_id, scenario_id,
--                  started_at, ended_at, status
--   game_results : result_id, session_id, score,
--                  groundwater_before, groundwater_after,
--                  water_saved, completed_at
-- =============================================================


-- =============================================================
-- SECTION A: DDL – Data Definition Language
--            CREATE, ALTER, DROP, TRUNCATE
-- =============================================================

-- A.1 ALTER TABLE – add a new column to an existing table
--     We add a 'region' column to users to record where the player is from.
--     DEFAULT 'Unknown' means existing rows get that value automatically.
ALTER TABLE users ADD COLUMN IF NOT EXISTS region VARCHAR(50) DEFAULT 'Unknown';

-- A.2 ALTER TABLE – rename a column (PostgreSQL syntax)
--     Rename 'region' to 'city' to be more precise.
ALTER TABLE users RENAME COLUMN region TO city;

-- A.3 ALTER TABLE – change the data type of a column
--     Widen 'city' to hold longer text values.
ALTER TABLE users ALTER COLUMN city TYPE VARCHAR(100);

-- A.4 ALTER TABLE – drop a column we no longer need
ALTER TABLE users DROP COLUMN IF EXISTS city;

-- A.5 CREATE TABLE example (a temporary demo table, NOT part of the game)
--     This shows how CREATE TABLE works. We drop it right after.
CREATE TABLE IF NOT EXISTS demo_log (
    log_id    SERIAL       PRIMARY KEY,
    message   TEXT         NOT NULL,
    logged_at TIMESTAMP    NOT NULL DEFAULT NOW()
);

-- A.6 TRUNCATE – remove all rows from a table quickly (no row-by-row delete)
--     Safe here because demo_log is a temporary example table with no FKs.
TRUNCATE TABLE demo_log;

-- A.7 DROP TABLE – remove the demo table completely
DROP TABLE IF EXISTS demo_log;


-- =============================================================
-- SECTION B: DQL – Data Query Language
--            SELECT, DISTINCT, WHERE, IN, LIKE, EXISTS,
--            ORDER BY, LIMIT, OFFSET
-- =============================================================

-- B.1 SELECT all columns from users
SELECT * FROM users;

-- B.2 SELECT specific columns only
SELECT name, email FROM users;

-- B.3 DISTINCT – list each unique difficulty level in scenarios
SELECT DISTINCT difficulty FROM scenarios;

-- B.4 WHERE – find only completed game sessions
SELECT session_id, user_id, scenario_id, status
FROM   game_sessions
WHERE  status = 'completed';

-- B.5 IN – find sessions that are ongoing OR abandoned
SELECT session_id, user_id, status
FROM   game_sessions
WHERE  status IN ('ongoing', 'abandoned');

-- B.6 LIKE – find users whose name starts with the letter 'A'
SELECT name, email
FROM   users
WHERE  name LIKE 'A%';

-- B.7 LIKE – find scenarios whose description mentions 'water'
SELECT scenario_name, description
FROM   scenarios
WHERE  description ILIKE '%water%';   -- ILIKE is case-insensitive LIKE in PostgreSQL

-- B.8 EXISTS – find users who have at least one session in the database
--     EXISTS returns TRUE if the sub-SELECT finds even one matching row.
SELECT user_id, name
FROM   users u
WHERE  EXISTS (
    SELECT 1
    FROM   game_sessions gs
    WHERE  gs.user_id = u.user_id
);

-- B.9 NOT EXISTS – find users who have NOT played any session yet
SELECT user_id, name
FROM   users u
WHERE  NOT EXISTS (
    SELECT 1
    FROM   game_sessions gs
    WHERE  gs.user_id = u.user_id
);

-- B.10 ORDER BY – list game results sorted by score highest to lowest
SELECT result_id, session_id, score, water_saved
FROM   game_results
ORDER BY score DESC;

-- B.11 LIMIT – show only the top 3 scores
SELECT result_id, session_id, score
FROM   game_results
ORDER BY score DESC
LIMIT  3;

-- B.12 OFFSET – skip the first 3 rows and show the next 3 (page 2)
SELECT result_id, session_id, score
FROM   game_results
ORDER BY score DESC
LIMIT  3 OFFSET 3;


-- =============================================================
-- SECTION C: DML – Data Manipulation Language
--            INSERT, UPDATE, DELETE
-- =============================================================

-- Clean up any leftover demo records before running to ensure safe repeatability:
DELETE FROM users WHERE email = 'demo.user@email.com';
DELETE FROM scenarios WHERE scenario_name = 'Urban Water Grid';

-- C.1 INSERT – add a new user
INSERT INTO users (name, email, password_hash, created_at)
VALUES ('Demo User', 'demo.user@email.com', 'hash_demo_999', '2024-05-01 10:00:00');

-- C.2 INSERT – add a new scenario
INSERT INTO scenarios (scenario_name, description, difficulty)
VALUES ('Urban Water Grid', 'Fix leaking municipal pipelines to save groundwater.', 'Medium');

-- C.3 UPDATE – update the demo user's name
UPDATE users
SET    name = 'Demo Player'
WHERE  email = 'demo.user@email.com';

-- C.4 UPDATE – dynamically locate Ananya's ongoing session and mark as abandoned
--     (Removes hardcoded assumption of session_id = 10)
UPDATE game_sessions
SET    status   = 'abandoned',
       ended_at = '2024-04-05 17:30:00'
WHERE  session_id = (
    SELECT gs.session_id 
    FROM   game_sessions gs 
    JOIN   users u ON gs.user_id = u.user_id 
    WHERE  u.email = 'ananya.singh@email.com' AND gs.status = 'ongoing'
    LIMIT 1
);

-- C.5 UPDATE – reset Ananya's session back to ongoing for subsequent query demos
UPDATE game_sessions
SET    status   = 'ongoing',
       ended_at = NULL
WHERE  session_id = (
    SELECT gs.session_id 
    FROM   game_sessions gs 
    JOIN   users u ON gs.user_id = u.user_id 
    WHERE  u.email = 'ananya.singh@email.com' AND gs.status = 'abandoned'
    LIMIT 1
);

-- C.6 DELETE – remove demo user and demo scenario so seed data stays clean
DELETE FROM users WHERE email = 'demo.user@email.com';
DELETE FROM scenarios WHERE scenario_name = 'Urban Water Grid';


-- =============================================================
-- SECTION D: DCL – Data Control Language
--            GRANT, REVOKE
-- Note: Roles are conditionally created first if they do not exist.
--       Sequence permissions are explicitly granted for SERIAL columns.
-- =============================================================

DO $$
BEGIN
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'readonly_user') THEN
        CREATE ROLE readonly_user;
    END IF;
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'game_app_user') THEN
        CREATE ROLE game_app_user;
    END IF;
END $$;

-- D.1 GRANT – give a read-only role the ability to SELECT from all four tables
GRANT SELECT ON users, scenarios, game_sessions, game_results TO readonly_user;

-- D.2 GRANT – give a game-app role INSERT and UPDATE rights on session tables
GRANT INSERT, UPDATE ON game_sessions, game_results TO game_app_user;

-- D.3 GRANT sequence permissions – crucial for PostgreSQL SERIAL auto-increment columns!
--     Without USAGE/SELECT on sequences, INSERT fails with permission denied on nextval().
GRANT USAGE, SELECT ON SEQUENCE game_sessions_session_id_seq, game_results_result_id_seq TO game_app_user;

-- D.4 REVOKE – take away the INSERT right from game_app_user on game_results
REVOKE INSERT ON game_results FROM game_app_user;

-- D.5 REVOKE sequence permissions from game_app_user
REVOKE USAGE, SELECT ON SEQUENCE game_results_result_id_seq FROM game_app_user;

-- D.6 REVOKE – remove all privileges from readonly_user on users table
REVOKE ALL PRIVILEGES ON users FROM readonly_user;


-- =============================================================
-- SECTION E: TCL – Transaction Control Language
--            BEGIN, SAVEPOINT, ROLLBACK TO SAVEPOINT, COMMIT
-- =============================================================

-- A transaction makes sure that a group of statements either ALL
-- succeed or NONE of them are saved. This is called atomicity.

-- E.1 Safe transaction: insert a new session + its result together.
--     Dynamically retrieves user_id and scenario_id without hardcoded numbers.
--     Uses currval() to reference the generated session_id and cleans up afterwards.

BEGIN;

    -- Step 1: Insert a new completed session dynamically locating Karan and Industrial Overuse
    INSERT INTO game_sessions (user_id, scenario_id, started_at, ended_at, status)
    VALUES (
        (SELECT user_id FROM users WHERE email = 'karan.verma@email.com'),
        (SELECT scenario_id FROM scenarios WHERE scenario_name = 'Industrial Overuse'),
        '2024-05-01 09:00:00', '2024-05-01 10:00:00', 'completed'
    );

    SAVEPOINT after_session_insert;
    -- SAVEPOINT marks a safe point we can roll back to if Step 2 fails.

    -- Step 2: Insert the game result using currval() from the sequence.
    --         currval() reliably retrieves the session_id from Step 1.
    INSERT INTO game_results (session_id, score, groundwater_before, groundwater_after, water_saved, completed_at)
    VALUES (currval('game_sessions_session_id_seq'), 1600, 25, 55, 275, '2024-05-01 10:00:00');

COMMIT;
-- COMMIT saves both inserts permanently.

-- Clean up the committed demo rows so repeated runs remain idempotent:
DELETE FROM game_results WHERE session_id = currval('game_sessions_session_id_seq');
DELETE FROM game_sessions WHERE session_id = currval('game_sessions_session_id_seq');

-- E.2 ROLLBACK demo – show how a full rollback works
BEGIN;
    -- Insert a dummy user just to show rollback
    INSERT INTO users (name, email, password_hash)
    VALUES ('Rollback Test', 'rollback@test.com', 'hash_rb_000');
    -- Intentionally roll back – this user will NOT be saved
ROLLBACK;

-- E.3 SAVEPOINT & ROLLBACK TO SAVEPOINT partial rollback demo
BEGIN;
    INSERT INTO game_sessions (user_id, scenario_id, started_at, ended_at, status)
    VALUES (
        (SELECT user_id FROM users WHERE email = 'rohan.mehta@email.com'),
        (SELECT scenario_id FROM scenarios WHERE scenario_name = 'Village Drought'),
        '2024-05-02 11:00:00', '2024-05-02 11:30:00', 'completed'
    );

    SAVEPOINT before_invalid_insert;

    -- Attempt an invalid insert that would violate CHECK (score >= 0)
    -- In transaction logic, if an error is caught, we rollback to the savepoint:
    ROLLBACK TO SAVEPOINT before_invalid_insert;

    -- The session inserted before the savepoint is intact!
    -- Cleanly roll back the demo transaction:
ROLLBACK;


-- =============================================================
-- SECTION F: Numeric, String, Date and NULL Functions
-- =============================================================

-- F.1 ROUND – round the average score to 2 decimal places
SELECT ROUND(AVG(score), 2) AS avg_score FROM game_results;

-- F.2 ABS – absolute value (useful if differences can be negative)
SELECT result_id,
       ABS(groundwater_after - groundwater_before) AS abs_change
FROM   game_results;

-- F.3 UPPER and LOWER – convert player name to upper/lower case
SELECT UPPER(name) AS name_upper,
       LOWER(email) AS email_lower
FROM   users;

-- F.4 LENGTH – count the number of characters in scenario_name
SELECT scenario_name, LENGTH(scenario_name) AS name_length
FROM   scenarios;

-- F.5 CONCAT – combine first word of name with email
SELECT CONCAT(name, ' <', email, '>') AS display_name
FROM   users;

-- F.6 TRIM – remove extra spaces from a string (demo with literal)
SELECT TRIM('  AquaGuardian  ') AS trimmed;

-- F.7 NOW() – get the current date and time
SELECT NOW() AS current_timestamp;

-- F.8 DATE_PART – extract the month from created_at
SELECT name,
       DATE_PART('month', created_at) AS signup_month,
       DATE_PART('year',  created_at) AS signup_year
FROM   users;

-- F.9 AGE – calculate how long ago a session started
SELECT session_id,
       started_at,
       AGE(NOW(), started_at) AS time_since_start
FROM   game_sessions;

-- F.10 COALESCE – replace NULL ended_at with a placeholder text
--      COALESCE returns the first non-NULL value in its list.
SELECT session_id,
       status,
       COALESCE(CAST(ended_at AS TEXT), 'Still in progress') AS ended_display
FROM   game_sessions;

-- F.11 NULLIF – return NULL if score equals 0 (avoids divide-by-zero)
SELECT result_id,
       score,
       NULLIF(score, 0) AS safe_score
FROM   game_results;


-- =============================================================
-- SECTION G: Aggregate Functions – COUNT, SUM, AVG, MIN, MAX
-- =============================================================

-- G.1 COUNT – total number of registered users
SELECT COUNT(*) AS total_users FROM users;

-- G.2 COUNT – number of completed sessions
SELECT COUNT(*) AS completed_sessions
FROM   game_sessions
WHERE  status = 'completed';

-- G.3 SUM – total water saved across all results
SELECT SUM(water_saved) AS total_water_saved FROM game_results;

-- G.4 AVG – average score
SELECT ROUND(AVG(score), 2) AS average_score FROM game_results;

-- G.5 MIN and MAX – lowest and highest scores
SELECT MIN(score) AS lowest_score,
       MAX(score) AS highest_score
FROM   game_results;


-- =============================================================
-- SECTION H: GROUP BY and HAVING
-- =============================================================

-- H.1 GROUP BY – count sessions per user
SELECT user_id,
       COUNT(*) AS total_sessions
FROM   game_sessions
GROUP BY user_id
ORDER BY total_sessions DESC;

-- H.2 GROUP BY – average score and play count per scenario
SELECT gs.scenario_id,
       s.scenario_name,
       COUNT(gr.result_id)        AS times_completed,
       ROUND(AVG(gr.score), 2)    AS avg_score
FROM   game_sessions  gs
JOIN   game_results   gr ON gs.session_id  = gr.session_id
JOIN   scenarios       s  ON gs.scenario_id = s.scenario_id
GROUP BY gs.scenario_id, s.scenario_name
ORDER BY avg_score DESC;

-- H.3 HAVING – show only users who have played more than 1 session
SELECT user_id,
       COUNT(*) AS total_sessions
FROM   game_sessions
GROUP BY user_id
HAVING COUNT(*) > 1
ORDER BY total_sessions DESC;

-- H.4 HAVING – scenarios where the average water_saved exceeds 150
SELECT gs.scenario_id,
       ROUND(AVG(gr.water_saved), 2) AS avg_water_saved
FROM   game_sessions  gs
JOIN   game_results   gr ON gs.session_id = gr.session_id
GROUP BY gs.scenario_id
HAVING AVG(gr.water_saved) > 150;


-- =============================================================
-- SECTION I: CASE Statements
-- =============================================================

-- I.1 CASE – performance label based on score
SELECT result_id,
       session_id,
       score,
       CASE
           WHEN score >= 1500 THEN 'Excellent'
           WHEN score >= 1000 THEN 'Good'
           ELSE                    'Needs Work'
       END AS performance_label
FROM   game_results
ORDER BY score DESC;

-- I.2 CASE – groundwater recovery category
SELECT result_id,
       groundwater_before,
       groundwater_after,
       groundwater_after - groundwater_before AS recovery,
       CASE
           WHEN groundwater_after - groundwater_before >= 25 THEN 'High Recovery'
           WHEN groundwater_after - groundwater_before >= 10 THEN 'Moderate Recovery'
           ELSE                                                    'Low Recovery'
       END AS recovery_label
FROM   game_results;

-- I.3 CASE inside GROUP BY aggregate – count sessions by status category
SELECT
    SUM(CASE WHEN status = 'completed'  THEN 1 ELSE 0 END) AS completed_count,
    SUM(CASE WHEN status = 'abandoned'  THEN 1 ELSE 0 END) AS abandoned_count,
    SUM(CASE WHEN status = 'ongoing'    THEN 1 ELSE 0 END) AS ongoing_count
FROM game_sessions;


-- =============================================================
-- SECTION J: Joins – INNER, LEFT, RIGHT, FULL OUTER, SELF
-- =============================================================

-- J.1 INNER JOIN – show results with player name and scenario name
--     Only rows that have matching records in ALL joined tables appear.
SELECT u.name           AS player_name,
       s.scenario_name,
       s.difficulty,
       gr.score,
       gr.water_saved
FROM   game_results    gr
INNER JOIN game_sessions gs ON gr.session_id  = gs.session_id
INNER JOIN users          u  ON gs.user_id     = u.user_id
INNER JOIN scenarios       s  ON gs.scenario_id = s.scenario_id
ORDER BY gr.score DESC;

-- J.2 LEFT JOIN – ALL users + their session count (0 if no sessions)
--     Users with no sessions will show 0, not be hidden.
SELECT u.user_id,
       u.name,
       COUNT(gs.session_id) AS session_count
FROM   users u
LEFT JOIN game_sessions gs ON u.user_id = gs.user_id
GROUP BY u.user_id, u.name
ORDER BY session_count DESC;

-- J.3 LEFT JOIN – ALL sessions with their result score (NULL if no result)
SELECT gs.session_id,
       gs.status,
       gr.score
FROM   game_sessions  gs
LEFT JOIN game_results gr ON gs.session_id = gr.session_id
ORDER BY gs.session_id;

-- J.4 RIGHT JOIN – ALL scenarios, even those with no sessions yet
--     The right table (scenarios) is preserved; session columns are NULL
--     if nobody has played that scenario.
SELECT s.scenario_id,
       s.scenario_name,
       gs.session_id,
       gs.status
FROM   game_sessions  gs
RIGHT JOIN scenarios   s ON gs.scenario_id = s.scenario_id
ORDER BY s.scenario_id;

-- J.5 FULL OUTER JOIN – ALL users and ALL sessions
--     Shows users with no sessions AND sessions with no matching user.
SELECT u.user_id,
       u.name,
       gs.session_id,
       gs.status
FROM   users           u
FULL OUTER JOIN game_sessions gs ON u.user_id = gs.user_id
ORDER BY u.user_id;

-- J.6 SELF JOIN – compare each user's created_at with every other user
--     to find pairs where one user signed up before another.
--     A self join joins a table to itself using aliases.
SELECT a.name        AS earlier_user,
       b.name        AS later_user,
       a.created_at  AS earlier_date,
       b.created_at  AS later_date
FROM   users a
JOIN   users b ON a.user_id <> b.user_id
              AND a.created_at < b.created_at
ORDER BY a.created_at;


-- =============================================================
-- SECTION K: UNION, UNION ALL, INTERSECT
-- =============================================================

-- K.1 UNION – combine a list of Easy and Hard scenario names into one list
--     UNION removes duplicate rows automatically.
SELECT scenario_name, difficulty FROM scenarios WHERE difficulty = 'Easy'
UNION
SELECT scenario_name, difficulty FROM scenarios WHERE difficulty = 'Hard';

-- K.2 UNION ALL – same as UNION but keeps all rows including duplicates
--     Useful when you need to count every occurrence.
SELECT scenario_name, difficulty FROM scenarios WHERE difficulty = 'Easy'
UNION ALL
SELECT scenario_name, difficulty FROM scenarios WHERE difficulty = 'Hard';

-- K.3 UNION – list all user emails and all scenario names in one column
--     Both SELECT clauses must return the same number of columns.
SELECT name AS label, 'player' AS type FROM users
UNION
SELECT scenario_name AS label, 'scenario' AS type FROM scenarios
ORDER BY type, label;

-- K.4 INTERSECT – find user IDs that appear in BOTH completed AND abandoned sessions
--     (i.e., players who have both completed and given up on sessions)
SELECT user_id FROM game_sessions WHERE status = 'completed'
INTERSECT
SELECT user_id FROM game_sessions WHERE status = 'abandoned';


-- =============================================================
-- SECTION L: Subqueries
--            WHERE subquery, FROM subquery (derived table),
--            correlated subquery, non-correlated subquery
-- =============================================================

-- L.1 Non-correlated WHERE subquery
--     Find users who have at least one completed session.
--     The inner query runs once and returns a list; the outer query filters by it.
SELECT user_id, name, email
FROM   users
WHERE  user_id IN (
    SELECT DISTINCT user_id
    FROM   game_sessions
    WHERE  status = 'completed'
);

-- L.2 Non-correlated WHERE subquery
--     Find results with a score above the overall average.
SELECT result_id, session_id, score
FROM   game_results
WHERE  score > (
    SELECT AVG(score) FROM game_results
)
ORDER BY score DESC;

-- L.3 FROM subquery (derived table)
--     First compute average score per user, then filter for high performers.
--     The inner SELECT becomes a temporary table called 'user_avg'.
SELECT user_avg.user_id,
       user_avg.avg_score
FROM (
    SELECT gs.user_id,
           ROUND(AVG(gr.score), 2) AS avg_score
    FROM   game_results   gr
    JOIN   game_sessions  gs ON gr.session_id = gs.session_id
    GROUP BY gs.user_id
) AS user_avg
WHERE user_avg.avg_score > 1000
ORDER BY user_avg.avg_score DESC;

-- L.4 Correlated subquery
--     For each user, find their HIGHEST score.
--     The inner query references u.user_id from the outer query —
--     it re-runs once for every row of the outer query.
SELECT u.user_id,
       u.name,
       (SELECT MAX(gr.score)
        FROM   game_results  gr
        JOIN   game_sessions gs ON gr.session_id = gs.session_id
        WHERE  gs.user_id = u.user_id) AS highest_score
FROM   users u
ORDER BY highest_score DESC NULLS LAST;


-- =============================================================
-- SECTION M: Window Functions
--            ROW_NUMBER, RANK, DENSE_RANK, LAG, LEAD,
--            Moving Average
-- =============================================================

-- M.1 ROW_NUMBER – assign a unique sequential number by score (highest first)
SELECT result_id,
       session_id,
       score,
       ROW_NUMBER() OVER (ORDER BY score DESC) AS row_num
FROM   game_results;

-- M.2 RANK – same rank for tied scores, then skips numbers
--     e.g., scores 1980, 1750, 1750 → ranks 1, 2, 2 then next is 4
SELECT result_id,
       session_id,
       score,
       RANK() OVER (ORDER BY score DESC) AS score_rank
FROM   game_results;

-- M.3 DENSE_RANK – same rank for ties but NO gaps
--     e.g., scores 1980, 1750, 1750 → ranks 1, 2, 2 then next is 3
SELECT result_id,
       session_id,
       score,
       DENSE_RANK() OVER (ORDER BY score DESC) AS dense_rank
FROM   game_results;

-- M.4 RANK per user – reset the rank for each player separately
--     PARTITION BY user_id means each player gets their own ranking.
SELECT gs.user_id,
       gr.session_id,
       gr.score,
       RANK() OVER (PARTITION BY gs.user_id ORDER BY gr.score DESC) AS rank_per_user
FROM   game_results  gr
JOIN   game_sessions gs ON gr.session_id = gs.session_id;

-- M.5 LAG – show the previous session's score for each user
--     LAG looks at the row BEFORE the current row in the window.
SELECT gs.user_id,
       gr.session_id,
       gs.started_at,
       gr.score                                                              AS current_score,
       LAG(gr.score) OVER (PARTITION BY gs.user_id ORDER BY gs.started_at)  AS previous_score
FROM   game_results  gr
JOIN   game_sessions gs ON gr.session_id = gs.session_id
ORDER BY gs.user_id, gs.started_at;

-- M.6 LEAD – show the NEXT session's score for each user
--     LEAD looks at the row AFTER the current row in the window.
SELECT gs.user_id,
       gr.session_id,
       gs.started_at,
       gr.score                                                               AS current_score,
       LEAD(gr.score) OVER (PARTITION BY gs.user_id ORDER BY gs.started_at)  AS next_score
FROM   game_results  gr
JOIN   game_sessions gs ON gr.session_id = gs.session_id
ORDER BY gs.user_id, gs.started_at;

-- M.7 Moving Average – 2-session rolling average of score per user
--     ROWS BETWEEN 1 PRECEDING AND CURRENT ROW means: average this row
--     and the one before it, within each user's partition.
SELECT gs.user_id,
       gr.session_id,
       gs.started_at,
       gr.score,
       ROUND(
           AVG(gr.score) OVER (
               PARTITION BY gs.user_id
               ORDER BY gs.started_at
               ROWS BETWEEN 1 PRECEDING AND CURRENT ROW
           ), 2
       ) AS moving_avg_score
FROM   game_results  gr
JOIN   game_sessions gs ON gr.session_id = gs.session_id
ORDER BY gs.user_id, gs.started_at;


-- =============================================================
-- SECTION N: Validation & Error Handling (Constraint Violations)
--            Demonstrates PostgreSQL exception handling for
--            CHECK, UNIQUE, and FOREIGN KEY constraints.
-- =============================================================

-- N.1 Catching a UNIQUE constraint violation (Duplicate Email)
DO $$
BEGIN
    INSERT INTO users (name, email, password_hash)
    VALUES ('Duplicate Test', 'aarav.sharma@email.com', 'hash_test');
EXCEPTION WHEN unique_violation THEN
    RAISE NOTICE 'Handled unique_violation: User with this email already exists.';
END $$;

-- N.2 Catching a CHECK constraint violation (Negative Score)
DO $$
BEGIN
    INSERT INTO game_results (session_id, score, groundwater_before, groundwater_after, water_saved)
    VALUES (1, -500, 40, 60, 100);
EXCEPTION WHEN check_violation THEN
    RAISE NOTICE 'Handled check_violation: Score must be non-negative (CHECK score >= 0).';
END $$;

-- N.3 Catching a FOREIGN KEY constraint violation (Non-existent User)
DO $$
BEGIN
    INSERT INTO game_sessions (user_id, scenario_id)
    VALUES (99999, 1);
EXCEPTION WHEN foreign_key_violation THEN
    RAISE NOTICE 'Handled foreign_key_violation: Referenced user_id does not exist.';
END $$;

