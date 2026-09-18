using System;
using System.IO;
using Kruty1918.Moyva.Marketing.Contracts;

namespace Kruty1918.Moyva.Marketing.Output
{
    /// <summary>Single authority for output paths and file naming.</summary>
    public sealed class MarketingOutputLayout
    {
        public string Root { get; }
        public string RunFolder { get; private set; }

        public MarketingOutputLayout(string rootOverride = null)
        {
            Root = string.IsNullOrWhiteSpace(rootOverride)
                ? Path.GetFullPath(Path.Combine(ProjectRoot(), "MarketingOutput"))
                : Path.GetFullPath(rootOverride);
        }

        public string CreateRunFolder(string recipeId)
        {
            string stamp = DateTime.UtcNow.ToString("yyyy-MM-dd_HHmmss");
            string safe = Sanitize(recipeId);
            RunFolder = Path.Combine(Root, $"{stamp}_{safe}");
            Directory.CreateDirectory(RunFolder);
            Directory.CreateDirectory(Path.Combine(RunFolder, "Metadata"));
            Directory.CreateDirectory(Path.Combine(RunFolder, "Master"));
            Directory.CreateDirectory(TempFolder);
            return RunFolder;
        }

        public string TempFolder => Path.Combine(RunFolder ?? Root, ".temp");

        public string PlatformFolder(string platformId)
        {
            string dir = Path.Combine(RunFolder, Sanitize(platformId));
            Directory.CreateDirectory(dir);
            return dir;
        }

        public string MetadataFile(string name) => Path.Combine(RunFolder, "Metadata", name);
        public string ManifestFile => Path.Combine(RunFolder, "manifest.json");

        public static string Sanitize(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "run";
            var chars = name.Trim().ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                char c = chars[i];
                if (!(char.IsLetterOrDigit(c) || c == '-' || c == '_'))
                    chars[i] = '-';
            }
            return new string(chars);
        }

        /// <summary>Project root = parent of Assets. Works in editor play mode.</summary>
        public static string ProjectRoot()
        {
            string dataPath = UnityEngine.Application.dataPath; // .../Assets
            return Directory.GetParent(dataPath)?.FullName ?? Directory.GetCurrentDirectory();
        }
    }
}
