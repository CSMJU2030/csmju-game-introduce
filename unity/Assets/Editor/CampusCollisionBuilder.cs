using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class CampusCollisionBuilder
{
    // Footprints in the original full-map pixels (top-left origin). These are
    // physical props that share paving colours and cannot be classified by RGB.
    static Rect Pixels(float x,float y,float w,float h)=>new Rect(x/1672f,1-(y+h)/941f,w/1672f,h/941f);
    public static readonly Rect[] OutdoorProps={
        Pixels(51,324,42,23),Pixels(281,383,53,28),Pixels(571,375,57,29),
        Pixels(674,422,63,31),Pixels(915,421,65,31),Pixels(1359,409,65,29),
        Pixels(700,575,88,32),Pixels(887,575,88,33),Pixels(476,829,68,29),
        Pixels(371,346,34,28),Pixels(515,345,33,29), // Stair planters.
        Pixels(1167,339,31,34),Pixels(1260,336,29,33),
        Pixels(156,355,18,21),Pixels(382,408,18,23),Pixels(517,408,19,23),
        Pixels(733,434,21,23),Pixels(899,435,21,23),Pixels(975,585,18,23),
        Pixels(679,585,18,22),Pixels(427,521,19,20),Pixels(1150,447,20,24),
        Pixels(1307,446,19,24),Pixels(1459,481,20,24),Pixels(828,169,18,20),
        Pixels(848,205,19,22),Pixels(787,327,19,22),Pixels(429,843,20,24),
        Pixels(572,822,19,24), // Lamp bases; tall decorative tops don't close paths.
        Pixels(26,655,150,17),Pixels(47,619,215,17), // Lake bridge rails.
        Pixels(475,811,103,16),Pixels(354,827,79,17)
    };
    public static bool OutdoorBlocked(Vector2 uv)=>OutdoorProps.Any(r=>r.Contains(uv));
    public static void BuildOutdoor(DigitalCampusGame game,Transform world)
    {
        var old=world.Find("Solid campus props");if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);
        var root=new GameObject("Solid campus props");root.transform.SetParent(world,false);
        var colliders=new List<Collider2D>();
        for(int i=0;i<OutdoorProps.Length;i++){
            var uv=OutdoorProps[i];var min=game.mapMin+Vector2.Scale(uv.min,game.mapMax-game.mapMin);
            var size=Vector2.Scale(uv.size,game.mapMax-game.mapMin);
            colliders.Add(Box(root.transform,new Rect(min,size),"Bench planter lamp or rail "+i));
        }
        var stadium=world.Find("Stadium solids · stands goals lights planters");
        if(stadium!=null)colliders.AddRange(stadium.GetComponentsInChildren<Collider2D>(true));
        game.outdoorSolids=colliders.ToArray();
    }
    public static Rect SpriteBounds(Tilemap map,Vector3Int cell)
    {
        var sprite=map.GetSprite(cell);
        if(sprite==null)return CellBounds(map,cell);
        var b=sprite.bounds;
        var matrix=map.transform.localToWorldMatrix*Matrix4x4.Translate(LocalCell(map,(Vector3)cell+map.tileAnchor))*map.orientationMatrix*map.GetTransformMatrix(cell);
        var points=new[]{new Vector3(b.min.x,b.min.y),new Vector3(b.max.x,b.min.y),new Vector3(b.min.x,b.max.y),new Vector3(b.max.x,b.max.y)}.Select(matrix.MultiplyPoint3x4).ToArray();
        var min=new Vector2(points.Min(p=>p.x),points.Min(p=>p.y));
        return new Rect(min,new Vector2(points.Max(p=>p.x),points.Max(p=>p.y))-min);
    }
    // Authored maps can be standalone Tilemaps without a parent Grid.
    static Vector3 LocalCell(Tilemap map,Vector3 cell)=>map.layoutGrid!=null?map.CellToLocalInterpolated(cell):Vector3.Scale(cell,map.cellSize);
    public static Rect CellBounds(Tilemap map,Vector3Int cell)
    {
        var a=map.transform.TransformPoint(LocalCell(map,cell));var b=map.transform.TransformPoint(LocalCell(map,cell+new Vector3Int(1,1,0)));
        return Rect.MinMaxRect(Mathf.Min(a.x,b.x),Mathf.Min(a.y,b.y),Mathf.Max(a.x,b.x),Mathf.Max(a.y,b.y));
    }
    static BoxCollider2D Box(Transform parent,Rect r,string name)
    {
        var go=new GameObject(name);go.transform.SetParent(parent,false);go.transform.position=r.center;
        var box=go.AddComponent<BoxCollider2D>();box.isTrigger=true;
        box.size=Vector2.Scale(r.size,new Vector2(1/Mathf.Abs(go.transform.lossyScale.x),1/Mathf.Abs(go.transform.lossyScale.y)));
        return box;
    }
    public static void BuildInterior(DigitalCampusGame game,Tilemap room,Tilemap[] maps)
    {
        var old=game.buildingInterior.transform.Find("Collision · authored interior");
        if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);
        var root=new GameObject("Collision · authored interior");root.transform.SetParent(game.buildingInterior.transform,false);
        var solid=new List<Collider2D>();var floors=new List<Rect>();var floor=room.GetTile(new Vector3Int(90,0,0));
        if(floor==null)throw new Exception("Authored floor reference missing");
        foreach(var cell in room.cellBounds.allPositionsWithin) {
            if(!room.HasTile(cell))continue;
            if(room.GetTile(cell)==floor)floors.Add(CellBounds(room,cell));
            else solid.Add(Box(root.transform,CellBounds(room,cell),"Wall "+cell));
        }
        // Cover the rendered footprint, including multi-cell cabinets and boards.
        // Scanning all prop layers also picks up furniture added on another layer.
        foreach(var map in maps.Where(m=>m!=null && m!=room && m.name!="layer0"))
            foreach(var cell in map.cellBounds.allPositionsWithin)if(map.HasTile(cell))
                solid.Add(Box(root.transform,SpriteBounds(map,cell),"Furniture "+map.GetTile(cell).name+" "+cell));
        game.interiorColliders=solid.ToArray();game.interiorFurniture=Array.Empty<Rect>();game.interiorWalkableFloors=floors.ToArray();
    }
    public static void ApplyPixelMaterials(DigitalCampusGame game,SpriteRenderer ground,SpriteRenderer canopy,Tilemap[] maps)
    {
        Material Get(string name,Vector4 grid) {
            var shader=Shader.Find("Campus/8 Bit Background");if(shader==null)throw new Exception("8-bit shader missing");
            var path="Assets/Art/Campus/"+name+".mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(material==null){material=new Material(shader);AssetDatabase.CreateAsset(material,path);}
            material.shader=shader;material.SetVector("_PixelGrid",grid);EditorUtility.SetDirty(material);return material;
        }
        game.campusMapMaterial=Get("Campus 8 Bit",new Vector4(512,288,0,0));ground.sharedMaterial=game.campusMapMaterial;
        canopy.sharedMaterial.SetVector("_PixelGrid",new Vector4(512,288,0,0));EditorUtility.SetDirty(canopy.sharedMaterial);
        var indoor=Get("Interior 8 Bit",Vector4.zero);
        foreach(var map in maps.Where(m=>m!=null)){var renderer=map.GetComponent<TilemapRenderer>();if(renderer!=null)renderer.sharedMaterial=indoor;}
    }
}
