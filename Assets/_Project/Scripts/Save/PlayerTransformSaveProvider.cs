using System;
using UnityEngine;

namespace Tartaria.Save
{
    /// <summary>
    /// PlayerTransformSaveProvider — captures player position + rotation across
    /// scene loads. Self-registers as <see cref="ISaveDataProvider"/> in Awake.
    ///
    /// Per HANDOFFS 2026-06-01 22:30 → Systems Architect (save-load-hardening).
    ///
    /// PlayerHealthController already covers HP and QuestManager already covers
    /// quest state. The remaining gap was player world-space pose. This adds an
    /// MB you drop on the Player root (or auto-bootstrap below grabs it by tag).
    /// On <see cref="RestoreSaveData"/> we set transform directly — no
    /// CharacterController.Move dance — because the player root is teleported
    /// once on load.
    /// </summary>
    [DefaultExecutionOrder(-10)]
    public class PlayerTransformSaveProvider : MonoBehaviour, ISaveDataProvider
    {
        const string PlayerTag = "Player";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoAttach()
        {
            var existing = FindObjectOfType<PlayerTransformSaveProvider>();
            if (existing != null) return;
            var player = GameObject.FindGameObjectWithTag(PlayerTag);
            if (player == null) return;
            player.AddComponent<PlayerTransformSaveProvider>();
        }

        void Awake()
        {
            SaveManager.Instance?.RegisterProvider(this);
        }

        void OnDestroy()
        {
            SaveManager.Instance?.UnregisterProvider(this);
        }

        public string GetProviderKey() => "PlayerTransform";

        public object GetSaveData()
        {
            var t = transform;
            return new PlayerTransformData
            {
                posX = t.position.x,
                posY = t.position.y,
                posZ = t.position.z,
                rotY = t.eulerAngles.y,
                sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
            };
        }

        public void RestoreSaveData(object data)
        {
            if (data is not PlayerTransformData ptd)
            {
                Debug.LogWarning("[PlayerTransformSaveProvider] Restore skipped --- payload missing or wrong type.");
                return;
            }
            // Disable CharacterController so direct transform writes take effect.
            var cc = GetComponent<CharacterController>();
            bool wasEnabled = cc != null && cc.enabled;
            if (wasEnabled) cc.enabled = false;

            transform.position = new Vector3(ptd.posX, ptd.posY, ptd.posZ);
            transform.rotation = Quaternion.Euler(0f, ptd.rotY, 0f);

            if (wasEnabled) cc.enabled = true;
        }

        [Serializable]
        public class PlayerTransformData
        {
            public float posX;
            public float posY;
            public float posZ;
            public float rotY;
            public string sceneName;
        }
    }
}
