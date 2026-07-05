using Kruty1918.Moyva.Construction.Runtime;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Editor
{
    internal sealed class BuildingRegistryResolverAdapter : IResolverRegistryAdapter
    {
        public string AdapterName => "BuildingRegistrySO";

        public bool CanHandle(UnityEngine.Object registryAsset)
        {
            return registryAsset is BuildingRegistrySO;
        }

        public SerializedObject CreateSerializedObject(UnityEngine.Object registryAsset)
        {
            return registryAsset == null ? null : new SerializedObject(registryAsset);
        }

        public SerializedProperty GetCollectionsProperty(SerializedObject so)
        {
            return so?.FindProperty("WallCollections");
        }

        public string GetCollectionId(SerializedProperty collectionElement)
        {
            return collectionElement?.FindPropertyRelative("CollectionId")?.stringValue;
        }
    }
}
