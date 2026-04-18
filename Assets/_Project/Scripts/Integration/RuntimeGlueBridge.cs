using System.Reflection;
using UnityEngine;
using Tartaria.Core;
using Tartaria.Input;
using Tartaria.UI;

namespace Tartaria.Integration
{
    /// <summary>
    /// Runtime auto-wiring bridge — fills in Inspector references that weren't
    /// set at edit time. Runs after PlayerSpawner (-90) and before
    /// GameLoopController (-50).
    ///
    /// Wires:
    ///   - GameLoopController.playerInput → Player's PlayerInputHandler
    ///   - GameLoopController.cameraController → Main Camera's CameraController
    ///   - DebugCheatConsole — attaches if not already in scene (Editor only)
    /// </summary>
    [DefaultExecutionOrder(-70)]
    public class RuntimeGlueBridge : MonoBehaviour
    {
        [SerializeField] bool logWiring = true;

        void Awake()
        {
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            // Delay one frame to let PlayerSpawner.Start run first
            Invoke(nameof(WireAll), 0.05f);
        }

        void WireAll()
        {
            WireGameLoopController();
            EnsureVisualSystems();
            EnsureDebugConsole();
        }

        void WireGameLoopController()
        {
            var glc = GameLoopController.Instance;
            if (glc == null)
            {
                Log("GameLoopController not found -- skipping wiring");
                return;
            }

            var glcType = glc.GetType();
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

            // Wire playerInput
            var piField = glcType.GetField("playerInput", flags);
            if (piField != null && piField.GetValue(glc) == null)
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    var handler = player.GetComponent<PlayerInputHandler>();
                    if (handler != null)
                    {
                        piField.SetValue(glc, handler);
                        glc.BindCombatEvents();
                        Log($"Wired GameLoopController.playerInput -> {player.name}");
                    }
                }
            }

            // Wire cameraController
            var ccField = glcType.GetField("cameraController", flags);
            if (ccField != null && ccField.GetValue(glc) == null)
            {
                var cam = UnityEngine.Camera.main;
                if (cam != null)
                {
                    var cc = cam.GetComponent<Camera.CameraController>();
                    if (cc != null)
                    {
                        ccField.SetValue(glc, cc);
                        Log($"Wired GameLoopController.cameraController -> {cam.name}");
                    }
                }
            }
        }

        void EnsureVisualSystems()
        {
            // TartariaPostProcessing — global URP Volume (Bloom + Vignette + ColorAdj)
            if (FindAnyObjectByType<TartariaPostProcessing>() == null)
            {
                var go = new GameObject("--- TARTARIA POSTPROCESSING ---");
                go.AddComponent<TartariaPostProcessing>();
                DontDestroyOnLoad(go);
                Log("Created TartariaPostProcessing (runtime URP Volume)");
            }

            // LeyLineVisualizer — GL ley line renderer (visible during AetherVision / Scan)
            if (FindAnyObjectByType<LeyLineVisualizer>() == null)
            {
                var go = new GameObject("--- LEY LINE VISUALIZER ---");
                go.AddComponent<LeyLineVisualizer>();
                DontDestroyOnLoad(go);
                Log("Created LeyLineVisualizer");
            }

            // ArchiveManager — Old World Archive unlock tracking
            if (FindAnyObjectByType<ArchiveManager>() == null)
            {
                var go = new GameObject("--- ARCHIVE MANAGER ---");
                go.AddComponent<ArchiveManager>();
                DontDestroyOnLoad(go);
                Log("Created ArchiveManager");
            }
        }

        void EnsureDebugConsole()
        {
            #if UNITY_EDITOR
            if (FindAnyObjectByType<DebugCheatConsole>() == null)
            {
                var go = new GameObject("--- DEBUG CONSOLE ---");
                go.AddComponent<DebugCheatConsole>();
                DontDestroyOnLoad(go);
                Log("Created DebugCheatConsole");
            }
            #endif
        }

        void Log(string msg)
        {
            if (logWiring)
                Debug.Log($"[RuntimeGlue] {msg}");
        }
    }
}
