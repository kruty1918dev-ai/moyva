using UnityEngine;

namespace Kruty1918.Moyva.GraphSystem.API
{
    public interface IPreviewableNode
    {
        Texture2D GeneratePreview(int width, int height);
    }
}
