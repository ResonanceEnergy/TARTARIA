Start-Sleep -Seconds 12
$claude = Get-Process -Name claude -ErrorAction SilentlyContinue | Where-Object { $_.MainWindowTitle }
foreach ($p in $claude) { Stop-Process -Id $p.Id -Force -ErrorAction SilentlyContinue }
Start-Sleep -Seconds 4
Start-Process explorer.exe -ArgumentList 'shell:AppsFolder\Claude_pzs8sxrjxfjjc!App'
