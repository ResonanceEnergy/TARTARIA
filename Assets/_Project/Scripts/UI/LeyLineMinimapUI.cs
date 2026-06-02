// LeyLineMinimapUI.cs
// Owner: Level Designer (per docs/agents/COORDINATION.md)
// Path: Assets/_Project/Scripts/UI/ (UI assembly owns the file; data lives in LeyLineMinimap.cs)
//
// Mandate refs (CLAUDE.md 2026-06-02 NO-DEBT):
//   Rule 3 - no silent fails; every catch logs file:line + payload.
//   Rule 4 - no silent fallbacks; missing Player tag warns every 5 s.
//   Rule 7 - no TODO stubs on the ship-gate path.
//
// Behaviour:
//   * RuntimeInitializeOnLoadMethod (AfterSceneLoad) bootstraps a single
//     screen-space-overlay canvas + this MonoBehaviour.
//   * Builds a 200x200 minimap pinned to the top-right with a 16 px margin.
//   * Player dot (white, 8 px) follows the GameObject tagged "Player".
//   * Three hero markers (12 px) are positioned once from
//     LeyLineMinimap.HeroPositions and flip from red -> green on
//     GameEvents.OnBuildingRestored.
//   * No TextMeshPro - pure UnityEngine.UI as required.

using System;
using System.Collections.Generic;
using Tartaria.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Tartaria.UI
{
    [DisallowMultipleComponent]
    public class LeyLineMinimapUI : MonoBehaviour
    {
        // ---- Layout constants ---------------------------------------------------
        private const float MinimapSize = 200f;
        private const float MinimapMargin = 16f;
        private const float PlayerDotSize = 8f;
        private const float HeroMarkerSize = 12f;
        private const int SortingOrder = 30200;
        private const float MissingPlayerWarnIntervalSec = 5f;

        // Colours
        private static readonly Color BackgroundColor = new Color(0f, 0f, 0f, 0.55f);
        private static readonly Color PlayerColor = Color.white;
        private static readonly Color HeroUnrestoredColor = new Color(0.9f, 0.18f, 0.18f, 1f); // red
        private static readonly Color HeroRestoredColor = new Color(0.18f, 0.9f, 0.32f, 1f);  // green

        // ---- Runtime state ------------------------------------------------------
        private RectTransform _minimapRect;
        private RectTransform _playerDotRect;
        private readonly List<RectTransform> _heroRects = new List<RectTransform>(3);
        private readonly List<Image> _heroImages = new List<Image>(3);
        private readonly HashSet<int> _restoredHeroIndices = new HashSet<int>();

        private Transform _playerTransform;
        private float _lastWarnedTime = -999f;
        private bool _subscribed;

        // ---- Bootstrap ----------------------------------------------------------
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            // Idempotent: if another scene load already spawned a minimap, skip.
            // Per Unity 6 Scripting Reference: FindObjectOfType is deprecated; the
            // canonical API is FindFirstObjectByType<T>(FindObjectsInactive) — explicit
            // semantics, non-allocating in the inactive-excluded path.
            if (FindFirstObjectByType<LeyLineMinimapUI>(FindObjectsInactive.Include) != null)
            {
                return;
            }

            var canvasGo = new GameObject("LeyLineMinimapCanvas");
            DontDestroyOnLoad(canvasGo);

            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = SortingOrder;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;

            canvasGo.AddComponent<GraphicRaycaster>();

            canvasGo.AddComponent<LeyLineMinimapUI>();
        }

        // ---- Unity lifecycle ----------------------------------------------------
        private void Awake()
        {
            BuildHierarchy();
            ResolvePlayer();
            PlaceHeroMarkersOnce();
        }

        private void OnEnable()
        {
            try
            {
                GameEvents.OnBuildingRestored += HandleBuildingRestored;
                _subscribed = true;
            }
            catch (Exception ex)
            {
                // Rule 3: log loud with file:line + payload, then rethrow.
                Debug.LogError($"[LeyLineMinimapUI] Failed to subscribe to GameEvents.OnBuildingRestored at {nameof(LeyLineMinimapUI)}.OnEnable: {ex}");
                throw;
            }
        }

        private void OnDisable()
        {
            if (!_subscribed)
            {
                return;
            }

            try
            {
                GameEvents.OnBuildingRestored -= HandleBuildingRestored;
                _subscribed = false;
            }
            catch (Exception ex)
            {
                // Rule 3: log loud and rethrow - unsubscribe failure is a leak.
                Debug.LogError($"[LeyLineMinimapUI] Failed to unsubscribe from GameEvents.OnBuildingRestored at {nameof(LeyLineMinimapUI)}.OnDisable: {ex}");
                throw;
            }
        }

        private void Update()
        {
            if (_playerTransform == null)
            {
                ResolvePlayer();
                if (_playerTransform == null)
                {
                    WarnMissingPlayer();
                    return;
                }
            }

            Vector2 norm = LeyLineMinimap.WorldToMinimap(_playerTransform.position);
            // Convert normalized 0..1 to local anchored position centred on the
            // minimap. With pivot (0.5,0.5) on the dot, offset range is
            // [-MinimapSize/2, +MinimapSize/2].
            _playerDotRect.anchoredPosition = new Vector2(
                (norm.x - 0.5f) * MinimapSize,
                (norm.y - 0.5f) * MinimapSize);
        }

        // ---- Hierarchy construction --------------------------------------------
        private void BuildHierarchy()
        {
            var canvasTransform = transform as RectTransform;
            if (canvasTransform == null)
            {
                // The canvas root always has a RectTransform once Canvas exists;
                // if it doesn't, the bootstrap is wrong - hard-fail rather than
                // silently degrade (Rule 3/4).
                Debug.LogError("[LeyLineMinimapUI] Canvas root has no RectTransform - bootstrap broken.");
                return;
            }

            // Minimap background panel - anchored top-right.
            var bgGo = new GameObject("MinimapBackground", typeof(RectTransform), typeof(Image));
            _minimapRect = bgGo.GetComponent<RectTransform>();
            _minimapRect.SetParent(canvasTransform, false);
            _minimapRect.anchorMin = new Vector2(1f, 1f);
            _minimapRect.anchorMax = new Vector2(1f, 1f);
            _minimapRect.pivot = new Vector2(1f, 1f);
            _minimapRect.anchoredPosition = new Vector2(-MinimapMargin, -MinimapMargin);
            _minimapRect.sizeDelta = new Vector2(MinimapSize, MinimapSize);

            var bgImage = bgGo.GetComponent<Image>();
            bgImage.color = BackgroundColor;
            bgImage.raycastTarget = false;

            // Hero markers (red until restored). Build before the player dot
            // so the dot renders on top.
            for (int i = 0; i < LeyLineMinimap.HeroIds.Length; i++)
            {
                string id = LeyLineMinimap.HeroIds[i];
                var heroGo = new GameObject($"HeroMarker_{id}", typeof(RectTransform), typeof(Image));
                var heroRect = heroGo.GetComponent<RectTransform>();
                heroRect.SetParent(_minimapRect, false);
                heroRect.anchorMin = new Vector2(0.5f, 0.5f);
                heroRect.anchorMax = new Vector2(0.5f, 0.5f);
                heroRect.pivot = new Vector2(0.5f, 0.5f);
                heroRect.sizeDelta = new Vector2(HeroMarkerSize, HeroMarkerSize);

                var heroImage = heroGo.GetComponent<Image>();
                heroImage.color = HeroUnrestoredColor;
                heroImage.raycastTarget = false;

                _heroRects.Add(heroRect);
                _heroImages.Add(heroImage);
            }

            // Player dot - drawn last so it sits above hero markers.
            var dotGo = new GameObject("PlayerDot", typeof(RectTransform), typeof(Image));
            _playerDotRect = dotGo.GetComponent<RectTransform>();
            _playerDotRect.SetParent(_minimapRect, false);
            _playerDotRect.anchorMin = new Vector2(0.5f, 0.5f);
            _playerDotRect.anchorMax = new Vector2(0.5f, 0.5f);
            _playerDotRect.pivot = new Vector2(0.5f, 0.5f);
            _playerDotRect.sizeDelta = new Vector2(PlayerDotSize, PlayerDotSize);

            var dotImage = dotGo.GetComponent<Image>();
            dotImage.color = PlayerColor;
            dotImage.raycastTarget = false;
        }

        private void PlaceHeroMarkersOnce()
        {
            int count = Mathf.Min(LeyLineMinimap.HeroPositions.Length, _heroRects.Count);
            for (int i = 0; i < count; i++)
            {
                Vector2 norm = LeyLineMinimap.WorldToMinimap(LeyLineMinimap.HeroPositions[i]);
                _heroRects[i].anchoredPosition = new Vector2(
                    (norm.x - 0.5f) * MinimapSize,
                    (norm.y - 0.5f) * MinimapSize);
            }
        }

        // ---- Player tracking ----------------------------------------------------
        private void ResolvePlayer()
        {
            GameObject playerGo = null;
            try
            {
                playerGo = GameObject.FindGameObjectWithTag("Player");
            }
            catch (UnityException ex)
            {
                // FindGameObjectWithTag throws if the "Player" tag isn't defined
                // in the TagManager. Log loud with path so the tag can be added
                // (Rule 3); cooldown the warning so we don't spam the console.
                if (Time.unscaledTime - _lastWarnedTime > MissingPlayerWarnIntervalSec)
                {
                    Debug.LogWarning($"[LeyLineMinimap] 'Player' tag not defined in TagManager.asset; minimap dot frozen. ({ex.GetType().Name}: {ex.Message})");
                    _lastWarnedTime = Time.unscaledTime;
                }
                return;
            }

            if (playerGo != null)
            {
                _playerTransform = playerGo.transform;
            }
        }

        private void WarnMissingPlayer()
        {
            if (Time.unscaledTime - _lastWarnedTime <= MissingPlayerWarnIntervalSec)
            {
                return;
            }
            _lastWarnedTime = Time.unscaledTime;
            // Rule 4: warn with the actual identifier we looked for, not just
            // "missing".
            Debug.LogWarning("[LeyLineMinimap] No Player-tagged GO; minimap dot frozen.");
        }

        // ---- Event handler ------------------------------------------------------
        private void HandleBuildingRestored(string buildingId)
        {
            // No silent catches (Rule 3): if the handler explodes, log file:line
            // + the payload that broke it, then rethrow so the dispatcher logs it
            // at its own site too (GameEvents.cs already catches and LogErrors).
            try
            {
                if (string.IsNullOrEmpty(buildingId))
                {
                    Debug.LogWarning("[LeyLineMinimap] HandleBuildingRestored called with null/empty buildingId; ignoring.");
                    return;
                }

                int matched = -1;
                for (int i = 0; i < LeyLineMinimap.HeroIds.Length; i++)
                {
                    string heroId = LeyLineMinimap.HeroIds[i];
                    if (buildingId.IndexOf(heroId, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        matched = i;
                        break;
                    }
                }

                if (matched < 0)
                {
                    // Not a hero - other systems (e.g. village buildings) will fire
                    // this event too. Trace-level info so we can confirm wiring.
                    return;
                }

                if (matched >= _heroImages.Count || _heroImages[matched] == null)
                {
                    Debug.LogWarning($"[LeyLineMinimap] Hero index {matched} (id='{LeyLineMinimap.HeroIds[matched]}') has no image instance; minimap not built yet?");
                    return;
                }

                if (_restoredHeroIndices.Add(matched))
                {
                    _heroImages[matched].color = HeroRestoredColor;
                    Debug.Log($"[LeyLineMinimap] Hero building restored: id='{buildingId}' -> marker[{matched}] '{LeyLineMinimap.HeroIds[matched]}' set green.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LeyLineMinimap] HandleBuildingRestored threw for buildingId='{buildingId}': {ex}");
                throw;
            }
        }
    }
}
