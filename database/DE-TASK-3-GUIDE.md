# AquaGuardian — Data Engineering Documentation
## DE Task 3 — Complete Reference Guide

**Project:** AquaGuardian – Development of an Educational Game on Groundwater Conservation  
**Component:** Data Engineering (DE)  
**Author:** Yash Kumar  
**Audience:** Second-Year B.Tech CSE — College Project Evaluation

---

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [Database Objectives](#2-database-objectives)
3. [ER Diagram and Relationships](#3-er-diagram-and-relationships)
4. [Table Descriptions](#4-table-descriptions)
5. [Keys and Constraints](#5-keys-and-constraints)
6. [Schema Explanation](#6-schema-explanation)
7. [Seed Data Explanation](#7-seed-data-explanation)
8. [DDL / DML / TCL / DCL Coverage](#8-ddl--dml--tcl--dcl-coverage)
9. [Retrieval Queries](#9-retrieval-queries)
10. [SQL Functions](#10-sql-functions)
11. [GROUP BY, HAVING and CASE](#11-group-by-having-and-case)
12. [Joins and Subqueries](#12-joins-and-subqueries)
13. [Window Functions](#13-window-functions)
14. [Normalization — 1NF to BCNF](#14-normalization--1nf-to-bcnf)
15. [ACID and Transaction Handling](#15-acid-and-transaction-handling)
16. [Analytics Queries and Interpretations](#16-analytics-queries-and-interpretations)
17. [Execution Instructions](#17-execution-instructions)
18. [Validation Results](#18-validation-results)
19. [DE Syllabus Coverage Checklist](#19-de-syllabus-coverage-checklist)

---

## 1. Project Overview

AquaGuardian is a web-based and mobile-based educational game designed to teach players about groundwater conservation and management. Players choose scenarios — such as a village drought or a farmland irrigation crisis — and make decisions that affect the groundwater level of a simulated aquifer. The game tracks each session, records outcomes, and ranks players based on how much groundwater they conserve.

The Data Engineering component designs and implements the relational database that stores all game data, writes queries to support reporting and analytics, and documents the database design according to academic standards.

---

## 2. Database Objectives

The AquaGuardian database must:

- Store registered player accounts securely.
- Record all available game scenarios and their difficulty levels.
- Track each gameplay session — when it started, which user played, which scenario was chosen, and whether the session was completed, abandoned, or is still ongoing.
- Record the outcome of every completed session — the score, groundwater levels before and after, and water saved.
- Support reporting queries such as leaderboards, completion statistics, groundwater recovery analysis, and player progress over time.
- Demonstrate all SQL concepts required by the Data Engineering course syllabus.

---

## 3. ER Diagram and Relationships

```
users ||--o{ game_sessions : "plays"
scenarios ||--o{ game_sessions : "is used in"
game_sessions ||--o| game_results : "produces"
```

### Entities and Their Roles

| Entity | Role |
|---|---|
| `users` | Registered players of the game |
| `scenarios` | Conservation missions available to play |
| `game_sessions` | One play-through — links a user to a scenario |
| `game_results` | Outcome data for a completed session |

### Relationships

| Relationship | Type | Explanation |
|---|---|---|
| `users` → `game_sessions` | One-to-Many | One user can start many sessions |
| `scenarios` → `game_sessions` | One-to-Many | One scenario can appear in many sessions |
| `game_sessions` → `game_results` | One-to-Zero-or-One | A session either has one result or none (ongoing/abandoned) |

The One-to-Zero-or-One relationship between `game_sessions` and `game_results` is enforced by a `UNIQUE` constraint on `game_results.session_id`. This guarantees that at most one result row can exist for any given session.

---

## 4. Table Descriptions

### 4.1 `users`

Stores one row per registered player account.

| Column | Type | Description |
|---|---|---|
| `user_id` | SERIAL (auto-increment INTEGER) | Primary key |
| `name` | VARCHAR(100) | Player display name |
| `email` | VARCHAR(255) | Login email — unique across all users |
| `password_hash` | VARCHAR(255) | Hashed password (never plain text) |
| `created_at` | TIMESTAMP | Account creation time, defaults to current time |

### 4.2 `scenarios`

Stores one row per game scenario, defined once and reused across many sessions.

| Column | Type | Description |
|---|---|---|
| `scenario_id` | SERIAL | Primary key |
| `scenario_name` | VARCHAR(150) | Short scenario name shown in the game menu |
| `description` | TEXT | Story and goal description for the scenario |
| `difficulty` | VARCHAR(20) | Allowed values: `'Easy'`, `'Medium'`, `'Hard'` |

### 4.3 `game_sessions`

Stores one row per play-through. Connects a user to a scenario and tracks timing and status.

| Column | Type | Description |
|---|---|---|
| `session_id` | SERIAL | Primary key |
| `user_id` | INTEGER | Foreign key → `users.user_id` |
| `scenario_id` | INTEGER | Foreign key → `scenarios.scenario_id` |
| `started_at` | TIMESTAMP | Session start time, defaults to current time |
| `ended_at` | TIMESTAMP | Session end time — NULL if the session is still running |
| `status` | VARCHAR(20) | Allowed: `'ongoing'`, `'completed'`, `'abandoned'` |

### 4.4 `game_results`

Stores one row per completed session. The `session_id` column is `UNIQUE`, enforcing at most one result per session.

| Column | Type | Description |
|---|---|---|
| `result_id` | SERIAL | Primary key |
| `session_id` | INTEGER UNIQUE | Foreign key → `game_sessions.session_id` |
| `score` | INTEGER ≥ 0 | Player's final score |
| `groundwater_before` | INTEGER 0–100 | Aquifer percentage before play |
| `groundwater_after` | INTEGER 0–100 | Aquifer percentage after play |
| `water_saved` | INTEGER ≥ 0 | Units of water conserved during the session |
| `completed_at` | TIMESTAMP | When the result was recorded |

---

## 5. Keys and Constraints

### Primary Keys

| Table | Primary Key | Type |
|---|---|---|
| `users` | `user_id` | SERIAL (auto-increment) |
| `scenarios` | `scenario_id` | SERIAL |
| `game_sessions` | `session_id` | SERIAL |
| `game_results` | `result_id` | SERIAL |

### Candidate Keys

A candidate key is any minimal set of attributes that uniquely identifies a row.

| Table | Candidate Keys |
|---|---|
| `users` | `{user_id}` and `{email}` |
| `scenarios` | `{scenario_id}` |
| `game_sessions` | `{session_id}` |
| `game_results` | `{result_id}` and `{session_id}` |

`users.email` is a candidate key because it is both UNIQUE and NOT NULL. It is a secondary candidate key; `user_id` is chosen as the primary key because integers are more efficient for joins.

`game_results.session_id` is a candidate key because it is `UNIQUE NOT NULL`. This also enforces the one-to-one constraint between sessions and results.

### Super Keys

A super key is any set of attributes that uniquely identifies a row (possibly non-minimal). Every superset of a candidate key is a super key. For example, in `users`:
- `{user_id}` — candidate key (minimal)
- `{email}` — candidate key (minimal)
- `{user_id, email}` — super key (not minimal)
- `{user_id, name}` — super key (not minimal)

### Foreign Keys

| Table | Column | References |
|---|---|---|
| `game_sessions` | `user_id` | `users(user_id)` |
| `game_sessions` | `scenario_id` | `scenarios(scenario_id)` |
| `game_results` | `session_id` | `game_sessions(session_id)` |

All foreign keys enforce referential integrity: you cannot insert a session for a user who does not exist, and you cannot insert a result for a session that does not exist.

### Unique Constraints

| Table | Column | Purpose |
|---|---|---|
| `users` | `email` | No two accounts can share an email address |
| `game_results` | `session_id` | One session can have at most one result row |

### NOT NULL Constraints

Every column in all four tables is NOT NULL except `game_sessions.ended_at`, which is NULL while a session is still running and filled in when the session ends.

### CHECK Constraints

| Table | Column | Rule |
|---|---|---|
| `scenarios` | `difficulty` | Must be `'Easy'`, `'Medium'`, or `'Hard'` |
| `game_sessions` | `status` | Must be `'ongoing'`, `'completed'`, or `'abandoned'` |
| `game_results` | `score` | Must be `>= 0` |
| `game_results` | `groundwater_before` | Must be between `0` and `100` |
| `game_results` | `groundwater_after` | Must be between `0` and `100` |
| `game_results` | `water_saved` | Must be `>= 0` |

---

## 6. Schema Explanation

The schema uses `SERIAL` as a shorthand for `INTEGER NOT NULL DEFAULT nextval(sequence)`. PostgreSQL automatically creates a sequence object that generates unique integer IDs.

Timestamps use `TIMESTAMP WITHOUT TIME ZONE` (the PostgreSQL default when you write `TIMESTAMP`). All timestamps in the seed data are in IST for consistency.

The `password_hash` column stores only the hash of a user's password — never the plain text. In a production system this would use a library like bcrypt. The seed data uses readable placeholder hashes for demonstration.

Foreign key constraints prevent orphan records — if you try to delete a user who has sessions, PostgreSQL will reject it unless you explicitly cascade or nullify dependent rows.

---

## 7. Seed Data Explanation

The seed data (`database/seed.sql`) populates all four tables with realistic demo records.

### Users (6 rows)

| user_id | name | Email |
|---|---|---|
| 1 | Aarav Sharma | aarav.sharma@email.com |
| 2 | Priya Patel | priya.patel@email.com |
| 3 | Rohan Mehta | rohan.mehta@email.com |
| 4 | Sneha Joshi | sneha.joshi@email.com |
| 5 | Karan Verma | karan.verma@email.com |
| 6 | Ananya Singh | ananya.singh@email.com |

### Scenarios (4 rows)

| scenario_id | Name | Difficulty |
|---|---|---|
| 1 | Village Drought | Easy |
| 2 | Farmland Crisis | Medium |
| 3 | Industrial Overuse | Hard |
| 4 | Monsoon Recharge | Easy |

### Game Sessions (10 rows)

Sessions 1–9 have `ended_at` values. Session 10 (Ananya, Monsoon Recharge) is currently `'ongoing'` with a NULL `ended_at`. Session 5 (Rohan, Farmland Crisis) was `'abandoned'` after 10 minutes.

### Game Results (8 rows)

Results exist only for completed sessions (sessions 1, 2, 3, 4, 6, 7, 8, 9). Sessions 5 and 10 have no result rows — session 5 was abandoned, session 10 is ongoing.

---

## 8. DDL / DML / TCL / DCL Coverage

### DDL — Data Definition Language

DDL defines the structure of the database. These commands change *what* exists, not *what data* is in it.

```sql
-- CREATE TABLE — builds a new table
CREATE TABLE users (
    user_id    SERIAL PRIMARY KEY,
    name       VARCHAR(100) NOT NULL,
    email      VARCHAR(255) NOT NULL UNIQUE,
    ...
);

-- ALTER TABLE — add a column to an existing table
ALTER TABLE users ADD COLUMN IF NOT EXISTS city VARCHAR(100) DEFAULT 'Unknown';

-- ALTER TABLE — rename a column
ALTER TABLE users RENAME COLUMN city TO home_city;

-- ALTER TABLE — change a column's data type
ALTER TABLE users ALTER COLUMN home_city TYPE TEXT;

-- ALTER TABLE — drop a column
ALTER TABLE users DROP COLUMN IF EXISTS home_city;

-- TRUNCATE — quickly delete all rows without logging individual deletions
TRUNCATE TABLE demo_log;

-- DROP TABLE — permanently remove a table and all its data
DROP TABLE IF EXISTS demo_log;
```

> `DROP` removes the table completely. `TRUNCATE` removes only the rows — the table structure survives.

### DML — Data Manipulation Language

DML works with the actual data stored in the tables.

```sql
-- INSERT
INSERT INTO users (name, email, password_hash)
VALUES ('Test User', 'test@email.com', 'hash_test');

-- SELECT (see Section 9)

-- UPDATE
UPDATE users SET name = 'Test Player' WHERE email = 'test@email.com';

-- DELETE
DELETE FROM users WHERE email = 'test@email.com';
```

### TCL — Transaction Control Language

TCL controls groups of DML statements that must all succeed or all fail together. See Section 15 for the full ACID explanation and Section F.1 in `analytics.sql` for the live transaction demonstration.

```sql
BEGIN;
    INSERT INTO game_sessions ...;
    SAVEPOINT after_session;
    INSERT INTO game_results ...;
ROLLBACK; -- or COMMIT
```

### DCL — Data Control Language

DCL controls database user permissions. These require a PostgreSQL superuser to execute.

```sql
-- Create a read-only role for reporting tools
CREATE ROLE readonly_user;
GRANT SELECT ON users, scenarios, game_sessions, game_results TO readonly_user;

-- Remove that permission
REVOKE SELECT ON users FROM readonly_user;

-- Grant full DML to the application account
GRANT SELECT, INSERT, UPDATE, DELETE
    ON users, scenarios, game_sessions, game_results
    TO game_app_user;
```

> DCL examples are provided for educational reference. The roles `readonly_user` and `game_app_user` must be created in PostgreSQL first with `CREATE ROLE` before the GRANT/REVOKE statements can run.

---

## 9. Retrieval Queries

### SELECT with DISTINCT

```sql
-- List all distinct difficulty levels in the game
SELECT DISTINCT difficulty FROM scenarios ORDER BY difficulty;
```

Result: `Easy`, `Hard`, `Medium`

### WHERE with IN, LIKE, ILIKE

```sql
-- Sessions that are either ongoing or abandoned
SELECT session_id, status, started_at
FROM game_sessions
WHERE status IN ('ongoing', 'abandoned');

-- Users whose name starts with the letter A
SELECT name, email FROM users WHERE name LIKE 'A%';

-- Scenarios with 'water' anywhere in the description (case-insensitive)
SELECT scenario_name FROM scenarios WHERE description ILIKE '%water%';
```

### ORDER BY, LIMIT, OFFSET

```sql
-- Top 3 scores ever recorded
SELECT session_id, score FROM game_results ORDER BY score DESC LIMIT 3;

-- Page 2 of scores (rows 4–6)
SELECT session_id, score FROM game_results ORDER BY score DESC LIMIT 3 OFFSET 3;
```

### EXISTS and NOT EXISTS

```sql
-- Users who have at least one session (EXISTS)
SELECT name FROM users u
WHERE EXISTS (
    SELECT 1 FROM game_sessions gs WHERE gs.user_id = u.user_id
);

-- Users who have never completed a session (NOT EXISTS)
SELECT name FROM users u
WHERE NOT EXISTS (
    SELECT 1 FROM game_sessions gs
    WHERE gs.user_id = u.user_id AND gs.status = 'completed'
);
```

---

## 10. SQL Functions

### Numeric Functions

| Function | Purpose | Example |
|---|---|---|
| `ROUND(x, n)` | Round to n decimal places | `ROUND(AVG(score), 2)` |
| `ABS(x)` | Absolute value | `ABS(-15)` → 15 |
| `NULLIF(a, b)` | Returns NULL if a = b | `NULLIF(COUNT(*), 0)` prevents division by zero |

### String Functions

Demonstrated in query **A.8** in `analytics.sql`:

```sql
SELECT
    UPPER(u.name)                     AS name_upper,
    LOWER(u.email)                    AS email_lower,
    LENGTH(u.name)                    AS name_length,
    CONCAT(u.name, ' <', u.email, '>')  AS display_label
FROM users u;
```

| Function | Result for `Aarav Sharma` |
|---|---|
| `UPPER(name)` | `AARAV SHARMA` |
| `LOWER(email)` | `aarav.sharma@email.com` |
| `LENGTH(name)` | `12` |
| `CONCAT(name, ' <', email, '>')` | `Aarav Sharma <aarav.sharma@email.com>` |

### Date and Time Functions

Demonstrated in queries **E.2**, **E.7**, and **E.8** in `analytics.sql`:

| Function | Purpose | Example |
|---|---|---|
| `NOW()` | Current timestamp | `TO_CHAR(NOW(), 'YYYY-MM-DD')` |
| `DATE_TRUNC('month', ts)` | Truncate to start of month | Groups Feb-05 and Feb-20 into Feb-01 |
| `TO_CHAR(ts, fmt)` | Format timestamp as string | `'FMMonth YYYY'` → `February 2024` |
| `EXTRACT(YEAR FROM ts)` | Extract year as number | `EXTRACT(YEAR FROM started_at)` → `2024` |
| `EXTRACT(MONTH FROM ts)` | Extract month number | `2` for February |
| `EXTRACT(DOW FROM ts)` | Day of week (0=Sunday) | `4` for Thursday |
| `ts2 - ts1` | Interval between two timestamps | `ended_at - started_at` → `00:25:00` |
| `EXTRACT(EPOCH FROM interval)` | Interval as total seconds | `÷ 60.0` gives minutes |

### NULL Handling Functions

```sql
-- COALESCE: return the first non-NULL value
COALESCE(SUM(gr.water_saved), 0)        -- shows 0 instead of NULL
COALESCE(ended_at, NOW())               -- uses NOW() if ended_at is NULL

-- NULLIF: return NULL if two expressions are equal
NULLIF(COUNT(gs.session_id), 0)         -- prevents division by zero in %
```

---

## 11. GROUP BY, HAVING and CASE

### GROUP BY

`GROUP BY` collapses multiple rows into summary groups, which aggregate functions then operate on.

```sql
-- Sessions grouped by status
SELECT status, COUNT(*) AS count
FROM game_sessions
GROUP BY status;
```

| status | count |
|---|---|
| completed | 8 |
| ongoing | 1 |
| abandoned | 1 |

### HAVING

`HAVING` filters groups **after** aggregation. It is not the same as `WHERE`, which filters individual rows **before** grouping.

```sql
-- Only scenarios where average water saved >= 150 (query B.7)
SELECT s.scenario_name, ROUND(AVG(gr.water_saved), 2) AS avg_saved
FROM scenarios s
JOIN game_sessions gs ON s.scenario_id = gs.scenario_id
JOIN game_results  gr ON gs.session_id = gr.session_id
GROUP BY s.scenario_id, s.scenario_name, s.difficulty
HAVING AVG(gr.water_saved) >= 150
ORDER BY avg_saved DESC;
```

| scenario_name | avg_saved |
|---|---|
| Industrial Overuse | 330.00 |
| Monsoon Recharge | 200.00 |
| Farmland Crisis | 167.50 |

Village Drought averages only 113.33 units saved, so it is filtered out by `HAVING`.

### CASE Statement

`CASE` works like an if-else inside a query. It returns a value based on evaluated conditions.

```sql
-- Classify each session result by recovery level (query D.3)
CASE
    WHEN (gr.groundwater_after - gr.groundwater_before) >= 25 THEN 'Substantial Recovery'
    WHEN (gr.groundwater_after - gr.groundwater_before) >= 15 THEN 'Moderate Recovery'
    ELSE                                                            'Minimal Recovery'
END AS recovery_category
```

`CASE` can also be used inside aggregate functions for conditional counting:

```sql
-- Count completed sessions separately without a WHERE clause
COUNT(CASE WHEN gs.status = 'completed' THEN 1 END) AS completed_count
```

---

## 12. Joins and Subqueries

### INNER JOIN

Returns only the rows where the join condition matches in **both** tables. If a user has no sessions, they will not appear.

```sql
-- Leaderboard: only users who have completed at least one game (query C.1)
SELECT u.name, SUM(gr.score) AS total_score
FROM users u
INNER JOIN game_sessions gs ON u.user_id    = gs.user_id
INNER JOIN game_results  gr ON gs.session_id = gr.session_id
GROUP BY u.user_id, u.name
ORDER BY total_score DESC;
```

### LEFT JOIN

Returns **all** rows from the left table plus matching rows from the right table. Where there is no match, the right-side columns are NULL.

```sql
-- All users, including Ananya who has no completed game result (query A.3)
SELECT u.name, COALESCE(ROUND(AVG(gr.score), 2), 0.00) AS avg_score
FROM users u
LEFT JOIN game_sessions gs ON u.user_id    = gs.user_id
LEFT JOIN game_results  gr ON gs.session_id = gr.session_id
GROUP BY u.user_id, u.name;
```

Ananya Singh has an ongoing session with no result, so her `avg_score` returns `NULL` from `AVG`. `COALESCE` converts it to `0.00`.

### Subqueries

**WHERE-clause (non-correlated) subquery:** The inner query runs once and its result is used by the outer query.

```sql
-- Find the highest score ever, then find all sessions matching it
SELECT session_id, score FROM game_results
WHERE score = (SELECT MAX(score) FROM game_results);
```

**FROM-clause (derived table) subquery:** The inner query produces a temporary result set that the outer query treats as a table.

```sql
-- Query A.7: players with cumulative score above 1500
SELECT sub.player_name, sub.total_score
FROM (
    SELECT u.name AS player_name, SUM(gr.score) AS total_score
    FROM users u
    JOIN game_sessions gs ON u.user_id    = gs.user_id
    JOIN game_results  gr ON gs.session_id = gr.session_id
    GROUP BY u.user_id, u.name
) AS sub
WHERE sub.total_score > 1500
ORDER BY sub.total_score DESC;
```

**Correlated subquery:** The inner query references the outer query's current row. It re-executes once per outer row.

```sql
-- Query E.4: users who have never finished a session
SELECT u.name FROM users u
WHERE NOT EXISTS (
    SELECT 1 FROM game_sessions gs
    WHERE gs.user_id = u.user_id AND gs.status = 'completed'
);
```

**Scalar subquery in SELECT:** Returns a single value used as a column in the outer query.

```sql
-- Query E.1: percentage calculation using total row count
ROUND(100.0 * COUNT(*) / (SELECT COUNT(*) FROM game_sessions), 2) AS percentage
```

---

## 13. Window Functions

A window function computes a value for each row relative to a **window** of related rows — without collapsing rows into one like `GROUP BY` does. The `OVER()` clause defines the window.

### ROW_NUMBER, RANK, DENSE_RANK

Demonstrated together in query **C.1**:

```sql
ROW_NUMBER()  OVER (ORDER BY SUM(gr.score) DESC)  -- always 1,2,3,4,5
RANK()        OVER (ORDER BY SUM(gr.score) DESC)  -- 1,2,2,4 on a tie
DENSE_RANK()  OVER (ORDER BY SUM(gr.score) DESC)  -- 1,2,2,3 on a tie
```

**Tie-handling comparison** (hypothetical tied scores at rank 2):

| Player | Score | ROW_NUMBER | RANK | DENSE_RANK |
|---|---|---|---|---|
| Sneha | 2850 | 1 | 1 | 1 |
| Priya | 2530 | 2 | 2 | 2 |
| Aarav | 2530 | 3 | 2 | 2 |
| Karan | 1100 | 4 | 4 | 3 |

(In the actual seed data all scores are unique, so RANK and DENSE_RANK produce the same values.)

### PARTITION BY

`PARTITION BY` resets the window for each distinct group, like a GROUP BY only for the window function.

```sql
-- Score progression per user: each user's history is independent
LAG(gr.score) OVER (PARTITION BY gs.user_id ORDER BY gs.started_at)
```

Without `PARTITION BY`, the LAG would compare rows across all users. With it, it only looks back within that user's own sessions.

### LAG and LEAD

Demonstrated in query **E.6**:

```sql
LAG(gr.score)  OVER (PARTITION BY gs.user_id ORDER BY gs.started_at) AS previous_score,
LEAD(gr.score) OVER (PARTITION BY gs.user_id ORDER BY gs.started_at) AS next_score
```

- `LAG(col)` → value from the **previous** row in the partition
- `LEAD(col)` → value from the **next** row in the partition
- First rows have `NULL` for `LAG`; last rows have `NULL` for `LEAD`

**Actual output for Aarav Sharma:**

| session | scenario | current_score | previous_score | score_growth | next_score |
|---|---|---|---|---|---|
| 1 | Village Drought | 850 | NULL | NULL | 1200 |
| 2 | Farmland Crisis | 1200 | 850 | +350 | NULL |

Aarav improved by 350 points between his two sessions.

---

## 14. Normalization — 1NF to BCNF

Normalization is the process of organizing a database to reduce redundancy and ensure data integrity. We analyze the AquaGuardian schema using the actual tables.

### Functional Dependencies

A functional dependency `A → B` means knowing the value of A is enough to determine the value of B.

**`users` table:**
- `user_id → name, email, password_hash, created_at` (PK determines all)
- `email → user_id, name, password_hash, created_at` (email is a candidate key)

**`scenarios` table:**
- `scenario_id → scenario_name, description, difficulty`

**`game_sessions` table:**
- `session_id → user_id, scenario_id, started_at, ended_at, status`

**`game_results` table:**
- `result_id → session_id, score, groundwater_before, groundwater_after, water_saved, completed_at`
- `session_id → result_id, score, groundwater_before, groundwater_after, water_saved, completed_at`

### First Normal Form (1NF)

**Rule:** Every column must hold atomic (indivisible) values. No repeating groups. Every row must be uniquely identifiable.

**AquaGuardian satisfies 1NF because:**
- Every column holds a single value. For example, `difficulty` holds one string (`'Easy'`), not a list.
- There are no repeating columns (no `score1`, `score2`, `score3` pattern).
- Every table has a primary key: `user_id`, `scenario_id`, `session_id`, `result_id`.

### Second Normal Form (2NF)

**Rule:** The table must be in 1NF, and every non-key column must depend on the **whole** primary key — not just part of it.

2NF is only relevant for tables with **composite primary keys**. All four AquaGuardian tables use **single-column** SERIAL primary keys, so there can be no partial dependencies.

**AquaGuardian satisfies 2NF** automatically because none of the tables have composite primary keys.

However, consider how the design avoids partial dependency problems. If we had stored `scenario_name` or `difficulty` inside `game_sessions`, those attributes would depend only on `scenario_id`, not on `session_id`. This would violate 2NF. By separating scenario data into its own table and referencing it via foreign key, we eliminate that redundancy.

### Third Normal Form (3NF)

**Rule:** The table must be in 2NF, and no non-key column may depend on another non-key column (no transitive dependencies).

A transitive dependency is `PK → A → B`, where B depends on A rather than directly on the primary key.

**Analysis of `game_results`:**

Suppose `score` somehow determined `groundwater_after`. That would be a transitive dependency: `result_id → score → groundwater_after`. But in this schema, `groundwater_after` depends directly on the specific game session outcome, not on the score. The dependency is `result_id → groundwater_after` directly. No transitive dependency exists.

**Analysis of `game_sessions`:**

Does `status` transitively depend on anything other than `session_id`? No. The status of a session is a property of that specific session row, not derivable from `user_id` or `scenario_id` alone.

**AquaGuardian satisfies 3NF** because there are no transitive dependencies in any of the four tables.

### Boyce-Codd Normal Form (BCNF)

**Rule:** For every non-trivial functional dependency `X → Y`, X must be a superkey (a set of columns that uniquely identifies every row).

BCNF is a stricter version of 3NF. It is violated when a non-superkey column determines another column.

**Analysis of `users`:**

Functional dependencies:
- `user_id → name, email, password_hash, created_at` — `user_id` is PK (superkey) ✓
- `email → user_id, name, password_hash, created_at` — `email` is a candidate key (superkey) ✓

Both determinants (`user_id` and `email`) are superkeys, so `users` satisfies BCNF.

**Analysis of `scenarios`:**

The only functional dependency is `scenario_id → scenario_name, description, difficulty`. The determinant `scenario_id` is the primary key (superkey). BCNF is satisfied.

**Analysis of `game_sessions`:**

The only functional dependency is `session_id → user_id, scenario_id, started_at, ended_at, status`. The determinant is the primary key. BCNF is satisfied.

**Analysis of `game_results`:**

Functional dependencies:
- `result_id → session_id, score, groundwater_before, groundwater_after, water_saved, completed_at` — `result_id` is PK ✓
- `session_id → result_id, score, groundwater_before, groundwater_after, water_saved, completed_at` — `session_id` is a candidate key ✓

Both determinants are superkeys. `game_results` satisfies BCNF.

**Conclusion:** All four AquaGuardian tables satisfy 1NF, 2NF, 3NF, and BCNF.

---

## 15. ACID and Transaction Handling

### What is a Transaction?

A transaction is a group of SQL statements that are treated as one single unit of work. Either all statements succeed and are saved, or none of them are saved. This prevents the database from being left in a half-updated, inconsistent state.

### ACID Properties

**Atomicity**
> All operations in a transaction succeed together, or none of them are applied.

In AquaGuardian, inserting a game session and its result must be atomic. If the session insert succeeds but the result insert fails (for example, due to a CHECK constraint violation), the entire transaction rolls back and neither row is saved.

**Consistency**
> A transaction takes the database from one valid state to another valid state. All constraints, foreign keys, and check constraints must hold at the end of the transaction.

The `game_results.session_id` foreign key constraint ensures that a result can only be inserted for a session that actually exists. If the session insert were rolled back, any attempt to insert a result referencing it would violate the FK constraint.

**Isolation**
> Concurrent transactions should not interfere with each other. The intermediate state of a transaction is not visible to other transactions.

PostgreSQL implements isolation using Multi-Version Concurrency Control (MVCC). The default isolation level is `READ COMMITTED`, which means a transaction sees only data that has been committed by other transactions. Changes inside an open `BEGIN...COMMIT` block are invisible to other sessions until `COMMIT` is executed.

**Durability**
> Once a transaction is committed, the changes are permanently written to disk even if the database server crashes immediately afterward.

PostgreSQL achieves this via a Write-Ahead Log (WAL). Every committed change is written to the WAL before the response is sent to the client.

### Transaction Anomalies

| Anomaly | Description | PostgreSQL Default |
|---|---|---|
| **Dirty Read** | A transaction reads data written by another transaction that has not yet committed | Prevented at READ COMMITTED and above |
| **Non-Repeatable Read** | A transaction reads the same row twice and gets different values because another transaction committed in between | Prevented at REPEATABLE READ and above |
| **Phantom Read** | A transaction re-executes a query and finds new rows that another transaction inserted and committed | Prevented at SERIALIZABLE level |
| **Lost Update** | Two transactions both read a value, update it, and write it back — the first update is overwritten | Prevented by PostgreSQL row-level locking or SERIALIZABLE |

### TCL Commands

| Command | What it does |
|---|---|
| `BEGIN` | Start a new transaction block |
| `SAVEPOINT name` | Mark a named checkpoint inside a transaction |
| `ROLLBACK TO SAVEPOINT name` | Undo everything since that savepoint, without ending the transaction |
| `ROLLBACK` | Undo the entire transaction back to the `BEGIN` |
| `COMMIT` | Permanently save all changes made since `BEGIN` |

### Transaction Example (from analytics.sql, Section F.1)

```sql
BEGIN;

    INSERT INTO game_sessions (user_id, scenario_id, started_at, ended_at, status)
    VALUES (6, 2, '2024-05-01 09:00:00', '2024-05-01 09:45:00', 'completed');

    SAVEPOINT after_session;

    INSERT INTO game_results
        (session_id, score, groundwater_before, groundwater_after, water_saved, completed_at)
    VALUES
        (currval('game_sessions_session_id_seq'), 1300, 28, 55, 240, '2024-05-01 09:45:00');

ROLLBACK; -- changed to COMMIT in production use
```

**Why `currval()`?** After an `INSERT INTO game_sessions`, PostgreSQL advances the SERIAL sequence. `currval('game_sessions_session_id_seq')` retrieves that exact value within the same session — no ambiguous subquery required, and it is safe to run repeatedly because sequences always advance monotonically.

**Why `ROLLBACK` in the demo?** The demo rolls back so the seed data remains clean and the file can be re-executed without producing duplicate or conflicting IDs. In a real application, `COMMIT` would be used here.

---

## 16. Analytics Queries and Interpretations

All queries in this section are located in `database/analytics.sql` and have been executed successfully against the `aquaguardian` database loaded with `schema.sql` and `seed.sql`. The query ID shown (e.g., A.1) matches the label in `analytics.sql`.

### Section A: User Analytics

| Query | Business Question | Key Concepts |
|---|---|---|
| A.1 | How active is each user? (all sessions) | LEFT JOIN, COUNT, GROUP BY |
| A.2 | How many missions did each player complete? | Conditional COUNT(CASE …) |
| A.3 | Who scores the highest on average? | AVG, ROUND, COALESCE |
| A.4 | Who conserves the most water? | SUM, COALESCE |
| A.5 | Comprehensive player ranking | DENSE_RANK, all aggregates |
| A.6 | Which players have finished more than 1 mission? | HAVING COUNT > 1 |
| A.7 | Which players have a total score above 1500? | FROM-subquery (derived table) |
| A.8 | Display player names and emails in formatted labels | UPPER, LOWER, LENGTH, CONCAT |

**Key finding:** Sneha Joshi leads the leaderboard with 2,850 total points and 480 water units saved across 2 completed missions.

### Section B: Scenario Analytics

| Query | Business Question | Key Concepts |
|---|---|---|
| B.1 | Which scenarios are played most? | LEFT JOIN on scenarios |
| B.2 | Which scenarios produce the best scores? | AVG(score) per scenario |
| B.3 | Which scenario restores the aquifer most? | AVG(after - before) |
| B.4 | What is the score range per scenario? | MIN, MAX, AVG together |
| B.5 | Single most popular scenario | ORDER BY DESC LIMIT 1 |
| B.6 | Completion vs abandonment stats per scenario | CASE-pivot, NULLIF |
| B.7 | High water-saving scenarios only | HAVING AVG(water_saved) >= 150 |

**Key finding:** Industrial Overuse (Hard) produces the highest average score (1865) and second-highest average groundwater recovery (+28.5%). Village Drought (Easy) has a 100% completion rate.

### Section C: Leaderboard

| Query | Business Question | Key Concepts |
|---|---|---|
| C.1 | Official leaderboard with ranking | ROW_NUMBER, RANK, DENSE_RANK |

The leaderboard uses INNER JOIN so only players with at least one verified result appear. `ROW_NUMBER` is unique per row; `RANK` and `DENSE_RANK` differ only when there are score ties.

### Section D: Groundwater Analytics

| Query | Business Question | Key Concepts |
|---|---|---|
| D.1 | What is the game-wide total environmental impact? | COUNT, SUM, AVG, MIN, MAX |
| D.2 | Average aquifer recovery across all missions? | AVG before/after/improvement |
| D.3 | Which sessions had the best groundwater recovery? | CASE classification |
| D.4 | Individual ecological impact per player | COALESCE, multi-aggregates |

**Key finding:** Across all 8 completed missions, players saved 1,535 units of water and improved average aquifer levels from 32.88% to 53.75% (+20.88 percentage points average recovery).

### Section E: Session & Temporal Analytics

| Query | Business Question | Key Concepts |
|---|---|---|
| E.1 | Session status distribution | Scalar subquery for % |
| E.2 | How long did each mission take? | EXTRACT(EPOCH …), CASE for NULL |
| E.3 | What happened recently? | ORDER BY timestamp DESC, LIMIT 5 |
| E.4 | Who has never completed a mission? | Correlated NOT EXISTS |
| E.5 | Completion rate per scenario | CASE-pivot, NULLIF |
| E.6 | Did players improve over time? | LAG, LEAD, PARTITION BY user_id |
| E.7 | Monthly session and water-saving trends | DATE_TRUNC, TO_CHAR, NOW |
| E.8 | Extract year, month, day-of-week from timestamps | EXTRACT(YEAR/MONTH/DOW …) |

**Key finding:** Rohan Mehta abandoned his Farmland Crisis session after only 10 minutes (session 5). Ananya Singh's Monsoon Recharge session (session 10) is currently ongoing with no result recorded.

### Section F: Transaction Demo

| Query | Purpose | Key Concepts |
|---|---|---|
| F.1 | Demonstrate atomicity of session + result insert | BEGIN, SAVEPOINT, currval, ROLLBACK |

---

## 17. Execution Instructions

### Prerequisites

1. PostgreSQL must be installed and running.
2. A database named `aquaguardian` must be created.

```bash
psql -U postgres -c "CREATE DATABASE aquaguardian;"
```

### Running the Files in Order

```bash
# Step 1: Create the tables
psql -U postgres -d aquaguardian -f database/schema.sql

# Step 2: Insert sample data
psql -U postgres -d aquaguardian -f database/seed.sql

# Step 3: Run all analytics queries
psql -U postgres -d aquaguardian -f database/analytics.sql

# Step 4: Run the full syllabus queries file
psql -U postgres -d aquaguardian -f database/queries.sql
```

Adjust the `-U postgres` flag to match your local PostgreSQL username if it is different.

### Running a Single Query

To test one query interactively, open the PostgreSQL shell:

```bash
psql -U postgres -d aquaguardian
```

Then paste any query from `analytics.sql` directly at the `aquaguardian=#` prompt.

> **Important:** Do not re-run `seed.sql` against a database that already has data. It will fail on the UNIQUE constraint for `email`. Always start from a fresh database (`DROP DATABASE aquaguardian; CREATE DATABASE aquaguardian;`) before re-running from scratch.

---

## 18. Validation Results

### Execution Environment

- **PostgreSQL version:** 18.6 (Homebrew, macOS aarch64)
- **Database name:** `aquaguardian`
- **Schema loaded from:** `database/schema.sql`
- **Seed data loaded from:** `database/seed.sql`

### Confirmed Row Counts (after seed, before analytics)

| Table | Rows |
|---|---|
| `users` | 6 |
| `scenarios` | 4 |
| `game_sessions` | 10 |
| `game_results` | 8 |

### File Execution Results

| File | Command | Exit Code | Errors |
|---|---|---|---|
| `schema.sql` | `psql … -f schema.sql` | 0 | None |
| `seed.sql` | `psql … -f seed.sql` | 0 | None |
| `queries.sql` | `psql … -f queries.sql` | 0 | None |
| `analytics.sql` | `psql … -f analytics.sql` | 0 | None |

### Post-Analytics Data Integrity

After running `analytics.sql` (which includes a `BEGIN … ROLLBACK` transaction demo), row counts were re-verified:

| Table | Expected | Actual |
|---|---|---|
| `game_sessions` | 10 | 10 |
| `game_results` | 8 | 8 |

The ROLLBACK correctly undid the demo inserts, leaving the seed data unchanged.

### Selected Query Outputs (Verified)

**Leaderboard (C.1):**

| row_num | rank | dense_rank | player_name | total_score | total_water_saved | completed_sessions |
|---|---|---|---|---|---|---|
| 1 | 1 | 1 | Sneha Joshi | 2850 | 480 | 2 |
| 2 | 2 | 2 | Priya Patel | 2530 | 400 | 2 |
| 3 | 3 | 3 | Aarav Sharma | 2050 | 300 | 2 |
| 4 | 4 | 4 | Karan Verma | 1100 | 155 | 1 |
| 5 | 5 | 5 | Rohan Mehta | 920 | 200 | 1 |

**Global Conservation Summary (D.1):**

| Metric | Value |
|---|---|
| Total results evaluated | 8 |
| Total water saved overall | 1535 |
| Avg water saved per game | 191.88 |
| Min water saved (single game) | 90 |
| Max water saved (single game) | 350 |
| Lowest score recorded | 780 |
| Highest score recorded | 1980 |

**Monthly Trends (E.7):**

| display_month | total_sessions | completed_sessions | total_water_saved |
|---|---|---|---|
| February 2024 | 4 | 4 | 700 |
| March 2024 | 4 | 3 | 680 |
| April 2024 | 2 | 1 | 155 |

**Transaction Demo (F.1):**
```
BEGIN
INSERT 0 1
SAVEPOINT
INSERT 0 1
ROLLBACK
```
Both inserts were reversed. Data counts confirmed at 10 sessions and 8 results after rollback.

---

## 19. DE Syllabus Coverage Checklist

The table below maps each required syllabus topic to the query or section that demonstrates it.

| # | Syllabus Topic | Demonstrated | Location |
|---|---|---|---|
| 1 | CREATE TABLE (DDL) | ✅ | `schema.sql` |
| 2 | ALTER TABLE (DDL) | ✅ | `queries.sql` Section A |
| 3 | DROP TABLE / TRUNCATE (DDL) | ✅ | `queries.sql` Section A |
| 4 | INSERT (DML) | ✅ | `seed.sql`, `queries.sql` Section C |
| 5 | SELECT (DML) | ✅ | All analytics queries |
| 6 | UPDATE (DML) | ✅ | `queries.sql` Section C |
| 7 | DELETE (DML) | ✅ | `queries.sql` Section C |
| 8 | GRANT / REVOKE (DCL) | ✅ | `queries.sql` Section D |
| 9 | BEGIN / COMMIT (TCL) | ✅ | `analytics.sql` F.1, `queries.sql` Section E |
| 10 | ROLLBACK (TCL) | ✅ | `analytics.sql` F.1 |
| 11 | SAVEPOINT (TCL) | ✅ | `analytics.sql` F.1 |
| 12 | SELECT with WHERE | ✅ | `queries.sql` Section B |
| 13 | DISTINCT | ✅ | `queries.sql` Section B |
| 14 | IN / NOT IN | ✅ | `queries.sql` Section B |
| 15 | LIKE / ILIKE | ✅ | `queries.sql` Section B |
| 16 | EXISTS / NOT EXISTS | ✅ | `analytics.sql` E.4 |
| 17 | ORDER BY | ✅ | Multiple queries |
| 18 | LIMIT and OFFSET | ✅ | `queries.sql` Section B, `analytics.sql` E.3 |
| 19 | COUNT() | ✅ | A.1, A.2, A.5, A.6, B.1–B.7, C.1, D.1, E.1 |
| 20 | SUM() | ✅ | A.4, A.5, A.6, C.1, D.1, D.4 |
| 21 | AVG() | ✅ | A.3, A.5, B.2, B.3, B.4, B.7, D.1, D.2, D.4 |
| 22 | MIN() | ✅ | B.4, D.1 |
| 23 | MAX() | ✅ | B.4, D.1 |
| 24 | GROUP BY | ✅ | A.1–A.7, B.1–B.7, C.1, D.4, E.1, E.5, E.7 |
| 25 | HAVING | ✅ | A.6, B.7 |
| 26 | CASE statement | ✅ | A.2, B.6, D.3, E.2, E.5 |
| 27 | INNER JOIN | ✅ | A.6, A.7, B.4, B.5, B.7, C.1, D.3, E.6 |
| 28 | LEFT JOIN | ✅ | A.1–A.5, B.1–B.3, B.6, D.4, E.3, E.5, E.7 |
| 29 | Subquery in WHERE | ✅ | E.1 (scalar), E.4 (NOT EXISTS) |
| 30 | Subquery in FROM (derived table) | ✅ | A.7 |
| 31 | Correlated subquery | ✅ | E.4 |
| 32 | ROW_NUMBER() | ✅ | C.1 |
| 33 | RANK() | ✅ | C.1 |
| 34 | DENSE_RANK() | ✅ | A.5, C.1 |
| 35 | LAG() | ✅ | E.6 |
| 36 | LEAD() | ✅ | E.6 |
| 37 | PARTITION BY | ✅ | E.6 |
| 38 | OVER() | ✅ | A.5, C.1, E.6 |
| 39 | NOW() | ✅ | E.7 |
| 40 | DATE_TRUNC() | ✅ | E.7 |
| 41 | TO_CHAR() | ✅ | E.7 |
| 42 | EXTRACT() | ✅ | E.2, E.8 |
| 43 | Timestamp arithmetic (interval) | ✅ | E.2 |
| 44 | UPPER() / LOWER() | ✅ | A.8 |
| 45 | LENGTH() | ✅ | A.8 |
| 46 | CONCAT() | ✅ | A.8 |
| 47 | ROUND() | ✅ | Throughout analytics |
| 48 | COALESCE() | ✅ | A.3, A.4, A.5, D.4, E.2, E.3, E.7 |
| 49 | NULLIF() | ✅ | B.6, E.5 |
| 50 | 1NF | ✅ | Section 14 of this guide |
| 51 | 2NF | ✅ | Section 14 of this guide |
| 52 | 3NF | ✅ | Section 14 of this guide |
| 53 | BCNF | ✅ | Section 14 of this guide |
| 54 | Functional dependencies | ✅ | Section 14 of this guide |
| 55 | Candidate keys and super keys | ✅ | Sections 5 and 14 of this guide |
| 56 | ACID properties | ✅ | Section 15 of this guide |
| 57 | Transaction anomalies | ✅ | Section 15 of this guide |
| 58 | PRIMARY KEY constraint | ✅ | `schema.sql` |
| 59 | FOREIGN KEY constraint | ✅ | `schema.sql` |
| 60 | UNIQUE constraint | ✅ | `schema.sql` |
| 61 | NOT NULL constraint | ✅ | `schema.sql` |
| 62 | CHECK constraint | ✅ | `schema.sql` |
| 63 | Referential integrity | ✅ | `schema.sql`, Section 5 |
