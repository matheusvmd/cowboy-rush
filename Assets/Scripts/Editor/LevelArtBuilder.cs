using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LevelArtBuilder
{
    private const string Level1Path = "Assets/Scenes/Level1.unity";
    private const string RootName = "LevelArt_Ground";
    private const float TileScale = 3f;
    private const int GroundSorting = 2;
    private const int PropSorting = 4;

    private static readonly string[] TerrenoOrdemPreferida =
    {
        "Chao_Principal",
        "Chao_Principal (1)",
        "Plataforma_1",
        "Plataforma_2",
        "Plataforma_3",
        "Plataforma_Fim"
    };

    [MenuItem("CowboyRush/Arte/Aplicar Chao e Plataformas")]
    public static void AplicarChaoEPlataformas()
    {
        Scene cena = AbrirLevel1();
        LimparBackgroundPersistente();
        ConstruirArteTerreno();
        EditorSceneManager.MarkSceneDirty(cena);
        EditorSceneManager.SaveScene(cena);
        Debug.Log("[CowboyRush] Arte do chao/plataformas aplicada.");
    }

    public static void AplicarLevel1Completo()
    {
        AplicarChaoEPlataformas();
    }

    private static Scene AbrirLevel1()
    {
        Scene cena = SceneManager.GetActiveScene();
        if (cena.path == Level1Path)
            return cena;

        return EditorSceneManager.OpenScene(Level1Path, OpenSceneMode.Single);
    }

    private static void LimparBackgroundPersistente()
    {
        Camera camera = Camera.main;
        if (camera == null)
            camera = UnityEngine.Object.FindAnyObjectByType<Camera>();

        if (camera == null)
            return;

        CameraBackgroundController controller = camera.GetComponent<CameraBackgroundController>();
        if (controller == null)
            return;

        controller.RecriarCamadas();
        EditorUtility.SetDirty(controller);
    }

    private static void ConstruirArteTerreno()
    {
        Sprite spriteTopo = CarregarSprite("Assets/Sprites/Tileset/Props/platform-long.png");
        Sprite spriteCorpo = CarregarSprite("Assets/Sprites/Tileset/Props/block-big.png");
        Sprite spriteBloco = CarregarSprite("Assets/Sprites/Tileset/Props/block.png");

        GameObject rootExistente = EncontrarObjetoNaCena(RootName);
        if (rootExistente != null)
            UnityEngine.Object.DestroyImmediate(rootExistente);

        GameObject root = new GameObject(RootName);

        foreach (GameObject terreno in ColetarTerrenos())
        {
            string nome = terreno.name;
            BoxCollider2D collider = terreno.GetComponent<BoxCollider2D>();
            if (collider == null)
            {
                Debug.LogWarning("[CowboyRush] Terreno sem BoxCollider2D: " + nome);
                continue;
            }

            SpriteRenderer rendererOriginal = terreno.GetComponent<SpriteRenderer>();
            if (rendererOriginal != null)
            {
                rendererOriginal.enabled = false;
                EditorUtility.SetDirty(rendererOriginal);
            }

            Bounds bounds = collider.bounds;
            bool chaoPrincipal = nome.StartsWith("Chao_Principal", System.StringComparison.Ordinal);
            ConstruirVisualTerreno(root.transform, nome, bounds, chaoPrincipal, spriteTopo, spriteCorpo, spriteBloco);
        }

        ConstruirProps(root.transform);
        Selection.activeGameObject = root;
        EditorUtility.SetDirty(root);
    }

    private static List<GameObject> ColetarTerrenos()
    {
        List<GameObject> terrenos = new List<GameObject>();
        HashSet<GameObject> adicionados = new HashSet<GameObject>();

        foreach (string nome in TerrenoOrdemPreferida)
            AdicionarTerrenoSeValido(EncontrarObjetoNaCena(nome), terrenos, adicionados);

        Scene cena = SceneManager.GetActiveScene();
        foreach (GameObject raiz in cena.GetRootGameObjects())
            ColetarTerrenosRecursivo(raiz.transform, terrenos, adicionados);

        if (terrenos.Count == 0)
            Debug.LogWarning("[CowboyRush] Nenhum chao/plataforma com BoxCollider2D foi encontrado.");

        return terrenos;
    }

    private static void ColetarTerrenosRecursivo(Transform atual, List<GameObject> terrenos, HashSet<GameObject> adicionados)
    {
        AdicionarTerrenoSeValido(atual.gameObject, terrenos, adicionados);

        for (int i = 0; i < atual.childCount; i++)
            ColetarTerrenosRecursivo(atual.GetChild(i), terrenos, adicionados);
    }

    private static void AdicionarTerrenoSeValido(GameObject objeto, List<GameObject> terrenos, HashSet<GameObject> adicionados)
    {
        if (objeto == null || adicionados.Contains(objeto) || !NomeEhTerreno(objeto.name))
            return;

        if (objeto.GetComponent<BoxCollider2D>() == null)
            return;

        terrenos.Add(objeto);
        adicionados.Add(objeto);
    }

    private static bool NomeEhTerreno(string nome)
    {
        return nome.StartsWith("Chao", StringComparison.Ordinal) ||
            nome.StartsWith("Plataforma", StringComparison.Ordinal);
    }

    private static void ConstruirVisualTerreno(
        Transform root,
        string nome,
        Bounds bounds,
        bool chaoPrincipal,
        Sprite spriteTopo,
        Sprite spriteCorpo,
        Sprite spriteBloco)
    {
        GameObject grupo = new GameObject("Art_" + nome);
        grupo.transform.SetParent(root, false);

        float topoAltura = AlturaMundo(spriteTopo, TileScale);
        float topoY = bounds.max.y - topoAltura * 0.5f;
        CriarLinhaTiles(grupo.transform, nome + "_Topo", spriteTopo, bounds.min.x, bounds.max.x, topoY, TileScale, GroundSorting);

        if (chaoPrincipal)
        {
            float corpoY = bounds.max.y - topoAltura - AlturaMundo(spriteCorpo, TileScale) * 0.5f;
            CriarLinhaTiles(grupo.transform, nome + "_Corpo", spriteCorpo, bounds.min.x, bounds.max.x, corpoY, TileScale, GroundSorting);
            return;
        }

        float undersideY = bounds.min.y - AlturaMundo(spriteBloco, TileScale) * 0.15f;
        CriarLinhaTiles(grupo.transform, nome + "_Base", spriteBloco, bounds.min.x, bounds.max.x, undersideY, TileScale, GroundSorting);
    }

    private static void CriarLinhaTiles(Transform parent, string nome, Sprite sprite, float minX, float maxX, float y, float escala, int sortingOrder)
    {
        if (sprite == null)
            return;

        GameObject linha = new GameObject(nome);
        linha.transform.SetParent(parent, false);

        float larguraTile = Mathf.Max(0.1f, LarguraMundo(sprite, escala));
        int quantidade = Mathf.CeilToInt((maxX - minX) / larguraTile) + 2;
        float xInicial = minX - larguraTile * 0.25f;

        for (int i = 0; i < quantidade; i++)
        {
            float x = xInicial + i * larguraTile;
            GameObject tile = new GameObject(nome + "_" + i.ToString("00"));
            tile.transform.SetParent(linha.transform, false);
            tile.transform.position = new Vector3(x, y, 0f);
            tile.transform.localScale = Vector3.one * escala;

            SpriteRenderer renderer = tile.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
        }
    }

    private static void ConstruirProps(Transform root)
    {
        GameObject grupo = new GameObject("Props_Decorativos");
        grupo.transform.SetParent(root, false);

        CriarProp(grupo.transform, "Rock_Start", "Assets/Sprites/Tileset/Props/rock.png", -7.4f, -2.5f, 2.3f);
        CriarProp(grupo.transform, "Bush_01", "Assets/Sprites/Tileset/Props/bush.png", -1.6f, -2.5f, 2.1f);
        CriarProp(grupo.transform, "Skulls_01", "Assets/Sprites/Tileset/Props/skulls.png", 6.7f, -2.5f, 2.4f);
        CriarProp(grupo.transform, "Rock_01", "Assets/Sprites/Tileset/Props/rock-1.png", 13.2f, -2.5f, 1.35f);
        CriarProp(grupo.transform, "Sign_01", "Assets/Sprites/Tileset/Props/sign.png", 18.4f, -2.5f, 2.6f);
        CriarProp(grupo.transform, "Rock_02", "Assets/Sprites/Tileset/Props/rock-2.png", 25.8f, -2.5f, 1.25f);
    }

    private static void CriarProp(Transform parent, string nome, string caminhoSprite, float x, float yTopo, float escala)
    {
        Sprite sprite = CarregarSprite(caminhoSprite);
        if (sprite == null)
            return;

        GameObject prop = new GameObject(nome);
        prop.transform.SetParent(parent, false);
        prop.transform.position = new Vector3(x, yTopo + sprite.bounds.extents.y * escala, 0f);
        prop.transform.localScale = Vector3.one * escala;

        SpriteRenderer renderer = prop.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = PropSorting;
    }

    private static Sprite CarregarSprite(string caminho)
    {
        UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(caminho);
        foreach (UnityEngine.Object asset in assets)
        {
            Sprite sprite = asset as Sprite;
            if (sprite != null)
                return sprite;
        }

        Debug.LogWarning("[CowboyRush] Sprite nao encontrado: " + caminho);
        return null;
    }

    private static float LarguraMundo(Sprite sprite, float escala)
    {
        return sprite != null ? sprite.bounds.size.x * escala : 1f;
    }

    private static float AlturaMundo(Sprite sprite, float escala)
    {
        return sprite != null ? sprite.bounds.size.y * escala : 1f;
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
