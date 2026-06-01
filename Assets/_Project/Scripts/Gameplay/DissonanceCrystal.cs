using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Gameplay
{
    public class DissonanceCrystal : MonoBehaviour
    {
        public float DissonanceHz = Random.Range(666f, 889f);
        public float DrainRadius = 4f;
        public float DrainPerSecond = 5f;
        public bool IsCleansed { get; private set; }
        public event System.Action OnCleansed;

        private Transform playerTransform;
        private Renderer crystalRenderer;

        void Awake()
        {
            if (transform.childCount == 0)
            {
                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.SetParent(transform, false);
                cube.transform.localPosition = Vector3.zero;
                cube.transform.localScale = new Vector3(0.6f, 1.8f, 0.6f);
                cube.transform.rotation = Quaternion.Euler(0f, 45f, 0f);

                var urpLit = Shader.Find("Universal Render Pipeline/Lit");
                if (urpLit != null)
                {
                    crystalRenderer = cube.GetComponent<Renderer>();
                    crystalRenderer.sharedMaterial = new Material(urpLit);
                    SetCrystalColor();
                    crystalRenderer.sharedMaterial.EnableKeyword("_EMISSION");
                    crystalRenderer.sharedMaterial.SetColor("_EmissionColor", crystalRenderer.sharedMaterial.GetColor("_BaseColor") * 1.4f);
                }
            }
        }

        void Update()
        {
            if (!IsCleansed)
            {
                playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
                if (Vector3.Distance(transform.position, playerTransform.position) <= DrainRadius)
                {
                    GameEvents.FireRSChange(-DrainPerSecond * Time.deltaTime);
                }
            }
        }

        public void Cleanse()
        {
            IsCleansed = true;
            OnCleansed?.Invoke();
            crystalRenderer.sharedMaterial.EnableKeyword("_EMISSION");
            crystalRenderer.sharedMaterial.SetColor("_EmissionColor", Color.white * 0.6f);
            Destroy(gameObject, 3f);
        }

        private void SetCrystalColor()
        {
            switch (DissonanceHz)
            {
                case >= 666f and < 778f:
                    crystalRenderer.sharedMaterial.SetColor("_BaseColor", new Color(0.45f, 0.15f, 0.6f));
                    break;
                case >= 778f and < 889f:
                    crystalRenderer.sharedMaterial.SetColor("_BaseColor", new Color(0.7f, 0.2f, 0.5f));
                    break;
                default:
                    crystalRenderer.sharedMaterial.SetColor("_BaseColor", new Color(0.2f, 0.4f, 0.7f));
                    break;
            }
        }
    }
}
