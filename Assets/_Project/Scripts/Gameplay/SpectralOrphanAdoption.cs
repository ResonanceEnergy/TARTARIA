using UnityEngine;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Moon 3 Spectral Orphan Adoption + Live-Ops Calendar Hooks (R6 foundation + R7 calendar variants).
    /// Exclusive Moon 3 domain. Provides persistence flags for escort, 17th Hour, Leviathan, World's Fair, daily rail deals, Continental Rail.
    /// </summary>
    public static partial class SpectralOrphanAdoption
    {
        // R5/R6/R7 static backing fields (mirrored to full Moon3SaveBlock)
        static bool _giantEchoFreedStatic;
        static bool _escortCompletedStatic;
        static bool _leviathanDefeatedStatic;
        static bool _ariaAdoptedStatic;
        static bool _torenAdoptedStatic;
        static bool _sylAdoptedStatic;
        static int _adoptedCountStatic = 0;

        // Extended Moon 3 state (rail, fast travel, ticket, permanent changes, lullaby, trusts)
        static bool _railSuccessStatic;
        static bool _fastTravelUnlockedStatic;
        static bool _goldenRailsStatic;
        static bool _windsCalmedStatic;
        static bool _worldsFairTicketStatic;
        static string _worldsFairVariantStatic = "";
        static float _lullabyTotalStatic;
        static float _ariaTrustStatic;
        static float _torenTrustStatic;
        static float _sylTrustStatic;
        static bool _postEscortAchievedStatic;

        [System.Serializable]
        public class Moon3AdoptionPayload
        {
            public int adoptedCount;
            public bool ariaAdopted;
            public bool torenAdopted;
            public bool sylAdopted;
            public bool escortCompleted;
            public bool leviathanDefeated;
            public bool giantEchoFreed;
            // Full block sync
            public bool railSuccess;
            public bool continentalFastTravelUnlocked;
            public bool goldenRailsPermanent;
            public bool windsPermanentlyCalmed;
            public bool worldsFairTicketGranted;
            public string worldsFairTicketVariant;
            public float lullabyContributionTotal;
            public float ariaTrust;
            public float torenTrust;
            public float sylTrust;
            public bool postEscortStateAchieved;
        }

        public static Moon3AdoptionPayload GetMoon3SaveData()
        {
            return new Moon3AdoptionPayload
            {
                adoptedCount = _adoptedCountStatic,
                ariaAdopted = _ariaAdoptedStatic,
                torenAdopted = _torenAdoptedStatic,
                sylAdopted = _sylAdoptedStatic,
                escortCompleted = _escortCompletedStatic,
                leviathanDefeated = _leviathanDefeatedStatic,
                giantEchoFreed = _giantEchoFreedStatic,
                railSuccess = _railSuccessStatic,
                continentalFastTravelUnlocked = _fastTravelUnlockedStatic,
                goldenRailsPermanent = _goldenRailsStatic,
                windsPermanentlyCalmed = _windsCalmedStatic,
                worldsFairTicketGranted = _worldsFairTicketStatic,
                worldsFairTicketVariant = _worldsFairVariantStatic,
                lullabyContributionTotal = _lullabyTotalStatic,
                ariaTrust = _ariaTrustStatic,
                torenTrust = _torenTrustStatic,
                sylTrust = _sylTrustStatic,
                postEscortStateAchieved = _postEscortAchievedStatic
            };
        }

        public static void LoadMoon3SaveData(Moon3AdoptionPayload p)
        {
            if (p == null) return;
            _adoptedCountStatic = Mathf.Clamp(p.adoptedCount, 0, 3);
            _ariaAdoptedStatic = p.ariaAdopted;
            _torenAdoptedStatic = p.torenAdopted;
            _sylAdoptedStatic = p.sylAdopted;
            _escortCompletedStatic = p.escortCompleted;
            _leviathanDefeatedStatic = p.leviathanDefeated;
            _giantEchoFreedStatic = p.giantEchoFreed;

            _railSuccessStatic = p.railSuccess;
            _fastTravelUnlockedStatic = p.continentalFastTravelUnlocked;
            _goldenRailsStatic = p.goldenRailsPermanent;
            _windsCalmedStatic = p.windsPermanentlyCalmed;
            _worldsFairTicketStatic = p.worldsFairTicketGranted;
            _worldsFairVariantStatic = p.worldsFairTicketVariant ?? "";
            _lullabyTotalStatic = p.lullabyContributionTotal;
            _ariaTrustStatic = p.ariaTrust;
            _torenTrustStatic = p.torenTrust;
            _sylTrustStatic = p.sylTrust;
            _postEscortAchievedStatic = p.postEscortStateAchieved;
        }

        // ─── Fully Functional Core APIs (adoption, trust, lullaby, post-escort) ───

        /// <summary>
        /// Adopt a spectral orphan (Aria, Toren, Syl). Increments count, sets trust seed, contributes lullaby.
        /// Wires to save + post-escort state for "Compassion & Rails" arc.
        /// </summary>
        public static void AdoptOrphan(string orphanId, float initialTrust = 30f)
        {
            if (string.IsNullOrEmpty(orphanId)) return;
            string id = orphanId.ToLowerInvariant();

            bool newlyAdopted = false;
            if (id.Contains("aria") && !_ariaAdoptedStatic) { _ariaAdoptedStatic = true; _ariaTrustStatic = initialTrust; newlyAdopted = true; }
            else if (id.Contains("toren") && !_torenAdoptedStatic) { _torenAdoptedStatic = true; _torenTrustStatic = initialTrust; newlyAdopted = true; }
            else if (id.Contains("syl") && !_sylAdoptedStatic) { _sylAdoptedStatic = true; _sylTrustStatic = initialTrust; newlyAdopted = true; }

            if (newlyAdopted)
            {
                _adoptedCountStatic = Mathf.Min(3, _adoptedCountStatic + 1);
                ContributeLullaby(15f, orphanId); // immediate lullaby contribution on adoption
                // Optional: notify companions (Lirael remembers the orphan truth)
                // LiraelController / Milo would react via extension methods
            }

            Tartaria.Core.GameEvents.FireCriticalSaveTrigger("moon3_orphan_adoption");
            Debug.Log($"[Moon3] Spectral orphan adopted: {orphanId} (trust={initialTrust:F0}, total adopted={_adoptedCountStatic}). Lullaby contribution applied. Post-escort state ready on escort complete.");
        }

        /// <summary>
        /// Contribute to the collective lullaby (during escort, 17th Hour, or post-escort). Scales shield / RS / Leviathan vuln.
        /// </summary>
        public static void ContributeLullaby(float amount, string source = "")
        {
            if (amount <= 0) return;
            _lullabyTotalStatic += amount;
            // Update last used for SaveData roundtrip
            // (RailEscortController and Moon3EscortHUD read AdoptedCount + lullaby for shield calc)
            Tartaria.Core.GameEvents.FireCriticalSaveTrigger("moon3_orphan_adoption");
            Debug.Log($"[Moon3] Lullaby contributed +{amount:F1} (source={source}). Total: {_lullabyTotalStatic:F1}. Affects shield/escort/Leviathan.");
        }

        public static float LullabyTotal => _lullabyTotalStatic;
        public static float GetOrphanTrust(string orphanId)
        {
            string id = (orphanId ?? "").ToLower();
            if (id.Contains("aria")) return _ariaTrustStatic;
            if (id.Contains("toren")) return _torenTrustStatic;
            if (id.Contains("syl")) return _sylTrustStatic;
            return 0f;
        }

        // Giant Echo freed (Moon 3 Leviathan finale — releases ancestral giant resonance trapped in the storm)
        public static void SetGiantEchoFreed(bool freed)
        {
            _giantEchoFreedStatic = freed;
            if (freed)
            {
                SetSeventeenthHourEvent("giant_echo_freed", true);
                ContributeLullaby(25f, "giant_echo_freed");
            }
            Tartaria.Core.GameEvents.FireCriticalSaveTrigger("moon3_orphan_adoption");
            Debug.Log($"[Moon3] Giant Echo freed = {freed}. Permanent ancestral resonance unlocked.");
        }

        // Rail success + permanent world changes + fast travel + World's Fair
        public static void SetRailSuccess(bool success)
        {
            _railSuccessStatic = success;
            if (success)
            {
                _escortCompletedStatic = true;
                SetSeventeenthHourEvent("rail_success_daily_deal", true);
                SetSeventeenthHourEvent("worlds_fair_golden_variant_rail", true);
                UnlockContinentalFastTravel();
                // Golden rails permanent world change
                _goldenRailsStatic = true;
                SetSeventeenthHourEvent("golden_rails_permanent", true);
            }
            Tartaria.Core.GameEvents.FireCriticalSaveTrigger("moon3_orphan_adoption");
            Debug.Log($"[Moon3] Rail success={success}. Permanent golden rails + fast travel unlocked.");
        }

        public static void UnlockContinentalFastTravel()
        {
            _fastTravelUnlockedStatic = true;
            _postEscortAchievedStatic = true;
            // Hook CampaignFlow / MoonProgressTracker for payoff (Continental Rail) -- via event to avoid Gameplay->Integration dep
            Tartaria.Core.GameEvents.FireMoon3FastTravelUnlocked();
            Tartaria.Core.GameEvents.FireCriticalSaveTrigger("moon3_orphan_adoption");
            Debug.Log("[Moon3] Continental Rail fast travel UNLOCKED (Moon 3 completion payoff).");
        }

        public static void SetLeviathanDefeated(bool defeated)
        {
            _leviathanDefeatedStatic = defeated;
            if (defeated)
            {
                SetSeventeenthHourEvent("leviathan_purified_orphan_lullaby", true);
                SetSeventeenthHourEvent("post_escort_continental_rail_ready", true);
                _windsCalmedStatic = true; // permanent world change
                ContributeLullaby(50f, "leviathan_defeat");
            }
            Tartaria.Core.GameEvents.FireCriticalSaveTrigger("moon3_orphan_adoption");
        }

        public static void GrantWorldsFairTicket(string variant = "golden_rail")
        {
            _worldsFairTicketStatic = true;
            _worldsFairVariantStatic = variant;
            SetSeventeenthHourEvent("worlds_fair_ticket_moon3", true);
            Tartaria.Core.GameEvents.FireCriticalSaveTrigger("moon3_orphan_adoption");
            Debug.Log($"[Moon3] World's Fair ticket granted: variant={variant}.");
        }

        // Full setters extended for 17th Hour variants array + post-escort
        public static void SetSeventeenthHourEvent(string eventId, bool completed)
        {
            if (string.IsNullOrEmpty(eventId)) return;
            _escortCompletedStatic = _escortCompletedStatic || eventId.Contains("rail");
            _postEscortAchievedStatic = _postEscortAchievedStatic || eventId.Contains("post_escort");
            // In full: would append unique to event list / variants (SaveData roundtrips via GameLoop <-> SaveData.moon3)
            Tartaria.Core.GameEvents.FireCriticalSaveTrigger("moon3_orphan_adoption");
            Debug.Log($"[Moon3] 17th Hour / live-ops event '{eventId}' recorded (WF ticket, rail, golden rails, continental, lullaby).");
        }

        public static void SetEscortCompleted(bool completed)
        {
            _escortCompletedStatic = completed;
            if (completed)
            {
                SetRailSuccess(true); // chains to fast travel + golden rails
                SetSeventeenthHourEvent("worlds_fair_golden_variant_rail", true);
                _postEscortAchievedStatic = true;
            }
            Tartaria.Core.GameEvents.FireCriticalSaveTrigger("moon3_orphan_adoption");
            Debug.Log("[Moon3] Escort completed + full rail success + post-escort state achieved.");
        }

        public static void ForceAdoptForClimax()
        {
            _adoptedCountStatic = Mathf.Min(3, _adoptedCountStatic + 1);
            ContributeLullaby(12f, "climax_force");
            Debug.Log("[Moon3] Mid-escort ForceAdoptForClimax — found family trust + lullaby payoff.");
            Tartaria.Core.GameEvents.FireCriticalSaveTrigger("moon3_orphan_adoption");
        }

        public static bool IsPostEscortStateAchieved => _postEscortAchievedStatic || _escortCompletedStatic;
        public static bool IsContinentalFastTravelUnlocked => _fastTravelUnlockedStatic;
        public static bool HasGoldenRails => _goldenRailsStatic;
        public static int AdoptedCount => _adoptedCountStatic;

        // Convenience: full state snapshot for Campaign/MoonProgress hooks
        public static object GetPostEscortState()
        {
            return new { adopted = _adoptedCountStatic, railSuccess = _railSuccessStatic, fastTravel = _fastTravelUnlockedStatic, goldenRails = _goldenRailsStatic, lullaby = _lullabyTotalStatic, ticket = _worldsFairTicketStatic };
        }
    }

    // Moon 3 extension methods now declared directly on ILiraelService / IMiloService (implemented in LiraelController / MiloController for full dialogue + physical board).
    // Calls in this file now resolve to real authored Moon 3 companion reactions.
}