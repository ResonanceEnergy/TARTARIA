using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Tartaria.Editor
{
    /// <summary>
    /// Animation Event Helper — Editor tool for adding animation events to attack clips.
    /// Menu: Tartaria → Add Combat Animation Events
    /// Searches for attack animation clips and adds OnAttackHit events at optimal timing.
    /// </summary>
    public class AnimationEventHelper : EditorWindow
    {
        private AnimationClip[] _attackClips;
        private Vector2 _scrollPos;
        private string _statusMessage = "";
        private Color _statusColor = Color.white;

        [MenuItem("Tartaria/Animation/Add Combat Events")]
        static void ShowWindow()
        {
            var window = GetWindow<AnimationEventHelper>("Animation Events");
            window.minSize = new Vector2(600, 400);
            window.Show();
        }

        void OnEnable()
        {
            FindAttackClips();
        }

        void FindAttackClips()
        {
            // Search for animation clips with "Attack" in the name
            string[] guids = AssetDatabase.FindAssets("t:AnimationClip Attack", new[] { "Assets" });
            _attackClips = guids
                .Select(guid => AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(clip => clip != null && !clip.name.Contains("@"))
                .ToArray();

            _statusMessage = $"Found {_attackClips.Length} attack animation clips";
            _statusColor = Color.cyan;
        }

        void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Combat Animation Event Manager", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "This tool adds animation events to attack clips for precise hit detection timing.\n" +
                "Events are placed at optimal frames (typically 40-60% through the animation).",
                MessageType.Info
            );

            EditorGUILayout.Space(10);

            // Refresh button
            if (GUILayout.Button("Refresh Clips", GUILayout.Height(30)))
            {
                FindAttackClips();
            }

            EditorGUILayout.Space(5);

            // Status message
            GUI.color = _statusColor;
            EditorGUILayout.LabelField(_statusMessage, EditorStyles.wordWrappedLabel);
            GUI.color = Color.white;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField($"Found {_attackClips.Length} clips:", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            foreach (var clip in _attackClips)
            {
                EditorGUILayout.BeginHorizontal("box");

                // Clip name and info
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField(clip.name, EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Duration: {clip.length:F2}s | FPS: {clip.frameRate:F0} | Events: {clip.events.Length}");
                EditorGUILayout.EndVertical();

                GUILayout.FlexibleSpace();

                // Action buttons
                if (GUILayout.Button("Add Hit Event", GUILayout.Width(120)))
                {
                    AddHitEvent(clip);
                }

                if (GUILayout.Button("Clear Events", GUILayout.Width(120)))
                {
                    ClearEvents(clip);
                }

                if (GUILayout.Button("Preview", GUILayout.Width(80)))
                {
                    PreviewClip(clip);
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(3);
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(10);

            // Bulk actions
            EditorGUILayout.LabelField("Bulk Actions:", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Add Events to All Clips", GUILayout.Height(35)))
            {
                AddEventsToAll();
            }

            if (GUILayout.Button("Clear All Events", GUILayout.Height(35)))
            {
                ClearAllEvents();
            }

            EditorGUILayout.EndHorizontal();
        }

        void AddHitEvent(AnimationClip clip)
        {
            // Check if event already exists
            if (clip.events.Any(e => e.functionName == "OnAttackHit"))
            {
                _statusMessage = $"{clip.name}: OnAttackHit event already exists";
                _statusColor = Color.yellow;
                return;
            }

            // Calculate optimal timing (50% through animation)
            float hitTime = clip.length * 0.5f;

            // Create animation event
            AnimationEvent evt = new AnimationEvent
            {
                time = hitTime,
                functionName = "OnAttackHit",
                stringParameter = "",
                intParameter = 0,
                floatParameter = 0f,
                objectReferenceParameter = null
            };

            // Add event to clip
            var events = clip.events.ToList();
            events.Add(evt);
            AnimationUtility.SetAnimationEvents(clip, events.ToArray());

            EditorUtility.SetDirty(clip);
            AssetDatabase.SaveAssets();

            _statusMessage = $"✅ Added OnAttackHit event to {clip.name} at {hitTime:F2}s";
            _statusColor = Color.green;

            Debug.Log($"[AnimationEventHelper] Added OnAttackHit to {clip.name} at time {hitTime:F2}s");
        }

        void ClearEvents(AnimationClip clip)
        {
            if (clip.events.Length == 0)
            {
                _statusMessage = $"{clip.name}: No events to clear";
                _statusColor = Color.gray;
                return;
            }

            AnimationUtility.SetAnimationEvents(clip, new AnimationEvent[0]);
            EditorUtility.SetDirty(clip);
            AssetDatabase.SaveAssets();

            _statusMessage = $"🗑️ Cleared {clip.events.Length} events from {clip.name}";
            _statusColor = Color.yellow;

            Debug.Log($"[AnimationEventHelper] Cleared events from {clip.name}");
        }

        void PreviewClip(AnimationClip clip)
        {
            // Select the clip in the Project window
            Selection.activeObject = clip;
            EditorGUIUtility.PingObject(clip);

            _statusMessage = $"Selected {clip.name} in Project window";
            _statusColor = Color.cyan;
        }

        void AddEventsToAll()
        {
            int successCount = 0;
            int skippedCount = 0;

            foreach (var clip in _attackClips)
            {
                if (clip.events.Any(e => e.functionName == "OnAttackHit"))
                {
                    skippedCount++;
                    continue;
                }

                float hitTime = clip.length * 0.5f;
                AnimationEvent evt = new AnimationEvent
                {
                    time = hitTime,
                    functionName = "OnAttackHit"
                };

                var events = clip.events.ToList();
                events.Add(evt);
                AnimationUtility.SetAnimationEvents(clip, events.ToArray());
                EditorUtility.SetDirty(clip);
                successCount++;
            }

            AssetDatabase.SaveAssets();

            _statusMessage = $"✅ Added events to {successCount} clips (skipped {skippedCount} existing)";
            _statusColor = Color.green;

            Debug.Log($"[AnimationEventHelper] Bulk add complete: {successCount} clips modified, {skippedCount} skipped");
        }

        void ClearAllEvents()
        {
            if (!EditorUtility.DisplayDialog("Clear All Events",
                $"Are you sure you want to clear all animation events from {_attackClips.Length} clips?",
                "Yes", "Cancel"))
            {
                return;
            }

            int clearedCount = 0;

            foreach (var clip in _attackClips)
            {
                if (clip.events.Length > 0)
                {
                    AnimationUtility.SetAnimationEvents(clip, new AnimationEvent[0]);
                    EditorUtility.SetDirty(clip);
                    clearedCount++;
                }
            }

            AssetDatabase.SaveAssets();

            _statusMessage = $"🗑️ Cleared events from {clearedCount} clips";
            _statusColor = Color.yellow;

            Debug.Log($"[AnimationEventHelper] Bulk clear complete: {clearedCount} clips cleared");
        }
    }
}
