using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class EnemyController : MonoBehaviour
{
    [SerializeField] private float velocidadePatrulha = 2f;
    [SerializeField] private float velocidadePerseguicao = 4f;
    [SerializeField] private float rangeDeteccao = 5f;
    [SerializeField] private float rangeAtaque = 1.2f;
    [SerializeField] private float intervaloAtaque = 1.5f;
    [SerializeField] private Transform limiteEsquerdo;
    [SerializeField] private Transform limiteDireito;

    // Verifica borda para não cair do chão
    [SerializeField] private LayerMask camadaChao;
    [SerializeField] private float distanciaBorda = 0.4f;

    private Rigidbody2D rb;
    private Animator anim;
    private Transform player;
    private bool olhandoDireita = true;
    private float timerAtaque;

    private enum Estado { Patrulha, Perseguindo, Atacando, Morto }
    private Estado estadoAtual = Estado.Patrulha;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        // Descobre layer do chão automaticamente se não setado
        if (camadaChao == 0)
            camadaChao = LayerMask.GetMask("Chao");
    }

    private void Update()
    {
        if (estadoAtual == Estado.Morto) return;

        timerAtaque -= Time.deltaTime;
        AtualizarEstado();
    }

    private void FixedUpdate()
    {
        if (estadoAtual == Estado.Morto) return;

        switch (estadoAtual)
        {
            case Estado.Patrulha:
                Patrulhar();
                break;
            case Estado.Perseguindo:
                Perseguir();
                break;
            case Estado.Atacando:
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                break;
        }
    }

    private void AtualizarEstado()
    {
        if (player == null) return;

        float distancia = Vector2.Distance(transform.position, player.position);

        if (distancia <= rangeAtaque)
        {
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
            estadoAtual = Estado.Perseguindo;
            anim.SetBool("Correndo", true);
            anim.SetBool("Atacando", false);
        }
        else
        {
            estadoAtual = Estado.Patrulha;
            anim.SetBool("Correndo", false);
            anim.SetBool("Atacando", false);
        }
    }

    private void Patrulhar()
    {
        // Verifica limite de patrulha
        if (limiteEsquerdo != null && limiteDireito != null)
        {
            if (olhandoDireita && transform.position.x >= limiteDireito.position.x)
                Virar();
            else if (!olhandoDireita && transform.position.x <= limiteEsquerdo.position.x)
                Virar();
        }

        // Verifica borda do chão à frente para não cair
        if (TendoCairDaBorda())
            Virar();

        rb.linearVelocity = new Vector2(olhandoDireita ? velocidadePatrulha : -velocidadePatrulha, rb.linearVelocity.y);
    }

    private void Perseguir()
    {
        if (player == null) return;

        float dir = player.position.x - transform.position.x;
        if (dir > 0 && !olhandoDireita) Virar();
        else if (dir < 0 && olhandoDireita) Virar();

        // Não persegue se vai cair
        if (!TendoCairDaBorda())
            rb.linearVelocity = new Vector2(Mathf.Sign(dir) * velocidadePerseguicao, rb.linearVelocity.y);
        else
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    // Verifica se há chão à frente antes de andar
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
