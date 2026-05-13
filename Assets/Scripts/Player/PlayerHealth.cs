using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float tempoInvencivel = 1.5f;

    private Animator anim;
    private SpriteRenderer sr;
    private bool invencivel;
    private float timerInvencivel;
    private HorseController cavaloDono;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (!invencivel) return;

        timerInvencivel -= Time.deltaTime;
        if (cavaloDono != null)
        {
            if (sr != null) sr.enabled = false;
            if (timerInvencivel <= 0f)
                invencivel = false;
            return;
        }

        sr.enabled = Mathf.Sin(timerInvencivel * 20f) > 0;

        if (timerInvencivel <= 0f)
        {
            invencivel = false;
            sr.enabled = true;
        }
    }

    public void DefinirCavalo(HorseController cavalo)
    {
        cavaloDono = cavalo;
        if (cavalo == null || sr == null) return;

        invencivel = false;
        sr.enabled = false;
    }

    public void ProtegerAposDesmontar(float duracao)
    {
        invencivel = duracao > 0f;
        timerInvencivel = duracao;
        if (sr != null) sr.enabled = true;
    }

    public void ReceberDano()
    {
        if (cavaloDono != null)
        {
            if (invencivel) return;

            invencivel = true;
            timerInvencivel = tempoInvencivel;
            cavaloDono.AbsorverDano();
            AudioManager.Instance?.TocarDano();
            return;
        }

        if (invencivel) return;

        invencivel = true;
        timerInvencivel = tempoInvencivel;

        if (anim != null)
            anim.SetTrigger("Dano");

        AudioManager.Instance?.TocarDano();
        EfeitosVisuais.SpawnBurst(transform.position, new Color(1f, 0.2f, 0.2f), 10, 3f, 0.45f);
        CameraFollow.Tremer(0.16f, 0.18f);
        UIManager.Instance?.MostrarFlashDano();
        GameManager.Instance?.PerdeuVida();
    }

    private void OnTriggerEnter2D(Collider2D outro)
    {
        if (outro.CompareTag("Inimigo") || outro.CompareTag("Cacto"))
            ReceberDano();
    }
}
