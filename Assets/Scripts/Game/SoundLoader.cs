using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SceneMusic : MonoBehaviour
{
    [Header("Réglages Audio")]
    [Tooltip("Glisse ton fichier musique (MP3, WAV, OGG) ici")]
    public AudioClip musicClip;
    
    [Range(0f, 1f)]
    public float volume = 0.5f;
    
    [Tooltip("Joue la musique en boucle")]
    public bool loop = true;
    
    [Tooltip("Si coché, la musique continue sans coupure quand tu changes de scène")]
    public bool keepBetweenScenes = false;

    private AudioSource _audioSource;

    void Awake()
    {
        if (keepBetweenScenes)
        {
            DontDestroyOnLoad(gameObject);
        }

        _audioSource = GetComponent<AudioSource>();
        _audioSource.clip = musicClip;
        _audioSource.volume = volume;
        _audioSource.loop = loop;
        _audioSource.playOnAwake = false;
        _audioSource.spatialBlend = 0f;
    }

    void Start()
    {
        if (musicClip != null)
        {
            _audioSource.Play();
        }
    }
}