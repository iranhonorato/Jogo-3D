using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Guaxinim.EditorTools
{
    /// <summary>
    /// Gera os builds de Windows e WebGL em Builds/&lt;plataforma&gt;/ com as cenas ativas do Build Profile.
    /// Pelo menu Tools/Guaxinim/Build ou pela linha de comando:
    /// Unity.exe -batchmode -quit -projectPath . -executeMethod Guaxinim.EditorTools.BuildScript.BuildWindows
    /// </summary>
    public static class BuildScript
    {
        private const string BuildsFolder = "Builds";
        private const string WindowsFolder = "Windows";
        private const string WebGLFolder = "WebGL";
        private const int FailureExitCode = 1;

        [MenuItem("Tools/Guaxinim/Build/Windows")]
        public static void BuildWindows()
        {
            string executable = PlayerSettings.productName + ".exe";
            Build(BuildTarget.StandaloneWindows64, Path.Combine(BuildsFolder, WindowsFolder, executable));
        }

        [MenuItem("Tools/Guaxinim/Build/WebGL")]
        public static void BuildWebGL() => Build(BuildTarget.WebGL, Path.Combine(BuildsFolder, WebGLFolder));

        private static void Build(BuildTarget target, string locationPath)
        {
            var options = new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray(),
                locationPathName = locationPath,
                target = target,
                options = BuildOptions.None,
            };

            if (options.scenes.Length == 0)
            {
                Fail($"No enabled scenes in the build settings for {target}.");
                return;
            }

            BuildSummary summary = BuildPipeline.BuildPlayer(options).summary;
            if (summary.result != BuildResult.Succeeded)
            {
                Fail($"{target} build {summary.result}: {summary.totalErrors} error(s).");
                return;
            }

            Debug.Log($"{target} build succeeded: {summary.outputPath} ({summary.totalSize / (1024 * 1024)} MB)");
        }

        private static void Fail(string message)
        {
            Debug.LogError(message);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(FailureExitCode); // o -executeMethod precisa de código de saída para scripts
            }
        }
    }
}
