// Report renderer bounds of each piece relative to its pivot.
string dir = "Assets/ThirdParty/KayKit/Packs/KayKit - Medieval Hexagon Pack (for Unity)/Models/units/neutral/";
string[] files = { "unit.fbx", "spear.fbx", "sword.fbx", "bow.fbx", "shield.fbx", "helmet.fbx", "banner.fbx", "hammer.fbx", "shovel.fbx", "horse_A.fbx", "horse_saddle.fbx", "projectile_arrow.fbx" };
var sb = new System.Text.StringBuilder();
foreach (var rel in files)
{
    var go = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(dir + rel);
    if (go == null) { sb.AppendLine("MISSING " + rel); continue; }
    var rends = go.GetComponentsInChildren<UnityEngine.Renderer>();
    var b = new UnityEngine.Bounds();
    bool first = true;
    foreach (var r in rends) { if (first) { b = r.bounds; first = false; } else b.Encapsulate(r.bounds); }
    sb.AppendLine(rel + "  center=" + b.center.ToString("F3") + " size=" + b.size.ToString("F3"));
}
return sb.ToString();
