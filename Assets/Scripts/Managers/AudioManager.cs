using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("SFX")]
    [SerializeField] private AudioClip sfxTiro;
    [SerializeField] private AudioClip sfxPulo;
    [SerializeField] private AudioClip sfxDanoPlayer;
    [SerializeField] private AudioClip sfxMorteInimigo;
    [SerializeField] private AudioClip sfxMontar;
    [SerializeField] private AudioClip sfxPortal;

    [Header("Música")]
    [SerializeField] private AudioClip musicaTrilha;
    [SerializeField] [Range(0f, 1f)] private float volumeMusica = 0.45f;
    [SerializeField] [Range(0f, 1f)] private float volumeSFX = 0.9f;

    private AudioSource sourceMusica;
    private AudioSource sourceSFX;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        sourceMusica = gameObject.AddComponent<AudioSource>();
        sourceMusica.loop = true;
        sourceMusica.volume = volumeMusica;
        sourceMusica.playOnAwake = false;

        sourceSFX = gameObject.AddComponent<AudioSource>();
        sourceSFX.loop = false;
        sourceSFX.volume = volumeSFX;
        sourceSFX.playOnAwake = false;
    }

    private void Start()
    {
        if (musicaTrilha != null)
        {
            sourceMusica.clip = musicaTrilha;
            sourceMusica.Play();
        }
    }

    public void TocarSFX(AudioClip clip)
    {
        if (clip == null || sourceSFX == null) return;
        sourceSFX.PlayOneShot(clip, volumeSFX);
    }

    public void TocarTiro()        => TocarSFX(sfxTiro);
    public void TocarPulo()        => TocarSFX(sfxPulo);
    public void TocarDano()        => TocarSFX(sfxDanoPlayer);
    public void TocarMorteInimigo() => TocarSFX(sfxMorteInimigo);
    public void TocarMontar()      => TocarSFX(sfxMontar);
    public void TocarPortal()      => TocarSFX(sfxPortal);

    public void PausarMusica()  { if (sourceMusica != null) sourceMusica.Pause(); }
    public void RetomarMusica() { if (sourceMusica != null) sourceMusica.UnPause(); }
}
