using System;

namespace Kruty1918.Moyva.Construction.API
{
    /// <summary>
    /// Converts stable placement reason codes to short player-facing Ukrainian text.
    /// Diagnostic details remain available in ConstructionPlacementDiagnostic.
    /// </summary>
    public static class ConstructionPlacementReasonText
    {
        public static string Resolve(string reasonCode, string fallback = null)
        {
            switch ((reasonCode ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "allowed":
                    return null;
                case "resources":
                    return "Недостатньо ресурсів.";
                case "authority":
                    return "Цю дію може виконати лише власник.";
                case "occupied-tile":
                    return "Місце вже зайняте.";
                case "spacing":
                    return "Надто близько до іншої будівлі.";
                case "fog":
                    return "Спочатку розвідайте цю ділянку.";
                case "influence-required":
                    return "Потрібна зона впливу поселення.";
                case "influence-overlap":
                    return "Зони поселень не можуть перетинатися.";
                case "terrain":
                    return "Непридатний рельєф для цієї будівлі.";
                case "adjacency":
                    return "Не виконано вимоги до сусідніх клітинок.";
                case "prerequisite":
                    return "Спочатку виконайте вимоги будівлі.";
                case "configuration":
                case "building-id-empty":
                    return "Будівля налаштована некоректно.";
                case "spatial-rules":
                    return "У цьому місці будувати не можна.";
                case "resource-context-deferred":
                    return "Вартість буде перевірена після вибору місця.";
                default:
                    return string.IsNullOrWhiteSpace(fallback)
                        ? "Не виконано умови будівництва."
                        : fallback;
            }
        }

        public static string Resolve(BuildingPlacementBlockerKind kind, string fallback = null)
        {
            string reasonCode = kind switch
            {
                BuildingPlacementBlockerKind.OccupiedTile => "occupied-tile",
                BuildingPlacementBlockerKind.Spacing => "spacing",
                BuildingPlacementBlockerKind.Fog => "fog",
                BuildingPlacementBlockerKind.InfluenceRequired => "influence-required",
                BuildingPlacementBlockerKind.InfluenceOverlap => "influence-overlap",
                BuildingPlacementBlockerKind.Configuration => "configuration",
                BuildingPlacementBlockerKind.Terrain => "terrain",
                BuildingPlacementBlockerKind.Adjacency => "adjacency",
                BuildingPlacementBlockerKind.Prerequisite => "prerequisite",
                _ => null,
            };
            return Resolve(reasonCode, fallback);
        }
    }
}
