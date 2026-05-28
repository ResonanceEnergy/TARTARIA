using UnityEngine;
using System.Collections.Generic;
using Tartaria.Input;

namespace Tartaria.Integration
{
    [DefaultExecutionOrder(-30)]
    public class Moon11Secrets : MonoBehaviour
    {
        [Header("Moon 11: Prismatic Secrets")]
        [SerializeField] int hiddenRoomCount = 1;
        [SerializeField] int loreTabletCount = 1;
        [SerializeField] int easterEggCount = 1;
        [SerializeField] int shortcutCount = 1;
        [SerializeField] int specialCollectibleCount = 1;
        List<GameObject> secrets = new List<GameObject>();
        void Start() { CreateSecrets(); }
        void CreateSecrets()
        {
            CreateHiddenRoom("HiddenRoom_Prismatic", new Vector3(Random.Range(-70f, 70f), 0.5f, Random.Range(-70f, 70f)), new Vector3(0f, 5f, 0f));
            CreateLoreTablet("LoreTablet_Prismatic", new Vector3(Random.Range(-60f, 60f), 1f, Random.Range(-60f, 60f)), "Ancient lore of the Prismatic moon...");
            CreateEasterEgg("EasterEgg_Prismatic", new Vector3(Random.Range(-65f, 65f), 1f, Random.Range(-65f, 65f)), "Developer secret");
            CreateShortcut("Shortcut_Prismatic", new Vector3(-70f, 0.5f, -70f), new Vector3(70f, 0.5f, 70f));
            CreateSpecialCollectible("SpecialItem_Prismatic", new Vector3(Random.Range(-60f, 60f), 2f, Random.Range(-60f, 60f)));
            Debug.Log($"🔍 Moon11Secrets: {secrets.Count} secrets created");
        }
        GameObject CreateHiddenRoom(string name, Vector3 entrance, Vector3 interior) { GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube); obj.name = name; obj.transform.position = entrance; obj.transform.localScale = new Vector3(5f, 5f, 5f); var secret = obj.AddComponent<Moon11SecretInteractable>(); secret.secretType = "hiddenRoom"; secret.secretName = name; secret.rsReward = 50f; secrets.Add(obj); return obj; }
        GameObject CreateLoreTablet(string name, Vector3 pos, string loreText) { GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube); obj.name = name; obj.transform.position = pos; obj.transform.localScale = new Vector3(2f, 3f, 0.2f); var secret = obj.AddComponent<Moon11SecretInteractable>(); secret.secretType = "loreTablet"; secret.secretName = name; secret.description = loreText; secret.rsReward = 35f; secrets.Add(obj); return obj; }
        GameObject CreateEasterEgg(string name, Vector3 pos, string reference) { GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere); obj.name = name; obj.transform.position = pos; obj.transform.localScale = Vector3.one * 0.5f; var secret = obj.AddComponent<Moon11SecretInteractable>(); secret.secretType = "easterEgg"; secret.secretName = name; secret.description = reference; secret.rsReward = 25f; Renderer rend = obj.GetComponent<Renderer>(); if (rend != null) { Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit")); mat.color = new Color(1f, 0.8f, 0.2f); mat.EnableKeyword("_EMISSION"); mat.SetColor("_EmissionColor", Color.yellow * 2f); rend.material = mat; } secrets.Add(obj); return obj; }
        GameObject CreateShortcut(string name, Vector3 start, Vector3 end) { GameObject obj = new GameObject(name); obj.transform.position = start; var secret = obj.AddComponent<Moon11SecretInteractable>(); secret.secretType = "shortcut"; secret.secretName = name; secret.rsReward = 40f; secrets.Add(obj); return obj; }
        GameObject CreateSpecialCollectible(string name, Vector3 pos) { GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere); obj.name = name; obj.transform.position = pos; obj.transform.localScale = Vector3.one * 0.7f; var secret = obj.AddComponent<Moon11SecretInteractable>(); secret.secretType = "specialCollectible"; secret.secretName = name; secret.rsReward = 50f; Renderer rend = obj.GetComponent<Renderer>(); if (rend != null) { Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit")); mat.color = new Color(1f, 0.6f, 1f); mat.EnableKeyword("_EMISSION"); mat.SetColor("_EmissionColor", new Color(2f, 1f, 2f)); rend.material = mat; } BobAnimation bobber = obj.AddComponent<BobAnimation>(); bobber.bobSpeed = 0.8f; bobber.bobHeight = 0.5f; bobber.rotationSpeed = 30f; secrets.Add(obj); return obj; }
        void OnDestroy() { foreach (var obj in secrets) if (obj != null) Destroy(obj); secrets.Clear(); }
    }
    public class Moon11SecretInteractable : MonoBehaviour, IInteractable
    {
        public string secretType;
        public string secretName;
        public string description;
        public float rsReward;
        bool discovered;
        public string GetInteractPrompt() { return discovered ? "" : $"Discover {secretName} (E)"; }
        public void Interact(GameObject player) { if (discovered) return; discovered = true; Core.GameLoopController.Instance?.QueueRSReward(rsReward, "secret"); Core.GameEvents.RaiseHUDShowObjective($"Secret Found: {secretName} (+{rsReward} RS)"); Audio.AudioManager.Instance?.PlaySFX2D("SecretDiscovered"); Debug.Log($"[Secret] Discovered: {secretName}"); }
    }
}
