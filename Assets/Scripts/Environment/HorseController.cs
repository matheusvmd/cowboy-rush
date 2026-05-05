using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HorseController : MonoBehaviour
{
    [SerializeField] private float offsetYNoPlayer = -0.5f;

    private bool montado;
    private PlayerHealth playerHealth;

    private void OnTriggerEnter2D(Collider2D outro)
    {
        if (montado || !outro.CompareTag("Player")) return;
        Montar(outro.gameObject);
    }

    private void Montar(GameObject player)
    {
        montado = true;
        playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
            playerHealth.DefinirCavalo(this);

        AudioManager.Instance?.TocarMontar();

        transform.SetParent(player.transform);
        transform.localPosition = new Vector3(0f, offsetYNoPlayer, 0f);

        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;
    }

    public void AbsorverDano()
    {
        if (playerHealth != null)
            playerHealth.DefinirCavalo(null);

        EfeitosVisuais.SpawnBurst(transform.position, new Color(0.75f, 0.5f, 0.2f), 12, 4f, 0.5f);
        Destroy(gameObject);
    }
}
