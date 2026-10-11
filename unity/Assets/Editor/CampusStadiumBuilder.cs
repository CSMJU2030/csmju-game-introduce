using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class CampusStadiumBuilder
{
    public static void Apply(DigitalCampusGame game,Transform world)
    {
        const string path="Assets/Art/Campus/campus-stadium.png";
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);
        if(importer==null)throw new Exception("Stadium art is missing");
        importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
        importer.spritePixelsPerUnit=64;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;
        importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
        var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);
        game.stadiumTexture=sprite.texture;
        foreach(var name in new[]{"Sports stadium","Stadium entrance · open walkway","Stadium solids · stands goals lights planters"}) {
            var old=world.Find(name);if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);
        }
        var root=new GameObject("Sports stadium");root.transform.SetParent(world,false);
        Vector2 At(Vector2 uv)=>game.mapMin+Vector2.Scale(uv,game.mapMax-game.mapMin);
        var uvRect=CampusStadiumLayout.MapUv;var size=Vector2.Scale(uvRect.size,game.mapMax-game.mapMin);
        root.transform.position=At(uvRect.center);
        var renderer=root.AddComponent<SpriteRenderer>();renderer.sprite=sprite;renderer.sortingOrder=2;
        root.transform.localScale=new Vector3(size.x/sprite.bounds.size.x/world.lossyScale.x,size.y/sprite.bounds.size.y/world.lossyScale.y,1);
        // Match the existing paving at the west gate, linking to the diagonal path.
        var bridge=new GameObject("Stadium entrance · open walkway");bridge.transform.SetParent(world,false);
        var paving=Sprite.Create(game.campusMapTexture,new Rect(820,195,24,24),new Vector2(.5f,.5f),24);
        // Persist a reference sprite so the scene is editable after reopening Unity.
        const string pavingPath="Assets/Art/Campus/Stadium entrance paving.asset";
        var saved=AssetDatabase.LoadAssetAtPath<Sprite>(pavingPath);
        if(saved==null){AssetDatabase.CreateAsset(paving,pavingPath);saved=paving;}else UnityEngine.Object.DestroyImmediate(paving);
        for(int i=1;i<CampusStadiumLayout.EntranceRoute.Length;i++) {
            var a=At(CampusStadiumLayout.EntranceRoute[i-1]);var b=At(CampusStadiumLayout.EntranceRoute[i]);var d=b-a;
            var segment=new GameObject("Paved connection "+i);segment.transform.SetParent(bridge.transform,false);segment.transform.position=(a+b)*.5f;
            segment.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(d.y,d.x)*Mathf.Rad2Deg);
            var road=segment.AddComponent<SpriteRenderer>();road.sprite=saved;road.sortingOrder=3;road.drawMode=SpriteDrawMode.Tiled;
            road.size=new Vector2(d.magnitude+.8f,(game.mapMax.x-game.mapMin.x)*.026f)/world.lossyScale.x;
        }
        var gate=CampusStadiumLayout.EntranceUv;var gatePiece=new GameObject("West gate paving");gatePiece.transform.SetParent(bridge.transform,false);gatePiece.transform.position=At(gate.center);
        var sr=gatePiece.AddComponent<SpriteRenderer>();sr.sprite=saved;sr.sortingOrder=3;sr.drawMode=SpriteDrawMode.Tiled;
        sr.size=Vector2.Scale(gate.size,game.mapMax-game.mapMin)/world.lossyScale.x;
        // Remove the former plaza flowerbed: there is now grass at that position.
        foreach(var blocked in game.blockedSurfaces)if(blocked!=null)UnityEngine.Object.DestroyImmediate(blocked.gameObject);
        game.blockedSurfaces=Array.Empty<PolygonCollider2D>();
        var blockers=new GameObject("Stadium solids · stands goals lights planters");blockers.transform.SetParent(world,false);
        for(int i=0;i<CampusStadiumLayout.Solids.Length;i++) {
            var r=CampusStadiumLayout.Solids[i];var go=new GameObject(i==0?"Grandstand":i<3?"Goal frame "+i:i<7?"Floodlight base "+i:"Planter "+i);
            go.transform.SetParent(blockers.transform,false);go.transform.position=CampusStadiumLayout.At(r.center,game.mapMin,game.mapMax);
            var collider=go.AddComponent<BoxCollider2D>();collider.isTrigger=true;
            collider.size=Vector2.Scale(Vector2.Scale(r.size,uvRect.size),game.mapMax-game.mapMin)/world.lossyScale.x;
        }
        var host=game.npcs.First(n=>n.npcId=="openhouse");host.transform.position=CampusStadiumLayout.At(new Vector2(.083f,.60f),game.mapMin,game.mapMax);
    }
    public static void Validate(DigitalCampusGame game)
    {
        var method=typeof(DigitalCampusGame).GetMethod("CanWalk",BindingFlags.NonPublic|BindingFlags.Instance);
        bool Can(Vector2 p)=>(bool)method.Invoke(game,new object[]{p});
        var targets=new[]{new Vector2(.083f,.60f),new Vector2(.54f,.43f),new Vector2(.54f,.68f),new Vector2(.54f,.18f)};
        foreach(var p in targets)if(!Can(CampusStadiumLayout.At(p,game.mapMin,game.mapMax)))throw new Exception("Stadium playable floor is blocked: "+p);
        foreach(var r in CampusStadiumLayout.Solids)if(Can(CampusStadiumLayout.At(r.center,game.mapMin,game.mapMax)))throw new Exception("Stadium obstacle is walkable: "+r);
        // Check reachability with the full hero footprint, including the west gate.
        const float step=.18f;var start=(Vector2)game.player.position;
        var seen=new HashSet<Vector2Int>{Vector2Int.zero};var pending=new Queue<Vector2Int>();pending.Enqueue(Vector2Int.zero);
        var reached=new bool[targets.Length];var parents=new Dictionary<Vector2Int,Vector2Int>();Vector2Int goal=Vector2Int.zero;
        while(pending.Count>0) {
            var cell=pending.Dequeue();var point=start+(Vector2)cell*step;
            for(int i=0;i<targets.Length;i++)if(Vector2.Distance(point,CampusStadiumLayout.At(targets[i],game.mapMin,game.mapMax))<.3f){reached[i]=true;if(i==1)goal=cell;}
            foreach(var d in new[]{Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right}) {
                var next=cell+d;if(!seen.Add(next))continue;var p=start+(Vector2)next*step;
                if(p.x<game.mapMin.x || p.x>game.mapMax.x || p.y<game.mapMin.y || p.y>game.mapMax.y || !Can(p))continue;
                parents[next]=cell;pending.Enqueue(next);
            }
        }
        if(reached.Any(v=>!v))throw new Exception("Stadium entrance/field/track disconnected from campus: "+string.Join(",",reached));
        var route=new List<Vector2Int>();var current=goal;
        while(current!=Vector2Int.zero){route.Add(current);current=parents[current];}route.Reverse();
        var segments=new List<string>();var previous=Vector2Int.zero;var direction=Vector2Int.zero;int count=0;
        foreach(var cell in route){var d=cell-previous;if(d!=direction && count>0){segments.Add("["+direction.x+","+direction.y+","+(count*step).ToString(System.Globalization.CultureInfo.InvariantCulture)+"]");count=0;}direction=d;count++;previous=cell;}
        if(count>0)segments.Add("["+direction.x+","+direction.y+","+(count*step).ToString(System.Globalization.CultureInfo.InvariantCulture)+"]");
        System.IO.File.WriteAllText("/tmp/campus-stadium-route.json","["+string.Join(",",segments)+"]");
        Debug.Log("STADIUM VALIDATION PASSED: west entrance, football field and both track straights reachable; stands/goals/lights blocked.");
    }
}
