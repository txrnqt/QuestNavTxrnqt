using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class DisableOVRProjectSetupInBatchMode
{
    static DisableOVRProjectSetupInBatchMode()
    {
        if (Application.isBatchMode)
        {
            Debug.Log("Batch mode detected - disabling OVRProjectSetupUpdater");
            DisableOVRProjectSetupUpdater();
        }
    }

    private static void DisableOVRProjectSetupUpdater()
    {
        try
        {
            // Root cause: OVRProjectSetupUpdater.OnEditorSceneManagerSceneOpened is triggered during build
            // which calls OVRProjectConfig static constructor, which fails because OVRPlugin isn't initialized.
            //
            // Solution: Unregister the OVRProjectSetupUpdater from scene events during batch mode

            var updaterType = System.Type.GetType(
                "OVRProjectSetupUpdater, com.meta.xr.sdk.core.Editor"
            );
            if (updaterType != null)
            {
                // Find the OnEditorSceneManagerSceneOpened method
                var sceneOpenedMethod = updaterType.GetMethod(
                    "OnEditorSceneManagerSceneOpened",
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public
                );

                if (sceneOpenedMethod != null)
                {
                    // Create a delegate for the method using the correct delegate type
                    var methodDelegate = (EditorSceneManager.SceneOpenedCallback)System.Delegate.CreateDelegate(
                        typeof(EditorSceneManager.SceneOpenedCallback),
                        sceneOpenedMethod
                    );

                    // Unregister it from the sceneOpened event
                    EditorSceneManager.sceneOpened -= methodDelegate;

                    Debug.Log(
                        "Successfully disabled OVRProjectSetupUpdater.OnEditorSceneManagerSceneOpened for batch mode"
                    );
                }
                else
                {
                    Debug.LogWarning(
                        "Could not find OVRProjectSetupUpdater.OnEditorSceneManagerSceneOpened method"
                    );
                }
            }
            else
            {
                Debug.LogWarning("Could not find OVRProjectSetupUpdater type");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to disable OVRProjectSetupUpdater: {e.Message}");
        }
    }
}
