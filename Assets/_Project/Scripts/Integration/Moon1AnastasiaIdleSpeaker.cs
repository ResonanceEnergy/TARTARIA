using System.Collections;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration {
    [DisallowMultipleComponent]
    public class Moon1AnastasiaIdleSpeaker : MonoBehaviour {
        [SerializeField] float minInterval = 45f;
        [SerializeField] float maxInterval = 90f;
        [SerializeField] float playerNearRadius = 14f; // don't speak if no one's around
        static readonly string[] NodeNames = { "anastasia_idle_1", "anastasia_idle_2", "anastasia_idle_3", "anastasia_idle_4" };
        Transform _playerT;
        float _nextSpeakTime;
        int _lastIdx = -1;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoAttach() {
            // Per Unity Manual on Tags & Layers: FindGameObjectsWithTag throws UnityException
            // if the tag isn't registered in TagManager. Defensive wrap + loud log per
            // CLAUDE.md no-debt rule 3 (never silent catch) + rule 4 (warning carries the tag name).
            GameObject[] hits;
            try {
                hits = GameObject.FindGameObjectsWithTag("Anastasia");
            } catch (UnityException ex) {
                Debug.LogWarning($"[Moon1AnastasiaIdleSpeaker] Tag 'Anastasia' is not defined in TagManager — skipping auto-attach. ({ex.Message}) Add the tag via Project Settings → Tags and Layers.");
                return;
            }
            foreach (var go in hits) {
                if (go.GetComponent<Moon1AnastasiaIdleSpeaker>() == null) {
                    go.AddComponent<Moon1AnastasiaIdleSpeaker>();
                    Debug.Log($"[Moon1AnastasiaIdleSpeaker] Attached to '{go.name}'");
                }
            }
        }

        void Start() {
            _nextSpeakTime = UnityEngine.Time.time + Random.Range(minInterval, maxInterval);
        }

        void Update() {
            if (UnityEngine.Time.time < _nextSpeakTime) return;
            if (_playerT == null) {
                var pgo = GameObject.FindGameObjectWithTag("Player");
                if (pgo != null) _playerT = pgo.transform;
            }
            if (_playerT != null) {
                float dist = Vector3.Distance(transform.position, _playerT.position);
                if (dist > playerNearRadius) {
                    _nextSpeakTime = UnityEngine.Time.time + 5f; // poll again in 5s
                    return;
                }
            }
            SpeakRandomLine();
            _nextSpeakTime = UnityEngine.Time.time + Random.Range(minInterval, maxInterval);
        }

        void SpeakRandomLine() {
            int idx;
            do { idx = Random.Range(0, NodeNames.Length); } while (idx == _lastIdx && NodeNames.Length > 1);
            _lastIdx = idx;
            string node = NodeNames[idx];
            // Reflect into DialogueManager — log + rethrow per rule 3
            var dmType = System.Type.GetType("Tartaria.Dialogue.DialogueManager") ?? System.Type.GetType("DialogueManager");
            if (dmType == null) {
                GameEvents.RaiseHUDShowBanner("Anastasia", "(she hums)", 4f);
                Debug.LogWarning($"[Moon1AnastasiaIdleSpeaker] No DialogueManager type — fell back to HUD banner for node {node}");
                return;
            }
            var prop = dmType.GetProperty("Instance");
            var dm = prop?.GetValue(null);
            var method = dmType.GetMethod("PlayYarn") ?? dmType.GetMethod("PlayContextDialogue");
            if (dm == null || method == null) {
                GameEvents.RaiseHUDShowBanner("Anastasia", "(she hums)", 4f);
                Debug.LogWarning($"[Moon1AnastasiaIdleSpeaker] DialogueManager.Instance or PlayYarn missing — fallback HUD banner used for node {node}");
                return;
            }
            try { method.Invoke(dm, new object[] { node }); Debug.Log($"[Moon1AnastasiaIdleSpeaker] Spoke node '{node}'"); }
            catch (System.Exception ex) { Debug.LogError($"[Moon1AnastasiaIdleSpeaker] PlayYarn threw for '{node}': {ex.Message}"); throw; }
        }
    }
}
