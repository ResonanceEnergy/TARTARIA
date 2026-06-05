// File: Assets/_Project/Scripts/AI/ResetScout.cs
using UnityEngine;
using UnityEngine.InputSystem;

namespace Tartaria.AI
{
    public class ResetScout : MonoBehaviour
    {
        private void Awake()
        {
            EnsureVisual();
        }

        private void EnsureVisual()
        {
            if (transform.childCount > 0) return;

            var prefab = LoadKayKitPrefab("Assets/_Project/Prefabs/Characters/KayKit/Char_Rogue_Hooded.prefab");
            if (prefab != null)
            {
                var body = Instantiate(prefab, transform);
                body.name = "Body";
                body.transform.localPosition = Vector3.zero;
                TintMaroon(body);
            }
            else
            {
                // Fallback ONLY if prefab is missing — single primitive marker
                // (this is the bureaucrat that lost their model)
                var marker = GameObject.CreatePrimitive(PrimitiveType.Capsule); // URP-safe
                ApplyURP(marker, new Color(0.18f, 0.10f, 0.12f));
                marker.name = "FallbackBody";
                marker.transform.SetParent(transform);
                marker.transform.localPosition = new Vector3(0f, 1f, 0f);
                Destroy(marker.GetComponent<Collider>());
                Debug.LogWarning("[ResetScout] Char_Rogue_Hooded prefab missing — using fallback capsule");
            }

            AddBucketHat();
            AddClipboardAccent();
        }

        private void TintMaroon(GameObject body)
        {
            var c = new Color(0.18f, 0.10f, 0.12f);
            var urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null) return;
            foreach (var r in body.GetComponentsInChildren<Renderer>())
            {
                var mat = new Material(urpLit);
                mat.SetColor("_BaseColor", c);
                r.sharedMaterial = mat;
            }
        }

        private void AddBucketHat()
        {
            var fbx = LoadKayKitPrefab("Assets/KayKit_RPGToolsBits_1.0_FREE/KayKit_RPGToolsBits_1.0_FREE/Assets/fbx/bucket_metal.fbx");
            if (fbx == null) return;
            var hat = Instantiate(fbx, transform);
            hat.name = "BucketHat";
            hat.transform.localPosition = new Vector3(0f, 2.1f, 0f);
            hat.transform.localScale = new Vector3(0.6f, 0.9f, 0.6f);
            ApplyURP(hat, new Color(0.18f, 0.18f, 0.20f));
        }

        private void AddClipboardAccent()
        {
            var clipboard = GameObject.CreatePrimitive(PrimitiveType.Cube); // URP-safe — accent only
            clipboard.name = "Clipboard";
            clipboard.transform.SetParent(transform);
            clipboard.transform.localPosition = new Vector3(0.35f, 1.1f, 0.3f);
            clipboard.transform.localRotation = Quaternion.Euler(0f, 25f, 8f);
            clipboard.transform.localScale = new Vector3(0.32f, 0.42f, 0.05f);
            Destroy(clipboard.GetComponent<Collider>());
            ApplyURP(clipboard, new Color(0.65f, 0.18f, 0.18f));
        }

        private static void ApplyURP(GameObject go, Color c)
        {
            var urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null) return;
            var mat = new Material(urpLit);
            mat.SetColor("_BaseColor", c);
            foreach (var r in go.GetComponentsInChildren<Renderer>()) r.sharedMaterial = mat;
        }
    }
}
