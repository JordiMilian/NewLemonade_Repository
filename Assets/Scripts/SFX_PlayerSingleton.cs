
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SFX_PlayerSingleton : MonoBehaviour
{
    List<AudioSource> audioSourcePool = new List<AudioSource>();
    [SerializeField] private int initialPoolSize = 5;

    public static SFX_PlayerSingleton Instance;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
        InitializeInitialAudioSourcePool();

        //
        void InitializeInitialAudioSourcePool()
        {
            for (int i = 0; i < initialPoolSize; i++)
            {
                CreateNewAudioSource();
            }
        }
    }

    AudioSource CreateNewAudioSource()
    {
        AudioSource newAudioSource = gameObject.AddComponent<AudioSource>();
        audioSourcePool.Add(newAudioSource);
        return newAudioSource;
    }
   
    public void playSFX(AudioClip clip, float pitchVariationAdder = 0, float addedVolum = 0, float addedPitch = 0)//added volum should be a percent probably
    {
        AudioSource audioSource = GetAvailableAudioSource();

        if (clip == null) { Debug.LogWarning("Missing audio clip: " + clip.name); return; }
        audioSource.pitch = 1;
        SetGlobalVolume(ref audioSource);

        float randomAdder = Random.Range(-pitchVariationAdder, pitchVariationAdder);
        audioSource.pitch += randomAdder;
        audioSource.pitch += addedPitch;
        audioSource.volume += addedVolum;

        audioSource.clip = clip;
        audioSource.Play();


        //
        AudioSource GetAvailableAudioSource()
        {
            foreach (var source in audioSourcePool)
            {
                if (!source.isPlaying) // Si no está reproduciendo, está disponible
                {
                    return source;
                }
            }

            return CreateNewAudioSource();
        }
        void SetGlobalVolume(ref AudioSource audioSource)
        {

        }
    }
    
}
