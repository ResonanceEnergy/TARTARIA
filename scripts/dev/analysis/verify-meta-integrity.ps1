cd C:\dev\TARTARIA_new

Write-Host "`n════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "    TARTARIA META FILE INTEGRITY VERIFICATION    " -ForegroundColor White
Write-Host "════════════════════════════════════════════════`n" -ForegroundColor Cyan

$scriptsPath = "Assets/_Project/Scripts"

if (-not (Test-Path $scriptsPath)) {
    Write-Host "ERROR: Scripts path not found: $scriptsPath" -ForegroundColor Red
    exit 1
}

# Find all .cs files
$csFiles = Get-ChildItem -Path $scriptsPath -Filter "*.cs" -Recurse -File
$metaFiles = Get-ChildItem -Path $scriptsPath -Filter "*.cs.meta" -Recurse -File

Write-Host "Scanning $($csFiles.Count) C# files..." -ForegroundColor Yellow

# Data structures
$validMetas = @()
$missingMetas = @()
$orphanedMetas = @()
$duplicateGuids = @{}
$timestampMismatches = @()
$invalidGuids = @()

# Build lookup of .cs files
$csLookup = @{}
foreach ($cs in $csFiles) {
    $csLookup[$cs.FullName] = $cs
}

# Process each .cs file
foreach ($cs in $csFiles) {
    $metaPath = "$($cs.FullName).meta"
    
    if (-not (Test-Path $metaPath)) {
        $missingMetas += [PSCustomObject]@{
            ScriptPath = $cs.FullName.Replace("$PWD\", "")
            ScriptName = $cs.Name
        }
        continue
    }
    
    # Parse .meta file for GUID
    $metaContent = Get-Content $metaPath -Raw
    
    if ($metaContent -match 'guid:\s*([a-f0-9]+)') {
        $guid = $matches[1]
        
        # Validate GUID format (32 hex chars)
        if ($guid -notmatch '^[a-f0-9]{32}$') {
            $invalidGuids += [PSCustomObject]@{
                ScriptPath = $cs.FullName.Replace("$PWD\", "")
                ScriptName = $cs.Name
                GUID = $guid
                Issue = "Invalid format (expected 32 hex chars, got $($guid.Length))"
            }
        }
        
        # Track for duplicate detection
        if (-not $duplicateGuids.ContainsKey($guid)) {
            $duplicateGuids[$guid] = @()
        }
        $duplicateGuids[$guid] += $cs.FullName.Replace("$PWD\", "")
        
        # Check timestamps
        $meta = Get-Item $metaPath
        if ($cs.LastWriteTime -gt $meta.LastWriteTime) {
            $timestampMismatches += [PSCustomObject]@{
                ScriptPath = $cs.FullName.Replace("$PWD\", "")
                ScriptName = $cs.Name
                ScriptTime = $cs.LastWriteTime
                MetaTime = $meta.LastWriteTime
                Delta = ($cs.LastWriteTime - $meta.LastWriteTime).TotalSeconds
            }
        }
        
        $validMetas += [PSCustomObject]@{
            ScriptPath = $cs.FullName.Replace("$PWD\", "")
            GUID = $guid
        }
    }
    else {
        $invalidGuids += [PSCustomObject]@{
            ScriptPath = $cs.FullName.Replace("$PWD\", "")
            ScriptName = $cs.Name
            GUID = "N/A"
            Issue = "No GUID found in .meta file"
        }
    }
}

# Find orphaned .meta files
foreach ($meta in $metaFiles) {
    $expectedCs = $meta.FullName -replace '\.meta$', ''
    
    if (-not $csLookup.ContainsKey($expectedCs)) {
        # Parse GUID from orphaned meta
        $metaContent = Get-Content $meta.FullName -Raw
        $guid = "unknown"
        if ($metaContent -match 'guid:\s*([a-f0-9]+)') {
            $guid = $matches[1]
        }
        
        $orphanedMetas += [PSCustomObject]@{
            MetaPath = $meta.FullName.Replace("$PWD\", "")
            MetaName = $meta.Name
            GUID = $guid
            ExpectedScript = $expectedCs.Replace("$PWD\", "")
        }
    }
}

# Find actual duplicate GUIDs
$trueDuplicates = $duplicateGuids.GetEnumerator() | Where-Object { $_.Value.Count -gt 1 }

# Generate report
$report = @"

════════════════════════════════════════════════
    META FILE INTEGRITY REPORT
════════════════════════════════════════════════

Total .cs files scanned: $($csFiles.Count)
Valid .meta files: $($validMetas.Count)
Total issues found: $($missingMetas.Count + $orphanedMetas.Count + $trueDuplicates.Count + $invalidGuids.Count + $timestampMismatches.Count)

"@

# Duplicate GUIDs (CRITICAL)
if ($trueDuplicates.Count -gt 0) {
    $report += @"

═══════════════════════════════════════════════
⚠️  DUPLICATE GUIDS (CRITICAL - BREAKS UNITY) ⚠️
═══════════════════════════════════════════════

"@
    foreach ($dup in $trueDuplicates) {
        $report += "`nGUID: $($dup.Key)`n"
        $report += "Found in $($dup.Value.Count) files:`n"
        foreach ($file in $dup.Value) {
            $report += "  - $file`n"
        }
    }
}
else {
    $report += "`n[OK] No duplicate GUIDs found`n"
}

# Invalid GUIDs
if ($invalidGuids.Count -gt 0) {
    $report += @"

════════════════════════════════════════════════
INVALID GUIDS ($($invalidGuids.Count))
════════════════════════════════════════════════

"@
    foreach ($invalid in $invalidGuids) {
        $report += "`n$($invalid.ScriptName)`n"
        $report += "  Path: $($invalid.ScriptPath)`n"
        $report += "  GUID: $($invalid.GUID)`n"
        $report += "  Issue: $($invalid.Issue)`n"
    }
}

# Orphaned .meta files
if ($orphanedMetas.Count -gt 0) {
    $report += @"

════════════════════════════════════════════════
ORPHANED .META FILES ($($orphanedMetas.Count))
════════════════════════════════════════════════
(Source .cs file deleted, but .meta remains)

"@
    foreach ($orphan in $orphanedMetas) {
        $report += "`n$($orphan.MetaName)`n"
        $report += "  Meta path: $($orphan.MetaPath)`n"
        $report += "  GUID: $($orphan.GUID)`n"
        $report += "  Expected script: $($orphan.ExpectedScript)`n"
    }
}
else {
    $report += "`n[OK] No orphaned .meta files found`n"
}

# Missing .meta files
if ($missingMetas.Count -gt 0) {
    $report += @"

════════════════════════════════════════════════
MISSING .META FILES ($($missingMetas.Count))
════════════════════════════════════════════════
(.cs exists but no .meta - Unity will regenerate)

"@
    foreach ($missing in $missingMetas) {
        $report += "`n$($missing.ScriptName)`n"
        $report += "  Path: $($missing.ScriptPath)`n"
    }
}
else {
    $report += "`n[OK] All .cs files have corresponding .meta files`n"
}

# Timestamp mismatches
if ($timestampMismatches.Count -gt 0) {
    $report += @"

════════════════════════════════════════════════
TIMESTAMP MISMATCHES ($($timestampMismatches.Count))
════════════════════════════════════════════════
(.cs modified after .meta - may need reimport)

"@
    foreach ($mismatch in $timestampMismatches) {
        $report += "`n$($mismatch.ScriptName)`n"
        $report += "  Path: $($mismatch.ScriptPath)`n"
        $report += "  Script time: $($mismatch.ScriptTime)`n"
        $report += "  Meta time: $($mismatch.MetaTime)`n"
        $report += "  Delta: $([math]::Round($mismatch.Delta, 2)) seconds`n"
    }
}
else {
    $report += "`n[OK] All timestamps in sync`n"
}

$report += @"

════════════════════════════════════════════════
SUMMARY
════════════════════════════════════════════════

"@

if ($trueDuplicates.Count -gt 0) {
    $report += "[!] CRITICAL: $($trueDuplicates.Count) duplicate GUID(s) found - MUST FIX`n"
}
if ($invalidGuids.Count -gt 0) {
    $report += "[!] WARNING: $($invalidGuids.Count) invalid GUID(s)`n"
}
if ($orphanedMetas.Count -gt 0) {
    $report += "[i] INFO: $($orphanedMetas.Count) orphaned .meta file(s) - can be deleted`n"
}
if ($missingMetas.Count -gt 0) {
    $report += "[i] INFO: $($missingMetas.Count) missing .meta file(s) - Unity will regenerate`n"
}
if ($timestampMismatches.Count -gt 0) {
    $report += "[i] INFO: $($timestampMismatches.Count) timestamp mismatch(es) - may need reimport`n"
}

if ($trueDuplicates.Count -eq 0 -and $invalidGuids.Count -eq 0 -and $orphanedMetas.Count -eq 0 -and $missingMetas.Count -eq 0 -and $timestampMismatches.Count -eq 0) {
    $report += "`n[OK] ALL META FILES ARE HEALTHY!`n"
}

$report += "`n════════════════════════════════════════════════`n"

# Output to console and file
Write-Host $report

$outputFile = "META_INTEGRITY_REPORT.txt"
$report | Out-File -FilePath $outputFile -Encoding UTF8

Write-Host "`n[OK] Full report saved to: $PWD\$outputFile" -ForegroundColor Green
Write-Host "════════════════════════════════════════════════`n" -ForegroundColor Cyan
