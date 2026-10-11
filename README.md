# csmju-game-introduce

CSMJU Introduce: Digital Campus Adventure — เกม Unity 2D RPG แนะนำสาขาวิทยาการคอมพิวเตอร์ มหาวิทยาลัยแม่โจ้

โปรเจกต์เกมอยู่ใน `unity/` นำ asset ที่มีสิทธิ์ใช้งานกลับเข้าตาม [Asset setup](unity/ASSET_SETUP.md) ก่อน จากนั้นเปิดโฟลเดอร์นี้ผ่าน Unity Hub ด้วย Unity 6000.6.4f1 แล้วเปิด `Assets/Scenes/DigitalCampus.unity`.

บิวด์เว็บจากเมนู **CSMJU → Build WebGL to Build-WebGL** ผลลัพธ์อยู่ที่ `unity/Build/WebGL/` และต้องเสิร์ฟผ่าน HTTP เพื่อเล่นบนเว็บ ดูรายละเอียดเกมและ asset notices ใน [คู่มือ Unity](unity/README.md) และ `unity/Assets/Docs/THIRD_PARTY_ASSETS.md`.

ไฟล์แคช บิวด์ และข้อมูลส่วนตัวของ Editor ไม่ถูกเก็บใน Git เว็บ Next.js ใช้ template กลาง และ API NestJS ใช้ SSO/JWKS จาก reference implementation

## Deployment (standards 1.8.1)

URL: `https://csmju-game-introduce.jowave.com`

SSO callback: `https://csmju-game-introduce.jowave.com/auth/callback` (ไม่มี slash ปิดท้าย)

PL ต้องลงทะเบียนและขออนุมัติระบบนี้ใน Core ก่อนทดสอบ SSO จนจบ flow. DevOps ต้องตั้ง DNS/HTTPS/reverse proxy ให้ URL นี้ชี้ service `web`. พอร์ตที่จัดสรรให้ระบบคือ **5012**: เปิดในเครื่องที่ `http://localhost:5012` ทั้ง `pnpm --filter frontend dev` และ Docker Compose (`5012:3000`). web/api/db ใช้พอร์ตภายใน 3000/4000/5432 ตามมาตรฐานกลาง ส่วนโดเมนจริงยังใช้ HTTPS URL ด้านบน

```bash
git submodule update --init standards
corepack pnpm install
docker compose up -d --build
docker compose ps
```

`frontend/Dockerfile` คัดลอกทั้งไฟล์จาก template ของ standards v1.8.1 โดยไม่แก้ไข. Backend Dockerfile/entrypoint/dockerignore/compose มาจาก demo ล่าสุด. Pool DB อ่าน `DATABASE_POOL_MAX` ค่าเริ่มต้น 5. ตั้ง env ตาม `.env.example` และ `backend/.env.example`; ห้าม commit secret

หน้าแรกมีปุ่มเริ่มเล่นไป `/play` ซึ่งตรวจ session ผ่าน `/api/v1/me` ก่อนแสดงเกมเต็มหน้าจอ. WebGL build ที่แพ็กไว้ใน `frontend/public/game/` ถูกนำเข้า Docker image อัตโนมัติ. ปุ่มออกเกมกลับ `/portal` ซึ่งอ่าน `CORE_HUB_WEB_URL`.

เมื่อต้องการอัปเดตเกม ให้ build Unity แล้วรันจากราก repository:

```bash
node scripts/package-webgl.mjs /absolute/path/to/Unity/Build/WebGL-Staging
docker compose up -d --build --force-recreate web
```

สคริปต์เก็บเฉพาะไฟล์ที่ build ล่าสุดอ้างอิง พร้อม SHA-256 manifest และ credits; ไม่เผยแพร่ source sprite ที่มีข้อจำกัดสิทธิ์. ไฟล์เกมต้องอยู่ใน commit ก่อน GitHub สร้าง image ใหม่. หน้าเกมต้องคลิกก่อนเบราว์เซอร์อนุญาตเสียง.

ชื่อทีมใน CODEOWNERS และชื่อระบบใน CI รอ DevOps เปลี่ยนตาม Issue #8; ไฟล์ protected คงตรงกับ main เพื่อผ่าน GH-03. ไม่แก้ `.github/workflows/images.yml`

## มาตรฐานกลาง

CSMJU Introduce: Digital Campus Adventure — ระบบย่อยของโครงการ CSMJU2030

มาตรฐานกลางอยู่ใน `standards/` (submodule ของ CSMJU2030/csmju2030-standards)
เลือก standards v1.8.1 ทั้ง `.standards-version` และ submodule tag `v1.8.1`

## เริ่มทำงาน

```bash
git submodule update --init standards/
corepack pnpm install
git checkout -b feature/game-introduce/<เรื่องที่ทำ>
```

ก่อนเปิด PR อ่าน `standards/docs/github-workflow.md` ข้อ 1

### Campus life and battle update

The campus world is scaled to 155% with a following camera and a mini map. The existing campus artwork is retained; authored navigation polygons and a vegetation mask restrict movement to paths. Building 60 uses inset furniture colliders covering all occupied furniture tiles, three distinct students with random destinations and ambient conversation, and nearby quest prompts. Student appearances are shuffled between the computer room, classroom and lobby on each entry; spawn points are reachable from the entrance. The exterior void is a solid color and Unity editing Gizmos are hidden. Buttons play a dedicated menu click sound. Outdoor and indoor music are separate original synthesized loops, with softened footsteps. Battle actions include impact slashes over an animated green binary backdrop.

In Unity, `CSMJU > Apply Campus Life, Navigation and Battle` updates the authored scene. Student routes are validated against floor and furniture geometry before saving. Rebuild WebGL and run `scripts/package-webgl.mjs` to publish the updated game files.

Validation: Unity WebGL build succeeded; all 20 standards checks passed; Chromium loaded the game and battle screen without page errors.

Outdoor rendering uses Order in Layer 0 for ground/paths, 1 for canopy, 2 for the stadium, 3 for its paving connection, 4 for NPCs and 5 for the player, so the colour-masked vegetation cannot hide characters standing on paving. Indoors the orders remain 0 for floors, 1 for NPCs, 2 for the player and 3 for furniture. The illustrated campus remains one source texture; a separate masked canopy renderer selects vegetation from that texture. It is not an individually editable tree Tilemap. Quest labels use order 6 outdoors and 4 indoors; UI uses order 100. Legacy NPC interaction rings have been removed; nearby E prompts identify quest interactions.

Navigation update: the campus uses one connected paving mask with merged obstacle rectangles under `World > Background collision`. Rectangles are triggers for inspection; movement uses the same mask to prevent conflicting physics responses. Existing indoor walls/furniture remain separate. The player moves in four cardinal directions; when two direction keys are held, the most recently pressed axis takes priority. Directional frames are explicitly loaded from Adam in down/left/right/up order.

Building 60 exit interaction is separate from the entry spawn and located in the lower-right lobby near the planter, marked `ทางออก [E]`.

The southeast stage/plaza is replaced by a pixel-art sports stadium with a football pitch, red running track and covered grandstand. `CampusStadiumLayout` defines the matching rounded track footprint, entrance route and solid goal/stand/light/planter bounds. The old flowerbed blocker is removed. `CampusStadiumBuilder.Validate` checks every obstacle and searches from the original player spawn using the full hero footprint to prove the west gate, field and both track straights are reachable. Generated mask colliders remain editable under `World > Background collision`, with named stadium solids alongside them. The overview and mini map show the same stadium overlay. Artwork provenance and the generation prompt are in `unity/Assets/Docs/STADIUM_ART.md`.
