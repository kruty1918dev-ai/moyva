using System;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kruty1918.Moyva.Jsonization.Editor
{
    public sealed class MoyvaJsonConfigEditorWindow : EditorWindow
    {
        private TextField _path;
        private TextField _text;
        private Label _status;

        [MenuItem("Moyva/JSON/Config Editor")]
        public static void OpenWindow()
        {
            var w = GetWindow<MoyvaJsonConfigEditorWindow>();
            w.titleContent = new GUIContent("Moyva JSON");
            w.minSize = new Vector2(700, 500);
        }

        public void CreateGUI()
        {
            rootVisualElement.style.paddingLeft = 8;
            rootVisualElement.style.paddingRight = 8;
            rootVisualElement.style.paddingTop = 8;
            var toolbar = new VisualElement { style = { flexDirection = FlexDirection.Row } };
            _path = new TextField { style = { flexGrow = 1 } };
            var open = new Button(OpenFile) { text = "Open JSON" };
            var save = new Button(SaveFile) { text = "Validate + Save" };
            toolbar.Add(_path); toolbar.Add(open); toolbar.Add(save);
            _text = new TextField { multiline = true };
            _text.style.flexGrow = 1;
            _text.style.whiteSpace = WhiteSpace.Normal;
            _status = new Label("JSON is the source of truth. Unity Inspector is not used for Moyva gameplay configuration.");
            rootVisualElement.Add(toolbar);
            rootVisualElement.Add(_status);
            rootVisualElement.Add(_text);
        }

        private void OpenFile()
        {
            string absolute = EditorUtility.OpenFilePanel("Open Moyva JSON", Path.GetFullPath("Assets/Moyva/Presets"), "json");
            if (string.IsNullOrWhiteSpace(absolute)) return;
            _path.value = ToProjectRelative(absolute);
            _text.value = File.ReadAllText(absolute);
            ValidateText();
        }

        private void SaveFile()
        {
            try
            {
                JObject token = JObject.Parse(_text.value ?? string.Empty);
                string path = _path.value;
                if (string.IsNullOrWhiteSpace(path)) throw new InvalidOperationException("No JSON file selected.");
                File.WriteAllText(path, token.ToString(Newtonsoft.Json.Formatting.Indented) + "\n");
                AssetDatabase.Refresh();
                _status.text = "PASS — valid JSON saved: " + path;
            }
            catch (Exception ex)
            {
                _status.text = "FAIL — " + ex.Message;
            }
        }

        private void ValidateText()
        {
            try
            {
                JObject root = JObject.Parse(_text.value ?? string.Empty);
                _status.text = $"PASS — schema={root.Value<string>("schema")} id={root.Value<string>("id")}";
            }
            catch (Exception ex) { _status.text = "FAIL — " + ex.Message; }
        }

        private static string ToProjectRelative(string absolute)
        {
            string project = Path.GetFullPath(".").Replace('\\','/').TrimEnd('/');
            string normalized = Path.GetFullPath(absolute).Replace('\\','/');
            return normalized.StartsWith(project + "/", StringComparison.OrdinalIgnoreCase)
                ? normalized.Substring(project.Length + 1)
                : normalized;
        }
    }
}
