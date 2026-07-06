namespace Kruty1918.Moyva.Generator.Runtime
{
    internal static class MoyvaTwcGraphEditorProgress
    {
        public static void Clear()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.ClearProgressBar();
#endif
        }
    }
}
