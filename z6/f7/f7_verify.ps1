# Z6 F7: independent checks on the new pack, written to %TEMP%\z6\f7\verify.txt:
#  - Get-FileHash (independent of SHA256SUMS) of the three pack artifacts in the pack output dir;
#  - git hash-object of the two link-compiled sources vs the values required by the handover;
#  - public-surface dump of the new pack DLL vs pilot/fluenthub/surface_l1pack.txt (only line 1 may differ).
$w = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$L = Join-Path $env:TEMP 'z6\f7'
$out = Join-Path $w 'pilot\l1pack\bin\x64\Release\net8.0'
$o = @("head=" + (& git -C $w rev-parse HEAD).Trim(), "time=" + [DateTimeOffset]::Now.ToString('yyyy-MM-dd HH:mm:ss zzz'))
foreach ($f in 'FluidMotion.L1.dll','fluidmotion.dll','demo_motion.json') { $o += "Get-FileHash $f = " + (Get-FileHash (Join-Path $out $f) -Algorithm SHA256).Hash.ToLower() }
$o += "Get-FileHash MotionNative\fluidmotion.dll = " + (Get-FileHash (Join-Path $w 'MotionNative\fluidmotion.dll')).Hash.ToLower()
$o += "Get-FileHash code\capi\demo_motion.json = " + (Get-FileHash (Join-Path $w 'code\capi\demo_motion.json')).Hash.ToLower()
$req = @{ 'MotionSoftDemo/MotionSystem.cs' = 'fe72b08f9445723f926a2928345e23311503efec'; 'MotionSoftDemo/MotionEtw.cs' = '0404498ab4cb7e96168405a7ffb63591686797ca' }
foreach ($k in $req.Keys | Sort-Object) { $h = (& git -C $w hash-object -- $k).Trim(); $o += "git hash-object $k = $h expected=$($req[$k]) match=$($h -eq $req[$k])" }
$a = Get-Content (Join-Path $w 'pilot\fluenthub\surface_l1pack.txt')
$b = Get-Content (Join-Path $L 'surface_pack_release.txt')
$o += "surface_l1pack.txt lines=$($a.Count) new lines=$($b.Count)"
$diff = @(); $n = [Math]::Max($a.Count, $b.Count)
for ($i = 0; $i -lt $n; $i++) { if ($a[$i] -cne $b[$i]) { $diff += ($i + 1) } }
$o += "differing_line_numbers=[" + ($diff -join ',') + "]"
$o += "only_first_line_may_differ=" + ((@($diff | Where-Object { $_ -ne 1 })).Count -eq 0)
if ($diff -contains 1) { $o += "old line1: " + $a[0]; $o += "new line1: " + $b[0] }
[IO.File]::WriteAllText((Join-Path $L 'verify.txt'), (($o -join "`n") + "`n"), (New-Object Text.UTF8Encoding($false)))
$o
