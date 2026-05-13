using UnityEditor;
using UnityEngine;

public static class PlatformHitboxFixer
{
    [MenuItem("CowboyRush/Fix/Sincronizar Hitboxes Todas Plataformas")]
    public static void SincronizarHitboxes()
    {
        // Incluindo Plataforma_1 e Plataforma_2 que o usuário reportou erro
        string[] bases = { 
            "Plataforma_1", "Plataforma_2", 
            "Plataforma_Pulo_1", "Plataforma_Pulo_2", "Plataforma_Pulo_3", 
            "Plataforma_Final_1", "Plataforma_Final_2" 
        };

        foreach (var nomeBase in bases)
        {
            GameObject topo = GameObject.Find(nomeBase + "_Topo");
            // Caso não tenha _Topo, tenta o próprio nome se ele for o visual
            GameObject visualRef = (topo != null) ? topo : GameObject.Find(nomeBase);
            GameObject baseVisual = GameObject.Find(nomeBase + "_Base");
            GameObject hitboxObj = GameObject.Find(nomeBase);

            if (visualRef == null) 
            {
                Debug.LogWarning($"[CowboyRush] Referência visual para {nomeBase} não encontrada.");
                continue;
            }

            if (hitboxObj == null) hitboxObj = new GameObject(nomeBase);

            Undo.RecordObject(hitboxObj.transform, "Fix Platform Position");

            // 1. Calcular a extensão total (Bounds)
            Bounds totalBounds = new Bounds(visualRef.transform.position, Vector3.zero);
            
            SpriteRenderer srTopo = visualRef.GetComponent<SpriteRenderer>();
            if (srTopo != null) totalBounds.Encapsulate(srTopo.bounds);

            if (baseVisual != null)
            {
                SpriteRenderer srBase = baseVisual.GetComponent<SpriteRenderer>();
                if (srBase != null) totalBounds.Encapsulate(srBase.bounds);
            }

            // 2. Posicionar Hitbox no topo
            Vector3 novaPos = totalBounds.center;
            // Se tiver topo específico, usa o Y dele, senão usa o topo do bounds
            novaPos.y = (topo != null) ? topo.transform.position.y : totalBounds.max.y - 0.2f;
            hitboxObj.transform.position = novaPos;

            // 3. Configurar Collider
            BoxCollider2D col = hitboxObj.GetComponent<BoxCollider2D>();
            if (col == null) col = hitboxObj.AddComponent<BoxCollider2D>();
            
            Undo.RecordObject(col, "Fix Collider Size");
            col.size = new Vector2(totalBounds.size.x, 0.5f);
            col.offset = Vector2.zero;

            // 4. Configurações de Gameplay
            hitboxObj.layer = 6; // Layer "Chao"
            hitboxObj.tag = "Chao";
            
            Debug.Log($"[CowboyRush] ✅ Sincronizada: {nomeBase} | Largura: {totalBounds.size.x:F2}");
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }
}
