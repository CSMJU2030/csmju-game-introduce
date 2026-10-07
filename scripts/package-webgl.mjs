// node scripts/package-webgl.mjs /absolute/path/to/Unity/Build/WebGL-Staging
import { readFileSync, writeFileSync, mkdirSync, rmSync, readdirSync, copyFileSync } from "node:fs";
import { resolve, join, relative } from "node:path";
import { gunzipSync } from "node:zlib";
import { createHash } from "node:crypto";

const source = resolve(process.argv[2] ?? "unity/Build/WebGL-Staging");
const target = resolve("frontend/public/game");
let html = readFileSync(join(source, "index.html"), "utf8");
const paths = [...html.matchAll(/(?:dataUrl|frameworkUrl|codeUrl):\s*'([^']+)'|script\.src\s*=\s*'([^']+)'/g)].map(m => m[1] ?? m[2]);
if (paths.length !== 4 || paths.some(p => !p.startsWith("Build/") || p.includes(".."))) throw Error("Unexpected Unity entry point");
// Read everything first; a broken build must not replace the packaged release.
const files = paths.map(p => [p, readFileSync(join(source, p))]);
const font = readFileSync(join(source, "TAGameboy-Regular.otf"));
const frameworkPath = paths.find(p => p.includes(".framework.js"));
let framework = files.find(([p]) => p === frameworkPath)[1];
if (frameworkPath.endsWith(".unityweb")) framework = gunzipSync(framework);
let js = framework.toString();
js = js.replace(/function _CampusReturnToCore\(url\)\{[^}]*\}/, 'function _CampusReturnToCore(url){window.top.location.assign("/portal")}');
if (!js.includes('window.top.location.assign("/portal")')) throw Error("Missing Core return bridge");
rmSync(target, { recursive: true, force: true });
mkdirSync(join(target, "Build"), { recursive: true });
for (const [p, bytes] of files) if (p !== frameworkPath) writeFileSync(join(target, p), bytes);
writeFileSync(join(target, "Build/campus.framework.js"), js);
html = html.replace(frameworkPath, "Build/campus.framework.js");
writeFileSync(join(target, "index.html"), html);
writeFileSync(join(target, "TAGameboy-Regular.otf"), font);
copyFileSync("unity/Assets/Docs/THIRD_PARTY_ASSETS.md", join(target, "CREDITS.md"));
const manifest = {};
function walk(dir) {
  for (const item of readdirSync(dir, { withFileTypes: true })) {
    const p = join(dir, item.name);
    if (item.isDirectory()) walk(p);
    else manifest[relative(target, p)] = createHash("sha256").update(readFileSync(p)).digest("hex");
  }
}
walk(target);
writeFileSync(join(target, "build-manifest.json"), JSON.stringify({ files: manifest }, null, 2) + "\n");
console.log(`Packaged ${Object.keys(manifest).length} files at ${target}`);
