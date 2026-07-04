using GiantGrey.TileWorldCreator;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Ізолює cleanup runtime/editor fog volume output від <see cref="FogOfWarVolumeController"/>.
    /// Host-компонент не повинен сам керувати low-level destroy логікою.
    /// </summary>
    internal interface IFogVolumeOutputCleaner
    {
        /// <summary>
        /// Видаляє runtime/editor generated fog output із переданого TWC manager-а.
        /// </summary>
        /// <param name="manager">Manager, під чиїм transform лежить fog output.</param>
        void ClearGeneratedOutput(TileWorldCreatorManager manager);

        /// <summary>
        /// Видаляє тільки generated children із fog manager-а, не чіпаючи active runtime configuration.
        /// </summary>
        /// <param name="manager">Manager, під чиїм transform лежить fog output.</param>
        /// <param name="forceImmediate">Чи видаляти children одразу, щоб TWC не перевикористав старі layer objects.</param>
        /// <returns>Кількість видалених generated root children.</returns>
        int ClearGeneratedChildren(TileWorldCreatorManager manager, bool forceImmediate = false);
    }

    /// <summary>
    /// Сервіс cleanup-а для generated fog volume output.
    /// Підтримує як runtime <c>Destroy</c>, так і editor <c>DestroyImmediate</c> path.
    /// </summary>
    internal sealed class FogVolumeOutputCleaner : IFogVolumeOutputCleaner
    {
        /// <summary>
        /// Видаляє всі generated fog children і тимчасову runtime configuration з manager-а.
        /// </summary>
        /// <param name="manager">TWC manager, який тримає generated fog output.</param>
        public void ClearGeneratedOutput(TileWorldCreatorManager manager)
        {
            if (manager == null)
                return;

            ClearGeneratedChildren(manager);

            if (manager.configuration != null
                && manager.configuration.name.StartsWith("FogOfWar_", System.StringComparison.Ordinal))
            {
                DestroyGeneratedObject(manager.configuration);
                manager.configuration = null;
            }
        }

        /// <inheritdoc />
        public int ClearGeneratedChildren(TileWorldCreatorManager manager, bool forceImmediate = false)
        {
            if (manager == null)
                return 0;

            int removed = 0;
            for (int i = manager.transform.childCount - 1; i >= 0; i--)
            {
                var child = manager.transform.GetChild(i);
                if (child == null)
                    continue;

                DestroyGeneratedObject(child.gameObject, forceImmediate);
                removed++;
            }

            return removed;
        }

        private static void DestroyGeneratedObject(Object obj, bool forceImmediate = false)
        {
            if (obj == null)
                return;

            if (Application.isPlaying && !forceImmediate)
                Object.Destroy(obj);
            else
                Object.DestroyImmediate(obj);
        }
    }
}
