using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int vidaMaxima = 3;
    [SerializeField] private bool ehBoss = false;
    [SerializeField] private Color corFlashDano = new Color(1f, 0.48f, 0.22f, 1f);
    [SerializeField] private float duracaoFlashDano = 0.08f;

    public static bool BossVivo { get; private set; } = false;

    private int vidaAtual;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private Color corOriginal;
    private Coroutine rotinaFlash;

    private void Awake()
    {
        if (ehBoss)
        {
            vidaMaxima = 15;
            BossVivo = true;
        }
        vidaAtual = vidaMaxima;
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            corOriginal = spriteRenderer.color;
    }

    private void OnDestroy()
    {
        if (ehBoss && BossVivo)
            BossVivo = false;
    }

    public void ReceberDano(int dano)
    {
        vidaAtual -= dano;

        if (anim != null)
            anim.SetTrigger("Dano");

        EfeitosVisuais.SpawnBurst(transform.position, new Color(1f, 0.5f, 0f), 6, 2.5f, 0.35f);
        CameraFollow.Tremer(0.05f, 0.08f);
        FlashDano();

        if (vidaAtual <= 0)
        {
            if (ehBoss) BossVivo = false;
            AudioManager.Instance?.TocarMorteInimigo();
            EfeitosVisuais.SpawnBurst(transform.position, new Color(0.9f, 0.2f, 0.1f), 16, 4f, 0.5f);

            EnemyController ctrl = GetComponent<EnemyController>();
            if (ctrl != null)
                ctrl.Morrer();
            else
                Destroy(gameObject);
        }
    }

    private void FlashDano()
    {
        if (spriteRenderer == null)
            return;

        if (rotinaFlash != null)
            StopCoroutine(rotinaFlash);

        rotinaFlash = StartCoroutine(ExecutarFlashDano());
    }

    private IEnumerator ExecutarFlashDano()
    {
        spriteRenderer.color = corFlashDano;
        yield return new WaitForSeconds(duracaoFlashDano);
        if (spriteRenderer != null)
            spriteRenderer.color = corOriginal;
        rotinaFlash = null;
    }
}
