using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TMPro;

public static class CampusNavigationBuilder
{
    [MenuItem("CSMJU/Update Campus Navigation and NPCs")]
    public static void Apply()
    {
        CampusSceneBuilder.EnsureSpriteImporters();
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/DigitalCampus.unity", OpenSceneMode.Single);
        var game = UnityEngine.Object.FindFirstObjectByType<DigitalCampusGame>();
        float width = game.mapMax.x - game.mapMin.x, height = game.mapMax.y - game.mapMin.y;
        Vector2 At(float u, float v) => new Vector2((u-.5f)*width, (.5f-v)*height);
        foreach (var root in scene.GetRootGameObjects())
            if (root.name.StartsWith("Navigation")) UnityEngine.Object.DestroyImmediate(root);
        var nav = new GameObject("Navigation · edit polygon outlines to match paths");
        PolygonCollider2D Surface(string name, params float[] uv)
        {
            var go = new GameObject(name); go.transform.SetParent(nav.transform, false);
            var collider = go.AddComponent<PolygonCollider2D>(); collider.isTrigger = true;
            var points = new Vector2[uv.Length/2];
            for(int i=0;i<points.Length;i++) points[i] = At(uv[i*2], uv[i*2+1]);
            collider.points = points; return collider;
        }
        game.walkableSurfaces = new[] {
            Surface("Main promenade", .315f,.526f, .38f,.516f, .477f,.492f, .53f,.49f, .59f,.492f, .61f,.522f, .68f,.52f, .703f,.515f, .766f,.515f, .792f,.538f, .92f,.538f, .942f,.57f, .914f,.596f, .799f,.592f, .765f,.574f, .657f,.578f, .60f,.584f, .532f,.582f, .471f,.582f, .349f,.584f, .311f,.564f),
            Surface("Central avenue", .486f,.001f, .514f,.001f, .52f,.185f, .513f,.32f, .519f,.42f, .531f,.49f, .527f,.638f, .517f,.697f, .51f,.818f, .478f,.83f, .476f,.71f, .474f,.635f, .471f,.49f, .483f,.42f, .481f,.29f, .484f,.18f),
            Surface("Maejo 60 steps and forecourt", .239f,.342f, .312f,.342f, .31f,.43f, .303f,.467f, .324f,.49f, .342f,.526f, .325f,.564f, .297f,.535f, .27f,.521f, .151f,.523f, .153f,.472f, .221f,.463f, .241f,.445f),
            Surface("Chulabhorn steps", .704f,.386f, .754f,.386f, .759f,.469f, .778f,.52f, .763f,.558f, .705f,.548f, .694f,.509f, .705f,.471f),
            Surface("Garden diagonal to Open House", .653f,.55f, .681f,.551f, .693f,.615f, .722f,.672f, .729f,.708f, .778f,.76f, .772f,.815f, .729f,.813f, .706f,.767f, .682f,.738f, .691f,.692f, .674f,.641f, .66f,.60f),
            Surface("Open House plaza", .72f,.748f, .763f,.734f, .801f,.745f, .853f,.746f, .917f,.772f, .95f,.829f, .958f,.889f, .94f,.944f, .876f,.947f, .794f,.922f, .744f,.89f, .704f,.846f, .691f,.803f),
            Surface("South cross path", .483f,.763f, .519f,.766f, .58f,.771f, .64f,.799f, .694f,.814f, .72f,.84f, .704f,.872f, .643f,.844f, .58f,.817f, .514f,.808f, .477f,.826f, .446f,.851f, .412f,.855f, .406f,.822f, .445f,.805f)
        };
        var flowerPoints = new List<float>();
        for(int i=0;i<20;i++) { float a=i*Mathf.PI*2/20; flowerPoints.Add(.838f+Mathf.Cos(a)*.043f); flowerPoints.Add(.814f+Mathf.Sin(a)*.042f); }
        game.blockedSurfaces = new[] { Surface("Solid flower bed in plaza", flowerPoints.ToArray()) };
        game.walkableAreas = Array.Empty<Rect>();
        game.coreHomeUrl = "http://127.0.0.1:3100/";
        game.player.position = At(.5f,.675f); game.player.localScale = Vector3.one*2.8f;
        var positions = new[] { new Vector2(.50f,.555f), new Vector2(.27f,.49f), new Vector2(.734f,.526f), new Vector2(.381f,.55f), new Vector2(.87f,.567f), new Vector2(.747f,.843f) };
        const string matPath = "Assets/Art/Campus/NpcMarker.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if(material == null) { material=new Material(Shader.Find("Sprites/Default")); AssetDatabase.CreateAsset(material,matPath); }
        for(int i=0;i<game.npcs.Length;i++)
        {
            var npc=game.npcs[i]; npc.transform.position=At(positions[i].x,positions[i].y); npc.transform.localScale=Vector3.one*3;
            npc.spriteRenderer.color = i==2 ? new Color(1f,.8f,.75f) : Color.white;
            foreach(Transform child in npc.transform.Cast<Transform>().ToArray())
                if(child.name.StartsWith("Marker") || child.name.StartsWith("LabelShadow")) UnityEngine.Object.DestroyImmediate(child.gameObject);
            var label=npc.GetComponentInChildren<TextMeshPro>();
            label.fontSize=1.2f; label.transform.localPosition=new Vector3(0,.73f,0); label.GetComponent<MeshRenderer>().sortingOrder=23;
            var shadow=UnityEngine.Object.Instantiate(label.gameObject,npc.transform);shadow.name="LabelShadow";
            shadow.transform.localPosition=label.transform.localPosition+new Vector3(.011f,-.011f,0);
            shadow.GetComponent<TextMeshPro>().color=new Color(.07f,.12f,.09f);shadow.GetComponent<MeshRenderer>().sortingOrder=22;
            var marker=new GameObject("Marker · interaction ring");marker.transform.SetParent(npc.transform,false);
            var line=marker.AddComponent<LineRenderer>();line.sharedMaterial=material;line.useWorldSpace=false;line.loop=true;line.positionCount=40;
            line.startWidth=line.endWidth=.025f;line.sortingOrder=3;
            line.startColor=line.endColor=i==5?new Color(1f,.83f,.2f):new Color(.2f,1f,.9f);
            for(int n=0;n<40;n++){float a=n*Mathf.PI*2/40;line.SetPosition(n,new Vector3(Mathf.Cos(a)*.22f,Mathf.Sin(a)*.085f,0));}
        }
        Physics2D.SyncTransforms();
        Validate(game);
        EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
        PlayerSettings.bundleVersion="1.3.0";
        CampusSceneBuilder.BuildWebGl();
    }

    private static void Validate(DigitalCampusGame game)
    {
        var method=typeof(DigitalCampusGame).GetMethod("CanWalk",BindingFlags.NonPublic|BindingFlags.Instance);
        bool Can(Vector2 p) => (bool)method.Invoke(game,new object[]{p});
        if(!Can(game.player.position)) throw new Exception("Player spawn is outside walkable ground.");
        foreach(var npc in game.npcs) if(!Can(npc.transform.position)) throw new Exception("NPC outside path: "+npc.displayName);
        var start=(Vector2)game.player.position; const float step=.18f;
        var seen=new HashSet<Vector2Int>();var queue=new Queue<Vector2Int>();queue.Enqueue(Vector2Int.zero);seen.Add(Vector2Int.zero);
        var reached=new HashSet<CampusNpc>();var dirs=new[]{Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right};
        while(queue.Count>0)
        {
            var cell=queue.Dequeue();var point=start+(Vector2)cell*step;
            foreach(var npc in game.npcs) if(Vector2.Distance(point,npc.transform.position)<.4f) reached.Add(npc);
            foreach(var dir in dirs){var next=cell+dir;if(seen.Contains(next))continue;seen.Add(next);var p=start+(Vector2)next*step;if(p.x<game.mapMin.x||p.x>game.mapMax.x||p.y<game.mapMin.y||p.y>game.mapMax.y)continue;if(Can(p))queue.Enqueue(next);}
        }
        if(reached.Count!=game.npcs.Length) throw new Exception("Unreachable NPCs: "+string.Join(", ",game.npcs.Where(n=>!reached.Contains(n)).Select(n=>n.displayName)));
        if(Can(new Vector2(-13,-5)))throw new Exception("Pond is walkable.");
        Debug.Log("Navigation validation passed: all six NPCs reachable; pond blocked; foot collision enabled.");
    }
}
