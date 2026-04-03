using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Units.API;
using UnityEngine;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    /// <summary>
    /// ISaveModule для збереження та завантаження юнітів.
    /// Зберігає: typeId + позиція + стаміна для кожного активного юніта.
    /// При завантаженні recreates юнітів через IUnitFactory.
    /// </summary>
    internal sealed class UnitsSaveModule : ISaveModule
    {
        private readonly struct UnitRecord
        {
            public readonly string TypeId;
            public readonly Vector2Int Position;
            public readonly bool HasStamina;
            public readonly float Stamina;

            public UnitRecord(string typeId, Vector2Int position, bool hasStamina, float stamina)
            {
                TypeId = typeId;
                Position = position;
                HasStamina = hasStamina;
                Stamina = stamina;
            }
        }

        private readonly IUnitService _unitService;
        private readonly IUnitFactory _unitFactory;

        public UnitsSaveModule(IUnitService unitService, IUnitFactory unitFactory)
        {
            _unitService = unitService;
            _unitFactory = unitFactory;
        }

        public void OnSave(ISaveContext context)
        {
            var unitIds = _unitService.GetAllUnitIds();
            context.Writer.Write(unitIds.Count);

            foreach (var unitId in unitIds)
            {
                string typeId = _unitService.GetUnitTypeId(unitId) ?? "";
                bool hasPos   = _unitService.TryGetUnitPosition(unitId, out var pos);
                float stamina = _unitService.GetStamina(unitId);

                context.Writer.Write(typeId);
                context.Writer.Write(hasPos ? pos.x : 0);
                context.Writer.Write(hasPos ? pos.y : 0);
                context.Writer.Write(stamina);

                Debug.Log($"[UnitsSave] Збережено юніт: typeId={typeId}, pos={pos}, stamina={stamina}");
            }

            Debug.Log($"[UnitsSave] Збережено {unitIds.Count} юнітів.");
        }

        public void OnLoad(ISaveContext context)
        {
            int count = context.Reader.ReadInt32();
            long payloadStart = context.Reader.BaseStream.Position;

            if (!TryParseRecordsWithStamina(context.Reader, count, out var records))
            {
                context.Reader.BaseStream.Position = payloadStart;

                if (!TryParseLegacyRecords(context.Reader, count, out records))
                {
                    Debug.LogWarning("[UnitsSave] Не вдалося розібрати блок юнітів (ані новий, ані legacy формат).");
                    return;
                }

                Debug.Log("[UnitsSave] Завантаження виконано у legacy-режимі (без стаміни в сейві).");
            }

            for (int i = 0; i < records.Count; i++)
            {
                var record = records[i];

                if (string.IsNullOrEmpty(record.TypeId))
                {
                    Debug.LogWarning($"[UnitsSave] Пропущено запис {i}: порожній typeId.");
                    continue;
                }

                string newUnitId = _unitFactory.CreateUnit(record.TypeId, record.Position);
                if (!string.IsNullOrEmpty(newUnitId) && record.HasStamina)
                    _unitService.SetStamina(newUnitId, record.Stamina);

                Debug.Log(
                    $"[UnitsSave] Завантажено юніт: typeId={record.TypeId}, pos={record.Position}, " +
                    $"stamina={(record.HasStamina ? record.Stamina.ToString() : "<legacy>")}, unitId={newUnitId}");
            }

            Debug.Log($"[UnitsSave] Завантажено {records.Count} юнітів.");
        }

        private static bool TryParseRecordsWithStamina(System.IO.BinaryReader reader, int count, out System.Collections.Generic.List<UnitRecord> records)
        {
            records = new System.Collections.Generic.List<UnitRecord>(count);

            try
            {
                for (int i = 0; i < count; i++)
                {
                    string typeId = reader.ReadString();
                    int x = reader.ReadInt32();
                    int y = reader.ReadInt32();
                    float stamina = reader.ReadSingle();

                    if (float.IsNaN(stamina) || float.IsInfinity(stamina) || stamina < 0f || stamina > 100000f)
                        return false;

                    records.Add(new UnitRecord(typeId, new Vector2Int(x, y), true, stamina));
                }

                return reader.BaseStream.Position == reader.BaseStream.Length;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryParseLegacyRecords(System.IO.BinaryReader reader, int count, out System.Collections.Generic.List<UnitRecord> records)
        {
            records = new System.Collections.Generic.List<UnitRecord>(count);

            try
            {
                for (int i = 0; i < count; i++)
                {
                    string typeId = reader.ReadString();
                    int x = reader.ReadInt32();
                    int y = reader.ReadInt32();

                    records.Add(new UnitRecord(typeId, new Vector2Int(x, y), false, 0f));
                }

                return reader.BaseStream.Position == reader.BaseStream.Length;
            }
            catch
            {
                return false;
            }
        }
    }
}
