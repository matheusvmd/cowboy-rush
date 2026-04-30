using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;

public static class AnimatorBuilder
{
    [MenuItem("CowboyRush/Criar Animators e Prefab Bala")]
    public static void CriarTudo()
    {
        CriarAnimatorPlayer();
        CriarAnimatorInimigo();
        CriarPrefabBala();
        CriarCacto();
        ConfigurarPlayerPrefabRef();
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[CowboyRush] Animators, prefab bala e cacto criados!");
    }

    static void CriarAnimatorPlayer()
    {
        string path = "Assets/Animators/Player_Animator.controller";
        System.IO.Directory.CreateDirectory("Assets/Animators");

        var ctrl = AnimatorController.CreateAnimatorControllerAtPath(path);

        // Parâmetros
        ctrl.AddParameter("Velocidade", AnimatorControllerParameterType.Float);
        ctrl.AddParameter("NoChao", AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("Pulo", AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("Atirar", AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("Dano", AnimatorControllerParameterType.Trigger);

        var root = ctrl.layers[0].stateMachine;

        // Sprites do cowboy
        var clipsIdle   = CriarClip("Idle",   CarregarSprites("Assets/Sprites/Player/CowBoyIdle.png"),   0.1f);
        var clipsWalk   = CriarClip("Walk",   CarregarSprites("Assets/Sprites/Player/CowBoyWalking.png"), 0.08f);
        var clipsShoot  = CriarClip("Shoot",  CarregarSprites("Assets/Sprites/Player/CowBoyShoot.png"),  0.08f);
        var clipsHurt   = CriarClip("Hurt",   CarregarSprites("Assets/Sprites/Player/CowBoyIdle.png"),   0.1f);

        // Salva clips
        SalvarClip(clipsIdle, "Assets/Animators/Player_Idle.anim");
        SalvarClip(clipsWalk, "Assets/Animators/Player_Walk.anim");
        SalvarClip(clipsShoot, "Assets/Animators/Player_Shoot.anim");
        SalvarClip(clipsHurt, "Assets/Animators/Player_Hurt.anim");

        // Estados
        var stIdle  = root.AddState("Idle");  stIdle.motion  = clipsIdle;
        var stWalk  = root.AddState("Walk");  stWalk.motion  = clipsWalk;
        var stShoot = root.AddState("Shoot"); stShoot.motion = clipsShoot;
        var stHurt  = root.AddState("Hurt");  stHurt.motion  = clipsHurt;

        root.defaultState = stIdle;

        // Transições Idle ↔ Walk
        var t1 = stIdle.AddTransition(stWalk);
        t1.hasExitTime = false; t1.duration = 0;
        t1.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Velocidade");

        var t2 = stWalk.AddTransition(stIdle);
        t2.hasExitTime = false; t2.duration = 0;
        t2.AddCondition(AnimatorConditionMode.Less, 0.1f, "Velocidade");

        // Any State → Shoot
        var tShoot = root.AddAnyStateTransition(stShoot);
        tShoot.hasExitTime = false; tShoot.duration = 0;
        tShoot.AddCondition(AnimatorConditionMode.If, 0, "Atirar");
        var tShootExit = stShoot.AddTransition(stIdle);
        tShootExit.hasExitTime = true; tShootExit.exitTime = 1f; tShootExit.duration = 0;

        // Any State → Hurt
        var tHurt = root.AddAnyStateTransition(stHurt);
        tHurt.hasExitTime = false; tHurt.duration = 0;
        tHurt.AddCondition(AnimatorConditionMode.If, 0, "Dano");
        var tHurtExit = stHurt.AddTransition(stIdle);
        tHurtExit.hasExitTime = true; tHurtExit.exitTime = 1f; tHurtExit.duration = 0;

        AssetDatabase.SaveAssets();

        // Aplica no Player
        var player = GameObject.Find("Player");
        if (player != null)
        {
            var anim = player.GetComponent<Animator>();
            if (anim != null) anim.runtimeAnimatorController = ctrl;
        }

        Debug.Log("[CowboyRush] Animator Player criado: " + path);
    }

    static void CriarAnimatorInimigo()
    {
        string path = "Assets/Animators/Enemy_Animator.controller";

        var ctrl = AnimatorController.CreateAnimatorControllerAtPath(path);

        ctrl.AddParameter("Correndo", AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("Atacando", AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("Ataque", AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("Dano", AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("Morte", AnimatorControllerParameterType.Trigger);

        var root = ctrl.layers[0].stateMachine;

        var clipsIdle   = CriarClip("Enemy_Idle",   CarregarSprites("Assets/Sprites/Enemy/Hyena_idle.png"),   0.12f);
        var clipsWalk   = CriarClip("Enemy_Walk",   CarregarSprites("Assets/Sprites/Enemy/Hyena_walk.png"),   0.1f);
        var clipsAtack  = CriarClip("Enemy_Attack", CarregarSprites("Assets/Sprites/Enemy/Hyena_attack.png"), 0.08f);
        var clipsHurt   = CriarClip("Enemy_Hurt",   CarregarSprites("Assets/Sprites/Enemy/Hyena_hurt.png"),   0.1f);
        var clipsDeath  = CriarClip("Enemy_Death",  CarregarSprites("Assets/Sprites/Enemy/Hyena_death.png"),  0.1f);

        SalvarClip(clipsIdle,  "Assets/Animators/Enemy_Idle.anim");
        SalvarClip(clipsWalk,  "Assets/Animators/Enemy_Walk.anim");
        SalvarClip(clipsAtack, "Assets/Animators/Enemy_Attack.anim");
        SalvarClip(clipsHurt,  "Assets/Animators/Enemy_Hurt.anim");
        SalvarClip(clipsDeath, "Assets/Animators/Enemy_Death.anim");

        var stIdle   = root.AddState("Idle");   stIdle.motion   = clipsIdle;
        var stWalk   = root.AddState("Walk");   stWalk.motion   = clipsWalk;
        var stAttack = root.AddState("Attack"); stAttack.motion = clipsAtack;
        var stHurt   = root.AddState("Hurt");   stHurt.motion   = clipsHurt;
        var stDeath  = root.AddState("Death");  stDeath.motion  = clipsDeath;

        root.defaultState = stIdle;

        // Idle ↔ Walk
        var tw1 = stIdle.AddTransition(stWalk);
        tw1.hasExitTime = false; tw1.duration = 0;
        tw1.AddCondition(AnimatorConditionMode.If, 0, "Correndo");

        var tw2 = stWalk.AddTransition(stIdle);
        tw2.hasExitTime = false; tw2.duration = 0;
        tw2.AddCondition(AnimatorConditionMode.IfNot, 0, "Correndo");

        // Any → Attack trigger
        var ta = root.AddAnyStateTransition(stAttack);
        ta.hasExitTime = false; ta.duration = 0;
        ta.AddCondition(AnimatorConditionMode.If, 0, "Ataque");
        var taExit = stAttack.AddTransition(stIdle);
        taExit.hasExitTime = true; taExit.exitTime = 1f; taExit.duration = 0;

        // Any → Hurt trigger
        var th = root.AddAnyStateTransition(stHurt);
        th.hasExitTime = false; th.duration = 0;
        th.AddCondition(AnimatorConditionMode.If, 0, "Dano");
        var thExit = stHurt.AddTransition(stIdle);
        thExit.hasExitTime = true; thExit.exitTime = 1f; thExit.duration = 0;

        // Any → Death trigger
        var td = root.AddAnyStateTransition(stDeath);
        td.hasExitTime = false; td.duration = 0;
        td.AddCondition(AnimatorConditionMode.If, 0, "Morte");

        AssetDatabase.SaveAssets();

        var inimigo = GameObject.Find("Hyena_Inimigo");
        if (inimigo != null)
        {
            var anim = inimigo.GetComponent<Animator>();
            if (anim != null) anim.runtimeAnimatorController = ctrl;
        }

        Debug.Log("[CowboyRush] Animator Inimigo criado: " + path);
    }

    static void CriarPrefabBala()
    {
        System.IO.Directory.CreateDirectory("Assets/Prefabs");

        var bala = new GameObject("Bala");
        bala.tag = "Bala";

        var sr = bala.AddComponent<SpriteRenderer>();
        // Usa um sprite simples para a bala (amarelo)
        sr.sprite = CarregarSpritePixel();
        sr.color = Color.yellow;
        sr.sortingOrder = 10;
        bala.transform.localScale = new Vector3(0.15f, 0.08f, 1);

        var rb = bala.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var col = bala.AddComponent<CircleCollider2D>();
        col.radius = 0.5f;
        col.isTrigger = true;

        bala.AddComponent<BulletController>();

        var prefabPath = "Assets/Prefabs/Bala.prefab";
        PrefabUtility.SaveAsPrefabAsset(bala, prefabPath);
        Object.DestroyImmediate(bala);

        // Conecta o prefab no PlayerController
        var player = GameObject.Find("Player");
        if (player != null)
        {
            var ctrl = player.GetComponent<PlayerController>();
            var pontoDisparo = player.transform.Find("PontoDisparo");
            if (ctrl != null)
            {
                var so = new SerializedObject(ctrl);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                so.FindProperty("prefabBala").objectReferenceValue = prefab;
                if (pontoDisparo != null)
                    so.FindProperty("pontoDisparo").objectReferenceValue = pontoDisparo;
                so.ApplyModifiedProperties();
            }
        }

        Debug.Log("[CowboyRush] Prefab Bala criado: " + prefabPath);
    }

    static Sprite CarregarSpritePixel()
    {
        var assets = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/pixel_branco.png");
        foreach (var asset in assets)
        {
            if (asset is Sprite sprite)
                return sprite;
        }

        return null;
    }

    static void CriarCacto()
    {
        var cacto = new GameObject("Cacto");
        cacto.tag = "Cacto";
        cacto.transform.position = new Vector3(0, -3.5f, 0);

        var sr = cacto.AddComponent<SpriteRenderer>();
        sr.color = new Color(0.1f, 0.6f, 0.1f);
        sr.sortingOrder = 3;
        cacto.transform.localScale = new Vector3(0.3f, 0.8f, 1);

        var col = cacto.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = Vector2.one;

        cacto.AddComponent<CactusController>();

        // Segundo cacto
        var cacto2 = Object.Instantiate(cacto);
        cacto2.name = "Cacto_2";
        cacto2.transform.position = new Vector3(8, -3.5f, 0);

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[CowboyRush] Cactos criados");
    }

    static void ConfigurarPlayerPrefabRef()
    {
        // Garante que PontoChao está referenciado no PlayerController
        var player = GameObject.Find("Player");
        if (player == null) return;
        var ctrl = player.GetComponent<PlayerController>();
        if (ctrl == null) return;
        var pontoChao = player.transform.Find("PontoChao");
        var so = new SerializedObject(ctrl);
        if (pontoChao != null)
            so.FindProperty("pontoChao").objectReferenceValue = pontoChao;
        so.FindProperty("camadaChao").intValue = 1 << LayerMask.NameToLayer("Chao");
        so.ApplyModifiedProperties();
    }

    // Cria AnimationClip com sprites de um spritesheet
    static AnimationClip CriarClip(string nome, Sprite[] sprites, float frameDuration)
    {
        var clip = new AnimationClip();
        clip.name = nome;
        clip.frameRate = Mathf.RoundToInt(1f / frameDuration);

        if (sprites == null || sprites.Length == 0) return clip;

        var editorCurveBinding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");

        var keyframes = new ObjectReferenceKeyframe[sprites.Length + 1];
        for (int i = 0; i < sprites.Length; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe
            {
                time = i * frameDuration,
                value = sprites[i]
            };
        }
        // Repete último frame para evitar salto
        keyframes[sprites.Length] = new ObjectReferenceKeyframe
        {
            time = sprites.Length * frameDuration,
            value = sprites[sprites.Length - 1]
        };

        AnimationUtility.SetObjectReferenceCurve(clip, editorCurveBinding, keyframes);

        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        return clip;
    }

    static void SalvarClip(AnimationClip clip, string path)
    {
        var existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (existing != null) AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(clip, path);
    }

    static Sprite[] CarregarSprites(string path)
    {
        var sprites = AssetDatabase.LoadAllAssetsAtPath(path);
        var lista = new System.Collections.Generic.List<Sprite>();
        foreach (var s in sprites)
            if (s is Sprite) lista.Add((Sprite)s);
        lista.Sort((a, b) => string.Compare(a.name, b.name));
        return lista.ToArray();
    }
}
