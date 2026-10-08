using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

[InitializeOnLoad]
public static class CampusPromptUpdate
{
    const string Request = "/tmp/campus-prompt-build.request";
    static CampusPromptUpdate() { EditorApplication.delayCall += Poll; }
    static void Poll() {
        if (!File.Exists(Request)) return;
        File.Delete(Request);
        try { Apply(); CampusSceneBuilder.BuildWebGl(); File.WriteAllText("/tmp/campus-prompt-build.result", "SUCCESS"); }
        catch(Exception e) { File.WriteAllText("/tmp/campus-prompt-build.result",e.ToString()); Debug.LogException(e); }
    }
    [MenuItem("CSMJU/Apply Prompt UI and Indoor Data NPC")]
    public static void Apply() {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/DigitalCampus.unity");
        var game = UnityEngine.Object.FindFirstObjectByType<DigitalCampusGame>();
        var source = AssetDatabase.LoadAssetAtPath<Font>("Assets/Resources/Fonts/Prompt-Regular.ttf");
        const string path = "Assets/Resources/Fonts/Prompt TMP.asset";
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
        if(font == null) {
            font = TMP_FontAsset.CreateFontAsset(source,64,9,UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,2048,2048,AtlasPopulationMode.Dynamic,true);
            AssetDatabase.CreateAsset(font,path);
            AssetDatabase.AddObjectToAsset(font.material,font);
            foreach(var atlas in font.atlasTextures) AssetDatabase.AddObjectToAsset(atlas,font);
        }
        font.TryAddCharacters(string.Concat(Enumerable.Range(32,95).Concat(Enumerable.Range(0x0E00,128)).Select(c=>(char)c)));
        font.material.SetFloat(ShaderUtilities.ID_OutlineWidth,.12f);
        font.material.SetColor(ShaderUtilities.ID_OutlineColor,new Color(.02f,.03f,.05f));
        game.thaiRegular = game.thaiSemibold = source;
        foreach(var root in scene.GetRootGameObjects()) foreach(var label in root.GetComponentsInChildren<TMP_Text>(true)) {
            label.font=font; label.fontSharedMaterial=font.material;
            EditorUtility.SetDirty(label);
        }
        var data = game.npcs.First(n=>n.npcId=="data");
        // Front-right classroom floor; avoid desks, bookshelves and the corridor.
        var desired = new Vector2(109.5f,5.5f);
        var points = game.interiorWalkableFloors.Select(r=>r.center)
            .Where(p=>p.x>103 && p.y>0 && !game.interiorFurniture.Any(r=>r.Contains(p)))
            .Where(p=>!game.interiorColliders.Any(c=>c!=null && c is BoxCollider2D box && new Rect((Vector2)box.transform.TransformPoint(box.offset) - Vector2.Scale(box.size,(Vector2)box.transform.lossyScale)*.5f,Vector2.Scale(box.size,(Vector2)box.transform.lossyScale)).Overlaps(new Rect(p-Vector2.one*.4f,Vector2.one*.8f))))
            .OrderBy(p=>(p-desired).sqrMagnitude).ToArray();
        if(points.Length==0) throw new Exception("No clear indoor NPC position");
        data.transform.position=new Vector3(points[0].x,points[0].y,0);
        data.transform.localScale=Vector3.one; data.gameObject.SetActive(false);
        game.coreHomeUrl="/portal";
        EditorUtility.SetDirty(game); EditorUtility.SetDirty(data); EditorUtility.SetDirty(font); EditorUtility.SetDirty(font.material);
        EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
    }
}
