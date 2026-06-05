// File: Assets/_Project/Scripts/Gameplay/DissonanceCrystal.cs
using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Tartaria.Gameplay
{
    public class DissonanceCrystal : MonoBehaviour
    {
        public float DissonanceHz { get; private set; }    // 666, 777, or 888 picked at Awake
        public float DrainRadius = 4f;
        public float DrainPerSecond = 5f;
        public bool IsCleansed { get; private set; }
        public event System.Action OnCleansed;

        public void Cleanse()
        {
            if (!IsCleansed)
            {
                IsCleansed = true;
                OnCleansed?.Invoke();
                StartCoroutine(CleanseCoroutine());
            }
        }

        private IEnumerator CleanseCoroutine()
        {
            float startTime = Time.time;
            while (Time.time - startTime < 3f)
            {
                float drainAmount = DrainPerSecond * Time.deltaTime * 0.1f;
                GameEvents.FireRSChange(-drainAmount);
                yield return new WaitForSeconds(0.1f);
            }

            Destroy(gameObject, 3f);
        }

        private void Awake()
        {
            DissonanceHz = Random.Range(666f, 889f); // Picked at Awake
            var baseColor = GetDissonanceColor();
            var urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit != null)
            {
                var mat = new Material(urpLit);
                mat.SetColor("_BaseColor", baseColor);
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", baseColor * 1.4f);
                foreach (var r in gameObject.GetComponentsInChildren<Renderer>()) r.sharedMaterial = mat;
            }

            SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
            sphereCollider.radius = DrainRadius;
        }

        private Color GetDissonanceColor()
        {
            switch (DissonanceHz)
            {
                case 666f:
                    return new Color(0.45f, 0.15f, 0.6f);
                case 777f:
                    return new Color(0.7f, 0.2f, 0.5f);
                case 888f:
                    return new Color(0.2f, 0.4f, 0.7f);
                default:
                    throw new ArgumentException("Invalid DissonanceHz value");
            }
        }

        private void Update()
        {
            if (!IsCleansed && Physics.CheckSphere(transform.position, DrainRadius))
            {
                GameEvents.FireRSChange(-DrainPerSecond * Time.deltaTime * 0.1f);
            }
        }
    }
}
