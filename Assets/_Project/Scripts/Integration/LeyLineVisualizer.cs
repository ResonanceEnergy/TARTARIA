using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Reads ECS LeyLineNode / LeyLineConnection data and renders the ley line network
    /// using GL immediate-mode lines in OnRenderObject().
    ///
    /// Visual identity ("future-proof 2100"):
    ///   Active lines  — golden / amber glow matching 432 Hz resonance palette.
    ///   Weak lines    — dim purple haze (energy flow below threshold).
    ///   Severed lines — dark crimson (corruption break).
    ///   Nodes         — small bright point drawn as a star cross (+).
    ///
    /// Visibility: hidden by default. Shown when AetherVision is active (Tab / AetherVision action)
    /// or when a building Scan fires. AlwaysVisible flag available for debugging.
    /// </summary>
    [DefaultExecutionOrder(-30)]
    public class LeyLineVisualizer : MonoBehaviour
    {
        // ─── Colours ─────────────────────────────────
        Color _goldActive  = new(0.95f, 0.82f, 0.30f, 0.80f);
        Color _goldPulse   = new(1.00f, 0.98f, 0.60f, 1.00f);
        Color _weakPurple  = new(0.40f, 0.28f, 0.55f, 0.45f);
        Color _severedRed  = new(0.55f, 0.10f, 0.10f, 0.35f);

        // ─── State ───────────────────────────────────
        public bool IsAlwaysVisible;   // Set true for dev/debug
        bool _aetherVisionOn;
        bool _scanActive;
        float _scanTimer;              // seconds remaining on post-scan visibility window

        Material _mat;

        // ─── ECS cache (refreshed every CacheInterval seconds) ───────────
        struct NodeCache   { public Vector3 pos; public float strength; public bool active; }
        struct ConnCache   { public int a; public int b; public float flow; public bool severed; }

        readonly List<NodeCache> _nodes = new();
        readonly List<ConnCache> _conns = new();
        float _refreshTimer;
        const float CacheInterval = 0.33f;  // ~3 Hz refresh — cheap for ≤64 nodes

        // ─── Lifecycle ───────────────────────────────

        void Awake()
        {
            _mat = new Material(Shader.Find("Hidden/Internal-Colored"))
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            _mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            _mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            _mat.SetInt("_Cull",     (int)UnityEngine.Rendering.CullMode.Off);
            _mat.SetInt("_ZWrite",   0);
        }

        void OnEnable()
        {
            GameEvents.OnToggleAetherVision += HandleAetherVisionToggle;
        }

        void OnDisable()
        {
            GameEvents.OnToggleAetherVision -= HandleAetherVisionToggle;
        }

        void OnDestroy()
        {
            if (_mat != null) Destroy(_mat);
        }

        void HandleAetherVisionToggle()
        {
            _aetherVisionOn = !_aetherVisionOn;
        }

        // Call this from PlayerInputHandler.OnScan (or GameLoopController) to
        // flash the ley lines briefly after a scan pulse.
        public void ShowScanFlash(float duration = 4f)
        {
            _scanActive = true;
            _scanTimer = duration;
        }

        bool ShouldRender => IsAlwaysVisible || _aetherVisionOn || _scanActive;

        // ─── Update ──────────────────────────────────

        void Update()
        {
            if (_scanActive)
            {
                _scanTimer -= Time.deltaTime;
                if (_scanTimer <= 0f) _scanActive = false;
            }

            _refreshTimer -= Time.deltaTime;
            if (_refreshTimer <= 0f)
            {
                _refreshTimer = CacheInterval;
                RefreshECSCache();
            }
        }

        void RefreshECSCache()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated) return;

            var em = world.EntityManager;
            _nodes.Clear();
            _conns.Clear();

            // Collect all LeyLineNode entities
            using var query  = em.CreateEntityQuery(ComponentType.ReadOnly<LeyLineNode>());
            using var entities = query.ToEntityArray(Allocator.Temp);

            // Map NodeIndex → list position for connection lookup
            var indexToSlot = new Dictionary<int, int>(entities.Length);

            for (int i = 0; i < entities.Length; i++)
            {
                var n = em.GetComponentData<LeyLineNode>(entities[i]);
                indexToSlot[n.NodeIndex] = _nodes.Count;
                _nodes.Add(new NodeCache
                {
                    pos      = new Vector3(n.Position.x, n.Position.y + 0.5f, n.Position.z),
                    strength = n.Strength,
                    active   = n.Active
                });
            }

            // Collect connections (each pair once: fromIndex < toIndex)
            for (int i = 0; i < entities.Length; i++)
            {
                if (!em.HasBuffer<LeyLineConnection>(entities[i])) continue;
                var buf = em.GetBuffer<LeyLineConnection>(entities[i], true);
                var n   = em.GetComponentData<LeyLineNode>(entities[i]);

                for (int j = 0; j < buf.Length; j++)
                {
                    var c = buf[j];
                    if (n.NodeIndex >= c.TargetNodeIndex) continue; // deduplicate

                    if (!indexToSlot.TryGetValue(n.NodeIndex,        out int slotA)) continue;
                    if (!indexToSlot.TryGetValue(c.TargetNodeIndex,   out int slotB)) continue;

                    _conns.Add(new ConnCache
                    {
                        a      = slotA,
                        b      = slotB,
                        flow   = c.FlowRate,
                        severed = c.Severed
                    });
                }
            }
        }

        // ─── GL Rendering ────────────────────────────

        void OnRenderObject()
        {
            if (!ShouldRender) return;
            if (_mat == null || _conns.Count == 0) return;

            _mat.SetPass(0);

            GL.PushMatrix();

            // ─── Lines ─────────────────────────────
            GL.Begin(GL.LINES);
            foreach (var c in _conns)
            {
                if (c.a < 0 || c.b < 0 || c.a >= _nodes.Count || c.b >= _nodes.Count) continue;

                var na = _nodes[c.a];
                var nb = _nodes[c.b];

                Color col;
                if (c.severed)
                    col = _severedRed;
                else
                {
                    float t   = Mathf.Clamp01(c.flow);
                    float avg = (na.strength + nb.strength) * 0.5f;
                    Color base_ = Color.Lerp(_weakPurple, _goldActive, avg);
                    // Pulse: flicker slightly at 432 Hz harmonic (2.618 = φ² visible throb)
                    float pulse = Mathf.Abs(Mathf.Sin(Time.time * 2.618f)) * 0.2f * avg;
                    col = Color.Lerp(base_, _goldPulse, pulse);
                    col.a *= t;
                }

                GL.Color(col);
                GL.Vertex(na.pos);
                GL.Vertex(nb.pos);
            }
            GL.End();

            // ─── Node markers (small × cross at each active node) ────────
            GL.Begin(GL.LINES);
            const float Half = 0.4f;
            foreach (var n in _nodes)
            {
                if (!n.active) continue;
                Color nc = Color.Lerp(_weakPurple, _goldPulse, n.strength);
                GL.Color(nc);
                // Horizontal arm
                GL.Vertex(n.pos + new Vector3(-Half, 0f, 0f));
                GL.Vertex(n.pos + new Vector3( Half, 0f, 0f));
                // Depth arm
                GL.Vertex(n.pos + new Vector3(0f, 0f, -Half));
                GL.Vertex(n.pos + new Vector3(0f, 0f,  Half));
            }
            GL.End();

            GL.PopMatrix();
        }
    }
}
