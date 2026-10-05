using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public static class CampusSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/DigitalCampus.unity";
    private const string CharacterPath = "Assets/Resources/Characters/ManaSeed/character_walk.png";
    private static readonly string[] ZoneIds = { "welcome_zone", "curriculum_hall", "software_lab", "ai_data_cave", "iot_garden" };
    private static readonly string[] NpcIds = { "mentor", "curriculum", "bug", "data", "iot" };
    private static readonly string[] NpcNames = { "พี่โค้ด", "อาจารย์ Algorithm", "น้องบั๊ก", "พี่ดาต้า", "เจ้าหน้าที่ Lab" };
    private static readonly string[] NpcRoles = { "รุ่นพี่ปี 3 · Web Developer", "อาจารย์แนะแนวหลักสูตร", "คู่ปรับสาย Debug", "ศิษย์เก่าสาย Data / AI", "ผู้ดูแลห้องปฏิบัติการ" };

    [MenuItem("CSMJU/Build Editable Campus Scene")]
    public static void BuildScene()
    {
        EnsureSpriteImporters();
        var walk = AssetDatabase.LoadAllAssetsAtPath(CharacterPath).OfType<Sprite>().ToArray();
        if (walk.Length < 24) throw new Exception("Mana Seed sprite sheet did not import 24 walk frames.");
        Sprite SpriteAt(string prefix, int direction = 0) => walk.FirstOrDefault(s => s.name == $"{prefix}_{direction}_0");
        // TA Game Boy is the single pixel font used by every in-game label and UI style.
        // Both fields intentionally reference the same asset because the font has one weight.
        var regular = Resources.Load<Font>("Fonts/TAGameboy-Regular");
        var semibold = regular;

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var world = new GameObject("World · Five campus zones");
        for (var i = 0; i < ZoneIds.Length; i++)
        {
            var center = -28f + i * 14f;
            var zone = new GameObject($"0{i + 1} · {ZoneIds[i]}");
            zone.transform.SetParent(world.transform, false);
            zone.transform.position = new Vector3(center, 0, 0);
            var renderer = zone.AddComponent<SpriteRenderer>();
            renderer.sprite = Resources.Load<Sprite>("Art/" + ZoneIds[i]);
            renderer.sortingOrder = -20;
            AddLabel(zone.transform, new Vector3(0, 4.6f, 0), new[] { "WELCOME ZONE", "CURRICULUM HALL", "SOFTWARE LAB", "AI & DATA CAVE", "IOT GARDEN" }[i], semibold, 0.20f, new Color(.1f, .18f, .14f));
        }
        var playerObject = new GameObject("Player · Mana Seed Adventurer");
        playerObject.transform.position = new Vector3(-32f, -2.5f, 0);
        var playerSprite = playerObject.AddComponent<SpriteRenderer>(); playerSprite.sprite = walk.First(s => s.name == "walk_0_0"); playerSprite.sortingOrder = 5;
        playerObject.transform.localScale = new Vector3(1.15f, 1.15f, 1);

        var npcObjects = new CampusNpc[NpcIds.Length + 1];
        var colors = new[] { new Color(1f,.83f,.55f), new Color(.67f,.83f,1f), new Color(1f,.54f,.47f), new Color(.83f,.69f,1f), new Color(.59f,.92f,.72f) };
        for (var i = 0; i < NpcIds.Length; i++)
        {
            var npcObject = new GameObject($"NPC · {NpcNames[i]}");
            npcObject.transform.position = new Vector3(-31f + i * 14f, -2.4f, 0);
            npcObject.transform.localScale = new Vector3(.95f, .95f, 1);
            var sprite = npcObject.AddComponent<SpriteRenderer>(); sprite.sprite = SpriteAt("walk", 0); sprite.color = colors[i]; sprite.sortingOrder = 4;
            var npc = npcObject.AddComponent<CampusNpc>(); npc.npcId = NpcIds[i]; npc.displayName = NpcNames[i]; npc.role = NpcRoles[i]; npc.spriteRenderer = sprite;
            npcObjects[i] = npc;
            AddLabel(npcObject.transform, new Vector3(0, 1.25f, 0), NpcNames[i], semibold, .14f, Color.white);
        }
        var openHouse = new GameObject("Open House · Ending Gate"); openHouse.transform.position = new Vector3(33.2f, -2.4f, 0);
        var gateSprite = openHouse.AddComponent<SpriteRenderer>(); gateSprite.sprite = SpriteAt("walk", 0); gateSprite.color = new Color(1f,.82f,.27f); gateSprite.sortingOrder = 4;
        var gateNpc = openHouse.AddComponent<CampusNpc>(); gateNpc.npcId = "openhouse"; gateNpc.displayName = "Open House Day"; gateNpc.role = "ปลดล็อกเมื่อเก็บ Skill Badge ครบ 6 ชิ้น"; gateNpc.spriteRenderer = gateSprite;
        npcObjects[5] = gateNpc; AddLabel(openHouse.transform, new Vector3(0, 1.25f, 0), "OPEN HOUSE", semibold, .14f, new Color(1f,.92f,.56f));

        var cameraObject = new GameObject("Main Camera"); cameraObject.tag = "MainCamera"; cameraObject.transform.position = new Vector3(-28, 0, -10);
        var camera = cameraObject.AddComponent<Camera>(); camera.orthographic = true; camera.orthographicSize = 6.2f; camera.backgroundColor = new Color(.58f,.75f,.68f); camera.clearFlags = CameraClearFlags.SolidColor;
        cameraObject.AddComponent<AudioListener>();

        var gameObject = new GameObject("CSMJU Quest · Game Systems");
        var game = gameObject.AddComponent<DigitalCampusGame>();
        game.player = playerObject.transform; game.playerRenderer = playerSprite; game.gameCamera = camera; game.npcs = npcObjects;
        game.walkSprites = new Sprite[24];
        for (var d = 0; d < 4; d++) for (var f = 0; f < 6; f++) game.walkSprites[d * 6 + f] = walk.First(s => s.name == $"walk_{d}_{f}");
        game.thaiRegular = regular; game.thaiSemibold = semibold;

        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
        Debug.Log("Built editable Digital Campus scene. Scene content is under World, Player, NPCs, Main Camera and Game Systems.");
    }

    [MenuItem("CSMJU/Apply Illustrated Campus")]
    public static void ApplyIllustratedCampus()
    {
        const string artPath = "Assets/Art/Campus/campus-map.png";
        AssetDatabase.Refresh();
        var probe = new Texture2D(2, 2); probe.LoadImage(File.ReadAllBytes(artPath));
        float width = 40f, height = 40f * probe.height / probe.width;
        float pixelsPerUnit = probe.width / width;
        UnityEngine.Object.DestroyImmediate(probe);
        var importer = (TextureImporter)AssetImporter.GetAtPath(artPath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = pixelsPerUnit;
        importer.maxTextureSize = 4096;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.CompressedHQ;
        importer.SaveAndReimport();
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        foreach (var root in scene.GetRootGameObjects())
            if (root.name.StartsWith("World")) UnityEngine.Object.DestroyImmediate(root);
        var world = new GameObject("World · Illustrated Maejo Campus");
        var renderer = world.AddComponent<SpriteRenderer>();
        renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(artPath);
        renderer.sortingOrder = -20;
        var game = UnityEngine.Object.FindFirstObjectByType<DigitalCampusGame>();
        Vector3 At(float u, float v) => new Vector3((u - .5f) * width, (.5f - v) * height, 0);
        Rect Area(float x, float y, float w, float h) => new Rect((x - .5f) * width, (.5f - y - h) * height, w * width, h * height);
        game.mapMin = new Vector2(-width / 2, -height / 2);
        game.mapMax = new Vector2(width / 2, height / 2);
        game.cameraViewSize = height * .495f; game.campusOverview = true;
        game.player.position = At(.50f, .65f);
        game.player.localScale = Vector3.one * 2.2f;
        var locations = new[] { new Vector2(.5f,.55f), new Vector2(.268f,.445f), new Vector2(.735f,.49f), new Vector2(.402f,.55f), new Vector2(.868f,.545f), new Vector2(.747f,.805f) };
        for (int i = 0; i < game.npcs.Length; i++)
        {
            var npc = game.npcs[i]; npc.transform.position = At(locations[i].x, locations[i].y);
            npc.transform.localScale = Vector3.one * 2.1f;
            foreach (var label in npc.GetComponentsInChildren<TextMeshPro>())
            { label.fontSize = 1.1f; label.transform.localPosition = new Vector3(0,.70f,0); }
        }
        var paths = new System.Collections.Generic.List<Rect> {
            Area(.16f,.515f,.77f,.065f), Area(.477f,.015f,.046f,.81f),
            Area(.241f,.34f,.063f,.18f), Area(.16f,.459f,.176f,.063f),
            Area(.30f,.49f,.08f,.065f), Area(.706f,.392f,.058f,.155f),
            Area(.69f,.78f,.14f,.065f), Area(.735f,.745f,.045f,.10f)
        };
        for (int i = 0; i <= 10; i++) {
            float t = i / 10f;
            paths.Add(Area(Mathf.Lerp(.653f,.75f,t), Mathf.Lerp(.545f,.79f,t), .049f, .062f));
        }
        game.walkableAreas = paths.ToArray();
        game.gameCamera.orthographicSize = game.cameraViewSize;
        game.gameCamera.transform.position = new Vector3(0,0,-10);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        PlayerSettings.WebGL.template = "PROJECT:Campus";
        PlayerSettings.bundleVersion = "1.2.0";
        BuildWebGl();
    }

    [MenuItem("CSMJU/Apply Map and Font Update")]
    public static void ApplyVisualUpdate()
    {
        AssetDatabase.Refresh();
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        foreach (var root in scene.GetRootGameObjects())
            foreach (var label in root.GetComponentsInChildren<TextMeshPro>(true))
            {
                label.fontSize = root.name.StartsWith("World") ? 2.2f : 1.5f;
            }
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        BuildWebGl();
    }

    [MenuItem("CSMJU/Build WebGL to Build-WebGL")]
    public static void BuildWebGl()
    {
        if (!File.Exists(ScenePath)) BuildScene();
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = true;
        // Old tabs must never combine cached engine code with a newer data file.
        PlayerSettings.bundleVersion = "1.8.0";
        PlayerSettings.WebGL.nameFilesAsHashes = true;
        PlayerSettings.WebGL.debugSymbolMode = WebGLDebugSymbolMode.Embedded;
        PlayerSettings.companyName = "CSMJU2030"; PlayerSettings.productName = "CSMJU Quest Digital Campus Adventure";
        const string staging = "Build/WebGL-Staging";
        if (Directory.Exists(staging)) Directory.Delete(staging, true);
        var options = new BuildPlayerOptions { scenes = new[] { ScenePath }, locationPathName = staging, target = BuildTarget.WebGL, options = BuildOptions.None };
        var report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded) throw new Exception("WebGL build failed: " + report.summary.result);
        // Publish assets first, then atomically replace the entry point. Keep older
        // hashed assets so tabs already loading the previous build can finish.
        const string live = "Build/WebGL";
        Directory.CreateDirectory(live);
        foreach (var file in Directory.GetFiles(staging, "*", SearchOption.AllDirectories))
        {
            if (file == Path.Combine(staging, "index.html")) continue;
            var target = Path.Combine(live, file.Substring(staging.Length + 1));
            Directory.CreateDirectory(Path.GetDirectoryName(target));
            File.Copy(file, target, true);
        }
        var entry = Path.Combine(live, "index.html");
        File.Copy(Path.Combine(staging, "index.html"), entry + ".next", true);
        if (File.Exists(entry)) File.Replace(entry + ".next", entry, null);
        else File.Move(entry + ".next", entry);
        Debug.Log($"WebGL build ready at {Path.GetFullPath(live)} ({report.summary.totalSize / 1024 / 1024f:F1} MiB)");
    }

    public static void EnsureSpriteImporters()
    {
        foreach (var file in ZoneIds.Select(id => $"Assets/Resources/Art/{id}.png").Append(CharacterPath))
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(file);
            if (importer == null) continue;
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = file == CharacterPath ? 64 : 32;
            importer.filterMode = FilterMode.Point; importer.mipmapEnabled = false; importer.textureCompression = TextureImporterCompression.Compressed;
            if (file == CharacterPath)
            {
                importer.spriteImportMode = SpriteImportMode.Multiple;
                var metas = new SpriteMetaData[24];
                for (var d = 0; d < 4; d++) for (var f = 0; f < 6; f++) metas[d * 6 + f] = new SpriteMetaData { name = $"walk_{d}_{f}", rect = new Rect(f * 64, 256 - (d + 1) * 64, 64, 64), alignment = (int)SpriteAlignment.Custom, pivot = new Vector2(.5f, .31f) };
                importer.spritesheet = metas;
            }
            else importer.spriteImportMode = SpriteImportMode.Single;
            EditorUtility.SetDirty(importer); importer.SaveAndReimport();
        }
    }

    private static void AddLabel(Transform parent, Vector3 offset, string text, Font font, float size, Color color)
    {
        var label = new GameObject("Label · " + text); label.transform.SetParent(parent, false); label.transform.localPosition = offset;
        var mesh = label.AddComponent<TextMeshPro>(); mesh.text = text; mesh.alignment = TextAlignmentOptions.Center; mesh.fontSize = size*10f; mesh.color = color;
        if (font != null) mesh.font = TMP_FontAsset.CreateFontAsset(font);
        label.GetComponent<MeshRenderer>().sortingOrder = 20;
    }

}
