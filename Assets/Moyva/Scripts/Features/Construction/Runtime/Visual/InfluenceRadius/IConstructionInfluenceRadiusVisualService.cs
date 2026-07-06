using System;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal interface IConstructionInfluenceRadiusVisualService : IDisposable
    {
        void Initialize();
        void ShowPreview(Vector2Int center, int radius);
        void HidePreview();
        void ShowInspection(Vector2Int center, int radius);
        void HideInspection();
        void Tick();
    }
}
