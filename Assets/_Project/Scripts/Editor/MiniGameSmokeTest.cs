#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using Tartaria.Gameplay;

namespace Tartaria.Editor.QA
{
    /// <summary>
    /// MiniGameSmokeTest — headless rapid-instantiate/destroy harness for the 3
    /// TuningMiniGame variants (Slider / Waveform / HarmonicPattern).
    ///
    /// Per agent/gameplay/mini-game-variant-polish: instantiates each variant 10x
    /// via reflection, calls StartTuning, then immediately destroys the host GO
    /// (the canonical "Cancel/Dispose" for a MonoBehaviour). Any thrown exception
    /// counts as a fail. No scene dependency — temp GameObjects are flagged
    /// HideAndDontSave so nothing leaks into the active scene.
    /// </summary>
    public static class MiniGameSmokeTest
    {
        // Type names resolved via reflection per spec. Assembly name = Tartaria.Gameplay
        // (matches Assets/_Project/Scripts/Gameplay/Tartaria.Gameplay.asmdef).
        static readonly string[] VARIANT_TYPE_NAMES = new[]
        {
            "Tartaria.Gameplay.TuningMiniGame, Tartaria.Gameplay",
            "Tartaria.Gameplay.TuningVariantB_Waveform, Tartaria.Gameplay",
            "Tartaria.Gameplay.TuningVariantC_Pattern, Tartaria.Gameplay",
        };

        const int ITERATIONS_PER_VARIANT = 10;

        [MenuItem("Tartaria/9 QA/Mini-Game Smoke Test", false, 94)]
        public static void Run()
        {
            Debug.Log("[MiniGameSmokeTest] === START === rapid open/close x10 per variant");

            int grandPass = 0;
            int grandFail = 0;

            foreach (var typeName in VARIANT_TYPE_NAMES)
            {
                var t = Type.GetType(typeName);
                if (t == null)
                {
                    Debug.LogError($"[MiniGameSmokeTest] Type not found: {typeName}");
                    grandFail += ITERATIONS_PER_VARIANT;
                    continue;
                }

                int pass = 0;
                int fail = 0;
                for (int i = 0; i < ITERATIONS_PER_VARIANT; i++)
                {
                    if (RunOne(t, i))
                        pass++;
                    else
                        fail++;
                }

                Debug.Log($"[MiniGameSmokeTest] {t.Name}: pass={pass}/{ITERATIONS_PER_VARIANT} fail={fail}/{ITERATIONS_PER_VARIANT}");
                grandPass += pass;
                grandFail += fail;
            }

            int totalRuns = VARIANT_TYPE_NAMES.Length * ITERATIONS_PER_VARIANT;
            string summary = $"[MiniGameSmokeTest] === DONE === pass={grandPass}/{totalRuns} fail={grandFail}/{totalRuns}";
            if (grandFail == 0)
                Debug.Log(summary);
            else
                Debug.LogError(summary);
        }

        /// <summary>
        /// One iteration: create temp GO, AddComponent(t), call StartTuning(Vector3, Action),
        /// then DestroyImmediate the GO. Returns true if no exception was thrown.
        /// </summary>
        static bool RunOne(Type t, int index)
        {
            GameObject go = null;
            try
            {
                go = new GameObject($"SmokeTest_{t.Name}_{index}")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };

                var comp = go.AddComponent(t) as MonoBehaviour;
                if (comp == null)
                {
                    Debug.LogError($"[MiniGameSmokeTest] AddComponent returned null for {t.Name} iter {index}");
                    return false;
                }

                var variant = comp as ITuningVariant;
                if (variant == null)
                {
                    Debug.LogError($"[MiniGameSmokeTest] {t.Name} does not implement ITuningVariant");
                    return false;
                }

                // StartGame equivalent — ITuningVariant.StartTuning(Vector3 nodePosition, Action onComplete).
                variant.StartTuning(Vector3.zero, null);

                // Immediate Stop/Cancel/Dispose — destroying the host GameObject is the
                // canonical Unity disposal path for a MonoBehaviour. Same-frame destroy
                // is exactly the "rapid open/close" stress this test exists to catch.
                UnityEngine.Object.DestroyImmediate(go);
                go = null;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[MiniGameSmokeTest] {t.Name} iter {index} threw: {e.GetType().Name}: {e.Message}\n{e.StackTrace}");
                return false;
            }
            finally
            {
                if (go != null)
                {
                    try { UnityEngine.Object.DestroyImmediate(go); }
                    catch { /* swallow — already failing */ }
                }
            }
        }
    }
}
#endif
