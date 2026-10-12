import { PageHeader, cardClass, primaryButtonClass } from "@/csmju";

// หน้าแรกใช้ title.default จาก layout · หน้าลูกให้ export `metadata = { title: "<ชื่อหน้า>" }`
// แล้วจะได้ "<ชื่อหน้า> · <ชื่อระบบ> · CSMJU" อัตโนมัติ (ui-design-system.md §11.4)

// ตัวอย่างหน้ารายการ — ลบ/แก้ได้ตามระบบของคุณ แต่ให้คงโครง PageHeader + การ์ด + ตาราง + empty state ไว้
export default function OverviewPage() {
  return (
    <>
      <PageHeader title="CSMJU Introduce" description="เกมแนะนำสาขาวิทยาการคอมพิวเตอร์ มหาวิทยาลัยแม่โจ้ ผ่านการสำรวจและลงมือทำ" />

      <section className={`${cardClass} space-y-6 p-6`}>
        <h2 className="font-display text-headline-md">ทำไมต้องเล่น CSMJU Introduce?</h2>
        <p className="text-body-md text-on-surface-variant">ลองเป็นนักศึกษาใหม่ก่อนเข้ารั้วมหาวิทยาลัย สำรวจห้องเรียน ห้องแล็บ และสวน IoT พร้อมพูดคุยกับรุ่นพี่และอาจารย์ เพื่อทำความรู้จักโลกของวิทยาการคอมพิวเตอร์ผ่านเกม</p>
        <p className="text-body-md text-on-surface-variant">เควสต์และปริศนาจะชวนฝึกคิดเป็นขั้นตอน ทดลองแก้ปัญหา และรู้จักทักษะ Programming, Database, Web, AI/Data และ IoT ช่วยให้ค้นพบว่าตัวเองสนใจด้านไหน โดยเริ่มเล่นได้แม้ยังไม่มีพื้นฐานเขียนโปรแกรม</p>
        <a className={primaryButtonClass} href="/play">เริ่มเล่น CSMJU Introduce</a>
        <p className="text-body-md text-on-surface-variant">เดินด้วย WASD หรือปุ่มลูกศร · คุยและเข้าตึกด้วย E · หยุดเกมด้วย Esc · เปิดเสียงหลังคลิกในเกม</p>
      </section>
    </>
  );
}
