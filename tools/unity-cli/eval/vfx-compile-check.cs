// Reports compile state and any assembly containing our Vfx feature.
var sb = new System.Text.StringBuilder();
sb.Append("compiling=").Append(UnityEditor.EditorApplication.isCompiling);
var vfx = UnityEditor.Compilation.CompilationPipeline.GetAssemblies()
    .FirstOrDefault(a => a.name == "Kruty1918.Moyva.Vfx");
sb.Append("|vfxAsm=").Append(vfx != null ? vfx.name : "MISSING");
// Recent error logs via reflection over Unity's log entries is brittle; instead
// surface scriptCompilationFailed-style state.
sb.Append("|domainReload=").Append(UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode);
return sb.ToString();
