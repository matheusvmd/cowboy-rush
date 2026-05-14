using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private const string NomeCenaLevel1 = "Level1";

    [SerializeField] private int vidasIniciais = 3;

    public int Vidas { get; private set; }
    public bool JogoAtivo { get; private set; }
    public bool EstaEmPausa { get; private set; }

    private bool voltarParaMenuAposCarregar;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += AoCarregarCena;
    }

    private void Start()
    {
        MostrarMenuInicial();
    }

    private void Update()
    {
        if (!JogoAtivo) return;
        if (Keyboard.current?.escapeKey.wasPressedThisFrame == true)
        {
            if (EstaEmPausa) RetomarJogo();
            else PausarJogo();
        }
    }

    private void OnDestroy()
    {
        if (Instance != this) return;
        SceneManager.sceneLoaded -= AoCarregarCena;
        Time.timeScale = 1f;
        Instance = null;
    }

    private void AoCarregarCena(Scene cena, LoadSceneMode modo)
    {
        UIManager.Instance?.AtualizarVidas(JogoAtivo ? Vidas : vidasIniciais);

        if (voltarParaMenuAposCarregar || !JogoAtivo)
            MostrarMenuInicial();
        else
            UIManager.Instance?.MostrarJogo();

        voltarParaMenuAposCarregar = false;
    }

    public void MostrarMenuInicial()
    {
        Vidas = vidasIniciais;
        JogoAtivo = false;
        EstaEmPausa = false;
        Time.timeScale = 0f;
        AudioManager.GarantirInstancia()?.TocarMusicaMenu();
        UIManager.Instance?.AtualizarVidas(Vidas);
        UIManager.Instance?.MostrarMenuInicial();
    }

    public void MostrarRegras()
    {
        JogoAtivo = false;
        Time.timeScale = 0f;
        UIManager.Instance?.MostrarRegras();
    }

    public void IniciarJogo()
    {
        Vidas = vidasIniciais;
        JogoAtivo = true;
        EstaEmPausa = false;
        voltarParaMenuAposCarregar = false;
        Time.timeScale = 1f;
        AudioManager.GarantirInstancia()?.PararMusica();
        SceneManager.LoadScene(NomeCenaLevel1);
    }

    public void PausarJogo()
    {
        if (!JogoAtivo || EstaEmPausa) return;
        EstaEmPausa = true;
        Time.timeScale = 0f;
        AudioManager.Instance?.PausarMusica();
        UIManager.Instance?.MostrarPausa();
    }

    public void RetomarJogo()
    {
        if (!EstaEmPausa) return;
        EstaEmPausa = false;
        Time.timeScale = 1f;
        AudioManager.Instance?.RetomarMusica();
        UIManager.Instance?.OcultarPausa();
    }

    public void PerdeuVida()
    {
        if (!JogoAtivo) return;
        Vidas--;
        UIManager.Instance?.AtualizarVidas(Vidas);
        if (Vidas <= 0) GameOver();
    }

    public void GameOver()
    {
        JogoAtivo = false;
        EstaEmPausa = false;
        Time.timeScale = 0f;
        UIManager.Instance?.MostrarGameOver();
    }

    public void Ganhou()
    {
        if (!JogoAtivo) return;
        JogoAtivo = false;
        EstaEmPausa = false;
        Time.timeScale = 0f;
        if (EstaNaUltimaFase())
            UIManager.Instance?.MostrarFimDoJogo();
        else
            UIManager.Instance?.MostrarVitoria();
    }

    private bool EstaNaUltimaFase()
    {
        int buildIndex = SceneManager.GetActiveScene().buildIndex;
        return buildIndex >= 0 && buildIndex >= SceneManager.sceneCountInBuildSettings - 1;
    }

    public void ProximaFase()
    {
        Vidas = vidasIniciais;
        JogoAtivo = true;
        EstaEmPausa = false;
        voltarParaMenuAposCarregar = false;
        Time.timeScale = 1f;
        AudioManager.GarantirInstancia()?.PararMusica();
        int proximoIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (proximoIndex < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(proximoIndex);
        else
            VoltarParaMenuInicial();
    }

    public void JogarNovamente()
    {
        Vidas = vidasIniciais;
        JogoAtivo = true;
        EstaEmPausa = false;
        voltarParaMenuAposCarregar = false;
        Time.timeScale = 1f;
        AudioManager.GarantirInstancia()?.PararMusica();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Reiniciar()
    {
        VoltarParaMenuInicial();
    }

    public void VoltarParaMenuInicial()
    {
        JogoAtivo = false;
        EstaEmPausa = false;
        voltarParaMenuAposCarregar = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
