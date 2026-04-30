using UnityEngine;
using UnityEngine.Serialization;

public class BulletController : MonoBehaviour
{
    [SerializeField] private float velocidade = 15f;
    [SerializeField, FormerlySerializedAs("tempoVida")] private float tempoMaximoSemColidir = 10f;
    [SerializeField] private int dano = 1;

    private Rigidbody2D rb;
    private Vector2 direcao;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Inicializar(Vector2 dir)
    {
        direcao = dir.normalized;
        if (direcao == Vector2.zero)
            direcao = Vector2.right;

        // vira o sprite da bala conforme direção
        if (dir.x < 0)
        {
            Vector3 escala = transform.localScale;
            escala.x = -Mathf.Abs(escala.x);
            transform.localScale = escala;
        }
        else
        {
            Vector3 escala = transform.localScale;
            escala.x = Mathf.Abs(escala.x);
            transform.localScale = escala;
        }

        if (rb != null)
            rb.linearVelocity = direcao * velocidade;

        if (tempoMaximoSemColidir > 0f)
            Destroy(gameObject, tempoMaximoSemColidir);
    }

    private void Update()
    {
        if (rb == null)
            transform.Translate(direcao * velocidade * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D outro)
    {
        if (outro.CompareTag("Player") || outro.CompareTag("Bala"))
            return;

        if (outro.CompareTag("Inimigo"))
        {
            EnemyHealth saude = outro.GetComponent<EnemyHealth>();
            if (saude != null)
                saude.ReceberDano(dano);
        }

        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D colisao)
    {
        if (colisao.collider.CompareTag("Player") || colisao.collider.CompareTag("Bala"))
            return;

        Destroy(gameObject);
    }
}
