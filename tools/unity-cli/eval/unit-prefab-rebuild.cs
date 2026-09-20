// Rebuild the four KayKit unit prefabs with class gear composed at tuned offsets.
// Each part: assetPath | localPos | localEuler | localScale
string dir = "Assets/ThirdParty/KayKit/Packs/KayKit - Medieval Hexagon Pack (for Unity)/Models/units/";

var inv = System.Globalization.CultureInfo.InvariantCulture;
System.Func<string,UnityEngine.Vector3> v3 = s => {
    var p = s.Split(',');
    return new UnityEngine.Vector3(float.Parse(p[0], inv), float.Parse(p[1], inv), float.Parse(p[2], inv));
};

// prefab path -> part lines
var specs = new System.Collections.Generic.Dictionary<string, string[]>
{
    ["Assets/Moyva/Prefabs/Units/KayKit/warrior.prefab"] = new[] {
        "blue/unit_blue_full.fbx|0,0,0|0,0,0|1",
        "neutral/helmet.fbx|0,0.26,0|0,0,0|1",
        "neutral/sword.fbx|0.17,0.20,0.03|0,0,-30|1.4",
        "neutral/shield.fbx|-0.15,0.15,0.02|0,100,0|0.9",
    },
    ["Assets/Moyva/Prefabs/Units/KayKit/spearman.prefab"] = new[] {
        "blue/unit_blue_full.fbx|0,0,0|0,0,0|1",
        "neutral/helmet.fbx|0,0.26,0|0,0,0|1",
        "neutral/spear.fbx|0.13,0.19,0|0,0,0|1",
        "neutral/shield.fbx|-0.15,0.15,0.02|0,100,0|0.9",
    },
    ["Assets/Moyva/Prefabs/Units/KayKit/archer.prefab"] = new[] {
        "blue/unit_blue_full.fbx|0,0,0|0,0,0|1",
        "neutral/helmet.fbx|0,0.27,0|0,0,0|0.85",
        "neutral/bow.fbx|-0.15,0.16,0.03|0,90,-12|0.85",
        "neutral/projectile_arrow.fbx|0.09,0.20,-0.09|45,0,30|1.4",
    },
    ["Assets/Moyva/Prefabs/Units/KayKit/light-cavalry.prefab"] = new[] {
        "blue/horse_blue_full.fbx|0,0,0|0,0,0|1",
        "blue/unit_blue_full.fbx|0,0.42,0|0,0,0|1",
        "neutral/helmet.fbx|0,0.68,0|0,0,0|1",
        "neutral/spear.fbx|0.13,0.52,0|0,0,0|0.9",
        "neutral/shield.fbx|-0.15,0.57,0.02|0,100,0|0.85",
    },
};

var sb = new System.Text.StringBuilder();
foreach (var kv in specs)
{
    string path = kv.Key;
    var root = UnityEditor.PrefabUtility.LoadPrefabContents(path);
    try
    {
        var visual = root.transform.Find("Visual");
        if (visual == null)
        {
            var vgo = new UnityEngine.GameObject("Visual");
            vgo.transform.SetParent(root.transform, false);
            visual = vgo.transform;
        }
        // wipe existing children
        for (int i = visual.childCount - 1; i >= 0; i--)
            UnityEngine.Object.DestroyImmediate(visual.GetChild(i).gameObject);

        foreach (var line in kv.Value)
        {
            var f = line.Split('|');
            var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(dir + f[0]);
            if (asset == null) { sb.AppendLine("MISSING " + f[0]); continue; }
            var inst = (UnityEngine.GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(asset, root.scene);
            inst.transform.SetParent(visual, false);
            inst.transform.localPosition = v3(f[1]);
            var e = v3(f[2]);
            inst.transform.localRotation = UnityEngine.Quaternion.Euler(e.x, e.y, e.z);
            float s = float.Parse(f[3], inv);
            inst.transform.localScale = new UnityEngine.Vector3(s, s, s);
            inst.name = System.IO.Path.GetFileNameWithoutExtension(f[0]);
        }

        // fit root BoxCollider to rendered bounds (local space)
        var box = root.GetComponent<UnityEngine.BoxCollider>();
        if (box != null)
        {
            var rends = root.GetComponentsInChildren<UnityEngine.Renderer>();
            if (rends.Length > 0)
            {
                var b = rends[0].bounds;
                for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
                var c = root.transform.InverseTransformPoint(b.center);
                box.center = c;
                box.size = new UnityEngine.Vector3(b.size.x, b.size.y, b.size.z);
            }
        }

        UnityEditor.PrefabUtility.SaveAsPrefabAsset(root, path);
        sb.AppendLine("saved " + path);
    }
    finally
    {
        UnityEditor.PrefabUtility.UnloadPrefabContents(root);
    }
}
return sb.ToString();
