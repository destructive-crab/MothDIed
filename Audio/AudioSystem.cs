using banging_code.common;
using MothDIed;
using MothDIed.Audio;
using MothDIed.Debug;
using UnityEngine;

namespace MohDIed.Audio
{
    public sealed class AudioSystem
    {
        public AudioMixerHandler AudioMixerHandler { get; private set; }
        private AudioContainer GlobalContainer;

        private static AudioSystem instance;
        
        //TODO: implement temp containers for scenes etc
       // private readonly List<AudioContainer> TempContainers;
       // private AudioContainer CreateNewContainer(AudioContainerPreset preset)
       // {
       //     var containerGameObject = new GameObject($"[AUDIO CONTAINER - {preset.PresetName}]");
       //     return containerGameObject.AddComponent<AudioContainer>();
       // }

       public AudioSystem()
       {
           if (instance != null)
           {
#if UNITY_EDITOR
               LogHistory.PushAsError("MULTIPLE INSTANCES OF AUDIO SYSTEM DETECTED");
#endif
               return;
           }
           instance = this;
       }

       public void Setup(AudioSystemConfig config)
        {
            AudioMixerHandler = new AudioMixerHandler(config.MasterMixer);
            GlobalContainer = CreateGlobalFromPreset(config.GlobalPreset);
            
            AudioMixerHandler.SetVolumeForMaster(config.MasterVolume);
            AudioMixerHandler.SetVolumeForMusic(config.MusicVolume);
            AudioMixerHandler.SetVolumeForSounds(config.SoundsVolume);
            AudioMixerHandler.SetVolumeForUI(config.UIVolume);
        }

        private AudioContainer CreateGlobalFromPreset(AudioContainerPreset preset)
        {
            if (GlobalContainer != null)
            {
#if UNITY_EDITOR 
                Debug.LogError("[AUDIO SYSTEM : CREATE GLOBAL CONTAINER FROM PRESET] GLOBAL PRESET IS ALREADY INITIALIZED");
#endif  
                return GlobalContainer;
            }

            var globalContainer = new GameObject($"[GLOBAL AUDIO CONTAINER - {preset.PresetName}]").AddComponent<AudioContainer>();
            Game.G<SceneSwitcher>().MoveToPersistentScene(globalContainer.gameObject);
            
            foreach (var audioData in preset.Audios)
            {
                globalContainer.Push(audioData);
            }

            return globalContainer;
        }

        public void Play(string audioName)
        {
            if (GlobalContainer.TryGet(audioName, out AudioInstance instance))
            {
                instance.Play();
                return;
            }
#if UNITY_EDITOR
            Debug.LogError($"NO AUDIO WITH NAME {audioName}");    
#endif    
        }

        public void Stop(string audioName)
        {
            if (GlobalContainer.TryGet(audioName, out AudioInstance instance))
            {
                instance.Stop();
                return;
            }
#if UNITY_EDITOR
            Debug.LogError($"NO AUDIO WITH NAME {audioName}");    
#endif    
        }
        
        public void Pause(string audioName)
        {
            if (GlobalContainer.TryGet(audioName, out AudioInstance instance))
            {
                instance.Pause();
                return;
            }
#if UNITY_EDITOR
            Debug.LogError($"NO AUDIO WITH NAME {audioName}");    
#endif    
        }

        public bool IsPlaying(string audioName)
        {
            if (GlobalContainer.TryGet(audioName, out AudioInstance instance))
            {
                return instance.Source.isPlaying;
            }
#if UNITY_EDITOR
            Debug.LogError($"NO AUDIO WITH NAME {audioName}");    
#endif
            return false;
        }

        public float GetLength(string audioName)
        {
            if (GlobalContainer.TryGet(audioName, out AudioInstance instance))
            {
                return instance.AudioData.Clip.length;
            }
#if UNITY_EDITOR
            Debug.LogError($"NO AUDIO WITH NAME {audioName}");    
#endif
            return 0;
        }

        public void StopAllNonCrossScene()
        {
            GlobalContainer.ForEach((audio) => audio.Source.Stop());
        }
    }
}