using UnityEngine;

namespace Tartaria.UI
{
    /// <summary>
    /// 2026-05-30 playtest fix: stubbed out — see PauseMenu.cs.
    /// PauseAndGameOverMenu is the single source of truth for in-game pause.
    /// </summary>
    [DisallowMultipleComponent]
    public class PauseOverlay : MonoBehaviour
    {
        public static PauseOverlay Instance { get; private set; }
        public void Toggle() { /* no-op stub */ }
        public void Open()   { /* no-op stub */ }
        public void Close()  { /* no-op stub */ }
    }
}
