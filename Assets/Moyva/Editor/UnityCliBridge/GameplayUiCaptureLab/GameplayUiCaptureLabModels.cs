#if MOYVA_LEGACY_SCRIPTABLEOBJECT_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Editor.UnityCliBridge.GameplayUiCaptureLab
{
    [Serializable]
    internal sealed class CaptureLabResult
    {
        public bool ok = true;
        public string operation;
        public string message;
        public string path;
        public string view;
        public string preset;
        public int width;
        public int height;
        public float pixelsPerPoint;
        public int frameDelay;
    }

    [Serializable]
    internal sealed class VisualStateResult
    {
        public bool ok = true;
        public string scene;
        public bool isPlaying;
        public int screenWidth;
        public int screenHeight;
        public float pixelsPerPoint;
        public WindowState gameView;
        public WindowState sceneView;
        public List<CanvasState> canvases = new();
        public List<UiState> ui = new();
    }

    [Serializable]
    internal sealed class WindowState
    {
        public bool exists;
        public bool focused;
        public float x;
        public float y;
        public float width;
        public float height;
        public string title;
    }

    [Serializable]
    internal sealed class CanvasState
    {
        public string path;
        public bool active;
        public string renderMode;
        public int sortingOrder;
        public float scaleFactor;
        public string worldCamera;
        public Vector2 referenceResolution;
        public float matchWidthOrHeight;
    }

    [Serializable]
    internal sealed class UiState
    {
        public string path;
        public bool activeSelf;
        public bool activeInHierarchy;
        public Vector2 anchorMin;
        public Vector2 anchorMax;
        public Vector2 pivot;
        public Vector2 anchoredPosition;
        public Vector2 sizeDelta;
        public Vector2 screenMin;
        public Vector2 screenMax;
        public bool hasGraphic;
        public Color graphicColor;
        public bool raycastTarget;
        public string text;
        public float fontSize;
        public bool interactable;
    }
}

#endif
