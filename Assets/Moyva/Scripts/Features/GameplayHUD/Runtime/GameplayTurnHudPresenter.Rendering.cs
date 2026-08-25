using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.UIActions.API;
using Kruty1918.Moyva.Units.API;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal sealed partial class GameplayTurnHudPresenter
    {
        private void RefreshTurnAuthority()
        {
            _authority = GameplayTurnHudAuthorityPolicy.Evaluate(_turns, _blockers);
            string owner = string.IsNullOrWhiteSpace(_authority.ActiveOwnerId)
                ? "—"
                : _authority.ActiveOwnerId;

            _turnText.text =
                $"Фракція: {owner}    Раунд {_turns.Round}    Хід {_turns.GlobalTurn}\n" +
                $"Фаза {GameplayTurnHudAuthorityPolicy.LocalizePhase(_authority.Phase)}    Дії {_turns.ActionsThisTurn}";
            _endTurnButton.interactable = _authority.CanEndTurn;

            bool blockersTakePriority = _authority.IsLocalOwnerTurn
                && _authority.BlockingReasons.Count > 0;
            _statusText.text = blockersTakePriority || string.IsNullOrWhiteSpace(_statusOverride)
                ? _authority.StatusText
                : _statusOverride;

            RefreshRecruitmentAuthority();
        }

        private string ActiveHudContext()
            => _recruitmentPanel != null && _recruitmentPanel.activeSelf
                ? "RecruitmentPanel"
                : "Gameplay";

        private void RefreshCostRows(UnitRecruitmentRecipeDefinition recipe)
        {
            ClearCostRows();
            if (_recruitmentView == null || recipe?.Costs == null)
                return;

            int visible = 0;
            for (int index = 0; index < recipe.Costs.Count && visible < _recruitmentView.CostRowCount; index++)
            {
                BuildingResourceAmount cost = recipe.Costs[index];
                if (cost == null || string.IsNullOrWhiteSpace(cost.ResourceId) || cost.Amount <= 0f)
                    continue;

                ResolveResourcePresentation(cost.ResourceId, out _, out Sprite icon);
                GameObject row = _recruitmentView.GetCostRow(visible);
                Image rowIcon = _recruitmentView.GetCostIcon(visible);
                TMP_Text rowLabel = _recruitmentView.GetCostLabel(visible);

                row.SetActive(true);
                rowIcon.sprite = icon;
                rowIcon.enabled = icon != null;
                rowLabel.text = $"{cost.Amount:0.#}";
                visible++;
            }
        }
    }
}
