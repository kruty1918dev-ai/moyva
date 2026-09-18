using UnityEngine;

namespace Kruty1918.Moyva.Jsonization.Editor
{
    /// <summary>
    /// Editor-only ручка до canonical Moyva JSON-документа.
    /// Gameplay-значення живуть тільки в JSON; цей asset зберігає лише TextAsset-посилання.
    /// </summary>
    public sealed class MoyvaJsonDocumentLink : ScriptableObject
    {
        [SerializeField, HideInInspector]
        private TextAsset _source;

        public TextAsset Source => _source;

        internal void SetSource(TextAsset source)
        {
            _source = source;
        }
    }
}
