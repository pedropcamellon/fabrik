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
            AddScenePhysics();
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(FabrikScenePath, true) };
        }

        [MenuItem("Tools/Fabrik/Add Warehouse Walker")]
        public static void AddWarehouseWalker()
        {
            DestroyIfPresent("Warehouse Ragdoll");
            GameObject floor = GameObject.Find("Rooms/Storage aisle/Floor");
            if (floor == null || !floor.TryGetComponent(out Renderer floorRenderer))
            {
                EditorUtility.DisplayDialog("Fabrik", "Open Assets/Scenes/Fabrik.unity first.", "OK");
                return;
            }

            GameObject avatar = GameObject.Find("Warehouse Walker");
            if (avatar == null)
            {
                avatar = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                avatar.name = "Warehouse Walker";
                avatar.transform.position = new Vector3(
                    floorRenderer.bounds.center.x,
                    floorRenderer.bounds.max.y + 1f,
                    floorRenderer.bounds.center.z);
                Material material = AssetDatabase.LoadAssetAtPath<Material>(
                    "Assets/WarehousePack/Materials/URP/Mat_Safety_Orange.mat");
                Undo.RegisterCreatedObjectUndo(avatar, "Add warehouse walker");
                Undo.DestroyObjectImmediate(avatar.GetComponent<CapsuleCollider>());
                CharacterController controller = avatar.AddComponent<CharacterController>();
                controller.height = 2f;
                controller.radius = 0.4f;
                avatar.AddComponent<WarehouseWalker>();
                avatar.GetComponent<Renderer>().sharedMaterial = material;
            }

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
                Vector3 initialForward = Vector3.ProjectOnPlane(
                    avatar.transform.position - cameraObject.transform.position,
                    Vector3.up).normalized;
                if (initialForward.sqrMagnitude > 0f)
                {
                    avatar.transform.rotation = Quaternion.LookRotation(initialForward, Vector3.up);
                }
                follow.SetTarget(avatar.transform);
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Selection.activeGameObject = avatar;
            SceneView.lastActiveSceneView?.FrameSelected();
        }

        private static void AddScenePhysics()
        {
            AddDynamicFixture(GameObject.Find("Fixtures/Storage aisle fixtures/PalletRack_Run"), 450f);
            GameObject shelving = GameObject.Find("Fixtures/Storage aisle fixtures/BoltlessShelving");
            if (shelving != null)
            {
                foreach (Transform unit in shelving.transform)
                {
                    AddDynamicFixture(unit.gameObject, 90f);
                }
            }

            RemoveNestedPhysicsNamed("AssembledCardboardBox");
            RemoveNestedPhysicsNamed("ShippedParcel");
            RemoveNestedPhysicsNamed("EuroPallet");
            AddPushableBoxes();

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
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
            Rigidbody body = package.AddComponent<Rigidbody>();
            body.isKinematic = true;
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

        private static void AddDynamicFixture(GameObject root, float mass)
        {
            if (root == null)
            {
                return;
            }

            foreach (MeshFilter meshFilter in root.GetComponentsInChildren<MeshFilter>())
            {
                if (meshFilter.GetComponent<Collider>() != null || meshFilter.sharedMesh == null)
                {
                    continue;
                }

                BoxCollider collider = meshFilter.gameObject.AddComponent<BoxCollider>();
                collider.center = meshFilter.sharedMesh.bounds.center;
                collider.size = meshFilter.sharedMesh.bounds.size;
            }

            Rigidbody body = root.GetComponent<Rigidbody>();
            if (body == null)
            {
                body = root.AddComponent<Rigidbody>();
            }
            body.mass = mass;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        }

        private static void AddPushableBoxes()
        {
            DestroyIfPresent("Pushable Boxes");
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/WarehousePack/Prefabs/URP/AssembledCardboardBox.prefab");
            GameObject floor = GameObject.Find("Rooms/Storage aisle/Floor");
            if (prefab == null || floor == null || !floor.TryGetComponent(out Renderer floorRenderer))
            {
                return;
            }

            GameObject group = new GameObject("Pushable Boxes");
            Vector3 center = floorRenderer.bounds.center;
            for (int index = 0; index < 3; index++)
            {
                GameObject box = (GameObject)PrefabUtility.InstantiatePrefab(prefab, SceneManager.GetActiveScene());
                box.name = $"Pushable Box {index + 1}";
                box.transform.SetParent(group.transform);
                box.transform.position = new Vector3(center.x + (index - 1) * 0.65f, floorRenderer.bounds.max.y, center.z + 1.8f);
                AddSingleBodyPhysics(box, 2f);
            }
        }

        private static void AddSingleBodyPhysics(GameObject root, float mass)
        {
            Bounds bounds = default;
            bool hasBounds = false;
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>())
            {
                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            if (!hasBounds)
            {
                return;
            }

            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.center = root.transform.InverseTransformPoint(bounds.center);
            Vector3 scale = root.transform.lossyScale;
            collider.size = new Vector3(
                bounds.size.x / Mathf.Abs(scale.x),
                bounds.size.y / Mathf.Abs(scale.y),
                bounds.size.z / Mathf.Abs(scale.z));

            Rigidbody body = root.AddComponent<Rigidbody>();
            body.mass = mass;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        private static void RemoveNestedPhysicsNamed(string namePrefix)
        {
            foreach (Transform candidate in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
            {
                if (!candidate.name.StartsWith(namePrefix) || candidate.GetComponent<MeshFilter>() != null)
                {
                    continue;
                }

                foreach (Rigidbody body in candidate.GetComponentsInChildren<Rigidbody>())
                {
                    Undo.DestroyObjectImmediate(body);
                }
                foreach (BoxCollider collider in candidate.GetComponentsInChildren<BoxCollider>())
                {
                    Undo.DestroyObjectImmediate(collider);
                }
            }
        }

        private static void DestroyIfPresent(string name)
        {
            GameObject existing = GameObject.Find(name);
            if (existing != null)
            {
                Undo.DestroyObjectImmediate(existing);
            }
        }
    }
}