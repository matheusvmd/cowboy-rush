using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class BirdHazard : MonoBehaviour
{
    [SerializeField] private float velocidade = 3.2f;
    [SerializeField] private float distanciaPatrulha = 7f;
    [SerializeField] private float fpsAnimacao = 10f;
    [SerializeField] private Sprite[] frames = new Sprite[0];

    private Vector3 posicaoInicial;
    private SpriteRenderer spriteRenderer;
    private bool indoParaDireita = true;
    private float timerAnimacao;
    private int frameAtual;

    private void Awake()
    {
        posicaoInicial = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();

        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void Update()
    {
        if (GameManager.Instance != null && (!GameManager.Instance.JogoAtivo || GameManager.Instance.EstaEmPausa))
            return;

        Patrulhar();
        Animar();
    }

    private void Patrulhar()
    {
        float limiteEsq = posicaoInicial.x - distanciaPatrulha;
        float limiteDir = posicaoInicial.x + distanciaPatrulha;

        if (indoParaDireita && transform.position.x >= limiteDir)
            indoParaDireita = false;
        else if (!indoParaDireita && transform.position.x <= limiteEsq)
            indoParaDireita = true;

        float direcao = indoParaDireita ? 1f : -1f;
        transform.position += Vector3.right * direcao * velocidade * Time.deltaTime;
        spriteRenderer.flipX = !indoParaDireita;
    }

    private void Animar()
    {
        if (frames == null || frames.Length == 0 || fpsAnimacao <= 0f)
            return;

        timerAnimacao += Time.deltaTime;
        if (timerAnimacao < 1f / fpsAnimacao)
            return;

        timerAnimacao -= 1f / fpsAnimacao;
        frameAtual = (frameAtual + 1) % frames.Length;
        spriteRenderer.sprite = frames[frameAtual];
    }

    private void OnTriggerEnter2D(Collider2D outro)
    {
        if (!outro.CompareTag("Player"))
            return;

        PlayerHealth saude = outro.GetComponent<PlayerHealth>();
        if (saude != null)
            saude.ReceberDano();
    }
}
