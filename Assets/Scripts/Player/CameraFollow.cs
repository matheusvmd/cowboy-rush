using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Instance { get; private set; }

    [SerializeField] private Transform alvo;
    [SerializeField] private float tempoSuavizacao = 0.16f;
    [SerializeField] private Vector3 offset = new Vector3(0, 2, -10);
    [SerializeField] private float lookAheadHorizontal = 1.2f;
    [SerializeField] private float velocidadeLookAhead = 6f;
    [SerializeField] private float limiteEsquerdoX = -100f;
    [SerializeField] private float limiteDireitoX = 100f;
    [SerializeField] private float limiteInferiorY = -5f;
    [SerializeField] private float limiteSuperiorY = 10f;

    private Vector3 velocidadeSuavizacao;
    private Vector3 posicaoBase;
    private float lookAheadAtual;
    private float ultimoAlvoX;
    private float intensidadeTremor;
    private float tempoTremor;
    private float duracaoTremor;

    private void Awake()
    {
        Instance = this;
        posicaoBase = transform.position;
        if (alvo != null)
            ultimoAlvoX = alvo.position.x;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void LateUpdate()
    {
        if (alvo == null) return;

        float deltaX = alvo.position.x - ultimoAlvoX;
        if (Mathf.Abs(deltaX) > 0.001f)
        {
            float lookAheadDesejado = Mathf.Sign(deltaX) * lookAheadHorizontal;
            lookAheadAtual = Mathf.Lerp(lookAheadAtual, lookAheadDesejado, velocidadeLookAhead * Time.deltaTime);
        }

        Vector3 posAlvo = alvo.position + offset;
        posAlvo.x += lookAheadAtual;
        posAlvo.x = Mathf.Clamp(posAlvo.x, limiteEsquerdoX, limiteDireitoX);
        posAlvo.y = Mathf.Clamp(posAlvo.y, limiteInferiorY, limiteSuperiorY);

        posicaoBase = Vector3.SmoothDamp(posicaoBase, posAlvo, ref velocidadeSuavizacao, tempoSuavizacao);
        transform.position = posicaoBase + CalcularTremor();
        ultimoAlvoX = alvo.position.x;
    }

    public static void Tremer(float intensidade, float duracao)
    {
        if (Instance == null)
            return;

        Instance.intensidadeTremor = Mathf.Max(Instance.intensidadeTremor, intensidade);
        Instance.tempoTremor = Mathf.Max(Instance.tempoTremor, duracao);
        Instance.duracaoTremor = Mathf.Max(Instance.duracaoTremor, duracao);
    }

    private Vector3 CalcularTremor()
    {
        if (tempoTremor <= 0f)
            return Vector3.zero;

        tempoTremor -= Time.deltaTime;
        float queda = duracaoTremor > 0f ? Mathf.Clamp01(tempoTremor / duracaoTremor) : 0f;
        Vector2 ruido = Random.insideUnitCircle * intensidadeTremor * queda;

        if (tempoTremor <= 0f)
        {
            intensidadeTremor = 0f;
            duracaoTremor = 0f;
        }

        return new Vector3(ruido.x, ruido.y, 0f);
    }
}
