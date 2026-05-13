using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private Image[] iconesVida;
    [SerializeField] private Image[] iconesVidaCavalo;
    [SerializeField] private GameObject painelMunicao;
    [SerializeField] private Image iconeMunicao;
    [SerializeField] private Text textoMunicao;
    [SerializeField] private Text textoRecarga;
    [SerializeField] private GameObject painelGameOver;
    [SerializeField] private GameObject painelMenuInicial;
    [SerializeField] private GameObject painelRegras;
    [SerializeField] private GameObject painelVitoria;
    [SerializeField] private GameObject painelPausa;
    [SerializeField] private Image flashDano;
    [SerializeField] private Button botaoComecar;
    [SerializeField] private Button botaoRegras;
    [SerializeField] private Button botaoVoltarRegras;
    [SerializeField] private Button botaoVoltarMenuGameOver;
    [SerializeField] private Button botaoJogarNovamente;
    [SerializeField] private Button botaoMenuVitoria;
    [SerializeField] private Button botaoProximaFase;
    [SerializeField] private Sprite spriteVidaCheia;
    [SerializeField] private Sprite spriteVidaVazia;
    [SerializeField] private Sprite spriteMunicao;
    [SerializeField] private Color corVidaCheia = new Color(0.92f, 0.03f, 0.09f, 1f);
    [SerializeField] private Color corVidaBrilho = new Color(1f, 0.38f, 0.46f, 1f);
    [SerializeField] private Color corVidaBorda = new Color(0.34f, 0.01f, 0.03f, 1f);
    [SerializeField] private Color corVidaVazia = new Color(0.34f, 0.01f, 0.03f, 0.45f);
    [SerializeField] private Color corVidaCavaloCheia = new Color(1f, 0.71f, 0.12f, 1f);
    [SerializeField] private Color corVidaCavaloBrilho = new Color(1f, 0.92f, 0.42f, 1f);
    [SerializeField] private Color corVidaCavaloBorda = new Color(0.38f, 0.21f, 0.03f, 1f);
    [SerializeField] private Color corVidaCavaloVazia = new Color(0.38f, 0.21f, 0.03f, 0.42f);
    [SerializeField] private Color corMunicaoNormal = new Color(1f, 0.86f, 0.42f, 1f);
    [SerializeField] private Color corMunicaoBaixa = new Color(1f, 0.42f, 0.24f, 1f);
    [SerializeField] private Color corFlashDanoTela = new Color(1f, 0.05f, 0.02f, 0.45f);

    private Button botaoRetomarPausa;
    private Button botaoMenuPausaBtn;
    private GameObject painelVidaCavalo;
    private RectTransform portalVitoriaRect;
    private Image[] aurasPortalVitoria;
    private Color[] coresPortalVitoria;
    private float alphaFlashDano;
    private int vidasCavaloAtuais;
    private int vidasMaximasCavalo = 2;
    private bool mostrarVidasCavalo;

    private const int PixelsPorUnidadeCoracao = 16;
    private const int PixelsPorUnidadeMunicao = 16;

    private const string TextoRegras =
        "MOVER         A / D  ou  Seta Esq / Dir\n" +
        "PULAR         Espaco, W ou Seta Cima\n" +
        "ATIRAR        C, Z ou Ctrl Esquerdo\n\n" +
        "Cavalos absorvem 2 danos — encoste para montar\n" +
        "Cactos causam dano ao toque\n" +
        "Inimigos patrulham e perseguem o jogador\n" +
        "Chegue ao portal verde para completar a fase\n\n" +
        "ESC para pausar";

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
    private Sprite spriteVidaCavaloCheiaGerado;
    private Sprite spriteVidaCavaloVaziaGerado;
    private Sprite spriteMunicaoGerado;
    private Sprite spritePainelWesternGerado;
    private Sprite spriteBotaoWesternGerado;
    private Sprite spritePortalVitoriaGerado;
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
        AtualizarVidasCavalo(0, vidasMaximasCavalo, false);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        LimparSpriteGerado(spriteVidaCheiaGerado);
        LimparSpriteGerado(spriteVidaVaziaGerado);
        LimparSpriteGerado(spriteVidaCavaloCheiaGerado);
        LimparSpriteGerado(spriteVidaCavaloVaziaGerado);
        LimparSpriteGerado(spriteMunicaoGerado);
        LimparSpriteGerado(spritePainelWesternGerado);
        LimparSpriteGerado(spriteBotaoWesternGerado);
        LimparSpriteGerado(spritePortalVitoriaGerado);
    }

    private void Update()
    {
        AtualizarFlashDano();
        AtualizarPortalVitoria();
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

    public void AtualizarVidasCavalo(int vidas, int vidasMaximas, bool visivel)
    {
        vidasMaximasCavalo = Mathf.Max(1, vidasMaximas);
        vidasCavaloAtuais = Mathf.Clamp(vidas, 0, vidasMaximasCavalo);
        mostrarVidasCavalo = visivel;

        GarantirHudCavalo();

        if (painelVidaCavalo != null)
            painelVidaCavalo.SetActive(visivel && GameManager.Instance != null && GameManager.Instance.JogoAtivo);

        if (iconesVidaCavalo == null) return;
        PrepararSpritesVida();
        for (int i = 0; i < iconesVidaCavalo.Length; i++)
        {
            if (iconesVidaCavalo[i] == null) continue;
            bool cheio = i < vidasCavaloAtuais;
            iconesVidaCavalo[i].sprite = cheio ? SpriteVidaCavaloCheia : SpriteVidaCavaloVazia;
            iconesVidaCavalo[i].color = Color.white;
            iconesVidaCavalo[i].type = Image.Type.Simple;
            iconesVidaCavalo[i].preserveAspect = true;
            iconesVidaCavalo[i].raycastTarget = false;
        }
    }

    public void AtualizarMunicao(int tirosRestantes, int tirosMaximos, bool estaRecarregando, float tempoRestanteRecarga)
    {
        ConstruirTelasSeNecessario();
        if (textoMunicao != null)
        {
            textoMunicao.text = tirosRestantes.ToString();
            textoMunicao.color = estaRecarregando || tirosRestantes <= 1 ? corMunicaoBaixa : corMunicaoNormal;
        }
        if (iconeMunicao != null)
            iconeMunicao.color = estaRecarregando ? new Color(1f, 1f, 1f, 0.55f) : Color.white;
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
        AtualizarTextoRegras();
        DefinirHudVisivel(false);
        if (painelMenuInicial != null) painelMenuInicial.SetActive(false);
        if (painelRegras      != null) painelRegras.SetActive(true);
        if (painelGameOver    != null) painelGameOver.SetActive(false);
        if (painelVitoria     != null) painelVitoria.SetActive(false);
        if (painelPausa       != null) painelPausa.SetActive(false);
    }

    private void AtualizarTextoRegras()
    {
        if (painelRegras == null) return;
        Transform t = EncontrarFilho(painelRegras.transform, "Texto_Regras");
        if (t == null) return;
        Text texto = t.GetComponent<Text>();
        if (texto != null) texto.text = TextoRegras;
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

        if (flashDano == null)
        {
            Transform existente = transform.Find("Flash_Dano");
            flashDano = existente != null ? existente.GetComponent<Image>() : CriarFlashDano();
        }

        if (painelMunicao == null)
        {
            Transform existente = transform.Find("Painel_Municao");
            painelMunicao = existente != null ? existente.gameObject : CriarPainelMunicao();
        }

        if (painelMunicao != null) ColetarReferenciasPainelMunicao();
        GarantirHudCavalo();
        ConfigurarPainelGameOver();
    }

    private GameObject CriarTelaMenuInicial()
    {
        GameObject painel = CriarPainelTelaCheia("Painel_MenuInicial", Color.clear);
        CriarFundoBg03(painel.transform, "Fundo_MenuInicial", 1f);

        GameObject cartaz = CriarCartazWestern("Cartaz_MenuInicial", painel.transform, new Vector2(660f, 430f), new Vector2(0f, 12f));

        CriarLinhaDecorativa(cartaz.transform, "Linha_Topo", new Vector2(0f, 132f), new Vector2(420f, 5f), new Color(0.9f, 0.57f, 0.18f, 0.85f));
        CriarLinhaDecorativa(cartaz.transform, "Linha_Base", new Vector2(0f, -178f), new Vector2(430f, 5f), new Color(0.9f, 0.57f, 0.18f, 0.75f));

        Text titulo = CriarTexto("Titulo_Menu", cartaz.transform, "COWBOY RUSH", 68,
            new Color(1f, 0.78f, 0.24f, 1f), TextAnchor.MiddleCenter,
            new Vector2(0f, 0.5f), new Vector2(1f, 0.5f),
            new Vector2(0f, 82f), new Vector2(-70f, 104f));
        titulo.fontStyle = FontStyle.Bold;
        AdicionarTextoSombra(titulo, new Color(0.12f, 0.045f, 0.01f, 0.95f), new Vector2(4f, -4f));
        AdicionarTextoContorno(titulo, new Color(0.28f, 0.09f, 0.015f, 1f), new Vector2(3f, 3f));

        Text subtitulo = CriarTexto("Subtitulo_Menu", cartaz.transform, "WESTERN RUN", 23,
            new Color(1f, 0.91f, 0.67f, 1f), TextAnchor.MiddleCenter,
            new Vector2(0f, 0.5f), new Vector2(1f, 0.5f),
            new Vector2(0f, 22f), new Vector2(-160f, 40f));
        subtitulo.fontStyle = FontStyle.Bold;
        AdicionarTextoSombra(subtitulo, new Color(0.12f, 0.045f, 0.01f, 0.8f), new Vector2(2f, -2f));

        botaoComecar = CriarBotaoWestern("Botao_Comecar", cartaz.transform, "COMECAR PARTIDA", new Vector2(0f, -64f), new Vector2(335f, 62f));
        botaoRegras  = CriarBotaoWestern("Botao_Regras",  cartaz.transform, "REGRAS",          new Vector2(0f, -138f), new Vector2(335f, 62f));
        painel.SetActive(false);
        return painel;
    }

    private GameObject CriarTelaRegras()
    {
        GameObject painel = CriarPainelTelaCheia("Painel_Regras", Color.clear);
        CriarFundoBg03(painel.transform, "Fundo_Regras", 0.65f);

        GameObject cartaz = CriarCartazWestern("Cartaz_Regras", painel.transform, new Vector2(760f, 520f), new Vector2(0f, 8f));

        Text titulo = CriarTexto("Titulo_Regras", cartaz.transform, "REGRAS", 52,
            new Color(1f, 0.79f, 0.29f, 1f), TextAnchor.MiddleCenter,
            new Vector2(0f, 0.5f), new Vector2(1f, 0.5f),
            new Vector2(0f, 188f), new Vector2(-120f, 76f));
        titulo.fontStyle = FontStyle.Bold;
        AdicionarTextoSombra(titulo, new Color(0.12f, 0.045f, 0.01f, 0.95f), new Vector2(3f, -3f));
        AdicionarTextoContorno(titulo, new Color(0.28f, 0.09f, 0.015f, 1f), new Vector2(2f, 2f));

        CriarLinhaDecorativa(cartaz.transform, "Linha_Regras", new Vector2(0f, 144f), new Vector2(520f, 4f), new Color(0.9f, 0.57f, 0.18f, 0.75f));

        Text textoRegras = CriarTexto("Texto_Regras", cartaz.transform, TextoRegras, 21,
            new Color(1f, 0.93f, 0.78f, 1f), TextAnchor.MiddleLeft,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(8f, -18f), new Vector2(610f, 300f));
        textoRegras.fontStyle = FontStyle.Bold;
        AdicionarTextoSombra(textoRegras, new Color(0.1f, 0.045f, 0.02f, 0.85f), new Vector2(2f, -2f));

        botaoVoltarRegras = CriarBotaoWestern("Botao_Voltar_Regras", cartaz.transform, "VOLTAR", new Vector2(0f, -206f), new Vector2(260f, 58f));
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
        GameObject painel = CriarPainelTelaCheia("Painel_Vitoria", Color.clear);
        CriarFundoBg03(painel.transform, "Fundo_Vitoria", 0.28f);

        GameObject vinheta = CriarRetanguloUI("Vinheta_Vitoria", painel.transform, new Color(0.025f, 0.012f, 0.004f, 0.58f));
        EsticarTela(vinheta.GetComponent<RectTransform>());

        GameObject cartaz = CriarCartazWestern("Cartaz_Vitoria", painel.transform, new Vector2(840f, 560f), new Vector2(0f, 4f));

        CriarPortalVitoria(cartaz.transform);
        CriarLinhaDecorativa(cartaz.transform, "Linha_Vitoria_Topo", new Vector2(0f, 112f), new Vector2(530f, 5f), new Color(0.98f, 0.68f, 0.18f, 0.82f));
        CriarLinhaDecorativa(cartaz.transform, "Linha_Vitoria_Base", new Vector2(0f, -142f), new Vector2(560f, 5f), new Color(0.98f, 0.68f, 0.18f, 0.72f));

        Text selo = CriarTexto("Selo_Vitoria", cartaz.transform, "PORTAL ALCANCADO", 20,
            new Color(0.48f, 1f, 0.66f, 1f), TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 178f), new Vector2(320f, 34f));
        selo.fontStyle = FontStyle.Bold;
        AdicionarTextoSombra(selo, new Color(0.01f, 0.12f, 0.05f, 0.95f), new Vector2(2f, -2f));

        Text titulo = CriarTexto("Titulo_Vitoria", cartaz.transform, "FASE CONCLUIDA", 56,
            new Color(1f, 0.83f, 0.28f, 1f), TextAnchor.MiddleCenter,
            new Vector2(0f, 0.5f), new Vector2(1f, 0.5f),
            new Vector2(0f, 54f), new Vector2(-110f, 88f));
        titulo.fontStyle = FontStyle.Bold;
        AdicionarTextoSombra(titulo, new Color(0.08f, 0.025f, 0.005f, 0.95f), new Vector2(4f, -4f));
        AdicionarTextoContorno(titulo, new Color(0.28f, 0.09f, 0.015f, 1f), new Vector2(3f, 3f));

        Text subtitulo = CriarTexto("Sub_Vitoria", cartaz.transform, "O caminho pelo velho oeste continua.", 25,
            new Color(1f, 0.94f, 0.76f, 1f), TextAnchor.MiddleCenter,
            new Vector2(0f, 0.5f), new Vector2(1f, 0.5f),
            new Vector2(0f, -18f), new Vector2(-170f, 44f));
        subtitulo.fontStyle = FontStyle.Bold;
        AdicionarTextoSombra(subtitulo, new Color(0.1f, 0.045f, 0.02f, 0.85f), new Vector2(2f, -2f));

        Text detalhe = CriarTexto("Detalhe_Vitoria", cartaz.transform, "Respire, recarregue e escolha o proximo passo.", 18,
            new Color(0.91f, 0.76f, 0.48f, 1f), TextAnchor.MiddleCenter,
            new Vector2(0f, 0.5f), new Vector2(1f, 0.5f),
            new Vector2(0f, -70f), new Vector2(-220f, 34f));
        detalhe.fontStyle = FontStyle.Bold;

        botaoProximaFase    = CriarBotaoWestern("Botao_ProximaFase",    cartaz.transform, "PROXIMA FASE", new Vector2(0f, -188f), new Vector2(285f, 62f));
        botaoJogarNovamente = CriarBotaoWestern("Botao_JogarNovamente", cartaz.transform, "REPETIR",      new Vector2(-248f, -188f), new Vector2(205f, 56f));
        botaoMenuVitoria    = CriarBotaoWestern("Botao_Menu_Vitoria",   cartaz.transform, "MENU",         new Vector2(248f, -188f), new Vector2(205f, 56f));

        painel.SetActive(false);
        return painel;
    }

    private void CriarPortalVitoria(Transform pai)
    {
        var portal = new GameObject("Portal_Vitoria", typeof(RectTransform));
        portal.transform.SetParent(pai, false);

        RectTransform portalRect = portal.GetComponent<RectTransform>();
        portalRect.anchorMin = new Vector2(0.5f, 0.5f);
        portalRect.anchorMax = new Vector2(0.5f, 0.5f);
        portalRect.pivot = new Vector2(0.5f, 0.5f);
        portalRect.anchoredPosition = new Vector2(0f, 150f);
        portalRect.sizeDelta = new Vector2(210f, 210f);

        Color verdeEscuro = new Color(0.02f, 0.34f, 0.17f, 0.72f);
        Color verdeMedio = new Color(0.12f, 0.84f, 0.42f, 0.62f);
        Color verdeClaro = new Color(0.62f, 1f, 0.66f, 0.88f);

        for (int i = 0; i < 12; i++)
        {
            GameObject raio = CriarRetanguloUI("Raio_Portal_" + i, portal.transform, new Color(0.55f, 1f, 0.52f, i % 2 == 0 ? 0.34f : 0.2f));
            RectTransform raioRect = raio.GetComponent<RectTransform>();
            raioRect.anchorMin = new Vector2(0.5f, 0.5f);
            raioRect.anchorMax = new Vector2(0.5f, 0.5f);
            raioRect.pivot = new Vector2(0.5f, 0f);
            raioRect.anchoredPosition = Vector2.zero;
            raioRect.sizeDelta = new Vector2(i % 2 == 0 ? 5f : 3f, i % 2 == 0 ? 104f : 82f);
            raioRect.localRotation = Quaternion.Euler(0f, 0f, i * 30f);
        }

        Image auraExterna = CriarImagemPortal("Aura_Externa", portal.transform, new Vector2(205f, 205f), verdeEscuro);
        Image auraMedia = CriarImagemPortal("Aura_Media", portal.transform, new Vector2(154f, 154f), verdeMedio);
        Image nucleo = CriarImagemPortal("Nucleo_Portal", portal.transform, new Vector2(86f, 86f), verdeClaro);

        portalVitoriaRect = portalRect;
        aurasPortalVitoria = new[] { auraExterna, auraMedia, nucleo };
        coresPortalVitoria = new[] { verdeEscuro, verdeMedio, verdeClaro };
        CriarCheckPortal(portal.transform);
    }

    private void AtualizarPortalVitoria()
    {
        if (portalVitoriaRect == null || !portalVitoriaRect.gameObject.activeInHierarchy)
            return;

        float tempo = Time.unscaledTime;
        float pulso = (Mathf.Sin(tempo * 2.4f) + 1f) * 0.5f;
        portalVitoriaRect.localScale = Vector3.one * Mathf.Lerp(0.985f, 1.035f, pulso);

        if (aurasPortalVitoria == null || coresPortalVitoria == null)
            return;

        for (int i = 0; i < aurasPortalVitoria.Length && i < coresPortalVitoria.Length; i++)
        {
            if (aurasPortalVitoria[i] == null) continue;
            Color cor = coresPortalVitoria[i];
            cor.a = Mathf.Clamp01(cor.a * Mathf.Lerp(0.78f, 1.12f, (pulso + i * 0.22f) % 1f));
            aurasPortalVitoria[i].color = cor;
        }
    }

    private void CriarCheckPortal(Transform pai)
    {
        Text check = CriarTexto("Check_Portal", pai, "✓", 58,
            new Color(0.05f, 0.18f, 0.08f, 1f), TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 2f), new Vector2(70f, 70f));
        check.fontStyle = FontStyle.Bold;
        check.raycastTarget = false;
    }

    private Image CriarImagemPortal(string nome, Transform pai, Vector2 tamanho, Color cor)
    {
        var objeto = new GameObject(nome, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        objeto.transform.SetParent(pai, false);

        RectTransform rect = objeto.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = tamanho;

        Image imagem = objeto.GetComponent<Image>();
        imagem.sprite = SpritePortalVitoria;
        imagem.color = cor;
        imagem.raycastTarget = false;
        return imagem;
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

    private Image CriarFlashDano()
    {
        var flashGO = new GameObject("Flash_Dano", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        flashGO.transform.SetParent(transform, false);
        RectTransform rect = flashGO.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image imagem = flashGO.GetComponent<Image>();
        imagem.color = Color.clear;
        imagem.raycastTarget = false;
        flashGO.SetActive(false);
        return imagem;
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
        rect.sizeDelta = new Vector2(190f, 76f);

        Image fundo = painel.GetComponent<Image>();
        fundo.color = new Color(0.04f, 0.03f, 0.02f, 0.72f);
        fundo.raycastTarget = false;

        GameObject iconeGO = new GameObject("Icone_Municao", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconeGO.transform.SetParent(painel.transform, false);
        RectTransform iconeRect = iconeGO.GetComponent<RectTransform>();
        iconeRect.anchorMin = new Vector2(0f, 0.5f);
        iconeRect.anchorMax = new Vector2(0f, 0.5f);
        iconeRect.pivot = new Vector2(0.5f, 0.5f);
        iconeRect.anchoredPosition = new Vector2(38f, 10f);
        iconeRect.sizeDelta = new Vector2(46f, 46f);
        iconeMunicao = iconeGO.GetComponent<Image>();
        iconeMunicao.sprite = SpriteMunicao;
        iconeMunicao.preserveAspect = true;
        iconeMunicao.raycastTarget = false;

        textoMunicao = CriarTexto("Texto_Municao", painel.transform, "3", 30,
            corMunicaoNormal, TextAnchor.MiddleLeft,
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(86f, 10f), new Vector2(70f, 42f));

        textoRecarga = CriarTexto("Texto_Recarga", painel.transform, "Recarregando: 3s", 18,
            Color.white, TextAnchor.MiddleRight,
            Vector2.zero, Vector2.one, new Vector2(-12f, -20f), new Vector2(-24f, -34f));
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

    private void CriarFundoBg03(Transform pai, string nome, float multiplicadorVelocidade)
    {
        var fundo = new GameObject(nome, typeof(RectTransform));
        fundo.transform.SetParent(pai, false);
        EsticarTela(fundo.GetComponent<RectTransform>());

        string[] camadas =
        {
            "Sky",
            "BG_Decor",
            "Middle_Decor",
            "Ground_02",
            "Ground_01",
            "Foreground"
        };

        float[] velocidades = { 7f, 12f, 18f, 28f, 42f, 58f };
        bool carregouAlgumaCamada = false;

        for (int i = 0; i < camadas.Length; i++)
        {
            Sprite sprite = CarregarSpriteBg03(camadas[i]);
            if (sprite == null)
                continue;

            carregouAlgumaCamada = true;
            var camada = new GameObject("Camada_" + camadas[i], typeof(RectTransform));
            camada.transform.SetParent(fundo.transform, false);
            EsticarTela(camada.GetComponent<RectTransform>());

            UIMovingBackgroundLayer scroller = camada.AddComponent<UIMovingBackgroundLayer>();
            scroller.Configurar(sprite, velocidades[i] * multiplicadorVelocidade, Color.white);
        }

        if (!carregouAlgumaCamada)
        {
            GameObject fallback = CriarRetanguloUI("Fundo_Fallback", fundo.transform, new Color(0.52f, 0.25f, 0.1f, 1f));
            EsticarTela(fallback.GetComponent<RectTransform>());
        }

        GameObject overlay = CriarRetanguloUI("Overlay_Aquecido", fundo.transform, new Color(0.1f, 0.045f, 0.015f, 0.24f));
        EsticarTela(overlay.GetComponent<RectTransform>());

        CriarFaixaFundo(fundo.transform, "Sombra_Topo", true, 150f, new Color(0.03f, 0.015f, 0.005f, 0.32f));
        CriarFaixaFundo(fundo.transform, "Sombra_Base", false, 170f, new Color(0.03f, 0.015f, 0.005f, 0.38f));
    }

    private GameObject CriarCartazWestern(string nome, Transform pai, Vector2 tamanho, Vector2 posicao)
    {
        var cartaz = new GameObject(nome, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        cartaz.transform.SetParent(pai, false);

        RectTransform rect = cartaz.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posicao;
        rect.sizeDelta = tamanho;

        Image imagem = cartaz.GetComponent<Image>();
        imagem.sprite = SpritePainelWestern;
        imagem.type = Image.Type.Sliced;
        imagem.color = Color.white;
        imagem.raycastTarget = false;

        Shadow sombra = cartaz.AddComponent<Shadow>();
        sombra.effectColor = new Color(0.04f, 0.015f, 0.005f, 0.75f);
        sombra.effectDistance = new Vector2(0f, -9f);

        return cartaz;
    }

    private void CriarLinhaDecorativa(Transform pai, string nome, Vector2 posicao, Vector2 tamanho, Color cor)
    {
        GameObject linha = CriarRetanguloUI(nome, pai, cor);
        RectTransform rect = linha.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posicao;
        rect.sizeDelta = tamanho;
        linha.GetComponent<Image>().raycastTarget = false;
    }

    private Button CriarBotaoWestern(string nome, Transform pai, string texto, Vector2 posicao, Vector2 tamanho)
    {
        var botaoGO = new GameObject(nome, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        botaoGO.transform.SetParent(pai, false);

        RectTransform rect = botaoGO.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posicao;
        rect.sizeDelta = tamanho;

        Image imagem = botaoGO.GetComponent<Image>();
        imagem.sprite = SpriteBotaoWestern;
        imagem.type = Image.Type.Sliced;
        imagem.color = Color.white;

        Button botao = botaoGO.GetComponent<Button>();
        ColorBlock cores = botao.colors;
        cores.normalColor = Color.white;
        cores.highlightedColor = new Color(1f, 0.88f, 0.58f, 1f);
        cores.pressedColor = new Color(0.72f, 0.42f, 0.16f, 1f);
        cores.selectedColor = cores.highlightedColor;
        cores.disabledColor = new Color(0.42f, 0.28f, 0.16f, 0.55f);
        botao.colors = cores;

        Text textoBotao = CriarTexto("Texto", botaoGO.transform, texto, 24,
            new Color(0.22f, 0.075f, 0.02f, 1f), TextAnchor.MiddleCenter,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        textoBotao.fontStyle = FontStyle.Bold;
        textoBotao.raycastTarget = false;
        AdicionarTextoSombra(textoBotao, new Color(1f, 0.83f, 0.48f, 0.5f), new Vector2(0f, 1f));

        return botao;
    }

    private void CriarFaixaFundo(Transform pai, string nome, bool topo, float altura, Color cor)
    {
        GameObject faixa = CriarRetanguloUI(nome, pai, cor);
        RectTransform rect = faixa.GetComponent<RectTransform>();
        rect.anchorMin = topo ? new Vector2(0f, 1f) : Vector2.zero;
        rect.anchorMax = topo ? Vector2.one : new Vector2(1f, 0f);
        rect.pivot = topo ? new Vector2(0.5f, 1f) : new Vector2(0.5f, 0f);
        rect.offsetMin = topo ? new Vector2(0f, -altura) : Vector2.zero;
        rect.offsetMax = topo ? Vector2.zero : new Vector2(0f, altura);
        faixa.GetComponent<Image>().raycastTarget = false;
    }

    private GameObject CriarRetanguloUI(string nome, Transform pai, Color cor)
    {
        var objeto = new GameObject(nome, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        objeto.transform.SetParent(pai, false);
        Image imagem = objeto.GetComponent<Image>();
        imagem.color = cor;
        imagem.raycastTarget = false;
        return objeto;
    }

    private void EsticarTela(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void AdicionarTextoSombra(Text texto, Color cor, Vector2 distancia)
    {
        if (texto == null) return;
        Shadow sombra = texto.gameObject.AddComponent<Shadow>();
        sombra.effectColor = cor;
        sombra.effectDistance = distancia;
    }

    private void AdicionarTextoContorno(Text texto, Color cor, Vector2 distancia)
    {
        if (texto == null) return;
        Outline contorno = texto.gameObject.AddComponent<Outline>();
        contorno.effectColor = cor;
        contorno.effectDistance = distancia;
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
        ConfigurarBotao(botaoProximaFase,       () => GameManager.Instance?.ProximaFase());
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
    private Sprite SpriteVidaCavaloCheia => spriteVidaCavaloCheiaGerado;
    private Sprite SpriteVidaCavaloVazia => spriteVidaCavaloVaziaGerado;
    private Sprite SpritePainelWestern
    {
        get
        {
            if (spritePainelWesternGerado == null)
                spritePainelWesternGerado = CriarSpriteWestern(false);
            return spritePainelWesternGerado;
        }
    }

    private Sprite SpriteBotaoWestern
    {
        get
        {
            if (spriteBotaoWesternGerado == null)
                spriteBotaoWesternGerado = CriarSpriteWestern(true);
            return spriteBotaoWesternGerado;
        }
    }

    private Sprite SpritePortalVitoria
    {
        get
        {
            if (spritePortalVitoriaGerado == null)
                spritePortalVitoriaGerado = CriarSpritePortalVitoria();
            return spritePortalVitoriaGerado;
        }
    }

    private Sprite SpriteMunicao
    {
        get
        {
            if (spriteMunicao != null)
                return spriteMunicao;
            if (spriteMunicaoGerado == null)
                spriteMunicaoGerado = CriarSpriteMunicao();
            return spriteMunicaoGerado;
        }
    }

    private Sprite CarregarSpriteBg03(string nome)
    {
#if UNITY_EDITOR
        string caminho = "Assets/backgrounds/desert_cartoon/bg_03/" + nome + ".png";
        UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(caminho);
        foreach (UnityEngine.Object asset in assets)
        {
            Sprite sprite = asset as Sprite;
            if (sprite != null)
                return sprite;
        }
#endif
        return null;
    }

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
            if (painelVidaCavalo != null) painelVidaCavalo.SetActive(visivel && mostrarVidasCavalo);
            return;
        }
        Transform painelVidas = iconesVida[0].transform.parent;
        if (painelVidas != null) painelVidas.gameObject.SetActive(visivel);
        if (painelMunicao != null) painelMunicao.SetActive(visivel);
        if (painelVidaCavalo != null) painelVidaCavalo.SetActive(visivel && mostrarVidasCavalo);
    }

    private Transform EncontrarFilho(Transform raiz, string nome)
    {
        if (raiz == null)
            return null;

        if (raiz.name == nome)
            return raiz;

        for (int i = 0; i < raiz.childCount; i++)
        {
            Transform encontrado = EncontrarFilho(raiz.GetChild(i), nome);
            if (encontrado != null)
                return encontrado;
        }

        return null;
    }

    private void ColetarReferenciasPainelMunicao()
    {
        if (iconeMunicao == null)
        {
            Transform t = painelMunicao.transform.Find("Icone_Municao");
            if (t != null) iconeMunicao = t.GetComponent<Image>();
        }
        if (iconeMunicao != null)
        {
            iconeMunicao.sprite = SpriteMunicao;
            iconeMunicao.preserveAspect = true;
            iconeMunicao.raycastTarget = false;
        }
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

    private void GarantirHudCavalo()
    {
        if (painelVidaCavalo != null && iconesVidaCavalo != null && iconesVidaCavalo.Length == vidasMaximasCavalo)
            return;

        Transform painelVidas = null;
        if (iconesVida != null && iconesVida.Length > 0 && iconesVida[0] != null)
            painelVidas = iconesVida[0].transform.parent;

        if (painelVidas == null)
        {
            Transform existente = transform.Find("Painel_Vidas");
            if (existente != null) painelVidas = existente;
        }

        if (painelVidas == null) return;

        Transform painelExistente = painelVidas.Find("Painel_Vida_Cavalo");
        if (painelExistente != null)
        {
            painelVidaCavalo = painelExistente.gameObject;
            iconesVidaCavalo = painelVidaCavalo.GetComponentsInChildren<Image>(true);
            return;
        }

        painelVidaCavalo = new GameObject("Painel_Vida_Cavalo", typeof(RectTransform));
        painelVidaCavalo.transform.SetParent(painelVidas, false);

        RectTransform painelRect = painelVidaCavalo.GetComponent<RectTransform>();
        painelRect.anchorMin = new Vector2(0f, 0.5f);
        painelRect.anchorMax = new Vector2(0f, 0.5f);
        painelRect.pivot = new Vector2(0f, 0.5f);
        painelRect.anchoredPosition = new Vector2(0f, -38f);
        painelRect.sizeDelta = new Vector2(120f, 34f);

        iconesVidaCavalo = new Image[vidasMaximasCavalo];
        for (int i = 0; i < iconesVidaCavalo.Length; i++)
        {
            GameObject iconeGO = new GameObject("Icone_Vida_Cavalo_" + (i + 1), typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconeGO.transform.SetParent(painelVidaCavalo.transform, false);

            RectTransform rect = iconeGO.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = new Vector2(i * 38f, 0f);
            rect.sizeDelta = new Vector2(32f, 32f);

            iconesVidaCavalo[i] = iconeGO.GetComponent<Image>();
        }

        painelVidaCavalo.SetActive(mostrarVidasCavalo);
    }

    public void MostrarFlashDano(float intensidade = 1f)
    {
        ConstruirTelasSeNecessario();
        alphaFlashDano = Mathf.Clamp01(Mathf.Max(alphaFlashDano, corFlashDanoTela.a * intensidade));
        if (flashDano != null)
        {
            flashDano.gameObject.SetActive(true);
            flashDano.transform.SetAsLastSibling();
        }
    }

    private void AtualizarFlashDano()
    {
        if (flashDano == null || alphaFlashDano <= 0f)
            return;

        alphaFlashDano = Mathf.MoveTowards(alphaFlashDano, 0f, 1.8f * Time.unscaledDeltaTime);
        Color cor = corFlashDanoTela;
        cor.a = alphaFlashDano;
        flashDano.color = cor;

        if (alphaFlashDano <= 0f)
            flashDano.gameObject.SetActive(false);
    }

    private void PrepararSpritesVida()
    {
        if (spriteVidaCheia == null && spriteVidaCheiaGerado == null) spriteVidaCheiaGerado = CriarSpriteCoracao(true);
        if (spriteVidaVazia == null && spriteVidaVaziaGerado == null) spriteVidaVaziaGerado = CriarSpriteCoracao(false);
        if (spriteVidaCavaloCheiaGerado == null) spriteVidaCavaloCheiaGerado = CriarSpriteCoracao(true, true);
        if (spriteVidaCavaloVaziaGerado == null) spriteVidaCavaloVaziaGerado = CriarSpriteCoracao(false, true);
    }

    private Sprite CriarSpriteCoracao(bool cheio, bool cavalo = false)
    {
        int largura = MascaraCoracao[0].Length;
        int altura  = MascaraCoracao.Length;
        var textura = new Texture2D(largura, altura, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode   = TextureWrapMode.Clamp,
            name       = cavalo ? (cheio ? "Coracao_Cavalo_Cheio" : "Coracao_Cavalo_Vazio") : (cheio ? "Coracao_Vida_Cheio" : "Coracao_Vida_Vazio")
        };
        for (int y = 0; y < altura; y++)
        {
            string linha = MascaraCoracao[altura - 1 - y];
            for (int x = 0; x < largura; x++)
                textura.SetPixel(x, y, CorDoPixel(linha[x], cheio, cavalo));
        }
        textura.Apply();
        var sprite = Sprite.Create(textura, new Rect(0, 0, largura, altura),
            new Vector2(0.5f, 0.5f), PixelsPorUnidadeCoracao, 0, SpriteMeshType.FullRect);
        sprite.name = textura.name;
        return sprite;
    }

    private Sprite CriarSpriteMunicao()
    {
        const int tamanho = 32;
        var textura = new Texture2D(tamanho, tamanho, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
            name = "Icone_Municao"
        };

        Color transparente = Color.clear;
        for (int y = 0; y < tamanho; y++)
            for (int x = 0; x < tamanho; x++)
                textura.SetPixel(x, y, transparente);

        DesenharCartucho(textura, 9, 23);
        DesenharCartucho(textura, 21, 21);
        DesenharCartucho(textura, 12, 10);
        DesenharCartucho(textura, 24, 8);

        textura.Apply();
        var sprite = Sprite.Create(textura, new Rect(0, 0, tamanho, tamanho),
            new Vector2(0.5f, 0.5f), PixelsPorUnidadeMunicao, 0, SpriteMeshType.FullRect);
        sprite.name = textura.name;
        return sprite;
    }

    private Sprite CriarSpritePortalVitoria()
    {
        const int tamanho = 96;
        float centro = (tamanho - 1) * 0.5f;
        var textura = new Texture2D(tamanho, tamanho, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            name = "UI_Portal_Vitoria"
        };

        for (int y = 0; y < tamanho; y++)
        {
            for (int x = 0; x < tamanho; x++)
            {
                float dx = (x - centro) / centro;
                float dy = (y - centro) / centro;
                float distancia = Mathf.Sqrt(dx * dx + dy * dy);
                float anel = Mathf.Clamp01(1f - Mathf.Abs(distancia - 0.58f) * 8f);
                float nucleo = Mathf.Clamp01(1f - distancia * 1.55f);
                float brilho = Mathf.Max(anel * 0.9f, nucleo * 0.72f);
                float alpha = Mathf.SmoothStep(0f, 1f, brilho) * Mathf.Clamp01(1f - distancia);

                textura.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        textura.Apply();
        Sprite sprite = Sprite.Create(textura, new Rect(0, 0, tamanho, tamanho),
            new Vector2(0.5f, 0.5f), 32f, 0, SpriteMeshType.FullRect);
        sprite.name = textura.name;
        return sprite;
    }

    private Sprite CriarSpriteWestern(bool botao)
    {
        int largura = botao ? 96 : 112;
        int altura = botao ? 36 : 72;
        int borda = botao ? 5 : 7;
        int bordaInterna = botao ? 8 : 11;

        var textura = new Texture2D(largura, altura, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
            name = botao ? "UI_Botao_Western" : "UI_Cartaz_Western"
        };

        Color bordaEscura = botao ? new Color(0.24f, 0.085f, 0.02f, 1f) : new Color(0.16f, 0.065f, 0.018f, 1f);
        Color bordaClara = botao ? new Color(0.86f, 0.47f, 0.15f, 1f) : new Color(0.68f, 0.34f, 0.11f, 1f);
        Color baseMadeira = botao ? new Color(0.72f, 0.37f, 0.12f, 1f) : new Color(0.43f, 0.21f, 0.075f, 0.96f);
        Color brilho = botao ? new Color(0.96f, 0.63f, 0.25f, 1f) : new Color(0.58f, 0.31f, 0.13f, 0.96f);

        for (int y = 0; y < altura; y++)
        {
            for (int x = 0; x < largura; x++)
            {
                bool ehBorda = x < borda || x >= largura - borda || y < borda || y >= altura - borda;
                bool ehLinhaInterna = x == bordaInterna || x == largura - bordaInterna - 1 ||
                    y == bordaInterna || y == altura - bordaInterna - 1;

                Color cor = baseMadeira;
                if (ehBorda)
                    cor = bordaEscura;
                else if (ehLinhaInterna)
                    cor = bordaClara;
                else
                {
                    float ruido = RuidoPixel(x, y) * (botao ? 0.08f : 0.055f);
                    float faixa = ((y / (botao ? 8 : 12)) % 2 == 0) ? 0.035f : -0.02f;
                    cor = Color.Lerp(baseMadeira, brilho, Mathf.Clamp01(0.35f + ruido + faixa));
                }

                textura.SetPixel(x, y, cor);
            }
        }

        textura.Apply();
        Vector4 border = botao ? new Vector4(12f, 12f, 12f, 12f) : new Vector4(18f, 18f, 18f, 18f);
        Sprite sprite = Sprite.Create(textura, new Rect(0, 0, largura, altura), new Vector2(0.5f, 0.5f),
            16f, 0, SpriteMeshType.FullRect, border);
        sprite.name = textura.name;
        return sprite;
    }

    private float RuidoPixel(int x, int y)
    {
        int hash = x * 73856093 ^ y * 19349663;
        hash = (hash << 13) ^ hash;
        return 1f - ((hash * (hash * hash * 15731 + 789221) + 1376312589) & 0x7fffffff) / 1073741824f;
    }

    private void DesenharCartucho(Texture2D textura, int centroX, int centroY)
    {
        Color borda = new Color(0.25f, 0.13f, 0.05f, 1f);
        Color sombra = new Color(0.47f, 0.28f, 0.11f, 1f);
        Color corpo = new Color(0.86f, 0.66f, 0.34f, 1f);
        Color brilho = new Color(1f, 0.88f, 0.56f, 1f);

        for (int y = -6; y <= 6; y++)
        {
            for (int x = -6; x <= 6; x++)
            {
                float distancia = Mathf.Sqrt(x * x + y * y);
                if (distancia > 6.1f)
                    continue;

                Color cor = corpo;
                if (distancia > 4.9f) cor = borda;
                else if (x > 1 || y < -2) cor = sombra;
                else if (x < -1 && y > 1) cor = brilho;

                textura.SetPixel(centroX + x, centroY + y, cor);
            }
        }

        for (int y = -2; y <= 2; y++)
        {
            for (int x = -2; x <= 2; x++)
            {
                if (x * x + y * y > 5)
                    continue;
                textura.SetPixel(centroX + x, centroY + y, new Color(0.36f, 0.2f, 0.08f, 1f));
            }
        }
    }

    private void LimparSpriteGerado(Sprite sprite)
    {
        if (sprite == null) return;
        Texture2D textura = sprite.texture;
        Destroy(sprite);
        Destroy(textura);
    }

    private Color CorDoPixel(char pixel, bool cheio, bool cavalo = false)
    {
        Color borda = cavalo ? corVidaCavaloBorda : corVidaBorda;
        Color vazia = cavalo ? corVidaCavaloVazia : corVidaVazia;
        Color brilho = cavalo ? corVidaCavaloBrilho : corVidaBrilho;
        Color preenchimento = cavalo ? corVidaCavaloCheia : corVidaCheia;

        if (pixel == 'B') return cheio ? borda : vazia;
        if (!cheio) return Color.clear;
        if (pixel == 'P') return brilho;
        if (pixel == 'F') return preenchimento;
        return Color.clear;
    }
}
