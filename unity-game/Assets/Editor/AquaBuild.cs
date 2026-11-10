using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class AquaBuild
{
    [MenuItem("Jalrakshak/Build WebGL")]
    public static void BuildFromMenu()
    {
        string output = EditorUtility.SaveFolderPanel("Choose WebGL output folder", "", "JalrakshakWebGL");
        if (!string.IsNullOrEmpty(output)) Build(output);
    }
    // Run with -executeMethod AquaBuild.BuildWebGL -aquaguardianOutput /absolute/path
    public static void BuildWebGL()
    {
        string[] args = Environment.GetCommandLineArgs();
        int index = Array.IndexOf(args, "-aquaguardianOutput");
        if (index < 0 || index + 1 >= args.Length) throw new ArgumentException("Specify -aquaguardianOutput followed by an output directory.");
        Build(Path.GetFullPath(args[index + 1]));
    }
    static void Build(string output)
    {
        var options = new BuildPlayerOptions { scenes = new[] { "Assets/Scenes/Village.unity" }, locationPathName = output, target = BuildTarget.WebGL };
        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded) throw new Exception("WebGL build failed: " + report.summary.result);
        Debug.Log("Jalrakshak WebGL build succeeded: " + output);
    }
}
