using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class SceneBuildHelper
{
    [MenuItem("CowboyRush/Montar UI do HUD")]
    public static void MontarUI()
    {
        var canvasGO = GameObject.Find("Canvas_HUD");
        if (canvasGO == null) { Debug.LogError("Canvas_HUD não encontrado"); return; }

        // Remove UI antiga se existir
        var painelVelha = canvasGO.transform.Find("Painel_Vidas");
        if (painelVelha) Object.DestroyImmediate(painelVelha.gameObject);
        var painelGOVelho = canvasGO.transform.Find("Painel_GameOver");
        if (painelGOVelho) Object.DestroyImmediate(painelGOVelho.gameObject);

        // Painel Vidas (top-left)
        var painelVidas = CriarRect("Painel_Vidas", canvasGO.transform);
        var rv = painelVidas.GetComponent<RectTransform>();
        rv.anchorMin = new Vector2(0, 1); rv.anchorMax = new Vector2(0, 1);
        rv.pivot = new Vector2(0, 1);
        rv.anchoredPosition = new Vector2(20, -20);
        rv.sizeDelta = new Vector2(160, 50);

        var icones = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            var icone = CriarRect("Icone_Vida_" + (i + 1), painelVidas.transform);
            var ri = icone.GetComponent<RectTransform>();
            ri.anchorMin = new Vector2(0, 0.5f); ri.anchorMax = new Vector2(0, 0.5f);
            ri.pivot = new Vector2(0, 0.5f);
            ri.anchoredPosition = new Vector2(i * 52, 0);
            ri.sizeDelta = new Vector2(44, 44);
            icones[i] = icone.AddComponent<Image>();
            icones[i].color = Color.white;
            icones[i].preserveAspect = true;
            icones[i].raycastTarget = false;
        }

        // Painel Game Over
        var painelGO = CriarRect("Painel_GameOver", canvasGO.transform);
        var rgo = painelGO.GetComponent<RectTransform>();
        rgo.anchorMin = Vector2.zero; rgo.anchorMax = Vector2.one;
        rgo.offsetMin = Vector2.zero; rgo.offsetMax = Vector2.zero;
        painelGO.AddComponent<Image>().color = new Color(0, 0, 0, 0.75f);

        var textoGO = CriarRect("Texto_GameOver", painelGO.transform);
        var rt = textoGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0, 50); rt.sizeDelta = new Vector2(400, 100);
        var txt = textoGO.AddComponent<Text>();
        txt.text = "GAME OVER"; txt.fontSize = 60;
        txt.alignment = TextAnchor.MiddleCenter; txt.color = Color.white;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var btnGO = CriarRect("Botao_Reiniciar", painelGO.transform);
        var rb2 = btnGO.GetComponent<RectTransform>();
        rb2.anchorMin = new Vector2(0.5f, 0.5f); rb2.anchorMax = new Vector2(0.5f, 0.5f);
        rb2.anchoredPosition = new Vector2(0, -60); rb2.sizeDelta = new Vector2(200, 55);
        btnGO.AddComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f);
        btnGO.AddComponent<Button>();

        var txtBtnGO = CriarRect("Texto_Btn", btnGO.transform);
        var rtb = txtBtnGO.GetComponent<RectTransform>();
        rtb.anchorMin = Vector2.zero; rtb.anchorMax = Vector2.one;
        rtb.offsetMin = Vector2.zero; rtb.offsetMax = Vector2.zero;
        var txtBtn = txtBtnGO.AddComponent<Text>();
        txtBtn.text = "REINICIAR"; txtBtn.fontSize = 24;
        txtBtn.alignment = TextAnchor.MiddleCenter; txtBtn.color = Color.white;
        txtBtn.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        painelGO.SetActive(false);

        // Serializa referências no UIManager
        var ui = canvasGO.GetComponent<UIManager>();
        if (ui != null)
        {
            var so = new SerializedObject(ui);
            var iconesArr = so.FindProperty("iconesVida");
            iconesArr.arraySize = 3;
            for (int i = 0; i < 3; i++)
                iconesArr.GetArrayElementAtIndex(i).objectReferenceValue = icones[i];
            so.FindProperty("painelGameOver").objectReferenceValue = painelGO;
            so.ApplyModifiedProperties();
        }

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[CowboyRush] UI montada com sucesso!");
    }

    [MenuItem("CowboyRush/Configurar Player Controller")]
    public static void ConfigurarPlayerController()
    {
        var player = GameObject.Find("Player");
        if (player == null) { Debug.LogError("Player não encontrado"); return; }

        var ctrl = player.GetComponent<PlayerController>();
        var pontoChao = player.transform.Find("PontoChao");
        var pontoDisparo = player.transform.Find("PontoDisparo");

        if (ctrl == null) { Debug.LogError("PlayerController não encontrado"); return; }

        var so = new SerializedObject(ctrl);
        if (pontoChao) so.FindProperty("pontoChao").objectReferenceValue = pontoChao;
        if (pontoDisparo) so.FindProperty("pontoDisparo").objectReferenceValue = pontoDisparo;

        // Layer mask Chao = layer 6
        so.FindProperty("camadaChao").intValue = 1 << 6;
        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[CowboyRush] PlayerController configurado!");
    }

    [MenuItem("CowboyRush/Configurar Enemy Controller")]
    public static void ConfigurarEnemyController()
    {
        var inimigo = GameObject.Find("Hyena_Inimigo");
        if (inimigo == null) { Debug.LogError("Hyena_Inimigo não encontrado"); return; }

        var ctrl = inimigo.GetComponent<EnemyController>();
        var limEsq = inimigo.transform.Find("LimiteEsquerdo");
        var limDir = inimigo.transform.Find("LimiteDireito");

        if (ctrl == null) { Debug.LogError("EnemyController não encontrado"); return; }

        var so = new SerializedObject(ctrl);
        if (limEsq) so.FindProperty("limiteEsquerdo").objectReferenceValue = limEsq;
        if (limDir) so.FindProperty("limiteDireito").objectReferenceValue = limDir;
        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[CowboyRush] EnemyController configurado!");
    }

    [MenuItem("CowboyRush/Configurar Camera Follow")]
    public static void ConfigurarCamera()
    {
        var cam = Object.FindAnyObjectByType<Camera>();
        if (cam == null) { Debug.LogError("Camera não encontrada"); return; }

        var follow = cam.GetComponent<CameraFollow>();
        if (follow == null) { Debug.LogError("CameraFollow não encontrado"); return; }

        var player = GameObject.Find("Player");
        if (player == null) { Debug.LogError("Player não encontrado"); return; }

        var so = new SerializedObject(follow);
        so.FindProperty("alvo").objectReferenceValue = player.transform;
        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[CowboyRush] CameraFollow configurado!");
    }

    [MenuItem("CowboyRush/Configurar Sprites Cactos")]
    public static void ConfigurarSpritesCactos()
    {
        Object[] todos = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Enviroment/Cactus_Sprite_Sheet.png");
        Sprite sprite0 = null, sprite7 = null, sprite9 = null;
        foreach (Object obj in todos)
        {
            if (obj is Sprite s)
            {
                if (s.name == "Cactus_Sprite_Sheet_0") sprite0 = s;
                else if (s.name == "Cactus_Sprite_Sheet_7") sprite7 = s;
                else if (s.name == "Cactus_Sprite_Sheet_9") sprite9 = s;
            }
        }
        AtribuirSpriteCacto("Cacto",   sprite0);
        AtribuirSpriteCacto("Cacto_2", sprite7);
        AtribuirSpriteCacto("Cacto_3", sprite9);
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log($"[CowboyRush] Cactos configurados: {sprite0?.name}, {sprite7?.name}, {sprite9?.name}");
    }

    private static void AtribuirSpriteCacto(string nome, Sprite sprite)
    {
        if (sprite == null) { Debug.LogWarning($"[CowboyRush] Sprite nulo para {nome}"); return; }
        GameObject go = GameObject.Find(nome);
        if (go == null) { Debug.LogWarning($"[CowboyRush] {nome} não encontrado"); return; }
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        if (sr == null) return;
        Undo.RecordObject(sr, "Assign Cactus Sprite");
        sr.sprite = sprite;
        sr.color  = Color.white;
        EditorUtility.SetDirty(sr);
    }

    [MenuItem("CowboyRush/Montar Tudo (Executar Tudo)")]
    public static void MontarTudo()
    {
        MontarUI();
        ConfigurarPlayerController();
        ConfigurarEnemyController();
        ConfigurarCamera();
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[CowboyRush] Setup completo!");
    }

    private static GameObject CriarRect(string nome, Transform pai)
    {
        var go = new GameObject(nome);
        go.transform.SetParent(pai, false);
        go.AddComponent<RectTransform>();
        return go;
    }
}
