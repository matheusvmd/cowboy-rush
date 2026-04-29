using UnityEngine;

public class BulletController : MonoBehaviour
{
    [SerializeField] private float velocidade = 15f;
    [SerializeField] private float tempoVida = 2f;
    [SerializeField] private int dano = 1;

    private Vector2 direcao;

    public void Inicializar(Vector2 dir)
    {
        direcao = dir.normalized;
        // vira o sprite da bala conforme direção
        if (dir.x < 0)
        {
            Vector3 escala = transform.localScale;
            escala.x *= -1;
            transform.localScale = escala;
        }
        Destroy(gameObject, tempoVida);
    }

    private void Update()
    {
        transform.Translate(direcao * velocidade * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D outro)
    {
        if (outro.CompareTag("Inimigo"))
        {
            EnemyHealth saude = outro.GetComponent<EnemyHealth>();
            if (saude != null)
                saude.ReceberDano(dano);
            Destroy(gameObject);
        }
        else if (outro.CompareTag("Chao"))
        {
            Destroy(gameObject);
        }
    }
}
