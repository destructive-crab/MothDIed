using UnityEngine;

namespace MothDIed.Audio
{
    [System.Serializable]
    public sealed class PitchedAudio
    {
        [field: SerializeField] public AudioSource AudioSource { get; private set; }

        public float minPitch = 0.8f;
        public float maxPitch = 1.2f;

        public PitchedAudio(AudioSource audioSource, float minPitch = 0.8f, float maxPitch = 1.2f)
        {
            AudioSource = audioSource;
            
            this.minPitch = minPitch;
            this.maxPitch = maxPitch;
        }

        public void Play()
        {
            AudioSource.pitch = Random.Range(minPitch, maxPitch);
            AudioSource.Play();
        }
        public void Stop() => AudioSource.Stop();
        public void Pause() => AudioSource.Pause();
    }
}