using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Editor.UnityCliBridge.GameplayUiAudit
{
    [Serializable]
    internal sealed class GameplayUiAuditReport
    {
        public string schema = "moyva.gameplay-ui-audit.v1";
        public string generatedAtUtc;
        public AuditProject project = new();
        public AuditScene scene = new();
        public List<AuditCanvas> canvases = new();
        public List<AuditUiNode> uiNodes = new();
        public List<AuditSerializedComponent> serializedComponents = new();
        public List<AuditEventBinding> events = new();
        public List<AuditUiPrefab> uiPrefabs = new();
        public List<AuditAsset> sprites = new();
        public List<AuditAsset> fonts = new();
        public List<AuditAsset> materials = new();
        public List<AuditSourceArea> sourceAreas = new();
        public List<AuditIssue> issues = new();
        public AuditScreenshots screenshots = new();
        public AuditSummary summary = new();
    }

    [Serializable]
    internal sealed class AuditProject
    {
        public string projectPath;
        public string unityVersion;
        public string activeBuildTarget;
        public string colorSpace;
        public string renderPipelineAsset;
        public string renderPipelineAssetPath;
        public string serializationMode;
        public string activeScenePath;
        public bool isPlaying;
        public bool isCompiling;
        public bool isUpdating;
        public Vector2 gameViewSize;
        public int assetPathCount;
    }

    [Serializable]
    internal sealed class AuditScene
    {
        public string targetScenePath;
        public string name;
        public string path;
        public bool loaded;
        public bool active;
        public bool dirty;
        public int rootCount;
        public int totalObjectCount;
        public int totalComponentCount;
        public string[] rootNames;
    }

    [Serializable]
    internal sealed class AuditCanvas
    {
        public string path;
        public string globalObjectId;
        public bool active;
        public string renderMode;
        public int sortingOrder;
        public bool overrideSorting;
        public int targetDisplay;
        public bool pixelPerfect;
        public string worldCamera;
        public bool hasScaler;
        public string scalerMode;
        public Vector2 referenceResolution;
        public float matchWidthOrHeight;
        public float referencePixelsPerUnit;
        public float scaleFactor;
    }

    [Serializable]
    internal sealed class AuditUiNode
    {
        public string name;
        public string path;
        public string parentPath;
        public string globalObjectId;
        public string prefabSourcePath;
        public bool activeSelf;
        public bool activeInHierarchy;
        public int siblingIndex;
        public int layer;
        public string tag;
        public string[] components;

        public Vector2 anchorMin;
        public Vector2 anchorMax;
        public Vector2 pivot;
        public Vector2 anchoredPosition;
        public Vector2 sizeDelta;
        public Vector3 localScale;
        public Vector2 rectSize;

        public bool screenRectValid;
        public Rect screenRect;

        public bool hasCanvasGroup;
        public float canvasGroupAlpha;
        public bool canvasGroupInteractable;
        public bool canvasGroupBlocksRaycasts;

        public bool hasGraphic;
        public string graphicType;
        public Color graphicColor;
        public bool graphicRaycastTarget;
        public string graphicMaterialPath;

        public bool hasImage;
        public string imageSpriteName;
        public string imageSpritePath;
        public string imageType;
        public bool imagePreserveAspect;
        public float imageFillAmount;

        public bool hasRawImage;
        public string rawTextureName;
        public string rawTexturePath;

        public bool hasText;
        public string text;
        public float fontSize;
        public string fontAssetName;
        public string fontAssetPath;
        public string textAlignment;
        public string overflowMode;
        public Color textColor;

        public bool hasSelectable;
        public string selectableType;
        public bool interactable;
        public string transition;
        public string navigationMode;
        public string targetGraphicPath;

        public bool hasScrollRect;
        public bool scrollHorizontal;
        public bool scrollVertical;
        public string scrollViewportPath;
        public string scrollContentPath;

        public bool hasLayoutGroup;
        public string layoutGroupType;
        public string layoutDetails;
        public bool hasLayoutElement;
        public string layoutElementDetails;
        public bool hasContentSizeFitter;
        public string contentSizeFitterDetails;
        public bool hasAspectRatioFitter;
        public string aspectRatioFitterDetails;
    }

    [Serializable]
    internal sealed class AuditSerializedComponent
    {
        public string objectPath;
        public string componentType;
        public string scriptPath;
        public List<AuditSerializedProperty> properties = new();
    }

    [Serializable]
    internal sealed class AuditSerializedProperty
    {
        public string propertyPath;
        public string displayName;
        public string propertyType;
        public string value;
        public string referenceType;
        public string referencePath;
        public string referenceHierarchyPath;
    }

    [Serializable]
    internal sealed class AuditEventBinding
    {
        public string objectPath;
        public string componentType;
        public string eventName;
        public int listenerIndex;
        public string targetName;
        public string targetType;
        public string targetPath;
        public string targetHierarchyPath;
        public string methodName;
    }

    [Serializable]
    internal sealed class AuditUiPrefab
    {
        public string name;
        public string path;
        public int rectTransformCount;
        public int graphicCount;
        public int selectableCount;
        public int tmpTextCount;
        public string[] projectComponentTypes;
    }

    [Serializable]
    internal sealed class AuditAsset
    {
        public string name;
        public string path;
        public string type;
        public string detail;
        public Vector2 size;
    }

    [Serializable]
    internal sealed class AuditSourceArea
    {
        public string label;
        public string root;
        public int scriptCount;
        public string[] scripts;
    }

    [Serializable]
    internal sealed class AuditIssue
    {
        public string severity;
        public string code;
        public string objectPath;
        public string message;
        public string detail;
    }

    [Serializable]
    internal sealed class AuditScreenshots
    {
        public string gameViewPath;
        public string sceneViewPath;
        public bool gameViewRequested;
        public bool sceneViewCaptured;
        public string note;
    }

    [Serializable]
    internal sealed class AuditSummary
    {
        public int canvases;
        public int uiNodes;
        public int buttons;
        public int texts;
        public int images;
        public int scrollRects;
        public int serializedProjectComponents;
        public int serializedProperties;
        public int eventBindings;
        public int uiPrefabs;
        public int sprites;
        public int fonts;
        public int issuesInfo;
        public int issuesWarning;
        public int issuesError;
    }

    [Serializable]
    internal sealed class AuditCommandResult
    {
        public bool ok;
        public string message;
        public string targetScene;
        public string reportJson;
        public string gameViewScreenshot;
        public string sceneViewScreenshot;
        public int uiNodes;
        public int issues;
    }
}
