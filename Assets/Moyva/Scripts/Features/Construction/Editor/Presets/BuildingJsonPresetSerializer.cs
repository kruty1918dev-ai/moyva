#if MOYVA_LEGACY_SCRIPTABLEOBJECT_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Editor.Presets
{
    internal static class BuildingJsonPresetSerializer
    {
        public static BuildingPresetPackSpec LoadPack()
        {
            Dictionary<string, object> root = ReadObject(BuildingPresetPaths.PackManifestPath);
            RequireString(root, "schema", "moyva.building-preset-pack", BuildingPresetPaths.PackManifestPath);
            var pack = new BuildingPresetPackSpec
            {
                PackId = GetString(root, "packId"),
                Version = GetInt(root, "version", 0),
                RequiresSchemaVersion = GetInt(root, "requiresSchemaVersion", 1),
                ApplyProposedModules = GetBool(root, "applyProposedModules", false),
            };
            foreach (object item in GetList(root, "presets"))
                if (item is string id && !string.IsNullOrWhiteSpace(id)) pack.PresetIds.Add(id.Trim());
            if (root.TryGetValue("resourceAliases", out object aliasNode) && aliasNode is Dictionary<string, object> aliases)
                foreach (KeyValuePair<string, object> pair in aliases)
                    if (pair.Value is string value) pack.ResourceAliases[pair.Key] = value;
            if (string.IsNullOrWhiteSpace(pack.PackId)) throw new InvalidDataException("preset-pack.json: packId is required.");
            if (pack.PresetIds.Count == 0) throw new InvalidDataException("preset-pack.json: no presets.");
            return pack;
        }

        public static BuildingPresetSpec LoadPreset(string id)
        {
            string path = $"{BuildingPresetPaths.PresetRoot}/{id}.json";
            Dictionary<string, object> root = ReadObject(path);
            RequireString(root, "schema", "moyva.building-preset", path);
            var spec = new BuildingPresetSpec { Id = GetString(root, "id"), Version = GetInt(root, "version", 0) };
            if (!string.Equals(spec.Id, id, StringComparison.Ordinal))
                throw new InvalidDataException($"{path}: id '{spec.Id}' does not match file id '{id}'.");

            if (TryObject(root, "identity", out Dictionary<string, object> identity))
            {
                spec.HasDisplayName = identity.ContainsKey("displayName"); spec.DisplayName = GetString(identity, "displayName");
                spec.HasDescription = identity.ContainsKey("description"); spec.Description = GetString(identity, "description");
                spec.HasCategory = identity.ContainsKey("category"); spec.Category = GetString(identity, "category");
                spec.HasRole = identity.ContainsKey("role"); spec.Role = GetString(identity, "role");
            }
            if (TryObject(root, "construction", out Dictionary<string, object> construction))
            {
                spec.HasBuildTurns = construction.ContainsKey("buildTurns"); spec.BuildTurns = GetInt(construction, "buildTurns", 0);
                foreach (object item in GetList(construction, "cost"))
                {
                    if (!(item is Dictionary<string, object> cost)) continue;
                    spec.Costs.Add(new BuildingPresetCostSpec { Resource = GetString(cost, "resource"), Amount = GetInt(cost, "amount", 0) });
                }
            }
            if (TryObject(root, "runtimeStats", out Dictionary<string, object> stats))
            {
                spec.HasMaxHp = stats.ContainsKey("maxHp"); spec.MaxHp = GetInt(stats, "maxHp", 0);
            }
            if (TryObject(root, "presentation", out Dictionary<string, object> presentation))
            {
                if (TryObject(presentation, "prefab", out Dictionary<string, object> prefab)) spec.Prefab = ParseObjectRef(prefab);
                if (TryObject(presentation, "icon", out Dictionary<string, object> icon)) spec.Icon = ParseObjectRef(icon);
            }
            if (TryObject(root, "modules", out Dictionary<string, object> modules))
            {
                spec.ModuleMode = GetString(modules, "mode") ?? "merge";
                ParseModules(GetList(modules, "active"), spec.ActiveModules);
                foreach (object item in GetList(modules, "remove")) if (item is string type && !string.IsNullOrWhiteSpace(type)) spec.RemoveModules.Add(type.Trim());
                ParseModules(GetList(modules, "conditional"), spec.ConditionalModules);
                ParseModules(GetList(modules, "deferred"), spec.DeferredModules);
            }
            return spec;
        }

        public static string ExportSelected(Kruty1918.Moyva.Construction.API.BuildingDefinitionAsset asset)
        {
            if (asset == null) throw new ArgumentNullException(nameof(asset));
            string escapedId = Escape(asset.Id);
            string escapedName = Escape(asset.DisplayName);
            return "{\n" +
                   "  \"$schema\": \"../Schemas/building-preset.schema.json\",\n" +
                   "  \"schema\": \"moyva.building-preset\",\n" +
                   "  \"version\": 1,\n" +
                   $"  \"id\": \"{escapedId}\",\n" +
                   $"  \"identity\": {{ \"displayName\": \"{escapedName}\", \"category\": \"{asset.Category}\", \"role\": \"{asset.Identity.Role}\" }},\n" +
                   $"  \"construction\": {{ \"buildTurns\": {asset.Construction.BuildTurns} }},\n" +
                   $"  \"runtimeStats\": {{ \"maxHp\": {asset.RuntimeStats.MaxHp} }}\n" +
                   "}\n";
        }

        private static void ParseModules(List<object> source, List<BuildingPresetModuleSpec> target)
        {
            foreach (object item in source)
            {
                if (!(item is Dictionary<string, object> module)) continue;
                var parsed = new BuildingPresetModuleSpec { Type = GetString(module, "type") };
                if (TryObject(module, "data", out Dictionary<string, object> data))
                    foreach (KeyValuePair<string, object> pair in data) parsed.Data[pair.Key] = pair.Value;
                target.Add(parsed);
            }
        }

        private static BuildingPresetObjectRefSpec ParseObjectRef(Dictionary<string, object> obj)
        {
            var spec = new BuildingPresetObjectRefSpec { Path = GetString(obj, "path"), Root = GetString(obj, "root") ?? "Assets", Required = GetBool(obj, "required", false) };
            AddStrings(GetList(obj, "exact"), spec.Exact); AddStrings(GetList(obj, "search"), spec.Search); AddStrings(GetList(obj, "fallback"), spec.Fallback);
            return spec;
        }

        private static void AddStrings(List<object> src, List<string> dst)
        {
            foreach (object value in src) if (value is string text && !string.IsNullOrWhiteSpace(text)) dst.Add(text.Trim());
        }

        private static Dictionary<string, object> ReadObject(string assetPath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? Directory.GetCurrentDirectory();
            string fullPath = Path.GetFullPath(Path.Combine(projectRoot, assetPath));
            if (!File.Exists(fullPath)) throw new FileNotFoundException("Preset file not found.", assetPath);
            object value = MiniJson.Parse(File.ReadAllText(fullPath, Encoding.UTF8));
            if (!(value is Dictionary<string, object> root)) throw new InvalidDataException($"{assetPath}: root must be an object.");
            return root;
        }
        private static bool TryObject(Dictionary<string, object> parent, string key, out Dictionary<string, object> value)
        { value = null; if (!parent.TryGetValue(key, out object node) || !(node is Dictionary<string, object> dict)) return false; value = dict; return true; }
        private static List<object> GetList(Dictionary<string, object> parent, string key)
        { return parent.TryGetValue(key, out object node) && node is List<object> list ? list : new List<object>(); }
        private static string GetString(Dictionary<string, object> parent, string key)
        { return parent.TryGetValue(key, out object node) && node is string text ? text : null; }
        private static int GetInt(Dictionary<string, object> parent, string key, int fallback)
        { if (!parent.TryGetValue(key, out object node) || node == null) return fallback; try { return Convert.ToInt32(node, CultureInfo.InvariantCulture); } catch { return fallback; } }
        private static bool GetBool(Dictionary<string, object> parent, string key, bool fallback)
        { return parent.TryGetValue(key, out object node) && node is bool b ? b : fallback; }
        private static void RequireString(Dictionary<string, object> root, string key, string expected, string path)
        { string value = GetString(root, key); if (!string.Equals(value, expected, StringComparison.Ordinal)) throw new InvalidDataException($"{path}: expected {key}='{expected}', got '{value}'."); }
        private static string Escape(string value) => (value ?? string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n");
    }

    internal static class MiniJson
    {
        public static object Parse(string json)
        {
            if (json == null) return null;
            var parser = new Parser(json);
            object value = parser.ParseValue();
            parser.SkipWhitespace();
            if (!parser.End) throw new FormatException($"Unexpected trailing JSON at {parser.Position}.");
            return value;
        }

        private sealed class Parser
        {
            private readonly string _json; private int _index;
            public Parser(string json) { _json = json; }
            public bool End => _index >= _json.Length; public int Position => _index;
            public void SkipWhitespace() { while (!End && char.IsWhiteSpace(_json[_index])) _index++; }
            public object ParseValue()
            {
                SkipWhitespace(); if (End) throw Error("Unexpected end of JSON");
                char c = _json[_index];
                if (c == '{') return ParseObject(); if (c == '[') return ParseArray(); if (c == '"') return ParseString();
                if (c == '-' || char.IsDigit(c)) return ParseNumber();
                if (Match("true")) return true; if (Match("false")) return false; if (Match("null")) return null;
                throw Error($"Unexpected token '{c}'");
            }
            private Dictionary<string, object> ParseObject()
            {
                Expect('{'); var result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase); SkipWhitespace();
                if (Peek('}')) { _index++; return result; }
                while (true)
                {
                    SkipWhitespace(); string key = ParseString(); SkipWhitespace(); Expect(':'); object value = ParseValue(); result[key] = value; SkipWhitespace();
                    if (Peek('}')) { _index++; break; } Expect(',');
                }
                return result;
            }
            private List<object> ParseArray()
            {
                Expect('['); var result = new List<object>(); SkipWhitespace(); if (Peek(']')) { _index++; return result; }
                while (true) { result.Add(ParseValue()); SkipWhitespace(); if (Peek(']')) { _index++; break; } Expect(','); }
                return result;
            }
            private string ParseString()
            {
                Expect('"'); var sb = new StringBuilder();
                while (!End)
                {
                    char c = _json[_index++]; if (c == '"') return sb.ToString();
                    if (c != '\\') { sb.Append(c); continue; }
                    if (End) throw Error("Bad escape"); char e = _json[_index++];
                    switch (e)
                    {
                        case '"': sb.Append('"'); break; case '\\': sb.Append('\\'); break; case '/': sb.Append('/'); break;
                        case 'b': sb.Append('\b'); break; case 'f': sb.Append('\f'); break; case 'n': sb.Append('\n'); break; case 'r': sb.Append('\r'); break; case 't': sb.Append('\t'); break;
                        case 'u': if (_index + 4 > _json.Length) throw Error("Bad unicode escape"); sb.Append((char)Convert.ToInt32(_json.Substring(_index,4),16)); _index += 4; break;
                        default: throw Error($"Bad escape '\\{e}'");
                    }
                }
                throw Error("Unterminated string");
            }
            private object ParseNumber()
            {
                int start = _index; if (Peek('-')) _index++; while (!End && char.IsDigit(_json[_index])) _index++;
                bool floating = false;
                if (!End && _json[_index] == '.') { floating = true; _index++; while (!End && char.IsDigit(_json[_index])) _index++; }
                if (!End && (_json[_index] == 'e' || _json[_index] == 'E')) { floating = true; _index++; if (!End && (_json[_index]=='+' || _json[_index]=='-')) _index++; while (!End && char.IsDigit(_json[_index])) _index++; }
                string token = _json.Substring(start, _index-start);
                if (!floating && long.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out long integer)) return integer;
                if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out double number)) return number;
                throw Error($"Invalid number '{token}'");
            }
            private bool Match(string token) { SkipWhitespace(); if (_index + token.Length > _json.Length || string.CompareOrdinal(_json,_index,token,0,token.Length)!=0) return false; _index += token.Length; return true; }
            private bool Peek(char c) => !End && _json[_index] == c;
            private void Expect(char c) { SkipWhitespace(); if (End || _json[_index] != c) throw Error($"Expected '{c}'"); _index++; }
            private FormatException Error(string message) => new FormatException($"{message} at JSON index {_index}.");
        }
    }
}

#endif
