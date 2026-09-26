using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Kruty1918.Localization;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal enum GameplayHtmlPanel
    {
        None,
        Construction,
        Kingdom,
        Notifications,
        Supply,
    }

    internal enum KingdomDashboardTab
    {
        Overview,
        Resources,
        Storage,
        Buildings,
        Units,
        Turns,
        Logistics,
    }

    internal enum GameplaySelectionTab
    {
        Details,
        Recruit,
        Queue,
        Cargo,
        Route,
    }

    internal sealed class GameplayHtmlState
    {
        private readonly ILocalizationService _localization;

        public GameplayHtmlState([InjectOptional] ILocalizationService localization = null)
        {
            _localization = localization;
            if (_localization != null)
                _localization.LanguageChanged += () => MarkDirty();
        }

        /// <summary>Локалізує статичний UI-текст; без сервісу повертає source.</summary>
        internal string T(string key) => _localization?.T(key) ?? key ?? string.Empty;

        /// <summary>Локалізує й форматує {0}..{n} плейсхолдери.</summary>
        internal string TF(string key, params object[] args) =>
            _localization?.TF(key, args) ?? key ?? string.Empty;

        /// <summary>Plural-форма для count (one/few/many/other за активною мовою).</summary>
        internal string TN(string oneKey, string otherKey, int count) =>
            _localization?.TN(oneKey, otherKey, count) ?? (count == 1 ? oneKey : otherKey);

        /// <summary>Сервіс локалізації для статичних хелперів (time text тощо).</summary>
        internal ILocalizationService Localization => _localization;

        public event Action Changed;
        public bool GamepadAim { get; set; }
        public string ControlHints { get; set; } = string.Empty;

        public GameplayHtmlPanel OpenPanelId { get; private set; }
        public KingdomDashboardTab DashboardTab { get; private set; }
        public GameplaySelectionTab SelectionTab { get; private set; }
        public bool IsPaused { get; private set; }
        public bool IsGameOver { get; private set; }
        public string WinnerId { get; private set; } = string.Empty;
        public string Feedback { get; private set; } = string.Empty;
        public string ConstructionCategory { get; private set; } = string.Empty;
        public string ConstructionSearch { get; private set; } = string.Empty;
        public string ConstructionProducerResource { get; private set; } = string.Empty;
        public int ConstructionPageIndex { get; private set; }
        /// <summary>Building card the construction list should jump to and
        /// highlight once (consumed by markup on render).</summary>
        private string _constructionFocusId = string.Empty;
        private float _constructionFocusUntil = -1f;
        private bool _constructionFocusPending;
        public Vector2Int? SupplyPosition { get; private set; }
        public string SupplyBuildingId { get; private set; } = string.Empty;
        /// <summary>Active guidance session — the saved goal plus popup
        /// visibility. The popup renders when <see cref="GuidanceSession.Open"/>
        /// is true; a minimized session stays reachable via the goal chip.</summary>
        public GuidanceSession Guidance { get; private set; }
        public IReadOnlyList<GameplayNotificationViewSnapshot> Notifications => _notifications;
        public int UnreadNotifications { get; private set; }
        public bool Dirty { get; private set; } = true;
        private float _feedbackExpiresAt = -1f;
        private readonly List<GameplayNotificationViewSnapshot> _notifications = new();
        private long _nextNotificationId;

        /// <summary>Seconds a closing panel stays mounted while its exit
        /// motion plays; input shields stay up for the whole window so a
        /// trailing pointer-up cannot reach the map.</summary>
        internal const float PanelCloseSeconds = 0.18f;

        /// <summary>Panel lifecycle: a close request plays the exit motion and
        /// settles on the next presenter tick past the deadline. While closing,
        /// new panel commands are blocked and the DOM node stays mounted.</summary>
        public bool PanelClosing { get; private set; }
        private float _panelCloseEndsAt = -1f;

        public void OpenPanel(GameplayHtmlPanel panel)
        {
            if (PanelClosing)
                SettlePanelClose();
            if (OpenPanelId == panel)
            {
                ClosePanel();
                return;
            }
            OpenPanelId = panel;
            if (OpenPanelId == GameplayHtmlPanel.Notifications) UnreadNotifications = 0;
            MarkDirty();
        }

        public void ClosePanel()
        {
            if (OpenPanelId == GameplayHtmlPanel.None || PanelClosing)
                return;
            PanelClosing = true;
            _panelCloseEndsAt = -1f;
            MarkDirty();
        }

        /// <summary>Presenter ticks this every frame; when the exit window has
        /// elapsed the panel finally unmounts. Returns true when it settled.</summary>
        public bool AdvancePanelClose()
            => AdvancePanelClose(Time.unscaledTime);

        internal bool AdvancePanelClose(float now)
        {
            if (!PanelClosing)
                return false;
            if (_panelCloseEndsAt < 0f)
                _panelCloseEndsAt = now + PanelCloseSeconds;
            if (now < _panelCloseEndsAt)
                return false;
            SettlePanelClose();
            return true;
        }

        /// <summary>The exit tween for the closing panel completed (or was
        /// cancelled). Settles the close immediately instead of waiting out
        /// the time window; a panel without an exit motion still closes via
        /// AdvancePanelClose.</summary>
        public void NotifyPanelExitFinished()
        {
            if (!PanelClosing)
                return;
            SettlePanelClose();
        }

        private void SettlePanelClose()
        {
            PanelClosing = false;
            _panelCloseEndsAt = -1f;
            OpenPanelId = GameplayHtmlPanel.None;
            SupplyPosition = null;
            SupplyBuildingId = string.Empty;
            ConstructionProducerResource = string.Empty;
            MarkDirty();
        }

        public void OpenSupplyPanel(Vector2Int position, string buildingId)
        {
            if (PanelClosing)
                SettlePanelClose();
            SupplyPosition = position;
            SupplyBuildingId = buildingId?.Trim() ?? string.Empty;
            OpenPanelId = GameplayHtmlPanel.Supply;
            MarkDirty();
        }

        public void SetDashboardTab(KingdomDashboardTab tab)
        {
            DashboardTab = tab;
            MarkDirty();
        }

        public void SetSelectionTab(GameplaySelectionTab tab)
        {
            if (SelectionTab == tab)
                return;
            SelectionTab = tab;
            MarkDirty();
        }

        public void ResetSelectionTab()
        {
            if (SelectionTab == GameplaySelectionTab.Details)
                return;
            SelectionTab = GameplaySelectionTab.Details;
            MarkDirty();
        }

        public void SetConstructionCategory(string category)
        {
            string normalized = category?.Trim() ?? string.Empty;
            if (string.Equals(ConstructionCategory, normalized, StringComparison.OrdinalIgnoreCase))
                return;
            ConstructionCategory = normalized;
            ConstructionProducerResource = string.Empty;
            ConstructionPageIndex = 0;
            ClearConstructionFocus();
            MarkDirty();
        }

        public void SetConstructionSearch(string search)
        {
            string normalized = search?.Trim() ?? string.Empty;
            if (string.Equals(ConstructionSearch, normalized, StringComparison.OrdinalIgnoreCase))
                return;
            ConstructionSearch = normalized;
            ConstructionProducerResource = string.Empty;
            ConstructionPageIndex = 0;
            ClearConstructionFocus();
            MarkDirty();
        }

        public void SetConstructionProducerFilter(string resourceId)
        {
            string normalized = resourceId?.Trim() ?? string.Empty;
            if (string.Equals(ConstructionProducerResource, normalized, StringComparison.Ordinal))
                return;
            ConstructionProducerResource = normalized;
            ConstructionPageIndex = 0;
            ClearConstructionFocus();
            MarkDirty();
        }

        /// <summary>Points the construction list at a specific building card:
        /// clears search/category/producer filters so the card is visible,
        /// arms a one-shot page jump and a ~6 s card highlight.</summary>
        public void SetConstructionFocus(string buildingId)
        {
            ConstructionSearch = string.Empty;
            ConstructionCategory = string.Empty;
            ConstructionProducerResource = string.Empty;
            ConstructionPageIndex = 0;
            _constructionFocusId = buildingId?.Trim() ?? string.Empty;
            _constructionFocusPending = !string.IsNullOrEmpty(_constructionFocusId);
            _constructionFocusUntil = Time.unscaledTime + 6f;
            MarkDirty();
        }

        public void ClearConstructionFocus()
        {
            _constructionFocusId = string.Empty;
            _constructionFocusUntil = -1f;
            _constructionFocusPending = false;
        }

        /// <summary>One-shot read of the pending page jump — markup consumes
        /// the flag while the highlight itself stays time-boxed.</summary>
        internal string ConsumeConstructionFocusJump()
        {
            if (!_constructionFocusPending)
                return null;
            _constructionFocusPending = false;
            return Time.unscaledTime <= _constructionFocusUntil
                ? _constructionFocusId : null;
        }

        /// <summary>Card id currently highlighted in the construction list —
        /// expires a few seconds after focus so the player actually sees it.</summary>
        public string HighlightedConstructionId =>
            !string.IsNullOrWhiteSpace(_constructionFocusId)
            && Time.unscaledTime <= _constructionFocusUntil
                ? _constructionFocusId : string.Empty;

        /// <summary>Markup jumps the construction pager to a focused card;
        /// keeps the stored index consistent with the visible page.</summary>
        internal void SetConstructionPageIndex(int index)
        {
            int next = Math.Max(0, index);
            if (next == ConstructionPageIndex)
                return;
            ConstructionPageIndex = next;
            MarkDirty();
        }

        /// <summary>Opens (or replaces) the guidance session for a rejected
        /// action. Repeated rejections reuse the single popup instance.</summary>
        public void OpenGuidance(GuidanceGoal goal)
        {
            if (goal == null)
                return;
            Guidance = new GuidanceSession(goal) { Open = true };
            MarkDirty();
        }

        public void ReopenGuidance(GuidanceGoal goal)
        {
            if (goal == null)
                return;
            Guidance = new GuidanceSession(goal) { Open = true };
            MarkDirty();
        }

        /// <summary>Hides the popup but keeps the goal — a chip offers to
        /// reopen it or resume the action.</summary>
        public void MinimizeGuidance()
        {
            if (Guidance == null || !Guidance.Open)
                return;
            Guidance.Open = false;
            MarkDirty();
        }

        public void CloseGuidance()
        {
            if (Guidance == null)
                return;
            Guidance = null;
            MarkDirty();
        }

        public void MoveGuidanceFocus(int delta, int blockerCount)
        {
            if (Guidance == null || blockerCount <= 0)
                return;
            int next = Math.Clamp(Guidance.FocusIndex + delta, 0, blockerCount - 1);
            if (next == Guidance.FocusIndex)
                return;
            Guidance.FocusIndex = next;
            MarkDirty();
        }

        /// <summary>Clicking a blocker card focuses it; clicking the focused
        /// card again collapses its option list (focus = -1).</summary>
        public void SetGuidanceFocus(int index)
        {
            if (Guidance == null || index < 0)
                return;
            Guidance.FocusIndex = index == Guidance.FocusIndex ? -1 : index;
            MarkDirty();
        }

        public void MoveConstructionPage(int delta)
        {
            int next = Math.Max(0, ConstructionPageIndex + delta);
            if (next == ConstructionPageIndex)
                return;
            ConstructionPageIndex = next;
            MarkDirty();
        }

        public void AddNotification(string message, string kind, Vector2Int? position = null,
            string targetId = null, long queueId = 0, string deliveryDetails = null,
            GuidanceGoal goal = null)
        {
            string normalized = message?.Trim();
            if (string.IsNullOrWhiteSpace(normalized))
                return;
            // Repeated blocked attempts on the same goal refresh the existing
            // entry instead of stacking duplicates.
            if (goal != null)
                _notifications.RemoveAll(item => SameGoal(item.Goal, goal));
            _notifications.Insert(0, new GameplayNotificationViewSnapshot(normalized, kind, ++_nextNotificationId, position, targetId, queueId, deliveryDetails, goal));
            if (OpenPanelId != GameplayHtmlPanel.Notifications) UnreadNotifications++;
            if (_notifications.Count > 20)
                _notifications.RemoveAt(_notifications.Count - 1);
            SetFeedback(normalized);
        }

        private static bool SameGoal(GuidanceGoal a, GuidanceGoal b)
        {
            if (a == null || b == null || a.Kind != b.Kind)
                return false;
            return a.Kind == GuidanceGoalKind.Recruitment
                ? a.Position == b.Position
                    && string.Equals(a.UnitTypeId, b.UnitTypeId, StringComparison.Ordinal)
                : a.Position == b.Position
                    && string.Equals(a.BuildingId, b.BuildingId, StringComparison.Ordinal);
        }

        public void ClearNotifications()
        {
            _notifications.Clear();
            UnreadNotifications = 0;
            MarkDirty();
        }

        public void RemoveNotification(long id)
        {
            _notifications.RemoveAll(item => item.Id == id);
            UnreadNotifications = Math.Min(UnreadNotifications, _notifications.Count);
            MarkDirty();
        }

        public void SetPaused(bool paused)
        {
            if (IsPaused == paused)
                return;
            IsPaused = paused;
            MarkDirty();
        }

        public void SetGameResult(bool isGameOver, string winnerId)
        {
            IsGameOver = isGameOver;
            IsPaused = false;
            WinnerId = winnerId?.Trim() ?? string.Empty;
            OpenPanelId = GameplayHtmlPanel.None;
            MarkDirty();
        }

        public void SetFeedback(string feedback)
        {
            Feedback = feedback?.Trim() ?? string.Empty;
            _feedbackExpiresAt = string.IsNullOrEmpty(Feedback)
                ? -1f
                : Time.unscaledTime + 4f;
            MarkDirty();
        }

        public void ExpireFeedbackIfNeeded()
        {
            if (_feedbackExpiresAt < 0f || Time.unscaledTime < _feedbackExpiresAt)
                return;
            Feedback = string.Empty;
            _feedbackExpiresAt = -1f;
            MarkDirty();
        }

        public void MarkDirty()
        {
            Dirty = true;
            Changed?.Invoke();
        }

        public bool ConsumeDirty()
        {
            bool dirty = Dirty;
            Dirty = false;
            return dirty;
        }
    }

    internal readonly struct GameplayResourceSnapshot
    {
        public GameplayResourceSnapshot(string id, float amount)
        {
            Id = id ?? string.Empty;
            Amount = amount;
        }

        public string Id { get; }
        public float Amount { get; }
    }

    internal sealed class GameplayGuidanceOptionSnapshot
    {
        public GameplayGuidanceOptionSnapshot(GuidanceOptionKind kind,
            string buildingId, string resourceId, string detail,
            Vector2Int position, bool hasPosition)
        {
            Kind = kind;
            BuildingId = buildingId ?? string.Empty;
            ResourceId = resourceId ?? string.Empty;
            Detail = detail ?? string.Empty;
            Position = position;
            HasPosition = hasPosition;
        }

        public GuidanceOptionKind Kind { get; }
        public string BuildingId { get; }
        public string ResourceId { get; }
        public string Detail { get; }
        public Vector2Int Position { get; }
        public bool HasPosition { get; }
    }

    internal sealed class GameplayGuidanceBlockerSnapshot
    {
        public GameplayGuidanceBlockerSnapshot(GuidanceBlockerKind kind,
            string title, string detail, string resourceId,
            float required, float available, float reserved,
            bool resolved, GameplayGuidanceOptionSnapshot[] options)
        {
            Kind = kind;
            Title = title ?? string.Empty;
            Detail = detail ?? string.Empty;
            ResourceId = resourceId ?? string.Empty;
            Required = required;
            Available = available;
            Reserved = reserved;
            Resolved = resolved;
            Options = options ?? Array.Empty<GameplayGuidanceOptionSnapshot>();
        }

        public GuidanceBlockerKind Kind { get; }
        public string Title { get; }
        public string Detail { get; }
        public string ResourceId { get; }
        public float Required { get; }
        public float Available { get; }
        public float Reserved { get; }
        public bool Resolved { get; }
        public GameplayGuidanceOptionSnapshot[] Options { get; }
        public float Missing => Required > Available ? Required - Available : 0f;
        public string IconGlobalKey => GameplayHtmlIconKeys.Resource(ResourceId);
    }

    /// <summary>View snapshot for the action-guidance popup — rebuilt from
    /// canonical queries on every capture so fulfilled blockers drop out and
    /// numbers stay live.</summary>
    internal sealed class GameplayGuidanceViewSnapshot
    {
        public GuidanceGoalKind GoalKind;
        /// <summary>Display label for the saved goal (building/unit name).</summary>
        public string GoalLabel = string.Empty;
        public string GoalBuildingId = string.Empty;
        public string GoalUnitTypeId = string.Empty;
        public Vector2Int GoalPosition;
        public int PlacementCount;
        public int FocusIndex;
        public int TotalBlockers;
        public int PendingBlockers;
        public bool AllResolved;
        public GameplayGuidanceBlockerSnapshot[] Blockers =
            Array.Empty<GameplayGuidanceBlockerSnapshot>();
    }

    /// <summary>The saved intent behind a blocked action plus popup
    /// visibility — persists while the player fixes the blockers so the popup
    /// can be reopened and the original goal resumed.</summary>
    internal sealed class GuidanceSession
    {
        public GuidanceSession(GuidanceGoal goal)
        {
            Goal = goal;
        }

        public GuidanceGoal Goal { get; }
        public bool Open;
        /// <summary>Index of the focused blocker inside the popup.</summary>
        public int FocusIndex;
    }

    internal readonly struct GameplayNotificationViewSnapshot
    {
        public GameplayNotificationViewSnapshot(string message, string kind, long id = 0,
            Vector2Int? position = null, string targetId = null, long queueId = 0,
            string deliveryDetails = null, GuidanceGoal goal = null)
        {
            Id = id;
            Position = position;
            TargetId = targetId ?? string.Empty;
            QueueId = queueId;
            DeliveryDetails = deliveryDetails ?? string.Empty;
            Goal = goal;
            CreatedAt = DateTime.Now;
            Message = message ?? string.Empty;
            Kind = kind ?? "Info";
        }

        public long Id { get; }
        public DateTime CreatedAt { get; }
        public Vector2Int? Position { get; }
        public string TargetId { get; }
        public long QueueId { get; }
        public string DeliveryDetails { get; }
        /// <summary>Non-null when this notification can reopen the guidance
        /// popup for the blocked action it describes.</summary>
        public GuidanceGoal Goal { get; }
        public string Message { get; }
        public string Kind { get; }
    }

    internal readonly struct GameplayGroupSnapshot
    {
        public GameplayGroupSnapshot(string id, string label, int count, string context)
        {
            Id = id ?? string.Empty;
            Label = label ?? Id;
            Count = count;
            Context = context ?? string.Empty;
        }

        public string Id { get; }
        public string Label { get; }
        public int Count { get; }
        public string Context { get; }
    }

    internal readonly struct GameplayFactSnapshot
    {
        public GameplayFactSnapshot(string label, string value, string context)
        {
            Label = label ?? string.Empty;
            Value = value ?? string.Empty;
            Context = context ?? string.Empty;
        }

        public string Label { get; }
        public string Value { get; }
        public string Context { get; }
    }

    internal readonly struct GameplayBuildingOptionSnapshot
    {
        public GameplayBuildingOptionSnapshot(
            string id,
            string name,
            string category,
            string description,
            string cost,
            bool canSelect = true,
            string unavailableReason = null,
            int buildTurns = 0)
            : this(id, name, category, description, cost, null, canSelect, unavailableReason, buildTurns)
        {
        }

        public GameplayBuildingOptionSnapshot(
            string id,
            string name,
            string category,
            string description,
            string cost,
            Sprite icon,
            bool canSelect = true,
            string unavailableReason = null,
            int buildTurns = 0,
            string[] producedResourceIds = null)
        {
            Id = id ?? string.Empty;
            Name = string.IsNullOrWhiteSpace(name) ? Id : name;
            Category = category ?? string.Empty;
            Description = description ?? string.Empty;
            Cost = cost ?? string.Empty;
            Icon = icon;
            CanSelect = canSelect;
            UnavailableReason = unavailableReason ?? string.Empty;
            BuildTurns = Math.Max(0, buildTurns);
            ProducedResourceIds = producedResourceIds ?? Array.Empty<string>();
        }

        public int BuildTurns { get; }
        public string[] ProducedResourceIds { get; }
        public string Id { get; }
        public string Name { get; }
        public string Category { get; }
        public string Description { get; }
        public string Cost { get; }
        public Sprite Icon { get; }
        public bool HasIcon => Icon != null;
        public string IconGlobalKey => GameplayHtmlIconKeys.Building(Id);
        public bool CanSelect { get; }
        public string UnavailableReason { get; }
    }

    internal readonly struct GameplayWarehouseViewSnapshot
    {
        public GameplayWarehouseViewSnapshot(
            string id,
            string buildingId,
            string settlement,
            Vector2Int position,
            float used,
            int capacity,
            GameplayResourceSnapshot[] resources)
        {
            Id = id ?? string.Empty;
            BuildingId = buildingId ?? string.Empty;
            Settlement = settlement ?? string.Empty;
            Position = position;
            Used = used;
            Capacity = capacity;
            Resources = resources ?? Array.Empty<GameplayResourceSnapshot>();
        }

        public string Id { get; }
        public string BuildingId { get; }
        public string Settlement { get; }
        public Vector2Int Position { get; }
        public float Used { get; }
        public int Capacity { get; }
        public GameplayResourceSnapshot[] Resources { get; }
    }

    internal readonly struct GameplaySettlementViewSnapshot
    {
        public GameplaySettlementViewSnapshot(
            string id,
            string name,
            int population,
            int buildingCount,
            GameplayResourceSnapshot[] resources)
        {
            Id = id ?? string.Empty;
            Name = string.IsNullOrWhiteSpace(name) ? Id : name;
            Population = Math.Max(0, population);
            BuildingCount = Math.Max(0, buildingCount);
            Resources = resources ?? Array.Empty<GameplayResourceSnapshot>();
        }

        public string Id { get; }
        public string Name { get; }
        public int Population { get; }
        public int BuildingCount { get; }
        public GameplayResourceSnapshot[] Resources { get; }
    }

    internal readonly struct GameplayTurnHistoryViewSnapshot
    {
        public GameplayTurnHistoryViewSnapshot(string ownerId, long completed, bool active, bool local,
            bool eliminated = false)
        {
            OwnerId = ownerId ?? string.Empty;
            Completed = completed;
            Active = active;
            Local = local;
            Eliminated = eliminated;
        }

        public string OwnerId { get; }
        public long Completed { get; }
        public bool Active { get; }
        public bool Local { get; }
        public bool Eliminated { get; }
    }

    internal readonly struct GameplayRecruitmentRecipeSnapshot
    {
        public GameplayRecruitmentRecipeSnapshot(
            string unitTypeId,
            string name,
            string role,
            string combatType,
            string cost,
            int trainingTurns,
            int hitPoints,
            float movement,
            bool canRecruit,
            string unavailableReason)
            : this(
                unitTypeId,
                name,
                role,
                combatType,
                cost,
                trainingTurns,
                hitPoints,
                movement,
                null,
                canRecruit,
                unavailableReason)
        {
        }

        public GameplayRecruitmentRecipeSnapshot(
            string unitTypeId,
            string name,
            string role,
            string combatType,
            string cost,
            int trainingTurns,
            int hitPoints,
            float movement,
            Sprite icon,
            bool canRecruit,
            string unavailableReason,
            float trainingSeconds = 0f,
            int populationCost = 1,
            string[] missingResourceIds = null,
            string[] producerActionResourceIds = null)
        {
            PopulationCost = populationCost;
            TrainingSeconds = trainingSeconds;
            UnitTypeId = unitTypeId ?? string.Empty;
            Name = string.IsNullOrWhiteSpace(name) ? UnitTypeId : name;
            Role = role ?? string.Empty;
            CombatType = combatType ?? string.Empty;
            Cost = cost ?? string.Empty;
            TrainingTurns = Math.Max(1, trainingTurns);
            HitPoints = Math.Max(0, hitPoints);
            Movement = Math.Max(0f, movement);
            Icon = icon;
            CanRecruit = canRecruit;
            UnavailableReason = unavailableReason ?? string.Empty;
            MissingResourceIds = missingResourceIds ?? Array.Empty<string>();
            ProducerActionResourceIds = producerActionResourceIds;
        }

        public string[] MissingResourceIds { get; }
        /// <summary>Parallel to <see cref="MissingResourceIds"/>: the resource
        /// whose producers the player can actually build next for that
        /// shortage (a prerequisite resource when the direct producer is
        /// itself blocked). Null entries mean no achievable producer; a null
        /// array means feasibility was not evaluated.</summary>
        public string[] ProducerActionResourceIds { get; }
        public string UnitTypeId { get; }
        public string Name { get; }
        public string Role { get; }
        public string CombatType { get; }
        public string Cost { get; }
        public int TrainingTurns { get; }
        public float TrainingSeconds { get; }
        public int PopulationCost { get; }
        public int HitPoints { get; }
        public float Movement { get; }
        public Sprite Icon { get; }
        public bool HasIcon => Icon != null;
        public string IconGlobalKey => GameplayHtmlIconKeys.Unit(UnitTypeId);
        public bool CanRecruit { get; }
        public string UnavailableReason { get; }
    }

    internal static class GameplayHtmlIconKeys
    {
        public static string Building(string id) => $"gameplay_building_icon_{Sanitize(id)}";
        public static string Unit(string id) => $"gameplay_unit_icon_{Sanitize(id)}";
        public static string Resource(string id) => $"gameplay_resource_icon_{Sanitize(id)}";

        private static string Sanitize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "unknown";

            var builder = new StringBuilder(value.Length);
            for (int index = 0; index < value.Length; index++)
            {
                char c = value[index];
                builder.Append(char.IsLetterOrDigit(c) ? char.ToLowerInvariant(c) : '_');
            }

            return builder.Length == 0 ? "unknown" : builder.ToString();
        }
    }

    internal readonly struct GameplayLogisticsEntrySnapshot
    {
        public GameplayLogisticsEntrySnapshot(string kind, string id, string title, string detail,
            Vector2Int? focusPosition)
        {
            Kind = kind ?? string.Empty;
            Id = id ?? string.Empty;
            Title = title ?? string.Empty;
            Detail = detail ?? string.Empty;
            FocusPosition = focusPosition;
        }

        public string Kind { get; }
        public string Id { get; }
        public string Title { get; }
        public string Detail { get; }
        public Vector2Int? FocusPosition { get; }
    }

    internal readonly struct GameplayRecruitmentQueueSnapshot
    {
        public GameplayRecruitmentQueueSnapshot(
            long queueId,
            string unitTypeId,
            string name,
            int completedTurns,
            int trainingTurns,
            bool ready,
            bool waiting = false,
            float remainingSeconds = 0f)
        {
            RemainingSeconds = remainingSeconds;
            Waiting = waiting;
            QueueId = queueId;
            UnitTypeId = unitTypeId ?? string.Empty;
            Name = string.IsNullOrWhiteSpace(name) ? UnitTypeId : name;
            CompletedTurns = Math.Max(0, completedTurns);
            TrainingTurns = Math.Max(1, trainingTurns);
            Ready = ready;
        }

        public long QueueId { get; }
        public string UnitTypeId { get; }
        public string Name { get; }
        public int CompletedTurns { get; }
        public int TrainingTurns { get; }
        public bool Ready { get; }
        public bool Waiting { get; }
        public float RemainingSeconds { get; }
    }

    internal sealed class GameplayHtmlSnapshot
    {
        public IReadOnlyDictionary<string, Sprite> Icons = new Dictionary<string, Sprite>();
        public string OwnerId = "player_0";
        public string KingdomName = "Your Kingdom";
        public int Round = 1;
        public long GlobalTurn = 1;
        public int ActionsThisTurn;
        public string ActiveOwnerId = "player_0";
        public bool IsLocalTurn = true;
        public bool TurnUiEnabled = true;
        public bool SandboxRealtime;
        public float SandboxSpeed = 1f;
        public double SandboxElapsedSeconds;
        public float SandboxSecondsUntilNextProgress;
        public float SandboxRoundSeconds = 10f;
        public bool CanIssueLocalCommands => !TurnUiEnabled || IsLocalTurn;
        public bool EndTurnPending;
        public int SettlementCount;
        public int Population;
        public int BuildingCount;
        public int UnitCount;
        public bool RequiresFirstCastle;
        public string CastleBuildingId = "castle";
        public string SelectedBuildingId = string.Empty;
        public int PendingPlacementCount;
        public string PlacementStatus = "Choose a location on the map.";
        public bool PlacementValid;
        public string SelectionKind = string.Empty;
        public string SelectionId = string.Empty;
        public string SelectionTitle = string.Empty;
        public string SelectionSubtitle = string.Empty;
        public Vector2Int SelectionPosition;
        public GameplayFactSnapshot[] SelectionFacts = Array.Empty<GameplayFactSnapshot>();
        public bool SelectionOwnedByLocalPlayer;
        public bool SelectionOperational;
        public string AttackSourceId = string.Empty;
        public bool CanAttackSelection;
        public string AttackPreview = string.Empty;
        public string AttackUnavailableReason = string.Empty;
        public bool CanCaptureSelection;
        public string CapturePreview = string.Empty;
        public string CaptureUnavailableReason = string.Empty;
        public string SelectedUnitGroupId = string.Empty;
        public int SelectedUnitGroupSize;
        public string SelectedUnitGroupMembers = string.Empty;
        public bool GroupMergeArmed;
        public bool SupportsRecruitment;
        public GameplayCargoSnapshot Cargo;
        public int RecruitmentQueueCapacity;
        public GameplayRecruitmentRecipeSnapshot[] RecruitmentRecipes = Array.Empty<GameplayRecruitmentRecipeSnapshot>();
        public GameplayRecruitmentQueueSnapshot[] RecruitmentQueue = Array.Empty<GameplayRecruitmentQueueSnapshot>();
        public GameplayResourceSnapshot[] Resources = Array.Empty<GameplayResourceSnapshot>();
        public GameplayBuildingOptionSnapshot[] BuildingOptions = Array.Empty<GameplayBuildingOptionSnapshot>();
        public GameplayGroupSnapshot[] BuildingGroups = Array.Empty<GameplayGroupSnapshot>();
        public GameplayGroupSnapshot[] UnitGroups = Array.Empty<GameplayGroupSnapshot>();
        public GameplayWarehouseViewSnapshot[] Warehouses = Array.Empty<GameplayWarehouseViewSnapshot>();
        public GameplaySettlementViewSnapshot[] Settlements = Array.Empty<GameplaySettlementViewSnapshot>();
        public GameplayTurnHistoryViewSnapshot[] TurnHistory = Array.Empty<GameplayTurnHistoryViewSnapshot>();
        public GameplaySupplySnapshot Supply;
        public GameplayLogisticsEntrySnapshot[] Logistics = Array.Empty<GameplayLogisticsEntrySnapshot>();
        public bool HasPendingSupplyDeficit;
        /// <summary>Non-null while a guidance session exists — blockers are
        /// rebuilt from canonical queries each capture.</summary>
        public GameplayGuidanceViewSnapshot Guidance;
        public Vector2Int PendingSupplyPosition;
        public string PendingSupplyBuildingId = string.Empty;

        internal static GameplayHtmlSnapshot CreatePreview(GameplayHtmlAnchor.PreviewScreen screen)
        {
            var snapshot = new GameplayHtmlSnapshot
            {
                KingdomName = "House Velym",
                Round = 4,
                GlobalTurn = 11,
                ActionsThisTurn = 2,
                TurnUiEnabled = screen != GameplayHtmlAnchor.PreviewScreen.Normal,
                SettlementCount = 2,
                Population = 38,
                BuildingCount = 14,
                UnitCount = 7,
                RequiresFirstCastle = screen == GameplayHtmlAnchor.PreviewScreen.FirstCastle,
                SelectedBuildingId = screen == GameplayHtmlAnchor.PreviewScreen.Construction ? "lumber-camp" : "castle",
                PendingPlacementCount = screen == GameplayHtmlAnchor.PreviewScreen.FirstCastle ? 1 : 0,
                PlacementStatus = "Valid location. The castle can be founded here.",
                PlacementValid = true,
                SelectionKind = screen == GameplayHtmlAnchor.PreviewScreen.Selection
                    || screen == GameplayHtmlAnchor.PreviewScreen.Recruitment ? "Building" : string.Empty,
                SelectionId = screen == GameplayHtmlAnchor.PreviewScreen.Recruitment ? "barracks" : "castle",
                SelectionTitle = screen == GameplayHtmlAnchor.PreviewScreen.Recruitment ? "Barracks" : "Castle",
                SelectionSubtitle = screen == GameplayHtmlAnchor.PreviewScreen.Recruitment
                    ? "Trains military units." : "Settlement seat and administrative center.",
                SelectionPosition = new Vector2Int(42, 27),
                SelectionFacts = new[]
                {
                    new GameplayFactSnapshot("Ownership", "Your kingdom", "Command authority"),
                    new GameplayFactSnapshot("Status", "Operational", "Current state"),
                },
                SelectionOwnedByLocalPlayer = true,
                SelectionOperational = true,
                SupportsRecruitment = screen == GameplayHtmlAnchor.PreviewScreen.Recruitment,
                RecruitmentQueueCapacity = 3,
                RecruitmentRecipes = screen == GameplayHtmlAnchor.PreviewScreen.Recruitment
                    ? new[]
                    {
                        new GameplayRecruitmentRecipeSnapshot("guard", "Guard", "Military", "Infantry", "Food 20 / Gold 5", 2, 120, 4f, true, string.Empty),
                        new GameplayRecruitmentRecipeSnapshot("archer", "Archer", "Military", "Infantry", "Food 16 / Wood 8", 2, 80, 5f, true, string.Empty),
                    }
                    : Array.Empty<GameplayRecruitmentRecipeSnapshot>(),
                RecruitmentQueue = screen == GameplayHtmlAnchor.PreviewScreen.Recruitment
                    ? new[]
                    {
                        new GameplayRecruitmentQueueSnapshot(1, "guard", "Guard", 1, 2, false),
                    }
                    : Array.Empty<GameplayRecruitmentQueueSnapshot>(),
                Resources = new[]
                {
                    new GameplayResourceSnapshot("food", 426),
                    new GameplayResourceSnapshot("wood", 318),
                    new GameplayResourceSnapshot("gold", 92),
                },
                BuildingOptions = new[]
                {
                    new GameplayBuildingOptionSnapshot("castle", "Castle", "Settlement", "Founds and governs a settlement.", "Wood 120 / Gold 40"),
                    new GameplayBuildingOptionSnapshot("lumber-camp", "Lumber Camp", "Industry", "Produces construction timber.", "Wood 35"),
                    new GameplayBuildingOptionSnapshot("warehouse", "Warehouse", "Economy", "Stores settlement resources.", "Wood 55"),
                    new GameplayBuildingOptionSnapshot("barracks", "Barracks", "Military", "Recruits military units.", "Wood 80 / Gold 25"),
                },
                BuildingGroups = new[]
                {
                    new GameplayGroupSnapshot("castle", "Castle", 2, "2 settlements"),
                    new GameplayGroupSnapshot("warehouse", "Warehouse", 4, "Northhold, Rivergate"),
                    new GameplayGroupSnapshot("lumber-camp", "Lumber Camp", 3, "Northhold"),
                },
                UnitGroups = new[]
                {
                    new GameplayGroupSnapshot("worker", "Workers", 4, "Available 2"),
                    new GameplayGroupSnapshot("guard", "Guards", 3, "Ready"),
                },
                Warehouses = new[]
                {
                    new GameplayWarehouseViewSnapshot("42:27", "warehouse", "Northhold", new Vector2Int(42, 27), 186, 250,
                        new[] { new GameplayResourceSnapshot("food", 110), new GameplayResourceSnapshot("wood", 76) }),
                    new GameplayWarehouseViewSnapshot("67:31", "warehouse", "Rivergate", new Vector2Int(67, 31), 98, 200,
                        new[] { new GameplayResourceSnapshot("food", 44), new GameplayResourceSnapshot("gold", 54) }),
                },
                Settlements = new[]
                {
                    new GameplaySettlementViewSnapshot("northhold", "Northhold", 24, 8,
                        new[] { new GameplayResourceSnapshot("food", 110), new GameplayResourceSnapshot("wood", 76) }),
                    new GameplaySettlementViewSnapshot("rivergate", "Rivergate", 14, 6,
                        new[] { new GameplayResourceSnapshot("food", 44), new GameplayResourceSnapshot("gold", 54) }),
                },
                TurnHistory = new[]
                {
                    new GameplayTurnHistoryViewSnapshot("player_0", 5, true, true),
                    new GameplayTurnHistoryViewSnapshot("player_1", 5, false, false),
                },
            };
            if (screen == GameplayHtmlAnchor.PreviewScreen.Cargo
                || screen == GameplayHtmlAnchor.PreviewScreen.Route)
            {
                snapshot.SelectionKind = "Unit";
                snapshot.SelectionId = "caravan-wagon-preview";
                snapshot.SelectionTitle = "Caravan Wagon";
                snapshot.SelectionSubtitle = "Transport";
                snapshot.SelectionPosition = new Vector2Int(41, 27);
                snapshot.SelectedBuildingId = string.Empty;
                snapshot.SupportsRecruitment = false;
                snapshot.Cargo = GameplayCargoSnapshot.CreatePreview();
                if (screen == GameplayHtmlAnchor.PreviewScreen.Route)
                    snapshot.Cargo.RouteAvailability = Kruty1918.Moyva.Economy.API.CaravanTransferResult.Success();
            }
            return snapshot;
        }
    }

}
