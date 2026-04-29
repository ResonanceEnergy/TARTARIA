using UnityEngine;
using Tartaria.Input;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Procedural player animator — drives limb / body motion using sin curves
    /// based on movement speed. No animation clips required.
    /// Animates: Arm_L, Arm_R, Leg_L, Leg_R, Body, Head, AetherCore (emission pulse).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerAnimator : MonoBehaviour
    {
        [Header("Tuning")]
        [SerializeField] float walkFrequency = 8f;     // strides per sec when moving
        [SerializeField] float idleFrequency = 0.8f;   // breathing rate
        [SerializeField] float armSwingDeg = 35f;
        [SerializeField] float legSwingDeg = 28f;
        [SerializeField] float bodyBobAmp = 0.06f;
        [SerializeField] float idleBobAmp = 0.015f;
        [SerializeField] float aetherPulseSpeed = 2.2f;

        Transform _armL, _armR, _legL, _legR, _body, _head, _aetherCore;
        Quaternion _armLRest, _armRRest, _legLRest, _legRRest;
        Vector3 _bodyRest, _headRest, _aetherRest;
        PlayerInputHandler _input;
        Renderer _aetherRenderer;
        MaterialPropertyBlock _aetherMpb;
        static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
        Color _aetherBaseEmission = new Color(0.4f, 0.85f, 1.0f);

        float _phase;

        void Awake()
        {
            _input = GetComponent<PlayerInputHandler>();
            _armL = transform.Find("Arm_L");
            _armR = transform.Find("Arm_R");
            _legL = transform.Find("Leg_L");
            _legR = transform.Find("Leg_R");
            _body = transform.Find("Body");
            _head = transform.Find("Head");
            _aetherCore = transform.Find("AetherCore");

            if (_armL != null) _armLRest = _armL.localRotation;
            if (_armR != null) _armRRest = _armR.localRotation;
            if (_legL != null) _legLRest = _legL.localRotation;
            if (_legR != null) _legRRest = _legR.localRotation;
            if (_body != null) _bodyRest = _body.localPosition;
            if (_head != null) _headRest = _head.localPosition;
            if (_aetherCore != null)
            {
                _aetherRest = _aetherCore.localScale;
                _aetherRenderer = _aetherCore.GetComponent<Renderer>();
                _aetherMpb = new MaterialPropertyBlock();
            }
        }

        void Update()
        {
            bool moving = _input != null && _input.IsMoving;
            float freq = moving ? walkFrequency : idleFrequency;
            _phase += Time.deltaTime * freq;

            float s = Mathf.Sin(_phase);
            float swingAmp = moving ? 1f : 0.15f;

            // Arms swing opposite to each other
            if (_armL != null)
                _armL.localRotation = _armLRest * Quaternion.Euler(s * armSwingDeg * swingAmp, 0f, 0f);
            if (_armR != null)
                _armR.localRotation = _armRRest * Quaternion.Euler(-s * armSwingDeg * swingAmp, 0f, 0f);

            // Legs swing opposite to arms (and to each other)
            if (_legL != null)
                _legL.localRotation = _legLRest * Quaternion.Euler(-s * legSwingDeg * swingAmp, 0f, 0f);
            if (_legR != null)
                _legR.localRotation = _legRRest * Quaternion.Euler(s * legSwingDeg * swingAmp, 0f, 0f);

            // Body bob — uses double-frequency cosine so feet "land" on each beat
            float bobAmp = moving ? bodyBobAmp : idleBobAmp;
            float bob = Mathf.Abs(Mathf.Cos(_phase)) * bobAmp;
            if (_body != null)
                _body.localPosition = _bodyRest + new Vector3(0f, bob, 0f);
            if (_head != null)
                _head.localPosition = _headRest + new Vector3(0f, bob, 0f);

            // Aether core pulse — emission + scale
            if (_aetherCore != null)
            {
                float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * aetherPulseSpeed);
                float scaleMul = 1f + pulse * 0.15f;
                _aetherCore.localScale = _aetherRest * scaleMul;
                if (_aetherRenderer != null)
                {
                    float intensity = 1.5f + pulse * 2.5f;
                    _aetherRenderer.GetPropertyBlock(_aetherMpb);
                    _aetherMpb.SetColor(EmissionColorId, _aetherBaseEmission * intensity);
                    _aetherRenderer.SetPropertyBlock(_aetherMpb);
                }
            }
        }
    }
}
