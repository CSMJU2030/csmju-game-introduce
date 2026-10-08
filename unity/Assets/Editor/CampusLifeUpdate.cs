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
        File.Delete(request);
        try { Apply(); CampusSceneBuilder.BuildWebGl(); File.WriteAllText("/tmp/campus-life-build.result","SUCCESS"); }
        catch(Exception e) { File.WriteAllText("/tmp/campus-life-build.result",e.ToString()); Debug.LogException(e); }
    }
    [MenuItem("CSMJU/Apply Campus Life, Navigation and Battle")]
    public static void Apply() {
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
        var room=maps.First(m=>m.name=="layer1"); var furniture=maps.First(m=>m.name=="layer2");
        var previous=game.buildingInterior.transform.Find("Collision · authored interior");
        if(previous!=null) UnityEngine.Object.DestroyImmediate(previous.gameObject);
        var collision=new GameObject("Collision · authored interior");collision.transform.SetParent(game.buildingInterior.transform,false);
        var solid=new List<Collider2D>();var floors=new List<Rect>();var floor=room.GetTile(new Vector3Int(90,0,0));
        void Block(Tilemap map,Vector3Int cell,bool foot) {
            var a=map.transform.TransformPoint((Vector3)cell);var b=map.transform.TransformPoint((Vector3)(cell+new Vector3Int(1,1,0)));
            float width=Mathf.Abs(b.x-a.x),height=Mathf.Abs(b.y-a.y);
            var go=new GameObject((foot?"Furniture feet ":"Wall ")+cell);go.transform.SetParent(collision.transform,false);
            go.transform.position=new Vector3((a.x+b.x)*.5f,foot?a.y+height*.36f:(a.y+b.y)*.5f,0);
            var box=go.AddComponent<BoxCollider2D>();box.size=new Vector2(width*(foot?.86f:1),height*(foot?.66f:1));solid.Add(box);
        }
        foreach(var cell in room.cellBounds.allPositionsWithin) {
            var tile=room.GetTile(cell);if(tile==null) continue;
            if(tile==floor) { var a=room.transform.TransformPoint((Vector3)cell);var b=room.transform.TransformPoint((Vector3)(cell+new Vector3Int(1,1,0)));floors.Add(new Rect(a.x,a.y,b.x-a.x,b.y-a.y)); }
            else Block(room,cell,false);
        }
        foreach(var cell in furniture.cellBounds.allPositionsWithin)
            if(furniture.HasTile(cell) && !furniture.HasTile(cell+Vector3Int.down)) Block(furniture,cell,true);
        game.interiorColliders=solid.ToArray();game.interiorFurniture=Array.Empty<Rect>();game.interiorWalkableFloors=floors.ToArray();
        game.playerCollider.size=new Vector2(.42f,.24f);
        var old=game.buildingInterior.transform.Find("Campus students");if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);
        var studentsRoot=new GameObject("Campus students");studentsRoot.transform.SetParent(game.buildingInterior.transform,false);
        bool Clear(Vector2 p) => floors.Any(r=>r.Contains(p)) && !solid.OfType<BoxCollider2D>().Any(c=>new Rect((Vector2)c.transform.position-c.size*.5f,c.size).Overlaps(new Rect(p-Vector2.one*.25f,Vector2.one*.5f)));
        var students=new List<CampusStudent>();string[] names={"Alex","Molly","Oscar"};
        for(int i=0;i<3;i++) {
            float x=95+i*6;
            var candidates=new[]{new Vector2(x,-6.5f),new Vector2(x+3,-6.5f),new Vector2(x+3,-5.5f),new Vector2(x,-5.5f)};
            var route=candidates.Where(Clear).ToArray();
            if(route.Length<2)throw new Exception("Student patrol has insufficient safe floor: "+names[i]);
            for(int n=0;n<route.Length;n++)for(int k=0;k<=20;k++)if(!Clear(Vector2.Lerp(route[n],route[(n+1)%route.Length],k/20f)))throw new Exception("Blocked student patrol");
            var go=new GameObject("Student · "+names[i]);go.transform.SetParent(studentsRoot.transform,false);go.transform.position=route[0];
            var frames=AssetDatabase.LoadAllAssetsAtPath("Assets/Art/ModernCampus/"+names[i]+".png").OfType<Sprite>().OrderBy(s=>s.name).ToArray();
            if(frames.Length<24)throw new Exception("Missing directional student sprites");
            var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=frames[0];renderer.sortingOrder=10;
            var student=go.AddComponent<CampusStudent>();student.game=game;student.frames=frames;student.route=route;student.studentName=names[i];
            student.chat=new[]{"ไปทำโปรเจกต์ที่แล็บกันไหม?","วันนี้เราเรียนเขียนโปรแกรมกัน","เดี๋ยวไปถามพี่ดาต้ากันนะ"};students.Add(student);
        }
        game.students=students.ToArray();game.buildingInterior.SetActive(false);
        EditorUtility.SetDirty(game);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
    }
}
