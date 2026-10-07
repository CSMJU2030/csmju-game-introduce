# Game Introduce deployment validation

Validation performed on 2026-10-07 (Asia/Bangkok).

- Initial `run-all-checks`: 19/20 passed. GH-03 failed because the subsystem rename changed `.github/workflows/ci.yml` and `.github/CODEOWNERS`.
- On 2026-10-08, those two protected files were restored to `origin/main`. Their old subsystem/team identifiers require a separate DevOps update. No exception or bypass has been applied.
- DEP-01 through DEP-04 passed.
- Lint, typecheck, builds and 126 authentication unit tests passed.
- `docker compose up -d --build` completed; db, api and web were healthy.
- Container HTTP checks: `/` 200, `/api/health` 200, `/api/v1/me` 401 without authentication, `/auth/login` 302 to Core, `/portal` 307 to Core Hub.
- `.github/workflows/images.yml` remains unchanged.

## Deployment limitations

The frontend is the deployment and SSO shell. A production Unity WebGL build must still be added to serve the playable game. Licensed art excluded from Git must be restored according to `unity/ASSET_SETUP.md` before building.

Core registration is not yet configured. Register `https://csmju-game-introduce.jowave.com/auth/callback` and configure the deployment secrets, DNS and reverse proxy. The public subdomain returned HTTP 503 during validation. End-to-end SSO login has not been verified.

Physical mobile, keyboard and visual Thai-language acceptance checks have not been certified by the user. This report does not assert production readiness.
