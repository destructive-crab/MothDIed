using MothDIed.Audio;
using UnityEngine;

namespace MohDIed.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioInstance : MonoBehaviour
    {
        public AudioSource Source { get; private set; }
        public AudioData AudioData { get; private set; }

        private PitchedAudio _pitchedAudio; //if randomize pitch is true, picthed audio instance will be created and audio will be played from this

        private void Awake()
        {
            Source = GetComponent<AudioSource>();
        }

        //controls of source
        public void Play()
        {
            if (_pitchedAudio != null)
            {
                _pitchedAudio.Play();
            }
            else
            {
                Source.Play();
            }
        }

        public void Stop()
        {
            if (_pitchedAudio != null)
            {
                _pitchedAudio.Stop();
            }
            else
            {
                Source.Stop();
            }
        }

        public void Pause()
        {
            if (_pitchedAudio != null)
            {
                _pitchedAudio.Pause();
            }
            else
            {
                Source.Pause();
            }   
        }
        
        //data control
        
        public void SetData(AudioData data)
        {
            if(data == null)
                return;

            gameObject.name = data.Name;
            AudioData = data;
            Source.clip = data.Clip;
            Source.outputAudioMixerGroup = data.Output;

            if (data.RandomizePitch)
            {
                _pitchedAudio = new PitchedAudio(Source);
            }
            else
            {
                _pitchedAudio = null;
            }

            CommitDataChanges();
        }

        public void CommitDataChanges()
        {
            Source.loop = AudioData.Looped;
            Source.volume = AudioData.Volume;
        }
    }
}