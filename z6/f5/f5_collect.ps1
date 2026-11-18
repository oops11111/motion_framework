# Z6 F5 collect: copy %TEMP%\z6\f5 evidence into z6/f5/raw with local absolute paths replaced by %USERPROFILE%.
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path          # worktree root
$raw  = Join-Path $root 'z6\f5\raw'
$src  = Join-Path $env:TEMP 'z6\f5'
$user = $env:USERPROFILE
$pf   = $env:ProgramFiles
$manifest = New-Object System.Collections.Generic.List[string]
$manifest.Add('redaction: %USERPROFILE% and USERPROFILE -> %USERPROFILE%; Program Files -> %ProgramFiles%')
$manifest.Add('binary screenshots copied unchanged (window-only PrintWindow)')

function Copy-Redacted([string]$From, [string]$To) {
  $dir = Split-Path $To -Parent
  New-Item -ItemType Directory -Force -Path $dir | Out-Null
  $bytes = [IO.File]::ReadAllBytes($From)
  $sha = ([Security.Cryptography.SHA256]::Create().ComputeHash($bytes) | ForEach-Object { $_.ToString('x2') }) -join ''
  $isText = $From -match '\.(txt|log|cmd|json|xml)$'
  if ($isText) {
    $text = [Text.Encoding]::UTF8.GetString($bytes)
    $before = $text
    $text = $text.Replace($user, '%USERPROFILE%').Replace($pf, '%ProgramFiles%')
    $redacted = ($text -ne $before)
    [IO.File]::WriteAllText($To, $text, (New-Object Text.UTF8Encoding($false)))
    $manifest.Add("$($To.Replace($root + '\', '%WORKTREE%\')) redacted=$redacted src_sha=$sha")
  } else {
    Copy-Item $From $To -Force
    $manifest.Add("$($To.Replace($root + '\', '%WORKTREE%\')) redacted=False binary src_sha=$sha")
  }
}

# build logs
foreach ($f in Get-ChildItem (Join-Path $src 'build_wnae') -File) {
  Copy-Redacted $f.FullName (Join-Path $raw ('build_wnae\' + $f.Name))
}
# first successful build capture (console code page 936 mangled the Chinese text; kept for completeness)
if (Test-Path (Join-Path $src 'attempt2_console_gbk')) {
  foreach ($f in Get-ChildItem (Join-Path $src 'attempt2_console_gbk') -File) {
    Copy-Redacted $f.FullName (Join-Path $raw ('attempt2_console_gbk\' + $f.Name))
  }
}

# run artefacts
$runDst = Join-Path $raw 'run'
New-Item -ItemType Directory -Force -Path $runDst | Out-Null
foreach ($f in Get-ChildItem (Join-Path $src 'run') -File) {
  Copy-Redacted $f.FullName (Join-Path $runDst $f.Name)
}
$framesDst = Join-Path $runDst 'frames'
New-Item -ItemType Directory -Force -Path $framesDst | Out-Null
foreach ($f in Get-ChildItem (Join-Path $src 'run\frames') -File) {
  Copy-Redacted $f.FullName (Join-Path $framesDst $f.Name)
}

# frozen-surface diff (must be empty)
$lines = @('git diff --name-only HEAD -- <frozen>')
$frozen = @('code/composition.h', 'code/capi/fm_capi.h', 'code/capi/fm_capi.cpp',
            'MotionSoftDemo/MotionEtw.cs', 'docs/api_v0_frozen.md',
            'docs/api_surface_m2_alpha_contract_LOCKED.md')
$d1 = & git -C $root diff --name-only HEAD -- @frozen 2>&1
$lines += "exit=$LASTEXITCODE"
$lines += $d1
$lines += ''
$lines += "git diff --name-only 81b3f15cdf44b8207fa843bb819bf01416973316 -- <frozen>"
$d2 = & git -C $root diff --name-only 81b3f15cdf44b8207fa843bb819bf01416973316 -- @frozen 2>&1
$lines += "exit=$LASTEXITCODE"
$lines += $d2
[IO.File]::WriteAllText((Join-Path $raw 'frozen_diff.txt'), (($lines -join "`n") + "`n"), (New-Object Text.UTF8Encoding($false)))

# F5 log analysis (no timing values)
$log = Join-Path $src 'run\z6_f5_motion.log'
$text = Get-Content $log -Encoding utf8
$f5 = $text | Select-String -Pattern 'F5 ' -SimpleMatch
$an = New-Object System.Collections.Generic.List[string]
$an.Add("lines=$($text.Count)")
$an.Add("F5 lines=$($f5.Count)")
foreach ($m in $f5) { $an.Add(("L{0}: {1}" -f $m.LineNumber, $m.Line)) }
$an.Add("F5 source=event lines=" + (($text | Select-String -Pattern 'source=event' -SimpleMatch).Count) + " (0 = system setting never switched in this run)")
$an.Add("FrameTick deltaUs payload lines=" + (($text | Select-String -Pattern 'deltaUs' -SimpleMatch).Count) + " (FrameTick appears only as a count inside ETW_SUMMARY)")
$an.Add("package still registered after run=" + ((& (Join-Path $env:SystemRoot 'System32\WindowsPowerShell\v1.0\powershell.exe') -NoProfile -Command "@(Get-AppxPackage -Name 49462fluenthub-uwp.FluentHub).Count") -join ''))
[IO.File]::WriteAllText((Join-Path $raw 'analysis.txt'), (($an -join "`n") + "`n"), (New-Object Text.UTF8Encoding($false)))

[IO.File]::WriteAllLines((Join-Path $raw 'redaction_manifest.txt'), $manifest, (New-Object Text.UTF8Encoding($false)))
'collected'
