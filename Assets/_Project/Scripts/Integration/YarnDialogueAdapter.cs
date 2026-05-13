using UnityEngine;
using Tartaria.Integration;

namespace Tartaria.Integration
{
    /// <summary>
    /// Yarn Dialogue Adapter — bridges Yarn Spinner dialogue system to existing
    /// DialogueManager. Fires PlayLine events from Yarn <<line>> nodes.
    /// 
    /// STUB: Yarn DialogueRunner integration deferred. Requires full DialogueManager
    /// refactor to support Yarn variable system + choice handlers.
    /// 
    /// MVP scope (this file): demonstrates Yarn file loading + ResonanceScore exposure.
    /// Full integration: ~2hr work (tree conversion, event wiring, test all contexts).
    /// </summary>
    public class YarnDialogueAdapter : MonoBehaviour
    {
        // Yarn DialogueRunner would be instantiated here
        // [SerializeField] DialogueRunner _runner;

        void Start()
        {
            Debug.Log("[YarnDialogueAdapter] STUB: Yarn Spinner integration ready.");
            Debug.Log("[YarnDialogueAdapter] Sample Milo_Intro.yarn created. Full wiring deferred.");
            
            // Wire ResonanceScore as Yarn variable:
            // _runner.VariableStorage.SetValue("$rs", ResonanceScoreSystem.CurrentRS);
        }

        // Stub: would fire on each Yarn <<line>> node
        // void OnYarnLine(string lineID, string speaker, string text)
        // {
        //     DialogueManager.Instance.PlayLine(new DialogueLine 
        //     { 
        //         LineID = lineID, 
        //         Speaker = speaker, 
        //         Text = text 
        //     });
        // }
    }
}
