using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float velocidade = 5f;
    [SerializeField] private float forcaPulo = 12f;
    [SerializeField] private Transform pontoChao;
    [SerializeField] private LayerMask camadaChao;
    [SerializeField] private GameObject prefabBala;
    [SerializeField] private Transform pontoDisparo;
    [SerializeField] private float intervaloDisparo = 0.3f;
    [SerializeField] private int tirosPorCarregamento = 3;
    [SerializeField] private float tempoRecarga = 3f;
    [SerializeField] private float aceleracao = 58f;
    [SerializeField] private float desaceleracao = 72f;
    [SerializeField] private float recuoTiro = 0.55f;

    private Rigidbody2D rb;
    private Animator anim;

    private bool noChao;
    private bool olhandoDireita = true;
    private float timerDisparo;
    private float timerRecarga;
    private float horizontal;
    private int tirosRestantes;
    private bool recarregando;
    private HorseController cavaloProximo;
    private HorseController cavaloAtual;

    // Coyote time: permite pular levemente após sair da plataforma
    private const float TempoCoyote = 0.12f;
    private float timerCoyote;

    // Buffer de pulo: registra input antes de tocar o chão
    private const float TempoBufferPulo = 0.14f;
    private float timerBufferPulo;

    // Detecção de parede para anti-wall-stick
    private readonly ContactPoint2D[] contatos = new ContactPoint2D[8];

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        RecarregarInstantaneamente();
    }

    private void Start()
    {
        AtualizarHudMunicao();
    }

    private void Update()
    {
        if (GameManager.Instance != null && (!GameManager.Instance.JogoAtivo || GameManager.Instance.EstaEmPausa))
        {
            horizontal = 0f;
            cavaloAtual?.AtualizarMontaria(horizontal, olhandoDireita);
            anim.SetFloat("Velocidade", 0f);
            return;
        }

        var kb = Keyboard.current;
        if (kb == null) return;

        // Movimento horizontal
        horizontal = 0f;
        if (kb.leftArrowKey.isPressed  || kb.aKey.isPressed) horizontal = -1f;
        if (kb.rightArrowKey.isPressed || kb.dKey.isPressed) horizontal =  1f;

        // Coyote time
        if (noChao)
            timerCoyote = TempoCoyote;
        else
            timerCoyote -= Time.deltaTime;

        // Buffer de pulo
        if (kb.spaceKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame)
            timerBufferPulo = TempoBufferPulo;
        else
            timerBufferPulo -= Time.deltaTime;

        if (timerBufferPulo > 0f && timerCoyote > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, forcaPulo);
            anim.SetTrigger("Pulo");
            AudioManager.Instance?.TocarPulo();
            timerCoyote     = 0f;
            timerBufferPulo = 0f;
        }

        timerDisparo -= Time.deltaTime;
        if (recarregando) AtualizarRecarga();

        // Disparo
        bool atirarPress = kb.zKey.wasPressedThisFrame || kb.leftCtrlKey.wasPressedThisFrame || kb.cKey.wasPressedThisFrame;
        if (atirarPress && timerDisparo <= 0f)
        {
            TentarAtirar();
            timerDisparo = intervaloDisparo;
        }

        // Virar
        if (horizontal > 0 && !olhandoDireita) Virar();
        else if (horizontal < 0 && olhandoDireita) Virar();

        cavaloAtual?.AtualizarMontaria(horizontal, olhandoDireita);

        // Montar/Desmontar Cavalo
        if (kb.eKey.wasPressedThisFrame)
        {
            if (cavaloAtual != null)
            {
                Desmontar();
            }
            else
            {
                // Busca cavalo próximo via física para maior precisão
                Collider2D[] proximos = Physics2D.OverlapCircleAll(transform.position, 2f);
                foreach (var col in proximos)
                {
                    if (col.CompareTag("Cavalo"))
                    {
                        cavaloProximo = col.GetComponent<HorseController>();
                        if (cavaloProximo != null)
                        {
                            Montar();
                            break;
                        }
                    }
                }
            }
        }

        // Animações
        anim.SetFloat("Velocidade", Mathf.Abs(horizontal));
        anim.SetBool("NoChao",    noChao);
    }

    private void Montar()
    {
        cavaloAtual = cavaloProximo;
        cavaloAtual.Montar(gameObject);
        cavaloAtual.AtualizarMontaria(0f, olhandoDireita);
        cavaloProximo = null;
    }

    private void Desmontar()
    {
        cavaloAtual.Desmontar();
        cavaloAtual = null;
    }

    public void NotificarDesmontou(HorseController cavalo)
    {
        if (cavaloAtual == cavalo)
            cavaloAtual = null;
    }

    private void OnTriggerEnter2D(Collider2D outro)
    {
        if (outro.CompareTag("Cavalo"))
        {
            cavaloProximo = outro.GetComponent<HorseController>();
        }
    }

    private void OnTriggerExit2D(Collider2D outro)
    {
        if (outro.CompareTag("Cavalo") && cavaloProximo == outro.GetComponent<HorseController>())
        {
            cavaloProximo = null;
        }
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance != null && (!GameManager.Instance.JogoAtivo || GameManager.Instance.EstaEmPausa))
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        if (pontoChao != null)
            noChao = Physics2D.OverlapCircle(pontoChao.position, 0.15f, camadaChao);

        float velXAlvo = horizontal * velocidade;
        float taxaMudanca = Mathf.Abs(velXAlvo) > 0.01f ? aceleracao : desaceleracao;
        float velX = Mathf.MoveTowards(rb.linearVelocity.x, velXAlvo, taxaMudanca * Time.fixedDeltaTime);

        // Anti-wall-stick: cancela velocidade contra a parede quando no ar
        if (!noChao && horizontal != 0f && TocandoParede())
            velX = 0f;

        rb.linearVelocity = new Vector2(velX, rb.linearVelocity.y);
    }

    // Retorna true se o jogador está empurrando contra uma superfície vertical
    private bool TocandoParede()
    {
        int n = rb.GetContacts(contatos);
        for (int i = 0; i < n; i++)
        {
            Vector2 normal = contatos[i].normal;
            if (Mathf.Abs(normal.x) > 0.7f && Mathf.Abs(normal.y) < 0.4f)
            {
                // Normal aponta para fora da parede; jogador empurra se vai na direção oposta
                if (normal.x * horizontal < 0f)
                    return true;
            }
        }
        return false;
    }

    private void TentarAtirar()
    {
        if (recarregando) return;

        if (tirosRestantes <= 0)
        {
            IniciarRecarga();
            return;
        }

        Atirar();
        tirosRestantes--;
        AtualizarHudMunicao();

        if (tirosRestantes <= 0)
            IniciarRecarga();
    }

    private void Atirar()
    {
        if (prefabBala == null || pontoDisparo == null) return;

        Vector2 direcaoDisparo = olhandoDireita ? Vector2.right : Vector2.left;
        GameObject bala = Instantiate(prefabBala, pontoDisparo.position, Quaternion.identity);
        BulletController bullet = bala.GetComponent<BulletController>();
        if (bullet != null) bullet.Inicializar(direcaoDisparo);

        rb.AddForce(-direcaoDisparo * recuoTiro, ForceMode2D.Impulse);
        EfeitosVisuais.SpawnMuzzleFlash(pontoDisparo.position, direcaoDisparo);
        CameraFollow.Tremer(0.045f, 0.08f);
        AudioManager.Instance?.TocarTiro();
        anim.SetTrigger("Atirar");
    }

    private void IniciarRecarga()
    {
        recarregando = true;
        timerRecarga = tempoRecarga;
        AudioManager.Instance?.TocarRecarga();
        AtualizarHudMunicao();
    }

    private void AtualizarRecarga()
    {
        timerRecarga -= Time.deltaTime;
        if (timerRecarga > 0f)
        {
            AtualizarHudMunicao();
            return;
        }
        RecarregarInstantaneamente();
    }

    private void RecarregarInstantaneamente()
    {
        tirosRestantes = Mathf.Max(1, tirosPorCarregamento);
        timerRecarga   = 0f;
        recarregando   = false;
        AtualizarHudMunicao();
    }

    private void AtualizarHudMunicao()
    {
        UIManager.Instance?.AtualizarMunicao(
            tirosRestantes,
            Mathf.Max(1, tirosPorCarregamento),
            recarregando,
            Mathf.Max(0f, timerRecarga));
    }

    private void Virar()
    {
        olhandoDireita = !olhandoDireita;
        Vector3 escala = transform.localScale;
        escala.x *= -1;
        transform.localScale = escala;
    }

    private void OnDrawGizmosSelected()
    {
        if (pontoChao == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(pontoChao.position, 0.15f);
    }
}
