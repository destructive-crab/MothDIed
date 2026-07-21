using System;
using System.Collections;
using UnityEngine;

public class AudioSourceDeco : MonoBehaviour
{
    public AudioSource Source;
    
    public bool IsPitched;
    
    public float MinPitch = 0.9f; 
    public float MaxPitch = 1.1f;

    public float DefaultVolume = 1;
    public bool SmoothStop = false;
    public bool SmoothPlay = false;

    private Coroutine currentSmooth;
    
    private void Reset()
    {
        if (TryGetComponent(out AudioSource source))
        {
            Source = source;
        }
    }

    public void Play()
    {
        if (IsPitched)
        {
            Source.pitch = UnityEngine.Random.Range(MinPitch, MaxPitch);
        }

        if(currentSmooth != null) StopCoroutine(currentSmooth);
        Source.volume = DefaultVolume;
        Source.Play();
    }

    public void Stop()
    {
        if (SmoothStop)
        {
            if(currentSmooth != null)
            {
                Source.volume = DefaultVolume;
                StopCoroutine(currentSmooth);
            }

            currentSmooth = StartCoroutine(SmoothMoveVolumeTo(0));
        }
        else
        {
            Source.Stop();
        }
    }

    public void Pause()
    {
        Source.Pause();
    }

    private IEnumerator SmoothMoveVolumeTo(float volume)
    {
        var startVolume = Source.volume;
        while (Math.Abs(Source.volume - volume) > 0.01f)
        {
            Source.volume = Mathf.MoveTowards(Source.volume, volume, 0.05f);
            yield return new WaitForSeconds(0.05f);
        }
        Source.Stop();
        volume = startVolume;
    }
}
