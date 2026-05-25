using UnityEngine;
using System.Collections;
using Tartaria.Core;
using Tartaria.Gameplay;
using Tartaria.Input;
using Tartaria.Save;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 7 ice thaw multi-session mechanics.
    /// Korath awakening requires 3 thaw sessions with RS channeling.
    /// Each session partially melts the Aether ice and reveals memory fragments.
    /// </summary>
    public class KorathIceThawSystem : MonoBehaviour, IInteractable
    {
        public static KorathIceThawSystem Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] int totalSessions = 3;
        [SerializeField] float sessionDuration = 8f;     // 8 seconds per thaw session
        [SerializeField] float rsChannelRate = 50f;      // 50 RS per second required

        [Header("State")]
        [SerializeField] int completedSessions = 0;
        [SerializeField] bool isThawing = false;
        [SerializeField] bool isFullyThawed = false;

        GameObject _iceBlock;
        Material _iceMaterial;
        Light _iceLight;

        float _sessionProgress = 0f;

        readonly string[] _memoryFragments = {
            "Korath (muffled): 'You... came. A small spark... carrying the old fire.'",
            "Korath (clearer): 'Maelix... my brother... where is Maelix?'",
            "Korath (resonant): 'The grid... it still sleeps. Let me WAKE it.'"
        };

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            _iceBlock = Moon7ContentSpawner.Instance?.GetKorathIceBlock();
            if (_iceBlock != null)
            {
                _iceMaterial = _iceBlock.GetComponent<Renderer>()?.material;
                _iceLight = _iceBlock.GetComponent<Light>();
            }

            UpdateIceVisual();
        }

        public string GetInteractPrompt()
        {
            if (isFullyThawed) return "Korath Awakened ✓";
            if (isThawing) return $"Channeling RS... {_sessionProgress / sessionDuration:P0}";
            return $"[E] Thaw Korath (Session {completedSessions + 1}/{totalSessions})";
        }

        public void Interact(GameObject player)
        {
            if (isFullyThawed || isThawing) return;

            // Check if player has enough RS capacity
            var abilities = player.GetComponent<PlayerAbilities>();
            if (abilities == null) return;

            StartCoroutine(ThawSession(abilities));
        }

        IEnumerator ThawSession(PlayerAbilities playerAbilities)
        {
            isThawing = true;
            _sessionProgress = 0f;

            Debug.Log($"[KorathThaw] Session {completedSessions + 1} started — channeling RS into ice...");
            GameEvents.RaiseHUDShowObjective($"Channeling RS into Aether ice... ({completedSessions + 1}/{totalSessions})");

            // VFX: RS energy flows from player to ice
            GameObject energyFlow = CreateRSFlowVFX();

            while (_sessionProgress < sessionDuration)
            {
                // Drain player RS
                float rsDrain = rsChannelRate * Time.deltaTime;
                if (!playerAbilities.ConsumeRS(rsDrain))
                {
                    // Player out of RS — abort session
                    Debug.Log("[KorathThaw] RS depleted! Session failed.");
                    GameEvents.RaiseHUDShowObjective("Insufficient RS! Wait for regeneration and try again.");
                    Audio.AudioManager.Instance?.PlaySFX2D("ThawFail");
                    
                    Destroy(energyFlow);
                    isThawing = false;
                    _sessionProgress = 0f;
                    yield break;
                }

                _sessionProgress += Time.deltaTime;

                // Update ice opacity (gradually more transparent)
                UpdateIceVisual();

                yield return null;
            }

            // Session complete!
            Destroy(energyFlow);
            isThawing = false;
            completedSessions++;

            Debug.Log($"[KorathThaw] Session {completedSessions} complete!");

            // Show memory fragment
            string memory = _memoryFragments[completedSessions - 1];
            GameEvents.RaiseHUDShowDialogue("Korath", memory);
            DialogueManager.Instance?.PlayContextDialogue($"korath_thaw_{completedSessions}");

            Audio.AudioManager.Instance?.PlaySFX2D("ThawSuccess");

            yield return new WaitForSeconds(3f);

            // Check if fully thawed
            if (completedSessions >= totalSessions)
            {
                FullyThaw();
            }
            else
            {
                GameEvents.RaiseHUDShowObjective($"Ice partially thawed. Return when ready for session {completedSessions + 1}.");
                
                // Save progress
                SaveManager.Instance?.SetGameFlag($"korath_thaw_session_{completedSessions}", true);
            }
        }

        void FullyThaw()
        {
            isFullyThawed = true;

            Debug.Log("[KorathThaw] KORATH FULLY AWAKENED!");
            GameEvents.RaiseHUDShowObjective("⚡⚡⚡ KORATH AWAKENS! ⚡⚡⚡");

            // Destroy ice block
            if (_iceBlock != null)
            {
                StartCoroutine(DissolveIce());
            }

            // Spawn awakened Korath
            KorathCompanionController.Instance?.SpawnAwakened();

            // Achievement
            AchievementSystem.Instance?.Unlock("korath_awakening");

            // Quest completion
            QuestManager.Instance?.CompleteQuest("moon7_korath_awakening");
        }

        IEnumerator DissolveIce()
        {
            float duration = 2f;
            float elapsed = 0f;

            Color startColor = _iceMaterial.color;
            Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                _iceMaterial.color = Color.Lerp(startColor, endColor, t);

                if (_iceLight != null)
                    _iceLight.intensity = Mathf.Lerp(3f, 0f, t);

                elapsed += Time.deltaTime;
                yield return null;
            }

            Destroy(_iceBlock);
        }

        void UpdateIceVisual()
        {
            if (_iceMaterial == null) return;

            // Ice becomes more transparent with each session + current progress
            float totalProgress = completedSessions + (_sessionProgress / sessionDuration);
            float alpha = Mathf.Lerp(0.7f, 0.1f, totalProgress / totalSessions);

            Color iceColor = new Color(0.6f, 0.4f, 0.9f, alpha); // Violet-aurora ice
            _iceMaterial.color = iceColor;

            // Light intensity increases as thaw progresses
            if (_iceLight != null)
                _iceLight.intensity = Mathf.Lerp(3f, 6f, totalProgress / totalSessions);
        }

        GameObject CreateRSFlowVFX()
        {
            GameObject vfx = new GameObject("RS_Flow_VFX");
            vfx.transform.position = transform.position;

            ParticleSystem ps = vfx.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 1.5f;
            main.startSpeed = 5f;
            main.startSize = 0.3f;
            main.startColor = new Color(1f, 0.8f, 0.2f); // Golden RS energy
            main.loop = true;
            main.maxParticles = 100;

            var emission = ps.emission;
            emission.rateOverTime = 50f;

            return vfx;
        }

        public int CompletedSessions => completedSessions;
        public bool IsFullyThawed => isFullyThawed;

        public void LoadState(int sessions)
        {
            completedSessions = Mathf.Clamp(sessions, 0, totalSessions);
            if (completedSessions >= totalSessions)
            {
                isFullyThawed = true;
            }
            UpdateIceVisual();
        }
    }

    /// <summary>
    /// 9-band aurora hum visualization for Moon 7.
    /// Violet-aurora energy field with 9 harmonic bands.
    /// </summary>
    public class NineBandAuroraHum : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] float radius = 50f;
        [SerializeField] int bandCount = 9;

        GameObject[] _bandRings;
        Light _auroraLight;

        void Start()
        {
            CreateAuroraVisuals();
        }

        void CreateAuroraVisuals()
        {
            _bandRings = new GameObject[bandCount];

            for (int i = 0; i < bandCount; i++)
            {
                GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                ring.name = $"AuroraBand_{i}";
                ring.transform.SetParent(transform);
                
                float ringRadius = radius * (i + 1) / (float)bandCount;
                ring.transform.localPosition = new Vector3(0f, i * 2f, 0f);
                ring.transform.localScale = new Vector3(ringRadius * 2f, 0.1f, ringRadius * 2f);

                // Violet-aurora gradient (darker to lighter)
                float bandIntensity = (i + 1) / (float)bandCount;
                Color bandColor = Color.Lerp(new Color(0.4f, 0.2f, 0.6f), new Color(0.8f, 0.6f, 1f), bandIntensity);
                bandColor.a = 0.3f;

                Renderer rend = ring.GetComponent<Renderer>();
                rend.material.color = bandColor;

                _bandRings[i] = ring;
            }

            // Central aurora light
            _auroraLight = gameObject.AddComponent<Light>();
            _auroraLight.type = LightType.Point;
            _auroraLight.color = new Color(0.7f, 0.5f, 1f);
            _auroraLight.range = radius * 2f;
            _auroraLight.intensity = 5f;

            Debug.Log("[9BandAurora] Aurora hum visualization created (9 harmonic bands)");
        }

        void Update()
        {
            if (_bandRings == null) return;

            // Gentle pulsing rotation
            float rotationSpeed = 5f;
            for (int i = 0; i < _bandRings.Length; i++)
            {
                if (_bandRings[i] != null)
                {
                    float speed = rotationSpeed * (i % 2 == 0 ? 1f : -1f); // Alternating directions
                    _bandRings[i].transform.Rotate(Vector3.up, speed * Time.deltaTime);
                }
            }

            // Light intensity pulse
            if (_auroraLight != null)
            {
                _auroraLight.intensity = 5f + Mathf.Sin(Time.time * 2f) * 1f;
            }
        }
    }
}
