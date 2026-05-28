using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Grid.API;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Editor
{
    public sealed class TileWorldCreatorRegistrySyncWindow : EditorWindow
    {
        private TileRegistrySO _tileRegistry;
        private TileWorldCreatorIdMappingSO _mapping;
        private bool _updateExisting = true;
        private bool _rebuildFromMapping = true;

        public static void Open()
        {
            GetWindow<TileWorldCreatorRegistrySyncWindow>("TWC Registry Sync");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("TileWorldCreator Registry Sync", EditorStyles.boldLabel);
            EditorGUILayout.Space(6f);

            _tileRegistry = (TileRegistrySO)EditorGUILayout.ObjectField("Tile Registry", _tileRegistry, typeof(TileRegistrySO), false);
            _mapping = (TileWorldCreatorIdMappingSO)EditorGUILayout.ObjectField("TWC ID Mapping", _mapping, typeof(TileWorldCreatorIdMappingSO), false);
            _rebuildFromMapping = EditorGUILayout.Toggle("Rebuild From TWC Mapping", _rebuildFromMapping);
            using (new EditorGUI.DisabledScope(_rebuildFromMapping))
                _updateExisting = EditorGUILayout.Toggle("Update Existing", _updateExisting);

            EditorGUILayout.HelpBox(
                "Синхронізує лише exact terrain ID без wildcard '*'. Registry стає gameplay/TWC-реєстром: ID, Movement Cost, TilePreset і TWC layer; legacy Visual Prefab очищається.",
                MessageType.Info);

            using (new EditorGUI.DisabledScope(_tileRegistry == null || _mapping == null))
            {
                if (GUILayout.Button("Sync Terrain IDs"))
                    SyncTerrainIds();
            }
        }

        private void SyncTerrainIds()
        {
            int added;
            int updated;
            int skipped;
            bool success;

            if (_rebuildFromMapping)
            {
                success = TileWorldCreatorRegistrySyncUtility.RebuildTerrainIds(
                    _tileRegistry,
                    _mapping,
                    out added,
                    out updated,
                    out skipped);
            }
            else
            {
                success = TileWorldCreatorRegistrySyncUtility.SyncTerrainIds(
                    _tileRegistry,
                    _mapping,
                    _updateExisting,
                    out added,
                    out updated,
                    out skipped);
            }

            if (!success)
            {
                EditorUtility.DisplayDialog("TWC Registry Sync", "Не знайдено поле _definitions у TileRegistrySO.", "OK");
                return;
            }

            EditorUtility.DisplayDialog(
                "TWC Registry Sync",
                $"Готово. Додано: {added}. Оновлено: {updated}. Пропущено: {skipped}.",
                "OK");
        }
    }
}