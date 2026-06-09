using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    private AudioSource musicSource;
    private AudioSource sfxSource;
    private AudioClip buttonSfx;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        CreateSources();
        ApplySettings();
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip == null || musicSource == null)
            return;

        if (musicSource.clip == clip && musicSource.isPlaying)
            return;

        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }

    public void PlaySfx(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null || sfxSource == null)
            return;

        sfxSource.PlayOneShot(clip, Mathf.Clamp01(volumeScale));
    }

    public void PlayButtonClick()
    {
        buttonSfx ??= Resources.Load<AudioClip>(GameAssetKeys.ButtonSfx);
        PlaySfx(buttonSfx, 0.75f);
    }

    public void ApplySettings()
    {
        bool soundEnabled =
            GameSettingsManager.Instance == null ||
            GameSettingsManager.Instance.SoundEnabled;
        float volume = soundEnabled ? 1f : 0f;

        if (musicSource != null)
            musicSource.volume = volume;
        if (sfxSource != null)
            sfxSource.volume = volume;
    }

    private void CreateSources()
    {
        musicSource = GetOrAddSource("MusicSource", true);
        sfxSource = GetOrAddSource("SfxSource", false);
    }

    private AudioSource GetOrAddSource(string childName, bool loop)
    {
        Transform child = transform.Find(childName);
        GameObject sourceObject =
            child != null ? child.gameObject : new GameObject(childName);
        sourceObject.transform.SetParent(transform, false);

        AudioSource source = sourceObject.GetComponent<AudioSource>();
        if (source == null)
            source = sourceObject.AddComponent<AudioSource>();

        source.playOnAwake = false;
        source.loop = loop;
        return source;
    }
}
