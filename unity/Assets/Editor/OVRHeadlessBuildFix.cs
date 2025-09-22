using UnityEditor;
using UnityEngine;
using System.IO;

[InitializeOnLoad]
public class OVRHeadlessBuildFix
{
    static OVRHeadlessBuildFix()
    {
        // Only run in headless/CI environments
        if (IsHeadlessBuild())
        {
            Debug.Log("Headless build detected, applying OVR configuration fixes...");
            EnsureOVRConfigurationExists();
        }
    }
    
    private static bool IsHeadlessBuild()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        return System.Array.Exists(args, arg => arg == "-batchmode") ||
               System.Environment.GetEnvironmentVariable("CI") == "true" ||
               System.Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true";
    }
    
    private static void EnsureOVRConfigurationExists()
    {
        try
        {
            // Ensure OVRBuildConfig exists in Resources
            EnsureOVRBuildConfig();
            
            // Ensure OculusProjectConfig exists
            EnsureOculusProjectConfig();
            
            Debug.Log("OVR configuration files verified for headless build");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"OVR configuration setup warning: {e.Message}");
        }
    }
    
    private static void EnsureOVRBuildConfig()
    {
        var ovrBuildConfig = Resources.Load("OVRBuildConfig");
        if (ovrBuildConfig == null)
        {
            Debug.LogWarning("OVRBuildConfig not found in Resources folder");
            
            // Create a minimal OVRBuildConfig if it doesn't exist
            string resourcesPath = "Assets/Resources";
            if (!Directory.Exists(resourcesPath))
            {
                Directory.CreateDirectory(resourcesPath);
            }
            
            // Create a minimal ScriptableObject for OVRBuildConfig
            var buildConfig = ScriptableObject.CreateInstance<ScriptableObject>();
            AssetDatabase.CreateAsset(buildConfig, "Assets/Resources/OVRBuildConfig.asset");
            AssetDatabase.SaveAssets();
            Debug.Log("Created minimal OVRBuildConfig for headless build");
        }
        else
        {
            Debug.Log("OVRBuildConfig found and loaded successfully");
        }
    }
    
    private static void EnsureOculusProjectConfig()
    {
        string configPath = "Assets/Oculus/OculusProjectConfig.asset";
        var oculusProjectConfig = AssetDatabase.LoadAssetAtPath<ScriptableObject>(configPath);
        
        if (oculusProjectConfig == null)
        {
            Debug.LogWarning($"OculusProjectConfig not found at {configPath}");
        }
        else
        {
            Debug.Log("OculusProjectConfig found and loaded successfully");
            
            // Force the asset to be loaded and initialized
            EditorUtility.SetDirty(oculusProjectConfig);
        }
    }
}
