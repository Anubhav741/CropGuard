# AquaGuardian — Database Normalization
**Author:** Yash Kumar | **Task:** DE Component — Normalization

---

## 1. The Problem with Unnormalized Data

Before designing a database properly, it helps to see what goes wrong when we do it badly. Imagine storing AquaGuardian data in a single flat table:

### Unnormalized Table (UNF)

| session_id | player_name | player_email | password_hash | scenario_name | difficulty | score | groundwater_before | groundwater_after | water_saved | started_at |
|---|---|---|---|---|---|---|---|---|---|---|
| 1 | Aarav Sharma | aarav.sharma@email.com | hash_aarav | Village Drought | Easy | 850 | 40 | 55 | 120 | 2024-02-01 |
| 2 | Aarav Sharma | aarav.sharma@email.com | hash_aarav | Farmland Crisis | Medium | 1200 | 35 | 58 | 180 | 2024-02-05 |
| 3 | Priya Patel | priya.patel@email.com | hash_priya | Village Drought | Easy | 780 | 40 | 50 | 90 | 2024-02-10 |
| 4 | Priya Patel | priya.patel@email.com | hash_priya | Industrial Overuse | Hard | 1750 | 20 | 45 | 310 | 2024-02-20 |

**Problems with this design:**

1. **Data redundancy:** Aarav Sharma's email and password hash are repeated in every row he plays. If Aarav changes his email, every row must be updated.
2. **Update anomaly:** If we update Aarav's email in row 1 but forget row 2, the data becomes inconsistent.
3. **Insertion anomaly:** We cannot add a new scenario (`Monsoon Recharge`) to the database without also having a player who has played it.
4. **Deletion anomaly:** If we delete all sessions for a scenario, we lose all knowledge that the scenario ever existed.

Normalization solves these problems step by step.

---

## 2. First Normal Form (1NF)

**Rule:** Every column must hold atomic (single, indivisible) values. There must be no repeating groups. Every row must be uniquely identifiable.

**Is the UNF table already in 1NF?** Nearly — each cell holds one value. But there are still redundancy problems. Let us make it cleaner by ensuring every row has a unique identifier.

**UNF → 1NF:** Give each session row a unique primary key `session_id`. Ensure every column holds one atomic value.

After this step, the table is in 1NF:

| session_id PK | player_name | player_email | scenario_name | difficulty | score | groundwater_before | groundwater_after | water_saved |
|---|---|---|---|---|---|---|---|---|
| 1 | Aarav Sharma | aarav.sharma@email.com | Village Drought | Easy | 850 | 40 | 55 | 120 |
| 2 | Aarav Sharma | aarav.sharma@email.com | Farmland Crisis | Medium | 1200 | 35 | 58 | 180 |

**1NF is satisfied because:**
- Every cell holds one value (no lists or sets).
- Every row is uniquely identified by `session_id`.
- There are no repeating column groups.

**Still has problems:** Player name, email, and scenario details are still repeated.

---

## 3. Second Normal Form (2NF)

**Rule:** The table must be in 1NF, and **every non-key column must depend on the whole primary key** — not just part of it.

Partial dependency only matters when the primary key is composite (made of multiple columns). The 1NF table above has a single-column key (`session_id`), so technically there cannot be partial dependencies in that table itself. However, the data in it **contains hidden composite keys** that expose the problem.

Consider a composite-key version: `(player_email, scenario_name)` could also serve as a key in the flat table. In that case:
- `player_name` depends only on `player_email` (not on the full composite key).
- `difficulty` depends only on `scenario_name` (not on the full composite key).

These are **partial dependencies**. The fix is to split the flat table into tables with single-column natural keys.

**Decomposition:**

**Table: users** (player facts depend only on the player)
| user_id PK | name | email | password_hash | created_at |
|---|---|---|---|---|
| 1 | Aarav Sharma | aarav.sharma@email.com | hash_aarav | 2024-01-10 |

**Table: scenarios** (scenario facts depend only on the scenario)
| scenario_id PK | scenario_name | description | difficulty |
|---|---|---|---|
| 1 | Village Drought | A village is running out of groundwater... | Easy |

**Table: game_sessions** (a session depends on who played and which scenario)
| session_id PK | user_id FK | scenario_id FK | started_at | ended_at | status |
|---|---|---|---|---|---|
| 1 | 1 | 1 | 2024-02-01 | 2024-02-01 | completed |

**2NF is satisfied because:**
- Every table has a single-column primary key.
- Every non-key attribute in each table depends on the **whole** primary key of that table.
  - In `users`: `name`, `email`, `password_hash` all describe the user identified by `user_id`.
  - In `scenarios`: `scenario_name`, `description`, `difficulty` all describe the scenario identified by `scenario_id`.
  - In `game_sessions`: `user_id`, `scenario_id`, `started_at`, `ended_at`, `status` all describe the session identified by `session_id`.

---

## 4. Third Normal Form (3NF)

**Rule:** The table must be in 2NF, and **no non-key column may depend on another non-key column** (no transitive dependencies).

A transitive dependency has the form: `PK → A → B`, where B is determined by A rather than directly by the PK.

**Check each AquaGuardian table:**

**`users`:**
- Dependencies: `user_id → name`, `user_id → email`, `user_id → password_hash`, `user_id → created_at`
- Does any non-key column determine another? No. `name` does not determine `email`. `email` → `user_id` (that makes `email` a candidate key, not a transitive dependency).
- **No transitive dependencies → 3NF satisfied.**

**`scenarios`:**
- Dependencies: `scenario_id → scenario_name`, `scenario_id → description`, `scenario_id → difficulty`
- Does `difficulty` depend on `scenario_name`? No — a scenario's difficulty is a property of that specific scenario record, not derivable from its name.
- **No transitive dependencies → 3NF satisfied.**

**`game_sessions`:**
- Dependencies: `session_id → user_id`, `session_id → scenario_id`, `session_id → started_at`, `session_id → ended_at`, `session_id → status`
- `user_id` and `scenario_id` are foreign keys. They are facts about the session. The user's name is NOT stored here — it must be looked up via JOIN. So there is no transitive dependency.
- **No transitive dependencies → 3NF satisfied.**

**`game_results`:**
- Dependencies: `result_id → session_id`, `result_id → score`, `result_id → groundwater_before`, `result_id → groundwater_after`, `result_id → water_saved`, `result_id → completed_at`
- Does `score` determine `groundwater_after`? No — these are independent game outcome metrics. Neither can be derived from the other.
- **No transitive dependencies → 3NF satisfied.**

**What if we had stored scenario_difficulty inside game_sessions?**

Then we would have: `session_id → scenario_id → difficulty` — a transitive dependency.  
Difficulty would depend on `scenario_id`, which depends on `session_id`. That violates 3NF.  
Our design avoids this by keeping `difficulty` only in `scenarios`.

---

## 5. Boyce-Codd Normal Form (BCNF)

**Rule:** For every non-trivial functional dependency `X → Y`, X must be a **superkey** (a minimal or non-minimal set of columns that uniquely identifies every row).

BCNF is a stricter version of 3NF. It is violated when a non-superkey column determines another column, even if no transitive dependency is apparent.

### Functional Dependencies for Each Table

**`users` table:**

| Functional Dependency | Determinant is Superkey? |
|---|---|
| `user_id → name, email, password_hash, created_at` | ✅ Yes (`user_id` is PK) |
| `email → user_id, name, password_hash, created_at` | ✅ Yes (`email` is a candidate key — UNIQUE NOT NULL) |

Both determinants are superkeys. **`users` satisfies BCNF.**

**`scenarios` table:**

| Functional Dependency | Determinant is Superkey? |
|---|---|
| `scenario_id → scenario_name, description, difficulty` | ✅ Yes (`scenario_id` is PK) |

**`scenarios` satisfies BCNF.**

**`game_sessions` table:**

| Functional Dependency | Determinant is Superkey? |
|---|---|
| `session_id → user_id, scenario_id, started_at, ended_at, status` | ✅ Yes (`session_id` is PK) |

No other non-superkey column determines any other column. **`game_sessions` satisfies BCNF.**

**`game_results` table:**

| Functional Dependency | Determinant is Superkey? |
|---|---|
| `result_id → session_id, score, groundwater_before, groundwater_after, water_saved, completed_at` | ✅ Yes (`result_id` is PK) |
| `session_id → result_id, score, groundwater_before, groundwater_after, water_saved, completed_at` | ✅ Yes (`session_id` is a candidate key — UNIQUE NOT NULL) |

Both determinants are superkeys. **`game_results` satisfies BCNF.**

---

## 6. Summary

| Normal Form | Rule Checked | AquaGuardian Status |
|---|---|---|
| **UNF** | Flat single-table with repeated data | ❌ Violated — redundant user and scenario data |
| **1NF** | Atomic values, unique rows | ✅ All tables satisfy 1NF |
| **2NF** | No partial dependencies on part of a composite key | ✅ All tables satisfy 2NF (single-column PKs eliminate this risk) |
| **3NF** | No transitive dependencies (non-key → non-key) | ✅ All tables satisfy 3NF |
| **BCNF** | Every determinant is a superkey | ✅ All tables satisfy BCNF |

The four-table design achieves BCNF because:
- Each table stores facts about exactly one entity.
- Every non-key fact in each table is determined directly and only by the primary key (or another candidate key that is itself a superkey).
- No non-key column in any table determines another non-key column.

---

## 7. Viva Questions on Normalization

**Q1: What is a functional dependency?**  
A functional dependency `A → B` means that knowing the value of A uniquely determines the value of B. In `users`, `user_id → email` because each user_id is linked to exactly one email.

**Q2: What is a partial dependency?**  
A partial dependency occurs when a non-key column depends on only *part* of a composite primary key. It is an issue only when the PK has multiple columns. All AquaGuardian tables use single-column PKs, so partial dependency cannot arise.

**Q3: What is a transitive dependency?**  
A transitive dependency `PK → A → B` means B depends on A rather than directly on the PK. Example: if `game_sessions` stored `difficulty`, then `session_id → scenario_id → difficulty` would be a transitive dependency.

**Q4: Why does AquaGuardian satisfy BCNF?**  
In every table, all functional dependencies have a superkey (PK or candidate key) as the determinant. No ordinary non-key column determines any other column.

**Q5: What is the difference between 3NF and BCNF?**  
3NF allows non-key columns to be determined by candidate keys as long as they don't create transitive dependencies. BCNF is stricter — every determinant must be a superkey, even if it is a candidate key. In practice, the difference matters only when there are overlapping composite candidate keys, which does not occur in the AquaGuardian schema.
