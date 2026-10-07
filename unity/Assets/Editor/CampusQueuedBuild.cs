using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class CampusQueuedBuild
{
    private const string Request = "/tmp/csmju-interior-build.request";
    private const string Result = "/tmp/csmju-interior-build.result";
    static CampusQueuedBuild() { EditorApplication.update += Tick; }
    private static void Tick()
    {
        if(Application.isBatchMode || !File.Exists(Request) || EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        File.Delete(Request);
        File.WriteAllText(Result,"BUILDING");
        try
        {
            // Preserve any unsaved scene before the builder opens the game's saved scene.
            for(int i=0;i<SceneManager.sceneCount;i++)
            {
                var scene=SceneManager.GetSceneAt(i);
                if(scene.isDirty)
                {
                    Directory.CreateDirectory("Assets/SceneBackups");
                    string path="Assets/SceneBackups/BeforeInterior_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+"_"+i+".unity";
                    if(!EditorSceneManager.SaveScene(scene,path,true))throw new Exception("Could not preserve unsaved scene: "+scene.name);
                }
            }
            CampusExpansionBuild.Build();
            File.WriteAllText(Result,"SUCCESS");
        }
        catch(Exception e) {File.WriteAllText(Result,"FAILED: "+e);Debug.LogException(e);}
    }
}
