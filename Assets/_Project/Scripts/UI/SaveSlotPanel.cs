using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Tartaria.Save;

namespace Tartaria.UI
{
    /// <summary>
    /// Sprint 6 Lane 3 / Sprint 7 Lane 4: Manages 3 save-slot cards (slots 0/1/2) in a single panel.
    ///
    /// Responsibilities:
    ///   - Build 3 SaveSlotEntry cards at runtime
    ///   - Refresh card content from SaveManager.GetSaveInfo(slot) (Assets/_Project/Scripts/Save/SaveManager.cs:654)
    ///   - Wire Load -> SaveManager.SwitchToSlot(slot)        (Assets/_Project/Scripts/Save/SaveManager.cs:595)
    ///   - Wire Delete -> runtime confirm modal then SaveManager.DeleteSlot(slot) (Assets/_Project/Scripts/Save/SaveManager.cs:693)
    ///   - Capture a screenshot after every save (hooked to SaveManager.OnBeforeSave / OnAfterLoad events,
    ///     fallback to mtime poll if events somehow unavailable). Encode PNG, write to
    ///     {persistentDataPath}/saves/slot{N}.png. Also write slot{N}.meta.json sidecar
    ///     so card can render Moon name, shards, buildings without re-decrypting the save.
    ///   - [S7 L4] Also capture on UnityEngine.SceneManagement.SceneManager.activeSceneChanged so the
    ///     LAST FRAME of the outgoing scene is cached as the current slot's thumbnail before
    ///     the new scene clobbers the framebuffer. Subscribed in Start, unsubscribed in OnDestroy.
    ///   - [S7 L4] PNG size knob: if EncodeToPNG produces >256KB the texture is downscaled to half
    ///     resolution via Graphics.Blit + RenderTexture and re-encoded. Loud log when triggered.
    ///
    /// Constraints honoured:
    ///   - Unity 6 API: uses FindFirstObjectByType + FindObjectsInactive
    ///   - No invented method names (grep evidence in this file + SaveSlotEntry.cs)
    ///   - No silent catches: every catch logs file:line + the value that broke + persistentDataPath
    ///   - Texture2D dispose pattern: cached and Destroy'd in OnDestroy
    /// </summary>
    public class SaveSlotPanel : MonoBehaviour
    {
        [Header("Slots")]
        [SerializeField] int slotCount = 3;

        [Header("Layout")]
        [SerializeField] Vector2 panelSize = new Vector2(1320f, 280f);
        [SerializeField] float slotSpacing = 12f;

        // ── S7 L4: PNG size knob ────────────────────────────────────────────
        // If EncodeToPNG exceeds this byte count, downscale to half-res and re-encode.
        public const int MaxThumbnailBytes = 256 * 1024; // 256 KB

        readonly List<SaveSlotEntry> _entries = new();
        Texture2D[] _loadedThumbs;  // cached per slot, destroyed in OnDestroy
        long[] _lastMtimeTicks;     // mtime poll fallback (loud-logged)
        bool _usingMtimePollFallback = false;

        // Confirm modal
        GameObject _confirmModalRoot;
        TMP_Text _confirmModalText;
        Button _confirmYesButton;
        Button _confirmNoButton;
        int _pendingDeleteSlot = -1;

        // Reference to SaveManager (resolved lazily; logs loud if missing)
        SaveManager _saveManager;

        // Pending screenshot capture (set true after subscribed save fires; consumed by next frame's LateUpdate
        // because ScreenCapture.CaptureScreenshotAsTexture must run end-of-frame).
        bool _capturePendingThisFrame = false;
        int _capturePendingForSlot = -1;

        // ── Public sidecar paths (also used by tests / external tools) ────
        public static string GetSavesDir()
        {
            string dir = Path.Combine(Application.persistentDataPath, "saves");
            if (!Directory.Exists(dir))
            {
                try { Directory.CreateDirectory(dir); }
                catch (Exception e)
                {
                    Debug.LogError($"[SaveSlotPanel] Failed to create saves dir '{dir}' (persistentDataPath='{Application.persistentDataPath}'): {e.GetType().Name}: {e.Message}");
                }
            }
            return dir;
        }

        public static string GetThumbnailPath(int slot) => Path.Combine(GetSavesDir(), $"slot{slot}.png");
        public static string GetMetadataPath(int slot) => Path.Combine(GetSavesDir(), $"slot{slot}.meta.json");

        void Awake()
        {
            _loadedThumbs = new Texture2D[slotCount];
            _lastMtimeTicks = new long[slotCount];

            BuildPanelLayout();
            BuildConfirmModal();
        }

        void Start()
        {
            _saveManager = ResolveSaveManager();
            HookSaveManagerEvents();

            // [S7 L4] Cache last scene's screenshot before new scene loads.
            SceneManager.activeSceneChanged += HandleActiveSceneChanged;
            Debug.Log("[SaveSlotPanel] Subscribed to SceneManager.activeSceneChanged for pre-load thumbnail capture.");

            RefreshAllCards();
        }

        void OnDestroy()
        {
            UnhookSaveManagerEvents();

            // [S7 L4] Unhook scene-change subscription to avoid leaks across reloads.
            SceneManager.activeSceneChanged -= HandleActiveSceneChanged;

            // Texture2D cleanup — entries own their own copies, but we also cleared cached ones here.
            if (_loadedThumbs != null)
            {
                for (int i = 0; i < _loadedThumbs.Length; i++)
                {
                    if (_loadedThumbs[i] != null)
                    {
                        Destroy(_loadedThumbs[i]);
                        _loadedThumbs[i] = null;
                    }
                }
            }
        }

        // ── Layout ──────────────────────────────────────────────────────────

        void BuildPanelLayout()
        {
            var rt = gameObject.GetComponent<RectTransform>();
            if (rt == null) rt = gameObject.AddComponent<RectTransform>();
            rt.sizeDelta = panelSize;

            var bg = gameObject.GetComponent<Image>();
            if (bg == null) bg = gameObject.AddComponent<Image>();
            bg.color = new Color(0.06f, 0.07f, 0.10f, 0.85f);

            var hlg = gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(20, 20, 20, 20);
            hlg.spacing = slotSpacing;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childAlignment = TextAnchor.MiddleCenter;

            for (int i = 0; i < slotCount; i++)
            {
                var go = new GameObject($"SlotCard_{i}", typeof(RectTransform));
                go.transform.SetParent(transform, false);
                var entry = go.AddComponent<SaveSlotEntry>();
                entry.BuildVisuals(i, OnLoadRequested, OnDeleteRequested);
                _entries.Add(entry);
            }
        }

        void BuildConfirmModal()
        {
            _confirmModalRoot = new GameObject("DeleteConfirmModal", typeof(RectTransform), typeof(Image));
            _confirmModalRoot.transform.SetParent(transform, false);

            var modalRt = _confirmModalRoot.GetComponent<RectTransform>();
            modalRt.anchorMin = Vector2.zero;
            modalRt.anchorMax = Vector2.one;
            modalRt.offsetMin = Vector2.zero;
            modalRt.offsetMax = Vector2.zero;
            _confirmModalRoot.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.85f);

            // Inner panel
            var inner = new GameObject("Inner", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
            inner.transform.SetParent(_confirmModalRoot.transform, false);
            var innerRt = inner.GetComponent<RectTransform>();
            innerRt.anchorMin = new Vector2(0.5f, 0.5f);
            innerRt.anchorMax = new Vector2(0.5f, 0.5f);
            innerRt.sizeDelta = new Vector2(420f, 180f);
            innerRt.anchoredPosition = Vector2.zero;
            inner.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.18f, 1f);
            var vlg = inner.GetComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(20, 20, 20, 20);
            vlg.spacing = 14f;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;

            var titleGo = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            titleGo.transform.SetParent(inner.transform, false);
            var titleText = titleGo.GetComponent<TextMeshProUGUI>();
            titleText.text = "Confirm Delete";
            titleText.fontSize = 20;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = Color.white;
            titleText.alignment = TextAlignmentOptions.Center;

            var bodyGo = new GameObject("Body", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            bodyGo.transform.SetParent(inner.transform, false);
            _confirmModalText = bodyGo.GetComponent<TextMeshProUGUI>();
            _confirmModalText.text = "Delete this save? This cannot be undone.";
            _confirmModalText.fontSize = 14;
            _confirmModalText.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            _confirmModalText.alignment = TextAlignmentOptions.Center;

            var btnRow = new GameObject("Buttons", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            btnRow.transform.SetParent(inner.transform, false);
            var hlg = btnRow.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10f;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            btnRow.GetComponent<LayoutElement>().minHeight = 40f;

            _confirmYesButton = MakeButton(btnRow.transform, "YesBtn", "DELETE", new Color(0.6f, 0.18f, 0.18f, 1f));
            _confirmNoButton  = MakeButton(btnRow.transform, "NoBtn",  "CANCEL", new Color(0.3f, 0.3f, 0.36f, 1f));

            _confirmYesButton.onClick.AddListener(OnConfirmYes);
            _confirmNoButton.onClick.AddListener(OnConfirmNo);

            _confirmModalRoot.SetActive(false);
        }

        static Button MakeButton(Transform parent, string name, string label, Color bgColor)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = bgColor;
            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(go.transform, false);
            var labelRt = labelGo.GetComponent<RectTransform>();
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = Vector2.zero;
            labelRt.offsetMax = Vector2.zero;
            var t = labelGo.GetComponent<TextMeshProUGUI>();
            t.text = label;
            t.alignment = TextAlignmentOptions.Center;
            t.fontSize = 14;
            t.fontStyle = FontStyles.Bold;
            t.color = Color.white;
            return go.GetComponent<Button>();
        }

        // ── SaveManager resolution + hooks ──────────────────────────────────

        SaveManager ResolveSaveManager()
        {
            var sm = SaveManager.Instance;
            if (sm != null) return sm;

            // Unity 6 API per docs/agents/API_CONTRACT.md
            sm = UnityEngine.Object.FindFirstObjectByType<SaveManager>(FindObjectsInactive.Include);
            if (sm == null)
            {
                Debug.LogWarning("[SaveSlotPanel] SaveManager not found in scene at Start. Cards will render placeholder data.");
            }
            return sm;
        }

        void HookSaveManagerEvents()
        {
            if (_saveManager == null)
            {
                _usingMtimePollFallback = true;
                Debug.LogWarning("[SaveSlotPanel] No SaveManager instance — using mtime POLL FALLBACK on every Update for thumbnail capture trigger. " +
                                 "Reason: SaveManager.Instance == null at Start.");
                return;
            }

            // Subscribe to events declared at Assets/_Project/Scripts/Save/SaveManager.cs:1376 (OnBeforeSave) + :1381 (OnAfterLoad)
            _saveManager.OnBeforeSave += HandleBeforeSave;
            _saveManager.OnAfterLoad  += HandleAfterLoad;
            _usingMtimePollFallback = false;
            Debug.Log("[SaveSlotPanel] Hooked SaveManager.OnBeforeSave + OnAfterLoad. Thumbnail captures will trigger on save events (no mtime poll).");
        }

        void UnhookSaveManagerEvents()
        {
            if (_saveManager != null)
            {
                _saveManager.OnBeforeSave -= HandleBeforeSave;
                _saveManager.OnAfterLoad  -= HandleAfterLoad;
            }
        }

        void HandleBeforeSave(SaveData _)
        {
            // OnBeforeSave fires before bytes hit disk. Defer capture until end-of-frame so
            // the screenshot reflects the gameplay frame the save was about (LateUpdate).
            int slot = _saveManager != null ? _saveManager.GetCurrentSlot() : -1;
            _capturePendingThisFrame = true;
            _capturePendingForSlot = slot;
        }

        void HandleAfterLoad(SaveData _)
        {
            // After load: just refresh cards (the loaded slot's metadata may differ from on-disk).
            RefreshAllCards();
        }

        // ── S7 L4: Scene-change capture ─────────────────────────────────────

        /// <summary>
        /// Fires on UnityEngine.SceneManagement.SceneManager.activeSceneChanged. Captures the
        /// last visible frame of the OUTGOING scene synchronously (no end-of-frame wait —
        /// the new scene is already mid-load so we can't yield) and writes it to the
        /// current slot's thumbnail path. This guarantees a thumbnail even if no save
        /// happened between scene transitions.
        /// </summary>
        void HandleActiveSceneChanged(Scene outgoing, Scene incoming)
        {
            int slot = _saveManager != null ? _saveManager.GetCurrentSlot() : -1;
            if (slot < 0)
            {
                Debug.LogWarning($"[SaveSlotPanel] activeSceneChanged ('{outgoing.name}' -> '{incoming.name}'): no SaveManager / slot, skipping cache thumb.");
                return;
            }

            Debug.Log($"[SaveSlotPanel] activeSceneChanged ('{outgoing.name}' -> '{incoming.name}') — caching slot {slot} thumbnail before new scene clobbers framebuffer.");
            CaptureAndWriteScreenshot(slot);
        }

        // ── Update / poll fallback ──────────────────────────────────────────

        void Update()
        {
            if (_usingMtimePollFallback && _saveManager == null)
            {
                _saveManager = SaveManager.Instance;
                if (_saveManager != null)
                {
                    Debug.Log("[SaveSlotPanel] SaveManager became available at runtime — switching from POLL FALLBACK to event hook.");
                    HookSaveManagerEvents();
                }
            }

            if (_usingMtimePollFallback)
            {
                PollSaveFileMtimesAndCaptureIfChanged();
            }
        }

        void LateUpdate()
        {
            if (_capturePendingThisFrame)
            {
                _capturePendingThisFrame = false;
                int slot = _capturePendingForSlot;
                _capturePendingForSlot = -1;
                StartCoroutine(CaptureAndWriteScreenshotCoroutine(slot));
            }
        }

        System.Collections.IEnumerator CaptureAndWriteScreenshotCoroutine(int slot)
        {
            yield return new WaitForEndOfFrame();
            CaptureAndWriteScreenshot(slot);
            // Also write the metadata sidecar now (after save has flushed).
            WriteMetadataSidecar(slot);
            // Refresh the cards so new thumbnail + metadata land.
            RefreshAllCards();
        }

        void PollSaveFileMtimesAndCaptureIfChanged()
        {
            for (int i = 0; i < slotCount; i++)
            {
                string p = Path.Combine(Application.persistentDataPath, $"save_slot_{i}.dat");
                if (!File.Exists(p)) continue;

                long ticks;
                try { ticks = File.GetLastWriteTimeUtc(p).Ticks; }
                catch (Exception e)
                {
                    Debug.LogWarning($"[SaveSlotPanel] mtime read failed for '{p}' (persistentDataPath='{Application.persistentDataPath}') at SaveSlotPanel.PollSaveFileMtimesAndCaptureIfChanged: {e.GetType().Name}: {e.Message}");
                    continue;
                }

                if (_lastMtimeTicks[i] == 0)
                {
                    _lastMtimeTicks[i] = ticks; // initialize without capturing
                    continue;
                }

                if (ticks != _lastMtimeTicks[i])
                {
                    _lastMtimeTicks[i] = ticks;
                    Debug.Log($"[SaveSlotPanel] POLL FALLBACK detected save mtime change for slot {i} — capturing screenshot.");
                    _capturePendingThisFrame = true;
                    _capturePendingForSlot = i;
                }
            }
        }

        // ── Screenshot capture ──────────────────────────────────────────────

        void CaptureAndWriteScreenshot(int slot)
        {
            if (slot < 0)
            {
                Debug.LogWarning("[SaveSlotPanel] CaptureAndWriteScreenshot called with negative slot; skipping.");
                return;
            }

            Texture2D shot = null;
            try
            {
                shot = ScreenCapture.CaptureScreenshotAsTexture();
                if (shot == null)
                {
                    Debug.LogError($"[SaveSlotPanel] ScreenCapture.CaptureScreenshotAsTexture returned null for slot {slot} (persistentDataPath='{Application.persistentDataPath}').");
                    return;
                }

                // Centralized PNG encode + size-knob downscale.
                string path = GetThumbnailPath(slot);
                int finalBytes = EncodeAndWritePngWithSizeKnob(shot, path, $"slot {slot}");
                if (finalBytes <= 0)
                {
                    Debug.LogError($"[SaveSlotPanel] EncodeAndWritePngWithSizeKnob returned {finalBytes} for slot {slot} (persistentDataPath='{Application.persistentDataPath}').");
                    return;
                }

                Debug.Log($"[SaveSlotPanel] Wrote slot {slot} thumbnail ({finalBytes} bytes) -> {path}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSlotPanel] Screenshot capture/write failed for slot {slot} (persistentDataPath='{Application.persistentDataPath}'): {e.GetType().Name}: {e.Message}\n{e.StackTrace}");
            }
            finally
            {
                // Capture texture is owned here; destroy after encode so we don't leak.
                if (shot != null) Destroy(shot);
            }
        }

        /// <summary>
        /// [S7 L4] Public so the Editor menu (SaveThumbnailMenu) can reuse the exact same
        /// encode+size-knob logic. Returns final byte count written (or 0 on failure).
        /// </summary>
        public static int EncodeAndWritePngWithSizeKnob(Texture2D source, string outputPath, string contextLabel)
        {
            if (source == null)
            {
                Debug.LogError($"[SaveSlotPanel] EncodeAndWritePngWithSizeKnob({contextLabel}): source texture is null (persistentDataPath='{Application.persistentDataPath}').");
                return 0;
            }

            byte[] png;
            try { png = source.EncodeToPNG(); }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSlotPanel] EncodeToPNG threw for {contextLabel} (persistentDataPath='{Application.persistentDataPath}'): {e.GetType().Name}: {e.Message}");
                return 0;
            }

            if (png == null || png.Length == 0)
            {
                Debug.LogError($"[SaveSlotPanel] EncodeToPNG returned empty for {contextLabel} ({source.width}x{source.height}) (persistentDataPath='{Application.persistentDataPath}').");
                return 0;
            }

            bool downscaled = false;
            if (png.Length > MaxThumbnailBytes)
            {
                // LOUD log so we can spot the threshold trigger in the console.
                Debug.LogWarning($"[SaveSlotPanel] ==== PNG SIZE KNOB TRIGGERED ==== {contextLabel}: encoded PNG was {png.Length} bytes > {MaxThumbnailBytes} threshold. Downscaling to half-res via Graphics.Blit + RenderTexture and re-encoding.");
                Texture2D half = DownscaleHalf(source, contextLabel);
                if (half != null)
                {
                    try
                    {
                        byte[] halfPng = half.EncodeToPNG();
                        if (halfPng != null && halfPng.Length > 0)
                        {
                            Debug.LogWarning($"[SaveSlotPanel] PNG SIZE KNOB result for {contextLabel}: {png.Length} bytes -> {halfPng.Length} bytes after half-res ({source.width}x{source.height} -> {half.width}x{half.height}).");
                            png = halfPng;
                            downscaled = true;
                        }
                        else
                        {
                            Debug.LogError($"[SaveSlotPanel] Downscaled EncodeToPNG returned empty for {contextLabel}; keeping original {png.Length}-byte PNG.");
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[SaveSlotPanel] Downscaled EncodeToPNG threw for {contextLabel} (persistentDataPath='{Application.persistentDataPath}'): {e.GetType().Name}: {e.Message}");
                    }
                    finally
                    {
                        // Half-res texture is owned here.
                        if (Application.isPlaying) UnityEngine.Object.Destroy(half);
                        else UnityEngine.Object.DestroyImmediate(half);
                    }
                }
            }

            try
            {
                // Ensure parent dir exists.
                string parent = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(parent) && !Directory.Exists(parent)) Directory.CreateDirectory(parent);
                File.WriteAllBytes(outputPath, png);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSlotPanel] WriteAllBytes failed for {contextLabel} -> '{outputPath}' (persistentDataPath='{Application.persistentDataPath}'): {e.GetType().Name}: {e.Message}");
                return 0;
            }

            Debug.Log($"[SaveSlotPanel] PNG write OK for {contextLabel}: {png.Length} bytes (downscaled={downscaled}) -> {outputPath}");
            return png.Length;
        }

        /// <summary>
        /// Downscale a Texture2D to half resolution using Graphics.Blit + RenderTexture.
        /// Caller owns the returned texture and must Destroy() it.
        /// </summary>
        static Texture2D DownscaleHalf(Texture2D source, string contextLabel)
        {
            int w = Mathf.Max(1, source.width / 2);
            int h = Mathf.Max(1, source.height / 2);

            RenderTexture rt = null;
            RenderTexture prevActive = RenderTexture.active;
            try
            {
                rt = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32);
                rt.filterMode = FilterMode.Bilinear;
                Graphics.Blit(source, rt);
                RenderTexture.active = rt;

                var dst = new Texture2D(w, h, TextureFormat.RGBA32, false);
                dst.ReadPixels(new Rect(0, 0, w, h), 0, 0, false);
                dst.Apply(false, false);
                return dst;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSlotPanel] DownscaleHalf failed for {contextLabel} (persistentDataPath='{Application.persistentDataPath}'): {e.GetType().Name}: {e.Message}");
                return null;
            }
            finally
            {
                RenderTexture.active = prevActive;
                if (rt != null) RenderTexture.ReleaseTemporary(rt);
            }
        }

        void WriteMetadataSidecar(int slot)
        {
            if (_saveManager == null)
            {
                Debug.LogWarning($"[SaveSlotPanel] WriteMetadataSidecar(slot {slot}) skipped — SaveManager null.");
                return;
            }

            var save = _saveManager.CurrentSave;
            if (save == null)
            {
                Debug.LogWarning($"[SaveSlotPanel] WriteMetadataSidecar(slot {slot}) skipped — SaveManager.CurrentSave null.");
                return;
            }

            var meta = new SaveSlotMetadata
            {
                slot = slot,
                currentMoon = save.header?.currentMoon ?? 1,
                moonName = ResolveMoonName(save.header?.currentMoon ?? 1),
                resonanceShards = save.economy?.aetherShards ?? 0,
                buildingsRestored = save.header?.buildingsRestored ?? 0,
                capturedUtc = DateTime.UtcNow.ToString("o")
            };

            try
            {
                string json = JsonUtility.ToJson(meta, true);
                File.WriteAllText(GetMetadataPath(slot), json);
                Debug.Log($"[SaveSlotPanel] Wrote slot {slot} metadata sidecar: moon={meta.moonName}, shards={meta.resonanceShards}, buildings={meta.buildingsRestored}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSlotPanel] Metadata sidecar write failed for slot {slot} at '{GetMetadataPath(slot)}' (persistentDataPath='{Application.persistentDataPath}'): {e.GetType().Name}: {e.Message}");
            }
        }

        static string ResolveMoonName(int moonNum)
        {
            // Canonical Moon names (matches docs/03_CAMPAIGN_13_MOONS.md). Falls back to "Moon N".
            switch (moonNum)
            {
                case 1:  return "Moon 1 - Echohaven";
                case 2:  return "Moon 2 - Tideheart";
                case 3:  return "Moon 3 - Orphan Train";
                case 4:  return "Moon 4 - 17th Hour";
                case 5:  return "Moon 5 - Crystal Caverns";
                case 6:  return "Moon 6 - Sky Citadel";
                case 7:  return "Moon 7 - Bell Tower";
                case 8:  return "Moon 8 - Airship Fleet";
                case 9:  return "Moon 9 - Ley Line Prophecy";
                case 10: return "Moon 10 - Aquifer Purge";
                case 11: return "Moon 11 - Cosmic Convergence";
                case 12: return "Moon 12 - Day Out of Time";
                case 13: return "Moon 13 - Aether Awakening";
                default: return $"Moon {moonNum}";
            }
        }

        // ── Refresh ─────────────────────────────────────────────────────────

        public void RefreshAllCards()
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                RefreshCard(i);
            }
        }

        void RefreshCard(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _entries.Count) return;

            SaveSlotInfo info;
            if (_saveManager != null)
            {
                info = _saveManager.GetSaveInfo(slotIndex);
            }
            else
            {
                // No SaveManager — synthesize a minimal "doesn't exist" info.
                info = new SaveSlotInfo { slot = slotIndex, exists = false };
                Debug.LogWarning($"[SaveSlotPanel] RefreshCard({slotIndex}): SaveManager null; rendering empty placeholder.");
            }

            SaveSlotMetadata meta = LoadMetadataSidecar(slotIndex);
            Texture2D thumb = LoadThumbnail(slotIndex);

            _entries[slotIndex].Populate(info, meta, thumb);
        }

        SaveSlotMetadata LoadMetadataSidecar(int slot)
        {
            string path = GetMetadataPath(slot);
            if (!File.Exists(path)) return null;

            try
            {
                string json = File.ReadAllText(path);
                return JsonUtility.FromJson<SaveSlotMetadata>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSlotPanel] Metadata sidecar read failed for slot {slot} at '{path}' (persistentDataPath='{Application.persistentDataPath}'): {e.GetType().Name}: {e.Message}");
                return null;
            }
        }

        Texture2D LoadThumbnail(int slot)
        {
            // Destroy any previously cached texture for this slot before loading new one (dispose pattern).
            if (_loadedThumbs[slot] != null)
            {
                Destroy(_loadedThumbs[slot]);
                _loadedThumbs[slot] = null;
            }

            string path = GetThumbnailPath(slot);
            if (!File.Exists(path)) return null;

            try
            {
                byte[] bytes = File.ReadAllBytes(path);
                var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (!tex.LoadImage(bytes))
                {
                    Debug.LogWarning($"[SaveSlotPanel] LoadImage failed for slot {slot} thumbnail '{path}' ({bytes.Length} bytes).");
                    Destroy(tex);
                    return null;
                }
                _loadedThumbs[slot] = tex;
                return tex;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSlotPanel] Thumbnail read failed for slot {slot} at '{path}' (persistentDataPath='{Application.persistentDataPath}'): {e.GetType().Name}: {e.Message}");
                return null;
            }
        }

        // ── Card callbacks ──────────────────────────────────────────────────

        void OnLoadRequested(int slot)
        {
            if (_saveManager == null)
            {
                Debug.LogError($"[SaveSlotPanel] OnLoadRequested(slot {slot}) — SaveManager null. Cannot load.");
                return;
            }

            // Canonical API: SaveManager.SwitchToSlot(int) at Assets/_Project/Scripts/Save/SaveManager.cs:595
            // (no LoadSlot exists — SwitchToSlot updates paths AND reloads via LoadOrCreate).
            Debug.Log($"[SaveSlotPanel] Load requested for slot {slot} — invoking SaveManager.SwitchToSlot({slot}).");
            _saveManager.SwitchToSlot(slot);
        }

        void OnDeleteRequested(int slot)
        {
            // Build a runtime confirm modal (replaces EditorUtility.DisplayDialog for build/play time).
            _pendingDeleteSlot = slot;
            _confirmModalText.text = $"Delete save slot {slot}? This cannot be undone.";
            _confirmModalRoot.SetActive(true);
            _confirmModalRoot.transform.SetAsLastSibling();
        }

        void OnConfirmYes()
        {
            int slot = _pendingDeleteSlot;
            _pendingDeleteSlot = -1;
            _confirmModalRoot.SetActive(false);

            if (slot < 0)
            {
                Debug.LogWarning("[SaveSlotPanel] OnConfirmYes invoked with no pending slot.");
                return;
            }

            if (_saveManager == null)
            {
                Debug.LogError($"[SaveSlotPanel] OnConfirmYes(slot {slot}) — SaveManager null. Cannot delete.");
                return;
            }

            // Canonical API: SaveManager.DeleteSlot(int) at Assets/_Project/Scripts/Save/SaveManager.cs:693
            Debug.Log($"[SaveSlotPanel] Confirmed delete for slot {slot} — invoking SaveManager.DeleteSlot({slot}).");
            _saveManager.DeleteSlot(slot);

            // Also remove the sidecar + thumbnail so card visually clears.
            TryDeleteFile(GetThumbnailPath(slot));
            TryDeleteFile(GetMetadataPath(slot));

            RefreshAllCards();
        }

        void OnConfirmNo()
        {
            Debug.Log($"[SaveSlotPanel] Delete cancelled for slot {_pendingDeleteSlot}.");
            _pendingDeleteSlot = -1;
            _confirmModalRoot.SetActive(false);
        }

        static void TryDeleteFile(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            try
            {
                if (File.Exists(path)) File.Delete(path);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSlotPanel] Failed to delete '{path}' (persistentDataPath='{Application.persistentDataPath}'): {e.GetType().Name}: {e.Message}");
            }
        }
    }
}
