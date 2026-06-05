// File: Assets/_Project/Scripts/Integration/Moon2CassianArrival.cs
using UnityEngine;
using UnityEngine.InputSystem;
using Tartaria.Core;

namespace Tartaria.Integration
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public class Moon2CassianArrival : MonoBehaviour
    {
        private static int CassianMet = PlayerPrefs.GetInt("TARTARIA_Moon2_CassianMet", 0);

        void Start()
        {
            if (CassianMet == 0)
            {
                SpawnCassian();
                SpawnLiraelEcho();
                SpawnTriggers();
                PlayerPrefs.SetInt("TARTARIA_Moon2_CassianMet", 1);
            }
        }

        private void SpawnCassian()
        {
            GameObject cassian = Resources.Load<GameObject>("Enemies/MudGolem");
            if (cassian == null)
            {
                cassian = Instantiate(Resources.Load<GameObject>("Prefabs/Characters/Cassian.prefab"));
            }
            cassian.transform.position = new Vector3(-65, 0.5f, -5);
        }

        private void SpawnLiraelEcho()
        {
            GameObject echo = GenerateEchoClip(432f);
            echo.transform.position = new Vector3(-90, 5, 5);
        }

        private void SpawnTriggers()
        {
            // Cassian trigger
            var cassianTrigger = new Trigger();
            cassianTrigger.enabled = true;
            cassianTrigger.AddCondition(new SphereCondition(8f));
            cassianTrigger.OnEnter += () =>
            {
                ShowBanner("Cassian", "A man in a long coat steps from the shadow. He doesn't smile.");
                Invoke(nameof(HideBanner), 6f);
            };

            // Lirael echo trigger
            var liraelEchoTrigger = new Trigger();
            liraelEchoTrigger.enabled = true;
            liraelEchoTrigger.AddCondition(new SphereCondition(5f));
            liraelEchoTrigger.OnEnter += () =>
            {
                ShowBanner("Lirael's echo", "The lullaby... 432 Hz... but broken here. Mixed with something else.");
                Invoke(nameof(HideBanner), 7f);
            };
        }

        private void ShowBanner(string title, string message)
        {
            var banner = new Banner(title, message);
            banner.Show();
        }

        private void HideBanner()
        {
            var banner = Banner.activeInstance;
            if (banner != null)
            {
                banner.Hide();
            }
        }

        private void ApplyURP(GameObject obj, Color color)
        {
            var material = obj.GetComponent<Renderer>().material;
            material.SetColor("_BaseColor", color);
        }

        private AudioClip GenerateEchoClip(float baseHz)
        {
            const int sr = 44100;
            const float dur = 4f;
            int samples = (int)(sr * dur);
            var clip = AudioClip.Create("Moon2_Echo", samples, 1, sr, false);
            var data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sr;
                float fundamental = Mathf.Sin(2f * Mathf.PI * baseHz * t);
                // Add a dissonant fifth (out-of-tune) for "broken" feel
                float dissonant = Mathf.Sin(2f * Mathf.PI * (baseHz * 1.51f) * t) * 0.4f;
                float env = 0.5f + 0.5f * Mathf.Sin(2f * Mathf.PI * 0.18f * t);
                data[i] = (fundamental + dissonant) * 0.25f * env;
            }
            clip.SetData(data, 0);
            return clip;
        }
    }
}
