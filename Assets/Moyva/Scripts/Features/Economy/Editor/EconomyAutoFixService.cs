using System;
using Kruty1918.Moyva.Economy.API;
using UnityEditor;

namespace Kruty1918.Moyva.Economy.Editor
{
    public sealed class EconomyAutoFixService
    {
        public int FixCommonIssues(EconomyDatabaseSO database)
        {
            if (database == null)
                return 0;

            var fixes = 0;

            for (var i = 0; i < database.Resources.Count; i++)
            {
                var resource = database.Resources[i];
                if (resource == null)
                    continue;

                fixes += TrimString(resource, "_id");
                fixes += TrimString(resource, "_displayName");
            }

            for (var i = 0; i < database.Settlements.Count; i++)
            {
                var settlement = database.Settlements[i];
                if (settlement == null)
                    continue;

                fixes += TrimString(settlement, "_settlementId");
                fixes += TrimString(settlement, "_centerBuildingId");
                fixes += ClampMinInt(settlement, "_buildRadius", 1);
            }

            for (var i = 0; i < database.WarehousePolicies.Count; i++)
            {
                var warehouse = database.WarehousePolicies[i];
                if (warehouse == null)
                    continue;

                var so = new SerializedObject(warehouse);
                var entries = so.FindProperty("_entries");
                if (entries == null)
                    continue;

                var changed = false;
                for (var entryIndex = 0; entryIndex < entries.arraySize; entryIndex++)
                {
                    var entry = entries.GetArrayElementAtIndex(entryIndex);
                    if (entry == null)
                        continue;

                    changed |= TrimSerializedString(entry.FindPropertyRelative("_resourceId"));

                    var priorityProp = entry.FindPropertyRelative("_priority");
                    if (priorityProp != null && priorityProp.intValue <= 0)
                    {
                        priorityProp.intValue = 1;
                        changed = true;
                    }
                }

                if (!changed)
                    continue;

                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(warehouse);
                fixes++;
            }

            for (var i = 0; i < database.ProductionProfiles.Count; i++)
            {
                var profile = database.ProductionProfiles[i];
                if (profile == null)
                    continue;

                fixes += TrimString(profile, "_buildingId");
                fixes += TrimString(profile, "_recipeId");
                fixes += ClampMinFloat(profile, "_cycleDurationSeconds", 1f);
                fixes += ClampMinInt(profile, "_outputAmountPerCycle", 1);
            }

            for (var i = 0; i < database.CaravanTemplates.Count; i++)
            {
                var caravan = database.CaravanTemplates[i];
                if (caravan == null)
                    continue;

                fixes += TrimString(caravan, "_templateId");
                fixes += ClampMinInt(caravan, "_capacity", 1);
                fixes += ClampMinInt(caravan, "_defaultPriority", 1);
            }

            for (var i = 0; i < database.AiRuleProfiles.Count; i++)
            {
                var profile = database.AiRuleProfiles[i];
                if (profile == null)
                    continue;

                fixes += TrimString(profile, "_profileId");
            }

            if (database.RulesConfig != null)
                fixes += FixRulesConfig(database.RulesConfig);

            return fixes;
        }

        private static int FixRulesConfig(EconomyRulesConfigSO rules)
        {
            var fixes = 0;
            var so = new SerializedObject(rules);
            so.Update();

            fixes += ClampSerializedMinInt(so.FindProperty("_settlement").FindPropertyRelative("_maxSettlements"), 1);
            fixes += ClampSerializedMinInt(so.FindProperty("_settlement").FindPropertyRelative("_minTownHallDistance"), 1);
            fixes += ClampSerializedMinInt(so.FindProperty("_population").FindPropertyRelative("_newResidentsArrivalIntervalTurns"), 1);
            fixes += ClampSerializedMinInt(so.FindProperty("_caravan").FindPropertyRelative("_maxCaravansPerSettlement"), 1);

            fixes += ClampSerializedMinFloat(so.FindProperty("_market").FindPropertyRelative("_targetStock"), 1f);
            fixes += ClampSerializedMinFloat(so.FindProperty("_market").FindPropertyRelative("_referenceTradeVolume"), 1f);
            fixes += ClampSerializedMinFloat(so.FindProperty("_market").FindPropertyRelative("_minPriceMultiplier"), 0.01f);
            fixes += ClampSerializedMinFloat(so.FindProperty("_market").FindPropertyRelative("_maxPriceMultiplier"), 0.01f);

            var minPrice = so.FindProperty("_market").FindPropertyRelative("_minPriceMultiplier");
            var maxPrice = so.FindProperty("_market").FindPropertyRelative("_maxPriceMultiplier");
            if (minPrice != null && maxPrice != null && maxPrice.floatValue < minPrice.floatValue)
            {
                maxPrice.floatValue = minPrice.floatValue;
                fixes++;
            }

            if (fixes > 0)
            {
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(rules);
            }

            return fixes;
        }

        private static int TrimString(UnityEngine.Object target, string propertyName)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(propertyName);
            if (!TrimSerializedString(prop))
                return 0;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
            return 1;
        }

        private static int ClampMinInt(UnityEngine.Object target, string propertyName, int minValue)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(propertyName);
            if (prop == null || prop.propertyType != SerializedPropertyType.Integer || prop.intValue >= minValue)
                return 0;

            prop.intValue = minValue;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
            return 1;
        }

        private static int ClampMinFloat(UnityEngine.Object target, string propertyName, float minValue)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(propertyName);
            if (prop == null || prop.propertyType != SerializedPropertyType.Float || prop.floatValue >= minValue)
                return 0;

            prop.floatValue = minValue;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
            return 1;
        }

        private static bool TrimSerializedString(SerializedProperty prop)
        {
            if (prop == null || prop.propertyType != SerializedPropertyType.String)
                return false;

            var current = prop.stringValue ?? string.Empty;
            var trimmed = current.Trim();
            if (string.Equals(current, trimmed, StringComparison.Ordinal))
                return false;

            prop.stringValue = trimmed;
            return true;
        }

        private static int ClampSerializedMinInt(SerializedProperty prop, int minValue)
        {
            if (prop == null || prop.propertyType != SerializedPropertyType.Integer || prop.intValue >= minValue)
                return 0;

            prop.intValue = minValue;
            return 1;
        }

        private static int ClampSerializedMinFloat(SerializedProperty prop, float minValue)
        {
            if (prop == null || prop.propertyType != SerializedPropertyType.Float || prop.floatValue >= minValue)
                return 0;

            prop.floatValue = minValue;
            return 1;
        }
    }
}
