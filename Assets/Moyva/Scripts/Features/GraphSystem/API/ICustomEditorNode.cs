namespace Kruty1918.Moyva.GraphSystem.API
{
    public interface ICustomEditorNode
    {
#if UNITY_EDITOR
        void OpenEditorWindow();
#endif
    }
}
