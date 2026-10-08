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

The initial frontend was the deployment and SSO shell. The playable WebGL update now packages the latest local Unity 1.8.0 build, with source artwork excluded from Git. Restore licensed artwork according to `unity/ASSET_SETUP.md` when rebuilding Unity.

Core registration is not yet configured. Register `https://csmju-game-introduce.jowave.com/auth/callback` and configure the deployment secrets, DNS and reverse proxy. The public subdomain returned HTTP 503 during validation. End-to-end SSO login has not been verified.

Physical mobile, keyboard and visual Thai-language acceptance checks have not been certified by the user. This report does not assert production readiness.

## Playable WebGL validation (2026-10-08)

- All 20 local compliance checks passed, including frontend lint/typecheck/build and the existing auth tests.
- Docker image build passed; the local packaged game entry returns HTTP 200 on port 5012.
- Real Core login succeeded. A local browser smoke test using that verified session reached `/play` and initialized Unity without page errors.
- The browser smoke test is not a full replay of every quest, combat branch or puzzle.
- Runtime game exit navigates the top-level window to `/portal`, using the server's `CORE_HUB_WEB_URL`.
- `scripts/package-webgl.mjs` packages only assets referenced by the latest entry point, preserving required attribution and producing SHA-256 hashes. Unity engine output is excluded from ESLint; handwritten frontend code remains checked.
- Public-server play requires merging this update, building its image, and deploying that image. Earlier public 503/SSO registration limitations were resolved: public home and authenticated `/api/v1/me` returned 200 during the preceding live SSO test.

## Prompt and responsive UI update (2026-10-08)

- Unity WebGL build completed successfully with Prompt TMP for UI/world labels and Prompt for the loading page and Next.js shell.
- Fixed IoT wire images using absolute tile rectangles: Canvas overlay graphics do not inherit IMGUI group clipping/coordinates.
- Modal UI fits the screen safe area; text is constrained to its label bounds.
- Moved Data NPC to an unobstructed indoor floor location in the right classroom; it is hidden outdoors and appears after entering Building 60.
- Browser loaded the new build without page errors. Viewports 1920x1080, 844x390 and 390x844 had matching canvas dimensions and no document overflow. This verifies fit, not touch gameplay or a full quest replay on mobile.
- All 20 local standards checks passed.
