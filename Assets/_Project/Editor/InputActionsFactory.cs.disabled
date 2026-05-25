using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;

namespace Tartaria.Editor
{
    /// <summary>
    /// Creates the TartariaInputActions.inputactions asset with all 10 player actions
    /// bound to keyboard/mouse + gamepad. Idempotent — skips if asset already exists.
    ///
    /// Actions (matching PlayerInputHandler / CameraController):
    ///   Move (Vector2)       — WASD / Left Stick
    ///   Sprint (Button)      — Shift / Left Stick Press
    ///   Interact (Button)    — E / South Button (A)
    ///   ResonancePulse       — Space / West Button (X)
    ///   HarmonicStrike       — Left Mouse / North Button (Y)
    ///   FrequencyShield      — Q / Left Shoulder
    ///   AetherVision         — Tab / Right Trigger
    ///   Pause                — Escape / Start
    ///   CameraLook (Vector2) — Mouse Delta / Right Stick
    ///   CameraZoom (Axis)    — Scroll Y / D-Pad Y
    /// </summary>
    public static class InputActionsFactory
    {
        public const string AssetPath = "Assets/_Project/Input/TartariaInputActions.inputactions";

        [MenuItem("Tartaria/Build Assets/Input Actions", false, 16)]
        public static void CreateInputActionsAsset()
        {
            if (AssetDatabase.LoadAssetAtPath<InputActionAsset>(AssetPath) != null)
            {
                Debug.Log("[Tartaria] Input actions asset already exists — skipping.");
                return;
            }

            string dir = Path.GetDirectoryName(Path.Combine(Application.dataPath, "..", AssetPath));
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            string json = BuildInputActionsJson();
            File.WriteAllText(Path.Combine(Application.dataPath, "..", AssetPath), json);
            AssetDatabase.ImportAsset(AssetPath);
            Debug.Log($"[Tartaria] Input actions asset created at {AssetPath}.");
        }

        static string BuildInputActionsJson()
        {
            return @"{
    ""name"": ""TartariaInputActions"",
    ""maps"": [
        {
            ""name"": ""Player"",
            ""id"": ""ad0e74e1-8b0e-4c9b-bf6e-a5f2e9e05231"",
            ""actions"": [
                {
                    ""name"": ""Move"",
                    ""type"": 1,
                    ""id"": ""b8a7d9c4-3e5f-4a1b-9c2d-e6f8a0b3c5d7"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Sprint"",
                    ""type"": 0,
                    ""id"": ""c1b2d3e4-5f6a-7b8c-9d0e-f1a2b3c4d5e6"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Interact"",
                    ""type"": 0,
                    ""id"": ""d2c3e4f5-6a7b-8c9d-0e1f-a2b3c4d5e6f7"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""ResonancePulse"",
                    ""type"": 0,
                    ""id"": ""e3d4f5a6-7b8c-9d0e-1f2a-b3c4d5e6f7a8"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""HarmonicStrike"",
                    ""type"": 0,
                    ""id"": ""f4e5a6b7-8c9d-0e1f-2a3b-c4d5e6f7a8b9"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""FrequencyShield"",
                    ""type"": 0,
                    ""id"": ""a5f6b7c8-9d0e-1f2a-3b4c-d5e6f7a8b9c0"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""AetherVision"",
                    ""type"": 0,
                    ""id"": ""b6a7c8d9-0e1f-2a3b-4c5d-e6f7a8b9c0d1"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Pause"",
                    ""type"": 0,
                    ""id"": ""c7b8d9e0-1f2a-3b4c-5d6e-f7a8b9c0d1e2"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""CameraLook"",
                    ""type"": 1,
                    ""id"": ""d8e9f0a1-2b3c-4d5e-6f7a-8b9c0d1e2f3a"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""CameraZoom"",
                    ""type"": 0,
                    ""id"": ""e9f0a1b2-3c4d-5e6f-7a8b-9c0d1e2f3a4b"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Scan"",
                    ""type"": 0,
                    ""id"": ""f0a1b2c3-4d5e-6f7a-8b9c-0d1e2f3a4b5c"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""WASD"",
                    ""id"": ""10a1b2c3-d4e5-f6a7-b8c9-d0e1f2a3b4c5"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""11a1b2c3-d4e5-f6a7-b8c9-d0e1f2a3b4c5"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""12a1b2c3-d4e5-f6a7-b8c9-d0e1f2a3b4c5"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""13a1b2c3-d4e5-f6a7-b8c9-d0e1f2a3b4c5"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""14a1b2c3-d4e5-f6a7-b8c9-d0e1f2a3b4c5"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Left Stick"",
                    ""id"": ""15a1b2c3-d4e5-f6a7-b8c9-d0e1f2a3b4c5"",
                    ""path"": ""<Gamepad>/leftStick"",
                    ""interactions"": """",
                    ""processors"": ""StickDeadzone"",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""20a1b2c3-d4e5-f6a7-b8c9-d0e1f2a3b4c5"",
                    ""path"": ""<Keyboard>/leftShift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Sprint"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""21a1b2c3-d4e5-f6a7-b8c9-d0e1f2a3b4c5"",
                    ""path"": ""<Gamepad>/leftStickPress"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Sprint"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""30a1b2c3-d4e5-f6a7-b8c9-d0e1f2a3b4c5"",
                    ""path"": ""<Keyboard>/e"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Interact"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""31a1b2c3-d4e5-f6a7-b8c9-d0e1f2a3b4c5"",
                    ""path"": ""<Gamepad>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Interact"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""40a1b2c3-d4e5-f6a7-b8c9-d0e1f2a3b4c5"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""ResonancePulse"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""41a1b2c3-d4e5-f6a7-b8c9-d0e1f2a3b4c5"",
                    ""path"": ""<Gamepad>/buttonWest"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""ResonancePulse"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""50a1b2c3-d4e5-f6a7-b8c9-d0e1f2a3b4c5"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""HarmonicStrike"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""51a1b2c3-d4e5-f6a7-b8c9-d0e1f2a3b4c5"",
                    ""path"": ""<Keyboard>/leftCtrl"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""HarmonicStrike"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""52a1b2c3-d4e5-f6a7-b8c9-d0e1f2a3b4c5"",
                    ""path"": ""<Gamepad>/buttonNorth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""HarmonicStrike"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""60a1b2c3-d4e5-f6a7-b8c9-d0e1f2a3b4c5"",
                    ""path"": ""<Keyboard>/q"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""FrequencyShield"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""61a1b2c3-d4e5-f6a7-b8c9-d0e1f2a3b4c5"",
                    ""path"": ""<Gamepad>/leftShoulder"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""FrequencyShield"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""70a1b2c3-d4e5-f6a7-b8c9-d0e1f2a3b4c5"",
                    ""path"": ""<Keyboard>/tab"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""AetherVision"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""71a1b2c3-d4e5-f6a7-b8c9-d0e1f2a3b4c5"",
                    ""path"": ""<Gamepad>/rightTrigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""AetherVision"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""80a1b2c3-d4e5-f6a7-b8c9-d0e1f2a3b4c5"",
                    ""path"": ""<Keyboard>/escape"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Pause"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""81a1b2c3-d4e5-f6a7-b8c9-d0e1f2a3b4c5"",
                    ""path"": ""<Gamepad>/start"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Pause"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""90a1b2c3-d4e5-f6a7-b8c9-d0e1f2a3b4c5"",
                    ""path"": ""<Gamepad>/rightStick"",
                    ""interactions"": """",
                    ""processors"": ""StickDeadzone(min=0.15)"",
                    ""groups"": ""Gamepad"",
                    ""action"": ""CameraLook"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a0a1b2c3-d4e5-f6a7-b8c9-d0e1f2a3b4c5"",
                    ""path"": ""<Mouse>/scroll/y"",
                    ""interactions"": """",
                    ""processors"": ""Normalize(min=-120,max=120)"",
                    ""groups"": ""Keyboard"",
                    ""action"": ""CameraZoom"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a1a1b2c3-d4e5-f6a7-b8c9-d0e1f2a3b4c5"",
                    ""path"": ""<Gamepad>/dpad/y"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""CameraZoom"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a2a1b2c3-d4e5-f6a7-b8c9-d0e1f2a3b4c5"",
                    ""path"": ""<Gamepad>/rightShoulder"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""CameraZoom"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""b0a1b2c3-d4e5-f6a7-b8c9-d0e1f2a3b4c5"",
                    ""path"": ""<Keyboard>/r"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Scan"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""b1a1b2c3-d4e5-f6a7-b8c9-d0e1f2a3b4c5"",
                    ""path"": ""<Gamepad>/leftShoulder"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Scan"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""Keyboard"",
            ""bindingGroup"": ""Keyboard"",
            ""devices"": [
                { ""devicePath"": ""<Keyboard>"", ""isOptional"": false, ""isOR"": false },
                { ""devicePath"": ""<Mouse>"", ""isOptional"": true, ""isOR"": false }
            ]
        },
        {
            ""name"": ""Gamepad"",
            ""bindingGroup"": ""Gamepad"",
            ""devices"": [
                { ""devicePath"": ""<Gamepad>"", ""isOptional"": false, ""isOR"": false }
            ]
        }
    ]
}";
        }
    }
}
