using UnityEngine;
using UnityEngine.Audio;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    /// <summary>
    /// ScriptableObject зі зв'язуванням параметрів AudioMixer для локальних налаштувань меню.
    /// Залежності: використовується LocalGameSettingsService і GameSettingsPanelService.
    /// </summary>
    [CreateAssetMenu(fileName = "AudioMixerBindings", menuName = "Moyva/Home Menu/Audio Mixer Bindings")]
    public sealed class AudioMixerBindingsSO : ScriptableObject
    {
        /// <summary>Посилання на Unity AudioMixer.</summary>
        public AudioMixer mixer;

        /// <summary>Назва параметра master-гучності в мікшері.</summary>
        public string masterParameter = "MasterVolume";

        /// <summary>Назва параметра музики в мікшері.</summary>
        public string musicParameter = "MusicVolume";

        /// <summary>Назва параметра SFX в мікшері.</summary>
        public string sfxParameter = "SfxVolume";

        /// <summary>Назва параметра UI-звуків у мікшері.</summary>
        public string uiParameter = "UiVolume";

        /// <summary>Значення master-гучності за замовчуванням.</summary>
        public float defaultMaster = 0.8f;

        /// <summary>Значення гучності музики за замовчуванням.</summary>
        public float defaultMusic = 0.7f;

        /// <summary>Значення гучності SFX за замовчуванням.</summary>
        public float defaultSfx = 0.9f;

        /// <summary>Значення гучності UI за замовчуванням.</summary>
        public float defaultUi = 0.9f;
    }
}