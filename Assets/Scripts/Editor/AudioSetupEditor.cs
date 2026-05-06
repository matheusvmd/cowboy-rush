using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class AudioSetupEditor
{
    private const string Level1Path = "Assets/Scenes/Level1.unity";
    private const string TiroPath = "Assets/tiro.mp3";
    private const string RecargaPath = "Assets/reload.mp3";
    private const string DanoPath = "Assets/damage.mp3";
    private const string VitoriaPath = "Assets/victory.mp3";

    [MenuItem("CowboyRush/Audio/Configurar SFX Principais")]
    public static void ConfigurarTiroERecarga()
    {
        Scene cena = SceneManager.GetActiveScene();
        if (cena.path != Level1Path)
            cena = EditorSceneManager.OpenScene(Level1Path, OpenSceneMode.Single);

        AudioManager audioManager = UnityEngine.Object.FindAnyObjectByType<AudioManager>();
        if (audioManager == null)
        {
            GameObject audioObject = new GameObject("AudioManager");
            audioManager = audioObject.AddComponent<AudioManager>();
        }

        SerializedObject serializedAudio = new SerializedObject(audioManager);
        serializedAudio.FindProperty("sfxTiro").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(TiroPath);
        serializedAudio.FindProperty("sfxRecarga").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(RecargaPath);
        serializedAudio.FindProperty("sfxDanoPlayer").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(DanoPath);
        serializedAudio.FindProperty("sfxPortal").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(VitoriaPath);
        serializedAudio.FindProperty("duracaoMaximaTiro").floatValue = 1f;
        serializedAudio.ApplyModifiedProperties();

        EditorUtility.SetDirty(audioManager);
        EditorSceneManager.MarkSceneDirty(cena);
        EditorSceneManager.SaveScene(cena);

        Selection.activeGameObject = audioManager.gameObject;
        Debug.Log("[CowboyRush] AudioManager configurado com tiro, recarga, dano e vitoria.");
    }
}
