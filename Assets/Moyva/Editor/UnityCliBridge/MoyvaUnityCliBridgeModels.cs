using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Editor.UnityCliBridge
{
    [Serializable]
    internal sealed class BridgeResult
    {
        public bool ok = true;
        public string operation;
        public string message;
        public string scene;
        public string scenePath;
        public string projectPath;
        public string unityVersion;
        public bool isPlaying;
        public bool isCompiling;
        public bool isUpdating;
        public string backupPath;
        public int count;
        public List<UiNodeSnapshot> nodes = new();
    }

    [Serializable]
    internal sealed class UiNodeSnapshot
    {
        public string name;
        public string path;
        public string globalObjectId;
        public bool activeSelf;
        public bool activeInHierarchy;
        public string tag;
        public int layer;
        public string[] components;

        public bool hasRectTransform;
        public Vector2 anchorMin;
        public Vector2 anchorMax;
        public Vector2 pivot;
        public Vector2 anchoredPosition;
        public Vector2 sizeDelta;
        public Vector3 localScale;

        public bool isCanvas;
        public string canvasRenderMode;
        public int canvasSortingOrder;

        public bool isImage;
        public string imageSprite;
        public string imageSpritePath;
        public Color imageColor;
        public bool imageRaycastTarget;

        public bool isText;
        public string text;
        public float fontSize;
        public Color textColor;
        public string fontAsset;

        public bool isButton;
        public bool buttonInteractable;
        public int buttonPersistentListenerCount;

        public bool isLayoutGroup;
        public string layoutGroupType;
        public bool hasContentSizeFitter;
        public string contentSizeHorizontal;
        public string contentSizeVertical;

        public string prefabAssetPath;
    }
}
