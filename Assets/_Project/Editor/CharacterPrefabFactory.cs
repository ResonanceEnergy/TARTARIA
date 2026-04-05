using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;
using Tartaria.Input;
using Tartaria.Integration;

namespace Tartaria.Editor
{
    /// <summary>
    /// Generates greybox character prefabs using Unity primitives.
    /// Player (capsule), Milo companion (small sphere), Mud Golem enemy (stacked spheres).
    ///
    /// Menu: Tartaria > Build Assets > Character Prefabs
    /// </summary>
    public static class CharacterPrefabFactory
    {
        const string PrefabsPath = "Assets/_Project/Prefabs/Characters";
        const string MaterialsPath = "Assets/_Project/Materials";

        [MenuItem("Tartaria/Build Assets/Character Prefabs", false, 14)]
        public static void BuildAllCharacters()
        {
            EnsureDirectory(PrefabsPath);
            CreatePlayerPrefab();
            CreateMiloPrefab();
            CreateMudGolemPrefab();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Tartaria] Character prefabs built (Player, Milo, MudGolem).");
        }

        static void CreatePlayerPrefab()
        {
            string path = $"{PrefabsPath}/Player.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;

            var root = new GameObject("Player");
            root.tag = "Player";

            // Body — capsule
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(root.transform);
            body.transform.localPosition = new Vector3(0f, 1f, 0f);
            body.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
            Object.DestroyImmediate(body.GetComponent<CapsuleCollider>());
            AssignMaterial(body, "Player_Aether");

            // Head — sphere
            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(root.transform);
            head.transform.localPosition = new Vector3(0f, 2.2f, 0f);
            head.transform.localScale = Vector3.one * 0.5f;
            Object.DestroyImmediate(head.GetComponent<SphereCollider>());
            AssignMaterial(head, "Player_Aether");

            // Aether glow indicator — small sphere on chest
            var glow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            glow.name = "AetherCore";
            glow.transform.SetParent(root.transform);
            glow.transform.localPosition = new Vector3(0f, 1.5f, 0.3f);
            glow.transform.localScale = Vector3.one * 0.2f;
            Object.DestroyImmediate(glow.GetComponent<SphereCollider>());
            AssignMaterial(glow, "Aether_Glow");

            // Components
            var cc = root.AddComponent<CharacterController>();
            cc.center = new Vector3(0f, 1f, 0f);
            cc.height = 2f;
            cc.radius = 0.4f;

            var pih = root.AddComponent<PlayerInputHandler>();

            // Assign input actions asset if it exists
            var inputAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                InputActionsFactory.AssetPath);
            if (inputAsset != null)
            {
                var so = new SerializedObject(pih);
                var prop = so.FindProperty("inputActions");
                if (prop != null) prop.objectReferenceValue = inputAsset;
                so.ApplyModifiedProperties();
            }

            root.AddComponent<Rigidbody>().isKinematic = true;

            // Camera follow target
            var camTarget = new GameObject("CameraTarget");
            camTarget.transform.SetParent(root.transform);
            camTarget.transform.localPosition = new Vector3(0f, 1.8f, 0f);

            // Interaction trigger
            var interactSphere = new GameObject("InteractionTrigger");
            interactSphere.transform.SetParent(root.transform);
            interactSphere.transform.localPosition = Vector3.zero;
            var sc = interactSphere.AddComponent<SphereCollider>();
            sc.isTrigger = true;
            sc.radius = 3f;

            SavePrefab(root, path);
        }

        static void CreateMiloPrefab()
        {
            string path = $"{PrefabsPath}/Milo.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;

            var root = new GameObject("Milo");

            // Body — small rounded shape (sphere + cylinder)
            var body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            body.name = "Body";
            body.transform.SetParent(root.transform);
            body.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            body.transform.localScale = new Vector3(0.6f, 0.5f, 0.6f);
            Object.DestroyImmediate(body.GetComponent<SphereCollider>());
            AssignMaterial(body, "Aether_Glow");

            // Eye indicators — two tiny spheres
            for (int i = 0; i < 2; i++)
            {
                var eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                eye.name = $"Eye_{i}";
                eye.transform.SetParent(body.transform);
                eye.transform.localPosition = new Vector3(i == 0 ? -0.15f : 0.15f, 0.2f, 0.35f);
                eye.transform.localScale = Vector3.one * 0.15f;
                Object.DestroyImmediate(eye.GetComponent<SphereCollider>());
                AssignMaterial(eye, "Crystal_Active");
            }

            // Inner glow core
            var core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            core.name = "GlowCore";
            core.transform.SetParent(root.transform);
            core.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            core.transform.localScale = Vector3.one * 0.35f;
            Object.DestroyImmediate(core.GetComponent<SphereCollider>());
            AssignMaterial(core, "Aether_Glow");

            // Components
            root.AddComponent<MiloController>();
            var rb = root.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            var col = root.AddComponent<SphereCollider>();
            col.radius = 0.4f;
            col.center = new Vector3(0f, 0.5f, 0f);

            // Follow target marker
            var followTarget = new GameObject("FollowOffset");
            followTarget.transform.SetParent(root.transform);
            followTarget.transform.localPosition = new Vector3(-1.5f, 0.5f, -0.5f);

            SavePrefab(root, path);
        }

        static void CreateMudGolemPrefab()
        {
            string path = $"{PrefabsPath}/MudGolem.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;

            var root = new GameObject("MudGolem");

            // Torso — large sphere
            var torso = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            torso.name = "Torso";
            torso.transform.SetParent(root.transform);
            torso.transform.localPosition = new Vector3(0f, 1.5f, 0f);
            torso.transform.localScale = new Vector3(1.8f, 2f, 1.5f);
            Object.DestroyImmediate(torso.GetComponent<SphereCollider>());
            AssignMaterial(torso, "MudGolem_Body");

            // Head — smaller sphere
            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(root.transform);
            head.transform.localPosition = new Vector3(0f, 3f, 0f);
            head.transform.localScale = Vector3.one * 0.9f;
            Object.DestroyImmediate(head.GetComponent<SphereCollider>());
            AssignMaterial(head, "MudGolem_Body");

            // Arms — cylinders
            for (int i = 0; i < 2; i++)
            {
                float side = i == 0 ? -1f : 1f;
                var arm = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                arm.name = $"Arm_{(i == 0 ? "L" : "R")}";
                arm.transform.SetParent(root.transform);
                arm.transform.localPosition = new Vector3(side * 1.3f, 1.5f, 0f);
                arm.transform.localScale = new Vector3(0.4f, 0.8f, 0.4f);
                arm.transform.localRotation = Quaternion.Euler(0f, 0f, side * 20f);
                Object.DestroyImmediate(arm.GetComponent<CapsuleCollider>());
                AssignMaterial(arm, "MudGolem_Body");
            }

            // Legs — short cylinders
            for (int i = 0; i < 2; i++)
            {
                float side = i == 0 ? -0.5f : 0.5f;
                var leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                leg.name = $"Leg_{(i == 0 ? "L" : "R")}";
                leg.transform.SetParent(root.transform);
                leg.transform.localPosition = new Vector3(side, 0.4f, 0f);
                leg.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                Object.DestroyImmediate(leg.GetComponent<CapsuleCollider>());
                AssignMaterial(leg, "MudGolem_Body");
            }

            // Corruption glow eyes
            for (int i = 0; i < 2; i++)
            {
                var eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                eye.name = $"Eye_{i}";
                eye.transform.SetParent(head.transform);
                eye.transform.localPosition = new Vector3(i == 0 ? -0.2f : 0.2f, 0.1f, 0.35f);
                eye.transform.localScale = Vector3.one * 0.15f;
                Object.DestroyImmediate(eye.GetComponent<SphereCollider>());
                // Corruption eyes — red/purple tinted (reuse Aether_Glow for now)
                AssignMaterial(eye, "Aether_Glow");
            }

            // Components
            var col = root.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0f, 1.5f, 0f);
            col.height = 3.5f;
            col.radius = 0.9f;

            var rb = root.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.mass = 10f;

            // Hit point
            var hitBox = new GameObject("HitBox");
            hitBox.transform.SetParent(root.transform);
            hitBox.transform.localPosition = new Vector3(0f, 1.5f, 0f);
            var bc = hitBox.AddComponent<BoxCollider>();
            bc.isTrigger = true;
            bc.size = new Vector3(2f, 3f, 1.5f);

            SavePrefab(root, path);
        }

        static void AssignMaterial(GameObject go, string matName)
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialsPath}/{matName}.mat");
            if (mat != null)
            {
                var renderer = go.GetComponent<MeshRenderer>();
                if (renderer != null) renderer.sharedMaterial = mat;
            }
        }

        static void SavePrefab(GameObject go, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            Debug.Log($"[Tartaria] Prefab created: {path}");
        }

        static void EnsureDirectory(string assetPath)
        {
            string fullPath = Path.Combine(Application.dataPath, "..", assetPath);
            if (!Directory.Exists(fullPath))
                Directory.CreateDirectory(fullPath);
            AssetDatabase.Refresh();
        }
    }
}
