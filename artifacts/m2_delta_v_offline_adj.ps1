# Offline adjudicator for M2-Δv (matches M2DeltaVProbe.cs G2 rules)
param([Parameter(Mandatory)][string]$RawPath, [Parameter(Mandatory)][string]$OutSummary, [string]$SessionId='1')
$ErrorActionPreference='Stop'
$NeedN=20; $FloorThr=20.0; $HangCap=2
$core=@('t060fwd','t060rev','t300fwd','t300rev','t300dbl')
$cells=@('t060fwd','t060rev','t156fwd','t156rev','t300fwd','t300rev','t300dbl')

function Percentile([double[]]$a, [double]$p) {
  if ($null -eq $a -or $a.Count -eq 0) { return [double]::NaN }
  $s = $a | Sort-Object
  if ($s.Count -eq 1) { return [double]$s[0] }
  $idx = [Math]::Max(0, [Math]::Min($s.Count-1, [int][Math]::Ceiling($p * $s.Count) - 1))
  return [double]$s[$idx]
}
function Median([double[]]$a) {
  if ($null -eq $a -or $a.Count -eq 0) { return [double]::NaN }
  $s = $a | Sort-Object
  return [double]$s[[int]($s.Count/2)]
}
function F([double]$x, [int]$d=3) { if ([double]::IsNaN($x)) { return 'NaN' }; return $x.ToString("F$d") }

$rows = New-Object System.Collections.Generic.List[object]
$hangByCell = @{}
foreach ($c in $cells) { $hangByCell[$c]=0 }

Get-Content -LiteralPath $RawPath | ForEach-Object {
  $line = $_
  if ($line -notmatch '^TRIAL arm=(\S+) cell=(\S+) k=(\d+)') { return }
  $arm=$Matches[1]; $cell=$Matches[2]; $k=[int]$Matches[3]
  $o = [ordered]@{Arm=$arm; Cell=$cell; K=$k; Valid=$false; Hang=$false; Dv=[double]::NaN; Thr=[double]::NaN; AbsA=[double]::NaN; SigmaTs=[double]::NaN; InvalidReason=''; Phi=[double]::NaN; Ts=[double]::NaN; VPre=[double]::NaN; VPost=[double]::NaN; R=[double]::NaN; IVelSet=[double]::NaN; VWgc=[double]::NaN; SeedPath=''; ShadowV=[double]::NaN}
  if ($line -match 'valid=True') { $o.Valid = $true }
  if ($line -match 'INVALID') { $o.Valid = $false }
  if ($line -match 'hang' -and $line -match 'INVALID') { $o.Hang=$true; $hangByCell[$cell]++ }
  if ($line -match 'phi=([0-9.]+)') { $o.Phi=[double]$Matches[1] }
  if ($line -match 't_s=([0-9.]+)') { $o.Ts=[double]$Matches[1]/1000.0 }
  if ($line -match 'v_pre=([-+0-9.eE]+|NaN)') { if ($Matches[1] -ne 'NaN') { $o.VPre=[double]$Matches[1] } }
  if ($line -match 'v_post=([-+0-9.eE]+|NaN)') { if ($Matches[1] -ne 'NaN') { $o.VPost=[double]$Matches[1] } }
  if ($line -match 'thr=([0-9.]+)') { $o.Thr=[double]$Matches[1] }
  if ($line -match 'Δv=([0-9.]+)' -or $line -match 'Dv=([0-9.]+)') { } 
  # delta char
  if ($line -match 'v=([0-9.]+) r=') { } # skip
  if ($line -match '[Δâ]?v=([0-9.]+)') {
    # try explicit
  }
  if ($line -match 'Δv=([0-9.]+)') { $o.Dv=[double]$Matches[1] }
  elseif ($line -match ' v=([0-9.]+) r=') { $o.Dv=[double]$Matches[1] } # fallback unlikely
  # More reliable: parse after thr=
  if ($line -match 'thr=([0-9.]+)\s+\S+=([0-9.]+)\s+r=([0-9.]+)') { $o.Thr=[double]$Matches[1]; $o.Dv=[double]$Matches[2]; $o.R=[double]$Matches[3] }
  if ($line -match '\|a\|=([0-9.]+)') { $o.AbsA=[double]$Matches[1] }
  if ($line -match 'σ_ts=([0-9.]+)' -or $line -match 'sigma_ts=([0-9.]+)') { $o.SigmaTs=[double]$Matches[1] }
  if ($line -match 'ivel_set=([-+0-9.eE]+|NaN)') { if ($Matches[1] -ne 'NaN') { $o.IVelSet=[double]$Matches[1] } }
  if ($line -match 'v_wgc=([-+0-9.eE]+|NaN)') { if ($Matches[1] -ne 'NaN') { $o.VWgc=[double]$Matches[1] } }
  if ($line -match 'seed_path=(\S+)') { $o.SeedPath=$Matches[1] }
  if ($line -match 'shadow_v_log=([-+0-9.eE]+|NaN)') { if ($Matches[1] -ne 'NaN') { $o.ShadowV=[double]$Matches[1] } }
  if ($line -match 'INVALID (\S+)') { $o.InvalidReason=$Matches[1] }
  $rows.Add([pscustomobject]$o)
}

# Fix Dv parse: lines use Unicode Δv=
Get-Content -LiteralPath $RawPath | ForEach-Object {
  if ($_ -match '^TRIAL arm=(\S+) cell=(\S+) k=(\d+).*\s(\S+)=([0-9.]+)\s+r=([0-9.]+)') {
    # find row and set if Dv nan
  }
}
# Re-parse Dv with .NET for unicode
$utf = [System.IO.File]::ReadAllLines($RawPath)
$ri=0
foreach ($line in $utf) {
  if ($line -notmatch '^TRIAL arm=') { continue }
  if ($ri -ge $rows.Count) { break }
  $m = [regex]::Match($line, 'thr=([0-9.]+)\s+.*?=([0-9.]+)\s+r=([0-9.]+)')
  if ($m.Success) {
    $rows[$ri].Thr = [double]$m.Groups[1].Value
    $rows[$ri].Dv = [double]$m.Groups[2].Value
    $rows[$ri].R = [double]$m.Groups[3].Value
  }
  $sm = [regex]::Match($line, 'σ_ts=([0-9.]+)')
  if (-not $sm.Success) { $sm = [regex]::Match($line, 'sigma_ts=([0-9.]+)') }
  if ($sm.Success) { $rows[$ri].SigmaTs = [double]$sm.Groups[1].Value }
  $ri++
}

$nvh=@{}; $sigmaTs=@{}; $aAbs=@{}
$sb = New-Object System.Text.StringBuilder
[void]$sb.AppendLine("# m2_delta_v_s${SessionId}_summary.txt (offline adjudicate; probe crashed before DONE)")
[void]$sb.AppendLine("session=$SessionId offline=1 note=IVel0_t300dbl_incomplete_10/20_not_required_for_ticket")
[void]$sb.AppendLine("composition_expected=a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418")

foreach ($cell in $cells) {
  $baseDvs = @($rows | Where-Object { $_.Arm -eq 'base' -and $_.Cell -eq $cell -and $_.Valid -and -not [double]::IsNaN($_.Dv) } | Select-Object -ExpandProperty Dv)
  $nvh[$cell] = Percentile $baseDvs 0.95
  $sts = @($rows | Where-Object { $_.Arm -eq 'base' -and $_.Cell -eq $cell -and $_.Valid -and -not [double]::IsNaN($_.SigmaTs) } | Select-Object -ExpandProperty SigmaTs)
  $sigmaTs[$cell] = if ($sts.Count -gt 0) { ($sts | Measure-Object -Average).Average } else { [double]::NaN }
  $aa = @($rows | Where-Object { $_.Arm -eq 'base' -and $_.Cell -eq $cell -and $_.Valid -and -not [double]::IsNaN($_.AbsA) } | Select-Object -ExpandProperty AbsA)
  $aAbs[$cell] = if ($aa.Count -gt 0) { ($aa | Measure-Object -Average).Average } else { [double]::NaN }
  [void]$sb.AppendLine(("NVH cell={0} Nv_h_P95={1} n_base={2} sigma_ts={3} |a|={4}" -f $cell, (F $nvh[$cell]), $baseDvs.Count, (F $sigmaTs[$cell] 5), (F $aAbs[$cell] 2)))
}

$allCorePass=$true; $basePass=$true; $ivel0FailOk=$true
$cellVerdicts = New-Object System.Collections.Generic.List[string]

foreach ($cell in $cells) {
  $nv=$nvh[$cell]; $st=$sigmaTs[$cell]; $aa=$aAbs[$cell]
  $wgcValid = @($rows | Where-Object { $_.Arm -eq 'Arm-WGC-seed' -and $_.Cell -eq $cell -and $_.Valid } | Sort-Object K | Select-Object -First $NeedN)
  $thrMed = if ($wgcValid.Count -gt 0) { Median @($wgcValid | Select-Object -ExpandProperty Thr) } else { $FloorThr }
  $cellJud = (-not [double]::IsNaN($nv)) -and ($thrMed -ge 2.0*$nv) -and (-not [double]::IsNaN($aa)) -and (-not [double]::IsNaN($st)) -and ($aa*$st -le $thrMed/2.0)
  $role = if ($core -contains $cell) { 'core' } else { 'aux-t156' }
  $nPass = @($wgcValid | Where-Object {
    $passThr = ($_.Dv -le $_.Thr) -and ([double]::IsNaN($nv) -or $nv -le $_.Thr)
    $failDet = (-not [double]::IsNaN($nv)) -and ($_.Dv -gt $_.Thr + $nv)
    $passThr -and -not $failDet
  }).Count
  $cellPass = $cellJud -and ($wgcValid.Count -ge $NeedN) -and ($nPass -ge 17)
  $cellSkip = -not $cellJud

  $baseValid = @($rows | Where-Object { $_.Arm -eq 'base' -and $_.Cell -eq $cell -and $_.Valid } | Sort-Object K | Select-Object -First $NeedN)
  $basePassN = @($baseValid | Where-Object { $_.Dv -le $_.Thr }).Count
  $baseCellPass = ($baseValid.Count -ge $NeedN) -and ($basePassN -ge 17)

  $ivelValid = @($rows | Where-Object { $_.Arm -eq 'IVel0' -and $_.Cell -eq $cell -and $_.Valid } | Sort-Object K | Select-Object -First $NeedN)
  $ivelPassN = @($ivelValid | Where-Object {
    $passThr = ($_.Dv -le $_.Thr) -and ([double]::IsNaN($nv) -or $nv -le $_.Thr)
    $failDet = (-not [double]::IsNaN($nv)) -and ($_.Dv -gt $_.Thr + $nv)
    $passThr -and -not $failDet
  }).Count
  $ivelCellFail = ($ivelValid.Count -ge $NeedN) -and ($ivelPassN -lt 17)

  if ($cellSkip) {
    $verdict = 'Δv-SKIP'
    if ($core -contains $cell) { $allCorePass = $false }
  } elseif ($cellPass) {
    $verdict = 'PASS'
  } else {
    $verdict = 'FAIL'
    if ($core -contains $cell) { $allCorePass = $false }
  }
  if (($core -contains $cell) -and -not $baseCellPass) { $basePass = $false }
  if ($cell -eq 't060fwd' -or $cell -eq 't060rev') {
    if (-not $ivelCellFail) { $ivel0FailOk = $false }
  }
  $line = "CELL cell=$cell role=$role Nv_h=$(F $nv) thrMed=$(F $thrMed) |a|*σ=$(F ($aa*$st)) jud=$cellJud wgc_n=$($wgcValid.Count) wgc_pass=$nPass/20 base_pass=$basePassN/$($baseValid.Count) IVel0_pass=$ivelPassN/$($ivelValid.Count) verdict=$verdict"
  [void]$sb.AppendLine($line)
  [void]$cellVerdicts.Add("${cell}:${verdict}")
  Write-Host $line
}

$sessInvalid = $false
# incomplete IVel0 t300dbl is not hang_cap; mark note only
$hangExceed = ($hangByCell.Values | Where-Object { $_ -gt $HangCap }).Count -gt 0
if ($hangExceed) { $sessInvalid = $true }

if ($sessInvalid) { $sessVerdict = 'SESSION_INVALID' }
elseif ($allCorePass -and $basePass -and $ivel0FailOk) { $sessVerdict = 'SESSION_PASS_CANDIDATE' }
elseif (-not $allCorePass -and (($cellVerdicts | Where-Object { $_ -match 'FAIL' }).Count -gt 0)) { $sessVerdict = 'SESSION_FAIL_CANDIDATE' }
else { $sessVerdict = 'SESSION_INCONCLUSIVE_CANDIDATE' }

[void]$sb.AppendLine("ctrl basePass=$basePass IVel0_t060_both_FAIL=$ivel0FailOk")
[void]$sb.AppendLine("cells=$($cellVerdicts -join ',')")
[void]$sb.AppendLine($sessVerdict)
[void]$sb.AppendLine('===== M2-DELTA-V PROBE DONE =====')
[System.IO.File]::WriteAllText($OutSummary, $sb.ToString(), [System.Text.UTF8Encoding]::new($false))
Write-Host "WROTE $OutSummary"
Write-Host $sessVerdict
Write-Host "allCorePass=$allCorePass basePass=$basePass ivel0FailOk=$ivel0FailOk"
