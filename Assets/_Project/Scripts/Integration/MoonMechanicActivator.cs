using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;
using Tartaria.UI;
using Tartaria.Audio;

namespace Tartaria.Integration
{
    /// <summary>
    /// Per-Moon mechanic activator. Sits next to MoonRuntimeBootstrapper on each
    /// moon stub. Switches on MoonDefinition.mechanic and spawns the corresponding
    /// gameplay loop (combat waves, excavation sites, ley-line beacons, etc.).
    ///
    /// Designed to be self-contained: relies on MudGolemHealth for damageable
    /// targets (already in EchohavenContentSpawner.cs as a public class) and
    /// runtime-built primitive props. No prefab references needed.
    /// </summary>
    [DisallowMultipleComponent]
    public class MoonMechanicActivator : MonoBehaviour
    {
        public MoonDefinition definition;
        public float startDelay = 3f;
        public int  baseEnemyCount = 4;

        readonly List<MudGolemHealth> _alive = new();
        bool _booted;

        void Start()
        {
            if (_booted) return;
            _booted = true;
            if (definition == null)
            {
                Debug.LogWarning("[MoonMechanic] No definition on " + name);
                return;
            }
            StartCoroutine(Run());
        }

        IEnumerator Run()
        {
            yield return new WaitForSeconds(startDelay);

            string banner = $"<b>MOON {definition.number:D2} — {definition.zoneName.ToUpperInvariant()}</b>";
            HUDController.Instance?.ShowObjective(banner);

            switch (definition.mechanic)
            {
                case MoonMechanic.Excavation:        yield return Mechanic_Excavation();      break;
                case MoonMechanic.DissonancePurge:   yield return Mechanic_Combat(6,  "Purge the dissonance — destroy {0} corrupted echoes."); break;
                case MoonMechanic.OrphanTrain:       yield return Mechanic_Escort();          break;
                case MoonMechanic.FortifyDefense:    yield return Mechanic_Defense();         break;
                case MoonMechanic.Amplification:     yield return Mechanic_Resonance(3);      break;
                case MoonMechanic.OrganRequiem:      yield return Mechanic_Resonance(5);      break;
                case MoonMechanic.GiantMode:         yield return Mechanic_Combat(8,  "Awaken the Giant — crush {0} stone wardens."); break;
                case MoonMechanic.AirshipArmada:     yield return Mechanic_Combat(7,  "Repel the armada — down {0} sky-wraiths."); break;
                case MoonMechanic.LeyProphecy:       yield return Mechanic_Resonance(7);      break;
                case MoonMechanic.LivingGrid:        yield return Mechanic_Resonance(9);      break;
                case MoonMechanic.SpectralVeil:      yield return Mechanic_Aquifer(); break;
                case MoonMechanic.BellTower:         yield return Mechanic_Resonance(12);     break;
                case MoonMechanic.Convergence:       yield return Mechanic_Boss();            break;
                default:                             yield return Mechanic_Combat(baseEnemyCount, "Defend the zone — defeat {0} enemies."); break;
            }

            HUDController.Instance?.ShowObjective($"<b>MOON {definition.number:D2} CLEARED</b>  +{Mathf.RoundToInt(15f + definition.number * 2f)} RS");
            GameLoopController.Instance?.QueueRSReward(15f + definition.number * 2f, $"moon{definition.number:D2}_clear");
            AudioManager.Instance?.PlaySFX2D("BuildingRestore");

            // Day-3: persist clear, raise event for HUD/obelisk, spawn return portal.
            MoonProgressTracker.Instance?.MarkCleared(definition.number);
            var portalPos = transform.position + Vector3.up * 0.5f + transform.forward * 4f;
            ReturnPortal.SpawnAt(portalPos);
            // Bridge to UI minimap waypoint via reflection (Integration→UI is one-way through reflection).
            try
            {
                var t = System.Type.GetType("Tartaria.UI.MinimapOverlay, Tartaria.UI");
                t?.GetMethod("SetWaypoint", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                  ?.Invoke(null, new object[] { portalPos, "Return Portal" });
            }
            catch { /* best effort */ }
        }

        IEnumerator Mechanic_Boss()
        {
            HUDController.Instance?.ShowObjective($"<b>BOSS:</b> Convergence—survive the harmonic siege.");
            var boss = BossEncounterSystem.Instance;
            if (boss != null) boss.StartBoss(definition.number);
            // Kick the Cosmic Convergence meta-mini-game (Moon 13).
            CosmicConvergenceMiniGame.Instance?.StartConvergence();
            // Run a parallel combat ring as wave fodder during the boss fight.
            yield return Mechanic_Combat(13, "Final convergence — silence {0} fragments.");
            // If boss not torn down, abort gracefully.
            if (boss != null && boss.IsActive) boss.AbortBoss();
            CosmicConvergenceMiniGame.Instance?.StopConvergence();
        }

        IEnumerator Mechanic_Aquifer()
        {
            HUDController.Instance?.ShowObjective("Pierce the veil — purge the corrupted aquifer.");
            AquiferPurgeMiniGame.Instance?.StartMiniGame();
            // Run a small combat ring concurrently as the spectral threat.
            yield return Mechanic_Combat(5, "Banish {0} spectral echoes while purging the depths.");
            AquiferPurgeMiniGame.Instance?.StopMiniGame();
        }

        // ─── Mechanic implementations ───────────────────────────────────────

        IEnumerator Mechanic_Combat(int count, string promptFmt)
        {
            HUDController.Instance?.ShowObjective(string.Format(promptFmt, count));
            SpawnGolemRing(count, RingRadiusForMechanic());
            yield return WaitForAllDead(60f);
        }

        // Day-12: per-mechanic radius / formation tweaks so moons feel distinct.
        float RingRadiusForMechanic()
        {
            if (definition == null) return 9f;
            switch (definition.mechanic)
            {
                case MoonMechanic.GiantMode:     return 14f;   // wide arena for big foes
                case MoonMechanic.AirshipArmada: return 16f;   // sky scatter
                case MoonMechanic.SpectralVeil:  return 6f;    // close, eerie
                case MoonMechanic.Convergence:   return 5f;    // boss arena
                default:                         return 9f;
            }
        }

        IEnumerator Mechanic_Excavation()
        {
            const int siteCount = 4;
            HUDController.Instance?.ShowObjective($"Excavate {siteCount} buried Aether sites — walk close to a beacon.");
            var beacons = new List<GameObject>();
            for (int i = 0; i < siteCount; i++)
            {
                float a = (i / (float)siteCount) * Mathf.PI * 2f;
                Vector3 p = transform.position + new Vector3(Mathf.Cos(a) * 8f, 0f, Mathf.Sin(a) * 8f);
                beacons.Add(BuildBeacon(p, new Color(0.95f, 0.78f, 0.30f)));
            }

            int excavated = 0;
            float t = 0f;
            while (excavated < siteCount && t < 90f)
            {
                t += Time.deltaTime;
                yield return null;
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player == null) continue;
                for (int i = beacons.Count - 1; i >= 0; i--)
                {
                    var b = beacons[i];
                    if (b == null) { beacons.RemoveAt(i); continue; }
                    if (Vector3.Distance(player.transform.position, b.transform.position) < 2.5f)
                    {
                        excavated++;
                        GameLoopController.Instance?.QueueRSReward(8f, "moon_excavation");
                        AudioManager.Instance?.PlaySFX("DigSuccess", b.transform.position);
                        Destroy(b);
                        beacons.RemoveAt(i);
                        HUDController.Instance?.ShowObjective($"Excavated {excavated}/{siteCount}");
                        break;
                    }
                }
            }
        }

        IEnumerator Mechanic_Escort()
        {
            HUDController.Instance?.ShowObjective("Escort the orphan train — survive 2 ambush waves.");
            for (int wave = 1; wave <= 2; wave++)
            {
                HUDController.Instance?.ShowObjective($"Ambush wave {wave}/2 incoming!");
                SpawnGolemRing(3 + wave, 7f);
                yield return WaitForAllDead(60f);
                yield return new WaitForSeconds(3f);
            }
        }

        IEnumerator Mechanic_Defense()
        {
            HUDController.Instance?.ShowObjective("Hold the bastion — survive 3 escalating waves.");
            int[] sizes = { 3, 5, 7 };
            for (int i = 0; i < sizes.Length; i++)
            {
                HUDController.Instance?.ShowObjective($"Wave {i + 1}/3 — defend!");
                SpawnGolemRing(sizes[i], 8f);
                yield return WaitForAllDead(70f);
                if (i < sizes.Length - 1) yield return new WaitForSeconds(3f);
            }
        }

        IEnumerator Mechanic_Resonance(int chimeCount)
        {
            // Day-15: surface the appropriate mini-game alongside the chime ring.
            if (definition != null)
            {
                switch (definition.mechanic)
                {
                    case MoonMechanic.BellTower:    BellTowerSyncMiniGame.Instance?.StartMiniGame(); break;
                    case MoonMechanic.LeyProphecy:  LeyLineProphecyMiniGame.Instance?.StartMiniGame(); break;
                }
            }
            HUDController.Instance?.ShowObjective($"Activate {chimeCount} resonance chimes — walk through each pillar.");
            var chimes = new List<GameObject>();
            for (int i = 0; i < chimeCount; i++)
            {
                float a = (i / (float)chimeCount) * Mathf.PI * 2f;
                Vector3 p = transform.position + new Vector3(Mathf.Cos(a) * 7f, 0f, Mathf.Sin(a) * 7f);
                chimes.Add(BuildBeacon(p, new Color(0.55f, 0.72f, 1.0f)));
            }

            int rung = 0;
            float t = 0f;
            while (rung < chimeCount && t < 90f)
            {
                t += Time.deltaTime;
                yield return null;
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player == null) continue;
                for (int i = chimes.Count - 1; i >= 0; i--)
                {
                    var c = chimes[i];
                    if (c == null) { chimes.RemoveAt(i); continue; }
                    if (Vector3.Distance(player.transform.position, c.transform.position) < 2.5f)
                    {
                        rung++;
                        GameLoopController.Instance?.QueueRSReward(5f, "moon_resonance");
                        AudioManager.Instance?.PlaySFX("Discovery", c.transform.position);
                        Destroy(c);
                        chimes.RemoveAt(i);
                        HUDController.Instance?.ShowObjective($"Chimes rung: {rung}/{chimeCount}");
                        break;
                    }
                }
            }

            // Tear down mini-game side-channel when chime ring is complete.
            if (definition != null)
            {
                switch (definition.mechanic)
                {
                    case MoonMechanic.BellTower:    BellTowerSyncMiniGame.Instance?.StopMiniGame(); break;
                    case MoonMechanic.LeyProphecy:  LeyLineProphecyMiniGame.Instance?.StopMiniGame(); break;
                }
            }
        }

        // ─── Spawning helpers ───────────────────────────────────────────────

        void SpawnGolemRing(int count, float radius)
        {
            // Day-12: per-mechanic stat + visual scaling so moons feel distinct.
            float hpScale = 1f, sizeScale = 1f, yOffset = 0f;
            Color tint = new Color(0.30f, 0.22f, 0.18f);
            bool spectral = false;
            if (definition != null)
            {
                switch (definition.mechanic)
                {
                    case MoonMechanic.GiantMode:     hpScale = 2.5f; sizeScale = 1.8f; tint = new Color(0.45f, 0.30f, 0.22f); break;
                    case MoonMechanic.AirshipArmada: hpScale = 0.7f; sizeScale = 0.85f; yOffset = 6f; tint = new Color(0.55f, 0.55f, 0.75f); break;
                    case MoonMechanic.SpectralVeil:  hpScale = 0.6f; sizeScale = 1.1f; tint = new Color(0.65f, 0.85f, 1.0f); spectral = true; break;
                    case MoonMechanic.DissonancePurge: tint = new Color(0.55f, 0.20f, 0.30f); break;
                    case MoonMechanic.Convergence:   hpScale = 4.0f; sizeScale = 2.4f; tint = new Color(0.85f, 0.30f, 0.85f); count = 1; break;
                }
                // Mild scaling by moon number for late-game pressure
                hpScale *= 1f + (definition.number - 1) * 0.05f;
            }

            Vector3 center = transform.position + Vector3.up * yOffset;
            for (int i = 0; i < count; i++)
            {
                float a = (i / (float)Mathf.Max(1, count)) * Mathf.PI * 2f;
                Vector3 p = center + new Vector3(Mathf.Cos(a) * radius, 0f, Mathf.Sin(a) * radius);
                var golem = BuildSimpleGolem(p);
                if (sizeScale != 1f) golem.transform.localScale = Vector3.one * sizeScale;
                if (tint != new Color(0.30f, 0.22f, 0.18f)) RetintGolem(golem, tint, spectral);
                var h = golem.GetComponent<MudGolemHealth>() ?? golem.AddComponent<MudGolemHealth>();
                h.MaxHealth = 50f * hpScale;
                h.CurrentHealth = h.MaxHealth;
                // Day-2: real chase/attack AI. MudGolemAI fires OnAnyGolemDied through MudGolemHealth.
                if (golem.GetComponent<Tartaria.AI.MudGolemAI>() == null)
                    golem.AddComponent<Tartaria.AI.MudGolemAI>();
                _alive.Add(h);
            }
        }

        static void RetintGolem(GameObject root, Color c, bool spectral)
        {
            foreach (var r in root.GetComponentsInChildren<MeshRenderer>())
            {
                var sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                var m = new Material(sh);
                Color final = c;
                if (spectral) final.a = 0.55f;
                m.color = final;
                if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", final);
                if (spectral)
                {
                    m.EnableKeyword("_EMISSION");
                    if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", c * 1.5f);
                }
                r.sharedMaterial = m;
            }
        }

        IEnumerator WaitForAllDead(float timeout)
        {
            float t = 0f;
            while (t < timeout)
            {
                t += Time.deltaTime;
                _alive.RemoveAll(g => g == null);
                if (_alive.Count == 0) yield break;
                yield return null;
            }
        }

        GameObject BuildSimpleGolem(Vector3 pos)
        {
            var root = new GameObject("MoonGolem");
            root.transform.position = pos;

            var torso = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            torso.transform.SetParent(root.transform, false);
            torso.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            torso.transform.localScale = new Vector3(1.2f, 1.4f, 1f);
            TintRenderer(torso, new Color(0.30f, 0.22f, 0.18f));

            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.transform.SetParent(root.transform, false);
            head.transform.localPosition = new Vector3(0f, 2.2f, 0f);
            head.transform.localScale = Vector3.one * 0.7f;
            TintRenderer(head, new Color(0.30f, 0.22f, 0.18f));

            var col = root.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0f, 1.2f, 0f);
            col.height = 2.8f;
            col.radius = 0.7f;

            var rb = root.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            int enemyLayer = LayerMask.NameToLayer("Enemy");
            if (enemyLayer >= 0) SetLayerRecursive(root, enemyLayer);
            return root;
        }

        GameObject BuildBeacon(Vector3 pos, Color color)
        {
            var root = new GameObject("Beacon");
            root.transform.position = pos;

            var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.transform.SetParent(root.transform, false);
            pillar.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            pillar.transform.localScale = new Vector3(0.4f, 1.2f, 0.4f);
            Object.DestroyImmediate(pillar.GetComponent<CapsuleCollider>());
            TintRenderer(pillar, color, emissive: true);

            var glow = pillar.AddComponent<Light>();
            glow.type = LightType.Point;
            glow.color = color;
            glow.intensity = 3f;
            glow.range = 6f;
            return root;
        }

        static void TintRenderer(GameObject go, Color c, bool emissive = false)
        {
            var r = go.GetComponent<MeshRenderer>();
            if (r == null) return;
            var sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var m = new Material(sh);
            m.color = c;
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
            if (emissive)
            {
                m.EnableKeyword("_EMISSION");
                if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", c * 2.0f);
            }
            r.sharedMaterial = m;
        }

        static void SetLayerRecursive(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform t in go.transform)
                SetLayerRecursive(t.gameObject, layer);
        }
    }
}
