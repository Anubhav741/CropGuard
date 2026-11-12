using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class CropGuardSetup {
    static CropGuardSetup() { EditorApplication.delayCall += EnsureScenes; }
    static void EnsureScenes() {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) { EditorApplication.delayCall += EnsureScenes; return; }
        if (System.IO.File.Exists("Assets/Scenes/MainMenu.unity") && System.IO.File.Exists("Assets/Scenes/GameLevel1.unity") && System.IO.File.Exists("Assets/Scenes/GameLevel2.unity") && System.IO.File.Exists("Assets/Scenes/GameLevel3.unity")) return;
        Build();
    }
    [MenuItem("CropGuard/Rebuild Scenes")]
    public static void Build() {
        System.IO.Directory.CreateDirectory("Assets/Scenes");
        foreach(string name in new[]{"MainMenu","GameLevel1","GameLevel2","GameLevel3"}) {
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var root=new GameObject("CropGuard Game"); root.AddComponent<CropGuardGame>();
            EditorSceneManager.SaveScene(scene,"Assets/Scenes/"+name+".unity");
        }
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity",true),new EditorBuildSettingsScene("Assets/Scenes/GameLevel1.unity",true),new EditorBuildSettingsScene("Assets/Scenes/GameLevel2.unity",true),new EditorBuildSettingsScene("Assets/Scenes/GameLevel3.unity",true)};
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity"); AssetDatabase.SaveAssets();
    }
}
