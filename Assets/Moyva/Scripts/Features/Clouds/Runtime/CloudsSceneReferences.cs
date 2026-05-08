using UnityEngine;

namespace Kruty1918.Moyva.Clouds.Runtime
{
    public sealed class CloudsSceneReferences
    {
        public UnityEngine.Camera SceneCamera { get; }
        public Transform Root { get; }

        public CloudsSceneReferences(UnityEngine.Camera sceneCamera, Transform root)
        {
            SceneCamera = sceneCamera;
            Root = root;
        }
    }
}