// Open the gameplay scene and enter play mode (direct gameplay launch
// auto-generates a world when GameLaunchContext.Mode == Unknown).
var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(
    "Assets/Moyva/Scenes/Gamplay_Scene.unity",
    UnityEditor.SceneManagement.OpenSceneMode.Single);
UnityEditor.EditorApplication.isPlaying = true;
return "opened:" + scene.path;
