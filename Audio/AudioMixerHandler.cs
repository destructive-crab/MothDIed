using UnityEngine;
using UnityEngine.Audio;

namespace banging_code.common
{
    public sealed class AudioMixerHandler
    {
        private const string MASTER_V_KEY = "master_volume";
        private const string MUSIC_V_KEY = "music_volume";
        private const string SOUNDS_V_KEY = "sounds_volume";
        private const string UI_V_KEY = "ui_volume";
        
        private AudioMixer mixer;

        public AudioMixerHandler(AudioMixer mixer)
        {
            this.mixer = mixer;
        }

        public static float ToDecibel(int volume)
        {
            if (volume <= 0) return -80f;
            float normalizedLinear = (float)volume / 100;
            return 20 * Mathf.Log10(normalizedLinear);
        }

        public static int FromDecibel(float db)
        {
            return (int)(Mathf.Pow(10, db / 20) * 100);
        }

        public int GetMasterVolume() => GetVolume(MASTER_V_KEY);
        public int GetMusicVolume() => GetVolume(MUSIC_V_KEY);
        public int GetSoundsVolume() => GetVolume(SOUNDS_V_KEY);
        public int GetUIVolume() => GetVolume(UI_V_KEY);

        public void SetVolumeForMaster(int volume) => mixer.SetFloat(MASTER_V_KEY, ToDecibel(volume));

        public void SetVolumeForMusic(int volume) => mixer.SetFloat(MUSIC_V_KEY, ToDecibel(volume));

        public void SetVolumeForSounds(int volume) => mixer.SetFloat(SOUNDS_V_KEY, ToDecibel(volume));

        public void SetVolumeForUI(int volume) => mixer.SetFloat(UI_V_KEY, ToDecibel(volume));

        private int GetVolume(string parameter)
        {
            if (mixer.GetFloat(parameter, out float value))
            {
                return FromDecibel(value);
            }
            return 0;
        }
    }
}