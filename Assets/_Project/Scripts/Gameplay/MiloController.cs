using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Milo - Companion NPC, sells "antique" bricks, provides comic relief
    /// Appears from behind dumpster, comments on player discoveries
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class MiloController : MonoBehaviour
    {
        [Header("Companion Settings")]
        public float followDistance = 3f;
        public float followSpeed = 4f;
        public float rotationSpeed = 5f;
        
        [Header("Dialogue")]
        public List<string> discov

eryQuips = new List<string>()
        {
            "Blimey... that thing just lit up like someone flipped a switch in the sky.",
            "I'"'"'ve been selling '"'"'antique'"'"' bricks for years and none of '"'"'em ever glowed.",
            "You look like you just saw ghosts in top hats dancing on airships.",
            "Genuine 1893 mud brick! Only slightly cursed!",
            "That cathedral hum... gives me goosebumps. Good ones, mind you."
        };
        
        [Header("Shop System")]
        public bool shopUnlocked = true;
        public int mudBrickPrice = 50; // Aether currency
        public int resonanceCrystalPrice = 200;
        
        private Transform player;
        private CharacterController controller;
        private int lastQuipIndex = -1;
        
        void Start()
        {
            controller = GetComponent<CharacterController>();
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            
            if (player == null)
            {
                Debug.LogWarning("[Milo] Player not found. Companion disabled.");
                enabled = false;
            }
        }
        
        void Update()
        {
            if (player == null) return;
            
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            
            // Follow player if too far
            if (distanceToPlayer > followDistance)
            {
                Vector3 direction = (player.position - transform.position).normalized;
                controller.SimpleMove(direction * followSpeed);
                
                // Rotate to face movement direction
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }
            }
        }
        
        /// <summary>
        /// Call this when player makes a discovery (excavation, tuning success, etc)
        /// </summary>
        public void ReactToDiscovery(string discoveryType)
        {
            int quipIndex;
            do
            {
                quipIndex = Random.Range(0, discoveryQuips.Count);
            } while (quipIndex == lastQuipIndex && discoveryQuips.Count > 1);
            
            lastQuipIndex = quipIndex;
            
            Debug.Log($"[Milo] {discoveryQuips[quipIndex]}");
            // TODO: Connect to dialogue UI system
        }
        
        /// <summary>
        /// Opens Milo'"'"'s shop menu
        /// </summary>
        public void OpenShop()
        {
            if (!shopUnlocked)
            {
                Debug.Log("[Milo] Shop not unlocked yet, mate. Help me clear out this cathedral first!");
                return;
            }
            
            Debug.Log($"[Milo'"'"'s Shop] Mud Brick: {mudBrickPrice} AE | Resonance Crystal: {resonanceCrystalPrice} AE");
            // TODO: Connect to shop UI system
        }
    }
}