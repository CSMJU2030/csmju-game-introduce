import { NextResponse } from "next/server";

export function GET() {
  const coreHubUrl = process.env.CORE_HUB_WEB_URL;
  if (!coreHubUrl) return new Response("Portal configuration unavailable", { status: 503 });
  return NextResponse.redirect(coreHubUrl);
}
