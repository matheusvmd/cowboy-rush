using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using System.Collections.Generic;

public static class HorseSetup
{
    private const string HorseSpritePath = "Assets/Sprites/Horse/Horse_SpriteSheet.png";
    private const string AnimatorPath = "Assets/Animators/Horse_Animator.controller";

    [MenuItem("CowboyRush/Mecanicas/Configurar Cavalo")]
    public static void ConfigurarCavalo()
    {
        Debug.Log("[CowboyRush] Iniciando configuração do cavalo...");

        // 1. Criar Clips de Animação
        AnimationClip clipIdle = CriarClipHorse("Horse_Idle", 0, 3, 0.15f);
        AnimationClip clipWalk = CriarClipHorse("Horse_Walk", 4, 11, 0.12f);
        
        if (clipIdle == null || clipWalk == null)
        {
            Debug.LogError("[CowboyRush] Falha ao criar clips de animação. Verifique se os sprites foram cortados corretamente.");
            return;
        }

        // 2. Criar Animator Controller
        AnimatorController ctrl = CriarAnimator(clipIdle, clipWalk);
        
        // 3. Criar/Atualizar GameObject do Cavalo na Cena
        GameObject horse = GameObject.Find("Cavalo_Interativo");
        if (horse == null)
        {
            horse = new GameObject("Cavalo_Interativo");
            Debug.Log("[CowboyRush] Criado novo GameObject: Cavalo_Interativo");
        }
        
        // Adicionar componentes necessários
        var sr = horse.GetComponent<SpriteRenderer>();
        if (sr == null) sr = horse.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 5;
        // Definir sprite inicial se houver clips criados
        if (sr.sprite == null && clipIdle != null)
        {
            var sprites = AssetDatabase.LoadAllAssetsAtPath(HorseSpritePath);
            foreach(var s in sprites) if(s is Sprite) { sr.sprite = (Sprite)s; break; }
        }

        var anim = horse.GetComponent<Animator>();
        if (anim == null) anim = horse.AddComponent<Animator>();
        anim.runtimeAnimatorController = ctrl;
        
        var rb = horse.GetComponent<Rigidbody2D>();
        if (rb == null) rb = horse.AddComponent<Rigidbody2D>();
        rb.gravityScale = 1f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        
        var col = horse.GetComponent<BoxCollider2D>();
        if (col == null) col = horse.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1.5f, 1f);
        col.offset = new Vector2(0f, 0f);
        
        // Adicionar script de controle
        var ctrlScript = horse.GetComponent<HorseController>();
        if (ctrlScript == null) ctrlScript = horse.AddComponent<HorseController>();
        
        horse.transform.position = new Vector3(15f, -2.5f, 0f);
        horse.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
        
        // Garante que a tag existe antes de atribuir (ou apenas atribui se você já tiver criado no Unity)
        horse.tag = "Cavalo"; 
        
        Debug.Log("[CowboyRush] Cavalo configurado com escala 1.2x e tag 'Cavalo'!");
    }

    private static AnimationClip CriarClipHorse(string nome, int startIdx, int endIdx, float frameDuration)
    {
        Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(HorseSpritePath);
        List<Sprite> sprites = new List<Sprite>();
        
        // Como o nome dos sprites contém o nome antigo longo, vamos buscar pelo final do nome
        for (int i = startIdx; i <= endIdx; i++)
        {
            string sufixo = "_" + i;
            foreach (var asset in allAssets)
            {
                if (asset is Sprite s && s.name.EndsWith(sufixo))
                {
                    sprites.Add(s);
                    break;
                }
            }
        }

        if (sprites.Count == 0)
        {
            Debug.LogWarning("[CowboyRush] Nenhum sprite encontrado para o clip " + nome + ". Verifique o Sprite Sheet.");
            return null;
        }

        AnimationClip clip = new AnimationClip();
        clip.name = nome;
        clip.frameRate = Mathf.RoundToInt(1f / frameDuration);

        var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        var keyframes = new ObjectReferenceKeyframe[sprites.Count + 1];
        for (int i = 0; i < sprites.Count; i++)
            keyframes[i] = new ObjectReferenceKeyframe { time = i * frameDuration, value = sprites[i] };
        keyframes[sprites.Count] = new ObjectReferenceKeyframe { time = (sprites.Count) * frameDuration, value = sprites[sprites.Count - 1] };

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
        
        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        string clipPath = "Assets/Animators/" + nome + ".anim";
        AssetDatabase.CreateAsset(clip, clipPath);
        return clip;
    }

    private static AnimatorController CriarAnimator(AnimationClip idle, AnimationClip walk)
    {
        var ctrl = AnimatorController.CreateAnimatorControllerAtPath(AnimatorPath);
        ctrl.AddParameter("Walking", AnimatorControllerParameterType.Bool);
        
        var root = ctrl.layers[0].stateMachine;
        var stIdle = root.AddState("Idle");
        stIdle.motion = idle;
        
        var stWalk = root.AddState("Walk");
        stWalk.motion = walk;
        
        root.defaultState = stIdle;
        
        var t1 = stIdle.AddTransition(stWalk);
        t1.AddCondition(AnimatorConditionMode.If, 0, "Walking");
        t1.hasExitTime = false;
        t1.duration = 0;
        
        var t2 = stWalk.AddTransition(stIdle);
        t2.AddCondition(AnimatorConditionMode.IfNot, 0, "Walking");
        t2.hasExitTime = false;
        t2.duration = 0;
        
        AssetDatabase.SaveAssets();
        return ctrl;
    }
}
