using System;
using System.Collections.Generic;
using System.IO;
using Kruty1918.Telemetry.Serialization;

namespace Kruty1918.Telemetry.Identity
{
    /// <summary>Tiny persistence abstraction for pseudonymous ids + config cache.</summary>
    public interface IKeyValueStore
    {
        string Get(string key);
        void Set(string key, string value);
        void Delete(string key);
    }

    /// <summary>
    /// File-backed key-value store (JSON file, atomic write via tmp+rename).
    /// Deliberately NOT Unity PlayerPrefs: needs atomicity and larger values.
    /// </summary>
    public sealed class FileKeyValueStore : IKeyValueStore
    {
        private readonly string _path;
        private readonly object _gate = new object();
        private Dictionary<string, string> _map;

        public FileKeyValueStore(string path) { _path = path; }

        private Dictionary<string, string> Map
        {
            get
            {
                if (_map == null)
                {
                    _map = new Dictionary<string, string>(StringComparer.Ordinal);
                    try
                    {
                        if (File.Exists(_path))
                        {
                            var json = TelemetryJsonReader.Parse(File.ReadAllText(_path));
                            if (json.IsObject)
                                foreach (var kv in json.Obj)
                                    if (kv.Value.Raw is string s) _map[kv.Key] = s;
                        }
                    }
                    catch { _map = new Dictionary<string, string>(StringComparer.Ordinal); }
                }
                return _map;
            }
        }

        public string Get(string key)
        {
            lock (_gate) return Map.TryGetValue(key, out var v) ? v : null;
        }

        public void Set(string key, string value)
        {
            lock (_gate)
            {
                Map[key] = value;
                Save();
            }
        }

        public void Delete(string key)
        {
            lock (_gate)
            {
                if (Map.Remove(key)) Save();
            }
        }

        private void Save()
        {
            var dir = Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            var tmp = _path + ".tmp";
            var w = new TelemetryJsonWriter();
            w.BeginObject();
            foreach (var kv in _map) w.Field(kv.Key, kv.Value);
            w.EndObject();
            File.WriteAllText(tmp, w.ToString());
            if (File.Exists(_path)) File.Delete(_path);
            File.Move(tmp, _path);
        }
    }

    /// <summary>Creates and persists the pseudonymous installation id.</summary>
    public sealed class InstallationIdentity
    {
        public const string KeyName = "telemetry.installationId";

        public static string GetOrCreate(IKeyValueStore store)
        {
            var id = store.Get(KeyName);
            if (string.IsNullOrEmpty(id) || id.Length != 32)
            {
                id = Guid.NewGuid().ToString("N");
                store.Set(KeyName, id);
            }
            return id;
        }
    }
}
