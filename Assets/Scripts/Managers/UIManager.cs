using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private Image[] iconesVida;
    [SerializeField] private GameObject painelGameOver;
    [SerializeField] private Sprite spriteVidaCheia;
    [SerializeField] private Sprite spriteVidaVazia;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void AtualizarVidas(int vidas)
    {
        for (int i = 0; i < iconesVida.Length; i++)
        {
            if (iconesVida[i] == null) continue;
            iconesVida[i].sprite = i < vidas ? spriteVidaCheia : spriteVidaVazia;
        }
    }

    public void MostrarGameOver()
    {
        if (painelGameOver != null)
            painelGameOver.SetActive(true);
    }
}
