using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    private const string TiroPadraoPath = "Assets/tiro.mp3";
    private const string RecargaPadraoPath = "Assets/reload.mp3";
    private const string DanoPadraoPath = "Assets/damage.mp3";
    private const string VitoriaPadraoPath = "Assets/victory.mp3";

    [Header("SFX")]
    [SerializeField] private AudioClip sfxTiro;
    [SerializeField] private AudioClip sfxRecarga;
    [SerializeField] private AudioClip sfxPulo;
    [SerializeField] private AudioClip sfxDanoPlayer;
    [SerializeField] private AudioClip sfxMorteInimigo;
    [SerializeField] private AudioClip sfxMontar;
    [SerializeField] private AudioClip sfxPortal;
    [SerializeField, Min(0.05f)] private float duracaoMaximaTiro = 1f;

    [Header("Música")]
    [SerializeField] private AudioClip musicaTrilha;
    [SerializeField] [Range(0f, 1f)] private float volumeMusica = 0.45f;
    [SerializeField] [Range(0f, 1f)] private float volumeSFX = 0.9f;

    private AudioSource sourceMusica;
    private AudioSource sourceSFX;
    private AudioSource sourceTiro;
    private int tokenTiro;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void GarantirInstanciaNaCena()
    {
        if (Instance != null || UnityEngine.Object.FindAnyObjectByType<AudioManager>() != null)
            return;

        new GameObject("AudioManager").AddComponent<AudioManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        CarregarClipsPadraoNoEditor();

        sourceMusica = gameObject.AddComponent<AudioSource>();
        sourceMusica.loop = true;
        sourceMusica.volume = volumeMusica;
        sourceMusica.playOnAwake = false;
        sourceMusica.spatialBlend = 0f;

        sourceSFX = gameObject.AddComponent<AudioSource>();
        sourceSFX.loop = false;
        sourceSFX.volume = volumeSFX;
        sourceSFX.playOnAwake = false;
        sourceSFX.spatialBlend = 0f;

        sourceTiro = gameObject.AddComponent<AudioSource>();
        sourceTiro.loop = false;
        sourceTiro.volume = volumeSFX;
        sourceTiro.playOnAwake = false;
        sourceTiro.spatialBlend = 0f;
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

    public void TocarTiro()
    {
        if (sfxTiro == null || sourceTiro == null) return;

        tokenTiro++;
        sourceTiro.Stop();
        sourceTiro.clip = sfxTiro;
        sourceTiro.time = 0f;
        sourceTiro.volume = volumeSFX;
        sourceTiro.Play();

        if (duracaoMaximaTiro > 0f && sfxTiro.length > duracaoMaximaTiro)
            StartCoroutine(PararTiroDepois(tokenTiro, duracaoMaximaTiro));
    }

    public void TocarRecarga()     => TocarSFX(sfxRecarga);
    public void TocarPulo()        => TocarSFX(sfxPulo);
    public void TocarDano()        => TocarSFX(sfxDanoPlayer);
    public void TocarMorteInimigo() => TocarSFX(sfxMorteInimigo);
    public void TocarMontar()      => TocarSFX(sfxMontar);
    public void TocarPortal()      => TocarSFX(sfxPortal);

    public void PausarMusica()  { if (sourceMusica != null) sourceMusica.Pause(); }
    public void RetomarMusica() { if (sourceMusica != null) sourceMusica.UnPause(); }

    private IEnumerator PararTiroDepois(int token, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        if (token == tokenTiro && sourceTiro != null && sourceTiro.isPlaying)
            sourceTiro.Stop();
    }

    private void CarregarClipsPadraoNoEditor()
    {
#if UNITY_EDITOR
        if (sfxTiro == null)
            sfxTiro = AssetDatabase.LoadAssetAtPath<AudioClip>(TiroPadraoPath);

        if (sfxRecarga == null)
            sfxRecarga = AssetDatabase.LoadAssetAtPath<AudioClip>(RecargaPadraoPath);

        if (sfxDanoPlayer == null)
            sfxDanoPlayer = AssetDatabase.LoadAssetAtPath<AudioClip>(DanoPadraoPath);

        if (sfxPortal == null)
            sfxPortal = AssetDatabase.LoadAssetAtPath<AudioClip>(VitoriaPadraoPath);
#endif
    }
}
