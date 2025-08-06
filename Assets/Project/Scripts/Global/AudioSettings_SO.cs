using UnityEngine;

namespace CurseOfNaga.Global
{
    [CreateAssetMenu(fileName = "AudioSettings", menuName = "ScriptableObject/Audio Settings")]
    public class AudioSettings_SO : ScriptableObject
    {
        [Range(-79.5f, 19.5f)] public float MasterVolume;
        [Range(-79.5f, 19.5f)] public float SFXVolume, BGMVolume, AmbientVolume;
    }
}