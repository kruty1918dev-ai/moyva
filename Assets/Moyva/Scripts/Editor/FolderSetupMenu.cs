// -----------------------------------------------------------------------------
//  FolderSetupMenu
//  Створює стандартну структуру теки фічі Moyva (API / Runtime / Editor),
//  генерує відповідні asmdef-и з кореневими неймспейсами та автоматично
//  синхронізує namespace і модифікатори доступу у C#-скриптах.
//
//  Використання: правий клік у вікні Project на теці → Moyva/Script/Folder Setup.
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.EditorTools.FolderSetup
{
    /// <summary>
    /// Меню + логіка налаштування теки фічі Moyva.
    /// </summary>
    public static class FolderSetupMenu
    {
        internal const string AssemblyPrefix = "Kruty1918.Moyva";
        private const string MenuPath = "Assets/Moyva/Script/Folder Setup";

        internal static readonly string[] SubFolders = { "API", "Runtime", "Editor" };

        /// <summary>Встановлено ключ — тека є "чистою API" (доступ public дозволений).</summary>
        private static readonly HashSet<string> PublicFolders = new HashSet<string> { "API" };

        // ---------------------------------------------------------------------
        //  Menu
        // ---------------------------------------------------------------------

        [MenuItem(MenuPath, false, 80)]
        private static void Execute()
        {
            string folder = GetSelectedFolderPath();
            if (string.IsNullOrEmpty(folder))
            {
                EditorUtility.DisplayDialog(
                    "Folder Setup",
                    "Виберіть теку у вікні Project перед викликом команди.",
                    "OK");
                return;
            }

            try
            {
                SetupFolder(folder);
            }
            catch (Exception e)
            {
                Debug.LogError($"[FolderSetup] Помилка під час налаштування '{folder}': {e}");
                EditorUtility.DisplayDialog("Folder Setup",
                    $"Помилка: {e.Message}\nДив. Console.", "OK");
            }
        }

        [MenuItem(MenuPath, true)]
        private static bool Validate() => !string.IsNullOrEmpty(GetSelectedFolderPath());

        private static string GetSelectedFolderPath()
        {
            var obj = Selection.activeObject;
            if (obj == null) return null;
            string path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(path)) return null;
            if (!AssetDatabase.IsValidFolder(path)) return null;
            if (!path.StartsWith("Assets/", StringComparison.Ordinal)) return null;
            return path.Replace('\\', '/');
        }

        // ---------------------------------------------------------------------
        //  Core
        // ---------------------------------------------------------------------

        /// <summary>
        /// Повне налаштування теки фічі: створює підтеки, asmdef-и та
        /// нормалізує існуючі скрипти (namespace + модифікатор доступу).
        /// </summary>
        public static void SetupFolder(string featureFolder)
        {
            if (!AssetDatabase.IsValidFolder(featureFolder))
                throw new InvalidOperationException($"'{featureFolder}' не є текою.");

            string featureName = SanitizeIdentifier(Path.GetFileName(featureFolder));
            string baseNamespace = $"{AssemblyPrefix}.{featureName}";
            string mainAsmName = baseNamespace;
            string editorAsmName = $"{baseNamespace}.Editor";

            // 1. Підтеки.
            foreach (var sub in SubFolders)
                CreateSubfolder(featureFolder, sub);

            // 2. Основний asmdef (фіча в цілому) — у корені теки фічі.
            EnsureMainAsmdef(featureFolder, mainAsmName);

            // 3. Editor asmdef — всередині Editor/.
            string editorFolder = $"{featureFolder}/Editor";
            EnsureEditorAsmdef(editorFolder, editorAsmName, mainAsmName);

            // 4. Нормалізація вже існуючих скриптів у підтеках.
            NormalizeFolder($"{featureFolder}/API", $"{baseNamespace}.API", allowPublic: true);
            NormalizeFolder($"{featureFolder}/Runtime", $"{baseNamespace}.Runtime", allowPublic: false);
            NormalizeFolder($"{featureFolder}/Editor", $"{baseNamespace}.Editor", allowPublic: true);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[FolderSetup] Готово для '{featureName}'. Базовий namespace: {baseNamespace}");
        }

        // ---------------------------------------------------------------------
        //  Folder / asmdef helpers
        // ---------------------------------------------------------------------

        private static void CreateSubfolder(string parent, string name)
        {
            string full = $"{parent}/{name}";
            if (AssetDatabase.IsValidFolder(full)) return;
            AssetDatabase.CreateFolder(parent, name);
        }

        private static void EnsureMainAsmdef(string folder, string asmName)
        {
            string path = $"{folder}/{asmName}.asmdef";
            if (File.Exists(path))
            {
                UpdateAsmdefRootNamespace(path, asmName);
                return;
            }

            var json = new AsmdefJson
            {
                name = asmName,
                rootNamespace = asmName,
                references = new List<string>(),
                includePlatforms = new List<string>(),
                excludePlatforms = new List<string>(),
                allowUnsafeCode = false,
                overrideReferences = false,
                precompiledReferences = new List<string>(),
                autoReferenced = true,
                defineConstraints = new List<string>(),
                versionDefines = new List<string>(),
                noEngineReferences = false
            };
            WriteAsmdef(path, json);
        }

        private static void EnsureEditorAsmdef(string folder, string asmName, string mainAsmRef)
        {
            string path = $"{folder}/{asmName}.asmdef";
            if (File.Exists(path))
            {
                UpdateAsmdefRootNamespace(path, asmName);
                EnsureAsmdefReference(path, mainAsmRef);
                return;
            }

            var json = new AsmdefJson
            {
                name = asmName,
                rootNamespace = asmName,
                references = new List<string> { mainAsmRef },
                includePlatforms = new List<string> { "Editor" },
                excludePlatforms = new List<string>(),
                allowUnsafeCode = false,
                overrideReferences = false,
                precompiledReferences = new List<string>(),
                autoReferenced = true,
                defineConstraints = new List<string>(),
                versionDefines = new List<string>(),
                noEngineReferences = false
            };
            WriteAsmdef(path, json);
        }

        private static void WriteAsmdef(string path, AsmdefJson json)
        {
            string text = EditorJsonUtility.ToJson(json, true);
            File.WriteAllText(path, text, new UTF8Encoding(false));
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        private static void UpdateAsmdefRootNamespace(string path, string expectedRootNs)
        {
            try
            {
                var json = new AsmdefJson();
                EditorJsonUtility.FromJsonOverwrite(File.ReadAllText(path), json);

                bool changed = false;
                if (json.rootNamespace != expectedRootNs)
                {
                    json.rootNamespace = expectedRootNs;
                    changed = true;
                }
                if (string.IsNullOrEmpty(json.name))
                {
                    json.name = expectedRootNs;
                    changed = true;
                }
                if (changed)
                    WriteAsmdef(path, json);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[FolderSetup] Не вдалося оновити {path}: {e.Message}");
            }
        }

        private static void EnsureAsmdefReference(string path, string reference)
        {
            try
            {
                var json = new AsmdefJson();
                EditorJsonUtility.FromJsonOverwrite(File.ReadAllText(path), json);
                json.references ??= new List<string>();
                if (!json.references.Contains(reference))
                {
                    json.references.Add(reference);
                    WriteAsmdef(path, json);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[FolderSetup] Не вдалося оновити reference у {path}: {e.Message}");
            }
        }

        // ---------------------------------------------------------------------
        //  Script normalization
        // ---------------------------------------------------------------------

        internal static void NormalizeFolder(string folder, string expectedNamespace, bool allowPublic)
        {
            if (!AssetDatabase.IsValidFolder(folder)) return;

            string abs = Path.GetFullPath(folder);
            foreach (var file in Directory.EnumerateFiles(abs, "*.cs", SearchOption.AllDirectories))
            {
                // Не чіпаємо asmdef/meta та порожні згенеровані AssemblyInfo — їх namespace не важливий.
                if (file.EndsWith(".meta", StringComparison.Ordinal)) continue;
                NormalizeScript(file, expectedNamespace, allowPublic);
            }
        }

        /// <summary>
        /// Приводить файл до потрібного namespace і (за потреби) замінює top-level
        /// оголошення public-типів на internal.
        /// </summary>
        internal static void NormalizeScript(string absoluteOrAssetPath, string expectedNamespace, bool allowPublic)
        {
            string path = absoluteOrAssetPath;
            if (!File.Exists(path))
            {
                // можливо прийшов asset-path
                path = Path.GetFullPath(absoluteOrAssetPath);
                if (!File.Exists(path)) return;
            }

            string original = File.ReadAllText(path);
            if (ShouldSkipScript(original)) return;

            string updated = ApplyNamespace(original, expectedNamespace);
            if (!allowPublic)
                updated = DemotePublicTypesToInternal(updated);

            if (updated != original)
            {
                File.WriteAllText(path, updated, DetectEncoding(original));
                Debug.Log($"[FolderSetup] Оновлено: {ToAssetPath(path)} → {expectedNamespace}{(allowPublic ? "" : " (internal)")}");
            }
        }

        private static bool ShouldSkipScript(string source)
        {
            // Службові файли типу AssemblyInfo.cs чи generated — не чіпаємо namespace.
            if (source.Contains("[assembly:", StringComparison.Ordinal) &&
                !Regex.IsMatch(source, @"\bnamespace\s+[\w\.]+"))
                return true;
            if (source.Contains("// <auto-generated", StringComparison.Ordinal)) return true;
            return false;
        }

        private static readonly Regex NamespaceBlockRegex =
            new Regex(@"^(?<indent>[ \t]*)namespace\s+(?<ns>[\w\.]+)\s*(?<brace>\{|;)",
                RegexOptions.Multiline | RegexOptions.Compiled);

        private static string ApplyNamespace(string source, string expectedNamespace)
        {
            var match = NamespaceBlockRegex.Match(source);
            if (match.Success)
            {
                if (match.Groups["ns"].Value == expectedNamespace) return source;
                return source.Substring(0, match.Groups["ns"].Index)
                       + expectedNamespace
                       + source.Substring(match.Groups["ns"].Index + match.Groups["ns"].Length);
            }

            // Немає namespace зовсім — обгортаємо. Спочатку ділимо на usings + решту.
            return WrapInNamespace(source, expectedNamespace);
        }

        private static string WrapInNamespace(string source, string ns)
        {
            var usings = new StringBuilder();
            var body = new StringBuilder();
            bool usingsDone = false;
            foreach (var lineRaw in source.Split('\n'))
            {
                string line = lineRaw.TrimEnd('\r');
                if (!usingsDone)
                {
                    string t = line.TrimStart();
                    if (t.StartsWith("using ", StringComparison.Ordinal) ||
                        t.StartsWith("//", StringComparison.Ordinal) ||
                        t.Length == 0)
                    {
                        usings.Append(line).Append('\n');
                        continue;
                    }
                    usingsDone = true;
                }
                body.Append(line).Append('\n');
            }

            var sb = new StringBuilder();
            sb.Append(usings);
            if (usings.Length > 0 && !usings.ToString().EndsWith("\n\n"))
                sb.Append('\n');
            sb.Append("namespace ").Append(ns).Append('\n').Append("{\n");
            foreach (var line in body.ToString().Split('\n'))
                sb.Append("    ").Append(line).Append('\n');
            // чистимо останній зайвий перенос
            while (sb.Length > 0 && sb[sb.Length - 1] == '\n') sb.Length--;
            sb.Append("\n}\n");
            return sb.ToString();
        }

        // Match: optional attributes/whitespace at line start → optional modifiers → public → type keyword.
        private static readonly Regex PublicTypeRegex = new Regex(
            @"\bpublic\b(?=(?:\s+(?:static|sealed|abstract|partial|unsafe|readonly|ref)\b)*\s+(?:class|struct|interface|enum|record)\b)",
            RegexOptions.Compiled);

        private static string DemotePublicTypesToInternal(string source)
        {
            return PublicTypeRegex.Replace(source, "internal");
        }

        // ---------------------------------------------------------------------
        //  OnWillCreateAsset hook: застосовується до щойно створених .cs у Moyva-теках.
        // ---------------------------------------------------------------------

        internal static bool TryResolveContext(string assetPath, out string expectedNamespace, out bool allowPublic)
        {
            expectedNamespace = null;
            allowPublic = true;

            if (string.IsNullOrEmpty(assetPath)) return false;
            assetPath = assetPath.Replace('\\', '/');
            if (!assetPath.EndsWith(".cs", StringComparison.Ordinal)) return false;

            // шукаємо сегмент теки, що знаходиться під API|Runtime|Editor
            // та вище нього — тека фічі, а ще вище — будь-що (Assets/Moyva/Scripts/Features/*).
            var parts = assetPath.Split('/');
            for (int i = parts.Length - 2; i >= 1; i--)
            {
                string seg = parts[i];
                if (!IsKnownSubFolder(seg)) continue;

                // батьківська тека — це тека фічі
                string featureName = parts[i - 1];
                if (string.IsNullOrEmpty(featureName)) continue;

                // перевірити, що у цій фічі існує відповідний asmdef з нашим префіксом
                string featureFolder = string.Join("/", parts, 0, i);
                string asmPath = $"{featureFolder}/{AssemblyPrefix}.{featureName}.asmdef";
                if (!File.Exists(asmPath)) return false;

                expectedNamespace = $"{AssemblyPrefix}.{featureName}.{seg}";
                allowPublic = PublicFolders.Contains(seg) || seg == "Editor";
                return true;
            }
            return false;
        }

        private static bool IsKnownSubFolder(string seg)
        {
            for (int i = 0; i < SubFolders.Length; i++)
                if (SubFolders[i] == seg) return true;
            return false;
        }

        // ---------------------------------------------------------------------
        //  Utils
        // ---------------------------------------------------------------------

        private static string SanitizeIdentifier(string raw)
        {
            if (string.IsNullOrEmpty(raw))
                throw new InvalidOperationException("Порожня назва теки.");

            var sb = new StringBuilder();
            foreach (char c in raw)
            {
                if (char.IsLetterOrDigit(c) || c == '_') sb.Append(c);
                else sb.Append('_');
            }
            if (char.IsDigit(sb[0])) sb.Insert(0, '_');
            return sb.ToString();
        }

        private static Encoding DetectEncoding(string original)
        {
            // збережемо без BOM — як Unity за замовчуванням.
            return new UTF8Encoding(false);
        }

        private static string ToAssetPath(string absolute)
        {
            absolute = absolute.Replace('\\', '/');
            int idx = absolute.IndexOf("/Assets/", StringComparison.Ordinal);
            if (idx >= 0) return absolute.Substring(idx + 1);
            return absolute;
        }

        // ---------------------------------------------------------------------
        //  Asmdef serializable mirror
        // ---------------------------------------------------------------------

        [Serializable]
        internal class AsmdefJson
        {
            public string name;
            public string rootNamespace;
            public List<string> references;
            public List<string> includePlatforms;
            public List<string> excludePlatforms;
            public bool allowUnsafeCode;
            public bool overrideReferences;
            public List<string> precompiledReferences;
            public bool autoReferenced;
            public List<string> defineConstraints;
            public List<string> versionDefines;
            public bool noEngineReferences;
        }
    }

    /// <summary>
    /// Перехоплює створення нових .cs файлів у Moyva-теках та автоматично
    /// виставляє namespace і модифікатор доступу, відповідно до підтеки.
    /// </summary>
    internal class FolderSetupScriptPostProcessor : UnityEditor.AssetModificationProcessor
    {
        private static void OnWillCreateAsset(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath)) return;
            if (!assetPath.EndsWith(".cs", StringComparison.Ordinal)) return;

            if (!FolderSetupMenu.TryResolveContext(assetPath, out var ns, out var allowPublic))
                return;

            string pathCopy = assetPath;
            string nsCopy = ns;
            bool publicCopy = allowPublic;

            // Unity ще не записав файл шаблону — відкладаємо до наступного тіку.
            EditorApplication.delayCall += () =>
            {
                try
                {
                    if (!File.Exists(pathCopy)) return;
                    FolderSetupMenu.NormalizeScript(pathCopy, nsCopy, publicCopy);
                    AssetDatabase.ImportAsset(pathCopy, ImportAssetOptions.ForceUpdate);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[FolderSetup] Пост-обробка {pathCopy} не вдалась: {e.Message}");
                }
            };
        }
    }
}
