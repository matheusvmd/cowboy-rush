using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class CameraBackgroundController : MonoBehaviour
{
    private const string PrefixoCamada = "CameraBackground_";
    private static readonly int[] CopiasHorizontais = { -1, 0, 1 };

    [SerializeField] private Sprite[] camadas = new Sprite[0];
    [SerializeField] private ModoAjuste modoAjuste = ModoAjuste.PreencherCamera;
    [SerializeField, Min(0.1f)] private float multiplicadorEscala = 1.02f;
    [SerializeField] private Vector2 deslocamento = Vector2.zero;
    [SerializeField, Range(0f, 1f)] private float parallax = 0f;
    [SerializeField, Min(0.1f)] private float distanciaDaCamera = 30f;
    [SerializeField] private int ordemInicial = -20;
    [SerializeField] private int incrementoOrdem = 1;
    [SerializeField] private float ajusteVerticalSky = 0.35f;
    [SerializeField, Min(0.1f)] private float escalaVerticalSky = 1.08f;

    private readonly List<LayerView> camadasGeradas = new List<LayerView>();
    private Camera cameraAlvo;
    private Vector3 cameraReferencia;
    private bool temCameraReferencia;
    private int maiorIndiceCamada;

    private void OnEnable()
    {
        cameraAlvo = GetComponent<Camera>();
        RecriarCamadas();
        AtualizarCamadas(true);
    }

    private void OnDisable()
    {
        LimparCamadas();
    }

    private void LateUpdate()
    {
        AtualizarCamadas(false);
    }

    public void RecriarCamadas()
    {
        LimparCamadas();
        maiorIndiceCamada = 0;

        for (int i = 0; i < camadas.Length; i++)
        {
            Sprite sprite = camadas[i];
            if (sprite == null)
                continue;

            maiorIndiceCamada = Mathf.Max(maiorIndiceCamada, i);

            foreach (int copia in CopiasHorizontais)
            {
                GameObject objetoCamada = new GameObject(PrefixoCamada + sprite.name + "_" + copia);
                objetoCamada.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                objetoCamada.transform.SetParent(transform, false);

                SpriteRenderer renderer = objetoCamada.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.sortingOrder = ordemInicial + i * incrementoOrdem;

                camadasGeradas.Add(new LayerView(objetoCamada.transform, sprite.bounds.size, i, copia));
            }
        }

        temCameraReferencia = false;
    }

    private void AtualizarCamadas(bool resetarReferencia)
    {
        if (cameraAlvo == null)
            cameraAlvo = GetComponent<Camera>();

        if (cameraAlvo == null || camadasGeradas.Count == 0)
            return;

        if (!temCameraReferencia || resetarReferencia)
        {
            cameraReferencia = cameraAlvo.transform.position;
            temCameraReferencia = true;
        }

        float alturaCamera = CalcularAlturaVisivel();
        float larguraCamera = alturaCamera * cameraAlvo.aspect;
        Vector3 deltaCamera = cameraAlvo.transform.position - cameraReferencia;

        for (int i = 0; i < camadasGeradas.Count; i++)
        {
            LayerView camada = camadasGeradas[i];
            if (camada.Transform == null)
                continue;

            float escala = CalcularEscala(camada.TamanhoSprite, larguraCamera, alturaCamera);
            float fatorParallax = CalcularFatorParallax(camada.Indice);
            bool camadaSky = camada.Indice == 0;
            float escalaY = camadaSky ? escala * escalaVerticalSky : escala;
            float ajusteY = camadaSky ? ajusteVerticalSky : 0f;
            float larguraFinal = camada.TamanhoSprite.x * escala;
            float xBase = deslocamento.x - deltaCamera.x * fatorParallax;

            if (larguraFinal > 0f)
                xBase = RepetirCentralizado(xBase, larguraFinal);

            camada.Transform.localScale = new Vector3(escala, escalaY, 1f);
            camada.Transform.localPosition = new Vector3(
                xBase + larguraFinal * camada.CopiaHorizontal,
                deslocamento.y + ajusteY - deltaCamera.y * fatorParallax,
                distanciaDaCamera);
        }
    }

    private float CalcularAlturaVisivel()
    {
        if (cameraAlvo.orthographic)
            return cameraAlvo.orthographicSize * 2f;

        float distancia = Mathf.Max(0.1f, distanciaDaCamera);
        return 2f * distancia * Mathf.Tan(cameraAlvo.fieldOfView * 0.5f * Mathf.Deg2Rad);
    }

    private float CalcularEscala(Vector2 tamanhoSprite, float larguraCamera, float alturaCamera)
    {
        if (tamanhoSprite.x <= 0f || tamanhoSprite.y <= 0f)
            return multiplicadorEscala;

        float escalaX = larguraCamera / tamanhoSprite.x;
        float escalaY = alturaCamera / tamanhoSprite.y;
        float escalaBase = modoAjuste == ModoAjuste.PreencherCamera
            ? Mathf.Max(escalaX, escalaY)
            : Mathf.Min(escalaX, escalaY);

        return escalaBase * multiplicadorEscala;
    }

    private float RepetirCentralizado(float valor, float largura)
    {
        return Mathf.Repeat(valor + largura * 0.5f, largura) - largura * 0.5f;
    }

    private float CalcularFatorParallax(int indice)
    {
        if (parallax <= 0f || maiorIndiceCamada <= 0)
            return 0f;

        float progresso = indice / (float)maiorIndiceCamada;
        return parallax * progresso;
    }

    private void LimparCamadas()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform filho = transform.GetChild(i);
            if (!filho.name.StartsWith(PrefixoCamada, System.StringComparison.Ordinal))
                continue;

            if (Application.isPlaying)
                Destroy(filho.gameObject);
            else
                DestroyImmediate(filho.gameObject);
        }

        camadasGeradas.Clear();
    }

    private sealed class LayerView
    {
        public LayerView(Transform transform, Vector2 tamanhoSprite, int indice, int copiaHorizontal)
        {
            Transform = transform;
            TamanhoSprite = tamanhoSprite;
            Indice = indice;
            CopiaHorizontal = copiaHorizontal;
        }

        public Transform Transform { get; }
        public Vector2 TamanhoSprite { get; }
        public int Indice { get; }
        public int CopiaHorizontal { get; }
    }

    private enum ModoAjuste
    {
        PreencherCamera,
        MostrarInteiro
    }
}
