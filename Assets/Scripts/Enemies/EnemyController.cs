using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class EnemyController : MonoBehaviour
{
    [SerializeField] private float velocidadePatrulha = 2f;
    [SerializeField] private float velocidadePerseguicao = 4f;
    [SerializeField] private float rangeDeteccao = 5f;
    [SerializeField] private float rangeAtaque = 1.2f;
    [SerializeField] private float toleranciaVerticalAtaque = 0.9f;
    [SerializeField] private float intervaloAtaque = 1.5f;
    [SerializeField] private Transform limiteEsquerdo;
    [SerializeField] private Transform limiteDireito;
    [SerializeField] private LayerMask camadaChao;
    [SerializeField] private float distanciaBorda = 0.4f;
    // Raio de patrulha automático quando limites não estiverem configurados no inspector
    [SerializeField] private float limitePatrulhaAuto = 4f;

    // false = sprite aponta para a esquerda por padrão (Hyena, Snake etc.)
    [SerializeField] private bool spriteFacesDireitaPadrao = false;

    // Multiplicador de escala — use 2.5 no boss (Mummy)
    [SerializeField] private float escalaMultiplicador = 1f;

    private Rigidbody2D rb;
    private Animator anim;
    private Transform player;
    private bool olhandoDireita;
    private float timerAtaque;
    // Cooldown entre viragens para evitar flip rápido no mesmo frame
    private float timerVirar;
    private Vector2 posicaoInicial;

    private enum Estado { Patrulha, Perseguindo, Atacando, Morto }
    private Estado estadoAtual = Estado.Patrulha;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        if (!Mathf.Approximately(escalaMultiplicador, 1f))
        {
            Vector3 s = transform.localScale;
            s.x *= escalaMultiplicador;
            s.y *= escalaMultiplicador;
            transform.localScale = s;
        }

        olhandoDireita = spriteFacesDireitaPadrao;
        AplicarOrientacao();
    }

    private void AplicarOrientacao()
    {
        Vector3 escala = transform.localScale;
        float absX = Mathf.Abs(escala.x);
        escala.x = olhandoDireita ? absX : -absX;
        transform.localScale = escala;
    }

    private void Start()
    {
        posicaoInicial = transform.position;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            // Impede colisão física entre inimigo e player — dano é tratado via trigger
            IgnorarColisaoComPlayer(playerObj);
        }

        if (camadaChao == 0)
            camadaChao = LayerMask.GetMask("Chao");
    }

    private void IgnorarColisaoComPlayer(GameObject playerObj)
    {
        Collider2D[] meusColliders   = GetComponents<Collider2D>();
        Collider2D[] collidersPlayer = playerObj.GetComponents<Collider2D>();
        foreach (var meu in meusColliders)
            foreach (var dono in collidersPlayer)
                if (!meu.isTrigger && !dono.isTrigger)
                    Physics2D.IgnoreCollision(meu, dono, true);
    }

    private void Update()
    {
        if (estadoAtual == Estado.Morto) return;
        if (GameManager.Instance != null && (!GameManager.Instance.JogoAtivo || GameManager.Instance.EstaEmPausa))
        {
            anim.SetBool("Correndo", false);
            anim.SetBool("Atacando", false);
            return;
        }

        timerAtaque -= Time.deltaTime;
        timerVirar  -= Time.deltaTime;
        AtualizarEstado();
    }

    private void FixedUpdate()
    {
        if (estadoAtual == Estado.Morto) return;
        if (GameManager.Instance != null && (!GameManager.Instance.JogoAtivo || GameManager.Instance.EstaEmPausa))
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        switch (estadoAtual)
        {
            case Estado.Patrulha:    Patrulhar();  break;
            case Estado.Perseguindo: Perseguir();  break;
            case Estado.Atacando:
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                break;
        }
    }

    private void AtualizarEstado()
    {
        if (player == null) return;

        float distancia = Vector2.Distance(transform.position, player.position);

        float diferencaVertical = Mathf.Abs(player.position.y - transform.position.y);

        if (distancia <= rangeAtaque && diferencaVertical <= toleranciaVerticalAtaque)
        {
            if (estadoAtual != Estado.Atacando) VirarParaPlayer();
            estadoAtual = Estado.Atacando;
            anim.SetBool("Correndo", false);
            anim.SetBool("Atacando", true);

            if (timerAtaque <= 0f)
            {
                anim.SetTrigger("Ataque");
                timerAtaque = intervaloAtaque;
                PlayerHealth ph = player.GetComponent<PlayerHealth>();
                if (ph != null) ph.ReceberDano();
            }
        }
        else if (distancia <= rangeDeteccao)
        {
            if (estadoAtual != Estado.Perseguindo) VirarParaPlayer();
            estadoAtual = Estado.Perseguindo;
            anim.SetBool("Correndo", true);
            anim.SetBool("Atacando", false);
        }
        else if (estadoAtual != Estado.Patrulha)
        {
            // Histerese: só volta a patrulhar quando o player está bem mais longe,
            // evitando jitter na borda exata do range de detecção
            if (distancia > rangeDeteccao * 1.35f)
            {
                estadoAtual = Estado.Patrulha;
                anim.SetBool("Correndo", false);
                anim.SetBool("Atacando", false);
            }
        }
    }

    private void Patrulhar()
    {
        // Usa limites do inspector ou limites automáticos pela posição inicial
        float limDir = limiteDireito  != null ? limiteDireito.position.x  : posicaoInicial.x + limitePatrulhaAuto;
        float limEsq = limiteEsquerdo != null ? limiteEsquerdo.position.x : posicaoInicial.x - limitePatrulhaAuto;

        bool deveVirar = false;

        if ( olhandoDireita && transform.position.x >= limDir) deveVirar = true;
        if (!olhandoDireita && transform.position.x <= limEsq) deveVirar = true;

        // Verifica borda à frente só se ainda não vai virar pelos limites
        if (!deveVirar && timerVirar <= 0f && TendoCairDaBorda())
            deveVirar = true;

        // Cooldown de virada impede flip rápido no mesmo ponto
        if (deveVirar && timerVirar <= 0f)
        {
            Virar();
            timerVirar = 0.4f;
        }

        rb.linearVelocity = new Vector2(
            olhandoDireita ? velocidadePatrulha : -velocidadePatrulha,
            rb.linearVelocity.y);
    }

    private void Perseguir()
    {
        if (player == null) return;

        float dir = player.position.x - transform.position.x;

        // Vira na direção do player com cooldown para não ficar chacoalhando
        if (timerVirar <= 0f)
        {
            if (dir > 0.1f && !olhandoDireita)      { Virar(); timerVirar = 0.2f; }
            else if (dir < -0.1f && olhandoDireita) { Virar(); timerVirar = 0.2f; }
        }

        // Persegue diretamente sem verificar borda (pode cair para alcançar o player)
        rb.linearVelocity = new Vector2(
            Mathf.Sign(dir) * velocidadePerseguicao,
            rb.linearVelocity.y);
    }

    private bool TendoCairDaBorda()
    {
        float direcaoX = olhandoDireita ? distanciaBorda : -distanciaBorda;
        Vector2 origem = (Vector2)transform.position + new Vector2(direcaoX, -0.5f);
        RaycastHit2D hit = Physics2D.Raycast(origem, Vector2.down, 0.4f, camadaChao);
        return hit.collider == null;
    }

    private void Virar()
    {
        olhandoDireita = !olhandoDireita;
        Vector3 escala = transform.localScale;
        escala.x *= -1;
        transform.localScale = escala;
    }

    private void VirarParaPlayer()
    {
        if (player == null) return;
        float dir = player.position.x - transform.position.x;
        bool deveOlharDireita = dir > 0f;
        if (deveOlharDireita == olhandoDireita) return;
        olhandoDireita = deveOlharDireita;
        Vector3 escala = transform.localScale;
        escala.x *= -1;
        transform.localScale = escala;
        timerVirar = 0.2f;
    }

    public void Morrer()
    {
        estadoAtual = Estado.Morto;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0;
        anim.SetTrigger("Morte");

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Destroy(gameObject, 1.5f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangeDeteccao);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangeAtaque);
    }
}
