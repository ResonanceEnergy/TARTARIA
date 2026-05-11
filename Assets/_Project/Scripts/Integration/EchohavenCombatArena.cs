using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tartaria.UI;
using Tartaria.Audio;

namespace Tartaria.Integration
{
    /// <summary>
    /// Drops the player into immediate, escalating combat the moment Echohaven loads.
    /// Three waves of corrupted Mud Golems converge on the player; each kill grants RS,
    /// each cleared wave plays a banner + spawns the next; victory restores the zone.
    ///
    /// Why: scenes were "boring — nothing happens". This makes something happen by
    /// second 4. Self-contained, no external designer wiring required.
    /// </summary>
    [DisallowMultipleComponent]
    public class EchohavenCombatArena : MonoBehaviour
    {
        public float startDelay = 4f;
        public float interWaveDelay = 4f;
        public int[] waveSizes = { 3, 5, 7 };
        public float spawnRadius = 9f;

        readonly List<MudGolemHealth> _alive = new();
        int _wave;
        int _killsThisWave;
        Coroutine _runner;

        void OnEnable()
        {
            MudGolemHealth.OnAnyGolemDied += OnGolemDied;
        }

        void OnDisable()
        {
            MudGolemHealth.OnAnyGolemDied -= OnGolemDied;
        }

        void Start()
        {
            _runner = StartCoroutine(Run());
        }

        IEnumerator Run()
        {
            // Wait for player + spawner to be alive
            float waited = 0f;
            while ((GameObject.FindWithTag("Player") == null
                    || EchohavenContentSpawner.Instance == null) && waited < 20f)
            {
                waited += 0.5f;
                yield return new WaitForSeconds(0.5f);
            }
            yield return new WaitForSeconds(startDelay);

            ShowBanner("DEFEND ECHOHAVEN", "Press F to fire Harmonic Strike. Kill the corrupted golems.");

            for (_wave = 0; _wave < waveSizes.Length; _wave++)
            {
                _killsThisWave = 0;
                _alive.Clear();
                int size = waveSizes[_wave];

                yield return new WaitForSeconds(2f);
                SpawnWave(size);
                ShowObjective($"WAVE {_wave + 1} / {waveSizes.Length}  —  Defeat {size} Corrupted Golems");

                // Wait until all dead OR safety timeout (90s per wave)
                float t = 0f;
                while (CountAlive() > 0 && t < 90f)
                {
                    int killed = size - CountAlive();
                    if (killed != _killsThisWave)
                    {
                        _killsThisWave = killed;
                        ShowObjective($"WAVE {_wave + 1} / {waveSizes.Length}  —  {killed} / {size} purged");
                    }
                    t += Time.deltaTime;
                    yield return null;
                }

                if (_wave < waveSizes.Length - 1)
                {
                    ShowBanner($"WAVE {_wave + 1} CLEARED",
                               $"Wave {_wave + 2} incoming...");
                    yield return new WaitForSeconds(interWaveDelay);
                }
            }

            ShowBanner("ECHOHAVEN RESTORED", "The corruption is purged. The Star Dome sings.");
            ShowObjective("Victory — explore the moons (the world remembers your harmonic).");

            // Big VFX + RS bonus + audio
            var p = GameObject.FindWithTag("Player");
            if (p != null)
            {
                VFXController.Instance?.PlayHarmonicStrike(p.transform.position, Vector3.up);
                VFXController.Instance?.PlayHarmonicStrike(p.transform.position + Vector3.right * 3f, Vector3.up);
                VFXController.Instance?.PlayHarmonicStrike(p.transform.position + Vector3.left  * 3f, Vector3.up);
            }
            GameLoopController.Instance?.QueueRSReward(50f, "echohaven_arena_clear");
            AudioManager.Instance?.PlaySFX("BuildingRestore", p != null ? p.transform.position : Vector3.zero);
        }

        void SpawnWave(int count)
        {
            var spawner = EchohavenContentSpawner.Instance;
            if (spawner == null) return;

            var p = GameObject.FindWithTag("Player");
            Vector3 center = p != null ? p.transform.position : Vector3.zero;

            for (int i = 0; i < count; i++)
            {
                float a = (Mathf.PI * 2f / count) * i + Random.Range(-0.2f, 0.2f);
                Vector3 offset = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * spawnRadius;
                spawner.SpawnMudGolem(center + offset);
            }

            // Index newly spawned golems
            _alive.Clear();
            var all = GameObject.FindObjectsByType<MudGolemHealth>(FindObjectsSortMode.None);
            foreach (var g in all)
                if (g != null) _alive.Add(g);
        }

        int CountAlive()
        {
            _alive.RemoveAll(g => g == null);
            return _alive.Count;
        }

        void OnGolemDied(MudGolemHealth g)
        {
            _alive.Remove(g);
        }

        // ─── HUD helpers ─────────────────────────

        void ShowBanner(string title, string subtitle)
        {
            // Prefer HUDController objective text; fall back to large debug log so it's at least visible
            var hud = HUDController.Instance;
            if (hud != null)
            {
                hud.ShowObjective($"<b><size=140%>{title}</size></b>\n{subtitle}");
            }
            Debug.Log($"[ARENA] {title} — {subtitle}");
        }

        void ShowObjective(string text)
        {
            var hud = HUDController.Instance;
            if (hud != null) hud.ShowObjective(text);
        }
    }
}
