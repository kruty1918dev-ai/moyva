using UnityEngine;

namespace Kruty1918.Moyva.Units.API
{
    /// <summary>
    /// Розв'язує світові позиції юнітів із координат сітки з урахуванням поверхні терейну.
    /// </summary>
    public interface IUnitWorldPositionResolver
    {
        /// <summary>Чи живуть юніти на площині XZ — тоді yaw-facing та bob є валідною презентацією.</summary>
        bool Uses3DWorldPlane { get; }

        /// <summary>Розв'язує світову позицію для координати сітки.</summary>
        Vector3 ResolveWorldPosition(Vector2Int gridPosition, float surfacePivotOffsetY = 0.05f);

        /// <summary>Розв'язує вертикальний зсув pivot над поверхнею для конкретного об'єкта юніта.</summary>
        float ResolveSurfacePivotOffsetY(GameObject unitObject, Vector2Int gridPosition, float fallbackOffsetY = 0.05f);

        /// <summary>Вирівнює нижню межу об'єкта юніта до поверхні терейну.</summary>
        void AlignBottomToSurface(GameObject unitObject, Vector2Int gridPosition, float clearance = 0.02f);

        /// <summary>Намагається отримати висоту поверхні терейну у клітинці сітки.</summary>
        bool TryGetTerrainSurfaceY(Vector2Int gridPosition, out float surfaceY);
    }
}
