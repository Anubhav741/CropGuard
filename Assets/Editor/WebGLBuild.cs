using UnityEditor;
using UnityEditor.Build.Reporting;
using System;
using System.IO;
public static class WebGLBuild {
    public static void Build(){
        string path=Path.GetFullPath("frontend/public/game"); Directory.CreateDirectory(path);
        PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.nameFilesAsHashes=false;
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/MainMenu.unity","Assets/Scenes/GameLevel1.unity","Assets/Scenes/GameLevel2.unity","Assets/Scenes/GameLevel3.unity"},locationPathName=path,target=BuildTarget.WebGL,options=BuildOptions.None});
        if(report.summary.result!=BuildResult.Succeeded)throw new Exception("WebGL build failed: "+report.summary.result);
    }
}
