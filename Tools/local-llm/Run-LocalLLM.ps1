<#
.SYNOPSIS
  Local-LLM hand-off launcher for TARTARIA.

.DESCRIPTION
  Iterates every ticket in LOCAL_TASKS\*.md, pipes it into Ollama with a
  TARTARIA-flavored system prompt, and writes the model output to
  LOCAL_OUTPUTS\<ticket-name>\. A human reviews before merging.

.PARAMETER Model
  Ollama model tag. Default: qwen3-coder:30b. Pull with:
    ollama pull qwen3-coder:30b

.PARAMETER OnlyTicket
  Optional filename (without .md) to run a single ticket.

.PARAMETER DryRun
  Print what would be sent to the model without invoking Ollama.

.EXAMPLE
  pwsh tools\local-llm\Run-LocalLLM.ps1
  pwsh tools\local-llm\Run-LocalLLM.ps1 -Model deepseek-coder-v2:16b
  pwsh tools\local-llm\Run-LocalLLM.ps1 -OnlyTicket EXAMPLE_stub-moon2-enemies
  pwsh tools\local-llm\Run-LocalLLM.ps1 -DryRun

.NOTES
  Created 2026-05-30 per NATRIX request. See README.md alongside this script.
  This script does NOT touch Assets/ — outputs go to LOCAL_OUTPUTS/ for review.
#>

[CmdletBinding()]
param(
    [string]$Model = "qwen3-coder:30b",
    [string]$OnlyTicket,
    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$TasksDir  = Join-Path $ScriptDir "LOCAL_TASKS"
$OutDir    = Join-Path $ScriptDir "LOCAL_OUTPUTS"

if (-not (Test-Path $TasksDir)) { New-Item -ItemType Directory -Path $TasksDir | Out-Null }
if (-not (Test-Path $OutDir))   { New-Item -ItemType Directory -Path $OutDir   | Out-Null }

# --- preflight ---
if (-not $DryRun) {
    $ollama = Get-Command ollama -ErrorAction SilentlyContinue
    if (-not $ollama) {
        Write-Host "[LocalLLM] ERROR: 'ollama' not on PATH. Install from https://ollama.com" -ForegroundColor Red
        Write-Host "[LocalLLM] Then: ollama pull $Model" -ForegroundColor Yellow
        exit 1
    }

    # Verify model is installed
    $installedModels = ollama list 2>$null
    if ($installedModels -notmatch [regex]::Escape($Model)) {
        Write-Host "[LocalLLM] Model '$Model' not pulled. Run: ollama pull $Model" -ForegroundColor Yellow
        $ans = Read-Host "Pull it now? (y/N)"
        if ($ans -match '^y') {
            ollama pull $Model
        } else {
            exit 1
        }
    }
}

# --- system prompt: hard-coded TARTARIA context ---
$SystemPrompt = @"
You are a code-generation assistant for the TARTARIA Unity 6 LTS project.

PROJECT CONTEXT (do not deviate):
- Unity 6 LTS (6000.3.6f1), URP Forward+, Vulkan.
- 23 assembly definitions. Common refs: Tartaria.Core, Tartaria.Gameplay,
  Tartaria.AI, Tartaria.Integration, Tartaria.Audio, Tartaria.Input, Tartaria.UI.
- AI -> Gameplay is one-way. Gameplay CANNOT reference AI back. Use
  SendMessage/SendMessageUpwards across that boundary.
- URP material color: use mat.SetColor("_BaseColor", c). Never mat.color = c.
- Static event bus: Tartaria.Core.GameEvents (do not invent new buses).
- Input System: read Keyboard.current / Gamepad.current directly. Use
  wasPressedThisFrame for edge-trigger.
- File hygiene: UTF-8 BOM, CRLF acceptable, brace-balanced. Never emit code
  past the namespace close.
- **CRITICAL: Do NOT insert newlines inside C# regular string literals**
  (the ones with double quotes). Regular `"strings"` in C# CANNOT span
  multiple lines — that's a compile error. If a string is long, either:
   1. Put the whole literal on one logical line (preferred), or
   2. Concatenate with `+` operator: `"part one " + \n    "part two"`
   3. Use @-verbatim strings: `@"line one\nline two"`
- For path strings, prefer forward slashes inside string literals (Unity
  cross-platform). Avoid em-dashes (—) or unicode arrows in identifiers;
  short ASCII is fine.

OUTPUT RULES (strict):
- Emit a single fenced code block per file, with a header line listing the
  destination path the ticket specified.
- No prose outside fenced blocks. No "Here's the code:" preambles.
- If the ticket is underspecified (missing acceptance criteria, no spec body,
  ambiguous destination), output only: TICKET REJECTED: <reason>.
- Never write to Assets/ paths directly — you write into a review folder.

You will receive the ticket as the user message. Generate the file(s).
"@

# --- helpers ---
function Invoke-Ollama {
    param([string]$Prompt, [string]$Model)

    # 2026-05-30 fix: was piping into `ollama run` which streamed live and
    # leaked cursor-back ([5D[K) sequences PLUS PowerShell's CP1252 mangled
    # em-dashes. Use HTTP API instead — returns clean UTF-8 in one shot.
    $body = @{
        model  = $Model
        prompt = "$SystemPrompt`n`n---TICKET START---`n$Prompt`n---TICKET END---"
        stream = $false
        options = @{
            num_ctx = 8192
            temperature = 0.2
        }
    } | ConvertTo-Json -Compress -Depth 5

    try {
        $resp = Invoke-RestMethod -Uri "http://localhost:11434/api/generate" `
                                  -Method POST `
                                  -Body $body `
                                  -ContentType "application/json; charset=utf-8" `
                                  -TimeoutSec 600
        return $resp.response
    } catch {
        Write-Host "[LocalLLM] HTTP API call failed: $($_.Exception.Message)" -ForegroundColor Red
        return "API_ERROR: $($_.Exception.Message)"
    }
}

# --- main loop ---
$tickets = if ($OnlyTicket) {
    @(Get-ChildItem -Path $TasksDir -Filter "$OnlyTicket.md")
} else {
    # 2026-05-30 fix: filter out metadata files. Tickets must be ".md" but NOT
    # _MANIFEST.md, README.md, or anything starting with underscore.
    @(Get-ChildItem -Path $TasksDir -Filter "*.md") |
        Where-Object { $_.Name -notmatch '^_' -and $_.Name -ne 'README.md' }
}

if ($tickets.Count -eq 0) {
    Write-Host "[LocalLLM] No tickets in $TasksDir" -ForegroundColor Yellow
    Write-Host "[LocalLLM] See $ScriptDir\README.md for ticket format" -ForegroundColor Yellow
    exit 0
}

Write-Host "[LocalLLM] Model: $Model" -ForegroundColor Cyan
Write-Host "[LocalLLM] Tickets to process: $($tickets.Count)" -ForegroundColor Cyan
Write-Host ""

foreach ($ticket in $tickets) {
    $name = [IO.Path]::GetFileNameWithoutExtension($ticket.Name)
    $outFolder = Join-Path $OutDir $name
    if (-not (Test-Path $outFolder)) { New-Item -ItemType Directory -Path $outFolder | Out-Null }

    $outFile = Join-Path $outFolder "response.md"

    # 2026-05-30 fix: skip tickets already produced (idempotent reruns).
    if ((Test-Path $outFile) -and ((Get-Item $outFile).Length -gt 200)) {
        Write-Host "[LocalLLM] === $name === (SKIP — already produced)" -ForegroundColor DarkGray
        continue
    }

    $promptText = Get-Content $ticket.FullName -Raw

    Write-Host "[LocalLLM] === $name ===" -ForegroundColor Green

    if ($DryRun) {
        Write-Host "  (dry run — would invoke '$Model' with $($promptText.Length) chars of ticket)"
        continue
    }

    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    $output = Invoke-Ollama -Prompt $promptText -Model $Model
    $sw.Stop()
    # 2026-05-30 fix: PowerShell's Out-File -Encoding utf8 was double-encoding
    # the response (UTF-8 chars from HTTP came in as UTF-16 strings, then got
    # re-encoded badly). Write raw UTF-8 bytes via .NET API instead.
    [System.IO.File]::WriteAllText($outFile, $output, [System.Text.UTF8Encoding]::new($false))

    $tokenEst = ($output | Out-String).Length / 4
    Write-Host "  -> $outFile  ($([int]$sw.Elapsed.TotalSeconds)s, ~$([int]$tokenEst) tokens out)" -ForegroundColor Gray
}

Write-Host ""
Write-Host "[LocalLLM] Done. Review outputs in $OutDir" -ForegroundColor Cyan
Write-Host "[LocalLLM] If a response is good, copy files into the destination paths" -ForegroundColor Cyan
Write-Host "[LocalLLM] listed in the ticket. If bad, tighten the ticket and re-run." -ForegroundColor Cyan
