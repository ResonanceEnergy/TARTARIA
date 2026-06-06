# TARTARIA — Troubleshooting Guide (Beta)

Quick reference for common issues and solutions during beta testing.

---

## Game Won't Launch

### Symptom: Double-click Tartaria.exe, nothing happens
**Causes:**
- Missing DirectX runtime
- Antivirus blocking executable
- Corrupted download/extraction

**Fixes:**
1. Right-click Tartaria.exe → Run as Administrator
2. Install [DirectX End-User Runtime](https://www.microsoft.com/en-us/download/details.aspx?id=35)
3. Add Tartaria.exe to antivirus exceptions
4. Re-extract .zip archive (ensure no corruption)
5. Check Windows Event Viewer (Application log) for crash details

---

### Symptom: Crash on startup with "Unity Player" error dialog
**Causes:**
- Outdated GPU drivers
- Incompatible graphics hardware
- Missing Visual C++ redistributables

**Fixes:**
1. Update GPU drivers:
   - NVIDIA: [GeForce Experience](https://www.nvidia.com/en-us/geforce/geforce-experience/)
   - AMD: [Radeon Software](https://www.amd.com/en/support)
   - Intel: [Driver & Support Assistant](https://www.intel.com/content/www/us/en/support/detect.html)
2. Install [Visual C++ Redistributable 2015-2022](https://aka.ms/vs/17/release/vc_redist.x64.exe)
3. Check minimum requirements (GTX 1050 / 4 GB VRAM)

---

## Performance Issues

### Symptom: Low FPS (below 30 fps)
**Causes:**
- Hardware below minimum spec
- Background applications consuming resources
- Wrong quality settings for hardware tier

**Fixes:**
1. Close background applications (Chrome, Discord, OBS, etc.)
2. Open Settings (F10) → Graphics:
   - Lower Quality preset (Medium → Low)
   - Reduce Resolution (1080p → 720p)
   - Disable Fullscreen (windowed mode can improve performance on some systems)
3. Check Task Manager → GPU/CPU usage during gameplay
4. Update GPU drivers (see above)
5. Disable Windows Game Bar (Settings → Gaming → Game Bar → Off)

---

### Symptom: Stuttering / Frame drops during restoration VFX
**Causes:**
- GPU particle system overload
- VRAM capacity reached
- Disk I/O bottleneck (slow HDD)

**Fixes:**
1. Close all background applications
2. Move game to SSD if available
3. Settings → Graphics → Quality: Low
4. Reduce Resolution to 720p
5. Check GPU temperature (overheating throttles performance)

---

### Symptom: Audio crackling / popping
**Causes:**
- Audio buffer underrun
- Incompatible audio drivers
- Windows audio enhancements interfering

**Fixes:**
1. Settings → Audio → Reduce Master Volume (sometimes high volume causes driver distortion)
2. Disable Windows audio enhancements:
   - Right-click speaker icon → Sounds → Playback tab
   - Select output device → Properties → Enhancements tab
   - Check "Disable all enhancements"
3. Update audio drivers
4. Close other audio applications (Spotify, YouTube, etc.)

---

## Gameplay Issues

### Symptom: Can't interact with buildings (Press E doesn't work)
**Causes:**
- Wrong GameObject layer (not on Interactable layer 9)
- Out of interaction range
- Tutorial not progressed far enough

**Fixes:**
1. Walk closer to building (within 5 meters)
2. Look directly at building (crosshair must be on building mesh)
3. Wait for Milo companion intro to complete (~3 seconds after spawn)
4. Check that "Press E to interact" prompt appears on HUD
5. If prompt doesn't appear, report bug via GitHub Issues

---

### Symptom: Save/Load not working (Continue button grayed out)
**Causes:**
- No save file created yet
- Save file corrupted
- Wrong save directory path

**Fixes:**
1. Ensure you've reached first save checkpoint (after restoring Great Dome)
2. Check save file location:
   - `%USERPROFILE%\AppData\LocalLow\ResonanceEnergy\Tartaria\Saves\`
3. If save exists but Continue grayed out, report bug with save file attached
4. Workaround: Start New Game (save system will create fresh file after first checkpoint)

---

### Symptom: Camera stuck / can't look around
**Causes:**
- Mouse sensitivity too low
- Gamepad input overriding mouse
- Camera collision with terrain/building

**Fixes:**
1. Settings (F10) → Mouse Sensitivity → Increase slider
2. If using gamepad + mouse simultaneously, unplug gamepad (input conflict)
3. Move player away from walls/buildings (camera may be clipping)
4. Press Esc → Resume to reset camera state

---

### Symptom: Player falling through floor / stuck in geometry
**Causes:**
- NavMesh collision bug
- Physics timestep desync
- Terrain hole

**Fixes:**
1. Report bug with exact location (screenshot + coordinates)
2. Workaround: Esc → Load Game (respawn at last checkpoint)
3. If issue repeats at same location, avoid that area and report

---

## Controls Issues

### Symptom: Gamepad not detected
**Causes:**
- Controller not connected before game launch
- Incompatible controller (non-XInput)
- Windows drivers not installed

**Fixes:**
1. Ensure controller is plugged in BEFORE launching Tartaria.exe
2. Test controller in Windows → Settings → Devices → Bluetooth & other devices → Xbox Accessories
3. For PlayStation controllers: Use [DS4Windows](http://ds4windows.com/) for XInput emulation
4. Restart game after connecting controller

---

### Symptom: Gamepad buttons mapped wrong
**Causes:**
- Non-standard controller layout
- Regional controller variant
- Input System detecting wrong device

**Fixes:**
1. Check controller type is Xbox or PlayStation (other brands may have non-standard mappings)
2. Report specific button mapping issues via GitHub Issues (include controller model)
3. Workaround: Use keyboard + mouse until mapping fixed

---

### Symptom: Keyboard keys not responding
**Causes:**
- Another application capturing input
- Keyboard language/layout conflict
- Unity Input System not initialized

**Fixes:**
1. Close overlay applications (Discord overlay, Steam overlay, MSI Afterburner, etc.)
2. Switch to US English keyboard layout (some keys may not map on non-QWERTY layouts)
3. Alt-Tab out and back in (sometimes refocuses input)
4. Restart game

---

## Graphics Issues

### Symptom: Black screen with audio playing
**Causes:**
- Display mode mismatch (fullscreen exclusive vs. windowed)
- Multi-monitor setup confusion
- GPU driver crash

**Fixes:**
1. Alt-Enter to toggle fullscreen/windowed mode
2. If multi-monitor: Move mouse to other screens to find game window
3. Check Task Manager → Tartaria.exe is running → Right-click → Bring to front
4. Ctrl-Alt-Delete → Sign out → Sign back in (resets GPU state)
5. Update GPU drivers

---

### Symptom: Textures missing / pink materials
**Causes:**
- Shader compilation failure
- Corrupted game files
- GPU incompatibility with URP shaders

**Fixes:**
1. Verify game files (if downloaded via Steam/itch.io)
2. Delete `Tartaria_Data\StreamingAssets\` folder → Re-extract from .zip
3. Update GPU drivers
4. Report bug with screenshot via GitHub Issues

---

### Symptom: Screen flickering / artifacts
**Causes:**
- GPU overheating
- VSync disabled causing tearing
- Unstable GPU overclock

**Fixes:**
1. Check GPU temperature (use MSI Afterburner or HWMonitor)
2. Settings → Graphics → Enable VSync
3. Reduce Quality preset to Medium or Low
4. Reset GPU overclock to stock speeds
5. Update GPU drivers

---

## Crash & Error Reporting

### How to Report Bugs
1. Go to [GitHub Issues](https://github.com/ResonanceEnergy/TARTARIA/issues)
2. Click "New Issue" → "Bug Report" template
3. Include:
   - **System specs:** GPU, CPU, RAM, OS version
   - **Steps to reproduce:** What you were doing when issue occurred
   - **Expected behavior:** What should have happened
   - **Actual behavior:** What actually happened
   - **Player.log file:** Located at `%USERPROFILE%\AppData\LocalLow\ResonanceEnergy\Tartaria\Player.log`
   - **Screenshots/video:** If visual bug
4. Submit issue

---

### Where to Find Logs
**Player.log (Runtime logs):**
- Windows: `%USERPROFILE%\AppData\LocalLow\ResonanceEnergy\Tartaria\Player.log`
- Contains full game session log, errors, warnings, stack traces

**Save Files:**
- Windows: `%USERPROFILE%\AppData\LocalLow\ResonanceEnergy\Tartaria\Saves\`
- Format: `save_slot_X.json` (human-readable JSON)

**How to Attach Logs:**
- Copy Player.log to desktop
- Rename to `Player_YOURNAME_DATE.log`
- Attach to GitHub Issue or upload to [Pastebin](https://pastebin.com/)

---

## Known Issues (Current Beta)

### Not Yet Implemented
- Full 13-Moon campaign (only Moon 1 Echohaven available)
- Giant Mode mechanics (teased but not playable)
- Skill trees / character progression
- Multiplayer
- Cloud saves
- Full voice acting (placeholder beep tones)
- Mac/Linux builds

### Planned Fixes (Next Build)
- See `KNOWN_ISSUES.md` in game folder for detailed tracker

---

## Still Having Issues?

1. **Check GitHub Issues:** [Known bugs](https://github.com/ResonanceEnergy/TARTARIA/issues)
2. **Search existing issues:** Your issue may already be reported
3. **Join Discord:** [Community support](https://discord.gg/resonanceenergy) (if available)
4. **Email support:** support@resonanceenergy.dev (for private issues)

---

**Last Updated:** 2026-05-21 (Beta Vertical Slice)  
**Build Version:** Echohaven Moon 1  
**Report issues:** https://github.com/ResonanceEnergy/TARTARIA/issues
