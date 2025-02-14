using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessingController : MonoBehaviour
{
    [SerializeField] float start_chromaticIntensity, end_chromaticIntensity, start_vignetteIntensity, end_vignetteIntensity;
    [Header("Other references")]
    [SerializeField] Volume postprocessVolum;
    [SerializeField] bool ignoreEverything;
    private void Awake()
    {  
        SetPostProcessing(0, 1);
    }
    public void SetPostProcessing(int currentIndex, int maxIndex)
    {
        if (ignoreEverything) { return; }
        float normalizedIndex = (float)currentIndex / (float)maxIndex;

        if (postprocessVolum.profile.TryGet(out ChromaticAberration chromaticAberation))
        {
            chromaticAberation.intensity.value = Mathf.Lerp(start_chromaticIntensity, end_chromaticIntensity, normalizedIndex);
            Debug.Log("Chromatic aberration intensity: "+chromaticAberation.intensity); 
        }
        if(postprocessVolum.profile.TryGet(out Vignette vignette))
        {
            vignette.intensity.value = Mathf.Lerp(start_vignetteIntensity, end_vignetteIntensity, normalizedIndex);
            Debug.Log("Vignette intensity: " + vignette.intensity);
        }
    }
    public void AddFilmGrain()
    {
        if (postprocessVolum.profile.TryGet(out FilmGrain filmGrain))
        {
            filmGrain.active = true;
        }
    }
    public void RemoveFilGrain()
    {
        if (postprocessVolum.profile.TryGet(out FilmGrain filmGrain))
        {
            filmGrain.active = false;
        }
    }
}
