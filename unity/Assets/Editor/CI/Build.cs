using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace CI
{
    public static class Build
    {
        public static void PerformAndroid()
        {
            string projectPath = Directory.GetCurrentDirectory().Replace("\\", "/");
            string outputDir = Path.Combine(projectPath, "build/Android");
            Directory.CreateDirectory(outputDir);

            string outputFile = Path.Combine(outputDir, "QuestNav-local.apk");

            // Ensure Android is the active target so package asmdefs pick correct platform
            EditorUserBuildSettings.SwitchActiveBuildTarget(
                BuildTargetGroup.Android,
                BuildTarget.Android
            );

            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;

            try
            {
                var group = BuildTargetGroup.Android;
                var defines =
                    PlayerSettings.GetScriptingDefineSymbolsForGroup(group) ?? string.Empty;
                string[] needed = new[]
                {
                    "OVRPLUGIN_ANDROID",
                    "USING_XR_SDK_OPENXR",
                    "OCULUS_OPENXR",
                    "META_XR",
                };
                foreach (var d in needed)
                {
                    if (
                        !defines
                            .Split(';')
                            .Any(x => string.Equals(x.Trim(), d, StringComparison.Ordinal))
                    )
                    {
                        defines = string.IsNullOrEmpty(defines) ? d : defines + ";" + d;
                    }
                }
                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, defines);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Define setup skipped: {e.Message}");
            }

            bool development = !string.IsNullOrEmpty(
                Environment.GetEnvironmentVariable("QUESTNAV_DEVELOPMENT_BUILD")
            );

            string version = Environment.GetEnvironmentVariable("QUESTNAV_VERSION");
            string versionCodeStr = Environment.GetEnvironmentVariable("QUESTNAV_VERSION_CODE");
            if (!string.IsNullOrEmpty(version))
            {
                PlayerSettings.bundleVersion = version;
            }
            if (int.TryParse(versionCodeStr, out int versionCode))
            {
                PlayerSettings.Android.bundleVersionCode = versionCode;
            }

            string[] scenes = EditorBuildSettings
                .scenes.Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();
            if (scenes.Length == 0)
            {
                throw new Exception("No enabled scenes in Build Settings.");
            }

            var buildOptions = BuildOptions.None;
            if (development)
            {
                buildOptions |= BuildOptions.Development;
            }

            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputFile,
                target = BuildTarget.Android,
                options = buildOptions,
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new Exception(
                    $"Android build failed: {report.summary.result} (errors: {report.summary.totalErrors})"
                );
            }

            if (!File.Exists(outputFile))
            {
                throw new Exception($"Expected APK not found at {outputFile}");
            }

            Debug.Log($"SUCCESS: APK built at {outputFile}");
        }
    }
}
