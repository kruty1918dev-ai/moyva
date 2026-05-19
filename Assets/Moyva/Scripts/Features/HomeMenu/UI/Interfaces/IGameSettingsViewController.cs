using System;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.Shared.Graphics;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    /// <summary>
    /// Контракт UI-контролера панелі ігрових/графічних налаштувань.
    /// Залежності: використовується GameSettingsPanelService.
    /// </summary>
    public interface IGameSettingsViewController
    {
        /// <summary>Ім'я гравця.</summary>
        string PlayerName { get; set; }
        /// <summary>Master-гучність.</summary>
        float MasterVolume { get; set; }
        /// <summary>Гучність музики.</summary>
        float MusicVolume { get; set; }
        /// <summary>Гучність SFX.</summary>
        float SfxVolume { get; set; }
        /// <summary>Гучність UI.</summary>
        float UiVolume { get; set; }
        /// <summary>Прапорець mute.</summary>
        bool IsMuted { get; set; }
        /// <summary>Профіль якості графіки.</summary>
        GraphicsQualityProfile GraphicsProfile { get; set; }
        /// <summary>Цільовий FPS.</summary>
        int TargetFrameRate { get; set; }
        /// <summary>Render scale.</summary>
        float RenderScale { get; set; }
        /// <summary>Прапорець dynamic render scale.</summary>
        bool DynamicRenderScale { get; set; }
        /// <summary>Оптимізація close-zoom.</summary>
        bool CloseZoomOptimization { get; set; }
        /// <summary>Ліміт mipmap.</summary>
        int TextureMipmapLimit { get; set; }
        /// <summary>Рівень anti-aliasing.</summary>
        int AntiAliasing { get; set; }
        /// <summary>Прапорець VSync.</summary>
        bool VSync { get; set; }
        /// <summary>Прапорець тіней.</summary>
        bool Shadows { get; set; }
        /// <summary>Прапорець anisotropic filtering.</summary>
        bool AnisotropicFiltering { get; set; }
        /// <summary>LOD bias.</summary>
        float LodBias { get; set; }

        /// <summary>Подія зміни імені гравця.</summary>
        event Action<string> OnPlayerNameChanged;
        /// <summary>Подія зміни master-гучності.</summary>
        event Action<float> OnMasterVolumeChanged;
        /// <summary>Подія зміни гучності музики.</summary>
        event Action<float> OnMusicVolumeChanged;
        /// <summary>Подія зміни гучності SFX.</summary>
        event Action<float> OnSfxVolumeChanged;
        /// <summary>Подія зміни гучності UI.</summary>
        event Action<float> OnUiVolumeChanged;
        /// <summary>Подія зміни mute.</summary>
        event Action<bool> OnMutedChanged;
        /// <summary>Подія зміни графічного профілю.</summary>
        event Action<GraphicsQualityProfile> OnGraphicsProfileChanged;
        /// <summary>Подія зміни target FPS.</summary>
        event Action<int> OnTargetFrameRateChanged;
        /// <summary>Подія зміни render scale.</summary>
        event Action<float> OnRenderScaleChanged;
        /// <summary>Подія зміни dynamic render scale.</summary>
        event Action<bool> OnDynamicRenderScaleChanged;
        /// <summary>Подія зміни оптимізації close-zoom.</summary>
        event Action<bool> OnCloseZoomOptimizationChanged;
        /// <summary>Подія зміни texture mipmap limit.</summary>
        event Action<int> OnTextureMipmapLimitChanged;
        /// <summary>Подія зміни anti-aliasing.</summary>
        event Action<int> OnAntiAliasingChanged;
        /// <summary>Подія зміни VSync.</summary>
        event Action<bool> OnVSyncChanged;
        /// <summary>Подія зміни тіней.</summary>
        event Action<bool> OnShadowsChanged;
        /// <summary>Подія зміни anisotropic filtering.</summary>
        event Action<bool> OnAnisotropicFilteringChanged;
        /// <summary>Подія зміни LOD bias.</summary>
        event Action<float> OnLodBiasChanged;
        /// <summary>Подія натискання reset графіки.</summary>
        event Action OnResetGraphicsClicked;
        /// <summary>Подія натискання delete saves.</summary>
        event Action OnDeleteSavesClicked;

        /// <summary>Оновити UI загальних локальних налаштувань.</summary>
        void Refresh(LocalGameSettings settings);
        /// <summary>Оновити UI графічних налаштувань.</summary>
        void RefreshGraphics(GraphicsSettingsData settings);
        /// <summary>Увімкнути/вимкнути всю панель.</summary>
        void SetInteractable(bool interactable);
    }
}
