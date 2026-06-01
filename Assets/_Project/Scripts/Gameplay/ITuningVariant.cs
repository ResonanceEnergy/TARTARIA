using System;
using UnityEngine;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// ITuningVariant — common surface area for all 3 tuning mini-game variants
    /// (Slider, Waveform, Pattern) per docs/15_MVP_BUILD_SPEC.md §9.
    ///
    /// InteractableBuilding picks one of the 3 implementations per node, calls
    /// StartTuning, and listens on OnTuningComplete(accuracy) / OnTuningFailed.
    /// Each variant auto-builds its own UI canvas on first StartTuning.
    /// </summary>
    public interface ITuningVariant
    {
        event Action<float> OnTuningComplete;
        event Action OnTuningFailed;
        event Action<float> OnFrequencyChanged;

        bool IsActive { get; }
        float CurrentAccuracy { get; }

        void StartTuning(Vector3 nodePosition, System.Action onComplete);
    }
}
