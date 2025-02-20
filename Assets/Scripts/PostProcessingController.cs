using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessingController : MonoBehaviour
{
    public  postProcesInfo postPoinfo_startQuestions, postPoInfo_endQuestion, postPoInfo_MainMenu, postPoInfo_GameOverScreens;
    [Header("Other references")]
    [SerializeField] Volume postprocessVolum;
    ChromaticAberration chromaticAberration;
    Vignette vignette;
    FilmGrain filmGrain;
    [Serializable]
    public class postProcesInfo
    {
        public float chromaticIntensity;
        public float vignetteIntensity;
        public float grainIntensity;

        public static postProcesInfo LerpInfos(postProcesInfo infoA, postProcesInfo infoB, float T)
        {
            postProcesInfo newInfo = new postProcesInfo();
            newInfo.chromaticIntensity = Mathf.Lerp(infoA.chromaticIntensity, infoB.chromaticIntensity, T);
            newInfo.vignetteIntensity = Mathf.Lerp(infoA.vignetteIntensity, infoB.vignetteIntensity, T);
            newInfo.grainIntensity = Mathf.Lerp(infoA.grainIntensity, infoB.grainIntensity, T);
            return newInfo;
        }
    }
    private void Awake()
    {  
        if(postprocessVolum.profile.TryGet(out ChromaticAberration chrom)) { chromaticAberration = chrom; }
        if(postprocessVolum.profile.TryGet(out Vignette vig)) { vignette = vig; }
        if(postprocessVolum.profile.TryGet(out FilmGrain film)) { filmGrain = film; }

        

        SetQuestionsPostProcessing(0, 1);
    }
    public void SetQuestionsPostProcessing(int currentIndex, int maxIndex)
    {
        float normalizedIndex = (float)currentIndex / (float)maxIndex;

        postProcesInfo lerpedInfo = postProcesInfo.LerpInfos(postPoinfo_startQuestions, postPoInfo_endQuestion, normalizedIndex);
        SetPostProcesing(lerpedInfo);
    }
    public void SetPostProcesing(postProcesInfo info)
    {
        chromaticAberration.intensity.value = info.chromaticIntensity;
        vignette.intensity.value = info.vignetteIntensity;
        filmGrain.intensity.value = info.grainIntensity;
    }
}
