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

    public musicDistortionStats musicStats_startQuestion, musicStats_endQuestion, musicStats_Billionare, musicStats_MainMenu;
    public musicDistortionStats musicStats_current;

    public float secondsToTransitionToBillionare;
    Coroutine currentTransition;
    [Serializable]
    public struct musicDistortionStats
    {
        public float Pitch;
        public float Volume;
        public float Distortion;
        public float Delay;
        public float Decay;
        public bool hasLowerFilter;

        public static musicDistortionStats CopyStats( musicDistortionStats referenceStats)
        {
            musicDistortionStats targetStats = new musicDistortionStats();

            targetStats.Volume = referenceStats.Volume;
            targetStats.Pitch = referenceStats.Pitch;
            targetStats.Distortion = referenceStats.Distortion;
            targetStats.Decay = referenceStats.Decay;
            targetStats.Delay = referenceStats.Delay;
            targetStats.hasLowerFilter = referenceStats.hasLowerFilter;
            return targetStats;
        }
        public static musicDistortionStats LerpStats(musicDistortionStats statsA, musicDistortionStats statsB, float t)
        {
            musicDistortionStats newStats = new musicDistortionStats();
            newStats.Volume = Mathf.Lerp(statsA.Volume, statsB.Volume, t);
            newStats.Pitch = Mathf.Lerp(statsA.Pitch, statsB.Pitch, t);
            newStats.Distortion = Mathf.Lerp(statsA.Distortion, statsB.Distortion, t);
            newStats.Delay = Mathf.Lerp(statsA.Delay, statsB.Delay, t);
            newStats.Decay = Mathf.Lerp(statsA.Decay, statsB.Decay, t);
            newStats.hasLowerFilter = statsB.hasLowerFilter;

            return newStats;
        }
    }

    public void SetQuestionsDistortion(int currentIndex, int maxIndex)
    {
        float normalizedIndex = (float)currentIndex / (float)maxIndex;
        SetDistortionStats(musicDistortionStats.LerpStats(musicStats_startQuestion, musicStats_endQuestion, normalizedIndex));
    }

    [Header("MainMenu transition")]
    public float seconds_TransitionToStartPlaying;
    public float seconds_TransitionToMainMenu;
    private void Start()
    {
        audioSource.Play();
    }
  
    public void TransitionFromCurrentStats(musicDistortionStats targetStats, float transitionTime)
    {
        musicDistortionStats baseStats = musicDistortionStats.CopyStats(musicStats_current);
        TransitionStats(baseStats, targetStats, transitionTime);
    }
    public void TransitionStats(musicDistortionStats statsA, musicDistortionStats statsB, float transitionTime)
    {
        if(currentTransition != null) { StopCoroutine(currentTransition); }
        currentTransition = StartCoroutine(transitionCoroutine());

        IEnumerator transitionCoroutine()
        {
            float timer = 0;
            while (timer < transitionTime)
            {
                timer += Time.deltaTime;
                SetDistortionStats(musicDistortionStats.LerpStats(statsA, statsB, timer / transitionTime));
                yield return null;
            }
            SetDistortionStats(statsB);
        }
    }
    public void SetDistortionStats(musicDistortionStats stats)
    {
        audioSource.pitch = stats.Pitch;
        audioSource.volume = stats.Volume;
        distortionFilter.distortionLevel = stats.Distortion;
        echoFilter.delay = stats.Delay;
        echoFilter.decayRatio = stats.Decay;
        lowPassFilter.enabled = stats.hasLowerFilter;
        musicStats_current = stats;
    }
    public void AddLowFilter() 
    {
        musicDistortionStats filteredStats =  musicStats_current;
        filteredStats.hasLowerFilter = true;
        SetDistortionStats(filteredStats);
    }
    public void RemoveLowFilter() 
    {
        musicDistortionStats filteredStats = musicStats_current;
        filteredStats.hasLowerFilter = false;
        SetDistortionStats(filteredStats);
    }
}
