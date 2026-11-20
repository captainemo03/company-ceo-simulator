using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace CompanyCEO.UnityRemake.Editor
{
    public static class WindowsBuild
    {
        [MenuItem("Company CEO Simulator/Build Windows x64")]
        public static void Build()
        {
            // The installed Unity editor has Windows Mono support but no IL2CPP module.
            // Select the available backend explicitly so a clean machine can build x64.
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);
            var result = BuildPipeline.BuildPlayer(new[] { "Assets/Scenes/OfficePrototype.unity" }, "Build/CompanyCEOSimulator.exe", BuildTarget.StandaloneWindows64, BuildOptions.None);
            Debug.Log($"Windows build result: {result.summary.result}, errors: {result.summary.totalErrors}");
        }
    }
}
