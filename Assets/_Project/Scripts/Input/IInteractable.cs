using UnityEngine;

namespace Tartaria.Input
{
    /// <summary>
    /// Canonical interaction contract used by PlayerInputHandler and all interactable
    /// MonoBehaviours across the Gameplay/Integration tiers. Lives in Tartaria.Input so
    /// that downstream asmdefs (Gameplay, Integration) reference a single source of truth
    /// without creating an asmdef cycle back into Input.
    /// </summary>
    public interface IInteractable
    {
        void Interact(GameObject player);
        string GetInteractPrompt();
    }
}
