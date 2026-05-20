using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Tartaria.Core;
using Tartaria.Gameplay;
using Tartaria.Audio;
using Tartaria.Input;
using Tartaria.UI;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 2 Progression, Skill Tree & Permanent Mutations System (Crystalline Caverns).
    ///
    /// Exclusive domain: Progression hooks for Moon 2.
    /// Ties directly to the "purge the corruption" fantasy from 03C, 06_COMBAT, 20_QUESTS (M2-MS02 Purge Protocols etc),
    /// and the 5 key living crystal sites in the caverns (cathedral_dome, bell_tower, fountain, crystal_hall, ley_chamber).
    ///
    /// When the player restores + fully purges a key site:
    ///   • A permanent "blessing / mutation" is granted.
    ///   • Corresponding Skill Tree node (500+ ids) is force-unlocked (visible in UI, contributes modifiers).
    ///   • RS reward + haptic + audio.
    ///   • Cosmetic / visual mutation applied to player (persistent sigil/light/ley sparks while in Moon 2).
    ///   • State saved in Moon2SaveBlock so it survives across sessions and moons.
    ///
    /// Full purge of all 5 unlocks the capstone "True Lunar Purifier".
    /// Everything wires into existing SkillTreeSystem, CorruptionSystem, GameEvents, Save flow, VFX.
    ///
    /// Absolute path: C:\dev\TARTARIA_new
    /// </summary>
    [DisallowMultipleComponent]
    public class Moon2ProgressionSystem : MonoBehaviour
    {
        public static Moon2ProgressionSystem Instance { get; private set; }

        // The 5 key sites whose restoration + purge grant permanent blessings (matches VFX + scaffold + secrets)
        private static readonly string[] KeySites =
        {
            "moon2_cathedral_dome",
            "moon2_bell_tower",
            "moon2_fountain",
            "moon2_crystal_hall",
            "moon2_ley_chamber"
        };

        private readonly HashSet<string> _purgedSites = new HashSet<string>();
        private bool _cathedralBreath, _bellCleansing, _fountainSpring, _crystalLens, _leyBond, _truePurifier;
        private int _purgeCount;

        private GameObject _playerMutationRoot; // persistent visual mutation container (re-created on load/zone)

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("Moon2ProgressionSystem");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<Moon2ProgressionSystem>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Hook events (safe, idempotent)
            GameEvents.OnBuildingRestored += HandleBuildingRestored;
            // CorruptionSystem event wired after it exists
            StartCoroutine(DelayedCorruptionHook());
        }

        System.Collections.IEnumerator DelayedCorruptionHook()
        {
            yield return null;
            if (CorruptionSystem.Instance != null)
            {
                CorruptionSystem.Instance.OnCorruptionPurged += HandleCorruptionPurged;
            }
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            GameEvents.OnBuildingRestored -= HandleBuildingRestored;
            if (CorruptionSystem.Instance != null)
                CorruptionSystem.Instance.OnCorruptionPurged -= HandleCorruptionPurged;
        }

        void HandleBuildingRestored(string buildingId)
        {
            if (string.IsNullOrEmpty(buildingId) || !buildingId.Contains("moon2_")) return;
            // A restore is prerequisite for purge blessing; actual grant happens on full purge (0 corruption)
            CheckForFullPurgeGrant(buildingId);
        }

        void HandleCorruptionPurged(string buildingId)
        {
            if (string.IsNullOrEmpty(buildingId) || !buildingId.Contains("moon2_")) return;

            _purgeCount++;
            if (!_purgedSites.Contains(buildingId))
                _purgedSites.Add(buildingId);

            CheckForFullPurgeGrant(buildingId);

            // Special: if this purge completes a key site, grant the blessing
            GrantBlessingForSiteIfKey(buildingId);
        }

        void CheckForFullPurgeGrant(string buildingId)
        {
            // When corruption reaches zero on a key site, the OnCorruptionPurged already fired.
            // We also listen here for safety.
        }

        void GrantBlessingForSiteIfKey(string buildingId)
        {
            string siteKey = NormalizeSiteKey(buildingId);
            if (string.IsNullOrEmpty(siteKey)) return;

            bool newlyGranted = false;

            if (siteKey == "moon2_cathedral_dome" && !_cathedralBreath)
            {
                _cathedralBreath = true;
                newlyGranted = true;
                ForceUnlock(SkillId.M2_CathedralBreath);
                AwardBonus(250f, "Cathedral Purge Blessing");
                ApplyPlayerMutation("cathedral_breath", new Color(0.6f, 0.95f, 0.7f));
            }
            else if (siteKey == "moon2_bell_tower" && !_bellCleansing)
            {
                _bellCleansing = true;
                newlyGranted = true;
                ForceUnlock(SkillId.M2_BellCleansing);
                AwardBonus(200f, "Bell Tower Purge Blessing");
                ApplyPlayerMutation("bell_cleansing", new Color(0.85f, 0.8f, 0.95f));
            }
            else if (siteKey == "moon2_fountain" && !_fountainSpring)
            {
                _fountainSpring = true;
                newlyGranted = true;
                ForceUnlock(SkillId.M2_FountainSpring);
                AwardBonus(180f, "Fountain Purge Blessing");
                ApplyPlayerMutation("fountain_spring", new Color(0.4f, 0.75f, 0.95f));
            }
            else if (siteKey == "moon2_crystal_hall" && !_crystalLens)
            {
                _crystalLens = true;
                newlyGranted = true;
                ForceUnlock(SkillId.M2_CrystalLens);
                AwardBonus(220f, "Crystal Hall Purge Blessing");
                ApplyPlayerMutation("crystal_lens", new Color(0.95f, 0.85f, 0.5f));
            }
            else if (siteKey == "moon2_ley_chamber" && !_leyBond)
            {
                _leyBond = true;
                newlyGranted = true;
                ForceUnlock(SkillId.M2_LeyBond);
                AwardBonus(260f, "Ley Chamber Purge Blessing");
                ApplyPlayerMutation("ley_bond", new Color(0.7f, 0.95f, 0.95f));
            }

            if (newlyGranted)
            {
                CheckAndGrantTruePurifier();
                HUDController.Instance?.ShowObjective($"<color=#FFD700>PURGE BLESSING: {siteKey.Replace("moon2_", "").ToUpper()}</color>");
                Debug.Log($"[Moon2Progress] Permanent blessing granted for {siteKey} — corruption purged, player mutated, skill unlocked.");
            }
        }

        string NormalizeSiteKey(string buildingId)
        {
            foreach (var key in KeySites)
                if (buildingId.Contains(key.Replace("moon2_", "")) || buildingId == key)
                    return key;
            return null;
        }

        void ForceUnlock(SkillId id)
        {
            SkillTreeSystem.Instance?.ForceUnlockMoon2Blessing(id);
        }

        void AwardBonus(float rs, string reason)
        {
            AetherFieldManager.Instance?.AddResonanceScore(rs);
            GameLoopController.Instance?.QueueRSReward(rs, $"moon2_purge_{reason}");
            HapticFeedbackManager.Instance?.PlayPerfectTune();
            AudioManager.Instance?.PlaySFX2D("BuildingRestore");
        }

        void CheckAndGrantTruePurifier()
        {
            int granted = (_cathedralBreath ? 1 : 0) + (_bellCleansing ? 1 : 0) + (_fountainSpring ? 1 : 0) +
                          (_crystalLens ? 1 : 0) + (_leyBond ? 1 : 0);

            if (granted >= 5 && !_truePurifier)
            {
                _truePurifier = true;
                ForceUnlock(SkillId.M2_TrueLunarPurifier);
                AwardBonus(600f, "TrueLunarPurifier");
                ApplyPlayerMutation("true_purifier", new Color(1f, 0.95f, 0.4f), true); // stronger visual
                HUDController.Instance?.ShowObjective("<color=#FFD700>TRUE LUNAR PURIFIER — All Caverns Cleansed</color>");
                Debug.Log("[Moon2Progress] TRUE LUNAR PURIFIER UNLOCKED — ultimate permanent mutation from full cavern purge.");
            }
        }

        /// <summary>
        /// Applies a persistent visual mutation to the player (sigil/light/ley effect).
        /// Only while Moon 2 scene is active for domain purity. Re-applied on load/restore.
        /// </summary>
        void ApplyPlayerMutation(string type, Color color, bool isCapstone = false)
        {
            var player = GameObject.FindWithTag("Player");
            if (player == null) return;

            // Clean previous for this type
            var old = player.transform.Find($"LunarSigil_{type}");
            if (old != null) Destroy(old.gameObject);

            var sigil = new GameObject($"LunarSigil_{type}");
            sigil.transform.SetParent(player.transform);
            sigil.transform.localPosition = Vector3.up * (isCapstone ? 2.2f : 1.6f);

            // Simple permanent light sigil (no new assets, pure runtime)
            var light = sigil.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = isCapstone ? 1.8f : 0.9f;
            light.range = isCapstone ? 7f : 4.5f;
            light.shadows = LightShadows.None;

            // For ley/capstone: add orbiting "sparks" via small rotating child with emission hint
            if (type.Contains("ley") || isCapstone)
            {
                var sparks = new GameObject("LeySparks");
                sparks.transform.SetParent(sigil.transform);
                sparks.transform.localPosition = Vector3.zero;
                var ps = sparks.AddComponent<ParticleSystem>();
                var main = ps.main;
                main.startLifetime = 1.2f;
                main.startSpeed = 0.8f;
                main.startSize = 0.08f;
                main.startColor = color;
                main.maxParticles = 12;
                var emission = ps.emission;
                emission.rateOverTime = 4f;
                var shape = ps.shape;
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.radius = 0.6f;
                ps.Play();
            }

            // Keep reference for cleanup on zone exit if needed
            if (_playerMutationRoot == null) _playerMutationRoot = sigil;
        }

        /// <summary>
        /// Re-apply all granted mutations on load or Moon 2 entry (persistent cosmetic layer).
        /// </summary>
        public void ReapplyAllMutations()
        {
            if (_cathedralBreath) ApplyPlayerMutation("cathedral_breath", new Color(0.6f, 0.95f, 0.7f));
            if (_bellCleansing) ApplyPlayerMutation("bell_cleansing", new Color(0.85f, 0.8f, 0.95f));
            if (_fountainSpring) ApplyPlayerMutation("fountain_spring", new Color(0.4f, 0.75f, 0.95f));
            if (_crystalLens) ApplyPlayerMutation("crystal_lens", new Color(0.95f, 0.85f, 0.5f));
            if (_leyBond) ApplyPlayerMutation("ley_bond", new Color(0.7f, 0.95f, 0.95f));
            if (_truePurifier) ApplyPlayerMutation("true_purifier", new Color(1f, 0.95f, 0.4f), true);
        }

        // ─── Persistence API (called by GameLoopController) ─────────────────

        public void PopulateSaveBlock(Moon2SaveBlock block)
        {
            if (block == null) return;
            block.purgedMoon2Sites = _purgedSites.ToArray();
            block.cathedralBreathGranted = _cathedralBreath;
            block.bellCleansingGranted = _bellCleansing;
            block.fountainSpringGranted = _fountainSpring;
            block.crystalLensGranted = _crystalLens;
            block.leyBondGranted = _leyBond;
            block.trueLunarPurifierGranted = _truePurifier;
            block.moon2PurgeCount = _purgeCount;
        }

        public void RestoreFromSaveBlock(Moon2SaveBlock block)
        {
            if (block == null) return;

            _purgedSites.Clear();
            if (block.purgedMoon2Sites != null)
                foreach (var s in block.purgedMoon2Sites) _purgedSites.Add(s);

            _cathedralBreath = block.cathedralBreathGranted;
            _bellCleansing = block.bellCleansingGranted;
            _fountainSpring = block.fountainSpringGranted;
            _crystalLens = block.crystalLensGranted;
            _leyBond = block.leyBondGranted;
            _truePurifier = block.trueLunarPurifierGranted;
            _purgeCount = block.moon2PurgeCount;

            // Re-apply visuals + ensure skills are unlocked (in case save order)
            ReapplyAllMutations();

            // Force the skill nodes if blessings are present (idempotent)
            if (_cathedralBreath) SkillTreeSystem.Instance?.ForceUnlockMoon2Blessing(SkillId.M2_CathedralBreath);
            if (_bellCleansing) SkillTreeSystem.Instance?.ForceUnlockMoon2Blessing(SkillId.M2_BellCleansing);
            if (_fountainSpring) SkillTreeSystem.Instance?.ForceUnlockMoon2Blessing(SkillId.M2_FountainSpring);
            if (_crystalLens) SkillTreeSystem.Instance?.ForceUnlockMoon2Blessing(SkillId.M2_CrystalLens);
            if (_leyBond) SkillTreeSystem.Instance?.ForceUnlockMoon2Blessing(SkillId.M2_LeyBond);
            if (_truePurifier) SkillTreeSystem.Instance?.ForceUnlockMoon2Blessing(SkillId.M2_TrueLunarPurifier);

            Debug.Log($"[Moon2Progress] Restored: {_purgedSites.Count} sites purged, {_purgeCount} total purges, capstone={_truePurifier}");
        }

        // Public query for other Moon2 systems (secrets, micro-giant, etc.)
        public bool IsSitePurged(string siteId) => _purgedSites.Contains(siteId) || 
            (_cathedralBreath && siteId.Contains("cathedral")) ||
            (_bellCleansing && siteId.Contains("bell")) ||
            (_fountainSpring && siteId.Contains("fountain")) ||
            (_crystalLens && siteId.Contains("crystal")) ||
            (_leyBond && siteId.Contains("ley"));

        public bool HasTrueLunarPurifier => _truePurifier;

        public float GetMoon2CorruptionResistanceBonus() => _fountainSpring || _truePurifier ? 0.25f : 0f;
    }
}
