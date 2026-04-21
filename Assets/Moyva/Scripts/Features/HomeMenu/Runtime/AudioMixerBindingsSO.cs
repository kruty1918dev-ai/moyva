using UnityEngine;
using UnityEngine.Audio;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    /// <summary>
    /// Прив'язка логічних аудіоканалів до параметрів <see cref="AudioMixer"/>.
    /// Створюється через меню Assets → Create → Moyva → HomeMenu → Audio Mixer Bindings.
    /// Якщо <see cref="Mixer"/> не призначено, <see cref="AudioSettingsService"/>
    /// зберігатиме значення лише у <see cref="PlayerPrefs"/>.
    /// </summary>
    [CreateAssetMenu(menuName = "Moyva/HomeMenu/Audio Mixer Bindings", fileName = "AudioMixerBindings")]
    public sealed class AudioMixerBindingsSO : ScriptableObject
    {
        [Tooltip("Основний AudioMixer проєкту. Якщо null, значення зберігаються лише у PlayerPrefs.")]
        [SerializeField] private AudioMixer mixer;

        [Header("Імена параметрів AudioMixer (виставляються у Exposed Parameters)")]
        [Tooltip("Параметр майстер-гучності (dB).")]
        [SerializeField] private string masterParameter = "MasterVolume";

        [Tooltip("Параметр гучності музики (dB).")]
        [SerializeField] private string musicParameter  = "MusicVolume";

        [Tooltip("Параметр гучності SFX (dB).")]
        [SerializeField] private string sfxParameter    = "SfxVolume";

        [Tooltip("Параметр гучності UI-звуків (dB).")]
        [SerializeField] private string uiParameter     = "UiVolume";

        [Header("Дефолтні значення (лінійні, 0..1)")]
        [Range(0f, 1f)] [SerializeField] private float defaultMaster = 0.8f;
        [Range(0f, 1f)] [SerializeField] private float defaultMusic  = 0.7f;
        [Range(0f, 1f)] [SerializeField] private float defaultSfx    = 0.9f;
        [Range(0f, 1f)] [SerializeField] private float defaultUi     = 0.9f;

        /// <summary>Основний AudioMixer.</summary>
        public AudioMixer Mixer => mixer;

        /// <summary>Ім'я параметра майстер-гучності.</summary>
        public string MasterParameter => masterParameter;

        /// <summary>Ім'я параметра гучності музики.</summary>
        public string MusicParameter  => musicParameter;

        /// <summary>Ім'я параметра гучності SFX.</summary>
        public string SfxParameter    => sfxParameter;

        /// <summary>Ім'я параметра гучності UI-звуків.</summary>
        public string UiParameter     => uiParameter;

        /// <summary>Дефолтне значення майстер-гучності (лінійне 0..1).</summary>
        public float DefaultMaster => defaultMaster;
        /// <summary>Дефолтне значення гучності музики.</summary>
        public float DefaultMusic  => defaultMusic;
        /// <summary>Дефолтне значення SFX.</summary>
        public float DefaultSfx    => defaultSfx;
        /// <summary>Дефолтне значення UI.</summary>
        public float DefaultUi     => defaultUi;
    }
}
