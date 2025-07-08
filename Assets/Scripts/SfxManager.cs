using UnityEngine;

public class SfxManager : MonoBehaviour
{
    [SerializeField] AudioSource sfxManagerAudioSource;

    public static SfxManager instance;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public void PlaySound(AudioClip audioClip, Transform spawnTransform, float volume)
    {
        if (audioClip == null || spawnTransform == null || sfxManagerAudioSource == null)
        {
            Debug.LogWarning("SfxManager: Cannot play sound - missing audio clip, transform, or audio source");
            return;
        }

        AudioSource audioSource = Instantiate(sfxManagerAudioSource, spawnTransform.position, Quaternion.identity);
        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.Play();

        Destroy(audioSource.gameObject, audioSource.clip.length);
    }

    public AudioSource PlaySoundHandled(AudioClip audioClip, Transform spawnTransform, float volume)
    {
        if (audioClip == null || spawnTransform == null || sfxManagerAudioSource == null)
        {
            Debug.LogWarning("SfxManager: Cannot play sound - missing audio clip, transform, or audio source");
            return null;
        }

        AudioSource audioSource = Instantiate(sfxManagerAudioSource, spawnTransform.position, Quaternion.identity);
        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.Play();

        return audioSource;
    }
}
