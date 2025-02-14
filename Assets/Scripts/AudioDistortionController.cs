using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioDistortionController : MonoBehaviour
{
    [SerializeField] float start_distortion, end_distortion, start_echoDecay, end_echoDecay;
    AudioDistortionFilter distortionController;
    AudioEchoFilter echoFilter;
    private void Start()
    {
        distortionController = GetComponent<AudioDistortionFilter>();
        echoFilter = GetComponent<AudioEchoFilter>();
    }
    public void SetAudioDistortion(int currentIndex, int maxIndex)
    {
        float normalizedIndex = (float)currentIndex / (float)maxIndex;
        distortionController.distortionLevel = Mathf.Lerp(start_distortion,end_distortion, normalizedIndex);
        echoFilter.decayRatio = Mathf.Lerp(start_echoDecay,end_echoDecay, normalizedIndex);
    }
}
