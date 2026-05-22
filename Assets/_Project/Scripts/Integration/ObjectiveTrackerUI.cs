using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using Tartaria.Core;
using Tartaria.Integration;

namespace Tartaria.Integration
{
    /// <summary>
    /// ObjectiveTrackerUI — persistent on-screen quest/checkpoint tracker.
    /// Displays current objectives, progress bars, checkmarks on completion.
    /// Auto-updates from QuestManager events + manual SetObjective calls.
    /// 
    /// Usage:
    /// - Attach to Canvas with LayoutGroup (VerticalLayoutGroup recommended)
    /// - Automatically subscribes to QuestManager events
    /// - Objectives auto-fade in/out, stack vertically
    /// 
    /// GDD refs: §05 (UI/UX), §03 (13 Moons Campaign)
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class ObjectiveTrackerUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] Transform objectiveContainer;  // Parent for objective entries
        [SerializeField] GameObject objectiveEntryPrefab;  // Prefab with Text + ProgressBar + Checkmark
        [SerializeField] CanvasGroup canvasGroup;

        [Header("Display Settings")]
        [SerializeField] int maxVisibleObjectives = 5;
        [SerializeField] float fadeInDuration = 0.5f;
        [SerializeField] float fadeOutDuration = 0.3f;
        [SerializeField] float completionHoldTime = 2f;  // Hold completed objective for 2s before fade

        Dictionary<string, ObjectiveEntry> _activeObjectives = new();
        Queue<string> _displayOrder = new();

        void Awake()
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        }

        void Start()
        {
            // Subscribe to QuestManager events
            var questMgr = Tartaria.Integration.QuestManager.Instance;
            if (questMgr != null)
            {
                questMgr.OnObjectiveProgressed += OnObjectiveProgressed;
                Debug.Log("[ObjectiveTracker] Subscribed to QuestManager.OnObjectiveProgressed");
            }

            // Subscribe to GameEvents for moon objectives
            GameEvents.OnMoonCleared += OnMoonCleared;
        }

        void OnDestroy()
        {
            var questMgr = Integration.QuestManager.Instance;
            if (questMgr != null)
            {
                questMgr.OnObjectiveProgressed -= OnObjectiveProgressed;
            }

            GameEvents.OnMoonCleared -= OnMoonCleared;
        }

        void OnObjectiveProgressed(string questId, int objectiveIndex)
        {
            // Fetch objective text from QuestManager
            var questMgr = Integration.QuestManager.Instance;
            if (questMgr != null)
            {
                var questDef = questMgr.GetQuestDefinition(questId);
                var questState = questMgr.GetQuestState(questId);

                if (questDef != null && questState != null && objectiveIndex < questDef.objectives.Count)
                {
                    var objective = questDef.objectives[objectiveIndex];
                    var progress = questState.objectiveProgress[objectiveIndex];
                    float progressPercent = (float)progress / objective.targetCount;

                    string objectiveText = $"{questDef.title}: {objective.description} ({progress}/{objective.targetCount})";
                    SetObjective($"{questId}_{objectiveIndex}", objectiveText, progressPercent, progress >= objective.targetCount);
                    return;
                }
            }

            // Fallback if QuestManager unavailable
            string objectiveId = $"{questId}_{objectiveIndex}";
            SetObjective(objectiveId, $"Quest {questId} - Objective {objectiveIndex}", 0.5f);
        }

        void OnMoonCleared(int moonIndex)
        {
            SetObjective($"moon_{moonIndex}_complete", $"Moon {moonIndex} Cleared!", 1f, true);
        }

        /// <summary>
        /// Add or update objective in tracker.
        /// </summary>
        public void SetObjective(string objectiveId, string text, float progress = 0f, bool isComplete = false)
        {
            if (_activeObjectives.TryGetValue(objectiveId, out var existing))
            {
                // Update existing
                existing.UpdateProgress(progress, isComplete);
            }
            else
            {
                // Create new entry
                if (objectiveEntryPrefab == null || objectiveContainer == null)
                {
                    Debug.LogWarning("[ObjectiveTracker] Missing prefab or container");
                    return;
                }

                var entryGO = Instantiate(objectiveEntryPrefab, objectiveContainer);
                var entry = entryGO.GetComponent<ObjectiveEntry>();
                if (entry == null)
                {
                    entry = entryGO.AddComponent<ObjectiveEntry>();
                }

                entry.Initialize(objectiveId, text, progress, isComplete);
                entry.onRemove += () => RemoveObjective(objectiveId);

                _activeObjectives[objectiveId] = entry;
                _displayOrder.Enqueue(objectiveId);

                // Enforce max visible limit
                while (_displayOrder.Count > maxVisibleObjectives)
                {
                    string oldestId = _displayOrder.Dequeue();
                    RemoveObjective(oldestId);
                }

                Debug.Log($"[ObjectiveTracker] Added: {objectiveId} - {text} ({progress:P0})");
            }

            // Auto-remove completed objectives after delay
            if (isComplete)
            {
                var entry = _activeObjectives[objectiveId];
                entry.StartCoroutine(RemoveAfterDelay(objectiveId, completionHoldTime));
            }
        }

        /// <summary>
        /// Remove objective from tracker.
        /// </summary>
        public void RemoveObjective(string objectiveId)
        {
            if (_activeObjectives.TryGetValue(objectiveId, out var entry))
            {
                Destroy(entry.gameObject);
                _activeObjectives.Remove(objectiveId);

                Debug.Log($"[ObjectiveTracker] Removed: {objectiveId}");
            }
        }

        System.Collections.IEnumerator RemoveAfterDelay(string objectiveId, float delay)
        {
            yield return new WaitForSeconds(delay);
            RemoveObjective(objectiveId);
        }

        /// <summary>
        /// Clear all objectives.
        /// </summary>
        public void ClearAllObjectives()
        {
            foreach (var entry in _activeObjectives.Values)
            {
                if (entry != null) Destroy(entry.gameObject);
            }
            _activeObjectives.Clear();
            _displayOrder.Clear();

            Debug.Log("[ObjectiveTracker] Cleared all objectives");
        }
    }

    /// <summary>
    /// Individual objective entry component.
    /// </summary>
    public class ObjectiveEntry : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] TextMeshProUGUI objectiveText;
        [SerializeField] Slider progressBar;
        [SerializeField] GameObject checkmarkIcon;
        [SerializeField] CanvasGroup canvasGroup;

        string _objectiveId;
        float _progress;
        bool _isComplete;

        public System.Action onRemove;

        public void Initialize(string objectiveId, string text, float progress, bool isComplete)
        {
            _objectiveId = objectiveId;
            _progress = progress;
            _isComplete = isComplete;

            if (objectiveText != null) objectiveText.text = text;
            if (progressBar != null)
            {
                progressBar.value = progress;
                progressBar.gameObject.SetActive(!isComplete);
            }
            if (checkmarkIcon != null) checkmarkIcon.SetActive(isComplete);

            // Fade in
            if (canvasGroup != null)
            {
                StartCoroutine(FadeInCoroutine(canvasGroup, 0.5f));
            }
        }

        public void UpdateProgress(float progress, bool isComplete)
        {
            _progress = progress;
            _isComplete = isComplete;

            if (progressBar != null)
            {
                progressBar.value = progress;
                progressBar.gameObject.SetActive(!isComplete);
            }

            if (checkmarkIcon != null && isComplete)
            {
                checkmarkIcon.SetActive(true);
                // Pulse animation (simple scale bounce)
                StartCoroutine(PulseScaleCoroutine(checkmarkIcon.transform, 1.2f, 0.2f));
            }

            if (isComplete && objectiveText != null)
            {
                objectiveText.color = Color.green;
                objectiveText.fontStyle = FontStyles.Strikethrough;
            }
        }

        public void FadeOut(float duration, System.Action onComplete = null)
        {
            if (canvasGroup != null)
            {
                StartCoroutine(FadeOutCoroutine(canvasGroup, duration, () =>
                {
                    onComplete?.Invoke();
                    onRemove?.Invoke();
                }));
            }
            else
            {
                onComplete?.Invoke();
                onRemove?.Invoke();
            }
        }

        System.Collections.IEnumerator FadeInCoroutine(CanvasGroup cg, float duration)
        {
            cg.alpha = 0f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                cg.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
                yield return null;
            }
            cg.alpha = 1f;
        }

        System.Collections.IEnumerator FadeOutCoroutine(CanvasGroup cg, float duration, System.Action onComplete)
        {
            float startAlpha = cg.alpha;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                cg.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / duration);
                yield return null;
            }
            cg.alpha = 0f;
            onComplete?.Invoke();
        }

        System.Collections.IEnumerator PulseScaleCoroutine(Transform target, float targetScale, float duration)
        {
            Vector3 originalScale = target.localScale;
            Vector3 pulseScale = Vector3.one * targetScale;
            float elapsed = 0f;
            
            // Scale up
            while (elapsed < duration / 2f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (duration / 2f);
                target.localScale = Vector3.Lerp(originalScale, pulseScale, t);
                yield return null;
            }
            
            // Scale back down
            elapsed = 0f;
            while (elapsed < duration / 2f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (duration / 2f);
                target.localScale = Vector3.Lerp(pulseScale, originalScale, t);
                yield return null;
            }
            
            target.localScale = originalScale;
        }
    }
}
