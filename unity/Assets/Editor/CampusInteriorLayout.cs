using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using TMPro;

public static class CampusInteriorLayout
{
    public static void Apply(DigitalCampusGame game)
    {
        var root=new GameObject("Building60 · Reference classroom layout");
        var blocked=new List<Rect>();
        var floors=new[]{new Rect(72,.6f,7,8.8f),new Rect(81,.6f,7,8.8f),new Rect(72,-3.3f,16,2.5f),new Rect(74.8f,-1.1f,1.4f,2),new Rect(83.8f,-1.1f,1.4f,2),new Rect(68.5f,3.5f,3.8f,2.8f),new Rect(87.7f,6.2f,3.8f,2.6f)};
        Sprite Sprite(string n)=>AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/ModernCampus/"+n+".png");
        GameObject Piece(string name,string art,Rect r,int order=0,Color? tint=null)
        {
            var go=new GameObject(name);go.transform.SetParent(root.transform);go.transform.position=new Vector3(r.center.x,r.center.y,0);
            var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=Sprite(art);renderer.sortingOrder=order;
            renderer.color=tint??Color.white;
            go.transform.localScale=new Vector3(r.width/renderer.sprite.bounds.size.x,r.height/renderer.sprite.bounds.size.y,1);return go;
        }
        void Solid(string name,string art,Rect r,int order=5) {Piece(name,art,r,order);blocked.Add(r);}
        void Wall(string name,Rect r)=>Solid(name,"wall",r,9);
        void Sign(string text,Vector2 position,float size=.075f)
        {
            var go=new GameObject(text);go.transform.SetParent(root.transform);go.transform.position=position;
            var label=go.AddComponent<TextMeshPro>();label.text=text;label.font=TMP_FontAsset.CreateFontAsset(game.thaiSemibold);label.fontSize=size*50f;label.alignment=TextAlignmentOptions.Center;label.color=Color.white;
            var renderer=go.GetComponent<MeshRenderer>();renderer.sharedMaterial=game.thaiSemibold.material;renderer.sortingOrder=25;
        }
        Piece("Backdrop","void",new Rect(65,-5,30,18),-12);
        foreach(var floor in floors)Piece("Walkable floor","stone",floor,-6,new Color(.76f,.76f,.83f));
        foreach(float x in new[]{72f,81f})
        {
            for(float y=.8f;y<6.1f;y+=.22f)Piece("Carpet seam","stone",new Rect(x+.15f,y,6.7f,.025f),-5,new Color(.65f,.65f,.73f));
            Piece("Raised timber teaching area","stage-wood",new Rect(x+.15f,6.2f,6.7f,3.05f),-3);
            Piece("Front stage trim","stage-wood",new Rect(x+.2f,6.06f,6.6f,.14f),-2);
            Wall("North wall",new Rect(x,9.25f,7,.15f));
            if(x==72) {Wall("West wall lower",new Rect(x,.6f,.15f,3.2f));Wall("West wall upper",new Rect(x,5.3f,.15f,4.1f));Wall("East wall",new Rect(x+6.85f,.6f,.15f,8.8f));}
            else {Wall("West wall",new Rect(x,.6f,.15f,8.8f));Wall("East wall lower",new Rect(x+6.85f,.6f,.15f,5.9f));Wall("East wall upper",new Rect(x+6.85f,7.9f,.15f,1.5f));}
            Wall("Door wall left",new Rect(x,.6f,2.8f,.15f));Wall("Door wall right",new Rect(x+4.2f,.6f,2.8f,.15f));
            Wall("Door passage left",new Rect(x+2.65f,-.8f,.15f,1.4f));Wall("Door passage right",new Rect(x+4.2f,-.8f,.15f,1.4f));
            Piece("Door runner",x==72?"runner-red":"runner-blue",new Rect(x+2.88f,-1.05f,1.24f,1.8f),3);
            Solid("Whiteboard","board",new Rect(x+1.7f,8.05f,3.6f,1),10);
            Solid("Speaker left","cabinet",new Rect(x+.3f,8.1f,.55f,1),8);
            Solid("Speaker right","cabinet",new Rect(x+6.1f,8.1f,.55f,1),8);
            if(x==72)Solid("Lecture podium","teacher-desk",new Rect(x+3.05f,5.8f,.9f,.55f),10);
            else Solid("Computer teaching desk","teacher-desk",new Rect(x+1.55f,6.65f,3.9f,.85f),10);
            foreach(float col in new[]{.65f,1.65f,4.6f,5.6f})
                for(int row=0;row<4;row++)Solid("Seat row "+row,"class-seat",new Rect(x+col,1.05f+row*.97f,.72f,.8f),7);
            Sign(x==72?"ห้องบรรยาย · อาจารย์ Algorithm":"ห้องปฏิบัติการคอมพิวเตอร์",new Vector2(x+3.5f,9.85f),.07f);
        }
        // Side wings, connected through real gaps in the classroom walls.
        Wall("Left wing north",new Rect(68.5f,6.15f,3.5f,.15f));Wall("Left wing west",new Rect(68.5f,3.5f,.15f,2.8f));Wall("Left wing south",new Rect(68.5f,3.5f,3.5f,.15f));
        Solid("Storage locker","cabinet",new Rect(68.8f,4.2f,.8f,1.6f));
        Piece("Left wing notice","poster",new Rect(70,5.4f,.7f,.7f),10);
        Wall("Right wing north",new Rect(88,8.65f,3.5f,.15f));Wall("Right wing east",new Rect(91.35f,6.2f,.15f,2.6f));Wall("Right wing south",new Rect(88,6.2f,3.5f,.15f));
        Solid("Lab equipment","cabinet",new Rect(90.3f,7,.65f,1.35f));
        Piece("Lab side door","door",new Rect(89,7.5f,.75f,1),8);
        // Bottom lobby with separate door thresholds and plants clear of the exit.
        Wall("Lobby west",new Rect(72,-3.3f,.15f,2.5f));Wall("Lobby east",new Rect(87.85f,-3.3f,.15f,2.5f));
        Wall("Lobby lower left",new Rect(72,-3.3f,7.3f,.15f));Wall("Lobby lower right",new Rect(80.7f,-3.3f,7.3f,.15f));
        Wall("Lobby top left",new Rect(72,-.9f,2.8f,.15f));Wall("Lobby top middle",new Rect(76.2f,-.9f,7.6f,.15f));Wall("Lobby top right",new Rect(85.2f,-.9f,2.8f,.15f));
        foreach(float x in new[]{78.1f,81f})Solid("Lobby planter","plant",new Rect(x,-1.9f,.8f,1.1f),8);
        Piece("Lobby poster left","poster",new Rect(76.7f,-1.55f,.55f,.55f),10);
        Piece("Lobby poster right","poster",new Rect(82.8f,-1.55f,.55f,.55f),10);
        Piece("Exit mat","rug",new Rect(79.4f,-3.05f,1.2f,.65f),3);
        Sign("ทางออก [E]",new Vector2(80,-3.65f),.06f);
        var characterRects=new List<Rect>();
        foreach(var item in new[]{("Abby",73.1f,5.15f),("Alex",77.9f,5.15f),("Molly",82.1f,5.15f),("Oscar",86.9f,5.15f)})
        {
            var go=new GameObject("Student bay · "+item.Item1);go.transform.SetParent(root.transform);go.transform.position=new Vector3(item.Item2,item.Item3,0);go.transform.localScale=Vector3.one*.85f;
            var sr=go.AddComponent<SpriteRenderer>();sr.sprite=AssetDatabase.LoadAllAssetsAtPath("Assets/Art/ModernCampus/"+item.Item1+".png").OfType<Sprite>().OrderBy(s=>s.name).First();sr.sortingOrder=15;
            var body=new Rect(item.Item2-.3f,item.Item3,.6f,.8f);characterRects.Add(body);blocked.Add(new Rect(item.Item2-.24f,item.Item3-.08f,.48f,.28f));
        }
        var teacher=game.npcs.First(n=>n.npcId=="curriculum");teacher.transform.position=new Vector3(75.5f,6.95f,0);teacher.transform.localScale=Vector3.one;
        foreach(var label in teacher.GetComponentsInChildren<TextMeshPro>(true))label.gameObject.SetActive(false);
        characterRects.Add(new Rect(75.2f,6.95f,.6f,.95f));
        foreach(var body in characterRects)
            foreach(var solid in blocked)if(body.Overlaps(solid) && solid.height>.3f)throw new Exception("Character overlaps furniture: "+body+" / "+solid);
        game.buildingInterior=root;game.interiorFurniture=blocked.ToArray();game.interiorWalkableFloors=floors;root.SetActive(false);
        ValidateRoutes(game);
    }
    private static void ValidateRoutes(DigitalCampusGame game)
    {
        var method=typeof(DigitalCampusGame).GetMethod("InsideFloor",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);
        bool Can(Vector2 p)=>(bool)method.Invoke(game,new object[]{p});
        var origin=new Vector2(80,-2.55f);var queue=new Queue<Vector2Int>();var seen=new HashSet<Vector2Int>();queue.Enqueue(Vector2Int.zero);seen.Add(Vector2Int.zero);
        var goals=new[]{new Vector2(75.5f,6.8f),new Vector2(84.5f,5.5f),new Vector2(70.5f,4.6f),new Vector2(89.5f,7.2f)};var reached=new bool[goals.Length];
        while(queue.Count>0)
        {
            var cell=queue.Dequeue();var p=origin+(Vector2)cell*.18f;
            for(int i=0;i<goals.Length;i++)if(Vector2.Distance(p,goals[i])<.4f)reached[i]=true;
            foreach(var d in new[]{Vector2Int.left,Vector2Int.right,Vector2Int.up,Vector2Int.down})
            {
                var next=cell+d;var q=origin+(Vector2)next*.18f;
                if(q.x<68 || q.x>92 || q.y< -4 || q.y>10 || !seen.Add(next) || !Can(q))continue;
                queue.Enqueue(next);
            }
        }
        if(reached.Any(r=>!r))throw new Exception("Interior route blocked: "+string.Join(",",reached));
        Debug.Log("INTERIOR LAYOUT CHECKS PASSED: both rooms, Algorithm, both side wings reachable; characters clear of furniture");
    }
}
