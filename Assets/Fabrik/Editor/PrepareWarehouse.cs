using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fabrik.Editor
{
    public static class PrepareWarehouse
    {
        [MenuItem("Tools/Fabrik/Add Package Transfer")]
        private static void AddPackageTransfer()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.name == "SampleScene")
            {
                EditorUtility.DisplayDialog("Fabrik", "Open an imported warehouse scene first.", "OK");
                return;
            }

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name == "Fabrik Transfer")
                {
                    Selection.activeGameObject = root;
                    return;
                }
            }

            if (Selection.activeTransform == null)
            {
                EditorUtility.DisplayDialog("Fabrik", "Select a floor object near an open two-meter stretch first.", "OK");
                return;
            }

            GameObject transfer = new GameObject("Fabrik Transfer");
            Undo.RegisterCreatedObjectUndo(transfer, "Add Fabrik transfer");
            transfer.transform.position = Selection.activeTransform.position + Vector3.up * 0.5f;

            GameObject package = GameObject.CreatePrimitive(PrimitiveType.Cube);
            package.name = "Blue Package";
            package.transform.SetParent(transfer.transform, false);
            package.transform.localScale = Vector3.one * 0.5f;
            Undo.DestroyObjectImmediate(package.GetComponent<Collider>());
            package.AddComponent<PackageTransfer>();

            Material material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Fabrik/Fabrik Blue.mat");
            if (material == null)
            {
                Shader shader = Shader.Find("HDRP/Lit") ?? Shader.Find("Standard");
                material = new Material(shader);
                material.color = new Color(0.08f, 0.35f, 0.95f);
                AssetDatabase.CreateAsset(material, "Assets/Fabrik/Fabrik Blue.mat");
            }
            package.GetComponent<Renderer>().sharedMaterial = material;

            EditorSceneManager.MarkSceneDirty(scene);
            Selection.activeGameObject = package;
            SceneView.lastActiveSceneView?.FrameSelected();
            EditorUtility.DisplayDialog("Fabrik", "Press Play to see the three-second transfer. Save a copy of the scene so the imported demo stays unchanged.", "OK");
        }
    }
}