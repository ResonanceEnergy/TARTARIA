// File: Assets/_Project/Scripts/AI/MudGolemAI.cs
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Tartaria.AI
{
    public class MudGolemAI : MonoBehaviour
    {
        public float MaxHealth = 100f;
        public float CurrentHealth = 100f;
        public bool IsAlive { get { return CurrentHealth > 0; } }

        private void Awake()
        {
            if (transform.childCount == 0)
            {
                var prefab = LoadMudGolemPrefab();
                if (prefab != null)
                {
                    Instantiate(prefab, transform);
                }
                else
                {
                    EnsureFallbackVisual();
                }
            }
        }

        private void TakeDamage(float damage, GameObject instigator = null)
        {
            CurrentHealth -= damage;
            Debug.Log($"[MudGolemAI] Player {instigator?.name ?? "Unknown"} dealt {damage} damage to Mud Golem. Health remaining: {CurrentHealth}");
            if (!IsAlive)
            {
                Die(instigator);
            }
        }

        private void Die(GameObject instigator = null)
        {
            Debug.Log($"[MudGolemAI] Mud Golem died. Instigator: {instigator?.name ?? "Unknown"}");
            // Additional cleanup or AI logic here
        }

        private GameObject LoadMudGolemPrefab()
        {
#if UNITY_EDITOR
    var p = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Characters/MudGolem.prefab");
    if (p != null) return p;
#endif
    return Resources.Load<GameObject>("MudGolem");
}

        private void EnsureFallbackVisual()
        {
            var urpLit = Shader.Find("Universal Render Pipeline/Lit");
            var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = "FallbackVisual_MudGolem";
            marker.transform.SetParent(transform);
            marker.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            marker.transform.localScale = Vector3.one * 0.9f;
            Destroy(marker.GetComponent<Collider>());
            if (urpLit != null)
            {
                var mat = new Material(urpLit);
                mat.SetColor("_BaseColor", new Color(0.32f, 0.22f, 0.14f));
                marker.GetComponent<Renderer>().sharedMaterial = mat;
            }
            Debug.LogWarning("[MudGolemAI] Prefab load failed — using fallback sphere marker. Check Assets/_Project/Prefabs/Characters/MudGolem.prefab");
        }
    }
}
