using UnityEngine;

// Coloque este script num GameObject com SpriteRenderer + Collider2D (isTrigger).
// Os sprites são carregados automaticamente de Assets/Resources/Portal/.
public class FimDeFase : MonoBehaviour
{
    [Header("Animação")]
    [SerializeField] private float fps = 10f;

    private bool chegou;
    private SpriteRenderer sr;
    private Sprite[] frames;
    private float timer;
    private int frameAtual;

    private void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();

        frames = Resources.LoadAll<Sprite>("Portal/Green Portal Sprite Sheet");
        System.Array.Sort(frames, (a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));

        if (frames.Length > 0)
            sr.sprite = frames[0];
    }

    private void Update()
    {
        if (frames == null || frames.Length == 0) return;

        timer += Time.deltaTime;
        if (timer >= 1f / fps)
        {
            timer -= 1f / fps;
            frameAtual = (frameAtual + 1) % frames.Length;
            sr.sprite = frames[frameAtual];
        }
    }

    private void OnTriggerEnter2D(Collider2D outro)
    {
        if (chegou || !outro.CompareTag("Player")) return;
        if (EnemyHealth.BossVivo) return;
        chegou = true;
        AudioManager.Instance?.TocarPortal();
        EfeitosVisuais.SpawnBurstPortal(transform.position);
        GameManager.Instance?.Ganhou();
    }
}
