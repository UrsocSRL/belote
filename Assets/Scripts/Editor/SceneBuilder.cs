using UnityEngine;
using UnityEditor;
using BeloteFreeze.UI;

/// <summary>
/// Script Editor : génère la GameScene Belote Freeze en un clic.
/// Menu : BeloteFreeze > Build GameScene  (Ctrl+Shift+B)
/// Version IMGUI — aucun package requis.
/// </summary>
public static class SceneBuilder
{
    [MenuItem("BeloteFreeze/Build GameScene %#b")]
    public static void BuildScene()
    {
        // Caméra fond vert table
        GameObject camGo = GameObject.FindWithTag("MainCamera");
        if (camGo == null) { camGo = new GameObject("Main Camera"); camGo.tag = "MainCamera"; }
        var cam = camGo.GetComponent<Camera>() ?? camGo.AddComponent<Camera>();
        cam.clearFlags      = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.08f, 0.36f, 0.16f);
        cam.orthographic    = true;

        // GameManager
        var gmGo = new GameObject("GameManager");
        var gm   = gmGo.AddComponent<GameManager>();

        // UIManager (IMGUI — pas de Canvas nécessaire)
        var uiGo = new GameObject("UIManager");
        var ui   = uiGo.AddComponent<UIManager>();
        gm.UIManager = ui;

        // EventSystem (pas strictement nécessaire en IMGUI, mais bonne pratique)
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // Sauvegarder la scène
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene(),
            "Assets/Scenes/GameScene.unity");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[BeloteFreeze] GameScene construite avec succes ! Clique Play pour jouer.");
    }
}
