using System;

namespace Tartaria.Core.Enums
{
    /// <summary>
    /// Represents the lifecycle status of a quest.
    /// </summary>
    public enum QuestStatus : byte
    {
        /// <summary>
        /// Quest is not yet available to the player (prerequisites not met).
        /// </summary>
        Locked = 0,
        
        /// <summary>
        /// Quest is currently active and being pursued by the player.
        /// </summary>
        Active = 1,
        
        /// <summary>
        /// Quest has been successfully completed.
        /// </summary>
        Completed = 2,
        
        /// <summary>
        /// Quest has been failed (objectives not met, time expired, or critical failure).
        /// </summary>
        Failed = 3
    }

    /// <summary>
    /// Types of objectives that can be assigned to quests.
    /// </summary>
    public enum QuestObjectiveType : byte
    {
        /// <summary>
        /// Discover a specific building or location.
        /// </summary>
        DiscoverBuilding = 0,
        
        /// <summary>
        /// Restore a building to operational status.
        /// </summary>
        RestoreBuilding = 1,
        
        /// <summary>
        /// Defeat a specified number of enemies.
        /// </summary>
        DefeatEnemies = 2,
        
        /// <summary>
        /// Reach a target Resonance Score (RS) threshold.
        /// </summary>
        ReachRS = 3,
        
        /// <summary>
        /// Complete all objectives within a zone.
        /// </summary>
        CompleteZone = 4,
        
        /// <summary>
        /// Initiate dialogue with a specific NPC.
        /// </summary>
        TalkToNPC = 5,
        
        /// <summary>
        /// Collect a specified item or resource.
        /// </summary>
        CollectItem = 6,
        
        /// <summary>
        /// Complete a tuning fork puzzle or resonance calibration.
        /// </summary>
        CompleteTuning = 7,
        
        /// <summary>
        /// Successfully complete a mini-game challenge.
        /// </summary>
        CompleteMiniGame = 8,
        
        /// <summary>
        /// Defeat a boss or elite enemy.
        /// </summary>
        DefeatBoss = 9,
        
        /// <summary>
        /// Reach a companion relationship milestone.
        /// </summary>
        CompanionMilestone = 10,
        
        /// <summary>
        /// Craft a specific item using the crafting system.
        /// </summary>
        CraftItem = 11,
        
        /// <summary>
        /// Excavate and explore a ruin site.
        /// </summary>
        ExcavateRuin = 12,
        
        /// <summary>
        /// Navigate airship to a specific destination.
        /// </summary>
        ReachAirshipDestination = 13,
        
        /// <summary>
        /// Increase companion trust level to a target threshold.
        /// </summary>
        RaiseCompanionTrust = 14,
        
        /// <summary>
        /// Uncover a hidden discovery or secret location.
        /// </summary>
        HiddenDiscovery = 15,
        
        // Moon-specific objectives (13-Moon campaign arc)
        
        /// <summary>
        /// Moon 2: Purge dissonance crystals from affected areas.
        /// </summary>
        PurgeCrystals = 20,
        
        /// <summary>
        /// Moon 3: Escort and free orphans from the orphan train system.
        /// </summary>
        FreeOrphans = 21,
        
        /// <summary>
        /// Moon 4: Align the bastions of a star fort network.
        /// </summary>
        AlignBastions = 22,
        
        /// <summary>
        /// Moon 6: Activate the hydraulic fountain network infrastructure.
        /// </summary>
        ActivateFountains = 23,
        
        /// <summary>
        /// Moon 7: Multi-session thawing of Korath frozen zone.
        /// </summary>
        ThawKorath = 24,
        
        /// <summary>
        /// Moon 8: Tune airship mercury-orb propulsion systems.
        /// </summary>
        TuneAirships = 25,
        
        /// <summary>
        /// Moon 9: Collect all six prophecy stones from hidden locations.
        /// </summary>
        CollectProphecyStones = 26,
    }
}
