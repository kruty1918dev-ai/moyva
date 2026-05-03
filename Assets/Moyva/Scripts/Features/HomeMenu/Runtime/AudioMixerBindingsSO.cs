using UnityEngine;
using UnityEngine.Audio;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    [CreateAssetMenu(fileName = "AudioMixerBindings", menuName = "Moyva/Home Menu/Audio Mixer Bindings")]
    public sealed class AudioMixerBindingsSO : ScriptableObject
    {
        public AudioMixer mixer;
        public string masterParameter = "MasterVolume";
        public string musicParameter = "MusicVolume";
        public string sfxParameter = "SfxVolume";
        public string uiParameter = "UiVolume";
        public float defaultMaster = 0.8f;
        public float defaultMusic = 0.7f;
        public float defaultSfx = 0.9f;
        public float defaultUi = 0.9f;
    }
}