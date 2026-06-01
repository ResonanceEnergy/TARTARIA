---

## Build & Player Settings — Moon 1 Development Build Configuration (2026-05-20)

**Build & Player Settings Agent — Mission Complete**

**Final Recommended Configuration for Moon 1 (Echohaven_VerticalSlice) Development Builds:**

- **EditorBuildSettings (first scene)**: `Assets/_Project/Scenes/Echohaven_VerticalSlice.unity` as index 0 (direct launch). Boot, moons, and UI_Overlay follow or are selectively included for focused dev. Use menu `Tartaria > Configure Moon 1 Dev Build Settings (Echohaven first)` to apply.
- **Build Pipeline**: `Tartaria > Build Development Standalone (Windows x64) — Moon 1 (Echohaven first)` in BuildPlayerPipeline.cs. Sets DevelopmentBuild | AllowDebugging | ConnectWithProfiler + calls the Moon1 config + PlayerSettings.
- **Player Settings (dev + perf)**:
  - Product: "TARTARIA - Moon 1 Vertical Slice (Dev)"
  - Company: "Resonance Forge"
  - Version: "0.9.0-Moon1-Dev"
  - Resolution: 1280x720 Windowed (fast iteration)
  - RunInBackground: true
  - Scripting Backend: Mono2x (dev) / IL2CPP (release perf)
  - API Level: .NET 4.6
  - Graphics APIs: Direct3D11 + Vulkan priority
  - vSync: off in dev for profiling
  - Call: `OneClickBuild.ConfigureRecommendedPlayerSettings(true)`
- **Fixed build errors**: Added Unity.Addressables + Unity.ResourceManager to Tartaria.Core.asmdef; added Tartaria.Save to Tartaria.Gameplay.asmdef. Eliminated Addressable, cross-namespace, and Save reference compile failures blocking dev builds.
- **Launch**: After dev build, run `Build/Windows/Tartaria.exe` — starts directly in Echohaven_VerticalSlice with runtime bootstraps (GameBootstrap, SceneLoader) auto-initializing via RuntimeInitializeOnLoad. Clean, fast Moon 1 iteration.
- **OneClickBuild integration**: Phase 10 still uses full (Boot first) for complete vertical slice; dev menus override for Moon 1 focus.
- **Verification**: Run the dev build menu or batch `-executeMethod Tartaria.EditorTools.BuildPlayerPipeline.BuildWindowsDevMoon1`. Echohaven loads cleanly; no Boot dependency for startup scene.

**Files updated**:
- `ProjectSettings/EditorBuildSettings.asset` (Echohaven first)
- `Assets/_Project/Editor/MoonScenesFactory.cs` (new ConfigureMoon1DevBuildSettings + menu)
- `Assets/_Project/Editor/OneClickBuild.cs` (ConfigureRecommendedPlayerSettings)
- `Assets/_Project/Editor/BuildPlayerPipeline.cs` (new Moon1 dev build entry)
- `Assets/_Project/Scripts/Core/Tartaria.Core.asmdef`, `Assets/_Project/Scripts/Gameplay/Tartaria.Gameplay.asmdef` (reference fixes)
- `CONTEXT.md`: this section.

**Production note**: For full game builds use normal pipeline (Boot first). Moon 1 dev builds are the fast path for Echohaven tuning, combat, quests, and slice validation.

**Absolute paths**: All C:\dev\TARTARIA_new\...
