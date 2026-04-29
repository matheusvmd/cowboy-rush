using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private int vidasIniciais = 3;

    public int Vidas { get; private set; }
    public bool JogoAtivo { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        IniciarJogo();
    }

    public void IniciarJogo()
    {
        Vidas = vidasIniciais;
        JogoAtivo = true;
        UIManager.Instance?.AtualizarVidas(Vidas);
    }

    public void PerdeuVida()
    {
        Vidas--;
        UIManager.Instance?.AtualizarVidas(Vidas);

        if (Vidas <= 0)
            GameOver();
    }

    public void GameOver()
    {
        JogoAtivo = false;
        UIManager.Instance?.MostrarGameOver();
    }

    public void Reiniciar()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
