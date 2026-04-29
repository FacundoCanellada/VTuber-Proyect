using UnityEngine;

namespace UndertaleEncounter
{
    [CreateAssetMenu(fileName = "NewSoundData", menuName = "Undertale/Sound Data")]
    public class SoundData : ScriptableObject
    {
        public AudioClip clip;
        [Range(0, 1)] public float volume = 1.0f;
        [Range(0.1f, 3)] public float pitch = 1.0f;
    }
}
