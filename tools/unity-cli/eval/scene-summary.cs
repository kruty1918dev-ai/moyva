using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.Linq;

var scene = SceneManager.GetActiveScene();
var roots = scene.GetRootGameObjects();
return $"scene={scene.name}; path={scene.path}; roots={roots.Length}; dirty={scene.isDirty}";
