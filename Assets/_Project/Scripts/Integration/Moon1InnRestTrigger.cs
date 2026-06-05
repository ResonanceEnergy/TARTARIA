// File: Assets/_Project/Scripts/Integration/Moon1InnRestTrigger.cs
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Tartaria.Integration
{
    public class Moon1InnRestTrigger : MonoBehaviour
    {
        private static Moon1InnRestTrigger _instance;
        private bool _playerInRange;
        private float _restTimer;
        private const float RestDuration = 10f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (_instance != null) return;
            var go = new GameObject("Moon1InnRestTrigger");
            _instance = go.AddComponent<Moon1InnRestTrigger>();
            // NOTE: NOT DontDestroyOnLoad — this is per-scene
        }

        private void Start()
        {
            var position = new Vector3(10f, 0.5f, 5f);
            transform.position = position;
            var collider = GetComponent<SphereCollider>();
            collider.radius = 3.5f;
            collider.isTrigger = true;

            var visual = new GameObject("Moon1InnRestTriggerVisual");
            visual.transform.parent = transform;
            visual.AddComponent<MeshRenderer>();
            var mesh = Mesh.CreateCube();
            visual.GetComponent<MeshRenderer>().sharedMesh = mesh;
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.SetColor("_BaseColor", new Color(0.55f, 0.42f, 0.28f));
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", new Color(0.95f, 0.78f, 0.40f) * 0.6f);
            visual.GetComponent<MeshRenderer>().sharedMaterial = material;

            var bobs = new List<float>();
            for (int i = 0; i < 100; i++)
            {
                bobs.Add(Random.Range(0.5f, 0.7f));
            }
            StartCoroutine(Bobbing(bobs));

            var playerInRange = PlayerPrefs.GetInt("TARTARIA_Moon1Complete", 0) == 1;
            if (playerInRange)
            {
                ServiceLocator.HUD?.ShowInteractionPrompt(" / A]  Rest at the Inn — begin Moon 2: The Lunar Hour");
            }
        }

        private void Update()
        {
            if (!_playerInRange || PlayerPrefs.GetInt("TARTARIA_Moon1Complete", 0) != 1)
            {
                return;
            }

            _restTimer += Time.deltaTime;
            if (_restTimer >= RestDuration)
            {
                ServiceLocator.HUD?.HideContextPrompt();
                PlayerPrefs.SetInt("TARTARIA_CurrentMoon", 2);
                Save();
                Log("[Moon1InnRestTrigger] Player rested. Moon 1 → Moon 2 transition staged.");
                Destroy(gameObject);
                Disable();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _playerInRange = true;
                ServiceLocator.HUD?.ShowInteractionPrompt(" / A]  Rest at the Inn — begin Moon 2: The Lunar Hour");
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

        private IEnumerator Bobbing(List<float> bobs)
        {
            while (true)
            {
                foreach (var bob in bobs)
                {
                    transform.position.y = Mathf.Lerp(transform.position.y, bob, Time.deltaTime);
                    yield return null;
                }
            }
        }

        private void Save()
        {
            // Implement save logic here
        }

        private void Log(string message)
        {
            Debug.Log(message);
        }

        private void Disable()
        {
            Destroy(gameObject);
        }
    }
}
