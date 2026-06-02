using System;
using System.Collections;
using UnityEngine;

namespace Tartaria.UI
{
    /// <summary>
    /// In-world lore page pickup. Attach to any GameObject with a trigger Collider.
    /// On OnTriggerEnter by a Player-tagged collider:
    ///   1) Pushes its (id, title, body) into <see cref="LorebookPanel.Instance"/>.AddEntry
    ///   2) Plays a 1-second fade-out then self-destroys.
    ///
    /// LorebookPanel.AddEntry is idempotent by id, so re-walking through a previously
    /// collected (but un-destroyed in a different scene reload) collectible is safe — the
    /// second call returns false and no banner fires.
    ///
    /// Per NO-DEBT rule 4: if <see cref="LorebookPanel.Instance"/> is null at trigger time
    /// (the panel bootstrap hasn't completed yet), we log a warning AND retry once on the
    /// next frame instead of silently failing. The Player's footprint is usually only
    /// inside the trigger for a few frames, so a one-frame retry is sufficient.
    /// </summary>
    [DisallowMultipleComponent]
    public class LorebookCollectible : MonoBehaviour
    {
        [Tooltip("Stable id used to dedupe in the lorebook + key PlayerPrefs persistence. REQUIRED.")]
        [SerializeField] private string entryId;

        [Tooltip("Display title for the banner toast and lorebook list row.")]
        [SerializeField] private string title;

        [Tooltip("Full body text shown in the lorebook reader pane.")]
        [TextArea(3, 12)]
        [SerializeField] private string body;

        [Tooltip("Tag the trigger collider checks. Defaults to 'Player' which is the canonical Echohaven player tag.")]
        [SerializeField] private string playerTag = "Player";

        [Tooltip("Seconds the visual takes to fade after pickup before the GameObject self-destructs.")]
        [SerializeField] private float fadeSeconds = 1f;

        bool _collected;

        void Awake()
        {
            // Per NO-DEBT rule 4: surface mis-configuration loudly.
            if (string.IsNullOrEmpty(entryId))
            {
                Debug.LogWarning($"[LorebookCollectible] entryId is empty on '{GetHierarchyPath(gameObject)}'. This collectible will be ignored on pickup. Assign 'entryId' in the Inspector.");
            }
            if (string.IsNullOrEmpty(title))
            {
                Debug.LogWarning($"[LorebookCollectible] title is empty on '{GetHierarchyPath(gameObject)}'. The lorebook list will show the entryId '{entryId}' as a placeholder.");
            }

            // Verify there's at least one trigger collider so OnTriggerEnter has any chance of firing.
            var cols = GetComponents<Collider>();
            bool anyTrigger = false;
            for (int i = 0; i < cols.Length; i++)
            {
                if (cols[i] != null && cols[i].isTrigger) { anyTrigger = true; break; }
            }
            if (!anyTrigger)
            {
                Debug.LogWarning($"[LorebookCollectible] No trigger Collider found on '{GetHierarchyPath(gameObject)}' (found {cols.Length} non-trigger collider(s)). OnTriggerEnter will never fire. Add a Collider component with 'Is Trigger' enabled, or it will never be picked up.");
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (_collected) return;
            if (other == null) return;

            // Tag check. CompareTag throws if the tag isn't registered in TagManager — wrap defensively.
            bool isPlayer;
            try
            {
                isPlayer = other.CompareTag(playerTag);
            }
            catch (UnityException ex)
            {
                Debug.LogWarning($"[LorebookCollectible] CompareTag('{playerTag}') threw on '{GetHierarchyPath(gameObject)}' (collider '{other.name}'): {ex.Message}. Add the tag in Project Settings -> Tags & Layers, or change 'playerTag' on this component.");
                return;
            }
            if (!isPlayer) return;

            // Guard against empty id even if the warning fired in Awake.
            if (string.IsNullOrEmpty(entryId))
            {
                Debug.LogWarning($"[LorebookCollectible] OnTriggerEnter on '{GetHierarchyPath(gameObject)}' with empty entryId — skipping AddEntry. Fix the prefab/scene reference.");
                return;
            }

            TryDeliver();
        }

        void TryDeliver()
        {
            var panel = LorebookPanel.Instance;
            if (panel == null)
            {
                // Per NO-DEBT rule 4: log loud, then retry once after a frame. The
                // LorebookPanel bootstrap runs at AfterSceneLoad; a collectible whose
                // OnTriggerEnter fires at frame 0 may beat the singleton. Retrying once
                // covers that race; a second null after the retry is a real failure.
                Debug.LogWarning($"[LorebookCollectible] LorebookPanel.Instance is null at pickup on '{GetHierarchyPath(gameObject)}' (entryId='{entryId}'). Retrying after one frame — if this repeats, the LorebookPanel bootstrap is not running.");
                StartCoroutine(RetryDeliverNextFrame());
                return;
            }

            Deliver(panel);
        }

        IEnumerator RetryDeliverNextFrame()
        {
            yield return null; // wait one frame

            var panel = LorebookPanel.Instance;
            if (panel == null)
            {
                // Second miss is a real failure — log error per NO-DEBT rule 3. The
                // collectible stays in the scene so the player can re-enter the trigger
                // after the panel comes up (e.g. they reloaded a save).
                Debug.LogError($"[LorebookCollectible] LorebookPanel.Instance is STILL null one frame after pickup on '{GetHierarchyPath(gameObject)}' (entryId='{entryId}'). Bootstrap may have failed — check for earlier '[LorebookPanel]' errors.");
                yield break;
            }

            Deliver(panel);
        }

        void Deliver(LorebookPanel panel)
        {
            bool isNew;
            try
            {
                isNew = panel.AddEntry(entryId, title, body, Time.realtimeSinceStartup);
            }
            catch (Exception ex)
            {
                // Log loud per NO-DEBT rule 3. Don't fade/destroy — leaving the
                // collectible in the world means the player can retry. Re-raising would
                // crash the trigger callback and break PhysX state, so we swallow with
                // a documented rationale.
                Debug.LogError($"[LorebookCollectible] LorebookPanel.AddEntry threw on '{GetHierarchyPath(gameObject)}' (entryId='{entryId}'): {ex.GetType().Name}: {ex.Message}. Collectible left in-world so player can retry.");
                return;
            }

            _collected = true;

            if (!isNew)
            {
                // Already collected previously. Skip the banner (panel already skipped it)
                // and fade out anyway so the world doesn't keep showing a picked-up page.
                Debug.Log($"[LorebookCollectible] Lore entry '{entryId}' was already in the lorebook (likely restored from PlayerPrefs). Fading collectible silently.");
            }

            StartCoroutine(FadeAndDestroy());
        }

        IEnumerator FadeAndDestroy()
        {
            // Disable colliders so the player can walk through during the fade.
            var cols = GetComponents<Collider>();
            for (int i = 0; i < cols.Length; i++)
            {
                if (cols[i] != null) cols[i].enabled = false;
            }
            var colsChild = GetComponentsInChildren<Collider>(includeInactive: false);
            for (int i = 0; i < colsChild.Length; i++)
            {
                if (colsChild[i] != null) colsChild[i].enabled = false;
            }

            // Gather all renderers + their material instances. Cache start colors per
            // (renderer index, material index) so the fade is deterministic.
            var renderers = GetComponentsInChildren<Renderer>(includeInactive: false);
            var startColors = new Color[renderers.Length][];
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null) { startColors[i] = Array.Empty<Color>(); continue; }
                // Reading .materials instantiates the mats (so we don't mutate shared assets).
                var mats = renderers[i].materials;
                startColors[i] = new Color[mats.Length];
                for (int m = 0; m < mats.Length; m++)
                {
                    if (mats[m] == null) { startColors[i][m] = Color.white; continue; }
                    // URP/Lit uses _BaseColor; legacy uses _Color. Probe in order.
                    if (mats[m].HasProperty("_BaseColor"))
                    {
                        startColors[i][m] = mats[m].GetColor("_BaseColor");
                    }
                    else if (mats[m].HasProperty("_Color"))
                    {
                        startColors[i][m] = mats[m].GetColor("_Color");
                    }
                    else
                    {
                        startColors[i][m] = Color.white;
                    }
                }
            }

            float t = 0f;
            float duration = Mathf.Max(0.05f, fadeSeconds);
            while (t < duration)
            {
                t += Time.deltaTime;
                float a = Mathf.Clamp01(1f - (t / duration));
                for (int i = 0; i < renderers.Length; i++)
                {
                    if (renderers[i] == null) continue;
                    var mats = renderers[i].materials;
                    for (int m = 0; m < mats.Length; m++)
                    {
                        if (mats[m] == null) continue;
                        var c = startColors[i][m];
                        c.a = c.a * a;
                        if (mats[m].HasProperty("_BaseColor"))
                        {
                            mats[m].SetColor("_BaseColor", c);
                        }
                        else if (mats[m].HasProperty("_Color"))
                        {
                            mats[m].SetColor("_Color", c);
                        }
                    }
                }
                yield return null;
            }

            Destroy(gameObject);
        }

        // ─────────────── Public configuration (for procedural spawns) ───────────────

        /// <summary>
        /// Procedural setup for spawners that instantiate this component at runtime.
        /// Call this BEFORE the player can collide with the trigger.
        /// </summary>
        public void Configure(string id, string displayTitle, string bodyText)
        {
            entryId = id;
            title = displayTitle;
            body = bodyText;
        }

        // ─────────────── Utility ───────────────

        static string GetHierarchyPath(GameObject go)
        {
            if (go == null) return "<null>";
            var t = go.transform;
            var path = go.name;
            while (t.parent != null)
            {
                t = t.parent;
                path = t.gameObject.name + "/" + path;
            }
            return path;
        }
    }
}
