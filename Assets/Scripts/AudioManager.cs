using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    private AudioSource audioSource;

    [Header("Music Clips")]
    public AudioClip menuMusic;
    public AudioClip gameplayMusic;

    private string currentScene = "";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = GetComponent<AudioSource>();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentScene = scene.name;

        // Grouped scene logic
        if (IsMenuScene(currentScene))
        {
            PlayMusic(menuMusic);
        }
        else if (IsGameplayScene(currentScene))
        {
            PlayMusic(gameplayMusic);
        }
    }

    bool IsMenuScene(string sceneName)
    {
        return sceneName == "Main Menu" || sceneName == "Level Select";
    }

    bool IsGameplayScene(string sceneName)
    {
        return sceneName == "MainGame" || sceneName == "BossLevel";
    }

    void PlayMusic(AudioClip clip)
    {
        if (audioSource.clip == clip) return; // Avoid restarting same music
        audioSource.clip = clip;
        audioSource.loop = true;
        audioSource.Play();
    }
}
