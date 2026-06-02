using UnityEngine;
using UnityEngine.AI;

namespace Tartaria.Animation
{
    /// <summary>
    /// Drives the EchohavenHumanoid.controller "IsWalking" bool based on the NPC's
    /// current motion. Reads NavMeshAgent.velocity when available; falls back to
    /// per-frame position delta so it still works for NPCs scripted via transform
    /// tweens or other non-agent movement.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    public class NPCWalkAnimator : MonoBehaviour
    {
        [SerializeField] float walkSpeedThreshold = 0.15f;

        Animator _anim;
        NavMeshAgent _nav;
        Vector3 _lastPos;
        int _isWalkingHash;

        void Awake()
        {
            _anim = GetComponent<Animator>();
            _nav = GetComponent<NavMeshAgent>();
            _isWalkingHash = Animator.StringToHash("IsWalking");
            _lastPos = transform.position;
        }

        void Update()
        {
            float speed;
            if (_nav != null && _nav.isActiveAndEnabled)
            {
                speed = _nav.velocity.magnitude;
            }
            else
            {
                speed = (transform.position - _lastPos).magnitude / Mathf.Max(Time.deltaTime, 0.001f);
                _lastPos = transform.position;
            }

            if (_anim != null && _anim.runtimeAnimatorController != null)
            {
                _anim.SetBool(_isWalkingHash, speed > walkSpeedThreshold);
            }
        }
    }
}
