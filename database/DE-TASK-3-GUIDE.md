# AquaGuardian — DE Task 3: Analytics and SQL Validation Guide

**Project:** AquaGuardian (Groundwater Conservation Educational Game)  
**Task:** DE Task 3 — Checkpoint 3 (Reviewed & Corrected)  
**Author:** Yash Kumar  
**Target Audience:** Second-Year B.Tech CSE (Viva-Ready Reference)

---

## PostgreSQL Execution Confirmation & Validation

All 21 analytical queries in this guide and in [`database/analytics.sql`](file:///Users/anubhavgupta/Desktop/groundwater-conservation-game/database/analytics.sql) were executed and validated directly on **PostgreSQL 18.6** against the `aquaguardian` database using the exact 4-table schema and sample seed records (`schema.sql` and `seed.sql`).

Every output table presented in this document reflects the **actual PostgreSQL query execution result** obtained from the live database.

### Command Used for Direct Execution
```bash
psql -U $(whoami) -d aquaguardian -f database/analytics.sql
```
*Result: Executed successfully with exit code 0 and zero runtime errors.*

---

## Syllabus Concepts Coverage Matrix

The matrix below lists **only** the concepts that are genuinely demonstrated in `analytics.sql`:

| Syllabus Concept | Verified Query Demonstrations | Purpose & Context |
|---|---|---|
| **INNER JOIN** | `A.6`, `B.4`, `B.7`, `C.1`, `D.3`, `E.6` | Combines matching records across tables |
| **LEFT JOIN** | `A.1`, `A.2`, `A.3`, `A.4`, `A.5`, `B.1`, `B.2`, `B.3`, `B.6`, `D.4`, `E.3`, `E.5`, `E.7` | Retains all left-table rows even without child records |
| **GROUP BY** | `A.1`, `A.2`, `A.3`, `A.4`, `A.5`, `A.6`, `B.1`, `B.2`, `B.3`, `B.4`, `B.5`, `B.6`, `B.7`, `C.1`, `D.4`, `E.1`, `E.5`, `E.7` | Aggregates data into categories and buckets |
| **HAVING** | `A.6`, `B.7` | Filters aggregated groups after `GROUP BY` |
| **COUNT()** | `A.1`, `A.2`, `A.5`, `A.6`, `B.1`, `B.2`, `B.3`, `B.4`, `B.5`, `B.6`, `B.7`, `C.1`, `D.1`, `D.4`, `E.1`, `E.5`, `E.7` | Counts rows or non-NULL column instances |
| **SUM()** | `A.4`, `A.5`, `A.6`, `C.1`, `D.1`, `D.4`, `E.7` | Computes cumulative totals |
| **AVG()** | `A.3`, `A.5`, `B.2`, `B.3`, `B.4`, `B.7`, `D.1`, `D.2`, `D.4` | Calculates arithmetic means |
| **MIN() & MAX()** | `B.4`, `D.1` | Discovers lowest and highest numeric values |
| **CASE Statements** | `A.2`, `B.6`, `D.3`, `E.2`, `E.5`, `E.7` | Implements conditional branching logic |
| **Scalar Subquery** | `E.1` | Computes overall session count for percentage denominator |
| **Correlated Subquery (`NOT EXISTS`)** | `E.4` | Performs fast anti-filtering to locate inactive users |
| **RANK()** | `C.1` | Ranks rows by score, leaving gaps on tied values |
| **DENSE_RANK()** | `A.5`, `C.1` | Ranks rows consecutively without gaps on ties |
| **LAG()** | `E.6` | Accesses preceding row data within a partition |
| **LEAD()** | `E.6` | Accesses succeeding row data within a partition |
| **NOW()** | `E.7` | Retrieves the current database timestamp |
| **DATE_TRUNC()** | `E.7` | Truncates timestamps to start of the calendar month |
| **TO_CHAR()** | `E.7` | Formats timestamps into readable strings |
| **COALESCE()** | `A.3`, `A.4`, `A.5`, `B.2`, `B.3`, `D.4`, `E.2`, `E.3`, `E.7` | Substitutes `NULL` with friendly default values |
| **ROUND()** | `A.3`, `A.5`, `B.2`, `B.3`, `B.4`, `B.6`, `B.7`, `D.1`, `D.2`, `D.4`, `E.1`, `E.2`, `E.5` | Rounds floats to 2 decimal places |
| **NULLIF()** | `B.6`, `E.5` | Prevents fatal division-by-zero errors |

---

## Section A: User Analytics

---

### A.1 Total Sessions Per User

#### Purpose
Count how many game sessions each registered player has started, including users with zero sessions.

#### SQL Code
```sql
SELECT 
    u.user_id, 
    u.name AS player_name, 
    COUNT(gs.session_id) AS total_sessions
FROM users u
LEFT JOIN game_sessions gs ON u.user_id = gs.user_id
GROUP BY u.user_id, u.name
ORDER BY total_sessions DESC, u.user_id;
```

#### Actual PostgreSQL 18 Execution Output
```text
 user_id | player_name  | total_sessions 
---------+--------------+----------------
       1 | Aarav Sharma |              2
       2 | Priya Patel  |              2
       3 | Rohan Mehta  |              2
       4 | Sneha Joshi  |              2
       5 | Karan Verma  |              1
       6 | Ananya Singh |              1
```

#### Viva Explanation & Concepts
- **`LEFT JOIN`:** Preserves every registered user. An `INNER JOIN` would have omitted players who never started a session.
- **`COUNT(gs.session_id)`:** Evaluates to `0` if a user has no sessions because `COUNT(column)` ignores `NULL`. If we used `COUNT(*)`, it would incorrectly return `1` for matched NULL rows.

---

### A.2 Completed Sessions Per User

#### Purpose
Count how many sessions each player successfully finished (filtering out ongoing or abandoned sessions).

#### SQL Code
```sql
SELECT 
    u.user_id, 
    u.name AS player_name, 
    COUNT(CASE WHEN gs.status = 'completed' THEN 1 END) AS completed_sessions
FROM users u
LEFT JOIN game_sessions gs ON u.user_id = gs.user_id
GROUP BY u.user_id, u.name
ORDER BY completed_sessions DESC, u.user_id;
```

#### Actual PostgreSQL 18 Execution Output
```text
 user_id | player_name  | completed_sessions 
---------+--------------+--------------------
       1 | Aarav Sharma |                  2
       2 | Priya Patel  |                  2
       4 | Sneha Joshi  |                  2
       3 | Rohan Mehta  |                  1
       5 | Karan Verma  |                  1
       6 | Ananya Singh |                  0
```

#### Viva Explanation & Concepts
- **Conditional Aggregation:** The `CASE` statement returns `1` when `status = 'completed'` and `NULL` otherwise. Because `COUNT()` ignores `NULL`, Ananya Singh correctly receives `0` completed sessions without being dropped from the report.

---

### A.3 Average Score Per User

#### Purpose
Calculate the mean score achieved across completed games for each player.

#### SQL Code
```sql
SELECT 
    u.user_id, 
    u.name AS player_name, 
    COALESCE(ROUND(AVG(gr.score), 2), 0.00) AS avg_score
FROM users u
LEFT JOIN game_sessions gs ON u.user_id = gs.user_id
LEFT JOIN game_results gr ON gs.session_id = gr.session_id
GROUP BY u.user_id, u.name
ORDER BY avg_score DESC;
```

#### Actual PostgreSQL 18 Execution Output
```text
 user_id | player_name  | avg_score 
---------+--------------+-----------
       4 | Sneha Joshi  |   1425.00
       2 | Priya Patel  |   1265.00
       5 | Karan Verma  |   1100.00
       1 | Aarav Sharma |   1025.00
       3 | Rohan Mehta  |    920.00
       6 | Ananya Singh |      0.00
```

#### Viva Explanation & Concepts
- **`ROUND(AVG(gr.score), 2)`:** Calculates the numeric mean rounded to two decimal places.
- **`COALESCE(..., 0.00)`:** Substitutes `NULL` with `0.00` for players who have not completed any game yet.

---

### A.4 Total Water Saved Per User

#### Purpose
Sum all conservation impact units (groundwater saved) contributed by each player.

#### SQL Code
```sql
SELECT 
    u.user_id, 
    u.name AS player_name, 
    COALESCE(SUM(gr.water_saved), 0) AS total_water_saved
FROM users u
LEFT JOIN game_sessions gs ON u.user_id = gs.user_id
LEFT JOIN game_results gr ON gs.session_id = gr.session_id
GROUP BY u.user_id, u.name
ORDER BY total_water_saved DESC;
```

#### Actual PostgreSQL 18 Execution Output
```text
 user_id | player_name  | total_water_saved 
---------+--------------+-------------------
       4 | Sneha Joshi  |               480
       2 | Priya Patel  |               400
       1 | Aarav Sharma |               300
       3 | Rohan Mehta  |               200
       5 | Karan Verma  |               155
       6 | Ananya Singh |                 0
```

---

### A.5 Best-Performing Users (Comprehensive Ranking)

#### Purpose
Rank all players by cumulative score using the `DENSE_RANK()` window function.

#### SQL Code
```sql
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
```

#### Actual PostgreSQL 18 Execution Output
```text
 user_id | player_name  | games_completed | total_score | avg_score | total_water_saved | rank_by_score 
---------+--------------+-----------------+-------------+-----------+-------------------+---------------
       4 | Sneha Joshi  |               2 |        2850 |   1425.00 |               480 |             1
       2 | Priya Patel  |               2 |        2530 |   1265.00 |               400 |             2
       1 | Aarav Sharma |               2 |        2050 |   1025.00 |               300 |             3
       5 | Karan Verma  |               1 |        1100 |   1100.00 |               155 |             4
       3 | Rohan Mehta  |               1 |         920 |    920.00 |               200 |             5
       6 | Ananya Singh |               0 |           0 |      0.00 |                 0 |             6
```

---

### A.6 Frequent Players Filter (GROUP BY + HAVING)

#### Purpose
Identify highly active players who have completed more than 1 session using the `HAVING` clause.

#### SQL Code
```sql
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
```

#### Actual PostgreSQL 18 Execution Output
```text
 user_id | player_name  | completed_sessions | total_score | total_water_saved 
---------+--------------+--------------------+-------------+-------------------
       4 | Sneha Joshi  |                  2 |        2850 |               480
       2 | Priya Patel  |                  2 |        2530 |               400
       1 | Aarav Sharma |                  2 |        2050 |               300
```

#### Viva Explanation & Concepts
- **`WHERE` vs `HAVING`:**
  - `WHERE` filters rows **before** aggregation happens.
  - `HAVING` filters rows **after** aggregation has grouped rows together. Since `COUNT(gr.result_id)` does not exist before grouping, filtering by it requires `HAVING`.

---

## Section B: Scenario Analytics

---

### B.1 Total Sessions Per Scenario

#### Purpose
Measure player engagement across all available scenarios regardless of completion status.

#### SQL Code
```sql
SELECT 
    s.scenario_id,
    s.scenario_name,
    s.difficulty,
    COUNT(gs.session_id) AS total_sessions
FROM scenarios s
LEFT JOIN game_sessions gs ON s.scenario_id = gs.scenario_id
GROUP BY s.scenario_id, s.scenario_name, s.difficulty
ORDER BY total_sessions DESC, s.scenario_id;
```

#### Actual PostgreSQL 18 Execution Output
```text
 scenario_id |   scenario_name    | difficulty | total_sessions 
-------------+--------------------+------------+----------------
           1 | Village Drought    | Easy       |              3
           2 | Farmland Crisis    | Medium     |              3
           3 | Industrial Overuse | Hard       |              2
           4 | Monsoon Recharge   | Easy       |              2
```

---

### B.2 Average Score Per Scenario

#### Purpose
Compare scoring difficulty across different conservation scenarios.

#### SQL Code
```sql
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
```

#### Actual PostgreSQL 18 Execution Output
```text
 scenario_id |   scenario_name    | difficulty | completed_plays | avg_score 
-------------+--------------------+------------+-----------------+-----------
           3 | Industrial Overuse | Hard       |               2 |   1865.00
           2 | Farmland Crisis    | Medium     |               2 |   1150.00
           4 | Monsoon Recharge   | Easy       |               1 |    920.00
           1 | Village Drought    | Easy       |               3 |    833.33
```

---

### B.3 Average Groundwater Improvement Per Scenario

#### Purpose
Calculate average aquifer level restoration per scenario (`groundwater_after - groundwater_before`).

#### SQL Code
```sql
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
```

#### Actual PostgreSQL 18 Execution Output
```text
 scenario_id |   scenario_name    | difficulty | recorded_results | avg_groundwater_improvement 
-------------+--------------------+------------+------------------+-----------------------------
           4 | Monsoon Recharge   | Easy       |                1 |                       30.00
           3 | Industrial Overuse | Hard       |                2 |                       28.50
           2 | Farmland Crisis    | Medium     |                2 |                       20.00
           1 | Village Drought    | Easy       |                3 |                       13.33
```

---

### B.4 Score Distribution Per Scenario (Demonstrating MIN, MAX, AVG)

#### Purpose
Demonstrate `MIN()` and `MAX()` aggregate functions to evaluate minimum and maximum scores achieved per scenario alongside the mean.

#### SQL Code
```sql
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
```

#### Actual PostgreSQL 18 Execution Output
```text
 scenario_id |   scenario_name    | difficulty | completed_count | lowest_score | highest_score | average_score 
-------------+--------------------+------------+-----------------+--------------+---------------+---------------
           1 | Village Drought    | Easy       |               3 |          780 |           870 |        833.33
           2 | Farmland Crisis    | Medium     |               2 |         1100 |          1200 |       1150.00
           3 | Industrial Overuse | Hard       |               2 |         1750 |          1980 |       1865.00
           4 | Monsoon Recharge   | Easy       |               1 |          920 |           920 |        920.00
```

#### Viva Explanation & Concepts
- **`MIN(col)` & `MAX(col)`:** Built-in aggregate functions that find the extremum values in a grouped partition.
- In `Industrial Overuse`, scores range from 1,750 (lowest) to 1,980 (highest).

---

### B.5 Most Popular Scenario

#### Purpose
Identify the scenario with the highest player attempts.

#### SQL Code
```sql
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
```

#### Actual PostgreSQL 18 Execution Output
```text
 scenario_id |  scenario_name  | difficulty | session_count 
-------------+-----------------+------------+---------------
           1 | Village Drought | Easy       |             3
```

---

### B.6 Scenario Completion Statistics

#### Purpose
Complete breakdown of attempts, completions, abandonments, and success rates.

#### SQL Code
```sql
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
```

#### Actual PostgreSQL 18 Execution Output
```text
 scenario_id |   scenario_name    | difficulty | total_attempts | completed_count | abandoned_count | ongoing_count | completion_rate_pct 
-------------+--------------------+------------+----------------+-----------------+-----------------+---------------+---------------------
           1 | Village Drought    | Easy       |              3 |               3 |               0 |             0 |              100.00
           2 | Farmland Crisis    | Medium     |              3 |               2 |               1 |             0 |               66.67
           3 | Industrial Overuse | Hard       |              2 |               2 |               0 |             0 |              100.00
           4 | Monsoon Recharge   | Easy       |              2 |               1 |               0 |             1 |               50.00
```

---

### B.7 High-Impact Conservation Scenarios (GROUP BY + HAVING)

#### Purpose
Demonstrate `HAVING` by filtering for scenarios that deliver high average water savings ($\ge 150$ units).

#### SQL Code
```sql
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
```

#### Actual PostgreSQL 18 Execution Output
```text
 scenario_id |   scenario_name    | difficulty | completed_plays | avg_score | avg_water_saved 
-------------+--------------------+------------+-----------------+-----------+-----------------
           3 | Industrial Overuse | Hard       |               2 |   1865.00 |          330.00
           4 | Monsoon Recharge   | Easy       |               1 |    920.00 |          200.00
           2 | Farmland Crisis    | Medium     |               2 |   1150.00 |          167.50
```

#### Viva Explanation & Concepts
- `Village Drought` averaged only 113.33 units of water saved, so it was filtered out by `HAVING AVG(gr.water_saved) >= 150`.

---

## Section C: Leaderboard (RANK vs DENSE_RANK)

---

### C.1 AquaGuardian Official Leaderboard

#### Purpose
Display competitive rankings comparing `RANK()` and `DENSE_RANK()` side-by-side.

#### SQL Code
```sql
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
```

#### Actual PostgreSQL 18 Execution Output
```text
 rank | dense_rank | player_name  | total_score | total_water_saved | completed_sessions 
------+------------+--------------+-------------+-------------------+--------------------
    1 |          1 | Sneha Joshi  |        2850 |               480 |                  2
    2 |          2 | Priya Patel  |        2530 |               400 |                  2
    3 |          3 | Aarav Sharma |        2050 |               300 |                  2
    4 |          4 | Karan Verma  |        1100 |               155 |                  1
    5 |          5 | Rohan Mehta  |         920 |               200 |                  1
```

#### Viva Explanation & Concepts
- **`RANK()` vs `DENSE_RANK()` Difference:**
  - Both award identical ranks to tied rows.
  - When a tie occurs at rank 2 (e.g., two players tied), `RANK()` assigns `1, 2, 2, 4` (skips rank 3).
  - `DENSE_RANK()` assigns `1, 2, 2, 3` (leaves no gaps).

---

## Section D: Groundwater Analytics

---

### D.1 Global Conservation Summary (COUNT, SUM, AVG, MIN, MAX)

#### Purpose
Showcase all 5 primary aggregate functions in a single table-level query.

#### SQL Code
```sql
SELECT 
    COUNT(result_id) AS total_results_evaluated,
    SUM(water_saved) AS total_water_saved_overall,
    ROUND(AVG(water_saved), 2) AS avg_water_saved_per_game,
    MIN(water_saved) AS min_water_saved_single_game,
    MAX(water_saved) AS max_water_saved_single_game,
    MIN(score) AS lowest_score_recorded,
    MAX(score) AS highest_score_recorded
FROM game_results;
```

#### Actual PostgreSQL 18 Execution Output
```text
 total_results_evaluated | total_water_saved_overall | avg_water_saved_per_game | min_water_saved_single_game | max_water_saved_single_game | lowest_score_recorded | highest_score_recorded 
-------------------------+---------------------------+--------------------------+-----------------------------+-----------------------------+-----------------------+------------------------
                       8 |                      1535 |                   191.88 |                          90 |                         350 |                   780 |                   1980
```

---

### D.2 Average Groundwater Before and After Gameplay

#### Purpose
Quantify net rehabilitation across all played missions.

#### SQL Code
```sql
SELECT 
    ROUND(AVG(groundwater_before), 2) AS avg_groundwater_before,
    ROUND(AVG(groundwater_after), 2) AS avg_groundwater_after,
    ROUND(AVG(groundwater_after - groundwater_before), 2) AS overall_avg_improvement
FROM game_results;
```

#### Actual PostgreSQL 18 Execution Output
```text
 avg_groundwater_before | avg_groundwater_after | overall_avg_improvement 
------------------------+-----------------------+-------------------------
                  32.88 |                 53.75 |                   20.88
```

---

### D.3 Groundwater Improvement Per Session With Recovery Category

#### Purpose
Classify individual completed runs into qualitative environmental categories using `CASE`.

#### SQL Code
```sql
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
```

#### Actual PostgreSQL 18 Execution Output
```text
 result_id | session_id | player_name  |   scenario_name    | groundwater_before | groundwater_after | groundwater_improvement | conservation_category 
-----------+------------+--------------+--------------------+--------------------+-------------------+-------------------------+-----------------------
         6 |          7 | Sneha Joshi  | Industrial Overuse |                 18 |                50 |                      32 | Substantial Recovery
         5 |          6 | Rohan Mehta  | Monsoon Recharge   |                 30 |                60 |                      30 | Substantial Recovery
         4 |          4 | Priya Patel  | Industrial Overuse |                 20 |                45 |                      25 | Substantial Recovery
         2 |          2 | Aarav Sharma | Farmland Crisis    |                 35 |                58 |                      23 | Moderate Recovery
         8 |          9 | Karan Verma  | Farmland Crisis    |                 38 |                55 |                      17 | Moderate Recovery
         7 |          8 | Sneha Joshi  | Village Drought    |                 42 |                57 |                      15 | Moderate Recovery
         1 |          1 | Aarav Sharma | Village Drought    |                 40 |                55 |                      15 | Moderate Recovery
         3 |          3 | Priya Patel  | Village Drought    |                 40 |                50 |                      10 | Minimal Recovery
```

---

### D.4 User-Wise Water Conservation Performance

#### Purpose
Assess each player's environmental impact, tracking volume and average recovery.

#### SQL Code
```sql
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
```

#### Actual PostgreSQL 18 Execution Output
```text
 user_id | player_name  | sessions_contributed | total_water_saved | avg_water_saved_per_session | avg_groundwater_level_gain 
---------+--------------+----------------------+-------------------+-----------------------------+----------------------------
       4 | Sneha Joshi  |                    2 |               480 |                      240.00 |                      23.50
       2 | Priya Patel  |                    2 |               400 |                      200.00 |                      17.50
       1 | Aarav Sharma |                    2 |               300 |                      150.00 |                      19.00
       3 | Rohan Mehta  |                    1 |               200 |                      200.00 |                      30.00
       5 | Karan Verma  |                    1 |               155 |                      155.00 |                      17.00
       6 | Ananya Singh |                    0 |                 0 |                        0.00 |                       0.00
```

---

## Section E: Session & Temporal Analytics

---

### E.1 Completed Versus Ongoing Sessions

#### Purpose
Examine session distribution across all three allowed states (`completed`, `ongoing`, `abandoned`).

#### SQL Code
```sql
SELECT 
    status,
    COUNT(*) AS session_count,
    ROUND(100.0 * COUNT(*) / (SELECT COUNT(*) FROM game_sessions), 2) AS percentage
FROM game_sessions
GROUP BY status
ORDER BY session_count DESC;
```

#### Actual PostgreSQL 18 Execution Output
```text
  status   | session_count | percentage 
-----------+---------------+------------
 completed |             8 |      80.00
 ongoing   |             1 |      10.00
 abandoned |             1 |      10.00
```

---

### E.2 Session Duration

#### Purpose
Calculate play duration using PostgreSQL intervals and minutes while handling in-progress runs safely.

#### SQL Code
```sql
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
```

#### Actual PostgreSQL 18 Execution Output
```text
 session_id | player_name  |   scenario_name    |  status   |     started_at      |      ended_at       | duration_interval | duration_minutes | duration_display  
------------+--------------+--------------------+-----------+---------------------+---------------------+-------------------+------------------+-------------------
          1 | Aarav Sharma | Village Drought    | completed | 2024-02-01 10:00:00 | 2024-02-01 10:25:00 | 00:25:00          |            25.00 | 25 mins
          2 | Aarav Sharma | Farmland Crisis    | completed | 2024-02-05 11:00:00 | 2024-02-05 11:45:00 | 00:45:00          |            45.00 | 45 mins
          3 | Priya Patel  | Village Drought    | completed | 2024-02-10 09:00:00 | 2024-02-10 09:30:00 | 00:30:00          |            30.00 | 30 mins
          4 | Priya Patel  | Industrial Overuse | completed | 2024-02-20 14:00:00 | 2024-02-20 14:50:00 | 00:50:00          |            50.00 | 50 mins
          5 | Rohan Mehta  | Farmland Crisis    | abandoned | 2024-03-01 15:00:00 | 2024-03-01 15:10:00 | 00:10:00          |            10.00 | 10 mins
          6 | Rohan Mehta  | Monsoon Recharge   | completed | 2024-03-05 16:00:00 | 2024-03-05 16:35:00 | 00:35:00          |            35.00 | 35 mins
          7 | Sneha Joshi  | Industrial Overuse | completed | 2024-03-10 10:00:00 | 2024-03-10 11:00:00 | 01:00:00          |            60.00 | 60 mins
          8 | Sneha Joshi  | Village Drought    | completed | 2024-03-15 09:30:00 | 2024-03-15 10:00:00 | 00:30:00          |            30.00 | 30 mins
          9 | Karan Verma  | Farmland Crisis    | completed | 2024-04-01 13:00:00 | 2024-04-01 13:40:00 | 00:40:00          |            40.00 | 40 mins
         10 | Ananya Singh | Monsoon Recharge   | ongoing   | 2024-04-05 17:00:00 |                     |                   |                  | Still in progress
```

---

### E.3 Recent Game History

#### Purpose
Retrieve the 5 most recent gaming events with user and scenario details.

#### SQL Code
```sql
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
```

#### Actual PostgreSQL 18 Execution Output
```text
 session_id |     started_at      | player_name  |   scenario_name    | difficulty |  status   | score | water_saved 
------------+---------------------+--------------+--------------------+------------+-----------+-------+-------------
         10 | 2024-04-05 17:00:00 | Ananya Singh | Monsoon Recharge   | Easy       | ongoing   |     0 |           0
          9 | 2024-04-01 13:00:00 | Karan Verma  | Farmland Crisis    | Medium     | completed |  1100 |         155
          8 | 2024-03-15 09:30:00 | Sneha Joshi  | Village Drought    | Easy       | completed |   870 |         130
          7 | 2024-03-10 10:00:00 | Sneha Joshi  | Industrial Overuse | Hard       | completed |  1980 |         350
          6 | 2024-03-05 16:00:00 | Rohan Mehta  | Monsoon Recharge   | Easy       | completed |   920 |         200
```

---

### E.4 Users With No Completed Sessions

#### Purpose
Identify registered players who have not completed any game session using a correlated `NOT EXISTS` subquery.

#### SQL Code
```sql
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
```

#### Actual PostgreSQL 18 Execution Output
```text
 user_id |     name     |         email          |     created_at      
---------+--------------+------------------------+---------------------
       6 | Ananya Singh | ananya.singh@email.com | 2024-03-20 16:00:00
```

---

### E.5 Scenario-Wise Completion Rate

#### Purpose
Calculate success and abandon percentages per scenario.

#### SQL Code
```sql
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
```

#### Actual PostgreSQL 18 Execution Output
```text
 scenario_id |   scenario_name    | difficulty | total_played | completed_sessions | abandoned_sessions | ongoing_sessions | completion_rate_percentage 
-------------+--------------------+------------+--------------+--------------------+--------------------+------------------+----------------------------
           1 | Village Drought    | Easy       |            3 |                  3 |                  0 |                0 |                     100.00
           3 | Industrial Overuse | Hard       |            2 |                  2 |                  0 |                0 |                     100.00
           2 | Farmland Crisis    | Medium     |            3 |                  2 |                  1 |                0 |                      66.67
           4 | Monsoon Recharge   | Easy       |            2 |                  1 |                  0 |                1 |                      50.00
```

---

### E.6 Session Score Progression Per Player (Demonstrating LAG and LEAD)

#### Purpose
Demonstrate `LAG()` and `LEAD()` window functions to inspect player performance trends across successive games.

#### SQL Code
```sql
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
```

#### Actual PostgreSQL 18 Execution Output
```text
 user_id | player_name  | session_id |   scenario_name    |     started_at      | current_score | previous_score | score_growth | next_score 
---------+--------------+------------+--------------------+---------------------+---------------+----------------+--------------+------------
       1 | Aarav Sharma |          1 | Village Drought    | 2024-02-01 10:00:00 |           850 |                |              |       1200
       1 | Aarav Sharma |          2 | Farmland Crisis    | 2024-02-05 11:00:00 |          1200 |            850 |          350 |           
       2 | Priya Patel  |          3 | Village Drought    | 2024-02-10 09:00:00 |           780 |                |              |       1750
       2 | Priya Patel  |          4 | Industrial Overuse | 2024-02-20 14:00:00 |          1750 |            780 |          970 |           
       3 | Rohan Mehta  |          6 | Monsoon Recharge   | 2024-03-05 16:00:00 |           920 |                |              |           
       4 | Sneha Joshi  |          7 | Industrial Overuse | 2024-03-10 10:00:00 |          1980 |                |              |        870
       4 | Sneha Joshi  |          8 | Village Drought    | 2024-03-15 09:30:00 |           870 |           1980 |        -1110 |           
       5 | Karan Verma  |          9 | Farmland Crisis    | 2024-04-01 13:00:00 |          1100 |                |              |           
```

#### Viva Explanation & Concepts
- **`PARTITION BY gs.user_id`:** Isolates the window so the previous/next scores are only computed within that specific user's match history.
- **`LAG(col)`:** Looks back 1 row. On Aarav Sharma's second game, `LAG()` gives 850, showing a score improvement (`score_growth`) of +350 points.
- **`LEAD(col)`:** Looks forward 1 row. On Aarav Sharma's first game, `LEAD()` previews his next score (1200).
- First rows naturally produce `NULL` for `LAG()`, and final rows produce `NULL` for `LEAD()`.

---

### E.7 Monthly Gameplay Activity Trends (Demonstrating DATE_TRUNC, TO_CHAR, NOW)

#### Purpose
Demonstrate PostgreSQL temporal functions (`DATE_TRUNC`, `TO_CHAR`, `NOW`) by aggregating sessions and water savings into monthly buckets.

#### SQL Code
```sql
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
```

#### Actual PostgreSQL 18 Execution Output
```text
     month_start     | display_month | total_sessions_started | completed_sessions | total_water_saved | report_generated_on 
---------------------+---------------+------------------------+--------------------+-------------------+---------------------
 2024-02-01 00:00:00 | February 2024 |                      4 |                  4 |               700 | 2026-11-08
 2024-03-01 00:00:00 | March 2024    |                      4 |                  3 |               680 | 2026-11-08
 2024-04-01 00:00:00 | April 2024    |                      2 |                  1 |               155 | 2026-11-08
```

#### Viva Explanation & Concepts
- **`DATE_TRUNC('month', timestamp)`:** Truncates a timestamp to the very first second of that month (e.g., `2024-02-05 11:00:00` becomes `2024-02-01 00:00:00`). Essential for time-series grouping.
- **`TO_CHAR(..., 'FMMonth YYYY')`:** Formats the date into clean readable English (`February 2024`). `FM` prefix removes leading fill blanks.
- **`NOW()`:** Returns the current transaction timestamp, used here to stamp when the analytical report was produced.
