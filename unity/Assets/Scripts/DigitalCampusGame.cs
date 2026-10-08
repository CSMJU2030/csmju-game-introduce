using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed partial class DigitalCampusGame : MonoBehaviour
{
    public Transform player;
    public SpriteRenderer playerRenderer;
    public Camera gameCamera;
    public CampusNpc[] npcs = Array.Empty<CampusNpc>();
    public Sprite[] walkSprites = Array.Empty<Sprite>();
    public Font thaiRegular;
    public Font thaiSemibold;

    private enum ScreenState { Title, Playing, Dialogue, Puzzle, Combat, Surprise, Paused, Ending }
    private static readonly string[] BadgeNames = { "Programming Badge", "Database Badge", "Web & Software Badge", "AI/Data Badge", "IoT Badge", "Teamwork Badge" };
    private static readonly string[] ZoneNames = { "Welcome Zone", "Curriculum Hall", "Software Lab", "AI & Data Cave", "IoT Garden" };
    private readonly HashSet<string> badges = new HashSet<string>();
    private readonly HashSet<string> completed = new HashSet<string>();
    private ScreenState state = ScreenState.Title;
    private CampusNpc activeNpc;
    private int dialogueLine;
    private ScreenState resumeState;
    private string feedback = "", toast = "";
    private float toastUntil, moveSpeed = 4.2f, lastFootstep, lastUiSfx;
    private int facing = 0, walkFrame, activeZone = -1;
    private bool inventoryOpen;
    private bool mapOpen, audioOn = true, wasWalking;
    public Vector2 mapMin = new Vector2(-35f, -5.5f);
    public Vector2 mapMax = new Vector2(35f, 5.5f);
    public float cameraViewSize = 5.2f;
    public bool campusOverview;
    public Rect[] walkableAreas = Array.Empty<Rect>();
    public PolygonCollider2D[] walkableSurfaces = Array.Empty<PolygonCollider2D>();
    public PolygonCollider2D[] blockedSurfaces = Array.Empty<PolygonCollider2D>();
    public string coreHomeUrl = "/portal";
#if UNITY_WEBGL && !UNITY_EDITOR
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void CampusReturnToCore(string url);
#endif
    private float viewWidth = 1280f, viewHeight = 720f;
    private GUIStyle titleStyle, headingStyle, bodyStyle, smallStyle, buttonStyle, badgeStyle;
    private AudioSource audioSource;
    private AudioClip footstepClip, badgeClip;

    private void Start()
    {
        Time.timeScale = 1f;
        thaiRegular = Resources.Load<Font>("Fonts/Prompt-Regular");
        thaiSemibold = thaiRegular;
        if(buildingInterior != null) buildingInterior.SetActive(false);
        foreach(var npc in npcs) if(npc != null && (npc.npcId == "data" || npc.npcId == "curriculum")) npc.gameObject.SetActive(false);
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = false;
        footstepClip = MakeTone("step", 112f, .075f, .055f);
        badgeClip = MakeTone("badge", 784f, .22f, .12f);
        InitializeBattleAudio();
        InitializeCampusAudio();
        foreach (var npc in npcs) if (npc != null && npc.npcId == "bug") npc.gameObject.SetActive(false);
        if (gameCamera == null) gameCamera = Camera.main;
        if (player != null && gameCamera != null) FollowCamera(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (state == ScreenState.Paused) Resume();
            else if (state == ScreenState.Playing || state == ScreenState.Dialogue || state == ScreenState.Puzzle || state == ScreenState.Combat || state == ScreenState.Surprise) Pause();
        }
        if (state == ScreenState.Playing)
        {
            if (Input.GetKeyDown(KeyCode.M)) audioOn = !audioOn;
            if (Input.GetKeyDown(KeyCode.Tab)) mapOpen = !mapOpen;
            if (Input.GetKeyDown(KeyCode.I)) inventoryOpen = !inventoryOpen;
            if (!mapOpen && !inventoryOpen) MovePlayer(); else wasWalking = false;
            TickEncounter();
            var zone = ZoneAt(player.position);
            if (zone != activeZone) { activeZone = zone; ShowToast("เข้าสู่ " + ZoneNames[zone], 2.2f); }
            if (state == ScreenState.Playing && !mapOpen && !inventoryOpen && Input.GetKeyDown(KeyCode.E)) Interact();
        }
        else if (state == ScreenState.Dialogue && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))) AdvanceDialogue();
        TickActivities();
        if (gameCamera != null && player != null) FollowCamera(false);
    }

    private void MovePlayer()
    {
        if (player == null) return;
        var input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (input.sqrMagnitude > 1f) input.Normalize();
        var walking = input.sqrMagnitude > .01f;
        var pos = (Vector2)player.position;
        var start = pos;
        var delta = input * moveSpeed * Mathf.Min(Time.deltaTime, .1f);
        int steps = Mathf.Max(1, Mathf.CeilToInt(delta.magnitude / .06f));
        delta /= steps;
        for (int i = 0; i < steps; i++) {
            if (CanWalk(pos + new Vector2(delta.x, 0))) pos.x += delta.x;
            if (CanWalk(pos + new Vector2(0, delta.y))) pos.y += delta.y;
        }
        walking = (pos - start).sqrMagnitude > .000001f;
        // Authored indoor floor cells already enforce the actual map boundary.
        // Outdoor bounds must not clamp the separate interior coordinates.
        player.position = insideBuilding
            ? new Vector3(pos.x, pos.y, player.position.z)
            : new Vector3(Mathf.Clamp(pos.x, mapMin.x + .35f, mapMax.x - .35f), Mathf.Clamp(pos.y, mapMin.y + .35f, mapMax.y - .35f), player.position.z);
        if(playerBody != null) playerBody.position = player.position;
        if (playerRenderer != null && walkSprites.Length >= 24)
        {
            if (walking)
            {
                if (Mathf.Abs(input.x) > Mathf.Abs(input.y)) facing = input.x < 0 ? 1 : 2;
                else facing = input.y < 0 ? 0 : 3;
                walkFrame = Mathf.FloorToInt(Time.time * 7f) % 6;
                if (audioOn && Time.time >= lastFootstep) { audioSource.PlayOneShot(footstepClip); lastFootstep = Time.time + .38f; }
            }
            else walkFrame = 0;
            playerRenderer.sprite = walkSprites[facing * 6 + walkFrame];
        }
        if (!walking && wasWalking) walkFrame = 0;
        wasWalking = walking;
    }

    private bool CanWalk(Vector2 point)
    {
        if (insideBuilding) return InsideFloor(point);
        // Sprite pivot is at the feet. A small footprint follows the visible ground.
        foreach(var offset in FootOffsets) if(!OnGround(point + offset)) return false;
        return true;
    }
    private bool OnGround(Vector2 point)
    {
        if (blockedSurfaces.Any(s => s != null && s.OverlapPoint(point))) return false;
        if (groundMask != null && groundMask.Blocks(point,mapMin,mapMax)) return false;
        if (walkableSurfaces.Length > 0) return walkableSurfaces.Any(s => s != null && s.OverlapPoint(point));
        return walkableAreas.Length == 0 || walkableAreas.Any(area => area.Contains(point));
    }

    private void FollowCamera(bool snap)
    {
        if (insideBuilding) { gameCamera.orthographicSize = Mathf.Max(10.8f, 15f / Mathf.Max(.1f, gameCamera.aspect)); gameCamera.transform.position = new Vector3(interiorCameraCenter.x,interiorCameraCenter.y,-10); return; }
        // Fit the camera viewport inside the map at every browser aspect ratio.
        var half = (mapMax - mapMin) * .5f;
        var aspect = Mathf.Max(.1f, gameCamera.aspect);
        gameCamera.orthographicSize = Mathf.Min(cameraViewSize, half.y - .02f, (half.x - .02f) / aspect);
        var extentY = gameCamera.orthographicSize;
        var extentX = extentY * aspect;
        float minX = mapMin.x + extentX, maxX = mapMax.x - extentX;
        float minY = mapMin.y + extentY, maxY = mapMax.y - extentY;
        var target = new Vector3(Mathf.Clamp(player.position.x, minX, maxX), Mathf.Clamp(player.position.y, minY, maxY), -10f);
        var next = snap ? target : Vector3.Lerp(gameCamera.transform.position, target, 1f - Mathf.Exp(-5f * Time.unscaledDeltaTime));
        // Clamp after smoothing as well, particularly on window resize.
        gameCamera.transform.position = new Vector3(Mathf.Clamp(next.x, minX, maxX), Mathf.Clamp(next.y, minY, maxY), -10f);
    }

    private int ZoneAt(Vector2 position)
    {
        if (insideBuilding || buildingInterior != null && Vector2.Distance(position, buildingEntrance) < 3.4f) return 1;
        if (!campusOverview) return Mathf.Clamp(Mathf.FloorToInt((position.x - mapMin.x) / 14f), 0, 4);
        int closest = 0; float distance = float.MaxValue;
        for (int i = 0; i < Mathf.Min(5, npcs.Length); i++)
        {
            if (npcs[i] == null) continue;
            float d = Vector2.SqrMagnitude(position - (Vector2)npcs[i].transform.position);
            if (d < distance) { closest = i; distance = d; }
        }
        return closest;
    }

    private void Interact()
    {
        if (TryBuildingPortal()) return;
        CampusNpc nearest = null;
        var nearestDistance = 2f;
        foreach (var npc in npcs)
        {
            if (npc == null || !npc.gameObject.activeInHierarchy || npc.npcId == "bug") continue;
            var d = Vector2.Distance(player.position, npc.transform.position);
            if (d < nearestDistance) { nearest = npc; nearestDistance = d; }
        }
        if (nearest == null) { ShowToast("เดินเข้าใกล้ NPC แล้วกด E เพื่อคุย", 2f); return; }
        if (nearest.npcId == "openhouse")
        {
            if (badges.Count < 6) { ShowToast("Open House ยังไม่พร้อม · เก็บ Skill Badge ให้ครบ 6 ชิ้นก่อน", 3f); return; }
            state = ScreenState.Ending; PlayUiSfx(); return;
        }
        if (completed.Contains(nearest.npcId)) { ShowToast("คุณทำภารกิจของ " + nearest.displayName + " แล้ว", 2f); return; }
        activeNpc = nearest; dialogueLine = 0; state = ScreenState.Dialogue; PlayUiSfx();
    }

    private string[] LinesFor(string id)
    {
        switch (id)
        {
            case "mentor": return new[] { "ยินดีต้อนรับสู่สาขาวิทยาการคอมพิวเตอร์ มหาวิทยาลัยแม่โจ้! ลองสำรวจ 5 โซนและเก็บ Skill Badge ให้ครบ 6 ชิ้นนะ", "เดินด้วย WASD หรือปุ่มลูกศร กด E เพื่อคุย กด Tab เพื่อเปิดแผนที่ และกด Esc เพื่อหยุดเกม", "เปิดบันทึกเก่า แล้วเรียงสัญลักษณ์ตามเรื่องราวเพื่อรับ Programming Badge" };
            case "curriculum": return new[] { "หลักสูตร วท.บ. วิทยาการคอมพิวเตอร์ เรียน 4 ปี รวม 120 หน่วยกิต", "วางพื้นฐานให้ครบทั้ง Programming, Mathematics และ Database ก่อนต่อยอดสู่ทักษะเฉพาะทาง", "ลากการ์ดวิชาลงช่องแผนการเรียน: คณิตศาสตร์ → Programming → Database (เป็นแผนฝึกในเกม)" };
            case "bug": return new[] { "ฮ่า ๆ! น้องบั๊กทำเว็บชมรมพังแล้ว ถ้าแน่จริงก็มาดวล Debug กัน!", "ใช้ทักษะคิดเป็นขั้นตอน เลือกวิธีตรวจปัญหาให้ถูกก่อน Bug HP จะหมด", "ชนะแล้วจะได้รับ Full-stack Apprentice · Web & Software Badge" };
            case "data": return new[] { "ช่วยจัดข้อมูลกิจกรรมของนักศึกษาให้หน่อย มีทั้งรูปภาพ ตาราง และข้อความรีวิว", "ภารกิจมี 3 ขั้น: ถอดรหัส → แยกประเภทข้อมูล → วาง pipeline โดยแบ่งข้อมูลก่อนฝึกโมเดล", "จำไว้ว่า AI ช่วยเสนอผลได้ แต่คนต้องตรวจสอบและอธิบายผลเสมอ" };
            default: return new[] { "ยินดีต้อนรับสู่ IoT Garden! ลองต่อเซนเซอร์วัดแสงและความชื้นกลับไปยัง Dashboard", "เส้นทางข้อมูลคือ Sensor → Microcontroller → Cloud/API → Dashboard", "เลื่อนแผ่นสายไฟ 3×3 ให้ข้อมูลเดินทางจาก Sensor ไปยัง Dashboard ก่อนหมดเวลา" };
        }
    }

    private void AdvanceDialogue()
    {
        dialogueLine++;
        PlayUiSfx();
        if (dialogueLine < LinesFor(activeNpc.npcId).Length) return;
        if (activeNpc.npcId == "bug") BeginEncounter();
        else BeginPuzzle();
    }

    private void FinishNpc(string id)
    {
        var badge = id == "mentor" ? BadgeNames[0] : id == "curriculum" ? BadgeNames[1] : id == "data" ? BadgeNames[3] : BadgeNames[4];
        completed.Add(id); badges.Add(badge); state = ScreenState.Playing; activeNpc = null; AwardTeamworkIfReady();
        ShowToast("ได้รับ " + badge, 3f); if (audioOn) audioSource.PlayOneShot(badgeClip);
    }

    private void FinishBugFight()
    {
        encounterCountdown = 45f; completed.Add("bug"); badges.Add(BadgeNames[2]); state = ScreenState.Playing; activeNpc = null; AwardTeamworkIfReady();
        ShowToast("ชนะน้องบั๊ก! ได้ Full-stack Apprentice · Web & Software Badge", 4f); if (audioOn) audioSource.PlayOneShot(badgeClip);
    }

    private void AwardTeamworkIfReady()
    {
        if (completed.Count == 5 && badges.Add(BadgeNames[5])) ShowToast("ทีมช่วยกันสำเร็จ! ได้ Teamwork Badge · Open House ปลดล็อก", 4f);
    }

    private void Pause() { if (state == ScreenState.Paused) return; resumeState = state; state = ScreenState.Paused; Time.timeScale = 0f; }
    private void Resume() { state = resumeState; Time.timeScale = 1f; }
    private void GoToCore()
    {
        Time.timeScale = 1f;
#if UNITY_WEBGL && !UNITY_EDITOR
        CampusReturnToCore(coreHomeUrl);
#else
        Application.OpenURL(coreHomeUrl);
#endif
        state = ScreenState.Title;
    }
    private void StartGame() { encounterCountdown = UnityEngine.Random.Range(16f, 28f); state = ScreenState.Playing; Time.timeScale = 1f; if (player != null) FollowCamera(true); ShowToast("เควสต์ 1: รายงานตัวกับพี่โค้ด", 3f); }

    private void ShowToast(string message, float seconds) { toast = message; toastUntil = Time.unscaledTime + seconds; }
    private void PlayUiSfx() { if (audioOn && audioSource != null && Time.unscaledTime - lastUiSfx > .05f) { audioSource.PlayOneShot(footstepClip, .65f); lastUiSfx = Time.unscaledTime; } }

    private void OnGUI()
    {
        EnsureStyles();
        CampusOutlinedText.Begin();
        var safe = Screen.safeArea;
        float scale = Mathf.Min(safe.width / 1280f, safe.height / 720f);
        viewWidth = Screen.width / scale; viewHeight = Screen.height / scale;
        var centered = Matrix4x4.TRS(new Vector3(safe.x + (safe.width - 1280f * scale) * .5f, Screen.height - safe.yMax + (safe.height - 720f * scale) * .5f, 0), Quaternion.identity, new Vector3(scale, scale, 1));
        GUI.matrix = centered;
        if (state == ScreenState.Title) { DrawTitle(); CampusOutlinedText.End(); return; }
        GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1));
        if (state != ScreenState.Combat && state != ScreenState.Surprise) DrawHud();
        DrawMiniMap();
        GUI.matrix = Matrix4x4.identity;
        DrawLifeOverlay();
        GUI.matrix = centered;
        if (mapOpen && state == ScreenState.Playing) DrawMap();
        if (state == ScreenState.Playing) DrawBuildingHint();
        if (state == ScreenState.Dialogue) DrawDialogue();
        if (state == ScreenState.Puzzle) DrawPuzzle();
        if (state == ScreenState.Combat) DrawCombat();
        if (state == ScreenState.Surprise) DrawSurprise();
        if (state == ScreenState.Paused) DrawPause();
        if (state == ScreenState.Ending) DrawEnding();
        if (Time.unscaledTime < toastUntil && state == ScreenState.Playing) { Panel(insideBuilding ? new Rect(350, 78, 580, 28) : new Rect(350, 612, 580, 64)); CampusOutlinedText.Label(insideBuilding ? new Rect(365, 79, 550, 26) : new Rect(370, 624, 540, 42), toast, insideBuilding ? smallStyle : bodyStyle); }
        CampusOutlinedText.End();
    }

    private void DrawTitle()
    {
        Dim(); Panel(new Rect(220, 78, 840, 570));
        CampusOutlinedText.Label(new Rect(285, 122, 710, 60), "CSMJU QUEST", titleStyle);
        CampusOutlinedText.Label(new Rect(285, 184, 710, 46), "Digital Campus Adventure", headingStyle);
        CampusOutlinedText.Label(new Rect(285, 246, 710, 190), "รับบทนักศึกษาใหม่ สำรวจโลก CS บนชั้น 6 อาคารแม่โจ้ 60 ปี\nเดินทางผ่าน 5 โซน พบรุ่นพี่และอาจารย์ ทำเควสต์ และสะสม Skill Badge ให้ครบ 6 ชิ้น\n\nProgramming · Database · Full-stack · AI/Data · IoT · Teamwork", bodyStyle);
        CampusOutlinedText.Label(new Rect(285, 444, 710, 50), "WASD / ลูกศร เดิน   •   E คุย   •   Tab แผนที่   •   Esc หยุดเกม", smallStyle);
        CampusOutlinedText.Label(new Rect(285, 607, 710, 26), "Campus: AI-assisted · Modern Interiors: LimeZu · Battlers: supplied pack", smallStyle);
        if (CampusOutlinedText.Button(new Rect(464, 530, 352, 60), "เริ่มผจญภัย", buttonStyle)) StartGame();
    }

    private void DrawHud()
    {
        if (insideBuilding) { DrawInteriorHud(); return; }
        Panel(new Rect(16, 16, 350, 108));
        CampusOutlinedText.Label(new Rect(34, 26, 275, 30), "CSMJU QUEST", headingStyle);
        CampusOutlinedText.Label(new Rect(34, 74, 172, 34), "Badges  " + badges.Count + "/6", badgeStyle);
        if (CampusOutlinedText.Button(new Rect(218, 68, 132, 40), "แบดจ์ [I]", buttonStyle)) inventoryOpen = !inventoryOpen;
        if (inventoryOpen)
        {
            Panel(new Rect(16, 125, 304, 228));
            for (int i = 0; i < BadgeNames.Length; i++)
                CampusOutlinedText.Label(new Rect(34, 142 + i * 32, 273, 30), (badges.Contains(BadgeNames[i]) ? "◆ " : "◇ ") + BadgeNames[i], badges.Contains(BadgeNames[i]) ? badgeStyle : smallStyle);
        }
        Panel(new Rect(viewWidth - 390, 16, 374, 156));
        CampusOutlinedText.Label(new Rect(viewWidth - 370, 30, 334, 34), ZoneNames[Mathf.Max(0, activeZone)], badgeStyle);
        CampusOutlinedText.Label(new Rect(viewWidth - 370, 72, 334, 32), "ภารกิจสำเร็จ " + completed.Count + "/5", smallStyle);
        if (CampusOutlinedText.Button(new Rect(viewWidth - 370, 116, 158, 40), "แผนที่ [Tab]", buttonStyle)) mapOpen = !mapOpen;
        if (CampusOutlinedText.Button(new Rect(viewWidth - 200, 116, 164, 40), "หยุด [Esc]", buttonStyle)) Pause();
        Panel(new Rect(16, viewHeight - 52, 640, 40));
        CampusOutlinedText.Label(new Rect(30, viewHeight - 47, 615, 32), "WASD เดิน · E คุย · I แบดจ์ · Tab แผนที่", smallStyle);
    }

    private void DrawMap()
    {
        Dim(); Panel(new Rect(250,90,780,540));
        CampusOutlinedText.Label(new Rect(285,112,720,44),"แผนที่ Digital Campus",titleStyle);
        if(campusMapTexture!=null && !insideBuilding) DrawCampusMap(new Rect(280,180,720,405),false);
        else CampusOutlinedText.Label(new Rect(300,205,670,190),"ชั้น 6 อาคารแม่โจ้ 60 ปี\nห้องซ้าย: อาจารย์ Algorithm\nห้องขวา: พี่ดาต้า\nทางออกอยู่โถงด้านล่าง",bodyStyle);
        if(CampusOutlinedText.Button(new Rect(520,584,240,36),"กลับไปเล่น",buttonStyle)) mapOpen=false;
    }

    private void DrawDialogue()
    {
        Dim(); Panel(new Rect(48, 478, 1184, 214)); CampusOutlinedText.Label(new Rect(82, 497, 1060, 42), activeNpc.displayName + " · " + activeNpc.role, headingStyle);
        CampusOutlinedText.Label(new Rect(82, 548, 1080, 82), LinesFor(activeNpc.npcId)[dialogueLine], bodyStyle);
        CampusOutlinedText.Label(new Rect(82, 640, 940, 34), "เควสต์: " + (activeNpc.npcId == "bug" ? "ต่อสู้และ debug เว็บชมรม" : activeNpc.role), badgeStyle);
        if (CampusOutlinedText.Button(new Rect(1010, 632, 180, 48), dialogueLine + 1 < LinesFor(activeNpc.npcId).Length ? "ถัดไป" : "เริ่มภารกิจ", buttonStyle)) AdvanceDialogue();
    }

    private void DrawPause()
    {
        Dim(); Panel(new Rect(350, 92, 580, 540)); CampusOutlinedText.Label(new Rect(420, 125, 440, 60), "หยุดเกมชั่วคราว", titleStyle);
        if (CampusOutlinedText.Button(new Rect(430, 220, 420, 62), "เล่นต่อ", buttonStyle)) Resume();
        if (CampusOutlinedText.Button(new Rect(430, 290, 420, 54), audioOn ? "ปิดเสียง" : "เปิดเสียง", buttonStyle)) audioOn = !audioOn;
        GUI.enabled = resumeState == ScreenState.Playing;
        if (CampusOutlinedText.Button(new Rect(430, 356, 420, 54), "แผนที่", buttonStyle)) { Resume(); mapOpen = true; }
        GUI.enabled = resumeState == ScreenState.Puzzle || resumeState == ScreenState.Dialogue || resumeState == ScreenState.Combat;
        if (CampusOutlinedText.Button(new Rect(430, 422, 420, 54), "ออกจากภารกิจ / กลับไปสำรวจ", buttonStyle)) ExitActivity();
        GUI.enabled = true;
        if (CampusOutlinedText.Button(new Rect(430, 488, 420, 54), "กลับหน้าหลัก Core", buttonStyle)) GoToCore();
        CampusOutlinedText.Label(new Rect(430, 548, 420, 42), "คีย์ Esc เพื่อเล่นต่อได้", smallStyle);
    }

    private void DrawEnding()
    {
        Dim(); Panel(new Rect(220, 90, 840, 540)); CampusOutlinedText.Label(new Rect(290, 130, 700, 58), "ยินดีด้วย คุณจบภารกิจแล้ว", titleStyle);
        CampusOutlinedText.Label(new Rect(290, 216, 700, 198), "คุณสะสม Skill Badge ครบ 6 ชิ้นและกลายเป็นรุ่นพี่\nพาเด็ก ม.ปลายสำรวจโลก CS ที่แม่โจ้\n\nProgramming · Database · Full-stack · AI/Data · IoT · Teamwork", bodyStyle);
        if (CampusOutlinedText.Button(new Rect(430, 500, 200, 58), "เล่นต่อ", buttonStyle)) state = ScreenState.Playing;
        if (CampusOutlinedText.Button(new Rect(650, 500, 270, 58), "กลับหน้าหลัก Core", buttonStyle)) GoToCore();
    }

    private void EnsureStyles()
    {
        if (titleStyle != null) return;
        titleStyle = MakeStyle(thaiSemibold, 36, Color.white); headingStyle = MakeStyle(thaiSemibold, 24, Color.white); bodyStyle = MakeStyle(thaiRegular, 22, Color.white); smallStyle = MakeStyle(thaiRegular, 18, new Color(.9f, .95f, .9f)); badgeStyle = MakeStyle(thaiSemibold, 19, new Color(1f, .84f, .27f));
        buttonStyle = new GUIStyle(GUI.skin.button) { font = thaiSemibold, fontSize = 21, alignment = TextAnchor.MiddleCenter, wordWrap = true };
    }
    private GUIStyle MakeStyle(Font font, int size, Color color) => new GUIStyle(GUI.skin.label) { font = font, fontSize = size, wordWrap = true, normal = { textColor = color } };
    private static void Dim() { GUI.color = new Color(.015f, .035f, .025f, .78f); var safe = Screen.safeArea;
        float scale = Mathf.Min(safe.width / 1280f, safe.height / 720f); float w = Screen.width / scale, h = Screen.height / scale; CampusOutlinedText.Picture(new Rect((1280f-w)*.5f, (720f-h)*.5f, w, h), Texture2D.whiteTexture); GUI.color = Color.white; }
    private static void Panel(Rect rect) { GUI.color = new Color(.025f, .13f, .10f, .96f); CampusOutlinedText.Picture(rect, Texture2D.whiteTexture); GUI.color = Color.white; }
    private static Color ZoneColor(int zone) => zone switch { 0 => new Color(.12f,.4f,.24f), 1 => new Color(.5f,.32f,.17f), 2 => new Color(.15f,.32f,.44f), 3 => new Color(.32f,.23f,.48f), _ => new Color(.16f,.43f,.24f) };

    private static AudioClip MakeTone(string name, float hz, float duration, float volume)
    {
        const int rate = 22050; var count = Mathf.CeilToInt(rate * duration); var samples = new float[count];
        for (var i = 0; i < count; i++) { var t = i / (float)rate; samples[i] = Mathf.Sin(t * hz * Mathf.PI * 2f) * volume * Mathf.Sin(Mathf.PI * i / count); }
        var clip = AudioClip.Create(name, count, 1, rate, false); clip.SetData(samples, 0); return clip;
    }
}
