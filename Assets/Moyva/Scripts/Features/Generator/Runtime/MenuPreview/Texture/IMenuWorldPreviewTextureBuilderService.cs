using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    public interface IMenuWorldPreviewTextureBuilderService
    {
        Texture2D Build(MenuWorldPreviewTextureBuildRequest request);
    }
}
