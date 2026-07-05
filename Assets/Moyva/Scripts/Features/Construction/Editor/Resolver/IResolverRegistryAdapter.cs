using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Editor
{
    internal interface IResolverRegistryAdapter
    {
        string AdapterName { get; }
        bool CanHandle(UnityEngine.Object registryAsset);
        SerializedObject CreateSerializedObject(UnityEngine.Object registryAsset);
        SerializedProperty GetCollectionsProperty(SerializedObject so);
        string GetCollectionId(SerializedProperty collectionElement);
    }
}
