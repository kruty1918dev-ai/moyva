var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
    UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
    UnityEditor.SceneManagement.NewSceneMode.Single);

var ground = UnityEngine.GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Cube);
ground.name = "Ground";
ground.transform.position = new UnityEngine.Vector3(2.4f, -0.05f, 0f);
ground.transform.localScale = new UnityEngine.Vector3(10f, 0.1f, 4f);
var groundMat = new UnityEngine.Material(UnityEngine.Shader.Find("Universal Render Pipeline/Lit"));
groundMat.color = new UnityEngine.Color(0.45f, 0.62f, 0.38f);
ground.GetComponent<UnityEngine.Renderer>().sharedMaterial = groundMat;

var lightGo = new UnityEngine.GameObject("Sun");
var light = lightGo.AddComponent<UnityEngine.Light>();
light.type = UnityEngine.LightType.Directional;
light.intensity = 1.3f;
light.shadows = UnityEngine.LightShadows.Soft;
lightGo.transform.rotation = UnityEngine.Quaternion.Euler(50f, -30f, 0f);
UnityEngine.RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
UnityEngine.RenderSettings.ambientSkyColor = new UnityEngine.Color(0.72f, 0.78f, 0.88f);
UnityEngine.RenderSettings.ambientEquatorColor = new UnityEngine.Color(0.55f, 0.55f, 0.55f);
UnityEngine.RenderSettings.ambientGroundColor = new UnityEngine.Color(0.35f, 0.32f, 0.28f);

string[] prefabPaths = {
    "Assets/Moyva/Prefabs/Units/KayKit/light-cavalry.prefab",
    "Assets/Moyva/Prefabs/Units/KayKit/archer.prefab",
    "Assets/Moyva/Prefabs/Units/KayKit/spearman.prefab",
    "Assets/Moyva/Prefabs/Units/KayKit/warrior.prefab",
};

float x = 0f;
foreach (var p in prefabPaths)
{
    var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(p);
    var inst = (UnityEngine.GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab);
    inst.transform.position = new UnityEngine.Vector3(x, 0f, 0f);
    inst.transform.rotation = UnityEngine.Quaternion.Euler(0f, 165f, 0f);
    x += 1.5f;
}

var camGo = new UnityEngine.GameObject("ShotCam");
var cam = camGo.AddComponent<UnityEngine.Camera>();
cam.clearFlags = UnityEngine.CameraClearFlags.SolidColor;
cam.backgroundColor = new UnityEngine.Color(0.53f, 0.72f, 0.92f);
cam.fieldOfView = 33f;
camGo.transform.position = new UnityEngine.Vector3(2.2f, 2.1f, 3.4f);
camGo.transform.LookAt(new UnityEngine.Vector3(2.2f, 0.15f, 0f));

return "done";
