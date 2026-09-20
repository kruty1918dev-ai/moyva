var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
    UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
    UnityEditor.SceneManagement.NewSceneMode.Single);

var ground = UnityEngine.GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Cube);
ground.name = "Ground";
ground.transform.position = new UnityEngine.Vector3(1.8f, -0.05f, 0f);
ground.transform.localScale = new UnityEngine.Vector3(8f, 0.1f, 4f);
var groundMat = new UnityEngine.Material(UnityEngine.Shader.Find("Universal Render Pipeline/Lit"));
groundMat.color = new UnityEngine.Color(0.45f, 0.62f, 0.38f);
ground.GetComponent<UnityEngine.Renderer>().sharedMaterial = groundMat;

var lightGo = new UnityEngine.GameObject("Sun");
var light = lightGo.AddComponent<UnityEngine.Light>();
light.type = UnityEngine.LightType.Directional;
light.intensity = 1.3f;
light.shadows = UnityEngine.LightShadows.Soft;
lightGo.transform.rotation = UnityEngine.Quaternion.Euler(50f, 150f, 0f);
UnityEngine.RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
UnityEngine.RenderSettings.ambientSkyColor = new UnityEngine.Color(0.72f, 0.78f, 0.88f);
UnityEngine.RenderSettings.ambientEquatorColor = new UnityEngine.Color(0.55f, 0.55f, 0.55f);
UnityEngine.RenderSettings.ambientGroundColor = new UnityEngine.Color(0.35f, 0.32f, 0.28f);

string dir = "Assets/ThirdParty/KayKit/Packs/KayKit - Medieval Hexagon Pack (for Unity)/Models/units/";

string[][] mixes = new string[][] {
    new[]{"blue/unit_blue_full.fbx","blue/spear_blue_full.fbx","blue/shield_blue_full.fbx","blue/helmet_blue_full.fbx"},
    new[]{"blue/unit_blue_full.fbx","blue/spear_blue_accent.fbx","blue/shield_blue_accent.fbx","blue/helmet_blue_accent.fbx"},
    new[]{"blue/unit_blue_full.fbx","neutral/spear.fbx","neutral/shield.fbx","neutral/helmet.fbx"},
    new[]{"blue/unit_blue_accent.fbx","neutral/spear.fbx","neutral/shield.fbx","neutral/helmet.fbx"},
};

float x = 0f;
foreach (var mix in mixes)
{
    var holder = new UnityEngine.GameObject("mix");
    holder.transform.position = new UnityEngine.Vector3(x, 0f, 0f);
    foreach (var rel in mix)
    {
        var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(dir + rel);
        var inst = (UnityEngine.GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab);
        inst.transform.SetParent(holder.transform, false);
        inst.transform.localPosition = UnityEngine.Vector3.zero;
    }
    x += 1.2f;
}

var camGo = new UnityEngine.GameObject("ShotCam");
var cam = camGo.AddComponent<UnityEngine.Camera>();
cam.clearFlags = UnityEngine.CameraClearFlags.SolidColor;
cam.backgroundColor = new UnityEngine.Color(0.53f, 0.72f, 0.92f);
cam.fieldOfView = 30f;
camGo.transform.position = new UnityEngine.Vector3(1.8f, 0.8f, -2.8f);
camGo.transform.LookAt(new UnityEngine.Vector3(1.8f, 0.3f, 0f));

return "done";
