# csmju-game-introduce

CSMJU Quest: Digital Campus Adventure — เกม Unity 2D RPG แนะนำสาขาวิทยาการคอมพิวเตอร์ มหาวิทยาลัยแม่โจ้

โปรเจกต์เกมอยู่ใน `unity/` นำ asset ที่มีสิทธิ์ใช้งานกลับเข้าตาม [Asset setup](unity/ASSET_SETUP.md) ก่อน จากนั้นเปิดโฟลเดอร์นี้ผ่าน Unity Hub ด้วย Unity 6000.6.4f1 แล้วเปิด `Assets/Scenes/DigitalCampus.unity`.

บิวด์เว็บจากเมนู **CSMJU → Build WebGL to Build-WebGL** ผลลัพธ์อยู่ที่ `unity/Build/WebGL/` และต้องเสิร์ฟผ่าน HTTP เพื่อเล่นบนเว็บ ดูรายละเอียดเกมและ asset notices ใน [คู่มือ Unity](unity/README.md) และ `unity/Assets/Docs/THIRD_PARTY_ASSETS.md`.

ไฟล์แคช บิวด์ และข้อมูลส่วนตัวของ Editor ไม่ถูกเก็บใน Git ส่วนโครง backend/frontend เดิมยังเก็บไว้สำหรับการเชื่อม Core ในอนาคต

## โครงระบบย่อยเดิม

CS SkillMap — ระบบย่อยของโครงการ CSMJU2030

มาตรฐานกลางอยู่ใน `standards/` (submodule ของ CSMJU2030/csmju2030-standards)
สร้างจาก standards v1.0.0

## เริ่มทำงาน

```bash
git submodule update --init --remote standards/
pnpm install
git checkout -b feature/skill-map/<เรื่องที่ทำ>
```

ก่อนเปิด PR อ่าน `standards/docs/github-workflow.md` ข้อ 1
