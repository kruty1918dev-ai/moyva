using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    internal sealed class ChunkFirstRuntimeMeshRegistry
    {
        private readonly List<Mesh> _meshes = new List<Mesh>(64);

        public void Register(Mesh mesh)
        {
            if (mesh != null && !_meshes.Contains(mesh))
                _meshes.Add(mesh);
        }

        public void Clear()
        {
            for (int i = 0; i < _meshes.Count; i++)
            {
                if (_meshes[i] != null)
                {
                    if (Application.isPlaying)
                        Object.Destroy(_meshes[i]);
                    else
                        Object.DestroyImmediate(_meshes[i]);
                }
            }

            _meshes.Clear();
        }
    }
}
