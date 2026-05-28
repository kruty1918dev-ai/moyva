using System;
using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.Construction.Runtime;
using Kruty1918.Moyva.Editor.Shared;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.HomeMenu.UI;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.HomeMenu.Editor
{
    [CustomEditor(typeof(HomeMenuBackgroundPreviewController))]
    public sealed class HomeMenuBackgroundPreviewControllerEditor : UnityEditor.Editor
    {
        private static bool _kingdomFoldout = true;

        private SerializedProperty _kingdomPlacementProp;
        private SerializedProperty _tileRegistryOverrideProp;
        private SerializedProperty _buildingRegistryProp;

        // Instance caches (rebuild only when registry reference changes)
        private TileRegistrySO             _lastTileReg;
        private string[]                   _cachedTileIds   = Array.Empty<string>();
        private Dictionary<string, Sprite> _tileSpriteCache = new();
        private Dictionary<string, GameObject> _tilePrefabCache = new();

        private BuildingRegistrySO         _lastBldReg;
        private string[]                   _cachedBldIds    = Array.Empty<string>();
        private Dictionary<string, Sprite> _bldSpriteCache  = new();
        private Dictionary<string, GameObject> _bldPrefabCache = new();
        private Dictionary<string, string> _bldDisplayCache = new();

        private void OnEnable()
        {
            _kingdomPlacementProp     = serializedObject.FindProperty("_kingdomPlacement");
            _tileRegistryOverrideProp = serializedObject.FindProperty("_tileRegistryOverride");
            _buildingRegistryProp     = serializedObject.FindProperty("_buildingRegistry");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "_kingdomPlacement");
            EditorGUILayout.Space(2f);
            DrawKingdomPlacement();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawKingdomPlacement()
        {
            if (_kingdomPlacementProp == null)
            {
                EditorGUILayout.HelpBox("_kingdomPlacement not found", MessageType.Error);
                return;
            }

            _kingdomFoldout = EditorGUILayout.Foldout(
                _kingdomFoldout, "Kingdom Placement (Preview Only)", true, EditorStyles.foldoutHeader);
            if (!_kingdomFoldout) return;

            EditorGUI.indentLevel++;

            var iter    = _kingdomPlacementProp.Copy();
            var endProp = _kingdomPlacementProp.GetEndProperty();
            bool enterChildren = true;

            while (iter.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iter, endProp))
            {
                enterChildren = false;
                switch (iter.name)
                {
                    case "CastleBuildingId":
                    case "TownHallBuildingId":
                    case "WarehouseBuildingId":
                    case "LocalSettlementBuildingId":
                        DrawBuildingIdField(iter.Copy());
                        break;
                    case "ForbiddenBiomeTileIds":
                        DrawForbiddenTilesField(iter.Copy());
                        break;
                    default:
                        EditorGUILayout.PropertyField(iter.Copy(), true);
                        break;
                }
            }

            EditorGUI.indentLevel--;
        }

        private void DrawForbiddenTilesField(SerializedProperty prop)
        {
            var tileReg = _tileRegistryOverrideProp?.objectReferenceValue as TileRegistrySO;
            if (tileReg == null)
            {
                EditorGUILayout.HelpBox(
                    "Встановіть «Tile Registry Override» на компоненті, щоб обирати тайли.",
                    MessageType.Warning);
                return;
            }

            RefreshTileCache(tileReg);

            int count = prop.arraySize;

            Rect headerRect = EditorGUILayout.GetControlRect(true);
            var  label      = new GUIContent(prop.displayName, prop.tooltip);
            Rect labelRect  = EditorGUI.PrefixLabel(headerRect, label);
            Rect btnRect    = new Rect(labelRect.xMax - 80f, headerRect.y, 80f, headerRect.height);

            string btnLabel = count == 0 ? "Обрати..." : $"Змінити ({count})";
            if (EditorGUI.DropdownButton(btnRect, new GUIContent(btnLabel), FocusType.Keyboard))
            {
                var current = new HashSet<string>();
                for (int i = 0; i < prop.arraySize; i++)
                    current.Add(prop.GetArrayElementAtIndex(i).stringValue);

                var    serObj = prop.serializedObject;
                string path   = prop.propertyPath;

                PopupWindow.Show(btnRect, new MultiTilePickerPopup(
                    new List<string>(_cachedTileIds),
                    _tileSpriteCache,
                    _tilePrefabCache,
                    current,
                    finalSet =>
                    {
                        serObj.Update();
                        var p = serObj.FindProperty(path);
                        if (p == null) return;
                        p.arraySize = finalSet.Count;
                        for (int i = 0; i < finalSet.Count; i++)
                            p.GetArrayElementAtIndex(i).stringValue = finalSet[i];
                        serObj.ApplyModifiedProperties();
                    }));
            }

            if (count == 0)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("(порожньо)", EditorStyles.miniLabel);
                EditorGUI.indentLevel--;
                return;
            }

            EditorGUI.indentLevel++;
            int removeIdx = -1;
            for (int i = 0; i < count; i++)
            {
                var    elemProp = prop.GetArrayElementAtIndex(i);
                string elemId   = elemProp.stringValue;
                Sprite spr      = _tileSpriteCache.TryGetValue(elemId, out var ts) ? ts : null;
                GameObject prefab = _tilePrefabCache.TryGetValue(elemId, out var tp) ? tp : null;

                Rect  rowRect  = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
                float iconSize = rowRect.height;

                Rect xRect = new Rect(rowRect.xMax - 20f, rowRect.y, 20f, rowRect.height);
                GUI.color = new Color(1f, 0.5f, 0.5f);
                if (GUI.Button(xRect, "x", EditorStyles.miniButton))
                    removeIdx = i;
                GUI.color = Color.white;

                if ((spr != null && spr.texture != null) || prefab != null)
                {
                    Rect iRect = new Rect(rowRect.x, rowRect.y, iconSize, iconSize);
                    AdaptivePrefabPreviewUtility.DrawPrefabOrSprite(iRect, prefab, spr);
                    Rect tRect = new Rect(rowRect.x + iconSize + 3f, rowRect.y,
                        rowRect.width - iconSize - 25f, rowRect.height);
                    EditorGUI.LabelField(tRect, elemId, EditorStyles.label);
                }
                else
                {
                    Rect tRect = new Rect(rowRect.x, rowRect.y, rowRect.width - 24f, rowRect.height);
                    EditorGUI.LabelField(tRect, elemId, EditorStyles.label);
                }
            }
            EditorGUI.indentLevel--;

            if (removeIdx >= 0)
            {
                prop.DeleteArrayElementAtIndex(removeIdx);
                prop.serializedObject.ApplyModifiedProperties();
            }
        }

        private void DrawBuildingIdField(SerializedProperty prop)
        {
            var bldReg = _buildingRegistryProp?.objectReferenceValue as BuildingRegistrySO;
            if (bldReg == null)
            {
                EditorGUILayout.HelpBox(
                    "Встановіть «Building Registry» на компоненті, щоб обирати будівлі.",
                    MessageType.Warning);
                return;
            }

            RefreshBldCache(bldReg);

            string currentValue = prop.stringValue;
            bool isValid = !string.IsNullOrEmpty(currentValue)
                           && Array.IndexOf(_cachedBldIds, currentValue) >= 0;

            Color prevColor = GUI.color;
            if (!isValid && !string.IsNullOrEmpty(currentValue))
                GUI.color = Color.red;

            Rect lineRect    = EditorGUILayout.GetControlRect(true);
            float spriteSize  = lineRect.height;
            Rect fieldRect   = new Rect(lineRect.x, lineRect.y, lineRect.width - spriteSize - 4f, lineRect.height);
            Rect spriteRect  = new Rect(lineRect.xMax - spriteSize, lineRect.y, spriteSize, spriteSize);

            var label = new GUIContent(prop.displayName, prop.tooltip);
            EditorGUI.BeginProperty(lineRect, label, prop);
            Rect dropdownRect = EditorGUI.PrefixLabel(fieldRect, label);

            string btnText = string.IsNullOrEmpty(currentValue)
                ? "(none)"
                : isValid ? currentValue : $"! {currentValue}";

            var    serObj   = prop.serializedObject;
            string propPath = prop.propertyPath;

            var idsSnap     = _cachedBldIds;
            var spritesSnap = _bldSpriteCache;
            var prefabsSnap = _bldPrefabCache;
            var displaySnap = _bldDisplayCache;

            if (EditorGUI.DropdownButton(dropdownRect, new GUIContent(btnText), FocusType.Keyboard))
            {
                PopupWindow.Show(dropdownRect, new BuildingPickerPopup(
                    currentValue, idsSnap, spritesSnap, prefabsSnap, displaySnap,
                    selectedId =>
                    {
                        serObj.Update();
                        var p = serObj.FindProperty(propPath);
                        if (p != null) { p.stringValue = selectedId; serObj.ApplyModifiedProperties(); }
                    }));
            }

            GUI.color = prevColor;

            _bldSpriteCache.TryGetValue(currentValue ?? string.Empty, out Sprite icon);
            _bldPrefabCache.TryGetValue(currentValue ?? string.Empty, out GameObject prefabIcon);
            if ((icon != null && icon.texture != null) || prefabIcon != null)
                AdaptivePrefabPreviewUtility.DrawPrefabOrSprite(spriteRect, prefabIcon, icon);

            EditorGUI.EndProperty();

            if (!isValid && !string.IsNullOrEmpty(currentValue))
                EditorGUILayout.HelpBox($"ID '{currentValue}' not found in BuildingRegistry!", MessageType.Warning);
        }

        private void RefreshTileCache(TileRegistrySO reg)
        {
            if (reg == _lastTileReg) return;
            _lastTileReg = reg;
            _tileSpriteCache.Clear();
            _tilePrefabCache.Clear();

            if (reg?.Definitions == null) { _cachedTileIds = Array.Empty<string>(); return; }

            var list = new List<string>();
            foreach (var def in reg.Definitions)
            {
                if (string.IsNullOrEmpty(def.Id)) continue;
                list.Add(def.Id);
                if (def.VisualPrefab != null)
                {
                    _tilePrefabCache[def.Id] = def.VisualPrefab;
                    if (AdaptivePrefabPreviewUtility.TryGetPrimarySprite(def.VisualPrefab, out var sprite, out _))
                        _tileSpriteCache[def.Id] = sprite;
                }
            }
            list.Sort();
            _cachedTileIds = list.ToArray();
        }

        private void RefreshBldCache(BuildingRegistrySO reg)
        {
            if (reg == _lastBldReg) return;
            _lastBldReg = reg;
            _bldSpriteCache.Clear();
            _bldPrefabCache.Clear();
            _bldDisplayCache.Clear();

            if (reg?.Buildings == null) { _cachedBldIds = Array.Empty<string>(); return; }

            var list = new List<string>();
            foreach (var def in reg.Buildings)
            {
                if (string.IsNullOrEmpty(def?.Id)) continue;
                list.Add(def.Id);
                _bldDisplayCache[def.Id] = string.IsNullOrWhiteSpace(def.DisplayName) ? def.Id : def.DisplayName;
                if (def.Prefab != null)
                    _bldPrefabCache[def.Id] = def.Prefab;

                if (def.Icon != null) _bldSpriteCache[def.Id] = def.Icon;
                else if (def.Prefab != null)
                {
                    if (AdaptivePrefabPreviewUtility.TryGetPrimarySprite(def.Prefab, out var sprite, out _))
                        _bldSpriteCache[def.Id] = sprite;
                }
            }
            list.Sort();
            _cachedBldIds = list.ToArray();
        }

        private sealed class MultiTilePickerPopup : PopupWindowContent
        {
            private const float PopupWidth  = 560f;
            private const float PopupHeight = 520f;
            private const float SearchH     = 20f;
            private const float BtnH        = 24f;
            private const float RowH        = 36f;
            private const float IconSize    = 28f;
            private const float Padding     = 6f;
            private const string SearchCtrl = "ForbiddenTilesSearch";

            private readonly List<string>               _ids;
            private readonly Dictionary<string, Sprite> _sprites;
            private readonly Dictionary<string, GameObject> _prefabs;
            private readonly Action<List<string>>       _onApply;
            private readonly HashSet<string>            _checked;

            private string  _search      = string.Empty;
            private Vector2 _scroll;
            private bool    _focusSearch = true;

            public MultiTilePickerPopup(
                List<string> allIds,
                Dictionary<string, Sprite> sprites,
                Dictionary<string, GameObject> prefabs,
                HashSet<string> current,
                Action<List<string>> onApply)
            {
                _ids     = allIds;
                _sprites = sprites;
                _prefabs = prefabs;
                _checked = new HashSet<string>(current);
                _onApply = onApply;
            }

            public override Vector2 GetWindowSize() => new Vector2(PopupWidth, PopupHeight);
            public override void OnOpen() { _focusSearch = true; }

            public override void OnGUI(Rect rect)
            {
                Rect searchRect = new Rect(Padding, Padding, rect.width - Padding * 2f, SearchH);
                GUI.SetNextControlName(SearchCtrl);
                EditorGUI.BeginChangeCheck();
                _search = EditorGUI.TextField(searchRect, _search, EditorStyles.toolbarSearchField);
                if (EditorGUI.EndChangeCheck()) _scroll = Vector2.zero;
                if (_focusSearch) { _focusSearch = false; EditorGUI.FocusTextInControl(SearchCtrl); }

                var filtered = GetFiltered();

                float btnY  = searchRect.yMax + Padding;
                float halfW = (rect.width - Padding * 3f) * 0.5f;
                Rect selAllRect = new Rect(Padding,                   btnY, halfW, BtnH);
                Rect clearRect  = new Rect(Padding + halfW + Padding, btnY, halfW, BtnH);

                GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f);
                if (GUI.Button(selAllRect, $"Вибрати всі ({filtered.Count})"))
                    foreach (var id in filtered) _checked.Add(id);

                GUI.backgroundColor = new Color(0.8f, 0.4f, 0.4f);
                if (GUI.Button(clearRect, "Зняти всі (фільтр)"))
                    foreach (var id in filtered) _checked.Remove(id);

                GUI.backgroundColor = Color.white;

                float listTop    = btnY + BtnH + Padding;
                float listH      = rect.height - listTop - BtnH - Padding * 3f;
                Rect viewRect    = new Rect(Padding, listTop, rect.width - Padding * 2f, listH);
                Rect contentRect = new Rect(0f, 0f, viewRect.width - 14f,
                    Mathf.Max(listH, filtered.Count * RowH));

                _scroll = GUI.BeginScrollView(viewRect, _scroll, contentRect);

                for (int i = 0; i < filtered.Count; i++)
                {
                    string id      = filtered[i];
                    Rect rowRect   = new Rect(0f, i * RowH, contentRect.width, RowH - 1f);
                    bool isChecked = _checked.Contains(id);

                    if (isChecked)
                        EditorGUI.DrawRect(rowRect, new Color(0.24f, 0.48f, 0.24f, 0.55f));
                    else if (rowRect.Contains(Event.current.mousePosition))
                        EditorGUI.DrawRect(rowRect, new Color(1f, 1f, 1f, 0.07f));

                    Rect toggleRect = new Rect(rowRect.x + 2f, rowRect.y + (RowH - 16f) * 0.5f, 16f, 16f);
                    bool newChecked = GUI.Toggle(toggleRect, isChecked, GUIContent.none);
                    if (newChecked != isChecked)
                    {
                        if (newChecked) _checked.Add(id);
                        else _checked.Remove(id);
                    }

                    Rect iconRect = new Rect(toggleRect.xMax + 4f,
                        rowRect.y + (RowH - IconSize) * 0.5f, IconSize, IconSize);
                    _sprites.TryGetValue(id, out Sprite spr);
                    _prefabs.TryGetValue(id, out GameObject prefab);
                    if ((spr != null && spr.texture != null) || prefab != null)
                        AdaptivePrefabPreviewUtility.DrawPrefabOrSprite(iconRect, prefab, spr);
                    else
                    {
                        GUI.Box(iconRect, string.Empty);
                    }

                    Rect labelRect = new Rect(iconRect.xMax + 5f, rowRect.y,
                        rowRect.width - iconRect.xMax - 6f, RowH);
                    GUI.Label(labelRect, id, EditorStyles.label);

                    if (Event.current.type == EventType.MouseDown &&
                        rowRect.Contains(Event.current.mousePosition))
                    {
                        if (_checked.Contains(id)) _checked.Remove(id);
                        else _checked.Add(id);
                        Event.current.Use();
                    }
                }

                GUI.EndScrollView();

                Rect applyRect = new Rect(Padding, rect.height - BtnH - Padding,
                    rect.width - Padding * 2f, BtnH);
                GUI.backgroundColor = new Color(0.2f, 0.7f, 0.2f);
                if (GUI.Button(applyRect, $"Застосувати ({_checked.Count} тайлів)"))
                {
                    var sorted = _checked.OrderBy(x => x).ToList();
                    _onApply?.Invoke(sorted);
                    editorWindow.Close();
                }
                GUI.backgroundColor = Color.white;
            }

            private List<string> GetFiltered()
            {
                if (string.IsNullOrWhiteSpace(_search)) return new List<string>(_ids);
                var terms = _search.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                return _ids.Where(id =>
                    terms.All(t => id.IndexOf(t, StringComparison.OrdinalIgnoreCase) >= 0))
                    .ToList();
            }
        }

        private sealed class BuildingPickerPopup : PopupWindowContent
        {
            private const float PopupWidth  = 520f;
            private const float PopupHeight = 460f;
            private const float SearchH     = 20f;
            private const float RowH        = 32f;
            private const float IconSize    = 24f;
            private const float Padding     = 6f;
            private const string SearchCtrl = "MenuBldPickerSearch";

            private readonly string                     _currentValue;
            private readonly string[]                   _ids;
            private readonly Dictionary<string, Sprite> _sprites;
            private readonly Dictionary<string, GameObject> _prefabs;
            private readonly Dictionary<string, string> _displayNames;
            private readonly Action<string>             _onSelected;

            private string  _search      = string.Empty;
            private Vector2 _scroll;
            private bool    _focusSearch = true;

            public BuildingPickerPopup(
                string currentValue,
                string[] ids,
                Dictionary<string, Sprite> sprites,
                Dictionary<string, GameObject> prefabs,
                Dictionary<string, string> displayNames,
                Action<string> onSelected)
            {
                _currentValue = currentValue;
                _ids          = ids;
                _sprites      = sprites;
                _prefabs      = prefabs;
                _displayNames = displayNames;
                _onSelected   = onSelected;
            }

            public override Vector2 GetWindowSize() => new Vector2(PopupWidth, PopupHeight);
            public override void OnOpen() { _focusSearch = true; }

            public override void OnGUI(Rect rect)
            {
                Rect searchRect = new Rect(Padding, Padding, rect.width - Padding * 2f, SearchH);
                GUI.SetNextControlName(SearchCtrl);
                EditorGUI.BeginChangeCheck();
                _search = EditorGUI.TextField(searchRect, _search, EditorStyles.toolbarSearchField);
                if (EditorGUI.EndChangeCheck()) _scroll = Vector2.zero;
                if (_focusSearch) { _focusSearch = false; EditorGUI.FocusTextInControl(SearchCtrl); }

                Rect noneRect = new Rect(Padding, searchRect.yMax + Padding,
                    rect.width - Padding * 2f, RowH);
                if (string.IsNullOrEmpty(_currentValue))
                    EditorGUI.DrawRect(noneRect, new Color(0.24f, 0.36f, 0.24f, 0.7f));
                if (GUI.Button(noneRect, "(none)", EditorStyles.miniButton))
                {
                    _onSelected?.Invoke(string.Empty);
                    editorWindow.Close();
                    return;
                }

                var filtered = GetFiltered();

                Rect viewRect = new Rect(Padding, noneRect.yMax + Padding,
                    rect.width - Padding * 2f, rect.height - noneRect.yMax - Padding * 2f);
                Rect contentRect = new Rect(0f, 0f, viewRect.width - 14f,
                    Mathf.Max(viewRect.height, filtered.Count * RowH));

                _scroll = GUI.BeginScrollView(viewRect, _scroll, contentRect);

                float y = 0f;
                foreach (var id in filtered)
                {
                    Rect rowRect    = new Rect(0f, y, contentRect.width, RowH - 1f);
                    bool isSelected = string.Equals(_currentValue, id, StringComparison.Ordinal);
                    if (isSelected)
                        EditorGUI.DrawRect(rowRect, new Color(0.24f, 0.36f, 0.24f, 0.7f));

                    if (GUI.Button(rowRect, GUIContent.none, GUIStyle.none))
                    {
                        _onSelected?.Invoke(id);
                        editorWindow.Close();
                        GUI.EndScrollView();
                        return;
                    }

                    Rect iconRect = new Rect(4f, y + (RowH - IconSize) * 0.5f, IconSize, IconSize);
                    _sprites.TryGetValue(id, out Sprite spr);
                    _prefabs.TryGetValue(id, out GameObject prefab);
                    if ((spr != null && spr.texture != null) || prefab != null)
                        AdaptivePrefabPreviewUtility.DrawPrefabOrSprite(iconRect, prefab, spr);

                    _displayNames.TryGetValue(id, out string displayName);
                    string labelText = string.IsNullOrEmpty(displayName) || displayName == id
                        ? id : $"{id}  --  {displayName}";
                    Rect labelRect = new Rect(iconRect.xMax + 4f, y + 2f,
                        contentRect.width - iconRect.xMax - 8f, RowH - 2f);
                    GUI.Label(labelRect, labelText, EditorStyles.label);

                    y += RowH;
                }

                GUI.EndScrollView();
            }

            private List<string> GetFiltered()
            {
                if (string.IsNullOrWhiteSpace(_search)) return new List<string>(_ids);
                var terms  = _search.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var result = new List<string>();
                foreach (var id in _ids)
                {
                    _displayNames.TryGetValue(id, out string dn);
                    bool match = terms.All(t =>
                        id.IndexOf(t, StringComparison.OrdinalIgnoreCase) >= 0 ||
                        (dn != null && dn.IndexOf(t, StringComparison.OrdinalIgnoreCase) >= 0));
                    if (match) result.Add(id);
                }
                return result;
            }

        }
    }
}
