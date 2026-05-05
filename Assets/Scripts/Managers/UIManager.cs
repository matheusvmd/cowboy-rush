using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private Image[] iconesVida;
    [SerializeField] private GameObject painelMunicao;
    [SerializeField] private Text textoMunicao;
    [SerializeField] private Text textoRecarga;
    [SerializeField] private GameObject painelGameOver;
    [SerializeField] private GameObject painelMenuInicial;
    [SerializeField] private GameObject painelRegras;
    [SerializeField] private GameObject painelVitoria;
    [SerializeField] private GameObject painelPausa;
    [SerializeField] private Button botaoComecar;
    [SerializeField] private Button botaoRegras;
    [SerializeField] private Button botaoVoltarRegras;
    [SerializeField] private Button botaoVoltarMenuGameOver;
    [SerializeField] private Button botaoJogarNovamente;
    [SerializeField] private Button botaoMenuVitoria;
    [SerializeField] private Sprite spriteVidaCheia;
    [SerializeField] private Sprite spriteVidaVazia;
    [SerializeField] private Color corVidaCheia = new Color(0.92f, 0.03f, 0.09f, 1f);
    [SerializeField] private Color corVidaBrilho = new Color(1f, 0.38f, 0.46f, 1f);
    [SerializeField] private Color corVidaBorda = new Color(0.34f, 0.01f, 0.03f, 1f);
    [SerializeField] private Color corVidaVazia = new Color(0.34f, 0.01f, 0.03f, 0.45f);

    private Button botaoRetomarPausa;
    private Button botaoMenuPausaBtn;

    private const int PixelsPorUnidadeCoracao = 16;

    private static readonly string[] MascaraCoracao =
    {
        "................",
        "..BBBB..BBBB....",
        ".BPPFFBBFFFFB...",
        "BPPFFFFFFFFFFB..",
        "BFFFFFFFFFFFFB..",
        "BFFFFFFFFFFFFB..",
        ".BFFFFFFFFFFB...",
        "..BFFFFFFFFB....",
        "...BFFFFFFB.....",
        "....BFFFFB......",
        ".....BFFB.......",
        "......BB........",
        "................",
        "................"
    };

    private Sprite spriteVidaCheiaGerado;
    private Sprite spriteVidaVaziaGerado;
    private Font fontePadrao;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        GarantirEventSystem();
        ConstruirTelasSeNecessario();
        ConfigurarBotoes();
        PrepararSpritesVida();
        AtualizarVidas(iconesVida != null ? iconesVida.Length : 0);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        LimparSpriteGerado(spriteVidaCheiaGerado);
        LimparSpriteGerado(spriteVidaVaziaGerado);
    }

    // ── Atualização de HUD ──────────────────────────────────────────────────

    public void AtualizarVidas(int vidas)
    {
        if (iconesVida == null) return;
        PrepararSpritesVida();
        for (int i = 0; i < iconesVida.Length; i++)
        {
            if (iconesVida[i] == null) continue;
            iconesVida[i].sprite = i < vidas ? SpriteVidaCheia : SpriteVidaVazia;
            iconesVida[i].color = Color.white;
            iconesVida[i].type = Image.Type.Simple;
            iconesVida[i].preserveAspect = true;
            iconesVida[i].raycastTarget = false;
        }
    }

    public void AtualizarMunicao(int tirosRestantes, int tirosMaximos, bool estaRecarregando, float tempoRestanteRecarga)
    {
        ConstruirTelasSeNecessario();
        if (textoMunicao != null)
            textoMunicao.text = "Balas: " + tirosRestantes + "/" + tirosMaximos;
        if (textoRecarga == null) return;
        textoRecarga.gameObject.SetActive(estaRecarregando);
        if (estaRecarregando)
            textoRecarga.text = "Recarregando: " + Mathf.CeilToInt(tempoRestanteRecarga) + "s";
    }

    // ── Navegação entre telas ───────────────────────────────────────────────

    public void MostrarMenuInicial()
    {
        ConstruirTelasSeNecessario();
        ConfigurarBotoes();
        DefinirHudVisivel(false);
        if (painelMenuInicial != null) painelMenuInicial.SetActive(true);
        if (painelRegras      != null) painelRegras.SetActive(false);
        if (painelGameOver    != null) painelGameOver.SetActive(false);
        if (painelVitoria     != null) painelVitoria.SetActive(false);
        if (painelPausa       != null) painelPausa.SetActive(false);
    }

    public void MostrarRegras()
    {
        ConstruirTelasSeNecessario();
        DefinirHudVisivel(false);
        if (painelMenuInicial != null) painelMenuInicial.SetActive(false);
        if (painelRegras      != null) painelRegras.SetActive(true);
        if (painelGameOver    != null) painelGameOver.SetActive(false);
        if (painelVitoria     != null) painelVitoria.SetActive(false);
        if (painelPausa       != null) painelPausa.SetActive(false);
    }

    public void MostrarJogo()
    {
        DefinirHudVisivel(true);
        if (painelMenuInicial != null) painelMenuInicial.SetActive(false);
        if (painelRegras      != null) painelRegras.SetActive(false);
        if (painelGameOver    != null) painelGameOver.SetActive(false);
        if (painelVitoria     != null) painelVitoria.SetActive(false);
        if (painelPausa       != null) painelPausa.SetActive(false);
    }

    public void MostrarGameOver()
    {
        ConstruirTelasSeNecessario();
        ConfigurarBotoes();
        DefinirHudVisivel(false);
        if (painelMenuInicial != null) painelMenuInicial.SetActive(false);
        if (painelRegras      != null) painelRegras.SetActive(false);
        if (painelGameOver    != null) painelGameOver.SetActive(true);
        if (painelVitoria     != null) painelVitoria.SetActive(false);
        if (painelPausa       != null) painelPausa.SetActive(false);
    }

    public void MostrarVitoria()
    {
        ConstruirTelasSeNecessario();
        ConfigurarBotoes();
        DefinirHudVisivel(false);
        if (painelMenuInicial != null) painelMenuInicial.SetActive(false);
        if (painelRegras      != null) painelRegras.SetActive(false);
        if (painelGameOver    != null) painelGameOver.SetActive(false);
        if (painelVitoria     != null) painelVitoria.SetActive(true);
        if (painelPausa       != null) painelPausa.SetActive(false);
    }

    public void MostrarPausa()
    {
        ConstruirTelasSeNecessario();
        ConfigurarBotoes();
        if (painelPausa != null) painelPausa.SetActive(true);
    }

    public void OcultarPausa()
    {
        if (painelPausa != null) painelPausa.SetActive(false);
    }

    // ── Construção de telas ─────────────────────────────────────────────────

    private void ConstruirTelasSeNecessario()
    {
        if (painelMenuInicial == null)
        {
            Transform existente = transform.Find("Painel_MenuInicial");
            painelMenuInicial = existente != null ? existente.gameObject : CriarTelaMenuInicial();
        }

        if (painelRegras == null)
        {
            Transform existente = transform.Find("Painel_Regras");
            painelRegras = existente != null ? existente.gameObject : CriarTelaRegras();
        }

        if (painelGameOver == null)
        {
            Transform existente = transform.Find("Painel_GameOver");
            painelGameOver = existente != null ? existente.gameObject : CriarTelaGameOver();
        }

        if (painelVitoria == null)
        {
            Transform existente = transform.Find("Painel_Vitoria");
            painelVitoria = existente != null ? existente.gameObject : CriarTelaVitoria();
        }

        if (painelPausa == null)
        {
            Transform existente = transform.Find("Painel_Pausa");
            painelPausa = existente != null ? existente.gameObject : CriarTelaPausa();
        }

        if (painelMunicao == null)
        {
            Transform existente = transform.Find("Painel_Municao");
            painelMunicao = existente != null ? existente.gameObject : CriarPainelMunicao();
        }

        if (painelMunicao != null) ColetarReferenciasPainelMunicao();
        ConfigurarPainelGameOver();
    }

    private GameObject CriarTelaMenuInicial()
    {
        GameObject painel = CriarPainelTelaCheia("Painel_MenuInicial", new Color(0.06f, 0.04f, 0.03f, 0.9f));

        CriarTexto("Titulo_Menu", painel.transform, "COWBOY RUSH", 64,
            new Color(1f, 0.78f, 0.34f, 1f), TextAnchor.MiddleCenter,
            new Vector2(0f, 0.5f), new Vector2(1f, 0.5f),
            new Vector2(0f, 130f), new Vector2(-80f, 90f));

        CriarTexto("Subtitulo_Menu", painel.transform, "Sobreviva ao deserto e alcance o portal.", 23,
            Color.white, TextAnchor.MiddleCenter,
            new Vector2(0f, 0.5f), new Vector2(1f, 0.5f),
            new Vector2(0f, 72f), new Vector2(-120f, 42f));

        botaoComecar = CriarBotao("Botao_Comecar", painel.transform, "COMECAR PARTIDA", new Vector2(0f, 0f),   new Vector2(270f, 58f));
        botaoRegras  = CriarBotao("Botao_Regras",  painel.transform, "REGRAS",          new Vector2(0f, -72f), new Vector2(270f, 58f));
        painel.SetActive(false);
        return painel;
    }

    private GameObject CriarTelaRegras()
    {
        GameObject painel = CriarPainelTelaCheia("Painel_Regras", new Color(0.04f, 0.035f, 0.03f, 0.94f));

        CriarTexto("Titulo_Regras", painel.transform, "REGRAS", 54,
            new Color(1f, 0.78f, 0.34f, 1f), TextAnchor.MiddleCenter,
            new Vector2(0f, 0.5f), new Vector2(1f, 0.5f),
            new Vector2(0f, 150f), new Vector2(-100f, 80f));

        CriarTexto("Texto_Regras", painel.transform,
            "Mova-se com A/D ou setas.\nPule com Espaco, W ou seta para cima.\nAgache com S ou seta para baixo.\nAtire com Z, C ou Ctrl esquerdo.\nMonte em cavalos para se proteger de 1 dano.\nEvite cactos e inimigos.\nChegue ao portal para completar a fase.\nEsc para pausar.", 22,
            Color.white, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 0f), new Vector2(680f, 340f));

        botaoVoltarRegras = CriarBotao("Botao_Voltar_Regras", painel.transform, "VOLTAR", new Vector2(0f, -200f), new Vector2(230f, 56f));
        painel.SetActive(false);
        return painel;
    }

    private GameObject CriarTelaGameOver()
    {
        GameObject painel = CriarPainelTelaCheia("Painel_GameOver", new Color(0f, 0f, 0f, 0.75f));

        CriarTexto("Texto_GameOver", painel.transform, "GAME OVER", 60,
            Color.white, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 50f), new Vector2(420f, 100f));

        botaoVoltarMenuGameOver = CriarBotao("Botao_Reiniciar", painel.transform, "MENU INICIAL", new Vector2(0f, -60f), new Vector2(240f, 56f));
        painel.SetActive(false);
        return painel;
    }

    private GameObject CriarTelaVitoria()
    {
        GameObject painel = CriarPainelTelaCheia("Painel_Vitoria", new Color(0.06f, 0.05f, 0.01f, 0.93f));

        GameObject borda = new GameObject("Borda_Vitoria", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        borda.transform.SetParent(painel.transform, false);
        var bordaRect = borda.GetComponent<RectTransform>();
        bordaRect.anchorMin = new Vector2(0.08f, 0.12f);
        bordaRect.anchorMax = new Vector2(0.92f, 0.88f);
        bordaRect.offsetMin = Vector2.zero;
        bordaRect.offsetMax = Vector2.zero;
        borda.GetComponent<Image>().color = new Color(0.85f, 0.68f, 0.05f, 0.18f);

        CriarTexto("Titulo_Vitoria", painel.transform, "VITORIA!", 72,
            new Color(1f, 0.88f, 0.18f, 1f), TextAnchor.MiddleCenter,
            new Vector2(0f, 0.5f), new Vector2(1f, 0.5f),
            new Vector2(0f, 100f), new Vector2(-80f, 110f));

        CriarTexto("Sub_Vitoria", painel.transform, "Fase concluida! Muito bem, cowboy!", 28,
            new Color(1f, 0.95f, 0.75f, 1f), TextAnchor.MiddleCenter,
            new Vector2(0f, 0.5f), new Vector2(1f, 0.5f),
            new Vector2(0f, 18f), new Vector2(-120f, 48f));

        botaoJogarNovamente = CriarBotao("Botao_JogarNovamente", painel.transform, "JOGAR NOVAMENTE", new Vector2(-140f, -80f), new Vector2(250f, 58f));
        botaoMenuVitoria    = CriarBotao("Botao_Menu_Vitoria",   painel.transform, "MENU INICIAL",    new Vector2( 140f, -80f), new Vector2(250f, 58f));

        painel.SetActive(false);
        return painel;
    }

    private GameObject CriarTelaPausa()
    {
        GameObject painel = CriarPainelTelaCheia("Painel_Pausa", new Color(0f, 0f, 0f, 0.65f));

        CriarTexto("Titulo_Pausa", painel.transform, "PAUSADO", 64,
            new Color(1f, 0.78f, 0.34f, 1f), TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 80f), new Vector2(420f, 100f));

        botaoRetomarPausa = CriarBotao("Botao_Retomar", painel.transform, "RETOMAR",      new Vector2(0f,  -20f), new Vector2(260f, 58f));
        botaoMenuPausaBtn = CriarBotao("Botao_Menu_Pausa", painel.transform, "MENU INICIAL", new Vector2(0f, -92f), new Vector2(260f, 58f));

        painel.SetActive(false);
        return painel;
    }

    private GameObject CriarPainelMunicao()
    {
        var painel = new GameObject("Painel_Municao", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        painel.transform.SetParent(transform, false);

        var rect = painel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(1f, 0f);
        rect.anchoredPosition = new Vector2(-20f, 20f);
        rect.sizeDelta = new Vector2(230f, 72f);

        Image fundo = painel.GetComponent<Image>();
        fundo.color = new Color(0.04f, 0.03f, 0.02f, 0.72f);
        fundo.raycastTarget = false;

        textoMunicao = CriarTexto("Texto_Municao", painel.transform, "Balas: 3/3", 24,
            new Color(1f, 0.86f, 0.42f, 1f), TextAnchor.MiddleRight,
            Vector2.zero, Vector2.one, new Vector2(-16f, 12f), new Vector2(-28f, -20f));

        textoRecarga = CriarTexto("Texto_Recarga", painel.transform, "Recarregando: 3s", 18,
            Color.white, TextAnchor.MiddleRight,
            Vector2.zero, Vector2.one, new Vector2(-16f, -18f), new Vector2(-28f, -32f));
        textoRecarga.gameObject.SetActive(false);

        return painel;
    }

    // ── Helpers de criação ──────────────────────────────────────────────────

    private GameObject CriarPainelTelaCheia(string nome, Color cor)
    {
        var painel = new GameObject(nome, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        painel.transform.SetParent(transform, false);
        var rect = painel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        painel.GetComponent<Image>().color = cor;
        return painel;
    }

    private Text CriarTexto(
        string nome, Transform pai, string conteudo, int tamanhoFonte, Color cor,
        TextAnchor alinhamento, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 posicao, Vector2 tamanho)
    {
        var textoGO = new GameObject(nome, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textoGO.transform.SetParent(pai, false);
        var rect = textoGO.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = posicao;
        rect.sizeDelta = tamanho;
        Text texto = textoGO.GetComponent<Text>();
        texto.text = conteudo;
        texto.font = FontePadrao;
        texto.fontSize = tamanhoFonte;
        texto.alignment = alinhamento;
        texto.color = cor;
        texto.horizontalOverflow = HorizontalWrapMode.Wrap;
        texto.verticalOverflow = VerticalWrapMode.Overflow;
        return texto;
    }

    private Button CriarBotao(string nome, Transform pai, string texto, Vector2 posicao, Vector2 tamanho)
    {
        var botaoGO = new GameObject(nome, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        botaoGO.transform.SetParent(pai, false);
        var rect = botaoGO.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posicao;
        rect.sizeDelta = tamanho;
        Image imagem = botaoGO.GetComponent<Image>();
        imagem.color = new Color(0.73f, 0.13f, 0.08f, 1f);
        Button botao = botaoGO.GetComponent<Button>();
        ColorBlock cores = botao.colors;
        cores.normalColor      = new Color(0.73f, 0.13f, 0.08f, 1f);
        cores.highlightedColor = new Color(0.95f, 0.24f, 0.12f, 1f);
        cores.pressedColor     = new Color(0.45f, 0.06f, 0.03f, 1f);
        cores.selectedColor    = cores.highlightedColor;
        botao.colors = cores;
        CriarTexto("Texto", botaoGO.transform, texto, 24, Color.white,
            TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        return botao;
    }

    // ── Configuração de botões ──────────────────────────────────────────────

    private void ConfigurarBotoes()
    {
        ConfigurarBotao(botaoComecar,           () => GameManager.Instance?.IniciarJogo());
        ConfigurarBotao(botaoRegras,            () => GameManager.Instance?.MostrarRegras());
        ConfigurarBotao(botaoVoltarRegras,      () => GameManager.Instance?.MostrarMenuInicial());
        ConfigurarBotao(botaoJogarNovamente,    () => GameManager.Instance?.JogarNovamente());
        ConfigurarBotao(botaoMenuVitoria,       () => GameManager.Instance?.VoltarParaMenuInicial());
        ConfigurarBotao(botaoRetomarPausa,      () => GameManager.Instance?.RetomarJogo());
        ConfigurarBotao(botaoMenuPausaBtn,      () => GameManager.Instance?.VoltarParaMenuInicial());

        if (botaoVoltarMenuGameOver == null && painelGameOver != null)
            botaoVoltarMenuGameOver = painelGameOver.GetComponentInChildren<Button>(true);

        ConfigurarBotao(botaoVoltarMenuGameOver, () => GameManager.Instance?.VoltarParaMenuInicial());
    }

    private void ConfigurarBotao(Button botao, UnityEngine.Events.UnityAction acao)
    {
        if (botao == null) return;
        botao.onClick.RemoveAllListeners();
        botao.onClick.AddListener(acao);
    }

    private void ConfigurarPainelGameOver()
    {
        if (painelGameOver == null) return;
        if (botaoVoltarMenuGameOver == null)
            botaoVoltarMenuGameOver = painelGameOver.GetComponentInChildren<Button>(true);
        if (botaoVoltarMenuGameOver == null) return;
        Image imagem = botaoVoltarMenuGameOver.GetComponent<Image>();
        if (imagem != null) imagem.color = new Color(0.73f, 0.13f, 0.08f, 1f);
        Text texto = botaoVoltarMenuGameOver.GetComponentInChildren<Text>(true);
        if (texto != null)
        {
            texto.text = "MENU INICIAL";
            texto.font = FontePadrao;
            texto.fontSize = 24;
            texto.color = Color.white;
            texto.alignment = TextAnchor.MiddleCenter;
        }
    }

    // ── Utilidades ──────────────────────────────────────────────────────────

    private Font FontePadrao
    {
        get
        {
            if (fontePadrao == null)
                fontePadrao = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return fontePadrao;
        }
    }

    private Sprite SpriteVidaCheia => spriteVidaCheia != null ? spriteVidaCheia : spriteVidaCheiaGerado;
    private Sprite SpriteVidaVazia => spriteVidaVazia != null ? spriteVidaVazia : spriteVidaVaziaGerado;

    private void GarantirEventSystem()
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            var eventSystemGO = new GameObject("EventSystem");
            eventSystem = eventSystemGO.AddComponent<EventSystem>();
        }
        InputSystemUIInputModule inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
        if (inputModule == null)
            inputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        if (inputModule.actionsAsset == null)
            inputModule.AssignDefaultActions();
    }

    private void DefinirHudVisivel(bool visivel)
    {
        if (iconesVida == null || iconesVida.Length == 0 || iconesVida[0] == null)
        {
            if (painelMunicao != null) painelMunicao.SetActive(visivel);
            return;
        }
        Transform painelVidas = iconesVida[0].transform.parent;
        if (painelVidas != null) painelVidas.gameObject.SetActive(visivel);
        if (painelMunicao != null) painelMunicao.SetActive(visivel);
    }

    private void ColetarReferenciasPainelMunicao()
    {
        if (textoMunicao == null)
        {
            Transform t = painelMunicao.transform.Find("Texto_Municao");
            if (t != null) textoMunicao = t.GetComponent<Text>();
        }
        if (textoRecarga == null)
        {
            Transform t = painelMunicao.transform.Find("Texto_Recarga");
            if (t != null) textoRecarga = t.GetComponent<Text>();
        }
    }

    private void PrepararSpritesVida()
    {
        if (spriteVidaCheia == null && spriteVidaCheiaGerado == null) spriteVidaCheiaGerado = CriarSpriteCoracao(true);
        if (spriteVidaVazia == null && spriteVidaVaziaGerado == null) spriteVidaVaziaGerado = CriarSpriteCoracao(false);
    }

    private Sprite CriarSpriteCoracao(bool cheio)
    {
        int largura = MascaraCoracao[0].Length;
        int altura  = MascaraCoracao.Length;
        var textura = new Texture2D(largura, altura, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode   = TextureWrapMode.Clamp,
            name       = cheio ? "Coracao_Vida_Cheio" : "Coracao_Vida_Vazio"
        };
        for (int y = 0; y < altura; y++)
        {
            string linha = MascaraCoracao[altura - 1 - y];
            for (int x = 0; x < largura; x++)
                textura.SetPixel(x, y, CorDoPixel(linha[x], cheio));
        }
        textura.Apply();
        var sprite = Sprite.Create(textura, new Rect(0, 0, largura, altura),
            new Vector2(0.5f, 0.5f), PixelsPorUnidadeCoracao, 0, SpriteMeshType.FullRect);
        sprite.name = textura.name;
        return sprite;
    }

    private void LimparSpriteGerado(Sprite sprite)
    {
        if (sprite == null) return;
        Texture2D textura = sprite.texture;
        Destroy(sprite);
        Destroy(textura);
    }

    private Color CorDoPixel(char pixel, bool cheio)
    {
        if (pixel == 'B') return cheio ? corVidaBorda : corVidaVazia;
        if (!cheio) return Color.clear;
        if (pixel == 'P') return corVidaBrilho;
        if (pixel == 'F') return corVidaCheia;
        return Color.clear;
    }
}
