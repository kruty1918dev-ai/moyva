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
        private void SelectFirstRecipeIfAvailable()
        {
            for (int index = 0; index < _recipeDefinitions.Count; index++)
            {
                if (_recipeDefinitions[index] == null)
                    continue;

                _selectedRecruitmentRecipe = _recipeDefinitions[index];
                _selectedRecipeIndex = index;
                _selectionFromQueue = false;
                _selectedQueueId = 0;
                _recruitmentView.SetSelectedRecipeIndex(index);
                RefreshSelectionDetails(
                    _selectedRecruitmentRecipe.UnitTypeId,
                    _selectedRecruitmentRecipe,
                    null);
                return;
            }

            _selectedRecruitmentRecipe = null;
            _selectedRecipeIndex = -1;
            _selectionFromQueue = false;
            _selectedQueueId = 0;
            _recruitmentView.SetSelectedRecipeIndex(-1);
            _recruitmentView.SelectionPanel.SetActive(false);
            _recruitmentView.ClearStatRows();
            _recruitmentView.SetActionHint(string.Empty);
            ClearCostRows();
        }

        private UnitRecruitmentRecipeDefinition ResolveRecipe(string unitTypeId)
        {
            if (_selectedRecruitmentModule?.Recipes == null || string.IsNullOrWhiteSpace(unitTypeId))
                return null;

            for (int index = 0; index < _selectedRecruitmentModule.Recipes.Count; index++)
            {
                UnitRecruitmentRecipeDefinition recipe = _selectedRecruitmentModule.Recipes[index];
                if (recipe != null
                    && string.Equals(recipe.UnitTypeId?.Trim(), unitTypeId.Trim(), StringComparison.Ordinal))
                {
                    return recipe;
                }
            }

            return null;
        }

        private int FindRecipeIndex(string unitTypeId)
        {
            if (string.IsNullOrWhiteSpace(unitTypeId))
                return -1;

            string normalized = unitTypeId.Trim();
            for (int index = 0; index < _recipeUnitTypeIds.Count; index++)
            {
                if (string.Equals(
                        _recipeUnitTypeIds[index]?.Trim(),
                        normalized,
                        StringComparison.Ordinal))
                {
                    return index;
                }
            }

            return -1;
        }

        private string ResolveRecruitmentActionHint(
            bool ownsBuilding,
            bool operational,
            bool queueFull,
            bool canRecruit)
        {
            if (_selectionFromQueue)
            {
                UnitRecruitmentQueueItemSnapshot? selected =
                    FindVisibleQueueItem(_selectedQueueId);
                if (selected.HasValue && selected.Value.IsReady)
                    return "Готовий юніт розміщується через індикатор біля будівлі.";
                return _selectedQueueId > 0 ? "Цей запис уже в черзі." : string.Empty;
            }

            if (!ownsBuilding)
                return "Найм доступний лише у власній будівлі.";
            if (!operational)
                return "Будівля будується.";
            if (!_authority.CanIssueLocalCommands)
                return "Зараз не ваш хід.";
            if (_selectedRecruitmentRecipe == null)
                return "Оберіть юніта.";
            if (queueFull)
                return "Черга заповнена.";

            return canRecruit ? string.Empty : "Найм зараз недоступний.";
        }
    }
}
