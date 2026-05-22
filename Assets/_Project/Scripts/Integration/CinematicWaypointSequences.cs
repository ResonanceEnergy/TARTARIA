using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
    /// <summary>
    /// Cinematic Waypoint Sequences — defines camera paths for Moon intro/outro scenes.
    /// Used by CinematicCameraController to play scripted camera movements.
    /// 
    /// Usage:
    /// 1. Call DefineSequence() with sequence name and waypoints array
    /// 2. Pass to CinematicCameraController.PlaySequence(name)
    /// 3. Camera interpolates through waypoints with configurable speed/easing
    /// 
    /// Example sequences:
    /// - moon2_intro: Pan over crystal caverns, reveal Cassian at cathedral
    /// - moon4_golem_reveal: Circle around 30-foot mud golem boss
    /// - moon13_ending: Pull back to show restored Tartarian cityscape
    /// </summary>
    public static class CinematicWaypointSequences
    {
        public static Dictionary<string, CinematicWaypoint[]> Sequences = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Initialize()
        {
            DefineMoon2Intro();
            DefineMoon4GolemReveal();
            DefineMoon10RailFlyby();
            DefineMoon13Ending();
        }

        static void DefineMoon2Intro()
        {
            var waypoints = new CinematicWaypoint[]
            {
                new() { position = new Vector3(0f, 25f, -30f), lookAt = new Vector3(0f, 0f, 0f), duration = 2f },  // High establishing shot
                new() { position = new Vector3(-10f, 15f, -10f), lookAt = new Vector3(0f, 2f, 10f), duration = 3f },  // Pan down to cathedral
                new() { position = new Vector3(15f, 3f, 20f), lookAt = new Vector3(15f, 1.8f, 20f), duration = 2f },  // Close-up Cassian
                new() { position = new Vector3(5f, 10f, 5f), lookAt = new Vector3(0f, 0f, 0f), duration = 2f }  // Pull back to overview
            };

            Sequences["moon2_intro"] = waypoints;
            Debug.Log("[Cinematics] Defined Moon2 intro sequence (4 waypoints, 9s total)");
        }

        static void DefineMoon4GolemReveal()
        {
            var waypoints = new CinematicWaypoint[]
            {
                new() { position = new Vector3(0f, 2f, -15f), lookAt = new Vector3(0f, 10f, 0f), duration = 2f },  // Ground level, look up
                new() { position = new Vector3(10f, 8f, -10f), lookAt = new Vector3(0f, 10f, 0f), duration = 3f },  // Circle right
                new() { position = new Vector3(10f, 8f, 10f), lookAt = new Vector3(0f, 10f, 0f), duration = 3f },  // Continue circle
                new() { position = new Vector3(-10f, 8f, 0f), lookAt = new Vector3(0f, 10f, 0f), duration = 3f },  // Complete circle
                new() { position = new Vector3(0f, 15f, -20f), lookAt = new Vector3(0f, 10f, 0f), duration = 2f }  // High dramatic angle
            };

            Sequences["moon4_golem_reveal"] = waypoints;
            Debug.Log("[Cinematics] Defined Moon4 golem reveal sequence (5 waypoints, 13s total)");
        }

        static void DefineMoon10RailFlyby()
        {
            var waypoints = new CinematicWaypoint[]
            {
                new() { position = new Vector3(-100f, 20f, 0f), lookAt = new Vector3(0f, 0f, 0f), duration = 1f },  // Far left along rail
                new() { position = new Vector3(-50f, 15f, 0f), lookAt = new Vector3(50f, 0f, 0f), duration = 2f },  // Approach station
                new() { position = new Vector3(0f, 12f, -10f), lookAt = new Vector3(0f, 5f, 0f), duration = 3f },  // Above mega-station
                new() { position = new Vector3(50f, 15f, 0f), lookAt = new Vector3(-50f, 0f, 0f), duration = 2f },  // Exit rail right
                new() { position = new Vector3(100f, 20f, 0f), lookAt = new Vector3(0f, 0f, 0f), duration = 1f }  // Far right lookback
            };

            Sequences["moon10_rail_flyby"] = waypoints;
            Debug.Log("[Cinematics] Defined Moon10 rail flyby sequence (5 waypoints, 9s total)");
        }

        static void DefineMoon13Ending()
        {
            var waypoints = new CinematicWaypoint[]
            {
                new() { position = new Vector3(0f, 5f, -10f), lookAt = new Vector3(0f, 2f, 0f), duration = 2f },  // Ground level, player POV
                new() { position = new Vector3(0f, 20f, -20f), lookAt = new Vector3(0f, 0f, 0f), duration = 4f },  // Rise and pull back
                new() { position = new Vector3(-30f, 40f, -30f), lookAt = new Vector3(0f, 0f, 0f), duration = 4f },  // Higher, reveal city
                new() { position = new Vector3(0f, 80f, -50f), lookAt = new Vector3(0f, 0f, 0f), duration = 5f },  // Epic aerial view
                new() { position = new Vector3(0f, 120f, 0f), lookAt = new Vector3(0f, 0f, 0f), duration = 3f }  // Overhead fade to credits
            };

            Sequences["moon13_ending"] = waypoints;
            Debug.Log("[Cinematics] Defined Moon13 ending sequence (5 waypoints, 18s total)");
        }
    }

    /// <summary>
    /// Single cinematic waypoint — position, look target, duration to reach.
    /// </summary>
    public struct CinematicWaypoint
    {
        public Vector3 position;
        public Vector3 lookAt;
        public float duration;  // Time to interpolate from previous waypoint (seconds)
    }
}
