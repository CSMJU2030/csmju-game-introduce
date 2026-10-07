# CSMJU Quest: Digital Campus Adventure

Unity 2D top-down RPG for introducing Computer Science at Maejo University through quests, NPC conversations, short puzzles and skill badges. This project keeps scene content as editable Unity objects and uses one small WebGL scene.

## Open and edit

1. Open this folder in Unity **6000.6.4f1**.
2. Open `Assets/Scenes/DigitalCampus.unity` from the Project window.
3. The Hierarchy separates `World · Five campus zones`, `Player · Mana Seed Adventurer`, NPCs, `Main Camera`, and `CSMJU Quest · Game Systems`.
4. To rebuild the scene after regenerating the small PNG assets, choose **CSMJU → Build Editable Campus Scene**.

## Play

- WASD or arrow keys: move
- E: talk / interact
- Tab: campus map
- Esc: pause or resume
- The pause menu preserves dialogue, puzzle, and battle state and returns to Core at http://127.0.0.1:3100/.
- Collect the five quest badges, then the Teamwork badge, and interact with the golden Open House gate to see the ending.

The 5 zones are Welcome Zone, Curriculum Hall, Software Lab, AI & Data Cave, and IoT Garden. Each scene zone, its art sprite, the player sprite and NPCs can be selected and edited in the Unity scene. The WebGL build can be recreated with **CSMJU → Build WebGL to Build-WebGL**.

## Web preview on port 3003

After building, serve `Build/WebGL` on port 3003, for example:

```sh
python3 -m http.server 3003 --directory Build/WebGL
```

Then open <http://127.0.0.1:3003/>. For a public site, upload the contents of `Build/WebGL` to a static host that supports Unity WebGL and gzip (or change WebGL compression to disabled and rebuild).

## Assets

See `Assets/Docs/THIRD_PARTY_ASSETS.md` for the character packs, font, and asset notices. The five campus maps use Cute Fantasy RPG Free tiles by Kenmi; see the retained license for non-commercial use.

## Illustrated campus update

The active scene uses `Assets/Art/Campus/campus-map.png`, generated from the supplied reference. NPCs and game systems remain editable scene objects; buildings and plants in this illustration are baked into the map sprite. `CSMJU/Apply Illustrated Campus` configures the campus layout and rebuilds WebGL. The legacy scene-generation menu recreates the older five-panel map. The Campus WebGL template fills the browser viewport; camera bounds adapt to its aspect ratio. Press I to open the badge inventory.

## Surprise battle and interactive puzzles

- Welcome: order sun, cloud, star, moon using the old diary.
- Curriculum: drag (or click then place) Mathematics, Programming, Database into three learning slots.
- AI/Data: rotate a 26-letter cipher wheel according to the generated shift clue.
- IoT: slide a solvable 3×3 circuit board to connect Sensor to Dashboard within 90 seconds. The hint shows the route while time continues; retry creates a new solvable board.
- Software: encounter Bug only through random surprise attacks while exploring. The original RPG battle screen has HP, energy, enemy intents, Debug, Test, Pair Programming, and limited healing patches. Victory awards the Web & Software badge. Defeat allows retry or retreat.

Ambushes count 16–28 seconds of actual movement initially; after a battle there is a 45-second movement cooldown. Maps, inventory, puzzles, dialogue, pause, and a completed six-badge journey suppress ambushes. Esc freezes puzzle and battle progress.

Rebuild with **CSMJU → Build Modern Campus Expansion**. The older Surprise Battle menu points to the same current build. Both preserve the illustrated map and navigation polygons, check puzzle and battle rules, and build WebGL.

## Web build loading (1.4.1)

WebGL assets use content hashes to prevent a browser combining files from different builds. Builds first complete in `Build/WebGL-Staging`; publishing copies the new assets before atomically replacing `index.html`. Older assets remain available for tabs still loading them. The preview disables Unity data caching. Embedded debug symbols identify engine functions if a loading exception recurs. The error panel stays inside the window and offers a reload button.

## Battle and circuit polish (1.5.0)

Circuit segments draw as clipped tile-local rectangles on scaled and Retina displays. Every puzzle has an Exit Quest button; leaving returns to exploration without awarding a badge and talking to the NPC starts a fresh attempt. The pause menu can also exit a quest or retreat from battle.

The battle uses the existing campus illustration, accented status cards, grass arenas and attack particles. The original 144 BPM track “CS Debug Rush” is synthesized in memory with lead, bass and percussion; it pauses with the game, follows the sound toggle, and stops on victory, defeat, retreat or leaving the battle. No music download is required.

## Animated ambush battles (1.6.0)

Bug is inactive in the saved campus scene: its sprite, name and interaction are hidden, while its transform remains a Software Lab zone anchor. Only walking-triggered random ambushes start combat.

Turns animate player wind-up and lunge, Debug/Test projectiles, hit recoil and damage numbers, a team shield for Pair Programming, and healing crosses for Patch. Bug uses eight jumping sprite frames to counterattack. HP changes appear at impact; commands and victory rewards wait until the turn finishes. Esc freezes the animation timeline and music, and retreat cancels the current animation.

## Modern campus expansion (1.7.0)

Use **CSMJU → Build Modern Campus Expansion** to rebuild the current game. Modern Interiors characters have unique player/NPC appearances with four correctly ordered walking directions. Press E at the Maejo 60 building entrance to enter the floor-six classroom layout. Algorithm is inside the left classroom. Press E at the exit mat in the south corridor to return. Floor, furniture, students and walls are editable objects beneath Building60 in the saved scene; portal activation controls their visibility.

Data has three stages: Caesar cipher, sorting six dataset cards into table/text/image bins, and assembling clean → split → train → evaluate. Rewards require all stages. The winged Bug demon has 28 HP, a low-health enrage phase, stronger distinct counterattacks and an energy-draining Memory Leak. Correct Debug/Test deals 6 damage; wrong attacks deal 3. Pair restores 3 energy and guards; three patches heal 8 HP each. Rule checks cover 100 tactical victories. Ambushes are suppressed inside classrooms.

## Reference classroom layout (1.7.1)

The building interior follows the supplied two-room reference: two blocks of seats per room, open central aisles, teaching platforms, side annexes, red/blue door runners and a shared lower lobby with plants. Each room has 16 seats. Unique students stand in clear side bays; Algorithm stands on the left teaching platform. Character foot obstacles prevent walking through NPCs. Interior player scale matches the furniture.

A fixed interior camera frames both rooms and the lobby. The compact top HUD leaves the rooms visible. Build validation checks character/furniture separation and navigable routes from the lobby to Algorithm, both classrooms and both annexes.

## TA Game Boy font (1.7.2)

All Unity in-game labels, dialogue, HUD text, buttons and scene signs use TA Game Boy Regular from TA Font. The supplied OpenType font is stored at `Assets/Resources/Fonts/TAGameboy-Regular.otf` and is loaded as both the regular and heading face so the pixel style stays consistent. Source: https://www.f0nt.com/release/ta-game-boy/.

UI backgrounds and TextMeshPro captions share one ordered Canvas so panels cannot cover their text. Caption sizes are increased with bounded auto-sizing for small controls; HUD panels are wider and NPC names use larger outlined text. `CampusOutlinedText` controls caption layout and `DigitalCampusGame.EnsureStyles` defines base sizes.

World labels and scene signs use TextMeshPro. The HUD, dialogue, puzzle cards and button captions now render through pooled TextMeshProUGUI labels; IMGUI still handles panel graphics and button input. All text uses the supplied TA Game Boy OTF with a dark outline. TMP Essential Resources are included under `Assets/TextMesh Pro`.

## User-authored classroom (1.8.0)

The user's Grid and layer0/layer1/layer2 Tilemaps are preserved in DigitalCampus. E enters at the lower lobby and exits at the same spot. Algorithm stands in the left classroom. Editable BoxCollider2D objects live under Building60 / Collision · authored interior; movement tests the player's foot area against them. Empty space and wall tiles are not walkable.

Use CSMJU → Connect Authored Interior and Font to regenerate collision from the authored layers and reconnect the font. CSMJU → Build Modern Campus Expansion detects the authored Tilemaps and preserves them. Use CSMJU → Build WebGL to Build-WebGL to build saved manual edits without regenerating collision. Scene backups are retained in Assets/SceneBackups.
