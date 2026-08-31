namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    /// <summary>
    /// Compatibility wrapper for scenes that still reference the original script GUID.
    /// Project-facing code should bind/use <see cref="HomeMenuMoyvaUiAnchor"/>.
    /// </summary>
    public sealed class HomeMenuHtmlShellAnchor : HomeMenuMoyvaUiAnchor
    {
        protected override string BuildEditorPreviewHtml()
        {
            var html = HtmlAsset != null ? HtmlAsset.text : string.Empty;
            var viewportClass = CurrentViewportClass;
            if (!string.IsNullOrWhiteSpace(viewportClass))
            {
                html = html.Replace(
                    "className=\"home-menu-shell\"",
                    $"className=\"home-menu-shell {viewportClass}\"",
                    System.StringComparison.Ordinal);
            }

            return html;
        }
    }
}
