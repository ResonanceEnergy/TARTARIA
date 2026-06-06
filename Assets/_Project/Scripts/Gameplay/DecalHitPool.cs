using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Decal Hit Pool — pre-spawns 32 decal projectors (or flat quads if DecalProjector
    /// is unavailable) and spawns them at hit positions. Auto-fades alpha 1→0 over 12s.
    /// Used for impact marks, blood splatter, etc.
    /// </summary>
    [DisallowMultipleComponent]
    public class DecalHitPool : MonoBehaviour
    {
        [SerializeField] int poolSize = 32;
        [SerializeField] float fadeOutDuration = 12f;
        [SerializeField] Vector2 decalSize = new(1f, 1f);
        [SerializeField] Color decalColor = new(0.2f, 0.1f, 0.05f, 1f); // Dark red-brown

        static DecalHitPool s_instance;

        readonly Queue<GameObject> _availableDecals = new();
        readonly List<DecalFadeState> _activeDecals = new();

        System.Type _decalProjectorType;
        bool _useQuadFallback;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (s_instance != null) return;
            var go = new GameObject("DecalHitPool");
            DontDestroyOnLoad(go);
            s_instance = go.AddComponent<DecalHitPool>();
        }

        void Awake()
        {
            // Check if URP DecalProjector is available
            _decalProjectorType = System.Type.GetType("UnityEngine.Rendering.Universal.DecalProjector, Unity.RenderPipelines.Universal.Runtime");
            _useQuadFallback = (_decalProjectorType == null);

            if (_useQuadFallback)
                Debug.Log("[DecalHitPool] DecalProjector not available — using flat quad fallback.");

            // Pre-spawn pool
            for (int i = 0; i < poolSize; i++)
            {
                var decal = _useQuadFallback ? CreateQuadDecal() : CreateDecalProjector();
                decal.SetActive(false);
                decal.transform.SetParent(transform);
                _availableDecals.Enqueue(decal);
            }

            Debug.Log($"[DecalHitPool] Initialized pool of {poolSize} decals.");
        }

        void OnDestroy()
        {
            if (s_instance == this) s_instance = null;
        }

        void Update()
        {
            // Fade active decals
            for (int i = _activeDecals.Count - 1; i >= 0; i--)
            {
                var state = _activeDecals[i];
                state.elapsed += Time.deltaTime;
                float alpha = 1f - (state.elapsed / fadeOutDuration);

                if (alpha <= 0f)
                {
                    // Return to pool
                    state.decal.SetActive(false);
                    _availableDecals.Enqueue(state.decal);
                    _activeDecals.RemoveAt(i);
                }
                else
                {
                    // Update alpha
                    SetDecalAlpha(state.decal, alpha);
                }
            }
        }

        // ─── Public API ──────────────────────────────

        /// <summary>
        /// Spawn a decal at the given world position aligned to the surface normal.
        /// </summary>
        public static void Spawn(Vector3 worldPos, Vector3 normal)
        {
            if (s_instance == null)
            {
                Debug.LogWarning("[DecalHitPool] Instance not initialized!");
                return;
            }

            if (s_instance._availableDecals.Count == 0)
            {
                // Pool exhausted — steal oldest active decal
                var oldest = s_instance._activeDecals[0];
                s_instance._activeDecals.RemoveAt(0);
                oldest.decal.SetActive(false);
                s_instance._availableDecals.Enqueue(oldest.decal);
            }

            var decal = s_instance._availableDecals.Dequeue();
            decal.transform.position = worldPos;
            decal.transform.rotation = Quaternion.LookRotation(normal);
            decal.SetActive(true);

            s_instance.SetDecalAlpha(decal, 1f);
            s_instance._activeDecals.Add(new DecalFadeState { decal = decal, elapsed = 0f });
        }

        // ─── Decal Creation ──────────────────────────

        GameObject CreateDecalProjector()
        {
            var go = new GameObject("Decal_Projector");
            var comp = go.AddComponent(_decalProjectorType);

            // Set size via reflection
            TrySetProperty(comp, "size", new Vector3(decalSize.x, decalSize.y, 0.5f));
            TrySetProperty(comp, "fadeFactor", 1f);

            return go;
        }

        GameObject CreateQuadDecal()
        {
            var go = new GameObject("Decal_Quad");

            var mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = CreateQuadMesh();

            var mr = go.AddComponent<MeshRenderer>();
            var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            mat.SetColor("_BaseColor", decalColor);
            mat.SetFloat("_Surface", 1); // Transparent
            mat.SetFloat("_Blend", 0);   // Alpha
            mat.renderQueue = 3000;      // Transparent queue
            mr.sharedMaterial = mat;

            return go;
        }

        Mesh CreateQuadMesh()
        {
            var mesh = new Mesh { name = "DecalQuad" };
            float hw = decalSize.x * 0.5f;
            float hh = decalSize.y * 0.5f;

            mesh.vertices = new Vector3[]
            {
                new(-hw, -hh, 0), new(hw, -hh, 0),
                new(hw, hh, 0), new(-hw, hh, 0)
            };
            mesh.uv = new Vector2[]
            {
                new(0, 0), new(1, 0), new(1, 1), new(0, 1)
            };
            mesh.triangles = new int[] { 0, 2, 1, 0, 3, 2 };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        void SetDecalAlpha(GameObject decal, float alpha)
        {
            if (_useQuadFallback)
            {
                var mr = decal.GetComponent<MeshRenderer>();
                if (mr != null && mr.sharedMaterial != null)
                {
                    var color = mr.sharedMaterial.GetColor("_BaseColor");
                    color.a = alpha;
                    mr.sharedMaterial.SetColor("_BaseColor", color);
                }
            }
            else
            {
                // DecalProjector fading via reflection (fadeFactor property)
                var comp = decal.GetComponent(_decalProjectorType);
                TrySetProperty(comp, "fadeFactor", alpha);
            }
        }

        void TrySetProperty(object obj, string propName, object value)
        {
            if (obj == null) return;
            var prop = obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.CanWrite)
            {
                try { prop.SetValue(obj, value); }
                catch { /* Ignore */ }
            }
        }

        struct DecalFadeState
        {
            public GameObject decal;
            public float elapsed;
        }
    }
}
