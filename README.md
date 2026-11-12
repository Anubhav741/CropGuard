# CropGuard — Plant Disease Rescue

CropGuard combines an existing Unity 2D farming game with a React/Vite player portal, an Express API, and PostgreSQL through Prisma. The Unity project remains at the repository root (`Assets`, `Packages`, `ProjectSettings`), so the existing Unity Hub project is not moved or overwritten. The web application is in `frontend/`; the API and schema are in `backend/`.

## Local setup

Requirements: Node 20+, PostgreSQL, Unity 6000.5.10f1 and its WebGL build module.

1. Create a new empty PostgreSQL database. Never run these commands against an existing shared database without reviewing the migration.
2. Copy `backend/.env.example` to `backend/.env`. Set `DATABASE_URL`, a unique 32+ character `JWT_SECRET`, `PORT`, and `FRONTEND_URL` (the exact frontend origin).
3. In `backend`, run `npm ci`, `npx prisma generate`, `npx prisma migrate deploy`, and `node prisma/seed.js`. Start with `npm run dev`.
4. Copy `frontend/.env.example` to `frontend/.env`; set `VITE_API_URL` to the backend origin. In `frontend`, run `npm ci` and `npm run dev`.
5. Open the repository root in Unity Hub. Run the editor method `WebGLBuild.Build` in batch mode or build WebGL to `frontend/public/game`. The React loader expects `frontend/public/game/Build/CropGuard.loader.js`, `CropGuard.data`, `CropGuard.framework.js`, and `CropGuard.wasm`. The build helper disables compression so a static host need not configure compressed file headers. WebGL build output is intentionally ignored by Git: publish it separately with the frontend build.

Example build, after Unity and WebGL support are installed:

```sh
/path/to/Unity -batchmode -projectPath "$PWD" -buildTarget WebGL -executeMethod WebGLBuild.Build -quit -logFile webgl-build.log
```

Run `npm run build` in `frontend` for production; serve `frontend/dist` over HTTPS. `_redirects` supplies a Netlify SPA fallback; configure equivalent `/* -> /index.html` rewrite on other hosts. Deploy the API to a Node host such as Render or Railway with `DATABASE_URL`, `JWT_SECRET`, `PORT`, and `FRONTEND_URL`; use HTTPS, restrictive CORS, and a managed PostgreSQL database. Do not commit `.env` files or secrets.

## Gameplay and online integration

Use A/D or arrows to move, Space to jump, E to inspect and F to apply a correct solution. Level 2 has a 60-second timer; Level 3 is a Unity-shape farm. The web portal sends the user's JWT to `CropGuardOnline` via Unity `SendMessage`, starts an authenticated session at each level, and submits the result at completion or failure. Browser status reports online-save errors; the game continues offline. The game does not store a JWT signing key or database credentials. Unity WebGL/browser functionality depends on successfully generating the build described above.

## REST API

All responses are JSON; failures use `{ "error": "..." }`. Authentication is `Authorization: Bearer <token>`.

| Method | Route | Authentication | Purpose |
| --- | --- | --- | --- |
| POST | `/api/auth/register` | No | `{name,email,password}`; returns user and JWT |
| POST | `/api/auth/login` | No | `{email,password}`; returns user and JWT |
| GET | `/api/auth/me` | Yes | Current account |
| POST | `/api/game/start` | Yes | `{levelId}`; returns `sessionId` |
| POST | `/api/game/complete` | Yes | `{sessionId,levelId,score,cropsTreated,livesRemaining,duration,completed}` |
| GET | `/api/game/progress` | Yes | Per-level progress |
| GET | `/api/game/levels` | Yes | Level metadata and unlocks |
| GET | `/api/game/stats` | Yes | Game count, best score, crops, completed levels |
| GET | `/api/game/history?page=1&limit=10` | Yes | Paginated sessions |
| GET | `/api/game/leaderboard?page=1&limit=10` | No | Paginated public rankings |

Sessions belong to the authenticated user; a result is accepted once in a transaction. The server checks level unlocks, game ownership, crop limits, score arithmetic and duration against server time. **These checks limit invalid submissions but cannot prove a browser client actually played honestly.** A tamper-resistant leaderboard requires server-authoritative gameplay or independent verification; treat current rankings as unverified.

## Tests

After migrating and seeding a disposable test PostgreSQL database, set `DATABASE_URL` and run `npm test` in `backend`. The API test creates and deletes only its own uniquely named test account. Run `npm run build` in `frontend`. No production database is touched by tests.

## Current limitations

The database schema was created for this project because no CropGuard DE schema was found in the repository branches. Align it with any later official DE design before migrating a shared database. Sessions for abandoned games can remain ACTIVE. Online saves that fail are reported but are not queued for retry. Client-generated gameplay results remain untrusted. The original Unity game has a static character sprite, simplified UI/art and incomplete end-to-end playtesting. The crop questions are educational, not a field diagnosis; see [UMN Extension](https://extension.umn.edu/garden-and-home/yard-and-garden/gardening-in-minnesota/yard-and-garden-problems/tomato-leaf-spot-diseases).
