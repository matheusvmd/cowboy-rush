using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(Animator))]
public class HorseController : MonoBehaviour
{
    [Header("Movimentação Autônoma")]
    [SerializeField] private float velocidade = 2f;
    [SerializeField] private float distanciaPatrulha = 5f;

    [Header("Configuração")]
    [SerializeField] private float offsetYNoPlayer = 0.6f;
    [SerializeField] private Sprite spriteCavaloSozinho;
    [SerializeField] private Sprite spritePlayerNoCavalo;

    private Vector3 posicaoInicial;
    private bool indoParaDireita = true;
    private bool montado;
    private PlayerHealth playerHealth;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator anim;
    private GameObject playerRef;

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
        foreach(var a in assetsP) if(a is Sprite s && s.name.EndsWith("_0")) { spritePlayerNoCavalo = s; break; }
#endif
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
        playerRef = player;
        playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
            playerHealth.DefinirCavalo(this);

        AudioManager.Instance?.TocarMontar();

        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
        GetComponent<Collider2D>().enabled = false;
        
        // Desativa renderizador do player e usa o sprite do player no cavalo
        var playerSR = player.GetComponent<SpriteRenderer>();
        if (playerSR != null) playerSR.enabled = false;

        if (anim != null) anim.enabled = false; // Desativa animação de cavalo sozinho
        
        // Se o sprite de Cowboy montado não carregou, tenta novamente
        if (spritePlayerNoCavalo == null) CarregarSpritesDefault();
        if (sr != null) 
        {
            sr.sprite = spritePlayerNoCavalo;
            sr.flipX = false; // Garante orientação padrão do sprite sheet
        }

        transform.SetParent(player.transform);
        transform.localPosition = new Vector3(0f, offsetYNoPlayer, 0f);
        Debug.Log("[CowboyRush] Montaria concluída.");
    }

    public void Desmontar()
    {
        Debug.Log("[CowboyRush] Desmontando do cavalo...");
        montado = false;
        transform.SetParent(null);
        posicaoInicial = transform.position; 

        rb.simulated = true;
        GetComponent<Collider2D>().enabled = true;
        
        if (playerRef != null)
        {
            var playerSR = playerRef.GetComponent<SpriteRenderer>();
            if (playerSR != null) playerSR.enabled = true;
            
            if (playerHealth != null) playerHealth.DefinirCavalo(null);
            playerRef = null;
        }

        if (anim != null) anim.enabled = true;
        if (sr != null) sr.sprite = spriteCavaloSozinho;
        Debug.Log("[CowboyRush] Desmontaria concluída.");
    }

    private void AtivarColisor()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = true;
    }

    public void AbsorverDano()
    {
        if (playerHealth != null)
            playerHealth.DefinirCavalo(null);

        if (playerRef != null)
        {
            var playerSR = playerRef.GetComponent<SpriteRenderer>();
            if (playerSR != null) playerSR.enabled = true;
        }

        EfeitosVisuais.SpawnBurst(transform.position, new Color(0.75f, 0.5f, 0.2f), 12, 4f, 0.5f);
        Destroy(gameObject);
    }
}
