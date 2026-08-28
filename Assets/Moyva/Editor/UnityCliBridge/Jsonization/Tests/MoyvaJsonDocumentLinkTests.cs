using System.Linq;
using System.Reflection;
using Kruty1918.Moyva.Jsonization;
using Kruty1918.Moyva.Jsonization.Editor;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Jsonization.Editor
{
    public sealed class MoyvaJsonDocumentLinkTests
    {
        [TestCase(
            "Assets/Moyva/Editor/JsonDocuments/Tiles/grass.asset",
            "Assets/Moyva/Presets/Tiles/grass.json")]
        [TestCase(
            "Assets/Moyva/Editor/JsonDocuments/Movement/infantry.asset",
            "Assets/Moyva/Presets/Movement/infantry.json")]
        public void GeneratedLinkReferencesCanonicalJson(string linkPath, string jsonPath)
        {
            MoyvaJsonDocumentLink link =
                AssetDatabase.LoadAssetAtPath<MoyvaJsonDocumentLink>(linkPath);

            Assert.That(link, Is.Not.Null, linkPath);
            Assert.That(link.Source, Is.Not.Null);
            Assert.That(AssetDatabase.GetAssetPath(link.Source), Is.EqualTo(jsonPath));
        }

        [Test]
        public void LinkSerializesOnlyTheJsonTextAssetReference()
        {
            FieldInfo[] serializedFields = typeof(MoyvaJsonDocumentLink)
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                .Where(field => field.IsPublic || field.GetCustomAttribute<SerializeField>() != null)
                .ToArray();

            Assert.That(serializedFields, Has.Length.EqualTo(1));
            Assert.That(serializedFields[0].FieldType, Is.EqualTo(typeof(TextAsset)));
        }

        [Test]
        public void RuntimeCloneIgnoresEditorDocumentLinkMetadata()
        {
            JObject canonical = JObject.Parse(
                "{'schema':'moyva.tile-type','version':1,'id':'grass','model':'tile-type','visual':{'gridMode':'Dual'}}");
            JObject withEditor = (JObject)canonical.DeepClone();
            withEditor["editor"] = JObject.Parse(
                "{'documentLink':{'enabled':true,'path':'Assets/Moyva/Editor/JsonDocuments'}}");

            Assert.That(
                JToken.DeepEquals(
                    MoyvaJsonDocumentMetadata.CloneRuntimeDocument(canonical),
                    MoyvaJsonDocumentMetadata.CloneRuntimeDocument(withEditor)),
                Is.True);
            Assert.That(
                MoyvaJsonDocumentMetadata.CloneConfigPayload(withEditor)["editor"],
                Is.Null);
        }
    }
}
