using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int vidaMaxima = 3;

    private int vidaAtual;
    private Animator anim;

    private void Awake()
    {
        vidaAtual = vidaMaxima;
        anim = GetComponent<Animator>();
    }

    public void ReceberDano(int dano)
    {
        vidaAtual -= dano;

        if (anim != null)
            anim.SetTrigger("Dano");

        EfeitosVisuais.SpawnBurst(transform.position, new Color(1f, 0.5f, 0f), 6, 2.5f, 0.35f);

        if (vidaAtual <= 0)
        {
            AudioManager.Instance?.TocarMorteInimigo();
            EfeitosVisuais.SpawnBurst(transform.position, new Color(0.9f, 0.2f, 0.1f), 16, 4f, 0.5f);

            EnemyController ctrl = GetComponent<EnemyController>();
            if (ctrl != null)
                ctrl.Morrer();
            else
                Destroy(gameObject);
        }
    }
}
