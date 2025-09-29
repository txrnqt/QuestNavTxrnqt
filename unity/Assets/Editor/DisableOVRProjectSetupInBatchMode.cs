// Disables Meta XR's OVR Project Setup background checks during batch/CI builds
#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class DisableOVRProjectSetupInBatchMode
{
    static DisableOVRProjectSetupInBatchMode()
    {
        if (!Application.isBatchMode && !IsCiEnvironment())
        {
            return;
        }

        // Disable background checks by swapping to temporary registry defaults
        TryInvokeStatic("OVRProjectSetupUpdater", "SetupTemporaryRegistry");
        // Also align OVRProjectSetup registry to temporary to avoid throwing errors
        TryInvokeStatic("OVRProjectSetup", "SetupTemporaryRegistry");
    }

    private static bool IsCiEnvironment()
    {
        var ci = Environment.GetEnvironmentVariable("CI");
        var gha = Environment.GetEnvironmentVariable("GITHUB_ACTIONS");
        return string.Equals(ci, "true", StringComparison.OrdinalIgnoreCase)
            || !string.IsNullOrEmpty(gha);
    }

    private static void TryInvokeStatic(string typeName, string methodName)
    {
        try
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = asm.GetType(typeName, false);
                if (t == null)
                    continue;
                var m = t.GetMethod(
                    methodName,
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic
                );
                if (m == null)
                    continue;
                m.Invoke(null, null);
                return;
            }
        }
        catch
        {
            // Swallow any reflection failures; best-effort disable only
        }
    }
}
#endif
