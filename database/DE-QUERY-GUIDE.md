# AquaGuardian – DE Query Guide
**Task:** DE Task 1 – Checkpoint 2 | **Author:** Yash Kumar

> This guide explains every SQL topic in the Data Engineering syllabus in simple language. Each section has a short explanation followed by an example from the AquaGuardian database. Use this as a viva reference.

---

## How to Run the Files

```bash
# 1. Open PostgreSQL
psql -U postgres

# 2. Create and connect to the database
CREATE DATABASE aquaguardian;
\c aquaguardian

# 3. Run files in order
\i database/schema.sql
\i database/seed.sql
\i database/queries.sql
```

---

## 1. DDL – Data Definition Language

DDL commands define or change the **structure** of the database (tables, columns). They do not deal with actual data rows.

| Command | What it does |
|---|---|
| `CREATE TABLE` | Makes a new table |
| `ALTER TABLE` | Adds, renames, or removes a column |
| `DROP TABLE` | Permanently deletes a table |
| `TRUNCATE` | Deletes all rows from a table very quickly |

```sql
-- Add a column
ALTER TABLE users ADD COLUMN IF NOT EXISTS city VARCHAR(100) DEFAULT 'Unknown';

-- Remove it
ALTER TABLE users DROP COLUMN IF EXISTS city;

-- Remove all rows quickly (no row-by-row delete)
TRUNCATE TABLE demo_log;

-- Delete the whole table
DROP TABLE IF EXISTS demo_log;
```

> [!NOTE]
> `DROP` removes the table itself. `TRUNCATE` removes only the rows — the table structure remains.

---

## 2. DML – Data Manipulation Language

DML commands work with the **data inside** tables.

| Command | What it does |
|---|---|
| `SELECT` | Read rows |
| `INSERT` | Add new rows |
| `UPDATE` | Change existing rows |
| `DELETE` | Remove rows |

```sql
-- Read
SELECT name, email FROM users;

-- Add
INSERT INTO users (name, email, password_hash)
VALUES ('Yash Kumar', 'yash@email.com', 'hash_yash');

-- Change
UPDATE users SET name = 'Yash K.' WHERE email = 'yash@email.com';

-- Remove
DELETE FROM users WHERE email = 'yash@email.com';
```

---

## 3. DCL – Data Control Language

DCL controls **who can do what** in the database.

| Command | What it does |
|---|---|
| `GRANT` | Give a user permission |
| `REVOKE` | Take permission away |

```sql
-- Allow a read-only user to SELECT from users
GRANT SELECT ON users TO readonly_user;

-- Remove that permission
REVOKE SELECT ON users FROM readonly_user;
```

> [!IMPORTANT]
> Run DCL commands as a **superuser** (e.g., `postgres`). Normal users cannot grant permissions they do not own.

---

## 4. TCL – Transaction Control Language

A **transaction** wraps multiple SQL statements so they either ALL succeed or NONE are saved. This protects data consistency.

| Command | What it does |
|---|---|
| `BEGIN` | Start a transaction |
| `SAVEPOINT` | Mark a checkpoint inside the transaction |
| `ROLLBACK TO SAVEPOINT` | Undo only back to that checkpoint |
| `ROLLBACK` | Undo everything since BEGIN |
| `COMMIT` | Save everything since BEGIN permanently |

```sql
BEGIN;
    INSERT INTO game_sessions (user_id, scenario_id, started_at, ended_at, status)
    VALUES (5, 3, '2024-05-01 09:00:00', '2024-05-01 10:00:00', 'completed')
    RETURNING session_id;

    SAVEPOINT after_session_insert;

    INSERT INTO game_results (session_id, score, groundwater_before, groundwater_after, water_saved, completed_at)
    VALUES (11, 1600, 25, 55, 275, '2024-05-01 10:00:00');

COMMIT; -- Both inserts are saved
```

> [!TIP]
> Use `RETURNING` after an `INSERT` to get back the auto-generated ID immediately — this avoids using an ambiguous subquery.

---

## 5. SELECT Clauses – DISTINCT, WHERE, IN, LIKE, EXISTS

```sql
-- DISTINCT: remove duplicate values in results
SELECT DISTINCT difficulty FROM scenarios;

-- WHERE: filter rows by a condition
SELECT * FROM game_sessions WHERE status = 'completed';

-- IN: match against a list of values
SELECT * FROM game_sessions WHERE status IN ('ongoing', 'abandoned');

-- LIKE: pattern matching. % means "any characters"
SELECT name FROM users WHERE name LIKE 'A%';  -- starts with A

-- ILIKE: same as LIKE but case-insensitive (PostgreSQL only)
SELECT scenario_name FROM scenarios WHERE description ILIKE '%water%';

-- EXISTS: check if a related row exists (returns TRUE/FALSE)
SELECT name FROM users u
WHERE EXISTS (
    SELECT 1 FROM game_sessions gs WHERE gs.user_id = u.user_id
);
```

---

## 6. ORDER BY, LIMIT, OFFSET

```sql
-- ORDER BY: sort results
SELECT * FROM game_results ORDER BY score DESC;  -- highest first

-- LIMIT: return only N rows
SELECT * FROM game_results ORDER BY score DESC LIMIT 3;

-- OFFSET: skip the first N rows (used for pagination)
SELECT * FROM game_results ORDER BY score DESC LIMIT 3 OFFSET 3;
-- Page 1 = LIMIT 3 OFFSET 0
-- Page 2 = LIMIT 3 OFFSET 3
-- Page 3 = LIMIT 3 OFFSET 6
```

---

## 7. Built-in Functions

### Numeric
```sql
ROUND(AVG(score), 2)   -- round to 2 decimal places
ABS(-50)               -- absolute value → 50
```

### String
```sql
UPPER(name)            -- 'aarav' → 'AARAV'
LOWER(email)           -- 'AARAV@EMAIL.COM' → 'aarav@email.com'
LENGTH(scenario_name)  -- number of characters
CONCAT(name, ' <', email, '>') -- join strings
TRIM('  hello  ')      -- remove leading/trailing spaces
```

### Date
```sql
NOW()                             -- current date and time
DATE_PART('month', created_at)    -- extract month number
AGE(NOW(), started_at)            -- time elapsed since started_at
```

### NULL Handling
```sql
COALESCE(ended_at, NOW())   -- use ended_at if not NULL, else use NOW()
NULLIF(score, 0)            -- return NULL if score = 0 (avoids divide-by-zero)
```

---

## 8. Aggregate Functions

Aggregate functions compute a single result from many rows.

| Function | Purpose |
|---|---|
| `COUNT(*)` | Count rows |
| `SUM(col)` | Total of a column |
| `AVG(col)` | Average value |
| `MIN(col)` | Smallest value |
| `MAX(col)` | Largest value |

```sql
SELECT COUNT(*)           AS total_users    FROM users;
SELECT SUM(water_saved)   AS total_saved    FROM game_results;
SELECT ROUND(AVG(score),2)AS avg_score      FROM game_results;
SELECT MIN(score)         AS lowest_score   FROM game_results;
SELECT MAX(score)         AS highest_score  FROM game_results;
```

---

## 9. GROUP BY and HAVING

`GROUP BY` splits rows into groups, then aggregates each group separately.
`HAVING` filters the **groups** (like WHERE, but for groups).

```sql
-- How many sessions per user?
SELECT user_id, COUNT(*) AS total_sessions
FROM   game_sessions
GROUP BY user_id
ORDER BY total_sessions DESC;

-- Only show users with more than 1 session
SELECT user_id, COUNT(*) AS total_sessions
FROM   game_sessions
GROUP BY user_id
HAVING COUNT(*) > 1;
```

> [!NOTE]
> `WHERE` filters individual rows **before** grouping. `HAVING` filters groups **after** grouping.

---

## 10. CASE Statement

`CASE` works like an if-else inside a SQL query. It returns a value based on conditions.

```sql
SELECT score,
       CASE
           WHEN score >= 1500 THEN 'Excellent'
           WHEN score >= 1000 THEN 'Good'
           ELSE                    'Needs Work'
       END AS performance_label
FROM game_results;
```

---

## 11. Joins

A **join** combines rows from two (or more) tables based on a related column.

| Join Type | What it returns |
|---|---|
| `INNER JOIN` | Only rows that match in BOTH tables |
| `LEFT JOIN` | All rows from the LEFT table + matches from right (NULL if no match) |
| `RIGHT JOIN` | All rows from the RIGHT table + matches from left (NULL if no match) |
| `FULL OUTER JOIN` | All rows from BOTH tables (NULL where no match) |
| `SELF JOIN` | A table joined to itself |

```sql
-- INNER JOIN: only sessions that have results
SELECT u.name, gr.score
FROM   game_results gr
INNER JOIN game_sessions gs ON gr.session_id = gs.session_id
INNER JOIN users          u  ON gs.user_id    = u.user_id;

-- LEFT JOIN: all users, even those with no sessions
SELECT u.name, COUNT(gs.session_id) AS sessions
FROM   users u
LEFT JOIN game_sessions gs ON u.user_id = gs.user_id
GROUP BY u.name;

-- RIGHT JOIN: all scenarios, even with no sessions
SELECT s.scenario_name, gs.session_id
FROM   game_sessions gs
RIGHT JOIN scenarios s ON gs.scenario_id = s.scenario_id;

-- FULL OUTER JOIN: all users + all sessions
SELECT u.name, gs.session_id
FROM   users u
FULL OUTER JOIN game_sessions gs ON u.user_id = gs.user_id;

-- SELF JOIN: compare users by signup date
SELECT a.name AS earlier, b.name AS later
FROM   users a JOIN users b
       ON a.user_id <> b.user_id AND a.created_at < b.created_at;
```

---

## 12. UNION, UNION ALL, INTERSECT

These combine results from two separate `SELECT` statements.

| Operator | What it does |
|---|---|
| `UNION` | Combines results, **removes duplicates** |
| `UNION ALL` | Combines results, **keeps duplicates** |
| `INTERSECT` | Returns only rows that appear in **both** results |

> Both SELECT statements must return the **same number of columns** with compatible types.

```sql
-- UNION: Easy + Hard scenarios in one list (no duplicates)
SELECT scenario_name FROM scenarios WHERE difficulty = 'Easy'
UNION
SELECT scenario_name FROM scenarios WHERE difficulty = 'Hard';

-- UNION ALL: same but keeps all rows
SELECT scenario_name FROM scenarios WHERE difficulty = 'Easy'
UNION ALL
SELECT scenario_name FROM scenarios WHERE difficulty = 'Hard';

-- INTERSECT: users who have BOTH completed AND abandoned sessions
SELECT user_id FROM game_sessions WHERE status = 'completed'
INTERSECT
SELECT user_id FROM game_sessions WHERE status = 'abandoned';
```

---

## 13. Subqueries

A **subquery** is a `SELECT` written inside another SQL statement.

### Types:

**WHERE subquery (non-correlated)** – inner query runs once, outer query uses its result.
```sql
SELECT name FROM users
WHERE user_id IN (
    SELECT DISTINCT user_id FROM game_sessions WHERE status = 'completed'
);
```

**FROM subquery (derived table)** – treat the inner query result as a temporary table.
```sql
SELECT user_avg.user_id, user_avg.avg_score
FROM (
    SELECT gs.user_id, ROUND(AVG(gr.score),2) AS avg_score
    FROM   game_results gr JOIN game_sessions gs ON gr.session_id = gs.session_id
    GROUP BY gs.user_id
) AS user_avg
WHERE user_avg.avg_score > 1000;
```

**Correlated subquery** – inner query references the outer query's row (runs once per outer row).
```sql
SELECT u.name,
       (SELECT MAX(gr.score)
        FROM   game_results gr JOIN game_sessions gs ON gr.session_id = gs.session_id
        WHERE  gs.user_id = u.user_id) AS highest_score
FROM   users u;
```

---

## 14. Window Functions

Window functions compute values **across a set of rows** without collapsing them into one row (unlike `GROUP BY`). The `OVER()` clause defines the window.

### ROW_NUMBER, RANK, DENSE_RANK

| Function | Tie handling |
|---|---|
| `ROW_NUMBER()` | Always unique — no ties ever |
| `RANK()` | Same rank for ties, **skips** next rank |
| `DENSE_RANK()` | Same rank for ties, **no skip** |

```sql
SELECT score,
    ROW_NUMBER()  OVER (ORDER BY score DESC) AS row_num,
    RANK()        OVER (ORDER BY score DESC) AS rank,
    DENSE_RANK()  OVER (ORDER BY score DESC) AS dense_rank
FROM game_results;
```

Example output with tied scores of 1750:

| score | row_num | rank | dense_rank |
|---|---|---|---|
| 1980 | 1 | 1 | 1 |
| 1750 | 2 | 2 | 2 |
| 1750 | 3 | 2 | 2 |
| 1600 | 4 | 4 | 3 |

### LAG and LEAD

```sql
-- LAG: look at the PREVIOUS row's value
LAG(gr.score)  OVER (PARTITION BY gs.user_id ORDER BY gs.started_at) AS prev_score

-- LEAD: look at the NEXT row's value
LEAD(gr.score) OVER (PARTITION BY gs.user_id ORDER BY gs.started_at) AS next_score
```

`PARTITION BY user_id` means the window resets for each user.

### Moving Average

A moving (rolling) average smooths out fluctuations by averaging the current row with its neighbours.

```sql
AVG(gr.score) OVER (
    PARTITION BY gs.user_id
    ORDER BY gs.started_at
    ROWS BETWEEN 1 PRECEDING AND CURRENT ROW
) AS moving_avg
```
`ROWS BETWEEN 1 PRECEDING AND CURRENT ROW` = average of the current row and the one immediately before it.

---

## 15. Syllabus Coverage Checklist

| Topic | Covered | Query Reference |
|---|---|---|
| DDL: CREATE, ALTER, DROP, TRUNCATE | ✅ | Section A |
| DML: SELECT, INSERT, UPDATE, DELETE | ✅ | Sections B, C |
| DCL: GRANT, REVOKE | ✅ | Section D |
| TCL: BEGIN, COMMIT, ROLLBACK, SAVEPOINT | ✅ | Section E |
| SELECT, DISTINCT, WHERE, IN, LIKE, EXISTS | ✅ | Section B |
| ORDER BY, LIMIT, OFFSET | ✅ | Section B |
| Numeric, String, Date, NULL functions | ✅ | Section F |
| COUNT, SUM, AVG, MIN, MAX | ✅ | Section G |
| GROUP BY, HAVING | ✅ | Section H |
| CASE statements | ✅ | Section I |
| INNER JOIN | ✅ | Section J |
| LEFT JOIN | ✅ | Section J |
| RIGHT JOIN | ✅ | Section J |
| FULL OUTER JOIN | ✅ | Section J |
| SELF JOIN | ✅ | Section J |
| UNION, UNION ALL, INTERSECT | ✅ | Section K |
| WHERE subquery (non-correlated) | ✅ | Section L |
| FROM subquery (derived table) | ✅ | Section L |
| Correlated subquery | ✅ | Section L |
| ROW_NUMBER, RANK, DENSE_RANK | ✅ | Section M |
| LAG | ✅ | Section M |
| LEAD | ✅ | Section M |
| Moving Average | ✅ | Section M |

---

## ⚠️ Queries Requiring Manual Testing

The following queries require an actual running PostgreSQL instance to verify output. They are **syntactically correct** based on schema.sql but were **not executed** in this session:

1. **Section D (DCL – GRANT/REVOKE)** – requires real database roles (`readonly_user`, `game_app_user`) to exist. Create them first with `CREATE ROLE readonly_user;`
2. **Section E (Transaction)** – session_id 11 is hardcoded. If seed.sql was modified or run multiple times, the SERIAL counter may differ. Adjust the value to match `SELECT MAX(session_id) FROM game_sessions;` before running.
3. **Section J (SELF JOIN)** – will return many rows since it creates all user pairs. Use `LIMIT` to preview.

All other queries use only columns and table names confirmed from schema.sql and seed data IDs confirmed from seed.sql.
