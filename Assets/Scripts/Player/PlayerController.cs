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
    [SerializeField] private int tirosPorCarregamento = 2;
    [SerializeField] private float tempoRecarga = 3f;

    private Rigidbody2D rb;
    private Animator anim;
    private bool noChao;
    private bool olhandoDireita = true;
    private float timerDisparo;
    private float timerRecarga;
    private float horizontal;
    private int tirosRestantes;
    private bool recarregando;

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
        if (GameManager.Instance != null && !GameManager.Instance.JogoAtivo)
        {
            horizontal = 0f;
            anim.SetFloat("Velocidade", 0f);
            return;
        }

        var kb = Keyboard.current;
        if (kb == null) return;

        // Movimento horizontal
        horizontal = 0f;
        if (kb.leftArrowKey.isPressed || kb.aKey.isPressed)  horizontal = -1f;
        if (kb.rightArrowKey.isPressed || kb.dKey.isPressed) horizontal =  1f;

        // Pulo
        bool puloPress = kb.spaceKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame;
        if (puloPress && noChao)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, forcaPulo);
            anim.SetTrigger("Pulo");
        }

        timerDisparo -= Time.deltaTime;

        if (recarregando)
            AtualizarRecarga();

        // Disparo
        bool atirarPress = kb.zKey.wasPressedThisFrame || kb.leftCtrlKey.wasPressedThisFrame || kb.cKey.wasPressedThisFrame;
        if (atirarPress && timerDisparo <= 0f)
        {
            TentarAtirar();
            timerDisparo = intervaloDisparo;
        }

        // Virar personagem
        if (horizontal > 0 && !olhandoDireita) Virar();
        else if (horizontal < 0 && olhandoDireita) Virar();

        // Animações
        anim.SetFloat("Velocidade", Mathf.Abs(horizontal));
        anim.SetBool("NoChao", noChao);
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance != null && !GameManager.Instance.JogoAtivo)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        rb.linearVelocity = new Vector2(horizontal * velocidade, rb.linearVelocity.y);

        if (pontoChao != null)
            noChao = Physics2D.OverlapCircle(pontoChao.position, 0.15f, camadaChao);
    }

    private void TentarAtirar()
    {
        if (recarregando)
            return;

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
        if (bullet != null)
            bullet.Inicializar(direcaoDisparo);

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
        timerRecarga = 0f;
        recarregando = false;
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
