using UnityEditor;
using UnityEngine;
using System.IO;

#if UNITY_TIMELINE
using UnityEngine.Playables;
using UnityEngine.Timeline;
#endif

namespace Tartaria.Editor
{
    /// <summary>
    /// Creates a Timeline for Moon-8 Armada flyover cinematic.
    /// Builds a PlayableDirector + TimelineAsset with:
    ///   - ActivationTrack toggling a transient ArmadaCamera GameObject
    ///   - AnimationTrack with a procedurally-built AnimationClip (200m fly on X, 12s)
    ///   - SignalTrack firing 3 SignalAssets (start, mid, end) wired to ClimaxSequenceSystem
    ///
    /// NOTE: Timeline package not available in this Unity build — method stubs to no-op.
    ///       Install com.unity.timeline package for full functionality.
    /// </summary>
    public static class ClimaxTimelineFactory
    {
        const string CinematicsDir = "Assets/_Project/Cinematics";
        const string TimelinePath = "Assets/_Project/Cinematics/Moon8_Armada.playable";
        const string ClipPath = "Assets/_Project/Cinematics/Moon8_ArmadaFly.anim";

        public static void Run()
        {
#if UNITY_TIMELINE
            if (!AssetDatabase.IsValidFolder(CinematicsDir))
            {
                Directory.CreateDirectory(Path.Combine(Application.dataPath, "_Project", "Cinematics"));
                AssetDatabase.Refresh();
            }

            var timeline = AssetDatabase.LoadAssetAtPath<TimelineAsset>(TimelinePath);
            if (timeline == null)
            {
                timeline = ScriptableObject.CreateInstance<TimelineAsset>();
                AssetDatabase.CreateAsset(timeline, TimelinePath);
                Debug.Log($"[ClimaxTimeline] Created Moon8_Armada timeline at {TimelinePath}");
            }
            else
            {
                Debug.Log("[ClimaxTimeline] Moon8_Armada timeline already exists — refreshing.");
            }

            // Clear existing tracks
            var existingTracks = timeline.GetOutputTracks();
            foreach (var track in existingTracks)
                timeline.DeleteTrack(track);

            // 1. ActivationTrack for ArmadaCamera GameObject
            var activationTrack = timeline.CreateTrack<ActivationTrack>(null, "ArmadaCamera_Activation");
            activationTrack.CreateClip(0.0, 12.0);

            // 2. AnimationTrack with procedural flyover clip
            var animTrack = timeline.CreateTrack<AnimationTrack>(null, "ArmadaCamera_FlyPath");
            var animClip = BuildFlyoverClip();
            var timelineClip = animTrack.CreateClip(animClip);
            timelineClip.duration = 12.0;

            // 3. SignalTrack with 3 signal markers (start, mid, end)
            var signalTrack = timeline.CreateTrack<SignalTrack>(null, "Climax_Signals");

            var startSignal = ScriptableObject.CreateInstance<SignalAsset>();
            startSignal.name = "Moon8_Start";
            AssetDatabase.AddObjectToAsset(startSignal, timeline);

            var midSignal = ScriptableObject.CreateInstance<SignalAsset>();
            midSignal.name = "Moon8_Mid";
            AssetDatabase.AddObjectToAsset(midSignal, timeline);

            var endSignal = ScriptableObject.CreateInstance<SignalAsset>();
            endSignal.name = "Moon8_End";
            AssetDatabase.AddObjectToAsset(endSignal, timeline);

            signalTrack.CreateMarker<SignalEmitter>(0.0).asset = startSignal;
            signalTrack.CreateMarker<SignalEmitter>(6.0).asset = midSignal;
            signalTrack.CreateMarker<SignalEmitter>(11.8).asset = endSignal;

            EditorUtility.SetDirty(timeline);
            AssetDatabase.SaveAssets();
            Debug.Log("[ClimaxTimeline] Moon8_Armada timeline built: 1 activation track, 1 anim track (12s 200m fly), 3 signals.");
#else
            Debug.LogWarning("[ClimaxTimeline] Timeline package not available — Moon8 flyover will use coroutine fallback. Install com.unity.timeline for full cinematic support.");
#endif
        }

#if UNITY_TIMELINE
        static AnimationClip BuildFlyoverClip()
        {
            var existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(ClipPath);
            if (existing != null)
            {
                Debug.Log("[ClimaxTimeline] Reusing existing flyover clip.");
                return existing;
            }

            var clip = new AnimationClip
            {
                name = "Moon8_ArmadaFly",
                frameRate = 30,
                legacy = false
            };

            // Position curve: X moves 0→200 over 12s
            var posX = AnimationCurve.Linear(0f, 0f, 12f, 200f);
            clip.SetCurve("", typeof(Transform), "localPosition.x", posX);

            // Y and Z stay constant
            clip.SetCurve("", typeof(Transform), "localPosition.y", AnimationCurve.Constant(0f, 12f, 50f));
            clip.SetCurve("", typeof(Transform), "localPosition.z", AnimationCurve.Constant(0f, 12f, 0f));

            AssetDatabase.CreateAsset(clip, ClipPath);
            Debug.Log($"[ClimaxTimeline] Created flyover clip at {ClipPath}");
            return clip;
        }
#endif
    }
}
