using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// PipeOrganMiniGame - Complex musical puzzle minigame.
    /// Player must play correct melody to restore cathedral organ.
    /// </summary>
    public class PipeOrganMiniGame : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private List<int> targetMelody = new() { 0, 2, 4, 5, 7 }; // C, E, G, A, B
        [SerializeField] private List<int> playerMelody = new();
        [SerializeField] private bool isPlaying = false;

        public void StartGame(System.Action onComplete)
        {
            isPlaying = true;
            playerMelody.Clear();
            Debug.Log("[PipeOrgan] Play the correct melody!");
        }

        public void PlayNote(int note)
        {
            if (!isPlaying) return;
            playerMelody.Add(note);
            AudioFeedbackController.Instance?.PlaySFX($"OrganNote{note}", Vector3.zero);

            if (playerMelody.Count >= targetMelody.Count)
            {
                CheckMelody();
            }
        }

        void CheckMelody()
        {
            bool correct = true;
            for (int i = 0; i < targetMelody.Count; i++)
            {
                if (playerMelody[i] != targetMelody[i])
                {
                    correct = false;
                    break;
                }
            }

            if (correct)
            {
                Debug.Log("[PipeOrgan] ✅ Melody correct!");
                HUDController.Instance?.ShowBanner("MELODY COMPLETE!", "The organ sings once more");
            }
            else
            {
                Debug.Log("[PipeOrgan] ❌ Melody incorrect");
                playerMelody.Clear();
            }
        }
    }
}
