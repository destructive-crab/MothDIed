using System;
using UnityEngine;
using UnityEngine.Audio;

namespace MohDIed.Audio
{
    [Serializable]
    public class AudioData
    {
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public AudioClip Clip { get; private set; }
        [field: SerializeField] public AudioMixerGroup Output { get; private set; }
        [field: SerializeField] public bool Looped { get; private set; }
        [field: SerializeField, Range(0, 1)] public float Volume { get; private set; }
        [field: SerializeField] public AudioTags[] Tags { get; private set; }
        [field: SerializeField] public bool RandomizePitch { get; private set; }
    }

    public enum AudioTags
    {
        
    }
}