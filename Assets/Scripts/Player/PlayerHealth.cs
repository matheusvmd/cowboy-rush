using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float tempoInvencivel = 1.5f;

    private Animator anim;
    private SpriteRenderer sr;
    private bool invencivel;
    private float timerInvencivel;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (!invencivel) return;

        timerInvencivel -= Time.deltaTime;
        // pisca para indicar invencibilidade
        sr.enabled = Mathf.Sin(timerInvencivel * 20f) > 0;

        if (timerInvencivel <= 0f)
        {
            invencivel = false;
            sr.enabled = true;
        }
    }

    public void ReceberDano()
    {
        if (invencivel) return;

        invencivel = true;
        timerInvencivel = tempoInvencivel;

        if (anim != null)
            anim.SetTrigger("Dano");

        GameManager.Instance?.PerdeuVida();
    }

    private void OnTriggerEnter2D(Collider2D outro)
    {
        if (outro.CompareTag("Inimigo") || outro.CompareTag("Cacto"))
            ReceberDano();
    }
}
