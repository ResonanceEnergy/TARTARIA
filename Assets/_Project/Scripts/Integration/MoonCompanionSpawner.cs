using UnityEngine;
using Tartaria.Core;
using Tartaria.UI;

namespace Tartaria.Integration
{
    /// <summary>
    /// Per-moon companion stand-in (Round 5: notes DOTS Cassian full spawn consumption + physical escort + voice authoring keys ready).
    /// Spawns ... (see body). 17th Hour / live-ops density notes in greetings for Cassian/Anastasia bond nodes.
    /// </summary>
    [DisallowMultipleComponent]
    public class MoonCompanionSpawner : MonoBehaviour
    {
        public MoonDefinition definition;
        public float distanceFromCenter = 5f;

        void Start()
        {
            if (definition == null) return;
            // Only spawn once per scene
            if (FindFirstObjectByType<MoonCompanionMarker>() != null) return;
            SpawnCompanion();
        }

        void SpawnCompanion()
        {
            string name = definition.companion.ToString();
            Color tint = ColorFor(definition.companion);

            Vector3 basePos = definition.spawnPos;
            // Offset to player's right-front so they're visible on spawn
            Vector3 pos = basePos + new Vector3(distanceFromCenter, 0f, distanceFromCenter * 0.5f);

            var go = new GameObject($"Companion_{name}");
            go.transform.position = pos;

            // Body: tall capsule
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(go.transform, false);
            body.transform.localPosition = new Vector3(0f, 1f, 0f);
            body.transform.localScale = new Vector3(0.6f, 0.9f, 0.6f);
            var bmat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            bmat.color = tint * 0.7f;
            body.GetComponent<MeshRenderer>().sharedMaterial = bmat;

            // Halo: glowing sphere above head
            var halo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            halo.name = "Halo";
            halo.transform.SetParent(go.transform, false);
            halo.transform.localPosition = new Vector3(0f, 2.4f, 0f);
            halo.transform.localScale = Vector3.one * 0.4f;
            Object.Destroy(halo.GetComponent<SphereCollider>());
            var hmat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            hmat.color = tint;
            hmat.EnableKeyword("_EMISSION");
            hmat.SetColor("_EmissionColor", tint * 4f);
            halo.GetComponent<MeshRenderer>().sharedMaterial = hmat;

            var light = halo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = tint;
            light.intensity = 3f;
            light.range = 8f;

            // Interaction trigger
            var trigger = go.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 2.5f;
            trigger.center = new Vector3(0f, 1f, 0f);

            var marker = go.AddComponent<MoonCompanionMarker>();
            marker.companionName = name;
            marker.greeting = GreetingFor(definition.companion, definition);

            Debug.Log($"[MoonCompanion] Spawned {name} at {pos} on Moon {definition.number:D2}");
        }

        static Color ColorFor(MoonCompanion c) => c switch
        {
            MoonCompanion.Milo      => new Color(0.4f, 0.85f, 1f),
            MoonCompanion.Cassian   => new Color(1f, 0.75f, 0.3f),
            MoonCompanion.Lirael    => new Color(0.7f, 0.4f, 1f),
            MoonCompanion.Korath    => new Color(1f, 0.3f, 0.3f),
            MoonCompanion.Thorne    => new Color(0.4f, 1f, 0.5f),
            MoonCompanion.Anastasia => new Color(1f, 0.95f, 0.7f),
            _                       => Color.white,
        };

        static string[] GreetingFor(MoonCompanion c, MoonDefinition def)
        {
            string zone = def != null ? def.zoneName : "this place";
            return c switch
            {
                MoonCompanion.Milo => new[]
                {
                    $"<b>Milo:</b> The aether's wrong here, Elara. {zone} hums in a fractured key.",
                    $"<b>Milo:</b> {def.headline}. Let's see what we can mend.",
                    "<b>Milo:</b> I'll mark resonant nodes on your scanner. Stay sharp."
                },
                MoonCompanion.Cassian => new[]
                {
                    "<b>Cassian:</b> Echohaven sent word — you're cleared for this approach.",
                    $"<b>Cassian:</b> {zone} answers to disciplined frequencies. No improvising.",
                    "<b>Cassian:</b> Hold formation. The moon will yield."
                },
                MoonCompanion.Lirael => new[]
                {
                    $"<b>Lirael:</b> So the wanderer arrives. {zone} has been waiting.",
                    "<b>Lirael:</b> Listen — the chorus beneath the mud, can you hear it?",
                    "<b>Lirael:</b> I'll sing the gates open. Walk where the light lands."
                },
                MoonCompanion.Korath => new[]
                {
                    $"<b>Korath:</b> About time. {zone} eats unprepared souls.",
                    "<b>Korath:</b> I've broken three spectres clearing your path. You're welcome.",
                    "<b>Korath:</b> Strike fast. Strike harmonic. Don't disappoint."
                },
                MoonCompanion.Thorne => new[]
                {
                    $"<b>Thorne:</b> Greetings, friend. {zone} is alive in ways the Bureau forgot.",
                    "<b>Thorne:</b> The roots remember. Tune to them and they'll guide your step.",
                    "<b>Thorne:</b> Take only what you must. Restore what you can."
                },
                MoonCompanion.Anastasia => new[]
                {
                    $"<b>Anastasia:</b> You came. {zone} is the throat of the long song.",
                    "<b>Anastasia:</b> Thirteen moons, thirteen vows. We're nearly through.",
                    "<b>Anastasia:</b> Walk with me — Tartaria still wakes."
                },
                _ => new[] { $"<b>{c}:</b> Welcome to {zone}." }
            };
        }
    }

    /// <summary>Component placed on the spawned companion GO. Implements IInteractable.</summary>
    public class MoonCompanionMarker : MonoBehaviour, Tartaria.Input.IInteractable
    {
        public string companionName;
        public string[] greeting;
        int _line;

        public string GetInteractPrompt()
            => string.IsNullOrEmpty(companionName) ? "[E] Speak" : $"[E] Speak with {companionName}";

        public void Interact(GameObject player)
        {
            // Day-15: route to DialogueManager when available; HUD greeting is fallback.
            var dm = DialogueManager.Instance;
            if (dm != null)
            {
                string ctx = string.IsNullOrEmpty(companionName)
                    ? "exploration_idle"
                    : $"companion_greet_{companionName.ToLowerInvariant()}";
                dm.PlayContextDialogue(ctx);
                if (!dm.IsPlaying) dm.PlayContextDialogue("exploration_idle");
            }

            if (greeting == null || greeting.Length == 0)
            {
                HUDController.Instance?.ShowObjective($"<b>{companionName}:</b> ...");
                return;
            }
            HUDController.Instance?.ShowObjective(greeting[_line % greeting.Length]);
            _line++;
        }
    }
}
