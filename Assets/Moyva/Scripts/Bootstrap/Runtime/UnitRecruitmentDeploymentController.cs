using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.InputRouting.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.Units.API;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Zenject;
using Object = UnityEngine.Object;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal sealed class UnitRecruitmentDeploymentController :
        IInitializable,
        ITickable,
        IDisposable,
        ITurnBlocker
    {
        private const string LogTag = "[UnitDeployment]";
        private const string PreviewRootName = "UnitDeploymentPreviewRoot";
        private const string ControlsRootName = "UnitDeploymentControls";
        private const float PreviewLift = 0.08f;
        private const float SpritePreviewHeight = 0.95f;
        private const float FallbackPreviewHeight = 0.85f;
        private const int OverlayRenderQueue = 3988;
        private const string TurnBlockReason =
            "Завершіть або скасуйте розміщення готового юніта.";

        private static readonly GameplayInputKind DeploymentInputMask =
            GameplayInputKind.PrimaryPointer
            | GameplayInputKind.SecondaryPointer
            | GameplayInputKind.Placement;

        private static readonly Color PreviewTint =
            new(0.72f, 1f, 0.74f, 0.72f);

        private readonly SignalBus _signalBus;
        private readonly ITurnService _turns;
        private readonly IUnitRecruitmentService _recruitment;
        private readonly IUnitClassConfig _unitConfigs;
        private readonly IGridProjection _gridProjection;
        private readonly IGridService _grid;
        private readonly IScreenToGridConverter _screenToGrid;
        private readonly IWorldPointerGridResolver _pointerGridResolver;
        private readonly IGridActionOverlayService _gridOverlay;
        private readonly IUnitWorldPositionResolver _worldPositionResolver;
        private readonly IGameplayInputPolicy _inputPolicy;
        private readonly IGameModeService _gameModeService;
        private readonly GameplayTurnHudView _hudView;
        private readonly List<GridActionOverlayCell> _overlayCells = new();
        private readonly MaterialPropertyBlock _previewPropertyBlock = new();

        private DeploymentSession _session;
        private Transform _previewRoot;
        private GameObject _previewObject;
        private Material _fallbackPreviewMaterial;
        private Canvas _canvas;
        private RectTransform _controlsRoot;
        private Button _confirmButton;
        private Button _cancelButton;
        private UnityEngine.Camera _camera;
        private bool _confirmInProgress;
        private bool _warnedMissingCamera;

        public UnitRecruitmentDeploymentController(
            SignalBus signalBus,
            ITurnService turns,
            IUnitRecruitmentService recruitment,
            IUnitClassConfig unitConfigs,
            IGridProjection gridProjection,
            [InjectOptional] IGridService grid = null,
            [InjectOptional] IScreenToGridConverter screenToGrid = null,
            [InjectOptional] IWorldPointerGridResolver pointerGridResolver = null,
            [InjectOptional] IGridActionOverlayService gridOverlay = null,
            [InjectOptional] IUnitWorldPositionResolver worldPositionResolver = null,
            [InjectOptional] IGameplayInputPolicy inputPolicy = null,
            [InjectOptional] IGameModeService gameModeService = null,
            [InjectOptional] GameplayTurnHudView hudView = null)
        {
            _signalBus = signalBus;
            _turns = turns;
            _recruitment = recruitment;
            _unitConfigs = unitConfigs;
            _gridProjection = gridProjection;
            _grid = grid;
            _screenToGrid = screenToGrid;
            _pointerGridResolver = pointerGridResolver;
            _gridOverlay = gridOverlay;
            _worldPositionResolver = worldPositionResolver;
            _inputPolicy = inputPolicy;
            _gameModeService = gameModeService;
            _hudView = hudView;
        }

        public void Initialize()
        {
            _turns.StateChanged += OnTurnStateChanged;
            _signalBus.Subscribe<UnitRecruitmentReadyIndicatorClickedSignal>(
                OnReadyIndicatorClicked);
            _signalBus.Subscribe<UnitRecruitmentQueueChangedSignal>(
                OnRecruitmentQueueChanged);
            _signalBus.Subscribe<UnitRecruitmentDeployedSignal>(
                OnRecruitmentDeployed);
            _signalBus.Subscribe<GameModeChangedSignal>(OnGameModeChanged);
        }

        public void Dispose()
        {
            _turns.StateChanged -= OnTurnStateChanged;
            _signalBus.TryUnsubscribe<UnitRecruitmentReadyIndicatorClickedSignal>(
                OnReadyIndicatorClicked);
            _signalBus.TryUnsubscribe<UnitRecruitmentQueueChangedSignal>(
                OnRecruitmentQueueChanged);
            _signalBus.TryUnsubscribe<UnitRecruitmentDeployedSignal>(
                OnRecruitmentDeployed);
            _signalBus.TryUnsubscribe<GameModeChangedSignal>(OnGameModeChanged);

            EndSession(destroyPreview: true);
            DestroyRuntimeRoot(_previewRoot);
            if (_controlsRoot != null)
                Object.Destroy(_controlsRoot.gameObject);
            _previewRoot = null;
            _controlsRoot = null;
            _confirmButton = null;
            _cancelButton = null;
            DestroyMaterial(_fallbackPreviewMaterial);
            _fallbackPreviewMaterial = null;
        }

        public void Tick()
        {
            if (_session == null)
                return;

            if (!_turns.CanOwnerAct(_session.OwnerId, out _))
            {
                CancelSession();
                return;
            }

            HandleKeyboard();
            HandleMouse();
            FaceSpritePreviewToCamera();
        }

        public bool IsTurnBlocked(out string reason)
        {
            if (_session == null)
            {
                reason = null;
                return false;
            }

            reason = TurnBlockReason;
            return true;
        }

        private void OnReadyIndicatorClicked(
            UnitRecruitmentReadyIndicatorClickedSignal signal)
        {
            string owner = NormalizeId(signal.OwnerId);
            if (owner == null || signal.QueueId < 1)
            {
                Debug.LogWarning($"{LogTag} Ignored ready indicator with invalid identity.");
                return;
            }

            string localOwner = NormalizeId(_turns.LocalOwnerId);
            if (localOwner != null
                && !string.Equals(owner, localOwner, StringComparison.Ordinal))
            {
                Debug.LogWarning(
                    $"{LogTag} Ignored ready indicator for non-local owner '{owner}'.");
                return;
            }

            if (!_turns.CanOwnerAct(owner, out string reason))
            {
                Debug.LogWarning($"{LogTag} Cannot enter deployment: {reason}");
                return;
            }

            if (_gameModeService != null
                && _gameModeService.CurrentMode != GameModeType.Normal)
            {
                Debug.LogWarning(
                    $"{LogTag} Cannot enter deployment while game mode is {_gameModeService.CurrentMode}.");
                return;
            }

            if (!_recruitment.TryPeekReady(
                    owner,
                    signal.RecruitingBuildingPosition,
                    out UnitRecruitmentQueueItemSnapshot ready)
                || ready.QueueId != signal.QueueId)
            {
                Debug.LogWarning(
                    $"{LogTag} Ready job {signal.QueueId} is no longer the queue head.");
                return;
            }

            if (_session != null)
            {
                if (_session.Matches(owner, signal.QueueId))
                    return;

                CancelSession();
            }

            BeginSession(
                owner,
                ready.QueueId,
                ready.UnitTypeId,
                ready.RecruitingBuildingPosition);
        }

        private void BeginSession(
            string ownerId,
            long queueId,
            string unitTypeId,
            Vector2Int recruitingBuildingPosition)
        {
            var session = new DeploymentSession(
                ownerId,
                queueId,
                unitTypeId,
                recruitingBuildingPosition);

            _session = session;
            _session.InputBlock = _inputPolicy?.AcquireBlock(
                DeploymentInputMask,
                this);
            _session.OverlayAcquired =
                _gridOverlay?.Acquire(GridActionOverlayOwner.Deployment) == true;
            if (!_session.OverlayAcquired)
            {
                Debug.LogWarning(
                    $"{LogTag} Shared grid overlay is unavailable or owned by another mode; deployment remains functional without tile highlights.");
            }

            EnsureWorldRoots();
            EnsureControls();
            RefreshDeploymentTiles();
            SetControlsVisible(true);
            UpdateConfirmInteractable();

            Debug.Log(
                $"{LogTag} Deployment session started queue={queueId} unit={unitTypeId} building={recruitingBuildingPosition}.");
        }

        private void RefreshDeploymentTiles()
        {
            if (_session == null)
                return;

            IReadOnlyList<UnitRecruitmentDeploymentTileSnapshot> tiles =
                _recruitment.GetDeploymentTiles(
                    _session.OwnerId,
                    _session.RecruitingBuildingPosition,
                    _session.QueueId);

            _session.SetTiles(tiles);
            RefreshDeploymentOverlay();

            if (_session.SelectedTile.HasValue
                && !_session.ValidTiles.Contains(_session.SelectedTile.Value))
            {
                ClearSelectedTile();
            }
        }

        private void RefreshDeploymentOverlay()
        {
            if (_session == null)
                return;

            _overlayCells.Clear();
            for (int index = 0; index < _session.Tiles.Count; index++)
            {
                UnitRecruitmentDeploymentTileSnapshot tile = _session.Tiles[index];
                if (!tile.IsValid && _grid != null && !_grid.ContainsCell(tile.Position))
                    continue;

                GridActionOverlayVisualState state =
                    _session.SelectedTile.HasValue
                    && _session.SelectedTile.Value == tile.Position
                        ? GridActionOverlayVisualState.Selected
                        : tile.IsValid
                            ? GridActionOverlayVisualState.Valid
                            : GridActionOverlayVisualState.Invalid;
                _overlayCells.Add(new GridActionOverlayCell(
                    tile.Position,
                    state,
                    tile.Reason));
            }

            if (_session.OverlayAcquired)
                _gridOverlay?.Show(GridActionOverlayOwner.Deployment, _overlayCells);
        }

        private void HandleKeyboard()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                CancelSession();
                return;
            }

            if (keyboard.enterKey.wasPressedThisFrame
                || keyboard.numpadEnterKey.wasPressedThisFrame)
            {
                ConfirmSelectedTile();
            }
        }

        private void HandleMouse()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null || _session == null)
                return;

            if (mouse.rightButton.wasPressedThisFrame)
            {
                CancelSession();
                return;
            }

            if (!mouse.leftButton.wasPressedThisFrame)
                return;

            Vector2 screenPosition = mouse.position.ReadValue();
            if (IsPointerOverUi(screenPosition))
                return;

            if (!TryResolveTile(screenPosition, out Vector2Int tile))
                return;

            SelectTile(tile);
        }

        private void SelectTile(Vector2Int tile)
        {
            if (_session == null)
                return;

            if (!_session.ValidTiles.Contains(tile))
            {
                if (_session.InvalidReasons.TryGetValue(tile, out string reason)
                    && !string.IsNullOrWhiteSpace(reason))
                {
                    Debug.Log($"{LogTag} Invalid deployment tile {tile}: {reason}");
                }

                return;
            }

            _session.SelectedTile = tile;
            MovePreviewTo(tile);
            RefreshDeploymentOverlay();
            UpdateConfirmInteractable();
        }

        private void ConfirmSelectedTile()
        {
            if (_session == null
                || !_session.SelectedTile.HasValue
                || _confirmInProgress)
            {
                return;
            }

            DeploymentSession session = _session;
            Vector2Int target = _session.SelectedTile.Value;
            if (!TryRevalidateTarget(target, out string reason))
            {
                Debug.LogWarning(
                    $"{LogTag} Selected deployment tile became invalid: {reason}");
                RefreshDeploymentTiles();
                return;
            }

            _confirmInProgress = true;
            UpdateConfirmInteractable();
            try
            {
                bool deployed = _recruitment.TryDeployReady(
                    session.OwnerId,
                    session.RecruitingBuildingPosition,
                    session.QueueId,
                    target,
                    out string unitId,
                    out reason);

                if (!deployed)
                {
                    Debug.LogWarning(
                        $"{LogTag} Deployment confirm failed: {reason}");
                    RefreshDeploymentTiles();
                    return;
                }

                Debug.Log(
                    $"{LogTag} Deployment confirmed queue={session.QueueId} unitId={unitId} tile={target}.");
                if (_session != null)
                    EndSession(destroyPreview: true);
            }
            finally
            {
                _confirmInProgress = false;
                UpdateConfirmInteractable();
            }
        }

        private bool TryRevalidateTarget(
            Vector2Int target,
            out string reason)
        {
            reason = null;
            if (_session == null)
                return false;

            IReadOnlyList<UnitRecruitmentDeploymentTileSnapshot> tiles =
                _recruitment.GetDeploymentTiles(
                    _session.OwnerId,
                    _session.RecruitingBuildingPosition,
                    _session.QueueId);

            for (int index = 0; index < tiles.Count; index++)
            {
                UnitRecruitmentDeploymentTileSnapshot tile = tiles[index];
                if (tile.Position != target)
                    continue;

                reason = tile.Reason;
                return tile.IsValid;
            }

            reason = "Selected tile is no longer a deployment candidate.";
            return false;
        }

        private void CancelSession()
        {
            if (_session == null)
                return;

            Debug.Log(
                $"{LogTag} Deployment session cancelled queue={_session.QueueId}.");
            EndSession(destroyPreview: true);
        }

        private void EndSession(bool destroyPreview)
        {
            DeploymentSession session = _session;
            session?.InputBlock?.Dispose();
            if (session?.OverlayAcquired == true)
                _gridOverlay?.Release(GridActionOverlayOwner.Deployment);
            _session = null;
            _confirmInProgress = false;
            ClearSelectedTile(destroyPreview);
            SetControlsVisible(false);
        }

        private void ClearSelectedTile(bool destroyPreview = true)
        {
            if (_session != null)
                _session.SelectedTile = null;

            if (destroyPreview && _previewObject != null)
            {
                Object.Destroy(_previewObject);
                _previewObject = null;
            }

            if (_session != null)
                RefreshDeploymentOverlay();

            UpdateConfirmInteractable();
        }

        private void MovePreviewTo(Vector2Int tile)
        {
            if (_previewObject == null)
                _previewObject = CreatePreviewObject();

            if (_previewObject == null)
                return;

            PositionPreviewObject(_previewObject, tile);
        }

        private GameObject CreatePreviewObject()
        {
            UnitClassConfig config = _unitConfigs.GetConfig(_session.UnitTypeId);
            EnsureWorldRoots();

            GameObject preview = null;
            if (config?.Prefab != null)
            {
                preview = Object.Instantiate(config.Prefab, _previewRoot);
                preview.name = $"UnitDeploymentPreview_{_session.UnitTypeId}";
                PreparePrefabPreview(preview);
            }
            else if (config?.CustomSprite != null)
            {
                preview = CreateSpritePreview(config.CustomSprite);
            }
            else
            {
                preview = CreateFallbackPreview();
            }

            return preview;
        }

        private void PreparePrefabPreview(GameObject preview)
        {
            DisablePreviewGameplayComponents(preview);
            ApplyPreviewTint(preview);
        }

        private GameObject CreateSpritePreview(Sprite sprite)
        {
            var preview = new GameObject(
                $"UnitDeploymentPreview_{_session.UnitTypeId}",
                typeof(SpriteRenderer));
            preview.transform.SetParent(_previewRoot, false);

            SpriteRenderer renderer = preview.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = PreviewTint;
            renderer.sortingOrder = 80;

            float height = Mathf.Max(0.1f, sprite.bounds.size.y);
            float scale = SpritePreviewHeight / height;
            preview.transform.localScale = new Vector3(scale, scale, scale);
            return preview;
        }

        private GameObject CreateFallbackPreview()
        {
            GameObject preview = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            preview.name = $"UnitDeploymentPreview_{_session.UnitTypeId}";
            preview.transform.SetParent(_previewRoot, false);
            preview.transform.localScale = new Vector3(
                0.45f,
                FallbackPreviewHeight * 0.5f,
                0.45f);

            Collider collider = preview.GetComponent<Collider>();
            if (collider != null)
                Object.Destroy(collider);

            EnsureFallbackPreviewMaterial();
            Renderer renderer = preview.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = _fallbackPreviewMaterial;

            return preview;
        }

        private void PositionPreviewObject(GameObject preview, Vector2Int tile)
        {
            if (preview == null)
                return;

            Vector3 tilePosition = ResolveWorldPosition(tile, PreviewLift);
            if (preview.GetComponent<SpriteRenderer>() != null)
            {
                preview.transform.position =
                    tilePosition + Vector3.up * SpritePreviewHeight * 0.5f;
                FaceSpritePreviewToCamera();
                return;
            }

            preview.transform.position = tilePosition;
            if (TryResolveRendererBounds(preview, out Bounds bounds))
            {
                Vector3 root = preview.transform.position;
                preview.transform.position = new Vector3(
                    tilePosition.x - (bounds.center.x - root.x),
                    tilePosition.y - (bounds.min.y - root.y),
                    tilePosition.z - (bounds.center.z - root.z));
            }
        }

        private void DisablePreviewGameplayComponents(GameObject preview)
        {
            Collider[] colliders = preview.GetComponentsInChildren<Collider>(true);
            for (int index = 0; index < colliders.Length; index++)
                colliders[index].enabled = false;

            Collider2D[] colliders2D =
                preview.GetComponentsInChildren<Collider2D>(true);
            for (int index = 0; index < colliders2D.Length; index++)
                colliders2D[index].enabled = false;

            MonoBehaviour[] behaviours =
                preview.GetComponentsInChildren<MonoBehaviour>(true);
            for (int index = 0; index < behaviours.Length; index++)
                behaviours[index].enabled = false;

            Animator[] animators = preview.GetComponentsInChildren<Animator>(true);
            for (int index = 0; index < animators.Length; index++)
                animators[index].enabled = false;
        }

        private void ApplyPreviewTint(GameObject preview)
        {
            SpriteRenderer[] sprites =
                preview.GetComponentsInChildren<SpriteRenderer>(true);
            for (int index = 0; index < sprites.Length; index++)
                sprites[index].color = PreviewTint;

            Renderer[] renderers = preview.GetComponentsInChildren<Renderer>(true);
            for (int index = 0; index < renderers.Length; index++)
            {
                Renderer renderer = renderers[index];
                if (renderer == null || renderer is SpriteRenderer)
                    continue;

                _previewPropertyBlock.Clear();
                renderer.GetPropertyBlock(_previewPropertyBlock);
                _previewPropertyBlock.SetColor("_Color", PreviewTint);
                _previewPropertyBlock.SetColor("_BaseColor", PreviewTint);
                _previewPropertyBlock.SetColor(
                    "_EmissionColor",
                    new Color(0.08f, 0.18f, 0.08f, 1f));
                renderer.SetPropertyBlock(_previewPropertyBlock);
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }
        }

        private void FaceSpritePreviewToCamera()
        {
            if (_previewObject == null
                || _previewObject.GetComponent<SpriteRenderer>() == null)
            {
                return;
            }

            UnityEngine.Camera camera = ResolveCamera();
            if (camera != null)
                _previewObject.transform.rotation = camera.transform.rotation;
        }

        private bool TryResolveTile(
            Vector2 screenPosition,
            out Vector2Int tile)
        {
            tile = default;

            if (_screenToGrid != null)
            {
                tile = _screenToGrid.ScreenToGrid(screenPosition);
                return true;
            }

            if (_pointerGridResolver != null
                && _pointerGridResolver.TryScreenToGrid(screenPosition, out tile))
            {
                return true;
            }

            UnityEngine.Camera camera = ResolveCamera();
            if (camera == null || _gridProjection == null)
                return false;

            Ray ray = camera.ScreenPointToRay(screenPosition);
            Plane plane = _gridProjection.WorldPlane == GridWorldPlane.XZ
                ? new Plane(Vector3.up, ResolveWorldPosition(
                    _session?.RecruitingBuildingPosition ?? Vector2Int.zero,
                    layerOffset: 0f))
                : new Plane(Vector3.forward, Vector3.zero);

            if (!plane.Raycast(ray, out float distance) || distance < 0f)
                return false;

            Vector3 worldPoint = ray.GetPoint(distance);
            if (_pointerGridResolver != null
                && _pointerGridResolver.TryWorldToGrid(worldPoint, out tile))
            {
                return true;
            }

            tile = _gridProjection.WorldToGrid(worldPoint);
            return true;
        }

        private Vector3 ResolveWorldPosition(
            Vector2Int tile,
            float layerOffset)
        {
            if (_worldPositionResolver != null)
                return _worldPositionResolver.ResolveWorldPosition(tile, layerOffset);

            if (_gridProjection == null)
                return new Vector3(tile.x, tile.y, 0f);

            return _gridProjection.GridToWorld(tile, 0f, layerOffset);
        }

        private void EnsureWorldRoots()
        {
            if (_previewRoot == null)
            {
                var root = new GameObject(PreviewRootName);
                _previewRoot = root.transform;
            }
        }

        private void EnsureFallbackPreviewMaterial()
        {
            _fallbackPreviewMaterial ??= CreateTransparentMaterial(
                "UnitDeploymentFallbackPreview",
                PreviewTint);
        }

        private static Material CreateTransparentMaterial(
            string name,
            Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Unlit/Color")
                ?? Shader.Find("Sprites/Default");
            if (shader == null)
                return null;

            var material = new Material(shader)
            {
                name = name,
                color = color,
                renderQueue = OverlayRenderQueue,
            };

            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color"))
                material.SetColor("_Color", color);

            material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.EnableKeyword("_ALPHABLEND_ON");
            return material;
        }

        private void EnsureControls()
        {
            if (_controlsRoot != null)
                return;

            _canvas = ResolveCanvas();
            if (_canvas == null)
            {
                Debug.LogWarning(
                    $"{LogTag} Gameplay Canvas not found. Deployment tile selection still works, but Confirm/Cancel UI is disabled.");
                return;
            }

            var root = new GameObject(
                ControlsRootName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(HorizontalLayoutGroup));
            _controlsRoot = root.GetComponent<RectTransform>();
            _controlsRoot.SetParent(_canvas.transform, false);
            _controlsRoot.anchorMin = new Vector2(0.5f, 0f);
            _controlsRoot.anchorMax = new Vector2(0.5f, 0f);
            _controlsRoot.pivot = new Vector2(0.5f, 0f);
            _controlsRoot.anchoredPosition = new Vector2(0f, 26f);
            _controlsRoot.sizeDelta = new Vector2(300f, 48f);

            Image background = root.GetComponent<Image>();
            background.color = new Color(0.08f, 0.10f, 0.12f, 0.90f);
            background.raycastTarget = true;

            HorizontalLayoutGroup layout = root.GetComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(8, 8, 7, 7);
            layout.spacing = 8f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            _confirmButton = CreateControlButton(
                _controlsRoot,
                "Confirm",
                "Підтвердити",
                ConfirmSelectedTile);
            _cancelButton = CreateControlButton(
                _controlsRoot,
                "Cancel",
                "Скасувати",
                CancelSession);

            _controlsRoot.SetAsLastSibling();
            SetControlsVisible(false);
        }

        private Button CreateControlButton(
            Transform parent,
            string name,
            string label,
            UnityEngine.Events.UnityAction action)
        {
            var buttonObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button),
                typeof(LayoutElement));
            buttonObject.transform.SetParent(parent, false);

            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.18f, 0.24f, 0.22f, 0.96f);
            image.raycastTarget = true;

            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(action);

            LayoutElement layoutElement = buttonObject.GetComponent<LayoutElement>();
            layoutElement.preferredWidth = 138f;
            layoutElement.preferredHeight = 34f;

            var labelObject = new GameObject(
                "Label",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI));
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.SetParent(buttonObject.transform, false);
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            TextMeshProUGUI text = labelObject.GetComponent<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = 16f;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.raycastTarget = false;
            return button;
        }

        private Canvas ResolveCanvas()
        {
            Canvas canvas = _hudView != null
                ? _hudView.GetComponentInParent<Canvas>(true)
                : null;
            if (canvas != null)
                return canvas;

            return Object.FindFirstObjectByType<Canvas>(
                FindObjectsInactive.Include);
        }

        private void SetControlsVisible(bool visible)
        {
            if (_controlsRoot != null)
                _controlsRoot.gameObject.SetActive(visible);
        }

        private void UpdateConfirmInteractable()
        {
            if (_confirmButton != null)
            {
                _confirmButton.interactable =
                    _session != null
                    && _session.SelectedTile.HasValue
                    && !_confirmInProgress;
            }

            if (_cancelButton != null)
                _cancelButton.interactable = _session != null;
        }

        private bool IsPointerOverUi(Vector2 screenPosition)
        {
            if (_inputPolicy != null)
                return _inputPolicy.IsPointerOverUi(screenPosition, interactiveOnly: true);

            return EventSystem.current != null
                && EventSystem.current.IsPointerOverGameObject();
        }

        private UnityEngine.Camera ResolveCamera()
        {
            if (_camera != null && _camera.isActiveAndEnabled)
                return _camera;

            _camera = UnityEngine.Camera.main;
            if (_camera == null && !_warnedMissingCamera)
            {
                _warnedMissingCamera = true;
                Debug.LogWarning(
                    $"{LogTag} Main Camera not found. Deployment pointer fallback and sprite preview facing are disabled.");
            }

            return _camera;
        }

        private void OnTurnStateChanged()
        {
            if (_session == null)
                return;

            if (!_turns.CanOwnerAct(_session.OwnerId, out _))
                CancelSession();
        }

        private void OnRecruitmentQueueChanged(
            UnitRecruitmentQueueChangedSignal signal)
        {
            if (_session == null
                || !string.Equals(
                    NormalizeId(signal.OwnerId),
                    _session.OwnerId,
                    StringComparison.Ordinal)
                || signal.BuildingPosition != _session.RecruitingBuildingPosition)
            {
                return;
            }

            if (!_recruitment.TryPeekReady(
                    _session.OwnerId,
                    _session.RecruitingBuildingPosition,
                    out UnitRecruitmentQueueItemSnapshot ready)
                || ready.QueueId != _session.QueueId)
            {
                EndSession(destroyPreview: true);
                return;
            }

            RefreshDeploymentTiles();
        }

        private void OnRecruitmentDeployed(
            UnitRecruitmentDeployedSignal signal)
        {
            if (_session == null
                || !_session.Matches(signal.OwnerId, signal.QueueId))
            {
                return;
            }

            EndSession(destroyPreview: true);
        }

        private void OnGameModeChanged(GameModeChangedSignal signal)
        {
            if (_session != null && signal.NewMode != GameModeType.Normal)
                CancelSession();
        }

        private static bool TryResolveRendererBounds(
            GameObject root,
            out Bounds bounds)
        {
            bounds = default;
            if (root == null)
                return false;

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            bool found = false;
            for (int index = 0; index < renderers.Length; index++)
            {
                Renderer renderer = renderers[index];
                if (renderer == null || !renderer.enabled)
                    continue;

                if (!found)
                {
                    bounds = renderer.bounds;
                    found = true;
                    continue;
                }

                bounds.Encapsulate(renderer.bounds);
            }

            return found;
        }

        private static string NormalizeId(string value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static void DestroyMaterial(Material material)
        {
            if (material != null)
                Object.Destroy(material);
        }

        private static void DestroyRuntimeRoot(Transform root)
        {
            if (root != null)
                Object.Destroy(root.gameObject);
        }

        private sealed class DeploymentSession
        {
            public readonly string OwnerId;
            public readonly long QueueId;
            public readonly string UnitTypeId;
            public readonly Vector2Int RecruitingBuildingPosition;
            public readonly List<UnitRecruitmentDeploymentTileSnapshot> Tiles = new();
            public readonly HashSet<Vector2Int> ValidTiles = new();
            public readonly Dictionary<Vector2Int, string> InvalidReasons = new();
            public IDisposable InputBlock;
            public Vector2Int? SelectedTile;
            public bool OverlayAcquired;

            public DeploymentSession(
                string ownerId,
                long queueId,
                string unitTypeId,
                Vector2Int recruitingBuildingPosition)
            {
                OwnerId = ownerId;
                QueueId = queueId;
                UnitTypeId = unitTypeId ?? string.Empty;
                RecruitingBuildingPosition = recruitingBuildingPosition;
            }

            public bool Matches(string ownerId, long queueId)
                => QueueId == queueId
                   && string.Equals(
                       OwnerId,
                       NormalizeId(ownerId),
                       StringComparison.Ordinal);

            public void SetTiles(
                IReadOnlyList<UnitRecruitmentDeploymentTileSnapshot> tiles)
            {
                Tiles.Clear();
                ValidTiles.Clear();
                InvalidReasons.Clear();

                if (tiles == null)
                    return;

                for (int index = 0; index < tiles.Count; index++)
                {
                    UnitRecruitmentDeploymentTileSnapshot tile = tiles[index];
                    Tiles.Add(tile);
                    if (tile.IsValid)
                        ValidTiles.Add(tile.Position);
                    else
                        InvalidReasons[tile.Position] = tile.Reason;
                }
            }
        }
    }
}
