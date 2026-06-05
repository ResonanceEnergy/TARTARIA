using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Tartaria.Integration
{
    public class Moon1FirstTimeHints : MonoBehaviour
    {
        private static Moon1FirstTimeHints _instance;
        private float _lastHintTime = 0f;
        private bool _hint1Shown = false;
        private bool _hint2Shown = false;
        private bool _hint3Shown = false;
        private bool _hint4Shown = false;
        private bool _hint5Shown = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (_instance != null) return;
            var go = new GameObject("Moon1FirstTimeHints");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<Moon1FirstTimeHints>();
        }

        private void Update()
        {
            if (Time.time - _lastHintTime < 2f)
                return;

            if (!_hint1Shown && PlayerPrefs.GetInt("TARTARIA_Hint_Welcome", 0) == 0)
            {
                ShowBanner("Welcome to Echohaven", "Walk with WASD or the left stick. Look around with the mouse or right stick.");
                _hint1Shown = true;
            }
            else if (!_hint2Shown && PlayerPrefs.GetInt("TARTARIA_Hint_Movement", 0) == 0)
            {
                ShowBanner("Movement", "Try WASD now. The buildings ahead are buried — get close to one.");
                _hint2Shown = true;
            }
            else if (!_hint3Shown && PlayerPrefs.GetInt("TARTARIA_Hint_Interact", 0) == 0)
            {
                ShowBanner("Press E", "Press E (or A on gamepad) near a glowing building to begin tuning.");
                _hint3Shown = true;
            }
            else if (!_hint4Shown && PlayerPrefs.GetInt("TARTARIA_Hint_Restoration", 0) == 0)
            {
                ShowBanner("First Restoration", "Three nodes per building. Restore all three Moon 1 hero buildings to complete this Moon.");
                _hint4Shown = true;
                GameEvents.OnBuildingRestoredTyped += OnBuildingRestoredTypedHandler;
            }
            else if (!_hint5Shown && PlayerPrefs.GetInt("TARTARIA_Hint_Combat", 0) == 0)
            {
                ShowBanner("Mud Golems Awaken", "Restoration draws enemies. Press G or RT for Giant Mode if overwhelmed.");
                _hint5Shown = true;
                GameEvents.OnTuningComplete += OnTuningCompleteHandler;
            }
        }

        private void ShowBanner(string title, string body)
        {
            ServiceLocator.HUD?.ShowBanner(title, body, 8f);
        }

        private void OnBuildingRestoredTypedHandler()
        {
            PlayerPrefs.SetInt("TARTARIA_Hint_Restoration", 1);
            _hint4Shown = true;
        }

        private void OnTuningCompleteHandler(float accuracy)
        {
            if (accuracy >= 0.25f)
            {
                PlayerPrefs.SetInt("TARTARIA_Hint_Combat", 1);
                _hint5Shown = true;
            }
        }
    }
}
