using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CopyStartMenuToScenes
{
    private const string SourceScenePath = "Assets/Scenes/SampleScene.unity";

    private static readonly string[] TargetScenePaths =
    {
        "Assets/Scenes/Level2.unity",
        "Assets/Scenes/Level3.unity"
    };

    [MenuItem("Tools/Copy StartMenu To All Scenes")]
    public static void CopyToAllScenes()
    {
        // Open source scene
        Scene sourceScene = EditorSceneManager.OpenScene(SourceScenePath, OpenSceneMode.Single);

        StartMenuController controller = Object.FindObjectOfType<StartMenuController>();
        if (controller == null)
        {
            Debug.LogError("StartMenuController not found in " + SourceScenePath);
            return;
        }

        GameObject sourceObj = controller.gameObject;

        foreach (string targetPath in TargetScenePaths)
        {
            Scene targetScene = EditorSceneManager.OpenScene(targetPath, OpenSceneMode.Additive);

            // Skip if target already has a StartMenuController
            if (Object.FindObjectOfType<StartMenuController>() != controller)
            {
                Debug.LogWarning("Skipping " + targetPath + " — already has a StartMenuController.");
                EditorSceneManager.CloseScene(targetScene, false);
                continue;
            }

            // Instantiate a copy and move it into the target scene
            GameObject copy = Object.Instantiate(sourceObj);
            copy.name = sourceObj.name;
            SceneManager.MoveGameObjectToScene(copy, targetScene);

            // Add CameraShake to Main Camera if missing
            foreach (GameObject rootObj in targetScene.GetRootGameObjects())
            {
                Camera cam = rootObj.GetComponentInChildren<Camera>(true);
                if (cam != null && cam.CompareTag("MainCamera"))
                {
                    if (cam.GetComponent<CameraShake>() == null)
                    {
                        cam.gameObject.AddComponent<CameraShake>();
                    }
                    break;
                }
            }

            EditorSceneManager.SaveScene(targetScene);
            EditorSceneManager.CloseScene(targetScene, false);
            Debug.Log("Copied StartMenuController + CameraShake to " + targetPath);
        }

        // Reopen source scene
        EditorSceneManager.OpenScene(SourceScenePath, OpenSceneMode.Single);
        Debug.Log("Done! All scenes updated.");
    }
}
