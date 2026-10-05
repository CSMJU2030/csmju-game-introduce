using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TMPro;

public static class CampusExpansionBuild
{
    private const string Art="Assets/Art/ModernCampus/";
    private static Sprite Load(string n) => AssetDatabase.LoadAssetAtPath<Sprite>(Art+n+".png");
    private static Sprite[] Frames(string name) => AssetDatabase.LoadAllAssetsAtPath(Art+name+".png").OfType<Sprite>().OrderBy(s=>s.name).ToArray();
    [MenuItem("CSMJU/Build Modern Campus Expansion")]
    public static void Build()
    {
        AssetDatabase.Refresh();
        var authored=UnityEngine.Object.FindFirstObjectByType<DigitalCampusGame>();
        if(authored!=null && authored.buildingInterior!=null && authored.buildingInterior.GetComponentsInChildren<UnityEngine.Tilemaps.Tilemap>(true).Length>0){
            CampusAuthoredInterior.Apply();CampusSceneBuilder.BuildWebGl();return;
        }
        string[] names={"Adam","Abby","Edward","Lucy","Bob","Josh","Jenny","Alex","Molly","Oscar"};
        foreach(string path in System.IO.Directory.GetFiles(Art,"*.png"))
        {
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.spritePixelsPerUnit=48;
            string name=System.IO.Path.GetFileNameWithoutExtension(path);
            if(names.Contains(name))
            {
                importer.spriteImportMode=SpriteImportMode.Multiple;
                importer.spritesheet=Enumerable.Range(0,24).Select(i=>new SpriteMetaData {name=name+"_"+i.ToString("00"),rect=new Rect(i%6*48,(3-i/6)*48,48,48),pivot=new Vector2(.5f,.08f)}).ToArray();
            }
            else importer.spriteImportMode=SpriteImportMode.Single;
            importer.SaveAndReimport();
        }
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/DigitalCampus.unity");
        var game=UnityEngine.Object.FindFirstObjectByType<DigitalCampusGame>();
        game.walkSprites=Frames("Adam");game.playerRenderer.sprite=game.walkSprites[0];game.player.localScale=Vector3.one*1.6f;
        string[] chars={"Josh","Bob","Abby","Lucy","Edward","Jenny"};
        for(int i=0;i<game.npcs.Length;i++)
        {
            var npc=game.npcs[i];npc.spriteRenderer.sprite=Frames(chars[i])[0];npc.spriteRenderer.color=Color.white;npc.transform.localScale=Vector3.one*1.6f;
            foreach(var label in npc.GetComponentsInChildren<TextMeshPro>(true)) { label.fontSize=1.3f;label.transform.localPosition=new Vector3(label.transform.localPosition.x,1.1f,0); }
            npc.gameObject.SetActive(npc.npcId!="bug" && npc.npcId!="curriculum");
        }
        game.bugSprites=new[]{Load("BugDemon")};game.bugAttackSprites=new[]{Load("BugDemon")};
        game.battleBackdrop=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Campus/campus-map.png");
        foreach(var obj in scene.GetRootGameObjects())if(obj.name.StartsWith("Building60") || obj.name.StartsWith("Entrance60"))UnityEngine.Object.DestroyImmediate(obj);
        CampusInteriorLayout.Apply(game);
        game.buildingEntrance=new Vector2(-9.2f,1f);
        var entrance=new GameObject("Entrance60 · E to enter");entrance.transform.position=new Vector3(-9.2f,2.8f,0);
        var text=entrance.AddComponent<TextMeshPro>();text.text="[E] อาคาร 60 ปี · ชั้น 6";text.font=TMP_FontAsset.CreateFontAsset(game.thaiSemibold);text.fontSize=1.2f;text.alignment=TextAlignmentOptions.Center;text.color=Color.white;
        var renderer=entrance.GetComponent<MeshRenderer>();renderer.sharedMaterial=game.thaiSemibold.material;renderer.sortingOrder=24;
        Require(game.walkSprites.Length==24,"24 player direction frames");
        Require(game.npcs.Where(n=>n.npcId!="bug").Select(n=>n.spriteRenderer.sprite).Distinct().Count()==5,"Unique NPC appearances");
        CampusActivitiesBuild.CheckRules();
        for(int seed=0;seed<100;seed++)
        {
            var b=new BugBattle(seed);
            for(int i=0;i<60 && !b.Won && !b.Lost;i++)b.Act(b.Energy<2 || b.Intent==1 ? 2 : b.PlayerHp<=10 && b.Patches>0 ? 3 : b.Intent==2?1:0);
            Require(b.Won,"Demon tactical victory seed "+seed);
        }
        Debug.Log("EXPANSION CHECKS PASSED: unique characters, 24 directional frames, 100 tactical demon victories");
        EditorUtility.SetDirty(game);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();CampusSceneBuilder.BuildWebGl();
    }
    private static void Require(bool condition,string message){if(!condition)throw new Exception(message);}
}
