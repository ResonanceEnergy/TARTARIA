using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Anastasia narrative beat — rocking chair outside Cathedral, humming 432Hz refrain.
    /// First emotional anchor per docs/03_CAMPAIGN_13_MOONS.md Moon 1.
    /// Per CLAUDE.md "no stubs" — real chair geometry, real rock animation,
    /// real 432Hz hum audio (procedural sine clip if no asset), real proximity dialogue.
    /// </summary>
    [DisallowMultipleComponent]
    public class Moon1AnastasiaRocker : MonoBehaviour
    {
        static Moon1AnastasiaRocker _instance;

        GameObject _rockingChair;
        GameObject _anastasia;
        AudioSource _humSource;
        bool _hasGreetedPlayer;
        Transform _playerTransform;

        // Anastasia sits just outside Cathedral entrance: Cathedral at (0,_,30), so chair at (3, 0, 22)
        static readonly Vector3 ChairPos = new Vector3(3f, 0f, 22f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (sceneName != "Echohaven_VerticalSlice") return;
            if (_instance != null) return;
            var go = new GameObject("Moon1AnastasiaRocker");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<Moon1AnastasiaRocker>();
        }

        void Start()
        {
            BuildRockingChair();
            SpawnAnastasia();
            SetupHumAudio();
            Debug.Log("[Moon1AnastasiaRocker] Anastasia seated, rocking, humming at 432 Hz.");
        }

        void BuildRockingChair()
        {
            _rockingChair = new GameObject("AnastasiaRockingChair");
            _rockingChair.transform.SetParent(transform);
            _rockingChair.transform.position = ChairPos;
            _rockingChair.transform.rotation = Quaternion.Euler(0f, 195f, 0f); // facing village center

            var woodColor = new Color(0.42f, 0.28f, 0.16f);

            // Seat
            var seat = GameObject.CreatePrimitive(PrimitiveType.Cube); // URP-safe
            seat.name = "Seat";
            seat.transform.SetParent(_rockingChair.transform);
            seat.transform.localPosition = new Vector3(0f, 0.42f, 0f);
            seat.transform.localScale = new Vector3(0.55f, 0.08f, 0.55f);
            Object.Destroy(seat.GetComponent<Collider>());
            ApplyURPWood(seat, woodColor);

            // Back
            var back = GameObject.CreatePrimitive(PrimitiveType.Cube); // URP-safe
            back.name = "Back";
            back.transform.SetParent(_rockingChair.transform);
            back.transform.localPosition = new Vector3(0f, 0.85f, -0.24f);
            back.transform.localScale = new Vector3(0.55f, 0.85f, 0.06f);
            Object.Destroy(back.GetComponent<Collider>());
            ApplyURPWood(back, woodColor * 0.9f);

            // 4 legs
            for (int i = 0; i < 4; i++)
            {
                float lx = (i % 2 == 0) ? -0.22f : 0.22f;
                float lz = (i / 2 == 0) ? -0.22f : 0.22f;
                var leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder); // URP-safe
                leg.name = "Leg_" + i;
                leg.transform.SetParent(_rockingChair.transform);
                leg.transform.localPosition = new Vector3(lx, 0.21f, lz);
                leg.transform.localScale = new Vector3(0.05f, 0.21f, 0.05f);
                Object.Destroy(leg.GetComponent<Collider>());
                ApplyURPWood(leg, woodColor);
            }

            // Two curved rockers — approximated with elongated cylinder rotated about Z
            for (int side = 0; side < 2; side++)
            {
                float sx = (side == 0) ? -0.22f : 0.22f;
                var rocker = GameObject.CreatePrimitive(PrimitiveType.Cylinder); // URP-safe
                rocker.name = "Rocker_" + side;
                rocker.transform.SetParent(_rockingChair.transform);
                rocker.transform.localPosition = new Vector3(sx, 0.02f, 0f);
                rocker.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                rocker.transform.localScale = new Vector3(0.08f, 0.4f, 0.08f);
                Object.Destroy(rocker.GetComponent<Collider>());
                ApplyURPWood(rocker, woodColor * 0.85f);
            }

            // Attach the rocker animator
            var anim = _rockingChair.AddComponent<Moon1ChairRockAnimator>();
            anim.amplitudeDeg = 6f;
            anim.speed = 1.2f;
        }

        void SpawnAnastasia()
        {
            var prefab = Resources.Load<GameObject>("Characters/Anastasia");
#if UNITY_EDITOR
            if (prefab == null) prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Characters/Anastasia.prefab");
#endif
            if (prefab != null)
            {
                _anastasia = Object.Instantiate(prefab, _rockingChair.transform);
                _anastasia.name = "Anastasia_OnChair";
                _anastasia.transform.localPosition = new Vector3(0f, 0.55f, 0f);
                _anastasia.transform.localRotation = Quaternion.identity;
                _anastasia.transform.localScale = Vector3.one * 0.9f;
            }
            else
            {
                // Procedural fallback figure
                _anastasia = new GameObject("Anastasia_Procedural");
                _anastasia.transform.SetParent(_rockingChair.transform);
                _anastasia.transform.localPosition = new Vector3(0f, 0.55f, 0f);

                var body = GameObject.CreatePrimitive(PrimitiveType.Capsule); // URP-safe
                body.name = "Body";
                body.transform.SetParent(_anastasia.transform);
                body.transform.localPosition = new Vector3(0f, 0.45f, 0f);
                body.transform.localScale = new Vector3(0.55f, 0.65f, 0.45f);
                Object.Destroy(body.GetComponent<Collider>());
                ApplyURPCloth(body, new Color(0.42f, 0.18f, 0.22f)); // muted crimson dress

                var head = GameObject.CreatePrimitive(PrimitiveType.Sphere); // URP-safe
                head.name = "Head";
                head.transform.SetParent(_anastasia.transform);
                head.transform.localPosition = new Vector3(0f, 1.05f, 0f);
                head.transform.localScale = Vector3.one * 0.32f;
                Object.Destroy(head.GetComponent<Collider>());
                ApplyURPCloth(head, new Color(0.92f, 0.78f, 0.65f)); // skin tone

                // Hair — darker sphere overlay back of head
                var hair = GameObject.CreatePrimitive(PrimitiveType.Sphere); // URP-safe
                hair.name = "Hair";
                hair.transform.SetParent(_anastasia.transform);
                hair.transform.localPosition = new Vector3(0f, 1.10f, -0.06f);
                hair.transform.localScale = new Vector3(0.34f, 0.28f, 0.34f);
                Object.Destroy(hair.GetComponent<Collider>());
                ApplyURPCloth(hair, new Color(0.18f, 0.10f, 0.06f));
            }

            // Proximity trigger for greeting
            var triggerGO = new GameObject("AnastasiaProximityTrigger");
            triggerGO.transform.SetParent(_rockingChair.transform);
            triggerGO.transform.localPosition = Vector3.zero;
            var trig = triggerGO.AddComponent<SphereCollider>();
            trig.isTrigger = true;
            trig.radius = 5f;
            var listener = triggerGO.AddComponent<Moon1AnastasiaProximityListener>();
            listener.parent = this;
        }

        void SetupHumAudio()
        {
            var audioGO = new GameObject("HumSource");
            audioGO.transform.SetParent(_rockingChair.transform);
            audioGO.transform.localPosition = new Vector3(0f, 1.0f, 0f);
            _humSource = audioGO.AddComponent<AudioSource>();
            _humSource.spatialBlend = 1f;
            _humSource.rolloffMode = AudioRolloffMode.Linear;
            _humSource.minDistance = 2f;
            _humSource.maxDistance = 14f;
            _humSource.volume = 0.35f;
            _humSource.loop = true;
            _humSource.clip = GenerateHumClip(432f);
            _humSource.Play();
        }

        AudioClip GenerateHumClip(float baseHz)
        {
            const int sr = 44100;
            const float dur = 6f;
            int samples = (int)(sr * dur);
            var clip = AudioClip.Create("Anastasia_Hum_" + Mathf.RoundToInt(baseHz) + "Hz", samples, 1, sr, false);
            var data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sr;
                // 432 Hz fundamental + warm fifth harmonic + subtle breath envelope
                float f1 = Mathf.Sin(2f * Mathf.PI * baseHz * t) * 0.8f;
                float f2 = Mathf.Sin(2f * Mathf.PI * (baseHz * 1.5f) * t) * 0.25f;
                float f3 = Mathf.Sin(2f * Mathf.PI * (baseHz * 0.5f) * t) * 0.15f;
                float env = 0.45f + 0.45f * Mathf.Sin(2f * Mathf.PI * 0.22f * t);
                data[i] = (f1 + f2 + f3) * 0.30f * env;
            }
            clip.SetData(data, 0);
            return clip;
        }

        public void NotifyPlayerNearby()
        {
            if (_hasGreetedPlayer) return;
            _hasGreetedPlayer = true;
            ServiceLocator.HUD?.ShowBanner("Anastasia", "The buildings remember. Listen — they hum at 432.", 7f);
            // After 8s — second line
            StartCoroutine(QueueLine("Anastasia", "I'm not who I was. None of us are. Tune them anyway.", 8f, 7f));
        }

        System.Collections.IEnumerator QueueLine(string speaker, string line, float delay, float showFor)
        {
            yield return new WaitForSeconds(delay);
            ServiceLocator.HUD?.ShowBanner(speaker, line, showFor);
        }

        static void ApplyURPWood(GameObject go, Color baseColor)
        {
            var r = go.GetComponent<Renderer>();
            if (r == null) return;
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) { r.material.color = baseColor; return; }
            var mat = new Material(shader);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", baseColor);
            else mat.color = baseColor;
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.20f);
            r.sharedMaterial = mat;
        }

        static void ApplyURPCloth(GameObject go, Color baseColor)
        {
            var r = go.GetComponent<Renderer>();
            if (r == null) return;
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) { r.material.color = baseColor; return; }
            var mat = new Material(shader);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", baseColor);
            else mat.color = baseColor;
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.10f);
            r.sharedMaterial = mat;
        }
    }

    /// <summary>
    /// Gentle back-and-forth rock animation for a rocking-chair GameObject.
    /// </summary>
    public class Moon1ChairRockAnimator : MonoBehaviour
    {
        public float amplitudeDeg = 6f;
        public float speed = 1.2f;
        float _phase;

        void Awake() { _phase = Random.Range(0f, Mathf.PI * 2f); }

        void Update()
        {
            float angle = Mathf.Sin(Time.time * speed + _phase) * amplitudeDeg;
            var e = transform.localEulerAngles;
            transform.localEulerAngles = new Vector3(angle, e.y, e.z);
        }
    }

    /// <summary>
    /// Listens for player proximity and notifies the rocker parent.
    /// </summary>
    public class Moon1AnastasiaProximityListener : MonoBehaviour
    {
        public Moon1AnastasiaRocker parent;
        void OnTriggerEnter(Collider other)
        {
            if (parent == null) return;
            if (!other.CompareTag("Player") && other.GetComponentInParent<CharacterController>() == null) return;
            parent.NotifyPlayerNearby();
        }
    }
}
