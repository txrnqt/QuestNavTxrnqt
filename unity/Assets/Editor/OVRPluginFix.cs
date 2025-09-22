using UnityEngine;
using UnityEditor;
using System.Reflection;

[InitializeOnLoad]
public static class OVRPluginFix
{
    static OVRPluginFix()
    {
        if (Application.isBatchMode)
        {
            Debug.Log("Batch mode detected - fixing OVRPlugin.wrapperVersion null reference");
            InitializeOVRPluginForBatchMode();
        }
    }
    
    private static void InitializeOVRPluginForBatchMode()
    {
        try
        {
            // Root cause: OVRProjectConfig.cs line 104 tries to access OVRPlugin.wrapperVersion.Minor
            // but OVRPlugin.wrapperVersion is null in batch mode because the plugin isn't initialized.
            //
            // Solution: Initialize OVRPlugin or provide a fallback for wrapperVersion
            
            var ovrPluginType = System.Type.GetType("OVRPlugin, OVRPlugin");
            if (ovrPluginType != null)
            {
                // Try to get the wrapperVersion field
                var wrapperVersionField = ovrPluginType.GetField("wrapperVersion", BindingFlags.Static | BindingFlags.Public);
                
                if (wrapperVersionField != null)
                {
                    var currentValue = wrapperVersionField.GetValue(null);
                    
                    if (currentValue == null)
                    {
                        Debug.Log("OVRPlugin.wrapperVersion is null, creating fallback version");
                        
                        // Create a fallback version object
                        // The OVRProjectConfig expects Minor - 32, so we need a version where Minor = 110 (78 + 32)
                        var versionType = wrapperVersionField.FieldType;
                        
                        // Try to create a version object with appropriate values
                        if (versionType.Name.Contains("Version"))
                        {
                            // Create a version with Major=0, Minor=110 to give currentSdkVersion = 78
                            var fallbackVersion = System.Activator.CreateInstance(versionType, 0, 110, 0, 0);
                            wrapperVersionField.SetValue(null, fallbackVersion);
                            
                            Debug.Log("Set OVRPlugin.wrapperVersion to fallback version (0.110.0.0) for batch mode");
                        }
                    }
                    else
                    {
                        Debug.Log("OVRPlugin.wrapperVersion is already initialized");
                    }
                }
                else
                {
                    Debug.LogWarning("Could not find OVRPlugin.wrapperVersion field");
                }
            }
            else
            {
                Debug.LogWarning("Could not find OVRPlugin type");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to initialize OVRPlugin for batch mode: {e.Message}");
        }
    }
}
