#if MOYVA_LEGACY_SCRIPTABLEOBJECT_EDITOR
using Unity.Pipeline.Commands;

namespace Kruty1918.Moyva.Editor.UnityCliBridge
{
    /// <summary>
    /// Stable project-specific command surface for Unity CLI / MCP.
    /// For operations not covered here, use Unity Pipeline eval/eval_file.
    /// </summary>
    public static class MoyvaUnityCliCommands
    {
        [CliCommand("moyva-project-status", "Return current project/editor/scene status as JSON.")]
        public static string ProjectStatus()
            => MoyvaUnityCliQuery.ProjectStatusJson();

        [CliCommand("moyva-scene-tree", "Return the complete active-scene hierarchy as JSON.")]
        public static string SceneTree()
            => MoyvaUnityCliQuery.SceneTreeJson();

        [CliCommand("moyva-ui-audit", "Return a structured snapshot of UI-related objects in the active scene.")]
        public static string UiAudit()
            => MoyvaUnityCliQuery.UiAuditJson();

        [CliCommand("moyva-ui-object", "Inspect one active-scene object by exact hierarchy path.")]
        public static string UiObject(
            [CliArg("path", "Exact hierarchy path, for example Canvas/TopBar/TurnText", Required = true)]
            string path)
            => MoyvaUnityCliQuery.InspectObjectJson(path);

        [CliCommand("moyva-ui-select", "Select and ping an object by exact hierarchy path.")]
        public static string UiSelect(
            [CliArg("path", "Exact hierarchy path", Required = true)] string path)
            => MoyvaUnityCliMutations.Select(path);

        [CliCommand("moyva-ui-set-active", "Set GameObject active state. Does not save the scene.")]
        public static string UiSetActive(
            [CliArg("path", "Exact hierarchy path", Required = true)] string path,
            [CliArg("active", "true or false", Required = true)] bool active)
            => MoyvaUnityCliMutations.SetActive(path, active);

        [CliCommand("moyva-ui-set-text", "Set TMP text. Does not save the scene.")]
        public static string UiSetText(
            [CliArg("path", "Exact hierarchy path", Required = true)] string path,
            [CliArg("text", "New text value", Required = true)] string text)
            => MoyvaUnityCliMutations.SetText(path, text);

        [CliCommand("moyva-ui-set-sprite", "Set Image.sprite from an Assets/... path. Empty path clears it. Does not save.")]
        public static string UiSetSprite(
            [CliArg("path", "Exact hierarchy path", Required = true)] string path,
            [CliArg("asset-path", "Sprite asset path under Assets", Required = true)] string assetPath)
            => MoyvaUnityCliMutations.SetSprite(path, assetPath);

        [CliCommand("moyva-ui-set-rect", "Set anchoredPosition and sizeDelta on RectTransform. Does not save.")]
        public static string UiSetRect(
            [CliArg("path", "Exact hierarchy path", Required = true)] string path,
            [CliArg("x", "anchoredPosition.x", Required = true)] float x,
            [CliArg("y", "anchoredPosition.y", Required = true)] float y,
            [CliArg("width", "sizeDelta.x", Required = true)] float width,
            [CliArg("height", "sizeDelta.y", Required = true)] float height)
            => MoyvaUnityCliMutations.SetRect(path, x, y, width, height);

        [CliCommand("moyva-ui-set-anchors", "Set anchorMin, anchorMax and pivot on RectTransform. Does not save.")]
        public static string UiSetAnchors(
            [CliArg("path", "Exact hierarchy path", Required = true)] string path,
            [CliArg("min-x", "anchorMin.x", Required = true)] float minX,
            [CliArg("min-y", "anchorMin.y", Required = true)] float minY,
            [CliArg("max-x", "anchorMax.x", Required = true)] float maxX,
            [CliArg("max-y", "anchorMax.y", Required = true)] float maxY,
            [CliArg("pivot-x", "pivot.x", Required = true)] float pivotX,
            [CliArg("pivot-y", "pivot.y", Required = true)] float pivotY)
            => MoyvaUnityCliMutations.SetAnchors(path, minX, minY, maxX, maxY, pivotX, pivotY);

        [CliCommand("moyva-ui-reparent", "Move an object under another active-scene object. Does not save.")]
        public static string UiReparent(
            [CliArg("path", "Object to move", Required = true)] string path,
            [CliArg("parent-path", "New parent hierarchy path", Required = true)] string parentPath)
            => MoyvaUnityCliMutations.Reparent(path, parentPath);

        [CliCommand("moyva-ui-create-panel", "Create a basic Image panel under a parent. Does not save.")]
        public static string UiCreatePanel(
            [CliArg("parent-path", "Exact parent hierarchy path", Required = true)] string parentPath,
            [CliArg("name", "New GameObject name", Required = true)] string name)
            => MoyvaUnityCliMutations.CreatePanel(parentPath, name);

        [CliCommand("moyva-ui-create-text", "Create a TextMeshProUGUI child under a parent. Does not save.")]
        public static string UiCreateText(
            [CliArg("parent-path", "Exact parent hierarchy path", Required = true)] string parentPath,
            [CliArg("name", "New GameObject name", Required = true)] string name,
            [CliArg("text", "Initial text", Required = true)] string text)
            => MoyvaUnityCliMutations.CreateText(parentPath, name, text);

        [CliCommand("moyva-ui-delete", "Delete an active-scene object with Undo. Requires --confirm DELETE. Does not save.")]
        public static string UiDelete(
            [CliArg("path", "Exact hierarchy path", Required = true)] string path,
            [CliArg("confirm", "Must be exactly DELETE", Required = true)] string confirm)
            => MoyvaUnityCliMutations.Delete(path, confirm);

        [CliCommand("moyva-ui-save-scene", "Back up the current scene externally, then save it.")]
        public static string UiSaveScene()
            => MoyvaUnityCliMutations.SaveActiveScene();

        [CliCommand("moyva-ui-undo", "Run one Unity Undo operation.")]
        public static string UiUndo()
            => MoyvaUnityCliMutations.UndoLast();
    }
}

#endif
