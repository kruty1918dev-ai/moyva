using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Editor.UnityCliBridge.GameplayUiRedesign
{
    [Serializable]
    internal sealed class GameplayUiRedesignResult
    {
        public bool ok = true;
        public string operation;
        public string message;
        public string scenePath;
        public string backupPath;
        public int changedObjects;
        public int warnings;
        public int errors;
        public List<GameplayUiCheck> checks = new();
    }

    [Serializable]
    internal sealed class GameplayUiCheck
    {
        public string severity;
        public string code;
        public string path;
        public string message;
    }
}
