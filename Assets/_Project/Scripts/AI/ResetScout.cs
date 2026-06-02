using UnityEngine;
using Tartaria.Core;
using Tartaria.Gameplay.Combat;

namespace Tartaria.AI
{
    [RequireComponent(typeof(CharacterController))]
    public class ResetScout : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private float maxHealth = 60f;
        [SerializeField] private float speed = 3.2f;
        [SerializeField] private float attackRange = 6f;
        [SerializeField] private float attackInterval = 1.8f;
        [SerializeField] private float attackDamage = 6f;
        [SerializeField] private float aggroRange = 22f;

        [Header("Rewards on kill")]
        [SerializeField] private float rsReward = 8f;

        private float _hp;
        private Transform _player;
        private CharacterController _cc;
        private float _nextAttackAt;
        private bool _dead;

        public float MaxHealth => maxHealth;
        public float CurrentHealth => _hp;
        public bool IsAlive => !_dead && _hp > 0f;

        void Awake()
        {
            _hp = maxHealth;
            _cc = GetComponent<CharacterController>();
            if (_cc == null) _cc = gameObject.AddComponent<CharacterController>();
            _cc.height = 2f;
            _cc.radius = 0.45f;
            _cc.center = new Vector3(0f, 1f, 0f);
            gameObject.tag = "Enemy";
            EnsureVisual();
        }

        void EnsureVisual()
        {
            if (transform.childCount > 0) return;
            var urpLit = Shader.Find("Universal Render Pipeline/Lit");

            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(transform);
            body.transform.localPosition = new Vector3(0f, 1f, 0f);
            body.transform.localScale = new Vector3(0.7f, 0.95f, 0.7f);
            Destroy(body.GetComponent<Collider>());
            ApplyURP(body, urpLit, new Color(0.18f, 0.16f, 0.20f));

            var hat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hat.name = "TopHat";
            hat.transform.SetParent(transform);
            hat.transform.localPosition = new Vector3(0f, 2.1f, 0f);
            hat.transform.localScale = new Vector3(0.55f, 0.35f, 0.55f);
            Destroy(hat.GetComponent<Collider>());
            ApplyURP(hat, urpLit, new Color(0.06f, 0.06f, 0.08f));

            var clipboard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            clipboard.name = "Clipboard";
            clipboard.transform.SetParent(transform);
            clipboard.transform.localPosition = new Vector3(0.35f, 1.1f, 0.3f);
            clipboard.transform.localRotation = Quaternion.Euler(0f, 25f, 8f);
            clipboard.transform.localScale = new Vector3(0.32f, 0.42f, 0.05f);
            Destroy(clipboard.GetComponent<Collider>());
            ApplyURP(clipboard, urpLit, new Color(0.65f, 0.18f, 0.18f));
        }

        static void ApplyURP(GameObject go, Shader urpLit, Color color)
        {
            var rend = go.GetComponent<Renderer>();
            if (rend == null) return;
            if (urpLit == null) { rend.material.color = color; return; }
            var mat = new Material(urpLit);
            mat.SetColor("_BaseColor", color);
            rend.sharedMaterial = mat;
        }

        void Update()
        {
            if (_dead) return;
            if (_player == null)
            {
                var p = GameObject.FindWithTag("Player");
                if (p == null) return;
                _player = p.transform;
            }
            float dist = Vector3.Distance(transform.position, _player.position);
            if (dist > aggroRange) return;
            Vector3 toPlayer = (_player.position - transform.position);
            toPlayer.y = 0f;
            if (toPlayer.sqrMagnitude > 0.01f)
            {
                Quaternion target = Quaternion.LookRotation(toPlayer.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, target, 6f * Time.deltaTime);
            }
            if (dist > attackRange)
            {
                Vector3 move = toPlayer.normalized * speed * Time.deltaTime;
                move.y = -9.81f * Time.deltaTime;
                _cc.Move(move);
            }
            else if (Time.time >= _nextAttackAt)
            {
                _nextAttackAt = Time.time + attackInterval;
                PerformAttack();
            }
        }

        void PerformAttack()
        {
            if (_player == null) return;
            if (Vector3.Distance(transform.position, _player.position) > attackRange + 1f) return;
            _player.SendMessage("TakeDamage", (int)attackDamage, SendMessageOptions.DontRequireReceiver);

            // Sprint 7 Lane 7: HitFeedback feedback (popup + hitstop + shake)
            try { HitFeedback.NotifyHit(_player.position, attackDamage, false); }
            catch (System.NullReferenceException) { Debug.LogWarning("[HitCallSite] HitFeedback not initialized at ResetScout.cs:PerformAttack"); }
        }

        public void TakeDamage(float damage, GameObject instigator = null)
        {
            if (_dead) return;
            _hp -= damage;
            if (_hp <= 0f) Die(instigator);
        }

        void Die(GameObject instigator)
        {
            _dead = true;
            GameEvents.FireRSChange(rsReward);
            ServiceLocator.HUD?.ShowBanner("Clipboard fragment",
                "Per Bureau directive 3-9: all anomalies to be cataloged for demolition.", 3.5f);
            Destroy(gameObject, 1.2f);
        }
    }
}
