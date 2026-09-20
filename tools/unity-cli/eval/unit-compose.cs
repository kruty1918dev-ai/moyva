// Build figures from a spec file: Temp/ai/unit-spec.json
// Spec: { "figures": [ { "name": "...", "x": 0.0, "parts": [ {"asset":"blue/unit_blue_full.fbx","pos":[x,y,z],"rot":[x,y,z],"scale":1.0} ] } ] }
string specPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Temp/ai/unit-spec.json");
string specText = System.IO.File.ReadAllText(specPath);

// tiny JSON parse via Unity's JsonUtility needs a type; use regex-free approach: load via a defined serializable class is unavailable here,
// so spec uses a simple line format instead. We'll parse manually below.

var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
    UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,
    UnityEditor.SceneManagement.NewSceneMode.Single);

var ground = UnityEngine.GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Cube);
ground.name = "Ground";
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

string dir = "Assets/ThirdParty/KayKit/Packs/KayKit - Medieval Hexagon Pack (for Unity)/Models/units/";
var sb = new System.Text.StringBuilder();

// Line format: FIGURE|name|x|z|rotY        PART|asset|x|y|z|rx|ry|rz|scale   CAM|x|y|z|lx|ly|lz|fov   GROUND|x|z|w|d
UnityEngine.GameObject cur = null;
foreach (var rawLine in specText.Split('\n'))
{
    var line = rawLine.Trim();
    if (line.Length == 0 || line.StartsWith("#")) continue;
    var f = line.Split('|');
    if (f[0] == "FIGURE")
    {
        cur = new UnityEngine.GameObject(f[1]);
        cur.transform.position = new UnityEngine.Vector3(float.Parse(f[2], System.Globalization.CultureInfo.InvariantCulture), 0f, float.Parse(f[3], System.Globalization.CultureInfo.InvariantCulture));
        cur.transform.rotation = UnityEngine.Quaternion.Euler(0f, float.Parse(f[4], System.Globalization.CultureInfo.InvariantCulture), 0f);
    }
    else if (f[0] == "PART" && cur != null)
    {
        var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(dir + f[1]);
        if (prefab == null) { sb.AppendLine("MISSING " + f[1]); continue; }
        var inst = (UnityEngine.GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab);
        inst.transform.SetParent(cur.transform, false);
        inst.transform.localPosition = new UnityEngine.Vector3(float.Parse(f[2], System.Globalization.CultureInfo.InvariantCulture), float.Parse(f[3], System.Globalization.CultureInfo.InvariantCulture), float.Parse(f[4], System.Globalization.CultureInfo.InvariantCulture));
        inst.transform.localRotation = UnityEngine.Quaternion.Euler(float.Parse(f[5], System.Globalization.CultureInfo.InvariantCulture), float.Parse(f[6], System.Globalization.CultureInfo.InvariantCulture), float.Parse(f[7], System.Globalization.CultureInfo.InvariantCulture));
        float s = f.Length > 8 ? float.Parse(f[8], System.Globalization.CultureInfo.InvariantCulture) : 1f;
        inst.transform.localScale = new UnityEngine.Vector3(s, s, s);
    }
    else if (f[0] == "GROUND")
    {
        ground.transform.position = new UnityEngine.Vector3(float.Parse(f[1], System.Globalization.CultureInfo.InvariantCulture), -0.05f, float.Parse(f[2], System.Globalization.CultureInfo.InvariantCulture));
        ground.transform.localScale = new UnityEngine.Vector3(float.Parse(f[3], System.Globalization.CultureInfo.InvariantCulture), 0.1f, float.Parse(f[4], System.Globalization.CultureInfo.InvariantCulture));
    }
    else if (f[0] == "CAM")
    {
        var camGo = new UnityEngine.GameObject("ShotCam");
        var cam = camGo.AddComponent<UnityEngine.Camera>();
        cam.clearFlags = UnityEngine.CameraClearFlags.SolidColor;
        cam.backgroundColor = new UnityEngine.Color(0.53f, 0.72f, 0.92f);
        cam.fieldOfView = float.Parse(f[7], System.Globalization.CultureInfo.InvariantCulture);
        camGo.transform.position = new UnityEngine.Vector3(float.Parse(f[1], System.Globalization.CultureInfo.InvariantCulture), float.Parse(f[2], System.Globalization.CultureInfo.InvariantCulture), float.Parse(f[3], System.Globalization.CultureInfo.InvariantCulture));
        camGo.transform.LookAt(new UnityEngine.Vector3(float.Parse(f[4], System.Globalization.CultureInfo.InvariantCulture), float.Parse(f[5], System.Globalization.CultureInfo.InvariantCulture), float.Parse(f[6], System.Globalization.CultureInfo.InvariantCulture)));
    }
}
return sb.ToString() == "" ? "done" : sb.ToString();
