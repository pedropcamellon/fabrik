using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Fabrik.Editor
{
    public static class PrepareWarehouse
    {
        private const string SampleScenePath = "Assets/WarehousePack/Samples/URP/URPScene.unity";
        private const string FabrikScenePath = "Assets/Scenes/Fabrik.unity";

        [MenuItem("Tools/Fabrik/Create Scene from Warehouse Sample")]
        public static void CreateSceneFromSample()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(SampleScenePath) == null)
            {
                EditorUtility.DisplayDialog("Fabrik", "Download and import Warehouse Pack from Package Manager > My Assets first.", "OK");
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(FabrikScenePath) == null &&
                !AssetDatabase.CopyAsset(SampleScenePath, FabrikScenePath))
            {
                EditorUtility.DisplayDialog("Fabrik", "Unity could not copy the warehouse sample scene.", "OK");
                return;
            }

            EditorSceneManager.OpenScene(FabrikScenePath, OpenSceneMode.Single);
            AddWarehouseWalker();
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(FabrikScenePath, true) };
        }

        [MenuItem("Tools/Fabrik/Add Warehouse Walker")]
        public static void AddWarehouseWalker()
        {
            GameObject existing = GameObject.Find("Warehouse Walker");
            if (existing != null)
            {
                Selection.activeGameObject = existing;
                return;
            }

            GameObject floor = GameObject.Find("Rooms/Storage aisle/Floor");
            if (floor == null || !floor.TryGetComponent(out Renderer floorRenderer))
            {
                EditorUtility.DisplayDialog("Fabrik", "Open Assets/Scenes/Fabrik.unity first.", "OK");
                return;
            }

            GameObject avatar = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            avatar.name = "Warehouse Walker";
            avatar.transform.position = new Vector3(
                floorRenderer.bounds.center.x,
                floorRenderer.bounds.max.y + 1f,
                floorRenderer.bounds.center.z);
            Undo.RegisterCreatedObjectUndo(avatar, "Add warehouse walker");
            Undo.DestroyObjectImmediate(avatar.GetComponent<CapsuleCollider>());
            CharacterController controller = avatar.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.4f;
            avatar.AddComponent<WarehouseWalker>();

            Material material = AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/WarehousePack/Materials/URP/Mat_Safety_Orange.mat");
            avatar.GetComponent<Renderer>().sharedMaterial = material;

            GameObject cameraObject = GameObject.Find("Hero Camera");
            if (cameraObject != null)
            {
                foreach (Animator animator in cameraObject.GetComponents<Animator>())
                {
                    animator.enabled = false;
                }

                cameraObject.tag = "MainCamera";
                WarehouseCamera follow = cameraObject.GetComponent<WarehouseCamera>();
                if (follow == null)
                {
                    follow = cameraObject.AddComponent<WarehouseCamera>();
                }
                follow.SetTarget(avatar.transform);
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Selection.activeGameObject = avatar;
            SceneView.lastActiveSceneView?.FrameSelected();
        }

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
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
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