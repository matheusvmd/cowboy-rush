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
    [SerializeField] private float velocidadeAgachado = 2.5f;

    private Rigidbody2D rb;
    private Animator anim;
    private CapsuleCollider2D capsuleCol;
    private BoxCollider2D boxCol;

    private bool noChao;
    private bool olhandoDireita = true;
    private float timerDisparo;
    private float timerRecarga;
    private float horizontal;
    private int tirosRestantes;
    private bool recarregando;

    // Agachar
    private bool agachado;
    private Vector2 tamanhoColNormal;
    private Vector2 offsetColNormal;

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
        capsuleCol = GetComponent<CapsuleCollider2D>();
        boxCol = GetComponent<BoxCollider2D>();

        if (capsuleCol != null)
        {
            tamanhoColNormal = capsuleCol.size;
            offsetColNormal  = capsuleCol.offset;
        }
        else if (boxCol != null)
        {
            tamanhoColNormal = boxCol.size;
            offsetColNormal  = boxCol.offset;
        }

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
            anim.SetFloat("Velocidade", 0f);
            return;
        }

        var kb = Keyboard.current;
        if (kb == null) return;

        // Movimento horizontal
        horizontal = 0f;
        if (kb.leftArrowKey.isPressed  || kb.aKey.isPressed) horizontal = -1f;
        if (kb.rightArrowKey.isPressed || kb.dKey.isPressed) horizontal =  1f;

        // Agachar (S ou seta baixo, só no chão)
        bool querAgachar = (kb.downArrowKey.isPressed || kb.sKey.isPressed) && noChao;
        if (querAgachar && !agachado)      Agachar();
        else if (!querAgachar && agachado) Levantar();

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

        // Pulo — não pula agachado
        if (timerBufferPulo > 0f && timerCoyote > 0f && !agachado)
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

        // Animações
        anim.SetFloat("Velocidade", Mathf.Abs(horizontal));
        anim.SetBool("NoChao",    noChao);
        anim.SetBool("Agachado",  agachado);
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

        float velX = horizontal * (agachado ? velocidadeAgachado : velocidade);

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

    private void Agachar()
    {
        agachado = true;
        RedimensionarColisao(0.55f);
    }

    private void Levantar()
    {
        agachado = false;
        RestaurarColisao();
    }

    private void RedimensionarColisao(float fatorAltura)
    {
        Vector2 novoTamanho = new Vector2(tamanhoColNormal.x, tamanhoColNormal.y * fatorAltura);
        Vector2 novoOffset  = new Vector2(offsetColNormal.x,
            offsetColNormal.y - tamanhoColNormal.y * (1f - fatorAltura) * 0.5f);

        if (capsuleCol != null) { capsuleCol.size = novoTamanho; capsuleCol.offset = novoOffset; }
        else if (boxCol != null) { boxCol.size    = novoTamanho; boxCol.offset     = novoOffset; }
    }

    private void RestaurarColisao()
    {
        if (capsuleCol != null) { capsuleCol.size = tamanhoColNormal; capsuleCol.offset = offsetColNormal; }
        else if (boxCol != null) { boxCol.size    = tamanhoColNormal; boxCol.offset     = offsetColNormal; }
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

        AudioManager.Instance?.TocarTiro();
        anim.SetTrigger("Atirar");
    }

    private void IniciarRecarga()
    {
        recarregando = true;
        timerRecarga = tempoRecarga;
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
