@echo off
del /Q C:\dev\TARTARIA_new\Library\_wave_pr.log 2>nul
cd /d C:\dev\_wt_ai_wave_spawner2
echo === PR CREATE === > C:\dev\TARTARIA_new\Library\_wave_pr.log
gh pr create --repo ResonanceEnergy/TARTARIA --base feature/consolidate-moon-architecture --head agent/ai/wave-spawner-tuning --title "[ai] wave spawner tuning --- 3-cap, hub-progress scaling, 10s cleanup" --body-file C:\dev\TARTARIA_new\Library\_wave_pr_body.md >> C:\dev\TARTARIA_new\Library\_wave_pr.log 2>&1
echo === DONE === >> C:\dev\TARTARIA_new\Library\_wave_pr.log
