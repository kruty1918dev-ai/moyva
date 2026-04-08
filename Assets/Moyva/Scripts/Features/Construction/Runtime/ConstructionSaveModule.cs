using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.SaveSystem;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    /// <summary>
    /// Save-модуль для системи будівництва.
    /// Зберігає всі будівлі, підтверджені гравцем (playerPlacedBuildings).
    ///
    /// Формат блоку:
    ///   int32  — кількість записів
    ///   для кожного:
    ///     int32  — X позиції тайлу
    ///     int32  — Y позиції тайлу
    ///     string — buildingId (UTF-8 з length prefix via BinaryWriter)
    /// </summary>
    internal sealed class ConstructionSaveModule : ISaveModule
    {
        private readonly IConstructionService _constructionService;

        public ConstructionSaveModule(IConstructionService constructionService)
        {
            _constructionService = constructionService;
        }

        public void OnSave(ISaveContext context)
        {
            var buildings = _constructionService.GetPlayerPlacedBuildings();
            context.Writer.Write(buildings.Count);

            foreach (var pair in buildings)
            {
                context.Writer.Write(pair.Key.x);
                context.Writer.Write(pair.Key.y);
                context.Writer.Write(pair.Value);
            }

            Debug.Log($"[ConstructionSave] Збережено {buildings.Count} будівель.");
        }

        public void OnLoad(ISaveContext context)
        {
            int count = context.Reader.ReadInt32();

            for (int i = 0; i < count; i++)
            {
                int x          = context.Reader.ReadInt32();
                int y          = context.Reader.ReadInt32();
                string id      = context.Reader.ReadString();
                var position   = new UnityEngine.Vector2Int(x, y);

                _constructionService.RestoreFromSave(position, id);
            }

            Debug.Log($"[ConstructionSave] Відновлено {count} будівель із збереження.");
        }
    }
}
