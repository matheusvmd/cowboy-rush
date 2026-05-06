using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class UIMovingBackgroundLayer : MonoBehaviour
{
    [SerializeField] private Sprite sprite;
    [SerializeField] private float velocidade = 20f;
    [SerializeField] private Color cor = Color.white;

    private readonly Image[] imagens = new Image[3];
    private RectTransform rectTransform;
    private float deslocamento;

    public void Configurar(Sprite novoSprite, float novaVelocidade, Color novaCor)
    {
        sprite = novoSprite;
        velocidade = novaVelocidade;
        cor = novaCor;
        GarantirImagens();
        AtualizarLayout();
    }

    private void OnEnable()
    {
        rectTransform = GetComponent<RectTransform>();
        GarantirImagens();
        AtualizarLayout();
    }

    private void Update()
    {
        if (sprite == null)
            return;

        deslocamento += velocidade * Time.unscaledDeltaTime;
        AtualizarLayout();
    }

    private void OnRectTransformDimensionsChange()
    {
        AtualizarLayout();
    }

    private void GarantirImagens()
    {
        for (int i = 0; i < imagens.Length; i++)
        {
            if (imagens[i] != null)
                continue;

            Transform existente = transform.Find("Copia_" + i);
            GameObject copia = existente != null
                ? existente.gameObject
                : new GameObject("Copia_" + i, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));

            copia.transform.SetParent(transform, false);
            imagens[i] = copia.GetComponent<Image>();
            imagens[i].raycastTarget = false;
        }
    }

    private void AtualizarLayout()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if (rectTransform == null || sprite == null)
            return;

        float larguraPai = rectTransform.rect.width;
        float alturaPai = rectTransform.rect.height;
        if (larguraPai <= 0f || alturaPai <= 0f)
            return;

        float aspectoSprite = sprite.rect.width / Mathf.Max(1f, sprite.rect.height);
        float largura = Mathf.Max(larguraPai, alturaPai * aspectoSprite);
        float altura = largura / aspectoSprite;

        if (altura < alturaPai)
        {
            altura = alturaPai;
            largura = altura * aspectoSprite;
        }

        if (deslocamento >= largura)
            deslocamento -= largura;

        for (int i = 0; i < imagens.Length; i++)
        {
            Image imagem = imagens[i];
            if (imagem == null)
                continue;

            RectTransform rect = imagem.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(largura, altura);
            rect.anchoredPosition = new Vector2(deslocamento + (i - 1) * largura, 0f);

            imagem.sprite = sprite;
            imagem.color = cor;
            imagem.type = Image.Type.Simple;
            imagem.preserveAspect = false;
            imagem.raycastTarget = false;
        }
    }
}
