using Unity.Entities;
using Unity.Mathematics;

namespace Tartaria.Core
{
    // ─────────────────────────────────────────────
    //  Aether Harmonic Bands (3-6-9)
    // ─────────────────────────────────────────────
    public enum HarmonicBand : byte
    {
        Telluric  = 3,  // 7.83 Hz — Schumann Resonance — deep amber
        Harmonic  = 6,  // 432 Hz  — universal healing  — bright gold
        Celestial = 9   // 1296 Hz — 3×432             — white-gold
    }

    // ─────────────────────────────────────────────
    //  Aether Node — each voxel/point in the field
    // ─────────────────────────────────────────────
    public struct AetherNode : IComponentData
    {
        public float3 WorldPosition;
        public float Intensity;       // 0–1
        public HarmonicBand Band;
        public float Frequency;       // Hz
        public float Coherence;       // purity factor 0–1
    }

    // ─────────────────────────────────────────────
    //  Aether Source — emitted by restored buildings
    // ─────────────────────────────────────────────
    public struct AetherSource : IComponentData
    {
        public float Strength;        // 0–1 (dome=1.0, fountain=0.6, spire=0.8)
        public float Radius;          // meters (dome=50, fountain=30, spire=40)
        public HarmonicBand Band;
    }

    // ─────────────────────────────────────────────
    //  Aether Sink — corruption absorbs aether
    // ─────────────────────────────────────────────
    public struct AetherSink : IComponentData
    {
        public float Strength;        // negative (golem=-0.5, corruption=-0.2)
        public float Radius;          // meters
    }

    // ─────────────────────────────────────────────
    //  Aether Field Cell — 3D grid simulation data
    // ─────────────────────────────────────────────
    public struct AetherFieldCell : IBufferElementData
    {
        public float Density;
        public float3 FlowVelocity;
        public HarmonicBand Band;
    }

    // ─────────────────────────────────────────────
    //  Aether States
    // ─────────────────────────────────────────────
    public enum AetherState : byte
    {
        Dormant    = 0,  // Buried/blocked — no glow
        Flowing    = 1,  // Active in tuned structures — golden veins
        Surplus    = 2,  // Excess beyond need — bright arcs
        Overloaded = 3,  // Past capacity — chain lightning
        Corrupted  = 4   // Inverted by dissonance — black sludge
    }

    public struct AetherStateComponent : IComponentData
    {
        public AetherState State;
    }

    // ─────────────────────────────────────────────
    //  Aether Collector — player proximity harvesting
    // ─────────────────────────────────────────────
    public struct AetherCollector : IComponentData
    {
        public float CollectionRadius;   // 15m default
        public float CollectionRate;     // units per second
        public int ConsecutiveCollections;
    }

    // ─────────────────────────────────────────────
    //  Global Aether Field Configuration (singleton)
    // ─────────────────────────────────────────────
    public struct AetherFieldConfig : IComponentData
    {
        public int GridSizeX;           // 64 default
        public int GridSizeY;           // 64 default
        public int GridSizeZ;           // 32 default
        public float CellSize;          // 2m default
        public float DissipationRate;   // φ-proportioned
        public float AdvectionSpeed;
    }
}
