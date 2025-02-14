using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicLoopDistorder : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioDistortionFilter distortionFilter;
    [SerializeField] AudioEchoFilter echoFilter;
    [SerializeField] AudioLowPassFilter lowPassFilter;

    [SerializeField] musicDistortionStats StartGameStats, EndGameStats, BillionareStats;

    [SerializeField] float secondsToTransitionToBillionare;
    [Serializable]
    struct musicDistortionStats
    {
        public float Pitch;
        public float Volume;
        public float Distortion;
        public float Delay;
        public float Decay;
    }
    public void AddLowFilter() { lowPassFilter.enabled = true; }

    public void RemoveLowFilter() { lowPassFilter.enabled = false; }

    public void SetNormalizedDistortion(int currentIndex, int maxIndex)
    {
        float normalizedIndex = (float)currentIndex / (float)maxIndex;

        SetDistortionStats(LerpStats(StartGameStats, EndGameStats, normalizedIndex));
    }
    public void startBillionareDistortion()
    {
        StartCoroutine(billionareMusicTransition());
    }
    IEnumerator billionareMusicTransition()
    {
        AddLowFilter();
        float timer = 0;

        while (timer < secondsToTransitionToBillionare)
        {
            timer += Time.deltaTime;
            float normalizedIndex = timer/ secondsToTransitionToBillionare;

            SetDistortionStats(LerpStats(EndGameStats, BillionareStats, normalizedIndex));

            yield return null;
        }
    }
    
    [Header("MainMenu transition")]
    [SerializeField] musicDistortionStats mainMenuStats;
    [SerializeField] float seconds_MainMenuTransition;
    private void Start()
    {
        AddLowFilter();
        SetDistortionStats(mainMenuStats);
        audioSource.Play();
    }
    public void startRegularMusicTransition()
    {
        StartCoroutine(startplayingTransition());
    }
    IEnumerator startplayingTransition()
    {
        RemoveLowFilter();
        musicDistortionStats baseStats = mainMenuStats;
        float timer = 0;
        while (timer < seconds_MainMenuTransition)
        {
            timer += Time.deltaTime;
            float normalizedTime = timer / seconds_MainMenuTransition;

            SetDistortionStats(LerpStats(baseStats, StartGameStats, normalizedTime));
            yield return null;
        }
        
    }
    void SetDistortionStats(musicDistortionStats stats)
    {
        audioSource.pitch = stats.Pitch;
        audioSource.volume = stats.Volume;
        distortionFilter.distortionLevel = stats.Distortion;
        echoFilter.delay = stats.Delay;
        echoFilter.decayRatio = stats.Decay;
    }
    musicDistortionStats LerpStats(musicDistortionStats statsA, musicDistortionStats statsB, float t)
    {
        musicDistortionStats newStats = new musicDistortionStats();
        newStats.Volume = Mathf.Lerp(statsA.Volume, statsB.Volume, t);
        newStats.Pitch = Mathf.Lerp(statsA.Pitch, statsB.Pitch, t);
        newStats.Distortion = Mathf.Lerp(statsA.Distortion, statsB.Distortion, t);
        newStats.Delay = Mathf.Lerp(statsA.Delay, statsB.Delay, t);
        newStats.Decay = Mathf.Lerp(statsA.Decay, statsB.Decay, t);

        return newStats;
    }
}
