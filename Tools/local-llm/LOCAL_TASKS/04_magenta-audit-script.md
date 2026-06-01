# TICKET: Magenta primitive audit — find all CreatePrimitive without URP shader fallback

## Output destination
`tools/audits/Find-MagentaPrimitives.ps1`

## Acceptance criteria
- PowerShell script (PS 7 compatible, `#!/usr/bin/env pwsh` shebang at top is fine)
- No external module dependencies — only built-in cmdlets (`Get-ChildItem`, `Select-String`, `Where-Object`)
- Runs from repo root: `pwsh tools\audits\Find-MagentaPrimitives.ps1`
- Exits 0 if no offenders, exits 1 if any found (so it can gate CI later)
- Output: a table with columns `File | Line | Reason`

## Spec

Magenta = Unity's "missing shader" fallback color. In TARTARIA we render with URP, but `GameObject.CreatePrimitive(PrimitiveType.X)` returns a primitive with the Built-in Render Pipeline `Standard` shader assigned. In URP, that shows up magenta.

The script must:

1. Recursively scan `Assets/_Project/Scripts/**/*.cs` (exclude `_archived_backups/`, `Assets/_Project/Scripts/Tests/**`, anything matching `.disabled`)
2. For each file, find every line containing `GameObject.CreatePrimitive(`
3. For each match, examine the next 40 lines of that file's content
4. The match is OK if EITHER:
   - The next 40 lines contain `Shader.Find("Universal Render Pipeline/Lit"` AND `SetColor("_BaseColor"`
   - OR the next 40 lines contain a comment with the literal `// URP-safe` (an explicit escape hatch)
5. The match is OFFENDING if neither condition is met. Emit `File | Line | Reason="primitive without URP shader assignment"`
6. Also flag any line containing `.color = ` on a `Material` variable — that's the Built-in shader path. Reason="material.color assignment (use SetColor _BaseColor)"
7. Also flag any line containing `mat.color = ` (same reason)

At the end:
- If offenders count > 0: print red `FOUND <n> magenta-risk sites` and `exit 1`
- If 0: print green `CLEAN — no magenta-risk primitive creations found` and `exit 0`

## Output format example (good)

```
File                                                               | Line | Reason
-------------------------------------------------------------------+------+------------------------------------------------
Assets/_Project/Scripts/AI/MudGolemAI.cs                           | 142  | primitive without URP shader assignment
Assets/_Project/Scripts/Gameplay/SomeOtherFile.cs                  | 87   | material.color assignment (use SetColor _BaseColor)

FOUND 2 magenta-risk sites
```

## Sample PowerShell skeleton you can build from

```powershell
#!/usr/bin/env pwsh
param([string]$Root = (Get-Location).Path)

$scriptsDir = Join-Path $Root "Assets/_Project/Scripts"
if (-not (Test-Path $scriptsDir)) {
    Write-Host "Scripts dir not found: $scriptsDir" -ForegroundColor Red
    exit 2
}

$offenders = @()

Get-ChildItem -Path $scriptsDir -Filter "*.cs" -Recurse |
    Where-Object {
        $_.FullName -notmatch '_archived_backups' -and
        $_.FullName -notmatch '\\Tests\\' -and
        $_.FullName -notmatch '\.disabled$'
    } |
    ForEach-Object {
        $file = $_.FullName
        $lines = Get-Content $file
        for ($i = 0; $i -lt $lines.Count; $i++) {
            $line = $lines[$i]
            if ($line -match 'GameObject\.CreatePrimitive\(') {
                # check next 40 lines
                $endIdx = [Math]::Min($i + 40, $lines.Count - 1)
                $window = $lines[$i..$endIdx] -join "`n"
                $hasUrp = ($window -match 'Shader\.Find\("Universal Render Pipeline/Lit"') -and ($window -match 'SetColor\("_BaseColor"')
                $hasEscape = $window -match '// URP-safe'
                if (-not $hasUrp -and -not $hasEscape) {
                    $offenders += [pscustomobject]@{ File=$file.Replace($Root+'\',''); Line=($i+1); Reason="primitive without URP shader assignment" }
                }
            }
            if ($line -match '\.color\s*=' -and $line -match '\b(mat|material|rend\.material|sharedMaterial)\b') {
                $offenders += [pscustomobject]@{ File=$file.Replace($Root+'\',''); Line=($i+1); Reason="material.color assignment (use SetColor _BaseColor)" }
            }
        }
    }

if ($offenders.Count -eq 0) {
    Write-Host "CLEAN - no magenta-risk primitive creations found" -ForegroundColor Green
    exit 0
}

$offenders | Format-Table -AutoSize
Write-Host ""
Write-Host "FOUND $($offenders.Count) magenta-risk sites" -ForegroundColor Red
exit 1
```

## Do NOT
- Do not write a Unity Editor script. This is a CI-side audit, must run with vanilla `pwsh`.
- Do not modify any `.cs` files. This is read-only.
- Do not require admin permissions or write outside `tools/audits/`.
- Do not delete or rename existing offending files — just report them.
