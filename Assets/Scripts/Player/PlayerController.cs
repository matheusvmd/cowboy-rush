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

    private Rigidbody2D rb;
    private Animator anim;
    private bool noChao;
    private bool olhandoDireita = true;
    private float timerDisparo;
    private float horizontal;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
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

        // Disparo
        timerDisparo -= Time.deltaTime;
        bool atirarPress = kb.zKey.isPressed || kb.leftCtrlKey.isPressed || kb.cKey.isPressed;
        if (atirarPress && timerDisparo <= 0f)
        {
            Atirar();
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

    private void Atirar()
    {
        if (prefabBala == null || pontoDisparo == null) return;

        GameObject bala = Instantiate(prefabBala, pontoDisparo.position, Quaternion.identity);
        BulletController bullet = bala.GetComponent<BulletController>();
        if (bullet != null)
            bullet.Inicializar(olhandoDireita ? Vector2.right : Vector2.left);

        anim.SetTrigger("Atirar");
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
