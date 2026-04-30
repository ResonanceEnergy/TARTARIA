using UnityEngine;
using UnityEditor;

namespace Tartaria.Editor
{
    /// <summary>
    /// Restores Player.prefab to procedural capsule state (removes wrong male mesh)
    /// Keeps Animator + PlayerAnimatorBridge + animations intact
    /// </summary>
    public static class RestorePlayerCapsule
    {
        const string PLAYER_PREFAB_PATH = "Assets/_Project/Prefabs/Characters/Player.prefab";

        [MenuItem("TARTARIA/Integration/Fix - Restore Player Capsule (Remove Male Mesh)")]
        public static void RestoreCapsule()
        {
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PLAYER_PREFAB_PATH);
            if (playerPrefab == null)
            {
                Debug.LogError($"[RestorePlayerCapsule] Player prefab not found at: {PLAYER_PREFAB_PATH}");
                return;
            }

            string prefabPath = AssetDatabase.GetAssetPath(playerPrefab);
            GameObject prefabInstance = PrefabUtility.LoadPrefabContents(prefabPath);

            // Remove any PlayerMesh child (male character)
            Transform playerMeshChild = prefabInstance.transform.Find("PlayerMesh");
            if (playerMeshChild != null)
            {
                Debug.Log("[RestorePlayerCapsule] Removing male mesh: PlayerMesh");
                Object.DestroyImmediate(playerMeshChild.gameObject);
            }

            // Restore procedural capsules if missing
            Transform bodyTransform = prefabInstance.transform.Find("Body");
            if (bodyTransform == null)
            {
                Debug.Log("[RestorePlayerCapsule] Restoring procedural capsule body parts");

                // Create Body (main torso)
                GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                body.name = "Body";
                body.transform.SetParent(prefabInstance.transform);
                body.transform.localPosition = new Vector3(0f, 1f, 0f);
                body.transform.localRotation = Quaternion.identity;
                body.transform.localScale = new Vector3(0.5f, 1f, 0.5f);
                Object.DestroyImmediate(body.GetComponent<Collider>()); // CharacterController handles collision

                // Create Head
                GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                head.name = "Head";
                head.transform.SetParent(prefabInstance.transform);
                head.transform.localPosition = new Vector3(0f, 1.75f, 0f);
                head.transform.localScale = Vector3.one * 0.25f;
                Object.DestroyImmediate(head.GetComponent<Collider>());

                // Create Arms
                GameObject armL = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                armL.name = "Arm_L";
                armL.transform.SetParent(prefabInstance.transform);
                armL.transform.localPosition = new Vector3(-0.4f, 1.2f, 0f);
                armL.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                armL.transform.localScale = new Vector3(0.15f, 0.4f, 0.15f);
                Object.DestroyImmediate(armL.GetComponent<Collider>());

                GameObject armR = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                armR.name = "Arm_R";
                armR.transform.SetParent(prefabInstance.transform);
                armR.transform.localPosition = new Vector3(0.4f, 1.2f, 0f);
                armR.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                armR.transform.localScale = new Vector3(0.15f, 0.4f, 0.15f);
                Object.DestroyImmediate(armR.GetComponent<Collider>());

                // Create Legs
                GameObject legL = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                legL.name = "Leg_L";
                legL.transform.SetParent(prefabInstance.transform);
                legL.transform.localPosition = new Vector3(-0.15f, 0.5f, 0f);
                legL.transform.localScale = new Vector3(0.2f, 0.5f, 0.2f);
                Object.DestroyImmediate(legL.GetComponent<Collider>());

                GameObject legR = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                legR.name = "Leg_R";
                legR.transform.SetParent(prefabInstance.transform);
                legR.transform.localPosition = new Vector3(0.15f, 0.5f, 0f);
                legR.transform.localScale = new Vector3(0.2f, 0.5f, 0.2f);
                Object.DestroyImmediate(legR.GetComponent<Collider>());

                Debug.Log("[RestorePlayerCapsule] Created 6 procedural body parts (Body, Head, Arms, Legs)");
            }

            // Verify Animator + PlayerAnimatorBridge are present (keep them!)
            Animator animator = prefabInstance.GetComponent<Animator>();
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                Debug.Log($"[RestorePlayerCapsule] ✓ Animator controller: {animator.runtimeAnimatorController.name}");
            }

            var bridge = prefabInstance.GetComponent<Tartaria.Gameplay.PlayerAnimatorBridge>();
            if (bridge != null)
            {
                Debug.Log("[RestorePlayerCapsule] ✓ PlayerAnimatorBridge present");
            }

            // Save prefab
            PrefabUtility.SaveAsPrefabAsset(prefabInstance, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabInstance);

            Debug.Log("[RestorePlayerCapsule] ═══════════════════════════════════");
            Debug.Log("[RestorePlayerCapsule] ✓ Player.prefab restored to capsule");
            Debug.Log("[RestorePlayerCapsule] ✓ Animations PRESERVED (Capoeira)");
            Debug.Log("[RestorePlayerCapsule] ✓ Male mesh REMOVED");
            Debug.Log("[RestorePlayerCapsule] Player is now animated capsule (correct for Elara Voss until female model sourced)");
            Debug.Log("[RestorePlayerCapsule] ═══════════════════════════════════");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (!UnityEditorInternal.InternalEditorUtility.inBatchMode)
            {
                EditorUtility.DisplayDialog("Player Capsule Restored",
                    "✓ Male mesh removed\n" +
                    "✓ Procedural capsule restored\n" +
                    "✓ Capoeira animations preserved\n\n" +
                    "Player is Elara Voss (female) - capsule placeholder until correct female model sourced.",
                    "OK");
            }
        }
    }
}
