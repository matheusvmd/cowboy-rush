using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(Animator))]
public class HorseController : MonoBehaviour
{
    [Header("Movimentação Autônoma")]
    [SerializeField] private float velocidade = 2f;
    [SerializeField] private float distanciaPatrulha = 5f;

    [Header("Configuração")]
    [SerializeField] private float offsetYNoPlayer = 0.6f;
    [SerializeField] private int vidaMaximaMontado = 2;
    [SerializeField] private float invencibilidadeAoDerrubar = 0.8f;
    [SerializeField] private float fpsCavalgada = 12f;
    [SerializeField] private bool spriteMontadoOlhaDireita = true;
    [SerializeField] private Sprite spriteCavaloSozinho;
    [SerializeField] private Sprite spritePlayerNoCavalo;
    [SerializeField] private Sprite[] framesPlayerNoCavalo = new Sprite[0];

    private Vector3 posicaoInicial;
    private bool indoParaDireita = true;
    private bool montado;
    private int vidaAtual;
    private int frameMontadoAtual;
    private float timerAnimacaoMontado;
    private PlayerHealth playerHealth;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator anim;
    private GameObject playerRef;

    private int VidaMaximaMontado => Mathf.Max(2, vidaMaximaMontado);

    private void OnValidate()
    {
        vidaMaximaMontado = Mathf.Max(2, vidaMaximaMontado);
        invencibilidadeAoDerrubar = Mathf.Max(0f, invencibilidadeAoDerrubar);
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        posicaoInicial = transform.position;

        // Se não tiver sprites definidos, tenta carregar do sheet
        if (spriteCavaloSozinho == null) CarregarSpritesDefault();
    }

    private void CarregarSpritesDefault()
    {
#if UNITY_EDITOR
        string path = "Assets/Sprites/Horse/Horse_SpriteSheet.png";
        Object[] assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path);
        foreach(var a in assets) if(a is Sprite s && s.name.EndsWith("_0")) { spriteCavaloSozinho = s; break; }

        string pathPlayer = "Assets/Sprites/PlayerOnHorse/PlayerOnHorse_SpriteSheet.png";
        Object[] assetsP = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(pathPlayer);
        framesPlayerNoCavalo = assetsP
            .OfType<Sprite>()
            .OrderBy(s => ExtrairIndiceSprite(s.name))
            .ToArray();

        if (framesPlayerNoCavalo.Length > 0)
            spritePlayerNoCavalo = framesPlayerNoCavalo[0];
#endif
    }

    private int ExtrairIndiceSprite(string nome)
    {
        int separador = nome.LastIndexOf('_');
        if (separador < 0 || separador >= nome.Length - 1)
            return 0;

        return int.TryParse(nome.Substring(separador + 1), out int indice) ? indice : 0;
    }

    private void Update()
    {
        if (montado) return;
        Patrulhar();
    }

    private void Patrulhar()
    {
        float limiteEsq = posicaoInicial.x - distanciaPatrulha;
        float limiteDir = posicaoInicial.x + distanciaPatrulha;

        if (indoParaDireita && transform.position.x >= limiteDir)
            indoParaDireita = false;
        else if (!indoParaDireita && transform.position.x <= limiteEsq)
            indoParaDireita = true;

        float direcao = indoParaDireita ? 1f : -1f;
        rb.linearVelocity = new Vector2(direcao * velocidade, rb.linearVelocity.y);

        if (sr != null) sr.flipX = !indoParaDireita;

        if (anim != null) 
        {
            anim.enabled = true;
            anim.SetBool("Walking", true);
        }
    }

    public void Montar(GameObject player)
    {
        Debug.Log("[CowboyRush] Montando no cavalo...");
        montado = true;
        vidaAtual = VidaMaximaMontado;
        playerRef = player;
        playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
            playerHealth.DefinirCavalo(this);
        UIManager.Instance?.AtualizarVidasCavalo(vidaAtual, VidaMaximaMontado, true);

        AudioManager.Instance?.TocarMontar();

        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
        GetComponent<Collider2D>().enabled = false;
        
        // Desativa renderizador do player e usa o sprite do player no cavalo
        var playerSR = player.GetComponent<SpriteRenderer>();
        if (playerSR != null) playerSR.enabled = false;

        if (anim != null) anim.enabled = false; // Desativa animação de cavalo sozinho
        
        // Se o sprite de Cowboy montado não carregou, tenta novamente
        if (spritePlayerNoCavalo == null || framesPlayerNoCavalo == null || framesPlayerNoCavalo.Length == 0) CarregarSpritesDefault();
        frameMontadoAtual = 0;
        timerAnimacaoMontado = 0f;
        if (sr != null) 
        {
            sr.sprite = spritePlayerNoCavalo;
            sr.sortingOrder = 8;
        }

        transform.SetParent(player.transform);
        transform.localPosition = new Vector3(0f, offsetYNoPlayer, 0f);
        AtualizarMontaria(0f, player.transform.localScale.x >= 0f);
        Debug.Log("[CowboyRush] Montaria concluída.");
    }

    public void AtualizarMontaria(float movimentoHorizontal, bool olhandoDireita)
    {
        if (!montado || sr == null) return;

        CorrigirOrientacaoMontado(olhandoDireita);

        bool estaCavalgando = Mathf.Abs(movimentoHorizontal) > 0.05f;
        if (!estaCavalgando || framesPlayerNoCavalo == null || framesPlayerNoCavalo.Length == 0)
        {
            frameMontadoAtual = 0;
            timerAnimacaoMontado = 0f;
            sr.sprite = spritePlayerNoCavalo;
            return;
        }

        timerAnimacaoMontado += Time.deltaTime;
        float intervaloFrame = 1f / Mathf.Max(1f, fpsCavalgada);
        if (timerAnimacaoMontado < intervaloFrame) return;

        timerAnimacaoMontado -= intervaloFrame;
        frameMontadoAtual = (frameMontadoAtual + 1) % framesPlayerNoCavalo.Length;
        sr.sprite = framesPlayerNoCavalo[frameMontadoAtual];
    }

    private void CorrigirOrientacaoMontado(bool olhandoDireita)
    {
        if (playerRef != null)
        {
            float sinalPlayer = Mathf.Sign(playerRef.transform.localScale.x);
            if (Mathf.Approximately(sinalPlayer, 0f)) sinalPlayer = 1f;
            Vector3 escala = transform.localScale;
            escala.x = Mathf.Abs(escala.x) * sinalPlayer;
            transform.localScale = escala;
        }

        sr.flipX = spriteMontadoOlhaDireita ? !olhandoDireita : olhandoDireita;
    }

    public void Desmontar()
    {
        Desmontar(false);
    }

    private void Desmontar(bool cavaloDerrotado)
    {
        Debug.Log("[CowboyRush] Desmontando do cavalo...");
        montado = false;
        UIManager.Instance?.AtualizarVidasCavalo(0, VidaMaximaMontado, false);
        transform.SetParent(null);
        posicaoInicial = transform.position; 

        rb.simulated = true;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = true;
        
        if (playerRef != null)
        {
            var playerSR = playerRef.GetComponent<SpriteRenderer>();
            if (playerSR != null) playerSR.enabled = true;
            
            if (playerHealth != null) playerHealth.DefinirCavalo(null);
            if (cavaloDerrotado && playerHealth != null)
                playerHealth.ProtegerAposDesmontar(invencibilidadeAoDerrubar);
            PlayerController playerController = playerRef.GetComponent<PlayerController>();
            if (playerController != null) playerController.NotificarDesmontou(this);
            playerRef = null;
        }

        if (cavaloDerrotado)
        {
            EfeitosVisuais.SpawnBurst(transform.position, new Color(0.95f, 0.68f, 0.16f), 18, 5f, 0.55f);
            Destroy(gameObject);
            return;
        }

        if (anim != null) anim.enabled = true;
        if (sr != null)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            sr.sprite = spriteCavaloSozinho;
            sr.sortingOrder = 5;
        }
        Debug.Log("[CowboyRush] Desmontaria concluída.");
    }

    private void AtivarColisor()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = true;
    }

    public void AbsorverDano()
    {
        if (!montado) return;

        vidaAtual = Mathf.Max(vidaAtual - 1, 0);
        UIManager.Instance?.AtualizarVidasCavalo(vidaAtual, VidaMaximaMontado, true);

        EfeitosVisuais.SpawnBurst(transform.position, new Color(0.75f, 0.5f, 0.2f), 12, 4f, 0.5f);
        CameraFollow.Tremer(0.12f, 0.12f);

        if (vidaAtual <= 0)
            Desmontar(true);
    }
}
