# AquaGuardian — Database Transactions and ACID
**Author:** Yash Kumar | **Task:** DE Component — Transactions

---

## 1. What Is a Transaction?

A **transaction** is a logical unit of work in a database — a sequence of SQL statements that must all succeed together or all fail together. Transactions prevent the database from being left in a half-updated, inconsistent state.

In AquaGuardian, the classic example is recording a completed game session:

1. Update `game_sessions.status` to `'completed'` and set `ended_at`.
2. Insert a row into `game_results` with the score and groundwater data.

If step 1 succeeds but step 2 fails (e.g., a CHECK constraint violation on `score`), the session would appear completed but have no result — a corrupted state. A transaction prevents this.

---

## 2. ACID Properties

### Atomicity

> All operations in a transaction are treated as one unit. Either ALL succeed and are committed, or NONE take effect.

**AquaGuardian example:** Inserting a game session and its result must be atomic. If the result insert fails, the session insert is rolled back automatically.

```sql
BEGIN;
    -- Step 1: Mark session as completed
    UPDATE game_sessions
    SET status = 'completed', ended_at = '2024-05-01 10:00:00'
    WHERE session_id = 10;

    -- Step 2: Record the result
    INSERT INTO game_results (session_id, score, groundwater_before, groundwater_after, water_saved, completed_at)
    VALUES (10, 1400, 25, 55, 230, '2024-05-01 10:00:00');

ROLLBACK; -- rolled back for demo safety; use COMMIT in production
```

If the INSERT fails (e.g., score is -1 which violates CHECK score >= 0), neither the UPDATE nor the INSERT is applied. **Atomicity guaranteed.**

### Consistency

> A transaction takes the database from one valid state to another. All constraints must hold at the end.

**AquaGuardian example:** The `game_results.session_id` UNIQUE constraint ensures a result can only be recorded once per session. The CHECK constraint ensures `groundwater_before BETWEEN 0 AND 100`. PostgreSQL enforces these at COMMIT time, ensuring consistency.

If we attempted:
```sql
INSERT INTO game_results (session_id, score, ...) VALUES (1, -100, ...);
-- Fails: score must be >= 0 → transaction rolls back → consistency maintained
```

### Isolation

> Concurrent transactions do not see each other's intermediate (uncommitted) state.

**AquaGuardian scenario:** Two players submit their game results at the same millisecond. Transaction A is updating session 7's result while Transaction B reads the leaderboard. Transaction B should NOT see Transaction A's uncommitted update.

PostgreSQL default isolation level: **READ COMMITTED** — a transaction sees only data committed before each statement begins. This prevents dirty reads.

```sql
-- Session A (player submitting result):
BEGIN;
UPDATE game_sessions SET status = 'completed' WHERE session_id = 10;
-- Session A has not committed yet

-- Session B (leaderboard reader) sees session 10 as still 'ongoing'
-- because Session A has not committed → Isolation maintained
```

### Durability

> Once a transaction is committed, the changes are permanent even if the database server crashes immediately afterward.

PostgreSQL achieves durability through its **Write-Ahead Log (WAL)**. Every committed change is first written to the WAL on disk before PostgreSQL acknowledges the commit. On restart after a crash, PostgreSQL replays the WAL to recover committed transactions.

---

## 3. TCL Commands

### BEGIN

Starts a transaction block. All subsequent statements are part of this transaction until `COMMIT` or `ROLLBACK`.

```sql
BEGIN;
```

### COMMIT

Saves all changes made since `BEGIN` permanently to the database.

```sql
COMMIT;
```

### ROLLBACK

Undoes all changes made since `BEGIN`. The database returns to its state before `BEGIN`.

```sql
ROLLBACK;
```

### SAVEPOINT

Marks a named checkpoint inside an active transaction. You can roll back to this point without undoing the entire transaction.

```sql
SAVEPOINT after_session_insert;
```

### ROLLBACK TO SAVEPOINT

Undoes all changes made after a specific savepoint, without ending the transaction.

```sql
ROLLBACK TO SAVEPOINT after_session_insert;
```

---

## 4. Live Transaction Demonstration

The following transaction was executed and verified on the `aquaguardian` database. The final `ROLLBACK` keeps the seed data clean so the demo can be run repeatedly.

```sql
BEGIN;

    -- Step 1: Insert a completed session for Ananya Singh (user_id = 6)
    --         playing Farmland Crisis (scenario_id = 2)
    INSERT INTO game_sessions
        (user_id, scenario_id, started_at, ended_at, status)
    VALUES
        (6, 2, '2024-05-01 09:00:00', '2024-05-01 09:45:00', 'completed');

    -- SAVEPOINT: checkpoint after session insert
    -- If the next INSERT fails, we can roll back only the result insert
    -- while keeping the session insert (useful for partial retry logic)
    SAVEPOINT after_session;

    -- Step 2: Insert the matching result
    -- currval() returns the SERIAL value generated in Step 1
    -- This is safe and unambiguous — no subquery needed
    INSERT INTO game_results
        (session_id, score, groundwater_before, groundwater_after, water_saved, completed_at)
    VALUES
        (currval('game_sessions_session_id_seq'), 1300, 28, 55, 240, '2024-05-01 09:45:00');

ROLLBACK;
-- After ROLLBACK, game_sessions still has 10 rows, game_results still has 8.
```

**Actual execution output:**
```
BEGIN
INSERT 0 1
SAVEPOINT
INSERT 0 1
ROLLBACK
```

**Verification after rollback:**
```sql
SELECT COUNT(*) FROM game_sessions;  -- → 10 (unchanged)
SELECT COUNT(*) FROM game_results;   -- → 8  (unchanged)
```

---

## 5. Partial Rollback with SAVEPOINT

This example demonstrates rolling back only the result insert while keeping the session insert:

```sql
BEGIN;

    INSERT INTO game_sessions (user_id, scenario_id, started_at, ended_at, status)
    VALUES (5, 1, '2024-06-01 10:00:00', '2024-06-01 10:30:00', 'completed');

    SAVEPOINT before_result;

    -- Intentionally invalid: score = -500 violates CHECK (score >= 0)
    INSERT INTO game_results (session_id, score, groundwater_before, groundwater_after, water_saved, completed_at)
    VALUES (currval('game_sessions_session_id_seq'), -500, 40, 60, 100, NOW());
    -- PostgreSQL raises an error here

    -- Roll back only the failed result insert; keep the session
    ROLLBACK TO SAVEPOINT before_result;

    -- Now insert a corrected result
    INSERT INTO game_results (session_id, score, groundwater_before, groundwater_after, water_saved, completed_at)
    VALUES (currval('game_sessions_session_id_seq'), 500, 40, 60, 100, NOW());

ROLLBACK; -- Full rollback for demo safety
```

---

## 6. Transaction Anomalies

### Dirty Read

**Definition:** A transaction reads data written by another transaction that has NOT yet been committed. If the other transaction rolls back, the first transaction has read "dirty" (non-existent) data.

**Example in AquaGuardian:**  
Transaction A starts inserting a game result with score = 2000. Before A commits, Transaction B reads the leaderboard and sees score = 2000. Then A rolls back (the score was invalid). Transaction B made decisions based on data that never actually existed.

**PostgreSQL behaviour:** Dirty reads are **not possible** at the default `READ COMMITTED` isolation level. Each statement sees only data committed at the moment the statement begins.

### Non-Repeatable Read

**Definition:** A transaction reads the same row twice and gets different values because another transaction committed a change in between.

**Example in AquaGuardian:**  
Transaction A reads user Aarav's total score as 2050. Meanwhile, Transaction B commits a new game result for Aarav, increasing his score. Transaction A reads the total again and gets 3250. The value changed between two reads within the same transaction.

**PostgreSQL behaviour:** Occurs at `READ COMMITTED`. Prevented at `REPEATABLE READ` and above.

```sql
-- Prevent with REPEATABLE READ:
BEGIN TRANSACTION ISOLATION LEVEL REPEATABLE READ;
SELECT SUM(score) FROM game_results WHERE session_id IN (
    SELECT session_id FROM game_sessions WHERE user_id = 1
);
-- ... other work ...
-- Second read guaranteed to return same result
COMMIT;
```

### Phantom Read

**Definition:** A transaction re-executes a query with a WHERE clause and finds new rows that another committed transaction inserted in between.

**Example in AquaGuardian:**  
Transaction A queries `SELECT * FROM game_sessions WHERE user_id = 6` and finds 1 row. Transaction B inserts another session for user 6 and commits. Transaction A re-runs the same query and now finds 2 rows — a "phantom" appeared.

**PostgreSQL behaviour:** Prevented at `SERIALIZABLE` isolation level.

```sql
BEGIN TRANSACTION ISOLATION LEVEL SERIALIZABLE;
SELECT * FROM game_sessions WHERE user_id = 6;
-- ... any concurrent inserts for user 6 are invisible until commit
COMMIT;
```

### Lost Update

**Definition:** Two transactions read the same row, both modify it, and the second write overwrites the first without knowing about it.

**Example in AquaGuardian:**  
Two admin processes simultaneously add bonus points to Aarav's score. Both read score = 850, both add 100 and write 950. The result is 950 instead of the correct 1050.

**Prevention:** Use `SELECT ... FOR UPDATE` to lock the row before modifying it.

```sql
BEGIN;
SELECT score FROM game_results WHERE result_id = 1 FOR UPDATE;
-- Row is now locked; other transactions must wait
UPDATE game_results SET score = score + 100 WHERE result_id = 1;
COMMIT;
```

---

## 7. PostgreSQL Isolation Levels

| Isolation Level | Dirty Read | Non-Repeatable Read | Phantom Read |
|---|---|---|---|
| **READ UNCOMMITTED** | Possible (but PG treats as READ COMMITTED) | Possible | Possible |
| **READ COMMITTED** (default) | ❌ Not possible | Possible | Possible |
| **REPEATABLE READ** | ❌ Not possible | ❌ Not possible | Possible |
| **SERIALIZABLE** | ❌ Not possible | ❌ Not possible | ❌ Not possible |

PostgreSQL does not implement dirty reads even at `READ UNCOMMITTED` — it treats it as `READ COMMITTED` for safety.

**Setting isolation level:**
```sql
BEGIN TRANSACTION ISOLATION LEVEL SERIALIZABLE;
-- your statements
COMMIT;
```

---

## 8. Concurrent Game-Result Submission Example

Imagine two players finish at the same instant. Here is how PostgreSQL handles it safely:

**Transaction A** (Aarav finishes session 2):
```sql
BEGIN;
UPDATE game_sessions SET status='completed', ended_at=NOW() WHERE session_id=2;
INSERT INTO game_results (session_id, score, ...) VALUES (2, 1200, ...);
COMMIT;
```

**Transaction B** (Priya finishes session 4):
```sql
BEGIN;
UPDATE game_sessions SET status='completed', ended_at=NOW() WHERE session_id=4;
INSERT INTO game_results (session_id, score, ...) VALUES (4, 1750, ...);
COMMIT;
```

Since they operate on **different rows** (session 2 vs session 4), there is no lock contention. Both transactions commit successfully in parallel. The `UNIQUE` constraint on `game_results.session_id` guarantees no duplicate results even if the same session_id were submitted twice — the second insert would fail and be rolled back.

---

## 9. Viva Questions on Transactions

**Q1: What does ACID stand for?**  
Atomicity, Consistency, Isolation, Durability.

**Q2: What is the difference between ROLLBACK and ROLLBACK TO SAVEPOINT?**  
`ROLLBACK` undoes the entire transaction since `BEGIN`. `ROLLBACK TO SAVEPOINT name` undoes changes only since that savepoint, keeping the transaction active so work can continue.

**Q3: Why do we use `currval()` in the transaction instead of a subquery?**  
`currval()` returns the last value generated by the SERIAL sequence in the current session. A subquery like `WHERE started_at = '...'` might match multiple rows if the same timestamp appears. `currval()` is unambiguous and safe.

**Q4: What isolation level does PostgreSQL use by default?**  
READ COMMITTED — each statement sees only data committed before that statement began.

**Q5: What is a dirty read and can it happen in PostgreSQL?**  
A dirty read is reading uncommitted data from another transaction. PostgreSQL prevents dirty reads even at the READ UNCOMMITTED level by internally treating it as READ COMMITTED.

**Q6: How does PostgreSQL ensure durability?**  
Through the Write-Ahead Log (WAL). Every committed change is written to WAL before the commit is acknowledged. On crash recovery, WAL is replayed to restore all committed transactions.
