using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class GameMusic : MonoBehaviour
{
    private static GameMusic instance;

    void Awake()
    {
        // Ensure only one persistent music object exists
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        var audio = GetComponent<AudioSource>();
        if (audio != null)
        {
            audio.loop = true;
            if (!audio.isPlaying)
            {
                audio.Play();
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Nothing needed; handled in Awake
    }

    // Update is called once per frame
    void Update()
    {
        // No per-frame logic required for looping music
    }
}
