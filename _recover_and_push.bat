@echo off
setlocal
set LOG=C:\dev\TARTARIA_new\Library\_wave_push.log
echo === RECOVER START %DATE% %TIME% === > %LOG%
cd /d C:\dev\TARTARIA_new
echo --- backup .cs files --- >> %LOG%
copy /Y C:\dev\_wt_ai_wave_spawner\Assets\_Project\Scripts\AI\MudGolemSpawner.cs C:\dev\TARTARIA_new\Library\_MudGolemSpawner.cs.bak >> %LOG% 2>&1
copy /Y C:\dev\_wt_ai_wave_spawner\Assets\_Project\Scripts\AI\WaveSystem.cs C:\dev\TARTARIA_new\Library\_WaveSystem.cs.bak >> %LOG% 2>&1
echo --- prune --- >> %LOG%
git worktree prune -v >> %LOG% 2>&1
echo --- delete old worktree dir --- >> %LOG%
rmdir /S /Q C:\dev\_wt_ai_wave_spawner >> %LOG% 2>&1
echo --- worktree add (fresh) --- >> %LOG%
git worktree add C:\dev\_wt_ai_wave_spawner2 agent/ai/wave-spawner-tuning >> %LOG% 2>&1
cd /d C:\dev\_wt_ai_wave_spawner2
echo --- restore .cs files --- >> %LOG%
if not exist Assets\_Project\Scripts\AI mkdir Assets\_Project\Scripts\AI
copy /Y C:\dev\TARTARIA_new\Library\_MudGolemSpawner.cs.bak Assets\_Project\Scripts\AI\MudGolemSpawner.cs >> %LOG% 2>&1
copy /Y C:\dev\TARTARIA_new\Library\_WaveSystem.cs.bak Assets\_Project\Scripts\AI\WaveSystem.cs >> %LOG% 2>&1
echo --- status before --- >> %LOG%
git status --short >> %LOG% 2>&1
git add Assets/_Project/Scripts/AI/MudGolemSpawner.cs Assets/_Project/Scripts/AI/WaveSystem.cs >> %LOG% 2>&1
echo --- status after add --- >> %LOG%
git status --short >> %LOG% 2>&1
echo --- commit --- >> %LOG%
git commit -m "[ai] wave spawner tuning --- 3-cap, hub-progress scaling, 10s cleanup" >> %LOG% 2>&1
echo --- log --- >> %LOG%
git log -1 --oneline >> %LOG% 2>&1
echo --- push --- >> %LOG%
git push -u origin agent/ai/wave-spawner-tuning >> %LOG% 2>&1
echo === DONE === >> %LOG%
