using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class CampusStadiumBuilder
{
    public const string MapPath="Assets/Art/Campus/campus-map-stadium.png";
    public static void LoadMap(DigitalCampusGame game,SpriteRenderer ground)
    {
        var importer=(TextureImporter)AssetImporter.GetAtPath(MapPath);
        if(importer==null)throw new Exception("Edited campus map is missing");
        var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(MapPath);
        importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
        importer.spritePixelsPerUnit=texture.width/40f;
        var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);
        settings.spriteMeshType=SpriteMeshType.FullRect;importer.SetTextureSettings(settings);
        importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;
        importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
        ground.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(MapPath);
        ground.drawMode=SpriteDrawMode.Sliced;
        ground.size=(game.mapMax-game.mapMin)/ground.transform.lossyScale.x;
        game.campusMapTexture=ground.sprite.texture;
    }
    public static void Apply(DigitalCampusGame game,Transform world)
    {
        // Retire the old patch and connector sprites. The stadium now belongs
        // to the complete campus texture, shared by the world and mini map.
        foreach(var name in new[]{"Sports stadium","Stadium entrance · open walkway","Stadium solids · stands goals lights planters"}) {
            var old=world.Find(name);if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);
        }
        // Remove the former plaza flowerbed: there is now grass at that position.
        foreach(var blocked in game.blockedSurfaces)if(blocked!=null)UnityEngine.Object.DestroyImmediate(blocked.gameObject);
        game.blockedSurfaces=Array.Empty<PolygonCollider2D>();
        var blockers=new GameObject("Stadium solids · stands goals lights planters");blockers.transform.SetParent(world,false);
        for(int i=0;i<CampusStadiumLayout.Solids.Length;i++) {
            var r=CampusStadiumLayout.Solids[i];var go=new GameObject(i==0?"Grandstand":i<3?"Goal frame "+i:"Lamp base "+i);
            go.transform.SetParent(blockers.transform,false);go.transform.position=CampusStadiumLayout.At(r.center,game.mapMin,game.mapMax);
            var collider=go.AddComponent<BoxCollider2D>();collider.isTrigger=true;
            collider.size=Vector2.Scale(r.size,game.mapMax-game.mapMin)/world.lossyScale.x;
        }
        var host=game.npcs.First(n=>n.npcId=="openhouse");host.transform.position=CampusStadiumLayout.At(CampusStadiumLayout.HostUv,game.mapMin,game.mapMax);
    }
    public static void Validate(DigitalCampusGame game)
    {
        var method=typeof(DigitalCampusGame).GetMethod("CanWalk",BindingFlags.NonPublic|BindingFlags.Instance);
        bool Can(Vector2 p)=>(bool)method.Invoke(game,new object[]{p});
        var targets=CampusStadiumLayout.Targets;
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
        Debug.Log("STADIUM VALIDATION PASSED: entrance, football field and both track straights reachable; stands/goals/lights blocked.");
    }
}
