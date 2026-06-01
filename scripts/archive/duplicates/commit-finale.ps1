#!/usr/bin/env pwsh
# Commit Moon 11-13 Finale

Write-Host "Committing Moon 11-13 Finale..." -ForegroundColor Cyan

# Add finale files
git add Assets/_Project/Scripts/Integration/CompanionFarewellSystem.cs
git add Assets/_Project/Scripts/Integration/ZerethResonanceDialogue.cs
git add Assets/_Project/Scripts/Integration/Moon11ContentSpawner.cs
git add Assets/_Project/Scripts/Integration/Moon13ContentSpawner.cs
git add Assets/_Project/Scripts/Integration/EndCardController.cs
git add Assets/_Project/Scripts/Gameplay/InventorySystem.cs
git add MOON_11-13_FINALE_COMPLETE.md

# Commit
git commit -m "MOON 11-13 FINALE COMPLETE: 13 echoes + 4 companion farewells + Zereth resonance + credits + 3 DLC hooks. ~955 lines. CS:0."

Write-Host "✓ Commit complete" -ForegroundColor Green
