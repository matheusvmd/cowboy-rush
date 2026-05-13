using UnityEditor;
using UnityEngine;

public static class LevelDesignAdjuster
{
    [MenuItem("CowboyRush/Level Design/Baixar Plataformas (Agressivo -3 unidades)")]
    public static void BaixarPlataformasAgressivo()
    {
        // Incluindo todas as plataformas para garantir acessibilidade
        string[] nomes = { 
            "Plataforma_1", "Plataforma_2", 
            "Plataforma_Pulo_1", "Plataforma_Pulo_2", "Plataforma_Pulo_3", 
            "Plataforma_Final_1", "Plataforma_Final_2" 
        };
        
        float decrementoY = 3.0f; // Baixando 3 unidades agora para garantir

        foreach (var nome in nomes)
        {
            GameObject pai = GameObject.Find(nome);
            if (pai == null) continue;

            Undo.RecordObject(pai.transform, "Lower Platform Agressive");
            pai.transform.position += Vector3.down * decrementoY;

            MoverFilhosSeExistirem(nome, decrementoY);
        }

        // Chama o sincronizador atualizado
        PlatformHitboxFixer.SincronizarHitboxes();
        
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[CowboyRush] Todas as plataformas baixadas agressivamente e hitboxes sincronizadas.");
    }

    [MenuItem("CowboyRush/Level Design/Fix Visibilidade Objetos (Cactos e Decor)")]
    public static void FixVisibilidadeObjetos()
    {
        SpriteRenderer[] renderers = Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var sr in renderers)
        {
            if (sr.gameObject.name.StartsWith("CameraBackground_")) continue;

            if (sr.transform.position.z != 0)
            {
                Undo.RecordObject(sr.transform, "Fix Z Position");
                Vector3 pos = sr.transform.position;
                pos.z = 0;
                sr.transform.position = pos;
            }

            if (sr.gameObject.name.Contains("Cacto") || sr.gameObject.name.Contains("Decor"))
            {
                Undo.RecordObject(sr, "Fix Sorting Order");
                sr.sortingOrder = 5; // Aumentado para garantir
            }
        }
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }

    private static void MoverFilhosSeExistirem(string nomeBase, float dy)
    {
        string[] sufixos = { "_Topo", "_Base", "_Topo (1)", "_Base (1)" };
        foreach (var s in sufixos)
        {
            GameObject go = GameObject.Find(nomeBase + s);
            if (go != null && go.transform.parent == null)
            {
                Undo.RecordObject(go.transform, "Lower Platform Visual");
                go.transform.position += Vector3.down * dy;
            }
        }
    }
}
