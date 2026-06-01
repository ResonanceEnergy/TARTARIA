# TARTARIA - Force Boot Scene Load

## Problem
Unity loaded backup scene `Temp/__Backupscenes/0.backup` instead of Boot.unity.
Input doesn't work because GameStateManager never initialized.

## Solution — Load Boot Scene Manually

### In Unity Editor:

1. **Look at top menu bar** → **File** → **Open Scene**
2. Navigate to: `Assets/_Project/Scenes/Boot.unity`
3. Click **Open**
4. **Press Play (▶)**

### Alternative — Double-click in Project Window:

1. **Look at bottom panel** (Project window)
2. Navigate to folder: `Assets → _Project → Scenes`
3. **Double-click Boot.unity**
4. **Press Play (▶)**

### Expected Result:
- Console shows: `[GameState] Boot → Exploration`
- Console shows: `[PlayerInput] Setup OK`
- Character spawns in Echohaven
- **Keyboard WASD + Mouse** should move character
- **Controller left stick** should move character

## If Still Stuck:
1. Check Console (bottom panel) for red errors
2. Press **Ctrl+Shift+C** to clear console
3. Press Play again
4. Type "still stuck" to chat

## Root Cause:
Unity project had Addressables initialization error on startup, fell back to backup scene.
Manual Boot scene load bypasses this issue.
