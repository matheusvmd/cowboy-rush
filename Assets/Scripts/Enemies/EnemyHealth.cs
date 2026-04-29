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

        if (vidaAtual <= 0)
        {
            EnemyController ctrl = GetComponent<EnemyController>();
            if (ctrl != null)
                ctrl.Morrer();
            else
                Destroy(gameObject);
        }
    }
}
