# Z6 F4: record that every repo file cited by docs/z6_aot_host_guide.md exists at HEAD, with blob sha and supporting lines.
param([Parameter(Mandatory=$true)][string]$Out)
$w = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$files = 'pilot/fluenthub/T5_REPORT.md','pilot/fluenthub/BASELINE.md','pilot/fluenthub/z5_t5_motion_run1_etw_off.log',
  'pilot/fluenthub/z5_t5_motion_run2.log','pilot/fluenthub/fluenthub_t5_side_fork.patch','docs/z5_closed.md','pilot/fluenthub/surface_l1pack.txt'
$o = @("head=" + (& git -C $w rev-parse HEAD).Trim())
foreach ($f in $files) { $ls = (& git -C $w ls-files -- $f); $blob = (& git -C $w rev-parse "HEAD:$f").Trim(); $o += "$f | tracked=$([bool]$ls) | blob=$blob" }
$o += '# supporting lines (file:line: text)'
$pats = @(@('pilot/fluenthub/z5_t5_motion_run1_etw_off.log','ETW_SUMMARY'), @('pilot/fluenthub/z5_t5_motion_run2.log','ETW_SUMMARY'),
  @('pilot/fluenthub/fluenthub_t5_side_fork.patch','EventSourceSupport'), @('docs/z5_closed.md','K-AOT-ETW|IsSupported|EventSourceSupport'),
  @('pilot/fluenthub/T5_REPORT.md','IsSupported'), @('pilot/fluenthub/BASELINE.md','PublishAot=true'))
foreach ($p in $pats) { Select-String -Path (Join-Path $w $p[0]) -Pattern $p[1] -Encoding UTF8 | ForEach-Object { $o += "$($p[0]):$($_.LineNumber): $($_.Line.Trim())" } }
[IO.File]::WriteAllText($Out, (($o -join "`n") + "`n"), (New-Object Text.UTF8Encoding($false)))
$o.Count
