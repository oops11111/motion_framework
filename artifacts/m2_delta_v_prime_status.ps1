$ErrorActionPreference='Continue'
Write-Host '=== TERM TAIL ==='
Get-Content C:\Users\admin\terminals\359332.txt -Tail 40
Write-Host '=== PROCS ==='
Get-Process MotionSoftDemo,dotnet -ErrorAction SilentlyContinue | Format-Table Id,ProcessName,CPU,StartTime -AutoSize
Write-Host '=== RAW ==='
$r='C:\Users\admin\motion_framework\artifacts\m2_delta_v_prime_capture_raw.txt'
if(Test-Path $r){
  $i=Get-Item $r
  Write-Host ("raw_len={0} mtime={1}" -f $i.Length, $i.LastWriteTime)
  Get-Content $r -Tail 10
  $trials = (Select-String -Path $r -Pattern '^TRIAL ' | Measure-Object).Count
  $lastCell = Select-String -Path $r -Pattern '^TRIAL arm=\S+ cell=(\S+)' | Select-Object -Last 1
  Write-Host ("trial_lines={0} last={1}" -f $trials, $lastCell.Line)
} else { Write-Host 'no raw' }
Write-Host '=== SUMMARY/SESS ==='
$s='C:\Users\admin\motion_framework\artifacts\m2_delta_v_prime_capture_summary.txt'
if(Test-Path $s){ Get-Content $s -Tail 25 } else { Write-Host 'no summary' }
Get-ChildItem C:\Users\admin\motion_framework\artifacts\m2_delta_v_prime_s* -ErrorAction SilentlyContinue | Format-Table Name,Length,LastWriteTime -AutoSize
Get-ChildItem C:\Users\admin\motion_framework\artifacts\m2_delta_v_prime_run.log -ErrorAction SilentlyContinue | Format-Table Name,Length,LastWriteTime -AutoSize
