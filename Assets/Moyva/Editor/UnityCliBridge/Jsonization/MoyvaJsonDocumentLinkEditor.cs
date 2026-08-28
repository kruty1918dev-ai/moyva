using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Jsonization.Editor
{
    [CustomEditor(typeof(MoyvaJsonDocumentLink))]
    public sealed class MoyvaJsonDocumentLinkEditor : UnityEditor.Editor
    {
        private readonly Dictionary<string, bool> _foldouts =
            new(StringComparer.Ordinal);
        private readonly Dictionary<string, string[]> _configIdsByModel =
            new(StringComparer.OrdinalIgnoreCase);

        private MoyvaJsonDocumentLink _link;
        private string _sourcePath;
        private string _baseText;
        private JObject _baseDocument;
        private JObject _stagedDocument;
        private JObject _schema;
        private IReadOnlyList<MoyvaJsonValidationIssue> _issues =
            Array.Empty<MoyvaJsonValidationIssue>();
        private Vector2 _scroll;
        private string _status;
        private MessageType _statusType = MessageType.Info;
        private bool _externalConflict;

        private bool IsDirty => _baseDocument != null
                                && _stagedDocument != null
                                && !JToken.DeepEquals(_baseDocument, _stagedDocument);

        private void OnEnable()
        {
            _link = target as MoyvaJsonDocumentLink;
            ReloadFromDisk();
        }

        public override void OnInspectorGUI()
        {
            if (_link == null || _link.Source == null)
            {
                EditorGUILayout.HelpBox(
                    "This generated link has no source TextAsset. Rebuild JSON document links.",
                    MessageType.Error);
                return;
            }

            DetectExternalChange();
            DrawHeader();
            if (_externalConflict)
            {
                DrawConflictControls();
                return;
            }

            if (_stagedDocument == null || _schema == null)
            {
                EditorGUILayout.HelpBox(
                    string.IsNullOrWhiteSpace(_status) ? "JSON could not be loaded." : _status,
                    MessageType.Error);
                return;
            }

            EditorGUI.BeginChangeCheck();
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            DrawObjectContents(_stagedDocument, _schema, string.Empty, isRoot: true);
            DrawEditorDocumentLinkControls();
            EditorGUILayout.EndScrollView();
            if (EditorGUI.EndChangeCheck())
            {
                _issues = MoyvaJsonDocumentValidation.Validate(_sourcePath, _stagedDocument);
                _status = _issues.Count == 0
                    ? "Staged JSON is valid."
                    : $"Staged JSON has {_issues.Count} issue(s).";
                _statusType = _issues.Count == 0 ? MessageType.Info : MessageType.Error;
            }

            DrawIssues();
            DrawApplyControls();
        }

        private void DrawHeader()
        {
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField(
                    "JSON source",
                    _link.Source,
                    typeof(TextAsset),
                    allowSceneObjects: false);
            }

            EditorGUILayout.LabelField("Path", _sourcePath ?? string.Empty);
            if (_stagedDocument != null)
            {
                EditorGUILayout.LabelField("Schema", _stagedDocument.Value<string>("schema") ?? string.Empty);
                EditorGUILayout.LabelField("Document ID", _stagedDocument.Value<string>("id") ?? string.Empty);
            }

            EditorGUILayout.Space(4f);
        }

        private void DrawConflictControls()
        {
            EditorGUILayout.HelpBox(
                "The JSON file changed outside this Inspector while staged edits exist.",
                MessageType.Warning);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Reload JSON"))
                    ReloadFromDisk();
                if (GUILayout.Button("Keep staged"))
                    KeepStagedAfterExternalChange();
            }
        }

        private void DrawObjectContents(
            JObject value,
            JObject rawSchema,
            string pointer,
            bool isRoot)
        {
            JObject schema = MoyvaJsonDocumentValidation.ResolveSchema(rawSchema, _schema);
            JObject properties = schema["properties"] as JObject;
            HashSet<string> required = schema["required"] is JArray requiredArray
                ? new HashSet<string>(requiredArray.Values<string>(), StringComparer.Ordinal)
                : new HashSet<string>(StringComparer.Ordinal);

            foreach (JProperty property in value.Properties().ToList())
            {
                if (isRoot && property.Name == MoyvaJsonDocumentMetadata.Editor)
                    continue;

                JObject propertySchema = properties?[property.Name] as JObject ?? new JObject();
                string childPointer = JoinPointer(pointer, property.Name);
                DrawToken(
                    ObjectNames.NicifyVariableName(property.Name),
                    property.Value,
                    propertySchema,
                    childPointer,
                    replacement => property.Value = replacement);
            }

            DrawPropertyMenus(value, schema, required, pointer, isRoot);
        }

        private void DrawToken(
            string label,
            JToken token,
            JObject rawSchema,
            string pointer,
            Action<JToken> replace)
        {
            JObject schema = MoyvaJsonDocumentValidation.ResolveSchema(rawSchema, _schema);
            if (token == null || token.Type is JTokenType.Null or JTokenType.Undefined)
            {
                DrawNull(label, schema, replace);
                return;
            }

            if (token is JObject objectValue)
            {
                if (objectValue.Property("$asset") != null)
                {
                    DrawAssetReference(label, objectValue, schema);
                    return;
                }

                if (objectValue["$config"] is JObject configReference)
                {
                    DrawConfigReference(label, configReference, schema);
                    return;
                }

                DrawObject(label, objectValue, schema, pointer);
                return;
            }

            if (token is JArray arrayValue)
            {
                DrawArray(label, arrayValue, schema, pointer);
                return;
            }

            if (schema.TryGetValue("const", out JToken constant))
            {
                using (new EditorGUI.DisabledScope(true))
                    EditorGUILayout.TextField(label, constant.ToString(Formatting.None));
                return;
            }

            if (schema["enum"] is JArray enumValues && enumValues.Count > 0)
            {
                DrawEnum(label, token, enumValues, replace);
                return;
            }

            string configModel = schema.Value<string>("x-moyva-configModel");
            if (!string.IsNullOrWhiteSpace(configModel) && token.Type == JTokenType.String)
            {
                replace(new JValue(DrawConfigIdPopup(
                    label,
                    configModel,
                    token.Value<string>())));
                return;
            }

            switch (token.Type)
            {
                case JTokenType.Boolean:
                    replace(new JValue(EditorGUILayout.Toggle(label, token.Value<bool>())));
                    break;
                case JTokenType.Integer:
                    replace(new JValue(EditorGUILayout.LongField(label, token.Value<long>())));
                    break;
                case JTokenType.Float:
                    replace(new JValue(EditorGUILayout.DoubleField(label, token.Value<double>())));
                    break;
                case JTokenType.String:
                    replace(new JValue(EditorGUILayout.TextField(label, token.Value<string>() ?? string.Empty)));
                    break;
                default:
                    EditorGUILayout.LabelField(label, token.ToString(Formatting.None));
                    break;
            }
        }

        private void DrawObject(string label, JObject value, JObject schema, string pointer)
        {
            bool expanded = Foldout(pointer, defaultValue: PointerDepth(pointer) < 2);
            expanded = EditorGUILayout.Foldout(
                expanded,
                string.IsNullOrWhiteSpace(label) ? "Object" : label,
                toggleOnLabelClick: true);
            _foldouts[pointer] = expanded;
            if (!expanded)
                return;

            using (new EditorGUI.IndentLevelScope())
                DrawObjectContents(value, schema, pointer, isRoot: false);
        }

        private void DrawArray(string label, JArray value, JObject schema, string pointer)
        {
            bool expanded = Foldout(pointer, defaultValue: PointerDepth(pointer) < 2);
            expanded = EditorGUILayout.Foldout(
                expanded,
                $"{label} [{value.Count}]",
                toggleOnLabelClick: true);
            _foldouts[pointer] = expanded;
            if (!expanded)
                return;

            JObject itemSchema = schema["items"] as JObject ?? new JObject();
            int removeIndex = -1;
            int moveFrom = -1;
            int moveTo = -1;
            using (new EditorGUI.IndentLevelScope())
            {
                for (int i = 0; i < value.Count; i++)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField($"Element {i}", EditorStyles.miniBoldLabel);
                        using (new EditorGUI.DisabledScope(i == 0))
                        {
                            if (GUILayout.Button("Up", GUILayout.Width(42f)))
                            {
                                moveFrom = i;
                                moveTo = i - 1;
                            }
                        }
                        using (new EditorGUI.DisabledScope(i >= value.Count - 1))
                        {
                            if (GUILayout.Button("Down", GUILayout.Width(48f)))
                            {
                                moveFrom = i;
                                moveTo = i + 1;
                            }
                        }
                        if (GUILayout.Button("Remove", GUILayout.Width(62f)))
                            removeIndex = i;
                    }

                    int capturedIndex = i;
                    DrawToken(
                        string.Empty,
                        value[i],
                        itemSchema,
                        JoinPointer(pointer, i.ToString(CultureInfo.InvariantCulture)),
                        replacement => value[capturedIndex] = replacement);
                    EditorGUILayout.EndVertical();
                }

                if (GUILayout.Button("Add element"))
                    value.Add(CreateDefaultToken(itemSchema));
            }

            if (removeIndex >= 0)
                value.RemoveAt(removeIndex);
            else if (moveFrom >= 0 && moveTo >= 0)
            {
                JToken item = value[moveFrom];
                item.Remove();
                value.Insert(moveTo, item);
            }
        }

        private void DrawAssetReference(string label, JObject value, JObject schema)
        {
            UnityEngine.Object current = MoyvaJsonDocumentValidation.ResolveAssetReference(value);
            Type expectedType = MoyvaJsonDocumentValidation.ResolveType(
                schema.Value<string>("x-moyva-assetType"));

            EditorGUI.BeginChangeCheck();
            UnityEngine.Object selected = EditorGUILayout.ObjectField(
                label,
                current,
                expectedType,
                allowSceneObjects: false);
            if (EditorGUI.EndChangeCheck())
            {
                value["$asset"] = selected != null
                    ? JsonizationEditorUtil.AssetKey(selected)
                    : string.Empty;
                value["editorPath"] = selected != null
                    ? AssetDatabase.GetAssetPath(selected)
                    : string.Empty;
            }

            string path = value.Value<string>("editorPath");
            if (!string.IsNullOrWhiteSpace(path))
                EditorGUILayout.LabelField("Editor path", path, EditorStyles.miniLabel);

            if (selected != null)
                DrawTilePresetModels(selected);
            else if (current != null)
                DrawTilePresetModels(current);
        }

        private void DrawTilePresetModels(UnityEngine.Object preset)
        {
            if (preset == null || preset.GetType().Name != "TilePreset")
                return;

            FieldInfo[] modelFields = preset.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.Public)
                .Where(field => typeof(GameObject).IsAssignableFrom(field.FieldType))
                .Where(field => field.GetValue(preset) is GameObject)
                .ToArray();
            if (modelFields.Length == 0)
                return;

            string key = "preset-models:" + preset.GetInstanceID();
            bool expanded = Foldout(key, defaultValue: false);
            expanded = EditorGUILayout.Foldout(
                expanded,
                $"Preset models [{modelFields.Length}]",
                toggleOnLabelClick: true);
            _foldouts[key] = expanded;
            if (!expanded)
                return;

            using (new EditorGUI.IndentLevelScope())
            using (new EditorGUI.DisabledScope(true))
            {
                for (int i = 0; i < modelFields.Length; i++)
                {
                    var model = modelFields[i].GetValue(preset) as GameObject;
                    EditorGUILayout.ObjectField(
                        ObjectNames.NicifyVariableName(modelFields[i].Name),
                        model,
                        typeof(GameObject),
                        allowSceneObjects: false);
                    EditorGUILayout.LabelField(
                        "Path",
                        AssetDatabase.GetAssetPath(model),
                        EditorStyles.miniLabel);
                }
            }
        }

        private void DrawConfigReference(string label, JObject reference, JObject schema)
        {
            string model = reference.Value<string>("model")
                           ?? schema.Value<string>("x-moyva-configModel")
                           ?? string.Empty;
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                using (new EditorGUI.DisabledScope(true))
                    EditorGUILayout.TextField("Model", model);
                reference["model"] = model;
                reference["id"] = DrawConfigIdPopup(
                    "Config",
                    model,
                    reference.Value<string>("id"));
            }
        }

        private string DrawConfigIdPopup(string label, string model, string currentId)
        {
            string[] ids = ConfigIds(model);
            if (ids.Length == 0)
                return EditorGUILayout.TextField(label, currentId ?? string.Empty);

            int currentIndex = Array.FindIndex(
                ids,
                value => string.Equals(value, currentId, StringComparison.OrdinalIgnoreCase));
            string[] options;
            if (currentIndex >= 0)
            {
                options = ids;
            }
            else
            {
                options = new[] { string.IsNullOrWhiteSpace(currentId) ? "<select>" : currentId }
                    .Concat(ids)
                    .ToArray();
                currentIndex = 0;
            }

            int selected = EditorGUILayout.Popup(label, currentIndex, options);
            return selected >= 0 && selected < options.Length
                ? options[selected]
                : currentId;
        }

        private static void DrawEnum(
            string label,
            JToken current,
            JArray values,
            Action<JToken> replace)
        {
            string[] labels = values.Select(value => value.ToString(Formatting.None).Trim('"')).ToArray();
            int currentIndex = values
                .Select((value, index) => (value, index))
                .Where(pair => JToken.DeepEquals(pair.value, current))
                .Select(pair => pair.index)
                .DefaultIfEmpty(0)
                .First();
            int selected = EditorGUILayout.Popup(label, currentIndex, labels);
            replace(values[Mathf.Clamp(selected, 0, values.Count - 1)].DeepClone());
        }

        private void DrawNull(string label, JObject schema, Action<JToken> replace)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(label, "null");
                if (GUILayout.Button("Create", GUILayout.Width(58f)))
                    replace(CreateDefaultToken(schema, allowNull: false));
            }
        }

        private void DrawPropertyMenus(
            JObject value,
            JObject schema,
            HashSet<string> required,
            string pointer,
            bool isRoot)
        {
            JObject properties = schema["properties"] as JObject;
            if (properties == null)
                return;

            string[] missing = properties.Properties()
                .Select(property => property.Name)
                .Where(name => value.Property(name) == null)
                .ToArray();
            string[] removable = value.Properties()
                .Select(property => property.Name)
                .Where(name => !required.Contains(name))
                .ToArray();
            if (missing.Length == 0 && removable.Length == 0)
                return;

            using (new EditorGUILayout.HorizontalScope())
            {
                if (missing.Length > 0 && GUILayout.Button("Add optional field"))
                {
                    var menu = new GenericMenu();
                    foreach (string propertyName in missing)
                    {
                        string capturedName = propertyName;
                        menu.AddItem(
                            new GUIContent(ObjectNames.NicifyVariableName(capturedName)),
                            false,
                            () =>
                            {
                                value[capturedName] = CreateDefaultToken(
                                    properties[capturedName] as JObject ?? new JObject());
                                Repaint();
                            });
                    }
                    menu.ShowAsContext();
                }

                if (removable.Length > 0 && GUILayout.Button("Remove optional field"))
                {
                    var menu = new GenericMenu();
                    foreach (string propertyName in removable)
                    {
                        string capturedName = propertyName;
                        menu.AddItem(
                            new GUIContent(ObjectNames.NicifyVariableName(capturedName)),
                            false,
                            () =>
                            {
                                value.Remove(capturedName);
                                _foldouts.Remove(JoinPointer(pointer, capturedName));
                                Repaint();
                            });
                    }
                    menu.ShowAsContext();
                }
            }

            if (isRoot)
                EditorGUILayout.Space(6f);
        }

        // Це службова editor-гілка JSON: вона керує тільки `.asset`-лінком і не входить у gameplay payload.
        private void DrawEditorDocumentLinkControls()
        {
            if (_stagedDocument == null)
                return;

            const string foldoutKey = "/editor/documentLink";
            bool expanded = Foldout(foldoutKey, defaultValue: false);
            expanded = EditorGUILayout.Foldout(
                expanded,
                "Editor Document Link",
                toggleOnLabelClick: true);
            _foldouts[foldoutKey] = expanded;
            if (!expanded)
                return;

            MoyvaJsonDocumentLinkTarget target =
                MoyvaJsonDocumentLinkPolicy.Read(_stagedDocument, _sourcePath);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUI.BeginChangeCheck();
                bool enabled = EditorGUILayout.Toggle("Generate .asset link", target.Enabled);
                string path = EditorGUILayout.TextField("Path or folder", target.RawPath);
                if (EditorGUI.EndChangeCheck())
                    MoyvaJsonDocumentLinkPolicy.Write(_stagedDocument, enabled, path);

                MoyvaJsonDocumentLinkTarget preview =
                    MoyvaJsonDocumentLinkPolicy.Read(_stagedDocument, _sourcePath);
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.TextField(
                        "Resolved path",
                        preview.Enabled ? preview.Path : "(disabled)");
                }
            }
        }

        private void DrawIssues()
        {
            if (!string.IsNullOrWhiteSpace(_status))
                EditorGUILayout.HelpBox(_status, _statusType);

            int visibleCount = Math.Min(_issues?.Count ?? 0, 8);
            for (int i = 0; i < visibleCount; i++)
                EditorGUILayout.HelpBox(_issues[i].ToString(), MessageType.Error);
            if ((_issues?.Count ?? 0) > visibleCount)
            {
                EditorGUILayout.LabelField(
                    $"{_issues.Count - visibleCount} more issue(s)",
                    EditorStyles.miniLabel);
            }
        }

        private void DrawApplyControls()
        {
            EditorGUILayout.Space(4f);
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(!IsDirty))
                {
                    if (GUILayout.Button("Apply"))
                        Apply();
                    if (GUILayout.Button("Revert"))
                        Revert();
                }

                if (GUILayout.Button("Validate"))
                    ValidateStaged();
            }
        }

        private void Apply()
        {
            DetectExternalChange();
            if (_externalConflict)
                return;

            _issues = MoyvaJsonDocumentValidation.Validate(_sourcePath, _stagedDocument);
            if (_issues.Count > 0)
            {
                _status = $"Apply blocked: {_issues.Count} validation issue(s).";
                _statusType = MessageType.Error;
                return;
            }

            string formatted = _stagedDocument.ToString(Formatting.Indented) + "\n";
            string temporaryPath = _sourcePath + ".moyva-staged";
            try
            {
                File.WriteAllText(temporaryPath, formatted);
                FileUtil.ReplaceFile(temporaryPath, _sourcePath);
                if (File.Exists(temporaryPath))
                    File.Delete(temporaryPath);

                AssetDatabase.ImportAsset(_sourcePath, ImportAssetOptions.ForceUpdate);
                _baseText = formatted;
                _baseDocument = (JObject)_stagedDocument.DeepClone();
                _status = "Applied to canonical JSON. Runtime snapshots update on the next gameplay context.";
                _statusType = MessageType.Info;
                BuildConfigIndex();
                MoyvaJsonDocumentLinkReconciler.Schedule(syncRuntime: true);
            }
            catch (Exception exception)
            {
                _status = "Apply failed: " + exception.Message;
                _statusType = MessageType.Error;
            }
            finally
            {
                if (File.Exists(temporaryPath))
                    File.Delete(temporaryPath);
            }
        }

        private void Revert()
        {
            if (_baseDocument == null)
                return;
            _stagedDocument = (JObject)_baseDocument.DeepClone();
            _issues = Array.Empty<MoyvaJsonValidationIssue>();
            _status = "Staged changes reverted.";
            _statusType = MessageType.Info;
            GUI.FocusControl(null);
        }

        private void ValidateStaged()
        {
            _issues = MoyvaJsonDocumentValidation.Validate(_sourcePath, _stagedDocument);
            _status = _issues.Count == 0
                ? "Validation passed."
                : $"Validation failed with {_issues.Count} issue(s).";
            _statusType = _issues.Count == 0 ? MessageType.Info : MessageType.Error;
        }

        private void DetectExternalChange()
        {
            if (string.IsNullOrWhiteSpace(_sourcePath) || !File.Exists(_sourcePath))
                return;

            string diskText = File.ReadAllText(_sourcePath);
            if (string.Equals(diskText, _baseText, StringComparison.Ordinal))
                return;

            if (IsDirty)
            {
                _externalConflict = true;
                return;
            }

            ReloadFromDisk();
        }

        private void KeepStagedAfterExternalChange()
        {
            try
            {
                string diskText = File.ReadAllText(_sourcePath);
                _baseText = diskText;
                _baseDocument = JObject.Parse(diskText);
                _externalConflict = false;
                _status = "External JSON acknowledged; staged values are kept.";
                _statusType = MessageType.Warning;
            }
            catch (Exception exception)
            {
                _status = "Could not acknowledge external JSON: " + exception.Message;
                _statusType = MessageType.Error;
            }
        }

        private void ReloadFromDisk()
        {
            _externalConflict = false;
            _issues = Array.Empty<MoyvaJsonValidationIssue>();
            _status = null;
            _sourcePath = _link?.Source != null
                ? AssetDatabase.GetAssetPath(_link.Source)
                : string.Empty;
            if (string.IsNullOrWhiteSpace(_sourcePath) || !File.Exists(_sourcePath))
            {
                _baseText = string.Empty;
                _baseDocument = null;
                _stagedDocument = null;
                _schema = null;
                _status = "Linked JSON file does not exist.";
                _statusType = MessageType.Error;
                return;
            }

            try
            {
                _baseText = File.ReadAllText(_sourcePath);
                _baseDocument = JObject.Parse(_baseText);
                _stagedDocument = (JObject)_baseDocument.DeepClone();
                _schema = MoyvaJsonDocumentValidation.LoadSchema(
                    _sourcePath,
                    _stagedDocument,
                    out string schemaError);
                if (_schema == null)
                    throw new InvalidOperationException(schemaError);
                BuildConfigIndex();
            }
            catch (Exception exception)
            {
                _baseDocument = null;
                _stagedDocument = null;
                _schema = null;
                _status = "JSON load failed: " + exception.Message;
                _statusType = MessageType.Error;
            }
        }

        private JToken CreateDefaultToken(JObject rawSchema, bool allowNull = true)
        {
            JObject schema = MoyvaJsonDocumentValidation.ResolveSchema(rawSchema, _schema);
            if (schema.TryGetValue("default", out JToken defaultValue))
                return defaultValue.DeepClone();
            if (schema.TryGetValue("const", out JToken constant))
                return constant.DeepClone();
            if (schema["enum"] is JArray enumValues && enumValues.Count > 0)
                return enumValues[0].DeepClone();

            string type = SelectType(schema["type"], allowNull);
            switch (type)
            {
                case "object":
                {
                    var result = new JObject();
                    JObject properties = schema["properties"] as JObject;
                    if (schema["required"] is JArray required && properties != null)
                    {
                        foreach (string propertyName in required.Values<string>())
                        {
                            if (properties[propertyName] is JObject propertySchema)
                                result[propertyName] = CreateDefaultToken(propertySchema, allowNull: false);
                        }
                    }
                    return result;
                }
                case "array":
                    return new JArray();
                case "boolean":
                    return new JValue(false);
                case "integer":
                    return new JValue((long)(schema.Value<double?>("minimum") ?? 0d));
                case "number":
                    return new JValue(schema.Value<double?>("minimum") ?? 0d);
                case "null":
                    return JValue.CreateNull();
                default:
                    return new JValue(string.Empty);
            }
        }

        private void BuildConfigIndex()
        {
            _configIdsByModel.Clear();
            MoyvaJsonDocumentIndex index = MoyvaJsonDocumentIndex.Build(
                _sourcePath,
                _stagedDocument);
            foreach (KeyValuePair<string, string[]> pair in index.IdsByModel)
                _configIdsByModel[pair.Key] = pair.Value;
        }

        private string[] ConfigIds(string model)
            => !string.IsNullOrWhiteSpace(model)
               && _configIdsByModel.TryGetValue(model, out string[] ids)
                ? ids
                : Array.Empty<string>();

        private bool Foldout(string key, bool defaultValue)
            => _foldouts.TryGetValue(key, out bool value) ? value : defaultValue;

        private static int PointerDepth(string pointer)
            => string.IsNullOrWhiteSpace(pointer) ? 0 : pointer.Count(character => character == '/');

        private static string SelectType(JToken typeToken, bool allowNull)
        {
            if (typeToken is JArray types)
            {
                string selected = types.Values<string>()
                    .FirstOrDefault(value => allowNull || value != "null");
                return selected ?? "null";
            }
            return typeToken?.Value<string>() ?? "string";
        }

        private static string JoinPointer(string pointer, string segment)
        {
            string escaped = (segment ?? string.Empty).Replace("~", "~0").Replace("/", "~1");
            return string.IsNullOrEmpty(pointer) ? "/" + escaped : pointer + "/" + escaped;
        }
    }
}
