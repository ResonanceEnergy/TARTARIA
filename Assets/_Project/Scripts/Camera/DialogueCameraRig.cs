using UnityEngine;
using System.Reflection;

namespace Tartaria.Camera
{
    /// <summary>
    /// Dialogue camera rig — smooth-frames between speaker shoulder-cam
    /// and listener reaction-cam every 3s when DialogueManager.IsActive.
    /// Lerps FOV 35→45. Restores main camera on dialogue end.
    /// Uses a transient Camera GameObject to avoid Cinemachine dependency.
    /// </summary>
    [DisallowMultipleComponent]
    public class DialogueCameraRig : MonoBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField] float lerpSpeed = 2.5f;
        [SerializeField] float minFOV = 35f;
        [SerializeField] float maxFOV = 45f;
        [SerializeField] float switchInterval = 3f;

        [Header("Shoulder Offset")]
        [SerializeField] Vector3 shoulderOffset = new(0.6f, 1.5f, -1.2f);
        [SerializeField] Vector3 reactionOffset = new(-0.6f, 1.5f, -1.2f);

        UnityEngine.Camera _dialogueCam;
        UnityEngine.Camera _mainCam;
        bool _isActive;
        float _nextSwitchTime;
        bool _onSpeaker = true; // true = shoulder-cam, false = reaction-cam

        object _dialogueManagerInstance;
        PropertyInfo _isActiveProp;
        System.Type _dmType;
        PropertyInfo _dmInstanceProp;
        float _nextResolveAttempt;
        bool _resolveWarned;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            var go = new GameObject("DialogueCameraRig");
            DontDestroyOnLoad(go);
            go.AddComponent<DialogueCameraRig>();
        }

        void Awake()
        {
            // Resolve DialogueManager type + Instance prop once; instance lookup is lazy.
            _dmType = System.Type.GetType("Tartaria.Integration.DialogueManager, Tartaria.Integration");
            if (_dmType != null)
            {
                _dmInstanceProp = _dmType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                _isActiveProp = _dmType.GetProperty("IsPlaying", BindingFlags.Public | BindingFlags.Instance);
            }
        }

        void TryResolveDialogueManager()
        {
            if (_dialogueManagerInstance != null || _dmInstanceProp == null) return;
            try { _dialogueManagerInstance = _dmInstanceProp.GetValue(null); } catch { /* not ready */ }
        }

        void LateUpdate()
        {
            bool shouldBeActive = GetDialogueActive();

            if (shouldBeActive && !_isActive)
                ActivateDialogueCam();
            else if (!shouldBeActive && _isActive)
                DeactivateDialogueCam();

            if (_isActive)
                UpdateDialogueCam();
        }

        bool GetDialogueActive()
        {
            if (_dialogueManagerInstance == null && Time.time >= _nextResolveAttempt)
            {
                _nextResolveAttempt = Time.time + 1f;
                TryResolveDialogueManager();
                if (_dialogueManagerInstance == null && !_resolveWarned && Time.time > 8f)
                {
                    _resolveWarned = true;
                    Debug.Log("[DialogueCameraRig] DialogueManager.Instance not present \u2014 dialogue camera will stay inactive.");
                }
            }

            if (_isActiveProp == null || _dialogueManagerInstance == null)
                return false;

            try
            {
                return (bool)_isActiveProp.GetValue(_dialogueManagerInstance);
            }
            catch
            {
                return false;
            }
        }

        void ActivateDialogueCam()
        {
            _isActive = true;
            _onSpeaker = true;
            _nextSwitchTime = Time.time + switchInterval;

            // Create transient dialogue camera
            var camGO = new GameObject("DialogueCamera_Transient");
            _dialogueCam = camGO.AddComponent<UnityEngine.Camera>();
            _dialogueCam.fieldOfView = minFOV;
            _dialogueCam.nearClipPlane = 0.1f;
            _dialogueCam.farClipPlane = 1000f;

            // Find and disable main camera
            _mainCam = UnityEngine.Camera.main;
            if (_mainCam != null)
                _mainCam.enabled = false;

            Debug.Log("[DialogueCameraRig] Activated dialogue camera.");
        }

        void DeactivateDialogueCam()
        {
            _isActive = false;

            if (_dialogueCam != null)
            {
                Destroy(_dialogueCam.gameObject);
                _dialogueCam = null;
            }

            if (_mainCam != null)
                _mainCam.enabled = true;

            Debug.Log("[DialogueCameraRig] Deactivated dialogue camera, restored main cam.");
        }

        void UpdateDialogueCam()
        {
            if (_dialogueCam == null) return;

            // Switch between speaker/listener every 3s
            if (Time.time >= _nextSwitchTime)
            {
                _onSpeaker = !_onSpeaker;
                _nextSwitchTime = Time.time + switchInterval;
            }

            // Find player (speaker in most cases)
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogWarning("[DialogueCameraRig] No Player found, camera idle.");
                return;
            }

            // Target position + rotation
            Vector3 targetPos = player.transform.position + (_onSpeaker ? shoulderOffset : reactionOffset);
            Quaternion targetRot = Quaternion.LookRotation(player.transform.forward);
            float targetFOV = _onSpeaker ? minFOV : maxFOV;

            // Smooth lerp
            _dialogueCam.transform.position = Vector3.Lerp(_dialogueCam.transform.position, targetPos, Time.deltaTime * lerpSpeed);
            _dialogueCam.transform.rotation = Quaternion.Slerp(_dialogueCam.transform.rotation, targetRot, Time.deltaTime * lerpSpeed);
            _dialogueCam.fieldOfView = Mathf.Lerp(_dialogueCam.fieldOfView, targetFOV, Time.deltaTime * lerpSpeed);
        }
    }
}
