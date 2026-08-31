using System;
using Kruty1918.Moyva.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityHTML.Runtime;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    [ExecuteAlways]
    public class HomeMenuMoyvaUiAnchor : MonoBehaviour
    {
        private enum EditorPreviewRoute
        {
            Main,
            Play,
            Continue,
            Multiplayer,
            CreateRoom,
            JoinRoom,
            WorldSetup,
            Lobby,
            Settings,
            KickPlayers
        }

        [SerializeField] private RectTransform _mountRoot;
        [SerializeField] private GameObject _legacyShell;
        [SerializeField] private GameObject[] _legacyUiRoots = Array.Empty<GameObject>();
        [SerializeField] private TextAsset _htmlAsset;
        [SerializeField] private TextAsset _cssAsset;
        [SerializeField] private TMP_FontAsset _fontAsset;
        [SerializeField] private bool _editorLivePreview = false;
        [SerializeField] private EditorPreviewRoute _editorPreviewRoute = EditorPreviewRoute.Main;

        private IUnityHtmlHost _editorPreviewHost;
        private int _editorPreviewSignature;
        private int _lastLayoutScreenWidth = -1;
        private int _lastLayoutScreenHeight = -1;
        private Rect _lastLayoutSafeArea;
        private Vector2 _lastLayoutRootSize;

        public RectTransform MountRoot => _mountRoot;
        public GameObject LegacyShell => _legacyShell;
        public TextAsset HtmlAsset => _htmlAsset;
        public TextAsset CssAsset => _cssAsset;
        public TMP_FontAsset FontAsset => _fontAsset;
        public bool EditorLivePreview => _editorLivePreview;
        public string CurrentViewportClass => ResolveViewportClass(_mountRoot);

        public void PrepareForMount()
        {
            if (_mountRoot == null)
                return;

            var rootTransform = _mountRoot.transform;
            if (_legacyShell != null && _legacyShell.transform.parent == rootTransform.parent)
                rootTransform.SetSiblingIndex(_legacyShell.transform.GetSiblingIndex());

            _mountRoot.localScale = Vector3.one;
            _mountRoot.gameObject.SetActive(true);
            ApplyViewportLayoutNow();
        }

        public bool ApplyViewportLayoutNow()
        {
            if (_mountRoot == null)
                return false;

            var parent = _mountRoot.parent as RectTransform;
            if (parent == null)
                return false;

            var canvas = _mountRoot.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                var scaler = canvas.GetComponent<CanvasScaler>();
                if (scaler != null)
                    UiCanvasScalePolicy.Apply(canvas, scaler);
            }

            var safeRect = HomeMenuViewportUtility.CalculateSafeRectInParent(parent, canvas);
            HomeMenuViewportUtility.StretchToRect(_mountRoot, safeRect);
            RecordLayoutSignature();
            return true;
        }

        public void SetMoyvaUiVisible(bool visible)
        {
            if (_mountRoot != null && _mountRoot.gameObject.activeSelf != visible)
                _mountRoot.gameObject.SetActive(visible);
        }

        public void SetHtmlShellVisible(bool visible) => SetMoyvaUiVisible(visible);

        public void SetLegacyUiVisible(bool visible)
        {
            if (_legacyUiRoots != null)
            {
                for (var i = 0; i < _legacyUiRoots.Length; i++)
                    SetActiveIfAssigned(_legacyUiRoots[i], visible);
            }

            SetActiveIfAssigned(_legacyShell, visible);
            SetAutoDetectedLegacyUiVisible(visible);
        }

        public void StopEditorPreview()
        {
            DisposeEditorPreview();
        }

        private void OnEnable()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged -= HandleEditorPlayModeStateChanged;
            UnityEditor.EditorApplication.playModeStateChanged += HandleEditorPlayModeStateChanged;
#endif
            if (!Application.isPlaying && _editorLivePreview)
                TryUpdateEditorPreview(force: true);
            else if (!Application.isPlaying)
                DisposeEditorPreview();
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged -= HandleEditorPlayModeStateChanged;
#endif
            if (!Application.isPlaying)
                DisposeEditorPreview();
        }

        private void OnValidate()
        {
            if (!Application.isPlaying && _editorLivePreview)
                _editorPreviewSignature = 0;
        }

        private void Update()
        {
            if (Application.isPlaying && LayoutSignatureChanged())
                ApplyViewportLayoutNow();

            if (!Application.isPlaying && _editorLivePreview)
                TryUpdateEditorPreview(force: false);
        }

        internal void ConfigureForTests(
            RectTransform mountRoot,
            GameObject legacyShell,
            TextAsset htmlAsset,
            TextAsset cssAsset)
        {
            _mountRoot = mountRoot;
            _legacyShell = legacyShell;
            _htmlAsset = htmlAsset;
            _cssAsset = cssAsset;
        }

        private void TryUpdateEditorPreview(bool force)
        {
            if (!_editorLivePreview || _mountRoot == null || _htmlAsset == null || _cssAsset == null)
            {
                DisposeEditorPreview();
                return;
            }

            var signature = 17;
            unchecked
            {
                signature = signature * 31 + _mountRoot.GetInstanceID();
                signature = signature * 31 + _htmlAsset.GetInstanceID();
                signature = signature * 31 + _cssAsset.GetInstanceID();
                signature = signature * 31 + (_fontAsset != null ? _fontAsset.GetInstanceID() : 0);
                signature = signature * 31 + (_htmlAsset.text != null ? _htmlAsset.text.Length : 0);
                signature = signature * 31 + (_cssAsset.text != null ? _cssAsset.text.Length : 0);
                signature = signature * 31 + Mathf.RoundToInt(_mountRoot.rect.width);
                signature = signature * 31 + Mathf.RoundToInt(_mountRoot.rect.height);
                signature = signature * 31 + (int)_editorPreviewRoute;
            }

            if (!force && signature == _editorPreviewSignature)
                return;

            _editorPreviewSignature = signature;
            PrepareForMount();
            _editorPreviewHost ??= new UnityHtmlHost();
            var globals = new System.Collections.Generic.Dictionary<string, object>
            {
                ["moyvaMenu"] = new EditorPreviewBridge()
            };

            if (_fontAsset != null)
                globals["moyvaFont"] = _fontAsset;

            _editorPreviewHost.Mount(
                _mountRoot,
                new UnityHtmlDocument(BuildEditorPreviewHtml(), _cssAsset.text, "MoyvaUI Editor Preview"),
                globals);
        }

        private string BuildEditorPreviewHtml()
        {
            var previewState = new HomeMenuMoyvaUiState();
            var previewView = new HomeMenuMoyvaUiViewController(previewState);
            try
            {
                previewState.Open(ToPanelName(_editorPreviewRoute));
                return HomeMenuMoyvaUiMarkup.Build(
                    previewState,
                    previewView,
                    CurrentViewportClass);
            }
            finally
            {
                previewView.Dispose();
            }
        }

        private void DisposeEditorPreview()
        {
            _editorPreviewHost?.Dispose();
            _editorPreviewHost = null;
            _editorPreviewSignature = 0;
        }

#if UNITY_EDITOR
        private void HandleEditorPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingEditMode ||
                state == UnityEditor.PlayModeStateChange.EnteredPlayMode)
                DisposeEditorPreview();
        }
#endif

        private void SetAutoDetectedLegacyUiVisible(bool visible)
        {
            var parent = _mountRoot != null ? _mountRoot.parent : transform.parent;
            if (parent == null)
                return;

            for (var i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (child == null || child == _mountRoot || child == transform)
                    continue;

                if (IsLegacyUiRootName(child.name))
                    child.gameObject.SetActive(visible);
            }
        }

        private static bool SetActiveIfAssigned(GameObject target, bool visible)
        {
            if (target == null)
                return false;

            if (target.activeSelf != visible)
                target.SetActive(visible);
            return true;
        }

        private static bool IsLegacyUiRootName(string name)
        {
            return string.Equals(name, "SidePanel", StringComparison.Ordinal) ||
                   string.Equals(name, "ContentArea", StringComparison.Ordinal) ||
                   string.Equals(name, "ConfirmDialog", StringComparison.Ordinal) ||
                   string.Equals(name, "InfoPanel", StringComparison.Ordinal) ||
                   string.Equals(name, "Loading", StringComparison.Ordinal) ||
                   string.Equals(name, "Overlay", StringComparison.Ordinal) ||
                   string.Equals(name, "RuntimeHomeMenuPanels", StringComparison.Ordinal);
        }

        private sealed class EditorPreviewBridge
        {
            public void Play() { }
            public void Continue() { }
            public void Multiplayer() { }
            public void Settings() { }
            public void Exit() { }
            public void WorldSetup() { }
            public void Back() { }
            public void BackForce() { }
            public void CreateLan() { }
            public void CreateGlobal() { }
            public void JoinLan() { }
            public void JoinGlobal() { }
            public void SetModeLan() { }
            public void SetModeGlobal() { }
            public void CreateRoom() { }
            public void TogglePublic() { }
            public void MaxPlayersMinus() { }
            public void MaxPlayersPlus() { }
            public void CreateWorld() { }
            public void RandomSeed() { }
            public void WorldSmall() { }
            public void WorldMedium() { }
            public void WorldLarge() { }
            public void MapContinents() { }
            public void MapPangaea() { }
            public void MapIslands() { }
            public void MapHighlands() { }
            public void DifficultyEasy() { }
            public void DifficultyNormal() { }
            public void DifficultyHard() { }
            public void DifficultyInsane() { }
            public void StartGame() { }
            public void LeaveLobby() { }
            public void RefreshRooms() { }
            public void JoinTypedRoom() { }
            public void SelectSlot(int index) { }
            public void SelectRoom(int index) { }
            public void RefreshKickPlayers() { }
            public void CloseKickPlayers() { }
            public void KickPlayer(int index) { }
            public void ChangePlayerName() { }
            public void MasterLow() { }
            public void MasterMid() { }
            public void MasterHigh() { }
            public void MusicLow() { }
            public void MusicHigh() { }
            public void SfxLow() { }
            public void SfxHigh() { }
            public void UiLow() { }
            public void UiHigh() { }
            public void ToggleMuted() { }
            public void GraphicsAuto() { }
            public void GraphicsPerformance() { }
            public void GraphicsBalanced() { }
            public void GraphicsQuality() { }
            public void RenderScaleDown() { }
            public void RenderScaleUp() { }
            public void FrameRateDown() { }
            public void FrameRateUp() { }
            public void ToggleVSync() { }
            public void ToggleShadows() { }
            public void ToggleAnisotropic() { }
            public void ResetGraphics() { }
            public void DeleteSaves() { }
            public void Confirm() { }
            public void Cancel() { }
            public void AcknowledgeInfo() { }
            public void PasswordEmpty() { }
            public void PasswordDemo() { }
            public void ConfirmPassword() { }
            public void CancelPassword() { }
        }

        private static string ToPanelName(EditorPreviewRoute route)
        {
            return route switch
            {
                EditorPreviewRoute.Play => "PlayModePanel",
                EditorPreviewRoute.Continue => "ContinuePanel",
                EditorPreviewRoute.Multiplayer => "SelectMultiplayerType",
                EditorPreviewRoute.CreateRoom => "CreateRoomPanel",
                EditorPreviewRoute.JoinRoom => "JoinRoomPanel",
                EditorPreviewRoute.WorldSetup => "WorldSetupPanel",
                EditorPreviewRoute.Lobby => "LobbyPanel",
                EditorPreviewRoute.Settings => "SettingsPanel",
                EditorPreviewRoute.KickPlayers => "KickPlayerPanel",
                _ => string.Empty
            };
        }

        private bool LayoutSignatureChanged()
        {
            if (_mountRoot == null)
                return false;

            var size = _mountRoot.rect.size;
            return _lastLayoutScreenWidth != Screen.width ||
                   _lastLayoutScreenHeight != Screen.height ||
                   _lastLayoutSafeArea != Screen.safeArea ||
                   Vector2.SqrMagnitude(size - _lastLayoutRootSize) > 0.25f;
        }

        private void RecordLayoutSignature()
        {
            _lastLayoutScreenWidth = Screen.width;
            _lastLayoutScreenHeight = Screen.height;
            _lastLayoutSafeArea = Screen.safeArea;
            _lastLayoutRootSize = _mountRoot != null ? _mountRoot.rect.size : Vector2.zero;
        }

        internal static string ResolveViewportClass(RectTransform root)
        {
            var size = root != null ? root.rect.size : new Vector2(Screen.width, Screen.height);
            return HomeMenuViewportUtility.ResolveViewportClass(size);
        }
    }
}
