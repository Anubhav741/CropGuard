-- =============================================================
-- AquaGuardian Seed Data
-- Task: DE Task 1 – Checkpoint 2
-- Author: Yash Kumar
-- Description: Sample records for all four tables.
--              Run schema.sql first, then run this file.
-- =============================================================


-- =============================================================
-- SECTION 1: users
-- 6 sample players with unique emails.
-- password_hash values are placeholder hashes (not real bcrypt).
-- =============================================================

INSERT INTO users (name, email, password_hash, created_at) VALUES
    ('Aarav Sharma',   'aarav.sharma@email.com',   'hash_aarav_001',  '2024-01-10 09:00:00'),
    ('Priya Patel',    'priya.patel@email.com',    'hash_priya_002',  '2024-01-15 10:30:00'),
    ('Rohan Mehta',    'rohan.mehta@email.com',    'hash_rohan_003',  '2024-02-01 08:45:00'),
    ('Sneha Joshi',    'sneha.joshi@email.com',    'hash_sneha_004',  '2024-02-14 11:00:00'),
    ('Karan Verma',    'karan.verma@email.com',    'hash_karan_005',  '2024-03-05 14:20:00'),
    ('Ananya Singh',   'ananya.singh@email.com',   'hash_ananya_006', '2024-03-20 16:00:00');


-- =============================================================
-- SECTION 2: scenarios
-- 4 game scenarios across all three difficulty levels.
-- =============================================================

INSERT INTO scenarios (scenario_name, description, difficulty) VALUES
    ('Village Drought',
     'A small village is running out of groundwater. Help the community reduce usage before the well runs dry.',
     'Easy'),

    ('Farmland Crisis',
     'Farmers are over-irrigating their fields. Introduce water-saving farming methods to restore the aquifer.',
     'Medium'),

    ('Industrial Overuse',
     'A factory is pumping groundwater unsustainably. Negotiate and implement conservation policies under time pressure.',
     'Hard'),

    ('Monsoon Recharge',
     'Guide the community to harvest rainwater and recharge the depleted groundwater table during the monsoon season.',
     'Easy');


-- =============================================================
-- SECTION 3: game_sessions
-- 10 sessions spread across users and scenarios.
-- Some sessions are completed, one is ongoing, one is abandoned.
-- ended_at is NULL for the ongoing session.
-- Foreign keys: user_id references users, scenario_id references scenarios.
-- =============================================================

INSERT INTO game_sessions (user_id, scenario_id, started_at, ended_at, status) VALUES
    -- Aarav plays twice
    (1, 1, '2024-02-01 10:00:00', '2024-02-01 10:25:00', 'completed'),
    (1, 2, '2024-02-05 11:00:00', '2024-02-05 11:45:00', 'completed'),

    -- Priya plays twice
    (2, 1, '2024-02-10 09:00:00', '2024-02-10 09:30:00', 'completed'),
    (2, 3, '2024-02-20 14:00:00', '2024-02-20 14:50:00', 'completed'),

    -- Rohan plays and abandons one
    (3, 2, '2024-03-01 15:00:00', '2024-03-01 15:10:00', 'abandoned'),
    (3, 4, '2024-03-05 16:00:00', '2024-03-05 16:35:00', 'completed'),

    -- Sneha plays twice
    (4, 3, '2024-03-10 10:00:00', '2024-03-10 11:00:00', 'completed'),
    (4, 1, '2024-03-15 09:30:00', '2024-03-15 10:00:00', 'completed'),

    -- Karan plays once
    (5, 2, '2024-04-01 13:00:00', '2024-04-01 13:40:00', 'completed'),

    -- Ananya has an ongoing session (ended_at is NULL)
    (6, 4, '2024-04-05 17:00:00', NULL, 'ongoing');


-- =============================================================
-- SECTION 4: game_results
-- 8 results for the 8 completed sessions (session IDs 1,2,3,4,6,7,8,9).
-- Sessions 5 (abandoned) and 10 (ongoing) have NO result row.
-- Foreign key: session_id references game_sessions.
-- groundwater values are between 0 and 100 (enforced by CHECK).
-- score and water_saved are non-negative (enforced by CHECK).
-- =============================================================

INSERT INTO game_results (session_id, score, groundwater_before, groundwater_after, water_saved, completed_at) VALUES
    -- session 1: Aarav – Village Drought (Easy)
    (1,  850,  40, 55, 120, '2024-02-01 10:25:00'),

    -- session 2: Aarav – Farmland Crisis (Medium)
    (2, 1200,  35, 58, 180, '2024-02-05 11:45:00'),

    -- session 3: Priya – Village Drought (Easy)
    (3,  780,  40, 50,  90, '2024-02-10 09:30:00'),

    -- session 4: Priya – Industrial Overuse (Hard)
    (4, 1750,  20, 45, 310, '2024-02-20 14:50:00'),

    -- session 6: Rohan – Monsoon Recharge (Easy)
    (6,  920,  30, 60, 200, '2024-03-05 16:35:00'),

    -- session 7: Sneha – Industrial Overuse (Hard)
    (7, 1980,  18, 50, 350, '2024-03-10 11:00:00'),

    -- session 8: Sneha – Village Drought (Easy)
    (8,  870,  42, 57, 130, '2024-03-15 10:00:00'),

    -- session 9: Karan – Farmland Crisis (Medium)
    (9, 1100,  38, 55, 155, '2024-04-01 13:40:00');
