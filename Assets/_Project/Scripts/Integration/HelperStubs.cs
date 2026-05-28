using UnityEngine;

namespace Tartaria.UI
{
    /// <summary>
    /// HUDController - Stub for showing banners and prompts.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        public static HUDController Instance { get; private set; }
        void Awake() { Instance = this; }
        public void ShowBanner(string title, string subtitle) => Debug.Log($"[HUD] {title}: {subtitle}");
        public void ShowInteractionPrompt(string text) => Debug.Log($"[HUD] Prompt: {text}");
    }
}

namespace Tartaria.Camera
{
    /// <summary>
    /// CameraShakeController - Stub for camera shake effects.
    /// </summary>
    public class CameraShakeController : MonoBehaviour
    {
        public static CameraShakeController Instance { get; private set; }
        void Awake() { Instance = this; }
        public void Shake(float intensity, float duration) => Debug.Log($"[CameraShake] {intensity} for {duration}s");
    }
}

namespace Tartaria.Integration
{
    /// <summary>
    /// DialogueManager - Stub for showing dialogue bubbles.
    /// </summary>
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }
        void Awake() { Instance = this; }
        public void ShowDialogue(string speaker, string line, float duration) => Debug.Log($"[{speaker}]: {line}");
    }
}
