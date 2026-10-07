# csmju-game-introduce

CSMJU Quest: Digital Campus Adventure — เกม Unity 2D RPG แนะนำสาขาวิทยาการคอมพิวเตอร์ มหาวิทยาลัยแม่โจ้

โปรเจกต์เกมอยู่ใน `unity/` นำ asset ที่มีสิทธิ์ใช้งานกลับเข้าตาม [Asset setup](unity/ASSET_SETUP.md) ก่อน จากนั้นเปิดโฟลเดอร์นี้ผ่าน Unity Hub ด้วย Unity 6000.6.4f1 แล้วเปิด `Assets/Scenes/DigitalCampus.unity`.

บิวด์เว็บจากเมนู **CSMJU → Build WebGL to Build-WebGL** ผลลัพธ์อยู่ที่ `unity/Build/WebGL/` และต้องเสิร์ฟผ่าน HTTP เพื่อเล่นบนเว็บ ดูรายละเอียดเกมและ asset notices ใน [คู่มือ Unity](unity/README.md) และ `unity/Assets/Docs/THIRD_PARTY_ASSETS.md`.

ไฟล์แคช บิวด์ และข้อมูลส่วนตัวของ Editor ไม่ถูกเก็บใน Git เว็บ Next.js ใช้ template กลาง และ API NestJS ใช้ SSO/JWKS จาก reference implementation

## Deployment (standards 1.8.1)

URL: `https://csmju-game-introduce.jowave.com`

SSO callback: `https://csmju-game-introduce.jowave.com/auth/callback` (ไม่มี slash ปิดท้าย)

PL ต้องลงทะเบียนและขออนุมัติระบบนี้ใน Core ก่อนทดสอบ SSO จนจบ flow. DevOps ต้องตั้ง DNS/HTTPS/reverse proxy ให้ URL นี้ชี้ service `web`. Compose ไม่เปิดพอร์ตสู่ host; web/api/db ใช้พอร์ตภายใน 3000/4000/5432 ตามมาตรฐานกลาง

```bash
git submodule update --init standards
corepack pnpm install
docker compose up -d --build
docker compose ps
```

`frontend/Dockerfile` คัดลอกทั้งไฟล์จาก template ของ standards v1.8.1 โดยไม่แก้ไข. Backend Dockerfile/entrypoint/dockerignore/compose มาจาก demo ล่าสุด. Pool DB อ่าน `DATABASE_POOL_MAX` ค่าเริ่มต้น 5. ตั้ง env ตาม `.env.example` และ `backend/.env.example`; ห้าม commit secret

สถานะนี้เตรียม deployment shell และ API auth/health เท่านั้น. WebGL build ยังต้องนำเข้าจากโปรเจกต์ Unity ที่มี asset ถูกลิขสิทธิ์ก่อนเปิดให้เล่นบนโดเมนจริง

ชื่อทีมใน CODEOWNERS และชื่อระบบใน CI เปลี่ยนเป็น game-introduce; ต้องให้ DevOps รับรองการเปลี่ยนไฟล์ protected (GH-03). ไม่แก้ `.github/workflows/images.yml`

## มาตรฐานกลาง

CSMJU Quest: Digital Campus Adventure — ระบบย่อยของโครงการ CSMJU2030

มาตรฐานกลางอยู่ใน `standards/` (submodule ของ CSMJU2030/csmju2030-standards)
เลือก standards v1.8.1 ทั้ง `.standards-version` และ submodule tag `v1.8.1`

## เริ่มทำงาน

```bash
git submodule update --init standards/
corepack pnpm install
git checkout -b feature/game-introduce/<เรื่องที่ทำ>
```

ก่อนเปิด PR อ่าน `standards/docs/github-workflow.md` ข้อ 1
