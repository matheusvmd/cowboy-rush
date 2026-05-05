using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class BackgroundSetupEditor
{
    private const string PastaBackground = "Assets/backgrounds/desert_cartoon/bg_01";

    private static readonly string[] NomesCamadas =
    {
        "Sky",
        "BG_Decor",
        "Middle_Decor",
        "Ground_02",
        "Ground_01",
        "Foreground"
    };

    [MenuItem("CowboyRush/Backgrounds/Aplicar Desert Cartoon na Main Camera")]
    public static void AplicarDesertCartoonNaMainCamera()
    {
        Camera camera = Camera.main;
        if (camera == null)
            camera = Object.FindObjectOfType<Camera>();

        if (camera == null)
        {
            Debug.LogWarning("[CowboyRush] Nenhuma camera encontrada na cena.");
            return;
        }

        CameraBackgroundController controller = camera.GetComponent<CameraBackgroundController>();
        if (controller == null)
            controller = Undo.AddComponent<CameraBackgroundController>(camera.gameObject);

        SerializedObject serializedController = new SerializedObject(controller);
        SerializedProperty camadas = serializedController.FindProperty("camadas");
        camadas.arraySize = NomesCamadas.Length;

        for (int i = 0; i < NomesCamadas.Length; i++)
        {
            Sprite sprite = CarregarSprite(NomesCamadas[i]);
            camadas.GetArrayElementAtIndex(i).objectReferenceValue = sprite;
        }

        serializedController.FindProperty("modoAjuste").enumValueIndex = 0;
        serializedController.FindProperty("multiplicadorEscala").floatValue = 1.02f;
        serializedController.FindProperty("deslocamento").vector2Value = Vector2.zero;
        serializedController.FindProperty("parallax").floatValue = 0f;
        serializedController.FindProperty("distanciaDaCamera").floatValue = 30f;
        serializedController.FindProperty("ordemInicial").intValue = -20;
        serializedController.FindProperty("incrementoOrdem").intValue = 1;
        serializedController.FindProperty("ajusteVerticalSky").floatValue = 0.35f;
        serializedController.FindProperty("escalaVerticalSky").floatValue = 1.08f;
        serializedController.ApplyModifiedProperties();

        controller.RecriarCamadas();
        EditorUtility.SetDirty(controller);
        EditorSceneManager.MarkSceneDirty(camera.gameObject.scene);
        Selection.activeGameObject = camera.gameObject;
        EditorGUIUtility.PingObject(camera.gameObject);

        Debug.Log("[CowboyRush] Background Desert Cartoon aplicado na Main Camera.");
    }

    private static Sprite CarregarSprite(string nome)
    {
        string caminho = PastaBackground + "/" + nome + ".png";
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(caminho);
        foreach (Object asset in assets)
        {
            Sprite sprite = asset as Sprite;
            if (sprite != null)
                return sprite;
        }

        Debug.LogWarning("[CowboyRush] Sprite nao encontrado: " + caminho);
        return null;
    }
}
