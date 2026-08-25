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
        private void RefreshRecruitmentAuthority()
        {
            if (_recruitmentPanel == null || !_recruitmentPanel.activeSelf)
                return;

            bool ownsBuilding = IsSelectedBuildingOwnedByLocalPlayer();
            bool operational = IsSelectedBuildingOperational();
            bool queueFull = false;
            if (_selectedBuilding.HasValue && _selectedRecruitmentModule != null)
            {
                IReadOnlyList<UnitRecruitmentQueueItemSnapshot> queue =
                    _recruitment.GetQueue(_turns.LocalOwnerId, _selectedBuilding.Value);
                queueFull = (queue?.Count ?? 0)
                    >= Mathf.Max(1, _selectedRecruitmentModule.QueueCapacity);
            }

            bool canRecruit = _authority.CanIssueLocalCommands
                && ownsBuilding
                && operational
                && !_selectionFromQueue
                && _selectedRecruitmentRecipe != null
                && !queueFull;

            for (int index = 0; index < _recipeButtons.Count; index++)
            {
                Button button = _recipeButtons[index];
                if (button != null)
                {
                    // Recipe cards remain inspectable outside the local action phase.
                    button.interactable = ownsBuilding
                        && !string.IsNullOrWhiteSpace(_recipeUnitTypeIds[index]);
                }
            }

            if (_recruitmentView != null)
            {
                _recruitmentView.HireButton.interactable = canRecruit;
                _recruitmentView.HireButtonLabel.text = "Найняти";
                _recruitmentView.SetActionHint(ResolveRecruitmentActionHint(
                    ownsBuilding,
                    operational,
                    queueFull,
                    canRecruit));
            }
        }

    }
}
