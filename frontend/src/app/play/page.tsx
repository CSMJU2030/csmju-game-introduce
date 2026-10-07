import { cookies } from "next/headers";
import { redirect } from "next/navigation";
import Link from "next/link";
import { cardClass, primaryButtonClass } from "@/csmju";

export const dynamic = "force-dynamic";

export default async function PlayPage() {
  const cookieHeader = (await cookies()).toString();
  const backendUrl = process.env.BACKEND_URL ?? "http://127.0.0.1:4203";
  let response: Response;
  try {
    response = await fetch(`${backendUrl}/api/v1/me`, {
      headers: { Cookie: cookieHeader },
      cache: "no-store",
    });
  } catch {
    return <section className={`${cardClass} space-y-4 p-6`}><h1 className="text-headline-md">เชื่อมต่อระบบไม่ได้</h1><p>ลองเปิดเกมอีกครั้ง</p><a className={primaryButtonClass} href="/play">ลองอีกครั้ง</a></section>;
  }
  if (response.status === 401) redirect("/auth/login?next=/play");
  if (!response.ok) {
    return <section className={`${cardClass} space-y-4 p-6`}><h1 className="text-headline-md">ยังเปิดเกมไม่ได้</h1><p>กรุณากลับหน้าหลักแล้วลองอีกครั้ง</p><Link className={primaryButtonClass} href="/">กลับหน้าหลัก</Link></section>;
  }
  return <iframe title="CSMJU Quest: Digital Campus Adventure" src="/game/index.html" className="fixed inset-0 z-50 h-dvh w-full border-0 bg-background" allow="fullscreen; autoplay" allowFullScreen />;
}
