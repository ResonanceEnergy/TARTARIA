using UnityEngine;

namespace Tartaria.UI
{
    /// <summary>
    /// 2026-05-30 playtest fix: previously this MonoBehaviour drew its own IMGUI
    /// pause overlay, but PauseAndGameOverMenu also drew one — both listened to
    /// Esc, both fought for input, neither one accepted clicks reliably. This
    /// class is now a stub kept only so external references compile. The actual
    /// in-game pause overlay is PauseAndGameOverMenu.
    /// </summary>
    [DisallowMultipleComponent]
    public class PauseMenu : MonoBehaviour
    {
        public static PauseMenu Instance { get; private set; }
        public void Toggle() { /* no-op stub */ }
        public void Open()   { /* no-op stub */ }
        public void Close()  { /* no-op stub */ }
    }
}
