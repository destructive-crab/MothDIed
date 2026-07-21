using UnityEngine;

namespace MohDIed.Audio
{
    [CreateAssetMenu(fileName = nameof(AudioContainerPreset), menuName = "MothDIed/Audio")]
    public class AudioContainerPreset : ScriptableObject
    {
        public string PresetName;
        public AudioData[] Audios;
    }
}