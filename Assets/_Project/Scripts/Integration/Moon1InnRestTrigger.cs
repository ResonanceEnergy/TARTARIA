using System.Collections;

using UnityEngine;
using UnityEngine.InputSystem;
using Tartaria.Core;

namespace Tartaria.Integration
{
    public class Moon1InnRestTrigger : MonoBehaviour
    {
        private static Moon1InnRestTrigger _instance;
        private static GameObject innObject;
        private static Renderer innRenderer;
        private float startY = 0.5f;
        private bool _playerInRange = false;

        
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (_instance != null) return;
            var go = new GameObject("Moon1InnRestTrigger");
            _instance = go.AddComponent<Moon1InnRestTrigger>();

            var transform = _instance.transform;
            transform.position = new Vector3(10f, 0.5f, 5f);

            var collider = 
transform.gameObject.AddComponent<SphereCollider>();
            collider.radius = 3.5f;
            collider.isTrigger = true;

            innObject = new GameObject("InnCube");
            innObject.transform.SetParent(_instance.transform);
            innRenderer = innObject.AddComponent<MeshRenderer>();

            var cube = innObject.AddComponent<MeshFilter>().mesh = 
Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            var urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit != null)
            {
                var mat = new Material(urpLit);
                mat.SetColor("_BaseColor", new Color(0.55f, 0.42f, 
0.28f));
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", new Color(0.95f, 0.78f, 
0.40f) * 0.6f);
                innRenderer.material = mat;
            }

            innObject.transform.localScale = new Vector3(1.4f, 0.4f, 
1.4f);

            _instance.StartCoroutine(_instance.BobInnVisual());
        }

        private IEnumerator BobInnVisual()
        {
            while (true)
            {
                transform.position = new Vector3(transform.position.x, 
startY + Mathf.Sin(Time.time * 2 * Mathf.PI / 0.6f) * 0.1f, 
transform.position.z);
                yield return null;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _playerInRange = true;
                if (PlayerPrefs.GetInt("TARTARIA_Moon1Complete", 0) == 1)
                {
                    ServiceLocator.HUD?.ShowInteractionPrompt(" / A]   Rest at the Inn - begin Moon 2: The Lunar Hour");
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _playerInRange = false;
                ServiceLocator.HUD?.HideContextPrompt();
            }
        }

        private void Update()
        {
            if (PlayerPrefs.GetInt("TARTARIA_Moon1Complete", 0) != 1) 
return;

            if (_playerInRange && 
(Keyboard.current.eKey.wasPressedThisFrame || 
Gamepad.current.buttonSouth.wasPressedThisFrame))
            {
                TriggerRest();
            }
        }

        private void TriggerRest()
        {
            ServiceLocator.HUD?.ShowBanner("You rest at the Inn", "Dawn  breaks on the Lunar Moon. Lirael waits at the gate.", 10f);
            ServiceLocator.HUD?.HideContextPrompt();
            PlayerPrefs.SetInt("TARTARIA_CurrentMoon", 2);
            PlayerPrefs.Save();
            Debug.Log("[Moon1InnRestTrigger] Player rested. Moon 1 -> Moon  2 transition staged.");
            Destroy(innObject);
            enabled = false;
        }
    }
}
