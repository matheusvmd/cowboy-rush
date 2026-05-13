using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Level2Setup
{
    private const string Level1Path = "Assets/Scenes/Level1.unity";
    private const string Level2Path = "Assets/Scenes/Level2.unity";
    private const string Level3Path = "Assets/Scenes/Level3.unity";
    private const string Bg02Path = "Assets/backgrounds/desert_cartoon/bg_02/";

    [MenuItem("CowboyRush/Configurar Level 2 Intermediario")]
    public static void ConfigurarLevel2()
    {
        Scene cena = EditorSceneManager.OpenScene(Level2Path, OpenSceneMode.Single);

        GarantirTags("Player", "Inimigo", "Cacto", "Cavalo");
        ConfigurarBackgroundBg02();
        LimparObjetosDeGameplay();
        ConfigurarLayoutIntermediario();
        LevelArtBuilder.AplicarChaoEPlataformas();
        ConfigurarObstaculos();
        ConfigurarCobras();
        ConfigurarPassaros();
        ConfigurarCavalo();
        ConfigurarPortalECamera();
        ConfigurarBuildSettings();

        EditorSceneManager.MarkSceneDirty(cena);
        EditorSceneManager.SaveScene(cena);
        AssetDatabase.SaveAssets();

        Debug.Log("[CowboyRush] Level 2 intermediario criado com bg_02, cobras, cactos, passaros e cavalo.");
    }

    private static void GarantirTags(params string[] tags)
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
        if (assets == null || assets.Length == 0)
            return;

        SerializedObject tagManager = new SerializedObject(assets[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        foreach (string tag in tags)
        {
            if (string.IsNullOrEmpty(tag) || TagExiste(tagsProp, tag))
                continue;

            tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
            tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
        }

        tagManager.ApplyModifiedProperties();
    }

    private static bool TagExiste(SerializedProperty tagsProp, string tag)
    {
        if (tag == "Player")
            return true;

        for (int i = 0; i < tagsProp.arraySize; i++)
            if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag)
                return true;

        return false;
    }

    private static void ConfigurarBackgroundBg02()
    {
        Camera cam = Camera.main;
        if (cam == null)
            cam = Object.FindAnyObjectByType<Camera>();
        if (cam == null)
            return;

        CameraBackgroundController bgCtrl = cam.GetComponent<CameraBackgroundController>();
        if (bgCtrl == null)
            bgCtrl = cam.gameObject.AddComponent<CameraBackgroundController>();

        string[] nomesCamadas = { "Sky", "BG_Decor", "Middle_Decor", "Ground_02", "Ground_01", "Foreground" };
        SerializedObject so = new SerializedObject(bgCtrl);
        SerializedProperty propCamadas = so.FindProperty("camadas");
        propCamadas.arraySize = nomesCamadas.Length;

        for (int i = 0; i < nomesCamadas.Length; i++)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Bg02Path + nomesCamadas[i] + ".png");
            propCamadas.GetArrayElementAtIndex(i).objectReferenceValue = sprite;
        }

        so.FindProperty("parallax").floatValue = 0.18f;
        so.ApplyModifiedProperties();
        bgCtrl.RecriarCamadas();
        EditorUtility.SetDirty(bgCtrl);
    }

    private static void LimparObjetosDeGameplay()
    {
        foreach (EnemyController inimigo in Object.FindObjectsByType<EnemyController>(FindObjectsInactive.Include))
            Object.DestroyImmediate(inimigo.gameObject);

        foreach (CactusController cacto in Object.FindObjectsByType<CactusController>(FindObjectsInactive.Include))
            Object.DestroyImmediate(cacto.gameObject);

        foreach (BirdHazard passaro in Object.FindObjectsByType<BirdHazard>(FindObjectsInactive.Include))
            Object.DestroyImmediate(passaro.gameObject);

        GameObject art = EncontrarObjetoNaCena("LevelArt_Ground");
        if (art != null)
            Object.DestroyImmediate(art);
    }

    private static void ConfigurarLayoutIntermediario()
    {
        ConfigurarPlayer();

        ConfigurarTerreno("Chao_Principal", new Vector3(11f, -4f, 0f), new Vector2(36f, 2f));
        ConfigurarTerreno("Chao_Principal (1)", new Vector3(72f, -4f, 0f), new Vector2(88f, 2f));

        LimparPlataformasExistentes();

        CriarPlataforma("Plataforma_Pulo_1", new Vector3(24f, -1.95f, 0f), new Vector2(9f, 0.55f));
        CriarPlataforma("Plataforma_Pulo_2", new Vector3(37f, -1.05f, 0f), new Vector2(9f, 0.55f));
        CriarPlataforma("Plataforma_Pulo_3", new Vector3(51f, -1.75f, 0f), new Vector2(10f, 0.55f));
        CriarPlataforma("Plataforma_Aerea_1", new Vector3(69f, -0.95f, 0f), new Vector2(11f, 0.55f));
        CriarPlataforma("Plataforma_Aerea_2", new Vector3(85f, -1.65f, 0f), new Vector2(10f, 0.55f));
        CriarPlataforma("Plataforma_Final_1", new Vector3(105f, -0.85f, 0f), new Vector2(12f, 0.55f));
    }

    private static void ConfigurarPlayer()
    {
        GameObject player = EncontrarObjetoNaCena("Player");
        if (player == null)
            return;

        player.transform.position = new Vector3(-8f, -1.45f, 0f);
        player.tag = "Player";
    }

    private static void ConfigurarTerreno(string nome, Vector3 posicao, Vector2 tamanho)
    {
        GameObject terreno = EncontrarObjetoNaCena(nome);
        if (terreno == null)
            terreno = new GameObject(nome);

        terreno.layer = LayerMask.NameToLayer("Chao");
        terreno.transform.position = posicao;
        BoxCollider2D collider = terreno.GetComponent<BoxCollider2D>();
        if (collider == null)
            collider = terreno.AddComponent<BoxCollider2D>();

        collider.size = tamanho;
        collider.offset = Vector2.zero;

        SpriteRenderer sr = terreno.GetComponent<SpriteRenderer>();
        if (sr == null)
            terreno.AddComponent<SpriteRenderer>();
    }

    private static void LimparPlataformasExistentes()
    {
        List<GameObject> remover = new List<GameObject>();
        Scene cena = SceneManager.GetActiveScene();
        foreach (GameObject raiz in cena.GetRootGameObjects())
            ColetarPorPrefixo(raiz.transform, "Plataforma", remover);

        foreach (GameObject go in remover)
            Object.DestroyImmediate(go);
    }

    private static void CriarPlataforma(string nome, Vector3 posicao, Vector2 tamanho)
    {
        GameObject plataforma = new GameObject(nome);
        plataforma.layer = LayerMask.NameToLayer("Chao");
        plataforma.transform.position = posicao;

        BoxCollider2D collider = plataforma.AddComponent<BoxCollider2D>();
        collider.size = tamanho;

        plataforma.AddComponent<SpriteRenderer>();
    }

    private static void ConfigurarObstaculos()
    {
        CriarCacto("Cacto_Entrada", new Vector3(14f, -1.9f, 0f));
        CriarCacto("Cacto_Antes_Parkour", new Vector3(31f, -1.9f, 0f));
        CriarCacto("Cacto_Pouso_1", new Vector3(59f, -1.9f, 0f));
        CriarCacto("Cacto_Corredor_1", new Vector3(78f, -1.9f, 0f));
        CriarCacto("Cacto_Corredor_2", new Vector3(95f, -1.9f, 0f));
        CriarCacto("Cacto_Final_1", new Vector3(116f, -1.9f, 0f));
        CriarCacto("Cacto_Final_2", new Vector3(124f, -1.9f, 0f));
    }

    private static void CriarCacto(string nome, Vector3 posicao)
    {
        Sprite sprite = CarregarPrimeiroSprite("Assets/Sprites/Enviroment/Cactus_Sprite_Sheet.png");

        GameObject cacto = new GameObject(nome);
        cacto.tag = "Cacto";
        cacto.transform.position = posicao;
        cacto.transform.localScale = new Vector3(3f, 3f, 1f);

        SpriteRenderer sr = cacto.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 5;

        BoxCollider2D collider = cacto.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(0.28f, 0.36f);
        collider.offset = Vector2.zero;

        cacto.AddComponent<CactusController>();
    }

    private static void ConfigurarCobras()
    {
        CriarCobra("Snake_Entrada", new Vector3(21f, -2.5f, 0f), 1.2f, 5.5f, 1.9f, 3.9f);
        CriarCobra("Snake_Parkour", new Vector3(55f, -2.5f, 0f), 1.2f, 6.5f, 2.1f, 4.4f);
        CriarCobra("Snake_Corredor", new Vector3(86f, -2.5f, 0f), 1.2f, 7.5f, 2.2f, 4.8f);
        CriarCobra("Snake_Final", new Vector3(110f, -2.5f, 0f), 1.2f, 8.5f, 2.25f, 5.1f);
    }

    private static void CriarCobra(string nome, Vector3 posicao, float escala, float rangeDeteccao, float velocidadePatrulha, float velocidadePerseguicao)
    {
        GameObject cobra = new GameObject(nome);
        cobra.tag = "Inimigo";
        cobra.transform.position = posicao;
        cobra.transform.localScale = Vector3.one * escala;

        SpriteRenderer sr = cobra.AddComponent<SpriteRenderer>();
        sr.sprite = CarregarPrimeiroSprite("Assets/Sprites/Enemy/Snake_idle.png");
        sr.sortingOrder = 6;

        Animator animator = cobra.AddComponent<Animator>();
        animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animators/Snake_Animator.controller");

        Rigidbody2D rb = cobra.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.gravityScale = 2.4f;

        BoxCollider2D col = cobra.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.85f, 0.7f);
        col.offset = new Vector2(0f, -0.05f);

        EnemyController ctrl = cobra.AddComponent<EnemyController>();
        EnemyHealth health = cobra.AddComponent<EnemyHealth>();

        Transform limEsq = CriarFilho(cobra.transform, nome + "_LimEsq", new Vector3(-3.5f, 0f, 0f));
        Transform limDir = CriarFilho(cobra.transform, nome + "_LimDir", new Vector3(3.5f, 0f, 0f));

        SerializedObject soCtrl = new SerializedObject(ctrl);
        soCtrl.FindProperty("velocidadePatrulha").floatValue = velocidadePatrulha;
        soCtrl.FindProperty("velocidadePerseguicao").floatValue = velocidadePerseguicao;
        soCtrl.FindProperty("rangeDeteccao").floatValue = rangeDeteccao;
        soCtrl.FindProperty("rangeAtaque").floatValue = 1.1f;
        soCtrl.FindProperty("toleranciaVerticalAtaque").floatValue = 0.65f;
        soCtrl.FindProperty("intervaloAtaque").floatValue = 1.35f;
        soCtrl.FindProperty("limiteEsquerdo").objectReferenceValue = limEsq;
        soCtrl.FindProperty("limiteDireito").objectReferenceValue = limDir;
        soCtrl.FindProperty("limitePatrulhaAuto").floatValue = 3.5f;
        soCtrl.FindProperty("spriteFacesDireitaPadrao").boolValue = false;
        soCtrl.ApplyModifiedProperties();

        SerializedObject soHealth = new SerializedObject(health);
        soHealth.FindProperty("vidaMaxima").intValue = 2;
        soHealth.FindProperty("ehBoss").boolValue = false;
        soHealth.ApplyModifiedProperties();
    }

    private static void ConfigurarPassaros()
    {
        CriarPassaro("Passaro_Plataforma_1", new Vector3(37f, 0.55f, 0f), 1.35f, 5.5f, 3.0f);
        CriarPassaro("Passaro_Plataforma_2", new Vector3(69f, 0.6f, 0f), 1.4f, 6.5f, 3.4f);
        CriarPassaro("Passaro_Final", new Vector3(105f, 0.7f, 0f), 1.45f, 7.5f, 3.8f);
    }

    private static void CriarPassaro(string nome, Vector3 posicao, float escala, float distancia, float velocidade)
    {
        GameObject passaro = new GameObject(nome);
        passaro.tag = "Inimigo";
        passaro.transform.position = posicao;
        passaro.transform.localScale = Vector3.one * escala;

        SpriteRenderer sr = passaro.AddComponent<SpriteRenderer>();
        Sprite[] frames = CarregarSprites("Assets/Sprites/Enemy/Vulture_walk.png");
        sr.sprite = frames.Length > 0 ? frames[0] : CarregarPrimeiroSprite("Assets/Sprites/Enemy/Vulture.png");
        sr.sortingOrder = 8;

        BoxCollider2D col = passaro.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1.15f, 1.05f);
        col.offset = new Vector2(0f, -0.25f);

        EnemyHealth health = passaro.AddComponent<EnemyHealth>();
        SerializedObject soHealth = new SerializedObject(health);
        soHealth.FindProperty("vidaMaxima").intValue = 1;
        soHealth.FindProperty("ehBoss").boolValue = false;
        soHealth.ApplyModifiedProperties();

        BirdHazard hazard = passaro.AddComponent<BirdHazard>();
        SerializedObject so = new SerializedObject(hazard);
        so.FindProperty("velocidade").floatValue = velocidade;
        so.FindProperty("distanciaPatrulha").floatValue = distancia;
        so.FindProperty("fpsAnimacao").floatValue = 10f;
        SerializedProperty framesProp = so.FindProperty("frames");
        framesProp.arraySize = frames.Length;
        for (int i = 0; i < frames.Length; i++)
            framesProp.GetArrayElementAtIndex(i).objectReferenceValue = frames[i];
        so.ApplyModifiedProperties();
    }

    private static void ConfigurarCavalo()
    {
        GameObject cavalo = EncontrarObjetoNaCena("Cavalo_Interativo");
        if (cavalo == null)
            cavalo = new GameObject("Cavalo_Interativo");

        SpriteRenderer sr = cavalo.GetComponent<SpriteRenderer>();
        if (sr == null)
            sr = cavalo.AddComponent<SpriteRenderer>();
        sr.sprite = CarregarPrimeiroSprite("Assets/Sprites/Horse/Horse_SpriteSheet.png");
        sr.sortingOrder = 5;

        Animator animator = cavalo.GetComponent<Animator>();
        if (animator == null)
            animator = cavalo.AddComponent<Animator>();
        animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animators/Horse_Animator.controller");

        Rigidbody2D rb = cavalo.GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = cavalo.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        BoxCollider2D collider = cavalo.GetComponent<BoxCollider2D>();
        if (collider == null)
            collider = cavalo.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(1.5f, 1f);
        collider.offset = Vector2.zero;
        collider.isTrigger = true;

        HorseController controller = cavalo.GetComponent<HorseController>();
        if (controller == null)
            controller = cavalo.AddComponent<HorseController>();

        SerializedObject so = new SerializedObject(controller);
        so.FindProperty("offsetYNoPlayer").floatValue = 0.6f;
        so.ApplyModifiedProperties();

        cavalo.transform.position = new Vector3(7f, -2.3f, 0f);
        cavalo.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
        cavalo.tag = "Cavalo";
    }

    private static void ConfigurarPortalECamera()
    {
        GameObject portal = EncontrarObjetoNaCena("FimDeFase");
        if (portal != null)
            portal.transform.position = new Vector3(130f, -2.35f, 0f);

        Camera cam = Camera.main;
        if (cam == null)
            cam = Object.FindAnyObjectByType<Camera>();
        if (cam == null)
            return;

        CameraFollow follow = cam.GetComponent<CameraFollow>();
        if (follow == null)
            return;

        SerializedObject so = new SerializedObject(follow);
        so.FindProperty("limiteEsquerdoX").floatValue = -12f;
        so.FindProperty("limiteDireitoX").floatValue = 135f;
        so.FindProperty("limiteInferiorY").floatValue = -5f;
        so.FindProperty("limiteSuperiorY").floatValue = 7f;
        so.ApplyModifiedProperties();
    }

    private static void ConfigurarBuildSettings()
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(Level1Path, true),
            new EditorBuildSettingsScene(Level2Path, true),
            new EditorBuildSettingsScene(Level3Path, true)
        };
    }

    private static Transform CriarFilho(Transform parent, string nome, Vector3 localPosition)
    {
        GameObject filho = new GameObject(nome);
        filho.transform.SetParent(parent, false);
        filho.transform.localPosition = localPosition;
        return filho.transform;
    }

    private static Sprite CarregarPrimeiroSprite(string caminho)
    {
        Sprite[] sprites = CarregarSprites(caminho);
        return sprites.Length > 0 ? sprites[0] : null;
    }

    private static Sprite[] CarregarSprites(string caminho)
    {
        List<Sprite> sprites = new List<Sprite>();
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(caminho);
        foreach (Object asset in assets)
            if (asset is Sprite sprite)
                sprites.Add(sprite);

        return sprites.ToArray();
    }

    private static void ColetarPorPrefixo(Transform atual, string prefixo, List<GameObject> encontrados)
    {
        if (atual.name.StartsWith(prefixo, System.StringComparison.Ordinal))
            encontrados.Add(atual.gameObject);

        for (int i = 0; i < atual.childCount; i++)
            ColetarPorPrefixo(atual.GetChild(i), prefixo, encontrados);
    }

    private static GameObject EncontrarObjetoNaCena(string nome)
    {
        Scene cena = SceneManager.GetActiveScene();
        foreach (GameObject raiz in cena.GetRootGameObjects())
        {
            Transform encontrado = EncontrarTransformPorNome(raiz.transform, nome);
            if (encontrado != null)
                return encontrado.gameObject;
        }

        return null;
    }

    private static Transform EncontrarTransformPorNome(Transform atual, string nome)
    {
        if (atual.name == nome)
            return atual;

        for (int i = 0; i < atual.childCount; i++)
        {
            Transform encontrado = EncontrarTransformPorNome(atual.GetChild(i), nome);
            if (encontrado != null)
                return encontrado;
        }

        return null;
    }
}
