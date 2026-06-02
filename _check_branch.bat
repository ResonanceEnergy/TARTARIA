@echo off
cd /d C:\dev\TARTARIA_new
echo === BRANCH CHECK ===
git branch --list agent/ai/wave-spawner-tuning
echo === ALL AI BRANCHES ===
git branch --list "agent/ai/*"
echo === WORKTREE LIST ===
git worktree list
echo === PRUNE ===
git worktree prune -v
