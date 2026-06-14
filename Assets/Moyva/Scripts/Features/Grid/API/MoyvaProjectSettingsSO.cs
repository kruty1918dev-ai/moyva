using UnityEngine;

namespace Kruty1918.Moyva.Grid.API
{
    [CreateAssetMenu(fileName = "MoyvaProjectSettings", menuName = "Moyva/Project Settings", order = 0)]
    public sealed class MoyvaProjectSettingsSO : ScriptableObject
    {
        public const string DefaultAssetPath = "Assets/Moyva/SO/ProjectDefaults/MoyvaProjectSettings.asset";

        [Header("Game Visual Mode")]
        public MoyvaProjectVisualMode ProjectVisualMode = MoyvaProjectVisualMode.Auto;
        public bool UseAdvancedProjectSettings = false;

        [Header("Grid Mode")]
        public GridTopology DefaultGridTopology = GridTopology.Orthogonal;
        public GridProjectionMode DefaultProjectionMode = GridProjectionMode.Orthographic3D;
        public GridRenderMode DefaultRenderMode = GridRenderMode.Mesh3D;
        public GridNeighborhoodMode DefaultNeighborhood = GridNeighborhoodMode.Auto;

        [Header("Cell Size")]
        [Min(0.01f)] public float OrthogonalCellWidth = 1f;
        [Min(0.01f)] public float OrthogonalCellHeight = 1f;
        [Min(0.01f)] public float OrthogonalCellDepth = 1f;

        [Header("Isometric")]
        [Min(0.01f)] public float IsometricTileWidth = 1f;
        [Min(0.01f)] public float IsometricTileHeight = 0.5f;

        [Header("Hex")]
        public HexOrientation HexOrientation = HexOrientation.PointyTop;
        [Min(0.01f)] public float HexRadius = 0.5f;

        [Header("Height Preview")]
        [Min(0f)] public float HeightScale = 0.25f;
        public bool UseHeightForPreview = true;

        [Header("3D Preview")]
        public bool EnableMeshPrefabPreviews = true;
        [Min(16)] public int PreviewTextureSize = 96;
        [Range(1f, 3f)] public float PreviewPadding = 1.35f;
        public bool GeneratePreviewMipmaps = false;
        public FilterMode PreviewFilterMode = FilterMode.Bilinear;
        public RenderTextureFormat PreviewRenderTextureFormat = RenderTextureFormat.ARGB32;
        public RenderTextureReadWrite PreviewRenderTextureReadWrite = RenderTextureReadWrite.Linear;
        public Vector3 Orthographic3DPreviewEuler = new Vector3(90f, 0f, 0f);
        public Vector3 Isometric3DPreviewEuler = new Vector3(52f, 45f, 0f);
        public bool UsePerspectivePreviewCameraIn3D = true;
        [Range(1f, 179f)] public float PreviewPerspectiveFieldOfView = 55f;
        public Vector3 PreviewLightEuler = new Vector3(50f, -30f, 0f);
        [Range(0f, 5f)] public float PreviewLightIntensity = 1.35f;

        [Header("Home Menu 3D Preview")]
        public bool UseLiveHomeMenuMeshPreview = true;
        public bool HomeMenuPreviewIncludeObjects = true;
        public bool HomeMenuPreviewIncludeBuildings = true;
        public bool HomeMenuPreviewCombineMeshesByMaterial = true;
        [Min(1)] public int HomeMenuPreviewTileStride = 1;
        [Min(1)] public int HomeMenuPreviewMaxTerrainTiles = 30000;
        [Min(1024)] public int HomeMenuPreviewMaxVerticesPerBatch = 60000;
        [Min(1)] public int HomeMenuPreviewMaxMaterialBatches = 128;
        [Range(0.75f, 3f)] public float HomeMenuPreviewCameraPadding = 1.2f;
        [Range(-1000f, 1000f)] public float HomeMenuPreviewCameraDepth = 100f;
        [Range(0, 31)] public int HomeMenuPreviewLayer = 30;
        public bool HomeMenuPreviewDisableFog = true;
        public bool HomeMenuPreviewUploadMeshData = true;
        public bool HomeMenuPreviewCastShadows = false;
        public bool HomeMenuPreviewReceiveShadows = false;
        public Color HomeMenuPreviewBackgroundColor = new Color(0.05f, 0.07f, 0.08f, 1f);

        [Header("3D Lighting")]
        public bool AutoConfigure3DLighting = true;
        public bool CreateDirectionalLightIn3D = true;
        public Vector3 Project3DLightEuler = new Vector3(50f, -35f, 0f);
        public Color Project3DLightColor = new Color(1f, 0.96f, 0.88f, 1f);
        [Range(0f, 8f)] public float Project3DLightIntensity = 1.35f;
        public bool Project3DLightShadows = true;
        public Color Project3DAmbientSkyColor = new Color(0.46f, 0.54f, 0.64f, 1f);
        public Color Project3DAmbientEquatorColor = new Color(0.42f, 0.38f, 0.32f, 1f);
        public Color Project3DAmbientGroundColor = new Color(0.18f, 0.16f, 0.14f, 1f);
        public bool Enable3DAtmosphericFog = true;
        public Color Project3DAtmosphericFogColor = new Color(0.62f, 0.68f, 0.72f, 1f);
        [Range(0f, 0.1f)] public float Project3DAtmosphericFogDensity = 0.012f;

        [Header("Camera Defaults")]
        public MoyvaCameraProjectPolicy CameraPolicy = MoyvaCameraProjectPolicy.AutoFromGrid;
        public MoyvaCameraAnglePolicy CameraAnglePolicy = MoyvaCameraAnglePolicy.Auto;
        public Vector3 Custom3DCameraEuler = new Vector3(60f, 45f, 0f);
        [Min(0.1f)] public float Project3DCameraDistance = 35f;
        [Min(0.1f)] public float Project3DOrthographicSize = 20f;
        [Range(1f, 179f)] public float Project3DFieldOfView = 40f;

        [Header("Startup Camera")]
        public bool EnsureStartupCameraShowsRevealedArea = true;
        [Min(0f)] public float StartupCameraPaddingTiles = 2f;
        public MoyvaStartupCameraRadiusSource StartupCameraRadiusSource = MoyvaStartupCameraRadiusSource.RevealedFogRadius;
        [Min(1)] public int ManualStartupCameraRadius = 15;

        public Vector2 OrthogonalCellSize => new Vector2(OrthogonalCellWidth, OrthogonalCellHeight);
        public Vector3 Orthogonal3DCellSize => new Vector3(OrthogonalCellWidth, HeightScale, OrthogonalCellDepth);
        public Vector2 IsometricTileSize => new Vector2(IsometricTileWidth, IsometricTileHeight);

        public static MoyvaProjectSettingsSO CreateRuntimeDefault()
        {
            var settings = CreateInstance<MoyvaProjectSettingsSO>();
            settings.name = "Runtime Moyva Project Settings";
            settings.Normalize();
            return settings;
        }

        public void Normalize()
        {
            // Full3D-only project: always force Full3D defaults regardless of asset values.
            ProjectVisualMode = MoyvaProjectVisualMode.Full3D;
            UseAdvancedProjectSettings = false;
            ApplyVisualModeDefaults(MoyvaProjectVisualMode.Full3D);
            NormalizeScalarValues();
        }

        public MoyvaProjectVisualMode ResolveProjectVisualMode()
        {
            // Project is locked to Full3D; legacy Classic2D path is retired.
            return MoyvaProjectVisualMode.Full3D;
        }

        public void UseFull3DDefaults()
        {
            ProjectVisualMode = MoyvaProjectVisualMode.Full3D;
            UseAdvancedProjectSettings = false;
            ApplyVisualModeDefaults(MoyvaProjectVisualMode.Full3D);
            NormalizeScalarValues();
        }

        public void UseClassic2DDefaults()
        {
            // Classic2D path is removed; fall back to Full3D defaults.
            ProjectVisualMode = MoyvaProjectVisualMode.Full3D;
            UseAdvancedProjectSettings = false;
            ApplyVisualModeDefaults(MoyvaProjectVisualMode.Full3D);
            NormalizeScalarValues();
        }

        private void ApplyVisualModeDefaults(MoyvaProjectVisualMode visualMode)
        {
            if (visualMode == MoyvaProjectVisualMode.Full3D)
            {
                DefaultGridTopology = GridTopology.Layered;
                DefaultProjectionMode = GridProjectionMode.Isometric3DPreview;
                DefaultRenderMode = GridRenderMode.Mesh3D;
                DefaultNeighborhood = GridNeighborhoodMode.Auto;
                EnableMeshPrefabPreviews = true;
                GeneratePreviewMipmaps = false;
                PreviewFilterMode = FilterMode.Bilinear;
                UsePerspectivePreviewCameraIn3D = true;
                AutoConfigure3DLighting = true;
                CreateDirectionalLightIn3D = true;
                Project3DLightShadows = true;
                Enable3DAtmosphericFog = true;
                CameraPolicy = MoyvaCameraProjectPolicy.Force3DPerspective;
                CameraAnglePolicy = MoyvaCameraAnglePolicy.Isometric;
                Custom3DCameraEuler = Isometric3DPreviewEuler;
                EnsureStartupCameraShowsRevealedArea = true;
                StartupCameraRadiusSource = MoyvaStartupCameraRadiusSource.RevealedFogRadius;
                return;
            }

            DefaultGridTopology = GridTopology.Orthogonal;
            DefaultProjectionMode = GridProjectionMode.Orthographic3D;
            DefaultRenderMode = GridRenderMode.Mesh3D;
            DefaultNeighborhood = GridNeighborhoodMode.Auto;
            CameraPolicy = MoyvaCameraProjectPolicy.AutoFromGrid;
            CameraAnglePolicy = MoyvaCameraAnglePolicy.Auto;
            EnsureStartupCameraShowsRevealedArea = true;
        }

        private void NormalizeScalarValues()
        {
            OrthogonalCellWidth = Mathf.Max(0.01f, OrthogonalCellWidth);
            OrthogonalCellHeight = Mathf.Max(0.01f, OrthogonalCellHeight);
            OrthogonalCellDepth = Mathf.Max(0.01f, OrthogonalCellDepth);
            IsometricTileWidth = Mathf.Max(0.01f, IsometricTileWidth);
            IsometricTileHeight = Mathf.Max(0.01f, IsometricTileHeight);
            HexRadius = Mathf.Max(0.01f, HexRadius);
            HeightScale = Mathf.Max(0f, HeightScale);
            PreviewTextureSize = Mathf.Clamp(PreviewTextureSize, 16, 512);
            PreviewPadding = Mathf.Clamp(PreviewPadding, 1f, 3f);
            PreviewPerspectiveFieldOfView = Mathf.Clamp(PreviewPerspectiveFieldOfView, 1f, 179f);
            PreviewLightIntensity = Mathf.Clamp(PreviewLightIntensity, 0f, 5f);
            HomeMenuPreviewTileStride = Mathf.Max(1, HomeMenuPreviewTileStride);
            HomeMenuPreviewMaxTerrainTiles = Mathf.Max(1, HomeMenuPreviewMaxTerrainTiles);
            HomeMenuPreviewMaxVerticesPerBatch = Mathf.Max(1024, HomeMenuPreviewMaxVerticesPerBatch);
            HomeMenuPreviewMaxMaterialBatches = Mathf.Max(1, HomeMenuPreviewMaxMaterialBatches);
            HomeMenuPreviewCameraPadding = Mathf.Clamp(HomeMenuPreviewCameraPadding, 0.75f, 3f);
            HomeMenuPreviewCameraDepth = Mathf.Clamp(HomeMenuPreviewCameraDepth, -1000f, 1000f);
            HomeMenuPreviewLayer = Mathf.Clamp(HomeMenuPreviewLayer, 0, 31);
            Project3DLightIntensity = Mathf.Clamp(Project3DLightIntensity, 0f, 8f);
            Project3DAtmosphericFogDensity = Mathf.Clamp(Project3DAtmosphericFogDensity, 0f, 0.1f);
            Project3DCameraDistance = Mathf.Max(0.1f, Project3DCameraDistance);
            Project3DOrthographicSize = Mathf.Max(0.1f, Project3DOrthographicSize);
            Project3DFieldOfView = Mathf.Clamp(Project3DFieldOfView, 1f, 179f);
            StartupCameraPaddingTiles = Mathf.Max(0f, StartupCameraPaddingTiles);
            ManualStartupCameraRadius = Mathf.Max(1, ManualStartupCameraRadius);
        }

        public bool Uses3DProjectMode()
        {
            return ResolveProjectVisualMode() == MoyvaProjectVisualMode.Full3D
                || Uses3DProjectModeRaw();
        }

        private bool Uses3DProjectModeRaw()
        {
            return DefaultProjectionMode == GridProjectionMode.Orthographic3D
                || DefaultProjectionMode == GridProjectionMode.Isometric3DPreview
                || DefaultRenderMode == GridRenderMode.Mesh3D
                || DefaultRenderMode == GridRenderMode.Mesh3DPreview;
        }

        public bool ResolveUse3DCamera(bool autoValue)
        {
            return CameraPolicy switch
            {
                MoyvaCameraProjectPolicy.Force3DOrthographic => true,
                MoyvaCameraProjectPolicy.Force3DPerspective => true,
                _ => autoValue,
            };
        }

        public bool ResolveUsePerspectiveCamera(bool autoOrthographicValue)
        {
            if (!UseAdvancedProjectSettings && ResolveProjectVisualMode() == MoyvaProjectVisualMode.Full3D)
                return true;

            return CameraPolicy switch
            {
                MoyvaCameraProjectPolicy.Force3DOrthographic => false,
                MoyvaCameraProjectPolicy.Force3DPerspective => true,
                _ => !autoOrthographicValue,
            };
        }

        public Vector3 Resolve3DCameraEuler(GridProjectionMode projectionMode)
        {
            return CameraAnglePolicy switch
            {
                MoyvaCameraAnglePolicy.OrthographicTopDown => Orthographic3DPreviewEuler,
                MoyvaCameraAnglePolicy.Isometric => Isometric3DPreviewEuler,
                MoyvaCameraAnglePolicy.Custom => Custom3DCameraEuler,
                _ => projectionMode == GridProjectionMode.Orthographic3D
                    ? Orthographic3DPreviewEuler
                    : Isometric3DPreviewEuler,
            };
        }

        public Vector3 ResolvePreviewCameraEuler()
        {
            return DefaultProjectionMode == GridProjectionMode.Orthographic3D
                ? Orthographic3DPreviewEuler
                : Isometric3DPreviewEuler;
        }

        public int ResolvePreviewTextureSize() => Mathf.Clamp(PreviewTextureSize, 16, 512);
        public float ResolvePreviewPadding() => Mathf.Clamp(PreviewPadding, 1f, 3f);
        public bool ResolveUsePerspectivePreviewCamera() => Uses3DProjectMode() && UsePerspectivePreviewCameraIn3D;
        public float ResolvePreviewPerspectiveFieldOfView() => Mathf.Clamp(PreviewPerspectiveFieldOfView, 1f, 179f);
        public float ResolvePreviewLightIntensity() => Mathf.Clamp(PreviewLightIntensity, 0f, 5f);
        public float ResolveProject3DCameraDistance() => Mathf.Max(0.1f, Project3DCameraDistance);
        public float ResolveProject3DOrthographicSize() => Mathf.Max(0.1f, Project3DOrthographicSize);
        public float ResolveProject3DFieldOfView() => Mathf.Clamp(Project3DFieldOfView, 1f, 179f);
        public float ResolveStartupCameraPaddingTiles() => Mathf.Max(0f, StartupCameraPaddingTiles);
        public int ResolveManualStartupCameraRadius() => Mathf.Max(1, ManualStartupCameraRadius);

        public GridNeighborhoodMode ResolveNeighborhoodMode()
        {
            if (DefaultNeighborhood != GridNeighborhoodMode.Auto)
                return DefaultNeighborhood;

            if (DefaultGridTopology == GridTopology.HexAxial)
                return GridNeighborhoodMode.HexAxial6;

            return DefaultProjectionMode == GridProjectionMode.Isometric3DPreview
                ? GridNeighborhoodMode.VonNeumann4
                : GridNeighborhoodMode.Moore8;
        }
    }
}