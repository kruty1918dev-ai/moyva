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

            for (int i = manager.transform.childCount - 1; i >= 0; i--)
                DestroyGeneratedObject(manager.transform.GetChild(i).gameObject);

            if (manager.configuration != null
                && manager.configuration.name.StartsWith("FogOfWar_", System.StringComparison.Ordinal))
            {
                DestroyGeneratedObject(manager.configuration);
                manager.configuration = null;
            }
        }

        private static void DestroyGeneratedObject(Object obj)
        {
            if (obj == null)
                return;

            if (Application.isPlaying)
                Object.Destroy(obj);
            else
                Object.DestroyImmediate(obj);
        }
    }
}
