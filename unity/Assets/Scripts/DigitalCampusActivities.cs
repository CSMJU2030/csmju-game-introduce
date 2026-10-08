using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class DigitalCampusGame
{
    public Sprite battleBackdrop;
    public Sprite[] bugAttackSprites = Array.Empty<Sprite>();
    public Sprite[] bugSprites = Array.Empty<Sprite>();
    private float encounterCountdown = 22f, surpriseRemaining, battleAnimation;
    private BugBattle battle;
    private WirePuzzle wire;
    private CipherPuzzle cipher;
    private readonly List<int> symbols = new List<int>();
    private readonly int[] courseSlots = { -1, -1, -1 };
    private int selectedCard = -1, draggedCard = -1;
    private bool showWireHint;
    private Texture2D circle;
    private readonly string[] courses = { "Programming", "Mathematics", "Database", "IoT ขั้นสูง" };
    private readonly string[] symbolNames = { "พระอาทิตย์", "พระจันทร์", "ดวงดาว", "ก้อนเมฆ" };

    private void TickEncounter()
    {
        if (insideBuilding || state != ScreenState.Playing || !wasWalking || mapOpen || inventoryOpen || badges.Count == 6) return;
        encounterCountdown -= Time.deltaTime;
        if (encounterCountdown <= 0) BeginEncounter();
    }
    private void BeginEncounter()
    {
        mapOpen = inventoryOpen = false; state = ScreenState.Surprise;
        surpriseRemaining = 1.3f; battle = new BugBattle(Environment.TickCount);
        ResetBattleTurn();
        feedback = "น้องบั๊กจู่โจมคุณ! ปกป้องระบบชมรม CS";
        if (audioOn) audioSource.PlayOneShot(badgeClip, .6f);
    }
    private void TickActivities()
    {
        SyncBattleMusic();
        SyncCampusAudio();
        if (state == ScreenState.Combat) TickBattleTurn();
        if (state == ScreenState.Surprise) { surpriseRemaining -= Time.deltaTime; if (surpriseRemaining <= 0) state = ScreenState.Combat; }
        if (state == ScreenState.Combat) battleAnimation += Time.deltaTime;
        if (state == ScreenState.Puzzle && activeNpc != null && activeNpc.npcId == "iot" && wire.Remaining > 0)
        {
            wire.Remaining = Mathf.Max(0, wire.Remaining - Time.deltaTime);
            if (wire.Remaining == 0) feedback = "หมดเวลาแล้ว ลองจัดสายไฟใหม่ได้ ไม่เสีย Badge";
        }
    }
    private void BeginPuzzle()
    {
        ResetDataQuest();
        state = ScreenState.Puzzle; feedback = ""; symbols.Clear(); selectedCard = draggedCard = -1;
        for (int i = 0; i < 3; i++) courseSlots[i] = -1;
        wire = new WirePuzzle(); wire.Shuffle(new System.Random(Environment.TickCount)); showWireHint = false;
        cipher = new CipherPuzzle { Shift = UnityEngine.Random.Range(2, 7) };
    }
    private void DrawPuzzle()
    {
        Dim(); Panel(new Rect(135, 45, 1010, 630));
        CampusOutlinedText.Label(new Rect(175, 66, 595, 40), "SKILL LAB · " + activeNpc.displayName, headingStyle);
        if (CampusOutlinedText.Button(new Rect(795, 62, 178, 38), "ออกจากภารกิจ", buttonStyle)) { ExitActivity(); return; }
        if (CampusOutlinedText.Button(new Rect(985, 62, 125, 38), "หยุด [Esc]", buttonStyle)) { Pause(); return; }
        CampusOutlinedText.Label(new Rect(175, 615, 900, 42), feedback, badgeStyle);
        switch (activeNpc.npcId)
        {
            case "mentor": DrawSymbols(); break;
            case "curriculum": DrawCourses(); break;
            case "data": DrawDataQuest(); break;
            default: DrawWire(); break;
        }
    }
    private void DrawSymbols()
    {
        CampusOutlinedText.Label(new Rect(175, 115, 900, 40), "บันทึกวันแรก · เรียงสัญลักษณ์ตามเรื่องราว", headingStyle);
        CampusOutlinedText.Label(new Rect(175, 165, 900, 90), "“เช้าตรู่แสงส่องเหนือคณะ เที่ยงฟ้าครึ้มฝน พอตกค่ำเห็นดาวระยิบ\nและก่อนนอนจันทร์ส่องนำทาง”\nกดสัญลักษณ์ 4 รูปตามลำดับ · กดย้อนกลับเพื่อแก้ไข", bodyStyle);
        for (int i = 0; i < 4; i++)
        {
            Rect r = new Rect(208 + i * 218, 288, 195, 126);
            if (CampusOutlinedText.Button(r, "", buttonStyle) && symbols.Count < 4) { symbols.Add(i); PlayUiSfx(); }
            DrawSymbol(i, new Vector2(r.center.x, r.y + 45));
            CampusOutlinedText.Label(new Rect(r.x + 15, r.y + 88, 170, 30), symbolNames[i], bodyStyle);
        }
        CampusOutlinedText.Label(new Rect(208, 447, 880, 65), "ลำดับของคุณ: " + string.Join(" → ", symbols.ConvertAll(i => symbolNames[i])), bodyStyle);
        if (CampusOutlinedText.Button(new Rect(208, 538, 200, 48), "ย้อนกลับ", buttonStyle) && symbols.Count > 0) symbols.RemoveAt(symbols.Count - 1);
        if (CampusOutlinedText.Button(new Rect(775, 538, 285, 48), "รันลำดับสัญลักษณ์", buttonStyle))
        {
            if (symbols.Count == 4 && symbols[0] == 0 && symbols[1] == 3 && symbols[2] == 2 && symbols[3] == 1) FinishNpc("mentor");
            else feedback = "ลำดับยังไม่ตรงบันทึก ลองอ่านช่วงเวลาแต่ละช่วงอีกครั้ง";
        }
    }
    private void DrawSymbol(int id, Vector2 c)
    {
        if (id == 0) { Disc(new Rect(c.x-17,c.y-17,34,34), new Color(1,.78f,.25f)); for(int i=0;i<8;i++) { float a=i*Mathf.PI/4; Line(c+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*24,c+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*32,new Color(1,.78f,.25f),3); } }
        else if (id == 1) { Disc(new Rect(c.x-22,c.y-22,44,44),new Color(1,.94f,.65f)); Disc(new Rect(c.x-10,c.y-27,38,38),new Color(.16f,.23f,.3f)); }
        else if (id == 2) { for(int i=0;i<5;i++) { float a=-Mathf.PI/2+i*Mathf.PI*2/5,b=a+Mathf.PI*4/5; Line(c+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*28,c+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*28,new Color(1,.86f,.35f),4); } }
        else { Disc(new Rect(c.x-33,c.y-9,35,29),Color.white); Disc(new Rect(c.x-16,c.y-25,39,42),Color.white); Disc(new Rect(c.x+5,c.y-8,32,28),Color.white); }
    }
    private void DrawCourses()
    {
        CampusOutlinedText.Label(new Rect(175, 120, 900, 45), "แผนฝึกทักษะ · ลากการ์ดลงช่อง หรือคลิกการ์ดแล้วคลิกช่อง", headingStyle);
        CampusOutlinedText.Label(new Rect(175, 173, 900, 60), "เริ่มจากคณิตศาสตร์ → ฝึกเขียนโปรแกรม → จัดเก็บข้อมูล\nเลือกเฉพาะวิชาพื้นฐาน 3 ใบ (เป็นแผนฝึกในเกม ไม่ใช่ตารางลงทะเบียนจริง)", bodyStyle);
        for (int i = 0; i < 4; i++)
        {
            Rect r = new Rect(192+i*226,260,208,90);
            GUI.color = selectedCard == i ? new Color(1,.87f,.4f) : Color.white;
            CampusOutlinedText.Box(r,courses[i],buttonStyle); GUI.color=Color.white;
            if (Event.current.type == EventType.MouseDown && r.Contains(Event.current.mousePosition)) { selectedCard=draggedCard=i; Event.current.Use(); }
        }
        for (int i=0;i<3;i++)
        {
            Rect r=new Rect(218+i*294,402,264,110);
            CampusOutlinedText.Box(r,(i+1)+". "+(courseSlots[i]<0?"วางการ์ดที่นี่":courses[courseSlots[i]]),buttonStyle);
            if (Event.current.type==EventType.MouseUp && r.Contains(Event.current.mousePosition) && selectedCard>=0)
            {
                for(int j=0;j<3;j++) if(courseSlots[j]==selectedCard) courseSlots[j]=-1;
                courseSlots[i]=selectedCard; selectedCard=draggedCard=-1; Event.current.Use(); PlayUiSfx();
            }
        }
        if(draggedCard>=0 && Event.current.type==EventType.Repaint) { var m=Event.current.mousePosition; CampusOutlinedText.Box(new Rect(m.x-85,m.y-20,170,45),courses[draggedCard],buttonStyle); }
        if(Event.current.type==EventType.MouseUp) draggedCard=-1;
        if(CampusOutlinedText.Button(new Rect(208,550,200,46),"ล้างแผน",buttonStyle)) for(int i=0;i<3;i++) courseSlots[i]=-1;
        if(CampusOutlinedText.Button(new Rect(775,550,285,46),"ตรวจแผนฝึก",buttonStyle))
        {
            if(courseSlots[0]==1 && courseSlots[1]==0 && courseSlots[2]==2) FinishNpc("curriculum");
            else feedback="ยังไม่ครบลำดับพื้นฐาน ลองย้ายการ์ดตามคำใบ้";
        }
    }
    private void DrawWire()
    {
        CampusOutlinedText.Label(new Rect(175,115,900,40),"IoT Circuit · เลื่อนแผ่นสายไฟ 3×3",headingStyle);
        CampusOutlinedText.Label(new Rect(175,164,430,118),"คลิกแผ่นที่ติดกับช่องว่างเพื่อเลื่อน\nต่อ Sensor ด้านซ้ายบนไปยัง Dashboard ด้านล่าง\nตรวจเฉพาะสายที่ต่อถึงกัน ไม่ต้องเรียงเลขทั้งหมด",bodyStyle);
        CampusOutlinedText.Label(new Rect(175,292,400,40),"เวลา "+Mathf.CeilToInt(wire.Remaining)+" วินาที · เลื่อน "+wire.Moves+" ครั้ง",badgeStyle);
        CampusOutlinedText.Label(new Rect(175,346,420,100),"สายเป้าหมาย: แถวบนไปขวา\nแถวกลางกลับซ้าย แล้วแถวล่างไปขวา\nเส้นออกอยู่ด้านล่างของช่อง 8",bodyStyle);
        if(CampusOutlinedText.Button(new Rect(175,457,350,45),"เริ่มสายไฟชุดใหม่",buttonStyle)) { wire.Shuffle(new System.Random(Environment.TickCount)); feedback=""; }
        if(CampusOutlinedText.Button(new Rect(175,514,350,45),showWireHint?"ปิดแผนสายไฟ":"ดูแผนสายไฟ (เวลายังเดิน)",buttonStyle)) showWireHint=!showWireHint;
        CampusOutlinedText.Label(new Rect(611,165,450,30),"Sensor →",badgeStyle);
        for(int i=0;i<9;i++)
        {
            Rect r=new Rect(650+(i%3)*124,207+(i/3)*124,116,116);
            GUI.enabled=wire.Remaining>0 && !showWireHint;
            if(CampusOutlinedText.Button(r,"",buttonStyle) && wire.Move(i)) { PlayUiSfx(); if(wire.Connected()) { FinishNpc("iot"); GUI.enabled=true; return; } }
            GUI.enabled=true;
            int mask=showWireHint?new WirePuzzle().PortAt(i):wire.PortAt(i);
            // Tile-local rectangles avoid screen-space rotation on scaled displays.
            // Render in the same absolute virtual coordinates as the tile buttons.
            // Overlay Canvas images do not inherit IMGUI group clipping.
            Vector2 c=r.center; Color color=new Color(.4f,.92f,1);
            if((mask&1)!=0) Fill(new Rect(c.x-4.5f,r.y,9,r.height/2),color);
            if((mask&2)!=0) Fill(new Rect(c.x,c.y-4.5f,r.width/2,9),color);
            if((mask&4)!=0) Fill(new Rect(c.x-4.5f,c.y,9,r.height/2),color);
            if((mask&8)!=0) Fill(new Rect(r.x,c.y-4.5f,r.width/2,9),color);
            if(mask!=0) Disc(new Rect(c.x-9,c.y-9,18,18),color);

            CampusOutlinedText.Label(new Rect(r.x+7,r.y+5,30,24),showWireHint?((i+1)%9).ToString():wire.Cells[i]==0?"":wire.Cells[i].ToString(),smallStyle);
        }
        CampusOutlinedText.Label(new Rect(766,584,290,28),"↓ Dashboard",badgeStyle);
    }
    private void DrawCipher()
    {
        CampusOutlinedText.Label(new Rect(175,115,900,40),"Cipher Wheel · หมุนวงล้อถอดรหัส",headingStyle);
        CampusOutlinedText.Label(new Rect(175,172,400,115),"บันทึกของพี่ดาต้า: เข้ารหัสโดยเลื่อนตัวอักษรไปข้างหน้า "+cipher.Shift+" ตำแหน่ง\nตั้งวงล้อ Shift ให้ตรงบันทึก แล้วถอดข้อมูลลับ",bodyStyle);
        CampusOutlinedText.Label(new Rect(175,307,400,40),"ข้อความเข้ารหัส: "+cipher.Encoded,headingStyle);
        CampusOutlinedText.Label(new Rect(175,364,400,45),"ถอดได้: "+cipher.Decoded,headingStyle);
        CampusOutlinedText.Label(new Rect(175,422,400,35),"Shift ปัจจุบัน: "+cipher.Rotation,badgeStyle);
        Vector2 center=new Vector2(845,360);
        Disc(new Rect(656,171,378,378),new Color(.19f,.34f,.39f));
        Disc(new Rect(717,232,256,256),new Color(.05f,.18f,.17f));
        for(int i=0;i<26;i++)
        {
            float a=-Mathf.PI/2+i*Mathf.PI*2/26; Vector2 d=new Vector2(Mathf.Cos(a),Mathf.Sin(a));
            CampusOutlinedText.Label(new Rect(center.x+d.x*167-9,center.y+d.y*167-12,26,28),((char)('A'+i)).ToString(),smallStyle);
            CampusOutlinedText.Label(new Rect(center.x+d.x*108-9,center.y+d.y*108-12,26,28),((char)('A'+(i+cipher.Rotation)%26)).ToString(),badgeStyle);
        }
        CampusOutlinedText.Label(new Rect(797,340,100,45),"SHIFT "+cipher.Rotation,headingStyle);
        if(CampusOutlinedText.Button(new Rect(660,562,165,42),"↶ −1",buttonStyle)) cipher.Rotate(-1);
        if(CampusOutlinedText.Button(new Rect(860,562,165,42),"+1 ↷",buttonStyle)) cipher.Rotate(1);
        if(CampusOutlinedText.Button(new Rect(175,510,340,50),"ยืนยันการถอดรหัส",buttonStyle))
        {
            if(cipher.Solved) { dataStage = 1; feedback = "ถอดรหัสแล้ว! ต่อไปจัดข้อมูลให้ถูกประเภท"; } else feedback="ถอดรหัสยังไม่ถูก ลองหมุนวงในตาม Shift ที่บันทึกไว้";
        }
    }
    private void DrawSurprise()
    {
        Dim(); float flash=.25f+.4f*Mathf.Abs(Mathf.Sin(surpriseRemaining*16));
        Fill(new Rect((1280-viewWidth)/2,(720-viewHeight)/2,viewWidth,viewHeight),new Color(.9f,.22f,.16f,flash));
        Panel(new Rect(275,255,730,185));
        CampusOutlinedText.Label(new Rect(340,290,630,55),"SURPRISE ATTACK!",titleStyle);
        CampusOutlinedText.Label(new Rect(340,355,630,55),"น้องบั๊กจู่โจมคุณระหว่างสำรวจ!",headingStyle);
    }
    private void DrawCombat()
    {
        Rect viewport=new Rect((1280-viewWidth)/2,(720-viewHeight)/2,viewWidth,viewHeight);
        DrawCodeRain(viewport);
        CampusOutlinedText.Label(new Rect(64,18,520,33),"CS CAMPUS  /  DEBUG DUEL",headingStyle);
        CampusOutlinedText.Label(new Rect(825,27,210,30),"ROUND "+(battle.Turn+1).ToString("00"),badgeStyle);
        ArenaPlatform(new Rect(740,270,435,104),new Color(.35f,.78f,.57f));
        ArenaPlatform(new Rect(90,425,555,118),new Color(.4f,.8f,.93f));
        float bob=Mathf.Sin(battleAnimation*4)*7;
        DrawBattleActors(bob);
        DrawTurnEffects();
        BattleStatus(new Rect(64,69,460,132),"ปีศาจบั๊ก · " + (battle.Enraged ? "ENRAGED" : "Glitch Demon"),visibleBugHp,new Color(.52f,.94f,.54f));
        BattleStatus(new Rect(724,387,490,132),"นักศึกษาใหม่ · CS Team",visiblePlayerHp,new Color(.3f,.84f,1));
        BattleCard(new Rect(64,214,540,56),new Color(1,.75f,.3f));
        CampusOutlinedText.Label(new Rect(82,227,505,38),"กำลังเตรียม: "+new[]{"Null Reference · Debug ได้ผลดี","Memory Leak · ทีมช่วยกันป้องกัน","Infinite Loop · Test ได้ผลดี"}[battle.Intent],badgeStyle);
        BattleCard(new Rect(28,548,1224,151),new Color(.45f,.8f,.77f));
        CampusOutlinedText.Label(new Rect(55,562,595,28),!turnAnimating && battle.Won?"MISSION CLEAR":!turnAnimating && battle.Lost?"พักฟื้นแล้วลองใหม่":"BATTLE LOG",badgeStyle);
        CampusOutlinedText.Label(new Rect(55,600,595,82),feedback,bodyStyle);
        if(CampusOutlinedText.Button(new Rect(1090,20,140,37),"หยุด [Esc]",buttonStyle)) { Pause(); return; }
        if(!turnAnimating && (battle.Won || battle.Lost))
        {
            if(CampusOutlinedText.Button(new Rect(750,570,425,48),battle.Won?"ชนะแล้ว! รับ Badge และสำรวจต่อ":"พักฟื้นแล้วลองใหม่",buttonStyle))
            {
                if(battle.Won) FinishBugFight(); else { battle=new BugBattle(Environment.TickCount); ResetBattleTurn(); feedback="ทีมพร้อมแล้ว! ใช้ Patch และ Pair Programming ช่วยฟื้นพลัง"; }
            }
            if(battle.Lost && CampusOutlinedText.Button(new Rect(750,633,425,42),"ถอยกลับไปสำรวจ",buttonStyle)) ExitActivity();
            return;
        }
        string[] labels={"01  DEBUG","02  TEST","03  PAIR PROGRAMMING","04  PATCH"};
        string[] details={"เจาะจุดอ่อน · พลัง −2","ทดสอบจุดอ่อน · พลัง −2","ตั้งเกราะ + ฟื้นพลัง 3","ฟื้น HP 8 · เหลือ "+battle.Patches};
        Color[] colors={new Color(.4f,.83f,1),new Color(.88f,.65f,1),new Color(1,.8f,.4f),new Color(.4f,.94f,.65f)};
        for(int i=0;i<4;i++)
        {
            GUI.enabled=!turnAnimating && !battle.Won && !battle.Lost && (i<2?battle.Energy>=2:i==3?battle.Patches>0:true);
            if(BattleButton(new Rect(684+(i%2)*275,559+(i/2)*64,264,59),labels[i],details[i],colors[i]))
            { BeginAnimatedTurn(i); }
        }
        GUI.enabled=true;
    }
    private void ExitActivity()
    {
        turnAnimating=false;
        state=ScreenState.Playing; Time.timeScale=1; activeNpc=null; draggedCard=selectedCard=-1;
        encounterCountdown=Mathf.Max(encounterCountdown,20f); mapOpen=false;
        SyncBattleMusic(); ShowToast("ออกจากภารกิจแล้ว · คุยกับ NPC เพื่อเริ่มใหม่ได้",3);
    }
    private void ArenaPlatform(Rect r,Color accent)
    {
        Disc(new Rect(r.x-9,r.y+12,r.width+18,r.height),new Color(.015f,.045f,.045f,.8f));
        Disc(r,accent); Disc(new Rect(r.x+5,r.y+5,r.width-10,r.height-10),new Color(.11f,.3f,.24f));
        Disc(new Rect(r.x+24,r.y+12,r.width-48,r.height-30),new Color(.25f,.43f,.29f));
        for(int i=0;i<14;i++) { float a=i*Mathf.PI*2/14; Vector2 c=r.center+new Vector2(Mathf.Cos(a)*r.width*.39f,Mathf.Sin(a)*r.height*.28f); Fill(new Rect(c.x,c.y,3,2),new Color(.65f,.88f,.56f)); }
    }
    private static void BattleCard(Rect r,Color accent)
    {
        Fill(new Rect(r.x+4,r.y+6,r.width,r.height),new Color(0,0,0,.27f));
        Fill(r,new Color(.23f,.38f,.39f));
        Fill(new Rect(r.x+1,r.y+1,r.width-2,r.height-2),new Color(.045f,.10f,.14f,.97f));
        Fill(new Rect(r.x,r.y,4,r.height),accent);
    }
    private bool BattleButton(Rect r,string label,string detail,Color accent)
    {
        bool enabled=GUI.enabled,hover=enabled && r.Contains(Event.current.mousePosition);
        bool clicked=CampusOutlinedText.Button(r,GUIContent.none,GUIStyle.none);
        BattleCard(r,enabled?accent:Color.gray);
        if(hover) Fill(new Rect(r.x+4,r.y+1,r.width-5,r.height-2),new Color(accent.r,accent.g,accent.b,.17f));
        CampusOutlinedText.Label(new Rect(r.x+14,r.y+4,r.width-22,28),label,badgeStyle);
        CampusOutlinedText.Label(new Rect(r.x+14,r.y+31,r.width-22,23),detail,smallStyle);
        return clicked;
    }
    private void BattleStatus(Rect r,string name,int hp,Color color)
    {
        BattleCard(r,color); CampusOutlinedText.Label(new Rect(r.x+20,r.y+13,r.width-35,34),name,headingStyle);
        Fill(new Rect(r.x+20,r.y+59,r.width-40,14),new Color(.13f,.22f,.23f));
        Fill(new Rect(r.x+20,r.y+59,(r.width-40)*hp/(name.StartsWith("นักศึกษา") ? BugBattle.PlayerMaxHp : BugBattle.BugMaxHp),14),color);
        CampusOutlinedText.Label(new Rect(r.x+20,r.y+86,r.width-30,28),"HP "+hp+" / "+(name.StartsWith("นักศึกษา") ? BugBattle.PlayerMaxHp : BugBattle.BugMaxHp)+(name.StartsWith("นักศึกษา")?"    ENERGY "+battle.Energy+" / "+BugBattle.MaxEnergy:"    SYSTEM GLITCH"),smallStyle);
    }
    private static void DrawSprite(Sprite sprite,Rect rect)
    {
        if(sprite==null) return; Rect source=sprite.rect; var t=sprite.texture;
        CampusOutlinedText.PictureUV(rect,t,new Rect(source.x/t.width,source.y/t.height,source.width/t.width,source.height/t.height));
    }
    private static void Fill(Rect r,Color color) { GUI.color=color; CampusOutlinedText.Picture(r,Texture2D.whiteTexture); GUI.color=Color.white; }
    private void Disc(Rect r,Color color)
    {
        if(circle==null) { circle=new Texture2D(64,64,TextureFormat.RGBA32,false); var pixels=new Color[4096]; for(int y=0;y<64;y++) for(int x=0;x<64;x++) pixels[y*64+x]=(new Vector2(x-31.5f,y-31.5f).sqrMagnitude<31*31)?Color.white:Color.clear; circle.SetPixels(pixels); circle.Apply(); }
        GUI.color=color; CampusOutlinedText.Picture(r,circle); GUI.color=Color.white;
    }
    private static void Line(Vector2 a,Vector2 b,Color color,float width)
    {
        Matrix4x4 saved=GUI.matrix;
        GUI.matrix=saved*Matrix4x4.TRS(new Vector3(a.x,a.y,0),Quaternion.Euler(0,0,Mathf.Atan2(b.y-a.y,b.x-a.x)*Mathf.Rad2Deg),Vector3.one);
        Fill(new Rect(0,-width/2,Vector2.Distance(a,b),width),color); GUI.matrix=saved;
    }
}
