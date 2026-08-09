using System;
using System.Collections.Generic;
using System.IO;
using Kruty1918.Moyva.Animations.API;
using Kruty1918.Moyva.Editor.Shared;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.Units.Runtime;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Units.Editor
{
    public sealed class UnitJsonSyncResult
    {
        public int UnitCount;
        public bool Changed;
        public string Summary;
    }

    public static class UnitJsonCatalogService
    {
        public const string JsonPath = "Assets/Moyva/Presets/Units/unit-registry.json";
        private const string ExpectedSchema = "moyva.unit-registry";
        private const int SupportedVersion = 1;

        [MenuItem("Moyva/Data/JSON/Units/Validate and Sync")]
        public static void SyncFromMenu()
        {
            UnitJsonSyncResult result = SyncFromJson();
            Debug.Log($"[MoyvaUnitJson] {result.Summary}");
        }

        [MenuItem("Moyva/Data/JSON/Units/Export Current Registry")]
        public static void ExportFromMenu()
        {
            UnitRegistrySO registry = FindOnlyRegistry();
            UnitRegistryJsonDocument document = Export(registry);
            MoyvaJsonFile.Write(JsonPath, document);
            AssetDatabase.ImportAsset(JsonPath, ImportAssetOptions.ForceUpdate);
            Debug.Log($"[MoyvaUnitJson] Exported {document.Units.Count} units to {JsonPath}.");
        }

        public static UnitJsonSyncResult SyncFromJson()
        {
            UnitRegistryJsonDocument document = MoyvaJsonFile.Read<UnitRegistryJsonDocument>(JsonPath);
            Validate(document);

            UnitRegistrySO registry = ResolveRegistry(document.TargetAsset);
            List<UnitClassConfig> configs = BuildConfigs(document.Units);
            bool changed = !HasSameSerializedData(registry, configs);
            if (changed)
            {
                Undo.RecordObject(registry, "Sync Unit Registry from JSON");
                registry.Configs = configs;
                EditorUtility.SetDirty(registry);
                AssetDatabase.SaveAssetIfDirty(registry);
            }

            return new UnitJsonSyncResult
            {
                UnitCount = configs.Count,
                Changed = changed,
                Summary = $"schema={ExpectedSchema}@{SupportedVersion}, units={configs.Count}, changed={changed}",
            };
        }

        private static UnitRegistrySO ResolveRegistry(string targetAsset)
        {
            if (string.IsNullOrWhiteSpace(targetAsset))
                return FindOnlyRegistry();

            UnitRegistrySO registry = AssetDatabase.LoadAssetAtPath<UnitRegistrySO>(targetAsset);
            if (registry == null)
                throw new InvalidDataException($"{JsonPath}: targetAsset is not a UnitRegistrySO: {targetAsset}");

            return registry;
        }

        private static UnitRegistrySO FindOnlyRegistry()
        {
            string[] guids = AssetDatabase.FindAssets("t:UnitRegistrySO", new[] { "Assets/Moyva" });
            if (guids.Length != 1)
                throw new InvalidOperationException($"Expected exactly one UnitRegistrySO in Assets/Moyva, found {guids.Length}.");

            return AssetDatabase.LoadAssetAtPath<UnitRegistrySO>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        private static List<UnitClassConfig> BuildConfigs(IReadOnlyList<UnitJsonDefinition> definitions)
        {
            var result = new List<UnitClassConfig>(definitions.Count);
            for (int index = 0; index < definitions.Count; index++)
            {
                UnitJsonDefinition source = definitions[index];
                string context = $"{JsonPath}: units[{index}] ({source.TypeId})";
                var config = new UnitClassConfig
                {
                    TypeId = source.TypeId,
                    DisplayName = source.DisplayName,
                    Role = ParseEnum<UnitRole>(source.Role, context + ".role"),
                    CombatType = ParseEnum<UnitCombatType>(source.CombatType, context + ".combatType"),
                    BaseStamina = source.Movement.BaseStamina,
                    StaminaRandomRange = new Vector2(source.Movement.StaminaRandomRange.Min, source.Movement.StaminaRandomRange.Max),
                    AnimationSettings = new PathAnimationSettings
                    {
                        MoveDurationPerTile = source.Movement.MoveDurationPerTile,
                        DelayOnTile = source.Movement.DelayOnTile,
                    },
                    VisionRange = source.Vision.Range,
                    VisionHeightBoostPerLevel = source.Vision.HeightBoostPerLevel,
                    CanSeeCrest = source.Vision.CanSeeCrest,
                    CrestVisibilityFactor = source.Vision.CrestVisibilityFactor,
                    DownSlopeVisionBonus = source.Vision.DownSlopeBonus,
                    SilhouettePenalty = source.Vision.SilhouettePenalty,
                    HitPoints = source.Combat.HitPoints,
                    BaseLevel = source.Combat.BaseLevel,
                    CuttingDamage = source.Combat.CuttingDamage,
                    PenetratingDamage = source.Combat.PenetratingDamage,
                    CrushingDamage = source.Combat.CrushingDamage,
                    CuttingDefense = source.Combat.CuttingDefense,
                    PenetratingDefense = source.Combat.PenetratingDefense,
                    CrushingDefense = source.Combat.CrushingDefense,
                    Prefab = MoyvaJsonAssetReferenceResolver.Resolve<GameObject>(source.Presentation.Prefab, true, context + ".presentation.prefab"),
                    CustomSprite = MoyvaJsonAssetReferenceResolver.Resolve<Sprite>(source.Presentation.CustomSprite, false, context + ".presentation.customSprite"),
                    AnimationClips = BuildAnimations(source.Animations, context),
                };
                result.Add(config);
            }

            return result;
        }

        private static List<UnitAnimationClip> BuildAnimations(IReadOnlyList<UnitJsonAnimation> definitions, string context)
        {
            var result = new List<UnitAnimationClip>(definitions?.Count ?? 0);
            if (definitions == null)
                return result;

            for (int index = 0; index < definitions.Count; index++)
            {
                UnitJsonAnimation source = definitions[index];
                string animationContext = $"{context}.animations[{index}]";
                var spriteFrames = new List<Sprite>(source.SpriteFrames?.Count ?? 0);
                if (source.SpriteFrames != null)
                {
                    for (int frameIndex = 0; frameIndex < source.SpriteFrames.Count; frameIndex++)
                    {
                        spriteFrames.Add(MoyvaJsonAssetReferenceResolver.Resolve<Sprite>(
                            source.SpriteFrames[frameIndex],
                            true,
                            $"{animationContext}.spriteFrames[{frameIndex}]"));
                    }
                }

                result.Add(new UnitAnimationClip
                {
                    Type = ParseEnum<AnimationType>(source.Type, animationContext + ".type"),
                    Name = source.Name,
                    AnimationClip = MoyvaJsonAssetReferenceResolver.Resolve<AnimationClip>(source.AnimationClip, false, animationContext + ".animationClip"),
                    AnimatorParameterName = source.AnimatorParameterName,
                    SpriteFrames = spriteFrames,
                    SpriteFPS = source.SpriteFps,
                    Loop = source.Loop,
                    Duration = source.Duration,
                });
            }

            return result;
        }

        private static bool HasSameSerializedData(UnitRegistrySO registry, List<UnitClassConfig> configs)
        {
            var staged = ScriptableObject.CreateInstance<UnitRegistrySO>();
            staged.hideFlags = HideFlags.HideAndDontSave;
            staged.name = registry.name;
            staged.Configs = configs;
            try
            {
                return string.Equals(
                    EditorJsonUtility.ToJson(registry, false),
                    EditorJsonUtility.ToJson(staged, false),
                    StringComparison.Ordinal);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(staged);
            }
        }

        private static void Validate(UnitRegistryJsonDocument document)
        {
            if (!string.Equals(document.Schema, ExpectedSchema, StringComparison.Ordinal))
                throw new InvalidDataException($"{JsonPath}: schema must be '{ExpectedSchema}'.");
            if (document.Version != SupportedVersion)
                throw new InvalidDataException($"{JsonPath}: unsupported version {document.Version}; expected {SupportedVersion}.");
            if (document.Units == null || document.Units.Count == 0)
                throw new InvalidDataException($"{JsonPath}: units must contain at least one entry.");

            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int index = 0; index < document.Units.Count; index++)
            {
                UnitJsonDefinition unit = document.Units[index]
                                          ?? throw new InvalidDataException($"{JsonPath}: units[{index}] is null.");
                string context = $"{JsonPath}: units[{index}]";
                if (string.IsNullOrWhiteSpace(unit.TypeId))
                    throw new InvalidDataException($"{context}: typeId is required.");
                if (unit.TypeId.Contains("_"))
                    throw new InvalidDataException($"{context}: typeId cannot contain '_' because it is reserved for instance IDs.");
                if (!ids.Add(unit.TypeId))
                    throw new InvalidDataException($"{context}: duplicate typeId '{unit.TypeId}'.");
                if (unit.Movement == null || unit.Vision == null || unit.Combat == null || unit.Presentation == null)
                    throw new InvalidDataException($"{context}: movement, vision, combat and presentation are required.");
                if (unit.Movement.BaseStamina <= 0f)
                    throw new InvalidDataException($"{context}: movement.baseStamina must be > 0.");
                if (unit.Movement.StaminaRandomRange == null
                    || unit.Movement.StaminaRandomRange.Min > unit.Movement.StaminaRandomRange.Max)
                {
                    throw new InvalidDataException($"{context}: movement.staminaRandomRange must have min <= max.");
                }
                if (unit.Movement.MoveDurationPerTile < 0f || unit.Movement.DelayOnTile < 0f)
                    throw new InvalidDataException($"{context}: movement timings cannot be negative.");
                if (unit.Vision.Range < 1)
                    throw new InvalidDataException($"{context}: vision.range must be >= 1.");
                if (unit.Combat.HitPoints < 1 || unit.Combat.BaseLevel < 1)
                    throw new InvalidDataException($"{context}: combat hitPoints and baseLevel must be >= 1.");
                if (unit.Presentation.Prefab == null)
                    throw new InvalidDataException($"{context}: presentation.prefab is required.");

                ParseEnum<UnitRole>(unit.Role, context + ".role");
                ParseEnum<UnitCombatType>(unit.CombatType, context + ".combatType");
                if (unit.Animations == null)
                    unit.Animations = new List<UnitJsonAnimation>();
                for (int animationIndex = 0; animationIndex < unit.Animations.Count; animationIndex++)
                {
                    UnitJsonAnimation animation = unit.Animations[animationIndex]
                                                    ?? throw new InvalidDataException($"{context}.animations[{animationIndex}] is null.");
                    ParseEnum<AnimationType>(animation.Type, $"{context}.animations[{animationIndex}].type");
                    if (animation.Duration < 0f || animation.SpriteFps < 0)
                        throw new InvalidDataException($"{context}.animations[{animationIndex}]: duration and spriteFps cannot be negative.");
                }
            }
        }

        private static TEnum ParseEnum<TEnum>(string value, string context) where TEnum : struct
        {
            if (!Enum.TryParse(value, true, out TEnum parsed) || !Enum.IsDefined(typeof(TEnum), parsed))
                throw new InvalidDataException($"{context}: unknown {typeof(TEnum).Name} '{value}'.");

            return parsed;
        }

        private static UnitRegistryJsonDocument Export(UnitRegistrySO registry)
        {
            var document = new UnitRegistryJsonDocument
            {
                TargetAsset = AssetDatabase.GetAssetPath(registry),
            };

            foreach (UnitClassConfig config in registry.Configs ?? new List<UnitClassConfig>())
            {
                if (config == null)
                    continue;

                var definition = new UnitJsonDefinition
                {
                    TypeId = config.TypeId,
                    DisplayName = config.DisplayName,
                    Role = config.Role.ToString(),
                    CombatType = config.CombatType.ToString(),
                    Movement = new UnitJsonMovement
                    {
                        BaseStamina = config.BaseStamina,
                        StaminaRandomRange = new UnitJsonFloatRange
                        {
                            Min = config.StaminaRandomRange.x,
                            Max = config.StaminaRandomRange.y,
                        },
                        MoveDurationPerTile = config.AnimationSettings.MoveDurationPerTile,
                        DelayOnTile = config.AnimationSettings.DelayOnTile,
                    },
                    Vision = new UnitJsonVision
                    {
                        Range = config.VisionRange,
                        HeightBoostPerLevel = config.VisionHeightBoostPerLevel,
                        CanSeeCrest = config.CanSeeCrest,
                        CrestVisibilityFactor = config.CrestVisibilityFactor,
                        DownSlopeBonus = config.DownSlopeVisionBonus,
                        SilhouettePenalty = config.SilhouettePenalty,
                    },
                    Combat = new UnitJsonCombat
                    {
                        HitPoints = config.HitPoints,
                        BaseLevel = config.BaseLevel,
                        CuttingDamage = config.CuttingDamage,
                        PenetratingDamage = config.PenetratingDamage,
                        CrushingDamage = config.CrushingDamage,
                        CuttingDefense = config.CuttingDefense,
                        PenetratingDefense = config.PenetratingDefense,
                        CrushingDefense = config.CrushingDefense,
                    },
                    Presentation = new UnitJsonPresentation
                    {
                        Prefab = MoyvaJsonAssetReferenceResolver.FromObject(config.Prefab),
                        CustomSprite = MoyvaJsonAssetReferenceResolver.FromObject(config.CustomSprite),
                    },
                };

                foreach (UnitAnimationClip animation in config.AnimationClips ?? new List<UnitAnimationClip>())
                {
                    if (animation == null)
                        continue;

                    var jsonAnimation = new UnitJsonAnimation
                    {
                        Type = animation.Type.ToString(),
                        Name = animation.Name,
                        AnimationClip = MoyvaJsonAssetReferenceResolver.FromObject(animation.AnimationClip),
                        AnimatorParameterName = animation.AnimatorParameterName,
                        SpriteFps = animation.SpriteFPS,
                        Loop = animation.Loop,
                        Duration = animation.Duration,
                    };
                    foreach (Sprite frame in animation.SpriteFrames ?? new List<Sprite>())
                        jsonAnimation.SpriteFrames.Add(MoyvaJsonAssetReferenceResolver.FromObject(frame));
                    definition.Animations.Add(jsonAnimation);
                }

                document.Units.Add(definition);
            }

            return document;
        }
    }

    internal sealed class UnitJsonAutoSync : AssetPostprocessor
    {
        private static bool _scheduled;
        private static bool _running;

        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (!ContainsCatalog(importedAssets) && !ContainsCatalog(movedAssets))
                return;

            if (_scheduled)
                return;

            _scheduled = true;
            EditorApplication.delayCall += Apply;
        }

        private static void Apply()
        {
            _scheduled = false;
            if (_running || EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                _scheduled = true;
                EditorApplication.delayCall += Apply;
                return;
            }

            _running = true;
            try
            {
                UnitJsonSyncResult result = UnitJsonCatalogService.SyncFromJson();
                Debug.Log($"[MoyvaUnitJson] Auto-sync {result.Summary}");
            }
            catch (Exception exception)
            {
                Debug.LogError($"[MoyvaUnitJson] Auto-sync failed: {exception}");
            }
            finally
            {
                _running = false;
            }
        }

        private static bool ContainsCatalog(string[] paths)
        {
            if (paths == null)
                return false;

            for (int index = 0; index < paths.Length; index++)
            {
                if (string.Equals(paths[index], UnitJsonCatalogService.JsonPath, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    internal sealed class UnitRegistryJsonDocument
    {
        [JsonProperty("$schema", Order = 0)] public string JsonSchema = "../Schemas/unit-registry.schema.json";
        [JsonProperty("schema", Order = 1)] public string Schema = "moyva.unit-registry";
        [JsonProperty("version", Order = 2)] public int Version = 1;
        [JsonProperty("targetAsset", Order = 3)] public string TargetAsset = "Assets/Moyva/Data/ScriptableObjects/Units/UnitRegistry.asset";
        [JsonProperty("units", Order = 4)] public List<UnitJsonDefinition> Units = new List<UnitJsonDefinition>();
    }

    [JsonObject(MemberSerialization.OptIn)]
    internal sealed class UnitJsonDefinition
    {
        [JsonProperty("typeId")] public string TypeId;
        [JsonProperty("displayName")] public string DisplayName;
        [JsonProperty("role")] public string Role;
        [JsonProperty("combatType")] public string CombatType;
        [JsonProperty("movement")] public UnitJsonMovement Movement;
        [JsonProperty("vision")] public UnitJsonVision Vision;
        [JsonProperty("combat")] public UnitJsonCombat Combat;
        [JsonProperty("presentation")] public UnitJsonPresentation Presentation;
        [JsonProperty("animations")] public List<UnitJsonAnimation> Animations = new List<UnitJsonAnimation>();
    }

    [JsonObject(MemberSerialization.OptIn)]
    internal sealed class UnitJsonMovement
    {
        [JsonProperty("baseStamina")] public float BaseStamina;
        [JsonProperty("staminaRandomRange")] public UnitJsonFloatRange StaminaRandomRange;
        [JsonProperty("moveDurationPerTile")] public float MoveDurationPerTile;
        [JsonProperty("delayOnTile")] public float DelayOnTile;
    }

    [JsonObject(MemberSerialization.OptIn)]
    internal sealed class UnitJsonFloatRange
    {
        [JsonProperty("min")] public float Min;
        [JsonProperty("max")] public float Max;
    }

    [JsonObject(MemberSerialization.OptIn)]
    internal sealed class UnitJsonVision
    {
        [JsonProperty("range")] public int Range;
        [JsonProperty("heightBoostPerLevel")] public float HeightBoostPerLevel;
        [JsonProperty("canSeeCrest")] public bool CanSeeCrest;
        [JsonProperty("crestVisibilityFactor")] public float CrestVisibilityFactor;
        [JsonProperty("downSlopeBonus")] public float DownSlopeBonus;
        [JsonProperty("silhouettePenalty")] public float SilhouettePenalty;
    }

    [JsonObject(MemberSerialization.OptIn)]
    internal sealed class UnitJsonCombat
    {
        [JsonProperty("hitPoints")] public int HitPoints;
        [JsonProperty("baseLevel")] public int BaseLevel;
        [JsonProperty("cuttingDamage")] public int CuttingDamage;
        [JsonProperty("penetratingDamage")] public int PenetratingDamage;
        [JsonProperty("crushingDamage")] public int CrushingDamage;
        [JsonProperty("cuttingDefense")] public int CuttingDefense;
        [JsonProperty("penetratingDefense")] public int PenetratingDefense;
        [JsonProperty("crushingDefense")] public int CrushingDefense;
    }

    [JsonObject(MemberSerialization.OptIn)]
    internal sealed class UnitJsonPresentation
    {
        [JsonProperty("prefab")] public MoyvaJsonAssetReference Prefab;
        [JsonProperty("customSprite")] public MoyvaJsonAssetReference CustomSprite;
    }

    [JsonObject(MemberSerialization.OptIn)]
    internal sealed class UnitJsonAnimation
    {
        [JsonProperty("type")] public string Type;
        [JsonProperty("name")] public string Name;
        [JsonProperty("animationClip")] public MoyvaJsonAssetReference AnimationClip;
        [JsonProperty("animatorParameterName")] public string AnimatorParameterName;
        [JsonProperty("spriteFrames")] public List<MoyvaJsonAssetReference> SpriteFrames = new List<MoyvaJsonAssetReference>();
        [JsonProperty("spriteFps")] public int SpriteFps;
        [JsonProperty("loop")] public bool Loop;
        [JsonProperty("duration")] public float Duration;
    }
}
