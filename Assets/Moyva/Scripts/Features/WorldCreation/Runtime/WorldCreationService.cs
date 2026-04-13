using System;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.WorldCreation.API;
using UnityEngine;

namespace Kruty1918.Moyva.WorldCreation.Runtime
{
    /// <summary>
    /// Реалізація <see cref="IWorldCreationService"/>.
    /// Зберігає поточну конфігурацію та надає утиліти для UI.
    /// Не залежить від Unity MonoBehaviour — є чистим C# класом.
    /// </summary>
    internal sealed class WorldCreationService : IWorldCreationService
    {
        private readonly WorldCreationDefaultsSO _defaults;
        private WorldCreationConfig _currentConfig;

        public WorldCreationService(WorldCreationDefaultsSO defaults)
        {
            _defaults = defaults;
            _currentConfig = defaults != null
                ? defaults.ToConfig()
                : new WorldCreationConfig();
        }

        /// <inheritdoc/>
        public WorldCreationConfig CurrentConfig => _currentConfig;

        /// <inheritdoc/>
        public void UpdateConfig(WorldCreationConfig config)
        {
            _currentConfig = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <inheritdoc/>
        public void ResetToDefaults()
        {
            _currentConfig = _defaults != null
                ? _defaults.ToConfig()
                : new WorldCreationConfig();
        }

        /// <inheritdoc/>
        public int GenerateRandomSeed()
        {
            // Уникаємо 0, щоб seed завжди був «явним».
            int seed;
            do { seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue); }
            while (seed == 0);

            _currentConfig.Seed = seed;
            return seed;
        }

        /// <inheritdoc/>
        public bool ValidateConfig(WorldCreationConfig config, out string errorMessage)
        {
            if (config == null)
            {
                errorMessage = "Конфігурація відсутня.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(config.WorldName))
            {
                errorMessage = "Назва світу не може бути порожньою.";
                return false;
            }

            if (config.WorldName.Length > 64)
            {
                errorMessage = "Назва світу не може перевищувати 64 символи.";
                return false;
            }

            if (config.SizePreset == WorldSizePreset.Custom)
            {
                if (config.CustomWidth < 16 || config.CustomHeight < 16)
                {
                    errorMessage = "Розмір карти (Custom) не може бути меншим за 16×16.";
                    return false;
                }

                if (config.CustomWidth > 512 || config.CustomHeight > 512)
                {
                    errorMessage = "Розмір карти (Custom) не може перевищувати 512×512.";
                    return false;
                }
            }

            if (config.HumanPlayerCount < 1 || config.HumanPlayerCount > 4)
            {
                errorMessage = "Кількість людських гравців: від 1 до 4.";
                return false;
            }

            if (config.BotCount < 0 || config.BotCount > 4)
            {
                errorMessage = "Кількість ботів: від 0 до 4.";
                return false;
            }

            if (!config.EnableBots && config.BotCount > 0)
            {
                errorMessage = "Боти вимкнені, але кількість ботів > 0.";
                return false;
            }

            if (config.TotalFactions < 2)
            {
                errorMessage = "Потрібно мінімум 2 фракції (гравці + боти).";
                return false;
            }

            if (config.StartingGold < 0)
            {
                errorMessage = "Стартове золото не може бути від'ємним.";
                return false;
            }

            if (config.StartingFood < 0)
            {
                errorMessage = "Стартова їжа не може бути від'ємною.";
                return false;
            }

            errorMessage = null;
            return true;
        }

        /// <inheritdoc/>
        public WorldCreationConfigData ToSignalData(WorldCreationConfig config)
        {
            return new WorldCreationConfigData
            {
                WorldName          = config.WorldName,
                Seed               = config.Seed != 0 ? config.Seed : GenerateRandomSeed(),
                SizePresetIndex    = (int)config.SizePreset,
                CustomWidth        = config.CustomWidth,
                CustomHeight       = config.CustomHeight,
                MapTypePresetIndex = (int)config.MapType,
                DifficultyIndex    = (int)config.Difficulty,
                EnableBots         = config.EnableBots,
                HumanPlayerCount   = config.HumanPlayerCount,
                BotCount           = config.BotCount,
                StartingGold       = config.StartingGold,
                StartingFood       = config.StartingFood,
                ForestDensity      = config.ForestDensity,
                MountainDensity    = config.MountainDensity,
                WaterDensity       = config.WaterDensity,
                VillageDensity     = config.VillageDensity,
                GenerateRivers     = config.GenerateRivers,
                GenerateBiomes     = config.GenerateBiomes,
                ApplyWFC           = config.ApplyWFC
            };
        }
    }
}
