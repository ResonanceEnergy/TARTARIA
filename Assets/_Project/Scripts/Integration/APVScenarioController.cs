using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace Tartaria.Integration
{
    /// <summary>
    /// Runtime APV scenario blend controller for Moon 1 dome awakening tests.
    /// </summary>
    public class APVScenarioController : MonoBehaviour
    {
        [SerializeField] string baseScenario = "Dawn_PreAwakening";
        [SerializeField] string awakeningScenario = "Dome_Awakening";
        [SerializeField] float blendSpeed = 0.35f;

        [Range(0f, 1f)]
        [SerializeField] float blend;

        bool _awakeningActive;
        bool _canBlendScenario;

        void Start()
        {
            var refVolume = ProbeReferenceVolume.instance;
            if (refVolume == null)
                return;

            refVolume.lightingScenario = baseScenario;
            _canBlendScenario = IsScenarioBaked(refVolume, awakeningScenario);

            if (_canBlendScenario)
            {
                refVolume.BlendLightingScenario(awakeningScenario, blend);
            }
            else
            {
                Debug.LogWarning($"[Tartaria] APV scenario '{awakeningScenario}' is unavailable or unbaked; runtime blend disabled.");
            }
        }

        void Update()
        {
            if (!_canBlendScenario)
                return;

            var refVolume = ProbeReferenceVolume.instance;
            if (refVolume == null)
                return;

            // Debug control for fast visual iteration in Moon 1.
            var keyboard = Keyboard.current;
            if (keyboard != null && (keyboard.digit9Key.wasPressedThisFrame || keyboard.numpad9Key.wasPressedThisFrame))
                _awakeningActive = !_awakeningActive;

            float target = _awakeningActive ? 1f : 0f;
            blend = Mathf.MoveTowards(blend, target, blendSpeed * Time.deltaTime);
            refVolume.BlendLightingScenario(awakeningScenario, blend);
        }

        static bool IsScenarioBaked(ProbeReferenceVolume refVolume, string scenarioName)
        {
            if (string.IsNullOrEmpty(scenarioName))
                return false;

            var bakingSet = refVolume.currentBakingSet;
            if (bakingSet == null)
                return false;

            var scenarios = bakingSet.lightingScenarios;
            if (scenarios == null || !scenarios.Contains(scenarioName))
                return false;

            // ProbeVolumeBakingSet.scenarios is internal, so use reflection to verify baked payload.
            var field = typeof(ProbeVolumeBakingSet).GetField("scenarios", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
                return false;

            if (field.GetValue(bakingSet) is not IDictionary dataMap)
                return false;

            if (!dataMap.Contains(scenarioName))
                return false;

            var scenarioData = dataMap[scenarioName];
            if (scenarioData == null)
                return false;

            var hasValidData = scenarioData.GetType().GetMethod("HasValidData", BindingFlags.Instance | BindingFlags.Public);
            if (hasValidData == null)
                return false;

            var result = hasValidData.Invoke(scenarioData, new object[] { refVolume.shBands });
            return result is bool ok && ok;
        }
    }
}
