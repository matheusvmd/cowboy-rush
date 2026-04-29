using UnityEngine;
using UnityEngine.SceneManagement;

public class FimDeFase : MonoBehaviour
{
    [SerializeField] private string proxFase = "";
    [SerializeField] private GameObject painelVitoria;

    private bool chegou = false;

    private void OnTriggerEnter2D(Collider2D outro)
    {
        if (chegou) return;
        if (!outro.CompareTag("Player")) return;

        chegou = true;

        if (painelVitoria != null)
            painelVitoria.SetActive(true);

        if (!string.IsNullOrEmpty(proxFase))
            Invoke(nameof(CarregarProxFase), 2f);
        else
            Invoke(nameof(ReiniciarFase), 3f);
    }

    private void CarregarProxFase()
    {
        SceneManager.LoadScene(proxFase);
    }

    private void ReiniciarFase()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
