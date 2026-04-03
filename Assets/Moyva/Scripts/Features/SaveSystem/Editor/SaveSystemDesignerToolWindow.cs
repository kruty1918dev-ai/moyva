using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.SaveSystem.Editor
{
    public sealed class SaveSystemDesignerToolWindow : EditorWindow
    {
        private const string Magic = "MVSA";

        private enum FileMode
        {
            Slot,
            Config,
            CustomPath,
        }

        private sealed class ParsedBlock
        {
            public uint BlockId;
            public byte[] Payload;
            public uint StoredBlockCrc;
            public uint ActualBlockCrc;
        }

        private sealed class ParsedFile
        {
            public string Path;
            public ushort Version;
            public List<ParsedBlock> Blocks = new List<ParsedBlock>();
            public bool GlobalCrcOk;
            public uint StoredGlobalCrc;
            public uint ActualGlobalCrc;
            public string ParseWarning;
        }

        private FileMode _fileMode = FileMode.Slot;
        private int _slot = 0;
        private string _customPath = string.Empty;
        private Vector2 _scroll;

        private ParsedFile _currentFile;
        private int _selectedBlockIndex = -1;

        private string _payloadUtf8Editor = string.Empty;
        private string _payloadHexEditor = string.Empty;

        [MenuItem("Moyva/Save System/Designer Tool")]
        public static void OpenWindow()
        {
            var window = GetWindow<SaveSystemDesignerToolWindow>("Save Designer Tool");
            window.minSize = new Vector2(900, 600);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Save System Designer Tool", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Інструмент для дизайнерів: перегляд/редагування .mvs файлів, блоків, слотів і config.",
                MessageType.Info);

            DrawFileSelector();

            EditorGUILayout.Space(8);
            using (new EditorGUI.DisabledScope(_currentFile == null))
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Зберегти поточний файл", GUILayout.Height(28)))
                    SaveCurrentFile();

                if (GUILayout.Button("Видалити поточний файл", GUILayout.Height(28)))
                    DeleteCurrentFile();

                if (GUILayout.Button("Очистити всі блоки", GUILayout.Height(28)))
                    ClearAllBlocks();

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(8);
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            DrawLoadedFileInfo();
            DrawBlocksList();
            DrawBlockEditor();

            EditorGUILayout.EndScrollView();
        }

        private void DrawFileSelector()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("1) Вибір файлу", EditorStyles.boldLabel);

            _fileMode = (FileMode)EditorGUILayout.EnumPopup("Режим", _fileMode);

            if (_fileMode == FileMode.Slot)
            {
                _slot = EditorGUILayout.IntSlider("Слот", _slot, 0, 99);
            }
            else if (_fileMode == FileMode.CustomPath)
            {
                EditorGUILayout.BeginHorizontal();
                _customPath = EditorGUILayout.TextField("Шлях", _customPath);
                if (GUILayout.Button("...", GUILayout.Width(36)))
                {
                    string picked = EditorUtility.OpenFilePanel(
                        "Вибрати .mvs файл",
                        GetSavesDirectory(),
                        "mvs");
                    if (!string.IsNullOrEmpty(picked))
                        _customPath = picked;
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.LabelField("Поточний шлях", GetSelectedPath());

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Відкрити файл", GUILayout.Height(28)))
                LoadSelectedFile();

            if (GUILayout.Button("Відкрити .bak", GUILayout.Height(28)))
                LoadSelectedBackup();

            if (GUILayout.Button("Видалити слот (файл + .bak + .tmp)", GUILayout.Height(28)))
                DeleteSlotArtifacts();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawLoadedFileInfo()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("2) Інформація про файл", EditorStyles.boldLabel);

            if (_currentFile == null)
            {
                EditorGUILayout.HelpBox("Файл ще не завантажено.", MessageType.None);
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.LabelField("Шлях", _currentFile.Path);
            EditorGUILayout.LabelField("Версія", _currentFile.Version.ToString());
            EditorGUILayout.LabelField("Кількість блоків", _currentFile.Blocks.Count.ToString());
            EditorGUILayout.LabelField(
                "Global CRC",
                _currentFile.GlobalCrcOk
                    ? $"OK ({_currentFile.StoredGlobalCrc:X8})"
                    : $"Помилка (stored={_currentFile.StoredGlobalCrc:X8}, actual={_currentFile.ActualGlobalCrc:X8})");

            if (!string.IsNullOrEmpty(_currentFile.ParseWarning))
                EditorGUILayout.HelpBox(_currentFile.ParseWarning, MessageType.Warning);

            EditorGUILayout.EndVertical();
        }

        private void DrawBlocksList()
        {
            if (_currentFile == null)
                return;

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("3) Блоки", EditorStyles.boldLabel);

            for (int i = 0; i < _currentFile.Blocks.Count; i++)
            {
                ParsedBlock block = _currentFile.Blocks[i];
                bool selected = i == _selectedBlockIndex;

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Toggle(selected, $"#{i}", "Button", GUILayout.Width(40)))
                {
                    if (_selectedBlockIndex != i)
                    {
                        _selectedBlockIndex = i;
                        LoadSelectedBlockToEditors();
                    }
                }

                EditorGUILayout.LabelField($"BlockId: 0x{block.BlockId:X8}", GUILayout.Width(220));
                EditorGUILayout.LabelField($"Payload: {block.Payload.Length} bytes", GUILayout.Width(160));

                bool blockCrcOk = block.StoredBlockCrc == block.ActualBlockCrc;
                EditorGUILayout.LabelField(
                    blockCrcOk
                        ? $"CRC: OK ({block.StoredBlockCrc:X8})"
                        : $"CRC: BAD (stored={block.StoredBlockCrc:X8}, actual={block.ActualBlockCrc:X8})",
                    GUILayout.Width(290));

                if (GUILayout.Button("Видалити", GUILayout.Width(80)))
                {
                    RemoveBlockAt(i);
                    GUIUtility.ExitGUI();
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Додати порожній блок"))
            {
                _currentFile.Blocks.Add(new ParsedBlock
                {
                    BlockId = 0,
                    Payload = Array.Empty<byte>(),
                    StoredBlockCrc = 0,
                    ActualBlockCrc = 0,
                });
                _selectedBlockIndex = _currentFile.Blocks.Count - 1;
                LoadSelectedBlockToEditors();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawBlockEditor()
        {
            if (_currentFile == null || _selectedBlockIndex < 0 || _selectedBlockIndex >= _currentFile.Blocks.Count)
                return;

            ParsedBlock block = _currentFile.Blocks[_selectedBlockIndex];

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("4) Редактор блоку", EditorStyles.boldLabel);

            block.BlockId = (uint)Math.Max(0, EditorGUILayout.LongField("BlockId (uint)", block.BlockId));

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Редагування як UTF-8 текст", EditorStyles.boldLabel);
            _payloadUtf8Editor = EditorGUILayout.TextArea(_payloadUtf8Editor, GUILayout.MinHeight(90));

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Застосувати UTF-8 → Payload"))
            {
                block.Payload = Encoding.UTF8.GetBytes(_payloadUtf8Editor ?? string.Empty);
                block.ActualBlockCrc = ComputeCrc32(block.Payload);
                block.StoredBlockCrc = block.ActualBlockCrc;
                _payloadHexEditor = BytesToHex(block.Payload);
            }
            if (GUILayout.Button("Оновити UTF-8 з Payload"))
            {
                _payloadUtf8Editor = Encoding.UTF8.GetString(block.Payload);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Редагування як HEX", EditorStyles.boldLabel);
            _payloadHexEditor = EditorGUILayout.TextArea(_payloadHexEditor, GUILayout.MinHeight(140));

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Застосувати HEX → Payload"))
            {
                if (TryParseHex(_payloadHexEditor, out var bytes, out var error))
                {
                    block.Payload = bytes;
                    block.ActualBlockCrc = ComputeCrc32(block.Payload);
                    block.StoredBlockCrc = block.ActualBlockCrc;
                    _payloadUtf8Editor = Encoding.UTF8.GetString(block.Payload);
                }
                else
                {
                    EditorUtility.DisplayDialog("HEX помилка", error, "OK");
                }
            }
            if (GUILayout.Button("Оновити HEX з Payload"))
            {
                _payloadHexEditor = BytesToHex(block.Payload);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.HelpBox(
                "Після змін натисни 'Зберегти поточний файл'. CRC перераховується автоматично.",
                MessageType.None);

            EditorGUILayout.EndVertical();
        }

        private void LoadSelectedFile()
        {
            string path = GetSelectedPath();
            if (string.IsNullOrEmpty(path))
            {
                EditorUtility.DisplayDialog("Помилка", "Шлях до файлу порожній.", "OK");
                return;
            }

            if (!File.Exists(path))
            {
                EditorUtility.DisplayDialog("Файл не знайдено", path, "OK");
                return;
            }

            if (!TryParseMvs(path, out var parsed, out var error))
            {
                EditorUtility.DisplayDialog("Помилка читання .mvs", error, "OK");
                return;
            }

            _currentFile = parsed;
            _selectedBlockIndex = _currentFile.Blocks.Count > 0 ? 0 : -1;
            LoadSelectedBlockToEditors();
            Repaint();
        }

        private void LoadSelectedBackup()
        {
            string path = GetSelectedPath() + ".bak";
            if (!File.Exists(path))
            {
                EditorUtility.DisplayDialog("Backup не знайдено", path, "OK");
                return;
            }

            if (!TryParseMvs(path, out var parsed, out var error))
            {
                EditorUtility.DisplayDialog("Помилка читання .bak", error, "OK");
                return;
            }

            _currentFile = parsed;
            _selectedBlockIndex = _currentFile.Blocks.Count > 0 ? 0 : -1;
            LoadSelectedBlockToEditors();
            Repaint();
        }

        private void SaveCurrentFile()
        {
            if (_currentFile == null)
                return;

            try
            {
                byte[] bytes = EncodeMvs(_currentFile.Version, _currentFile.Blocks);
                File.WriteAllBytes(_currentFile.Path, bytes);
                AssetDatabase.Refresh();

                // Reload to validate and refresh calculated CRC labels
                LoadSelectedFile();

                EditorUtility.DisplayDialog("Готово", "Файл успішно збережено.", "OK");
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Помилка збереження", e.Message, "OK");
            }
        }

        private void DeleteCurrentFile()
        {
            if (_currentFile == null)
                return;

            if (!EditorUtility.DisplayDialog(
                    "Видалення файлу",
                    $"Видалити файл?\n{_currentFile.Path}",
                    "Так, видалити",
                    "Скасувати"))
            {
                return;
            }

            try
            {
                if (File.Exists(_currentFile.Path))
                    File.Delete(_currentFile.Path);

                AssetDatabase.Refresh();
                _currentFile = null;
                _selectedBlockIndex = -1;
                _payloadHexEditor = string.Empty;
                _payloadUtf8Editor = string.Empty;
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Помилка видалення", e.Message, "OK");
            }
        }

        private void DeleteSlotArtifacts()
        {
            if (_fileMode != FileMode.Slot)
            {
                EditorUtility.DisplayDialog(
                    "Недоступно",
                    "Видалення слот-артефактів доступне тільки в режимі Slot.",
                    "OK");
                return;
            }

            string basePath = GetSlotPath(_slot);
            if (!EditorUtility.DisplayDialog(
                    "Видалити слот",
                    $"Буде видалено:\n{basePath}\n{basePath}.bak\n{basePath}.tmp",
                    "Так, видалити",
                    "Скасувати"))
            {
                return;
            }

            TryDelete(basePath);
            TryDelete(basePath + ".bak");
            TryDelete(basePath + ".tmp");

            if (_currentFile != null && string.Equals(_currentFile.Path, basePath, StringComparison.OrdinalIgnoreCase))
            {
                _currentFile = null;
                _selectedBlockIndex = -1;
                _payloadHexEditor = string.Empty;
                _payloadUtf8Editor = string.Empty;
            }

            AssetDatabase.Refresh();
        }

        private void ClearAllBlocks()
        {
            if (_currentFile == null)
                return;

            if (!EditorUtility.DisplayDialog(
                    "Очистити блоки",
                    "Видалити ВСІ блоки в поточному файлі?",
                    "Так",
                    "Скасувати"))
            {
                return;
            }

            _currentFile.Blocks.Clear();
            _selectedBlockIndex = -1;
            _payloadHexEditor = string.Empty;
            _payloadUtf8Editor = string.Empty;
        }

        private void RemoveBlockAt(int index)
        {
            if (_currentFile == null)
                return;

            if (index < 0 || index >= _currentFile.Blocks.Count)
                return;

            _currentFile.Blocks.RemoveAt(index);
            if (_currentFile.Blocks.Count == 0)
            {
                _selectedBlockIndex = -1;
                _payloadHexEditor = string.Empty;
                _payloadUtf8Editor = string.Empty;
                return;
            }

            _selectedBlockIndex = Mathf.Clamp(index, 0, _currentFile.Blocks.Count - 1);
            LoadSelectedBlockToEditors();
        }

        private void LoadSelectedBlockToEditors()
        {
            if (_currentFile == null || _selectedBlockIndex < 0 || _selectedBlockIndex >= _currentFile.Blocks.Count)
            {
                _payloadHexEditor = string.Empty;
                _payloadUtf8Editor = string.Empty;
                return;
            }

            ParsedBlock block = _currentFile.Blocks[_selectedBlockIndex];
            _payloadHexEditor = BytesToHex(block.Payload);
            _payloadUtf8Editor = Encoding.UTF8.GetString(block.Payload);
        }

        private static bool TryParseMvs(string path, out ParsedFile parsedFile, out string error)
        {
            parsedFile = null;
            error = null;

            byte[] data;
            try
            {
                data = File.ReadAllBytes(path);
            }
            catch (Exception e)
            {
                error = $"Не вдалося прочитати файл: {e.Message}";
                return false;
            }

            if (data.Length < 14)
            {
                error = $"Файл занадто малий: {data.Length} bytes";
                return false;
            }

            if (data[0] != (byte)'M' || data[1] != (byte)'V' || data[2] != (byte)'S' || data[3] != (byte)'A')
            {
                error = "Невірна сигнатура файлу (очікується MVSA).";
                return false;
            }

            uint storedGlobalCrc = BitConverter.ToUInt32(data, data.Length - 4);
            uint actualGlobalCrc = ComputeCrc32(data, 0, data.Length - 4);

            var result = new ParsedFile
            {
                Path = path,
                GlobalCrcOk = storedGlobalCrc == actualGlobalCrc,
                StoredGlobalCrc = storedGlobalCrc,
                ActualGlobalCrc = actualGlobalCrc,
            };

            try
            {
                using (var ms = new MemoryStream(data))
                using (var br = new BinaryReader(ms))
                {
                    // Header
                    string magic = Encoding.ASCII.GetString(br.ReadBytes(4));
                    if (magic != Magic)
                    {
                        error = "Невірний magic header.";
                        return false;
                    }

                    result.Version = br.ReadUInt16();
                    uint blockCount = br.ReadUInt32();

                    long bodyEnd = data.Length - 4;
                    for (uint i = 0; i < blockCount; i++)
                    {
                        if (ms.Position + 12 > bodyEnd)
                        {
                            result.ParseWarning = "Файл обрізаний: block header виходить за межі.";
                            break;
                        }

                        uint blockId = br.ReadUInt32();
                        uint blockSize = br.ReadUInt32();
                        uint blockCrc = br.ReadUInt32();

                        if (ms.Position + blockSize > bodyEnd)
                        {
                            result.ParseWarning = "Файл обрізаний: block payload виходить за межі.";
                            break;
                        }

                        byte[] payload = br.ReadBytes((int)blockSize);
                        uint actualBlockCrc = ComputeCrc32(payload);

                        result.Blocks.Add(new ParsedBlock
                        {
                            BlockId = blockId,
                            Payload = payload,
                            StoredBlockCrc = blockCrc,
                            ActualBlockCrc = actualBlockCrc,
                        });
                    }
                }
            }
            catch (Exception e)
            {
                error = $"Помилка розбору файлу: {e.Message}";
                return false;
            }

            parsedFile = result;
            return true;
        }

        private static byte[] EncodeMvs(ushort version, List<ParsedBlock> blocks)
        {
            using (var bodyStream = new MemoryStream())
            using (var bw = new BinaryWriter(bodyStream))
            {
                // Header
                bw.Write(Encoding.ASCII.GetBytes(Magic));
                bw.Write(version == 0 ? (ushort)1 : version);
                bw.Write((uint)blocks.Count);

                // Blocks
                foreach (ParsedBlock block in blocks)
                {
                    byte[] payload = block.Payload ?? Array.Empty<byte>();
                    uint blockCrc = ComputeCrc32(payload);

                    bw.Write(block.BlockId);
                    bw.Write((uint)payload.Length);
                    bw.Write(blockCrc);
                    bw.Write(payload);
                }

                bw.Flush();
                byte[] bodyBytes = bodyStream.ToArray();
                uint globalCrc = ComputeCrc32(bodyBytes);

                var finalBytes = new byte[bodyBytes.Length + 4];
                Buffer.BlockCopy(bodyBytes, 0, finalBytes, 0, bodyBytes.Length);
                Buffer.BlockCopy(BitConverter.GetBytes(globalCrc), 0, finalBytes, bodyBytes.Length, 4);
                return finalBytes;
            }
        }

        private string GetSelectedPath()
        {
            switch (_fileMode)
            {
                case FileMode.Slot:
                    return GetSlotPath(_slot);
                case FileMode.Config:
                    return GetConfigPath();
                case FileMode.CustomPath:
                    return _customPath;
                default:
                    return string.Empty;
            }
        }

        private static string GetSavesDirectory()
        {
            return Path.Combine(Application.persistentDataPath, "saves");
        }

        private static string GetSlotPath(int slot)
        {
            return Path.Combine(GetSavesDirectory(), $"slot{slot:D2}.mvs");
        }

        private static string GetConfigPath()
        {
            return Path.Combine(GetSavesDirectory(), "config.mvs");
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveDesignerTool] Не вдалося видалити {path}: {e.Message}");
            }
        }

        private static string BytesToHex(byte[] data)
        {
            if (data == null || data.Length == 0)
                return string.Empty;

            var sb = new StringBuilder(data.Length * 3);
            for (int i = 0; i < data.Length; i++)
            {
                sb.Append(data[i].ToString("X2"));
                if (i < data.Length - 1)
                    sb.Append(' ');
            }
            return sb.ToString();
        }

        private static bool TryParseHex(string input, out byte[] bytes, out string error)
        {
            bytes = null;
            error = null;

            if (string.IsNullOrWhiteSpace(input))
            {
                bytes = Array.Empty<byte>();
                return true;
            }

            string cleaned = input.Replace(" ", string.Empty)
                .Replace("\n", string.Empty)
                .Replace("\r", string.Empty)
                .Replace("\t", string.Empty)
                .Replace("-", string.Empty)
                .Replace("_", string.Empty);

            if (cleaned.Length % 2 != 0)
            {
                error = "HEX має непарну кількість символів.";
                return false;
            }

            try
            {
                bytes = new byte[cleaned.Length / 2];
                for (int i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = Convert.ToByte(cleaned.Substring(i * 2, 2), 16);
                }
                return true;
            }
            catch (Exception e)
            {
                error = $"Некоректний HEX: {e.Message}";
                return false;
            }
        }

        private static uint ComputeCrc32(byte[] data)
        {
            return ComputeCrc32(data, 0, data.Length);
        }

        private static uint ComputeCrc32(byte[] data, int offset, int length)
        {
            uint crc = 0xFFFFFFFFu;
            for (int i = offset; i < offset + length; i++)
            {
                crc ^= data[i];
                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 1u) != 0u)
                        crc = (crc >> 1) ^ 0xEDB88320u;
                    else
                        crc >>= 1;
                }
            }
            return crc ^ 0xFFFFFFFFu;
        }
    }
}
