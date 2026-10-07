using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

[InitializeOnLoad]
public static class CampusAuthoredInterior
{
    static CampusAuthoredInterior(){EditorApplication.update+=Tick;}
    static void Tick(){
        const string request="/tmp/campus-authored.request";
        if(!File.Exists(request)||EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)return;
        File.Delete(request);File.WriteAllText("/tmp/campus-authored.result","BUILDING");
        try{Apply();CampusSceneBuilder.BuildWebGl();File.WriteAllText("/tmp/campus-authored.result","SUCCESS");}
        catch(Exception ex){File.WriteAllText("/tmp/campus-authored.result",ex.ToString());Debug.LogException(ex);}
    }
    [MenuItem("CSMJU/Connect Authored Interior and Font")]
    public static void Apply(){
        var game=UnityEngine.Object.FindFirstObjectByType<DigitalCampusGame>();
        if(game==null)throw new Exception("Open DigitalCampus scene first");
        var scene=game.gameObject.scene;
        Directory.CreateDirectory("Assets/SceneBackups");
        EditorSceneManager.SaveScene(scene,"Assets/SceneBackups/Authored_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+".unity",true);
        var root=game.buildingInterior;
        var maps=root.GetComponentsInChildren<Tilemap>(true);
        var room=maps.First(m=>m.name=="layer1");
        var furniture=maps.First(m=>m.name=="layer2");
        var previous=root.transform.Find("Collision · authored interior");
        if(previous!=null)UnityEngine.Object.DestroyImmediate(previous.gameObject);
        var collision=new GameObject("Collision · authored interior");collision.transform.SetParent(root.transform,false);
        var colliders=new List<Collider2D>();var floors=new List<Rect>();
        var floor=room.GetTile(new Vector3Int(90,0,0));
        if(floor==null)throw new Exception("Expected floor tile at 90,0");
        void Block(Tilemap map,Vector3Int p,string name){
            var go=new GameObject(name+" "+p.x+","+p.y);go.transform.SetParent(collision.transform,false);
            var origin=map.transform.TransformPoint((Vector3)p);var end=map.transform.TransformPoint((Vector3)(p+new Vector3Int(1,1,0)));
            go.transform.position=(origin+end)*.5f;
            var c=go.AddComponent<BoxCollider2D>();c.size=new Vector2(Mathf.Abs(end.x-origin.x),Mathf.Abs(end.y-origin.y));colliders.Add(c);
        }
        foreach(var p in room.cellBounds.allPositionsWithin){
            var tile=room.GetTile(p);if(tile==null)continue;
            var origin=room.transform.TransformPoint((Vector3)p);var end=room.transform.TransformPoint((Vector3)(p+new Vector3Int(1,1,0)));
            if(tile==floor)floors.Add(new Rect(origin.x,origin.y,end.x-origin.x,end.y-origin.y));
            else Block(room,p,"Wall");
        }
        foreach(var p in furniture.cellBounds.allPositionsWithin)if(furniture.HasTile(p))Block(furniture,p,"Furniture");
        game.interiorColliders=colliders.ToArray();game.interiorFurniture=Array.Empty<Rect>();game.interiorWalkableFloors=floors.ToArray();
        // Start in the clear lower-right corridor. The previous spawn was beside
        // the locker bank and could overlap a furniture collider on entry.
        game.interiorSpawn=room.transform.TransformPoint(new Vector3(110.5f,-6.5f,0));
        game.interiorCameraCenter=room.transform.TransformPoint(new Vector3(102.5f,1.5f,0));
        var teacher=game.npcs.First(n=>n.npcId=="curriculum");teacher.transform.position=room.transform.TransformPoint(new Vector3(94.5f,5.5f,0));teacher.transform.localScale=Vector3.one;
        teacher.gameObject.SetActive(false);
        // Unity can retain a destroyed Tilemap reference after a build refresh;
        // ignore that stale entry while applying renderer ordering.
        foreach(var m in maps) if(m != null) {
            var renderer=m.GetComponent<TilemapRenderer>();
            if(renderer != null) renderer.sortingOrder=m==room?-6:m==furniture?0:-12;
        }
        game.playerRenderer.sortingOrder=10;
        game.playerCollider=game.player.GetComponent<BoxCollider2D>();
        if(game.playerCollider==null){
            game.playerCollider=game.player.gameObject.AddComponent<BoxCollider2D>();
            game.playerCollider.size=new Vector2(.56f,.36f);
            game.playerCollider.offset=Vector2.zero;
        }
        game.playerCollider.isTrigger=false;
        game.playerBody=game.player.GetComponent<Rigidbody2D>();
        if(game.playerBody==null)game.playerBody=game.player.gameObject.AddComponent<Rigidbody2D>();
        game.playerBody.bodyType=RigidbodyType2D.Kinematic;
        game.playerBody.gravityScale=0;
        game.playerBody.constraints=RigidbodyConstraints2D.FreezeRotation;
        teacher.spriteRenderer.sortingOrder=10;
        var font=AssetDatabase.LoadAssetAtPath<Font>("Assets/Resources/Fonts/TAGameboy-Regular.otf");game.thaiRegular=game.thaiSemibold=font;
        const string fontPath="Assets/Resources/Fonts/TA Game Boy TMP.asset";
        var tmp=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
        if(tmp==null){
            tmp=TMP_FontAsset.CreateFontAsset(font,48,6,UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,1024,1024,AtlasPopulationMode.Dynamic,true);
            AssetDatabase.CreateAsset(tmp,fontPath);AssetDatabase.AddObjectToAsset(tmp.material,tmp);
            foreach(var atlas in tmp.atlasTextures)AssetDatabase.AddObjectToAsset(atlas,tmp);
        }
        var chars=string.Concat(Enumerable.Range(32,95).Concat(Enumerable.Range(0x0E00,0x80)).Select(c=>(char)c));
        tmp.TryAddCharacters(chars);
        // Use actual glyph bounds; the source font's large typographic margins
        // otherwise push captions outside their UI rectangles.
        var face=tmp.faceInfo;
        face.ascentLine=tmp.glyphTable.Max(g=>g.metrics.horizontalBearingY)+2;
        face.descentLine=tmp.glyphTable.Min(g=>g.metrics.horizontalBearingY-g.metrics.height)-2;
        face.lineHeight=face.ascentLine-face.descentLine+2;
        tmp.faceInfo=face;
        tmp.atlasPopulationMode=AtlasPopulationMode.Dynamic;
        tmp.material.SetFloat(ShaderUtilities.ID_OutlineWidth,.24f);tmp.material.SetColor(ShaderUtilities.ID_OutlineColor,new Color(.02f,.03f,.05f));
        foreach(var r in scene.GetRootGameObjects()){
            foreach(var old in r.GetComponentsInChildren<TextMesh>(true)){
                var go=old.gameObject;var content=old.text;var color=old.color;var size=old.characterSize*10;
                UnityEngine.Object.DestroyImmediate(old);var renderer=go.GetComponent<MeshRenderer>();if(renderer!=null)UnityEngine.Object.DestroyImmediate(renderer);
                var text=go.AddComponent<TextMeshPro>();text.text=content;text.color=color;text.fontSize=Mathf.Max(.8f,size);text.alignment=TextAlignmentOptions.Center;
                text.rectTransform.sizeDelta=new Vector2(18,3);
                text.fontSize=Mathf.Max(1.6f,size*1.6f);
            }
            foreach(var text in r.GetComponentsInChildren<TMP_Text>(true)){
                text.font=tmp;text.fontSharedMaterial=tmp.material;text.GetComponent<Renderer>().sortingOrder=23;
                if(r.name.StartsWith("NPC")){
                    text.fontSize=4f;text.rectTransform.sizeDelta=new Vector2(15,2);
                    text.color=Color.white;text.gameObject.SetActive(text.name!="LabelShadow");
                    text.transform.localPosition=new Vector3(0,1.1f,0);
                }
                EditorUtility.SetDirty(text);
            }
        }
        root.SetActive(false);EditorUtility.SetDirty(game);EditorUtility.SetDirty(tmp);EditorUtility.SetDirty(tmp.material);
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        Debug.Log("Authored interior connected: "+floors.Count+" floor cells; "+colliders.Count+" colliders");
    }
}
