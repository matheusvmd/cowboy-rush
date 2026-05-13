using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public static class Level2Setup
{
    private const string Level2Path = "Assets/Scenes/Level2.unity";
    private const string NightBgPath = "Assets/backgrounds/desert_cartoon/bg_04/";

    [MenuItem("CowboyRush/Configurar Level 2 (Noite)")]
    public static void ConfigurarLevel2()
    {
        // 1. Abrir a cena Level2
        Scene cena = EditorSceneManager.OpenScene(Level2Path, OpenSceneMode.Single);

        // 2. Configurar Background de Noite
        ConfigurarBackgroundNoite();

        // 3. Reconfigurar Terreno e Plataformas (Level Design Único)
        ConfigurarLayoutLevelDesign();

        // 4. Adicionar/Reposicionar Inimigos (4 inimigos para ser desafiador)
        ConfigurarInimigos();

        // 5. Mover Portal (Fim de Fase)
        ConfigurarPortal();

        // 6. Configurar Cavalo (Rescale e Tag)
        GarantirTags();
        HorseSetup.ConfigurarCavalo();

        // 7. Adicionar ao Build Settings
        AdicionarAoBuildSettings();

        // 8. Salvar
        EditorSceneManager.MarkSceneDirty(cena);
        EditorSceneManager.SaveScene(cena);

        Debug.Log("[CowboyRush] Level 2 (Noite) configurado com 4 inimigos, novo layout e cavalo!");
    }

    private static void GarantirTags()
    {
        // Acessa o TagManager.asset via SerializedObject para adicionar tags via editor
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
        if (assets == null || assets.Length == 0) return;

        SerializedObject tagManager = new SerializedObject(assets[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        bool existe = false;
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            if (tagsProp.GetArrayElementAtIndex(i).stringValue == "Cavalo")
            {
                existe = true;
                break;
            }
        }

        if (!existe)
        {
            tagsProp.InsertArrayElementAtIndex(0);
            tagsProp.GetArrayElementAtIndex(0).stringValue = "Cavalo";
            tagManager.ApplyModifiedProperties();
            Debug.Log("[CowboyRush] Tag 'Cavalo' criada com sucesso.");
        }
    }

    private static void ConfigurarLayoutLevelDesign()
    {
        // 1. Limpar arte antiga
        GameObject oldArt = GameObject.Find("LevelArt_Ground");
        if (oldArt != null) Object.DestroyImmediate(oldArt);

        // 2. Configurar Chãos Principais
        GameObject chao1 = GameObject.Find("Chao_Principal");
        if (chao1 != null)
        {
            chao1.transform.position = new Vector3(10f, -4f, 0f);
            BoxCollider2D col = chao1.GetComponent<BoxCollider2D>();
            if (col != null) col.size = new Vector2(30f, 2f);
        }

        GameObject chao2 = GameObject.Find("Chao_Principal (1)");
        if (chao2 == null) chao2 = GameObject.Find("Chao_Principal_1");

        if (chao2 != null)
        {
            chao2.transform.position = new Vector3(85f, -4f, 0f);
            BoxCollider2D col = chao2.GetComponent<BoxCollider2D>();
            if (col != null) col.size = new Vector2(60f, 2f);
        }

        // 3. Limpar plataformas extras
        string[] extras = { "Plataforma_Pulo_1", "Plataforma_Pulo_2", "Plataforma_Pulo_3", "Plataforma_Final_1", "Plataforma_Final_2" };
        foreach (var nome in extras)
        {
            GameObject ex = GameObject.Find(nome);
            if (ex != null) Object.DestroyImmediate(ex);
            // Tenta achar com prefixo
            GameObject ex2 = GameObject.Find("Plataforma_" + nome);
            if (ex2 != null) Object.DestroyImmediate(ex2);
        }

        // 4. Criar o novo parkour
        CriarNovaPlataforma("Plataforma_Pulo_1", new Vector3(38f, -1.5f, 0f), new Vector2(8f, 0.5f));
        CriarNovaPlataforma("Plataforma_Pulo_2", new Vector3(52f, 1.0f, 0f), new Vector2(8f, 0.5f));
        CriarNovaPlataforma("Plataforma_Pulo_3", new Vector3(66f, -1.0f, 0f), new Vector2(8f, 0.5f));

        CriarNovaPlataforma("Plataforma_Final_1", new Vector3(95f, 1.2f, 0f), new Vector2(10f, 0.5f));
        CriarNovaPlataforma("Plataforma_Final_2", new Vector3(112f, 2.5f, 0f), new Vector2(8f, 0.5f));

        // 5. Aplicar arte
        LevelArtBuilder.AplicarChaoEPlataformas();
    }

    private static void CriarNovaPlataforma(string nome, Vector3 posicao, Vector2 tamanho)
    {
        string nomeFinal = nome.StartsWith("Plataforma") ? nome : "Plataforma_" + nome;
        GameObject go = GameObject.Find(nomeFinal);
        if (go == null) go = new GameObject(nomeFinal);
        
        go.transform.position = posicao;
        BoxCollider2D col = go.GetComponent<BoxCollider2D>();
        if (col == null) col = go.AddComponent<BoxCollider2D>();
        col.size = tamanho;

        if (go.GetComponent<SpriteRenderer>() == null) go.AddComponent<SpriteRenderer>();
    }

    private static void ConfigurarBackgroundNoite()
    {
        Camera cam = Camera.main;
        if (cam == null) cam = Object.FindAnyObjectByType<Camera>();
        if (cam == null) return;

        CameraBackgroundController bgCtrl = cam.GetComponent<CameraBackgroundController>();
        if (bgCtrl == null) return;

        string[] nomesCamadas = { "Sky", "BG_Decor", "Middle_Decor", "Ground_02", "Ground_01", "Foreground" };
        Sprite[] spritesNoite = new Sprite[nomesCamadas.Length];

        for (int i = 0; i < nomesCamadas.Length; i++)
        {
            string path = NightBgPath + nomesCamadas[i] + ".png";
            spritesNoite[i] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        SerializedObject so = new SerializedObject(bgCtrl);
        SerializedProperty propCamadas = so.FindProperty("camadas");
        propCamadas.arraySize = spritesNoite.Length;
        for (int i = 0; i < spritesNoite.Length; i++)
        {
            propCamadas.GetArrayElementAtIndex(i).objectReferenceValue = spritesNoite[i];
        }
        so.ApplyModifiedProperties();

        bgCtrl.RecriarCamadas();
        EditorUtility.SetDirty(bgCtrl);
    }

    private static void ConfigurarInimigos()
    {
        EnemyController[] antigos = Object.FindObjectsByType<EnemyController>(FindObjectsInactive.Include);
        foreach (var a in antigos) Object.DestroyImmediate(a.gameObject);

        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab Hyena");
        GameObject prefab = null;
        if (prefabGuids.Length > 0)
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(prefabGuids[0]));

        if (prefab == null) return;

        InstanciarInimigo(prefab, new Vector3(25f, -3f, 0f), "Inimigo_Chao_1");
        InstanciarInimigo(prefab, new Vector3(52f, 2.0f, 0f), "Inimigo_Plataforma_1");
        InstanciarInimigo(prefab, new Vector3(75f, -3f, 0f), "Inimigo_Chao_2");
        InstanciarInimigo(prefab, new Vector3(95f, -3f, 0f), "Inimigo_Final");
    }

    private static void InstanciarInimigo(GameObject prefab, Vector3 posicao, string nome)
    {
        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        go.name = nome;
        go.transform.position = posicao;
        
        Transform limEsq = go.transform.Find("LimiteEsquerdo");
        Transform limDir = go.transform.Find("LimiteDireito");
        if (limEsq) limEsq.localPosition = new Vector3(-3f, 0f, 0f);
        if (limDir) limDir.localPosition = new Vector3(3f, 0f, 0f);

        EnemyController ctrl = go.GetComponent<EnemyController>();
        if (ctrl != null)
        {
            SerializedObject so = new SerializedObject(ctrl);
            if (limEsq) so.FindProperty("limiteEsquerdo").objectReferenceValue = limEsq;
            if (limDir) so.FindProperty("limiteDireito").objectReferenceValue = limDir;
            so.ApplyModifiedProperties();
        }
    }

    private static void ConfigurarPortal()
    {
        GameObject portal = GameObject.Find("FimDeFase");
        if (portal != null)
        {
            portal.transform.position = new Vector3(120f, -2.5f, 0f);
        }
        ConfigurarCameraLimites(120f);
    }

    private static void ConfigurarCameraLimites(float maxX)
    {
        Camera cam = Camera.main;
        if (cam == null) cam = Object.FindAnyObjectByType<Camera>();
        if (cam == null) return;

        CameraFollow follow = cam.GetComponent<CameraFollow>();
        if (follow != null)
        {
            SerializedObject so = new SerializedObject(follow);
            so.FindProperty("limiteDireitoX").floatValue = maxX + 10f;
            so.FindProperty("limiteEsquerdoX").floatValue = -20f;
            so.ApplyModifiedProperties();
        }
    }

    private static void AdicionarAoBuildSettings()
    {
        string level1Path = "Assets/Scenes/Level1.unity";
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
        scenes.Add(new EditorBuildSettingsScene(level1Path, true));
        scenes.Add(new EditorBuildSettingsScene(Level2Path, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
