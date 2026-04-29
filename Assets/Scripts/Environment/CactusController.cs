using UnityEngine;

public class CactusController : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D outro)
    {
        if (!outro.CompareTag("Player")) return;

        PlayerHealth saude = outro.GetComponent<PlayerHealth>();
        if (saude != null)
            saude.ReceberDano();
    }
}
