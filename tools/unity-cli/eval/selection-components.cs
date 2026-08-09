using UnityEditor;
using UnityEngine;

var go = Selection.activeGameObject;
if (go == null)
    return "selection=<none>";

var components = go.GetComponents<Component>()
    .Where(c => c != null)
    .Select(c => c.GetType().FullName)
    .ToArray();

return $"{go.name}: {string.Join(", ", components)}";
