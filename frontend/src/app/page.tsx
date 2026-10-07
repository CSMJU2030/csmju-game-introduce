import { PageHeader, cardClass, primaryButtonClass } from "@/csmju";

// หน้าแรกใช้ title.default จาก layout · หน้าลูกให้ export `metadata = { title: "<ชื่อหน้า>" }`
// แล้วจะได้ "<ชื่อหน้า> · <ชื่อระบบ> · CSMJU" อัตโนมัติ (ui-design-system.md §11.4)

// ตัวอย่างหน้ารายการ — ลบ/แก้ได้ตามระบบของคุณ แต่ให้คงโครง PageHeader + การ์ด + ตาราง + empty state ไว้
export default function OverviewPage() {
  return (
    <>
      <PageHeader title="CSMJU Quest" description="Digital Campus Adventure — สำรวจโลกวิทยาการคอมพิวเตอร์ มหาวิทยาลัยแม่โจ้" />

      <section className={`${cardClass} space-y-6 p-6`}>
        <h2 className="font-display text-headline-md">นักศึกษาใหม่ออกสำรวจโลก CS</h2>
        <p className="text-body-md text-on-surface-variant">สำรวจคณะ ห้องเรียน ห้องแล็บ และสวน IoT ผ่านเควสต์กับรุ่นพี่และอาจารย์ สะสม Skill Badges เพื่อปลดล็อก Open House Day</p>
        <a className={primaryButtonClass} href="/auth/login?next=/">เข้าสู่ระบบผ่าน CSMJU Portal</a>
        <p className="text-body-md text-on-surface-variant">ตัวเกม Unity อยู่ในโฟลเดอร์ unity/ และต้องเตรียม WebGL build ก่อนเปิดให้เล่นบน server</p>
      </section>
    </>
  );
}
