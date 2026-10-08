using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class CampusLifeUpdate
{
    static CampusLifeUpdate() { EditorApplication.delayCall+=Poll; }
    static void Poll() {
        const string request="/tmp/campus-life-build.request";
        if(!File.Exists(request)) return;
        if(EditorApplication.isPlayingOrWillChangePlaymode) { EditorApplication.isPlaying=false;EditorApplication.delayCall+=Poll;return; }
        File.Delete(request);
        try { Apply(); CampusSceneBuilder.BuildWebGl(); File.WriteAllText("/tmp/campus-life-build.result","SUCCESS"); }
        catch(Exception e) { File.WriteAllText("/tmp/campus-life-build.result",e.ToString()); Debug.LogException(e); }
    }
    [MenuItem("CSMJU/Apply Campus Life, Navigation and Battle")]
    public static void Apply() {
        foreach(SceneView view in SceneView.sceneViews) { view.drawGizmos=false;view.showGrid=false; }
        foreach(var window in Resources.FindObjectsOfTypeAll<EditorWindow>()) if(window.GetType().Name=="GameView") {
            var property=window.GetType().GetProperty("drawGizmos",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic);
            if(property!=null && property.CanWrite)property.SetValue(window,false);
        }
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/DigitalCampus.unity");
        var game=UnityEngine.Object.FindFirstObjectByType<DigitalCampusGame>();
        var world=scene.GetRootGameObjects().First(r=>r.name.StartsWith("World"));
        float factor=1.55f/world.transform.localScale.x;
        world.transform.localScale=Vector3.one*1.55f;
        if(Mathf.Abs(factor-1)>.001f) {
            game.mapMin*=factor; game.mapMax*=factor; game.buildingEntrance*=factor;
            game.player.position*=factor;
            foreach(var npc in game.npcs) if(npc!=null && npc.npcId!="data" && npc.npcId!="curriculum") npc.transform.position*=factor;
            foreach(var root in scene.GetRootGameObjects()) if(root.name.StartsWith("Navigation")) root.transform.localScale*=factor;
            for(int i=0;i<game.walkableAreas.Length;i++) { var r=game.walkableAreas[i]; game.walkableAreas[i]=new Rect(r.position*factor,r.size*factor); }
        }
        var ground=world.GetComponent<SpriteRenderer>();ground.sortingOrder=0;
        const string canopyMaterialPath="Assets/Art/Campus/Canopy.mat";
        var canopyMaterial=AssetDatabase.LoadAssetAtPath<Material>(canopyMaterialPath);
        var canopyShader=Shader.Find("Campus/Tree Canopy");
        if(canopyShader==null)throw new Exception("Missing tree canopy shader");
        if(canopyMaterial==null) { canopyMaterial=new Material(canopyShader);AssetDatabase.CreateAsset(canopyMaterial,canopyMaterialPath); }
        canopyMaterial.shader=canopyShader;EditorUtility.SetDirty(canopyMaterial);
        var canopy=world.transform.Find("Trees · foreground order 2");
        if(canopy==null) { var go=new GameObject("Trees · foreground order 2");go.transform.SetParent(world.transform,false);canopy=go.transform; }
        var canopyRenderer=canopy.GetComponent<SpriteRenderer>();if(canopyRenderer==null)canopyRenderer=canopy.gameObject.AddComponent<SpriteRenderer>();
        canopyRenderer.sprite=ground.sprite;canopyRenderer.sharedMaterial=canopyMaterial;canopyRenderer.sortingOrder=2;
        game.playerRenderer.sortingOrder=1;
        foreach(var npc in game.npcs) if(npc!=null) {
            npc.spriteRenderer.sortingOrder=1;
            foreach(var label in npc.GetComponentsInChildren<MeshRenderer>(true))label.sortingOrder=3;
        }
        game.cameraViewSize=5.2f; game.campusOverview=false;
        game.campusMapTexture=world.GetComponent<SpriteRenderer>().sprite.texture;
        const string maskPath="Assets/Resources/Art/Campus Ground Mask.asset";
        var mask=AssetDatabase.LoadAssetAtPath<CampusGroundMask>(maskPath);
        if(mask==null) { mask=ScriptableObject.CreateInstance<CampusGroundMask>();AssetDatabase.CreateAsset(mask,maskPath); }
        var image=new Texture2D(2,2); image.LoadImage(File.ReadAllBytes("Assets/Art/Campus/campus-map.png"));
        mask.width=512;mask.height=288;mask.blocked=new byte[mask.width*mask.height];
        for(int y=0;y<mask.height;y++)for(int x=0;x<mask.width;x++) {
            var c=image.GetPixelBilinear((x+.5f)/mask.width,(y+.5f)/mask.height);
            // Green vegetation within the authored navigation polygons is solid.
            mask.blocked[y*mask.width+x]=(byte)(c.g>c.r*1.03f && c.g>c.b*1.16f && c.g-c.b>.04f?1:0);
        }
        UnityEngine.Object.DestroyImmediate(image);game.groundMask=mask;EditorUtility.SetDirty(mask);
        var maps=game.buildingInterior.GetComponentsInChildren<Tilemap>(true);
        var background=maps.FirstOrDefault(m=>m.name=="layer0");
        if(background!=null) background.GetComponent<TilemapRenderer>().enabled=false;
        var room=maps.First(m=>m.name=="layer1"); var furniture=maps.First(m=>m.name=="layer2");
        room.GetComponent<TilemapRenderer>().sortingOrder=0;furniture.GetComponent<TilemapRenderer>().sortingOrder=2;
        // Discard leftover editor grid tiles outside the authored building footprint.
        foreach(var map in maps) foreach(var cell in map.cellBounds.allPositionsWithin)
            if(cell.x<89 || cell.x>115 || cell.y< -8 || cell.y>9) map.SetTile(cell,null);
        var previous=game.buildingInterior.transform.Find("Collision · authored interior");
        if(previous!=null) UnityEngine.Object.DestroyImmediate(previous.gameObject);
        var collision=new GameObject("Collision · authored interior");collision.transform.SetParent(game.buildingInterior.transform,false);
        var solid=new List<Collider2D>();var floors=new List<Rect>();var floor=room.GetTile(new Vector3Int(90,0,0));
        void Block(Tilemap map,Vector3Int cell,bool foot) {
            var a=map.transform.TransformPoint((Vector3)cell);var b=map.transform.TransformPoint((Vector3)(cell+new Vector3Int(1,1,0)));
            float width=Mathf.Abs(b.x-a.x),height=Mathf.Abs(b.y-a.y);
            var go=new GameObject((foot?"Furniture feet ":"Wall ")+cell);go.transform.SetParent(collision.transform,false);
            go.transform.position=new Vector3((a.x+b.x)*.5f,(a.y+b.y)*.5f,0);
            var box=go.AddComponent<BoxCollider2D>();box.size=new Vector2(width*(foot?.86f:1),height*(foot?.9f:1));solid.Add(box);
        }
        foreach(var cell in room.cellBounds.allPositionsWithin) {
            var tile=room.GetTile(cell);if(tile==null) continue;
            if(tile==floor) { var a=room.transform.TransformPoint((Vector3)cell);var b=room.transform.TransformPoint((Vector3)(cell+new Vector3Int(1,1,0)));floors.Add(new Rect(a.x,a.y,b.x-a.x,b.y-a.y)); }
            else Block(room,cell,false);
        }
        foreach(var cell in furniture.cellBounds.allPositionsWithin)
            if(furniture.HasTile(cell)) Block(furniture,cell,true);
        game.interiorColliders=solid.ToArray();game.interiorFurniture=Array.Empty<Rect>();game.interiorWalkableFloors=floors.ToArray();
        game.playerCollider.size=new Vector2(.42f,.24f);
        var old=game.buildingInterior.transform.Find("Campus students");if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);
        var studentsRoot=new GameObject("Campus students");studentsRoot.transform.SetParent(game.buildingInterior.transform,false);
        bool Clear(Vector2 p) => floors.Any(r=>r.Contains(p)) && !solid.OfType<BoxCollider2D>().Any(c=>new Rect((Vector2)c.transform.position-c.size*.5f,c.size).Overlaps(new Rect(p-Vector2.one*.25f,Vector2.one*.5f)));
        // Keep quest NPCs in the open aisles, reachable from the entrance.
        var reachable=new HashSet<Vector2>();var pending=new Queue<Vector2>();
        pending.Enqueue(game.interiorSpawn);reachable.Add(game.interiorSpawn);
        while(pending.Count>0) {
            var point=pending.Dequeue();
            foreach(var step in new[]{Vector2.up,Vector2.down,Vector2.left,Vector2.right}) {
                var next=point+step*.5f;
                if(!reachable.Contains(next) && Clear(next)) { reachable.Add(next);pending.Enqueue(next); }
            }
        }
        foreach(var npc in game.npcs.Where(n=>n!=null && (n.npcId=="data" || n.npcId=="curriculum"))) {
            Vector2 desired=npc.npcId=="data"?new Vector2(105.5f,2.5f):new Vector2(97.5f,5.5f);
            var location=reachable.OrderBy(v=>Vector2.SqrMagnitude(v-desired)).First();
            if(Vector2.Distance(location,desired)>2f)throw new Exception("Quest NPC aisle is inaccessible: "+npc.npcId);
            npc.transform.position=location;
        }
        var students=new List<CampusStudent>();string[] names={"Alex","Molly","Oscar"};
        for(int i=0;i<3;i++) {
            Vector2 desired=i==0?new Vector2(97.5f,1.5f):i==1?new Vector2(112.5f,3.5f):new Vector2(100.5f,-6.5f);
            bool InZone(Vector2 v)=>i==0?v.x<100 && v.y> -3:i==1?v.x>102 && v.y> -3:v.y< -5;
            var eligible=reachable.Where(v=>InZone(v) && game.npcs.Where(n=>n!=null && (n.npcId=="data" || n.npcId=="curriculum")).All(n=>Vector2.Distance(v,n.transform.position)>.85f)).ToArray();
            if(eligible.Length<2)throw new Exception("No safe student spawn in zone "+i);
            var seed=eligible.OrderBy(v=>Vector2.SqrMagnitude(v-desired)).First();
            var route=eligible.Where(v=>Vector2.Distance(v,seed)<2.8f).OrderBy(v=>Vector2.SqrMagnitude(v-seed)).Take(24).ToArray();
            if(route.Length<2)throw new Exception("Student patrol has insufficient safe floor: "+names[i]);
            var go=new GameObject("Student · "+names[i]);go.transform.SetParent(studentsRoot.transform,false);go.transform.position=route[0];
            var frames=AssetDatabase.LoadAllAssetsAtPath("Assets/Art/ModernCampus/"+names[i]+".png").OfType<Sprite>().OrderBy(s=>s.name).ToArray();
            if(frames.Length<24)throw new Exception("Missing directional student sprites");
            var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=frames[0];renderer.sortingOrder=1;
            var student=go.AddComponent<CampusStudent>();student.game=game;student.frames=frames;student.route=route;student.studentName=names[i];
            student.chat=new[]{"ไปทำโปรเจกต์ที่แล็บกันไหม?","วันนี้เราเรียนเขียนโปรแกรมกัน","เดี๋ยวไปถามพี่ดาต้ากันนะ"};students.Add(student);
        }
        game.students=students.ToArray();game.buildingInterior.SetActive(false);
        EditorUtility.SetDirty(game);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
    }
}
