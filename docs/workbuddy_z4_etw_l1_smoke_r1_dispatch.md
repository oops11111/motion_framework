# workbuddy dispatch: Z4 ETW + L1 smoke r1 (tooling and collection runbook)

- **TICKET**: Z4 ETW + L1 smoke r1
- **BRANCH**: `claude/z3a-cabi-l1`
- **GATE**: `docs/z4_etw_l1_smoke_gate_r1_LOCKED.md`
- **GATE_BODY_SHA256**: `acb89d1b0cb975e5d63ae2c6fc47adcbc74dddd959b63089b2546b692cadab14`
- **APPROVED record**: `docs/z4_etw_l1_smoke_r1_approved.md` (pidan, phrase `APPROVED Z4 r1`). The LOCKED file body is not edited.
- **HOST**: MotionSoftDemo only. Not FluentHub, not Files.
- **PERF-CI**: HOLD. This is not a frame-budget run and not adopt.

## Declared collection tip

The declared collection tip is the commit that contains this file, `docs/workbuddy_z4_etw_l1_smoke_r1_dispatch.md`, on `claude/z3a-cabi-l1`. After this fix push that commit is HEAD. Confirm before building:

```powershell
git rev-parse HEAD
git log -1 --format=%H -- docs/workbuddy_z4_etw_l1_smoke_r1_dispatch.md
```

Those two SHAs must be the same. That SHA is the collection tip. `33c42c52f0c21522463fd5fb03fc11d0c56a792d` is the parent tooling commit and is not the collection tip. Do not collect from an ancestor, a descendant, or a dirty tree.

`MotionSoftDemo` stamps `git rev-parse HEAD` into `z4_build_info.txt` next to the exe. The tip is a commit id, not a file hash, so `core.autocrlf` does not change it. The same file stamps `composition.h` twice: `composition_sha256` is the CRLF→LF normalized digest (the PASS value) and `composition_sha256_raw` is the on-disk bytes. The session copies both into `env.json`. If `env.json` `tip` is empty or differs from the declared HEAD, the session is **INVALID**. Optional check: set `MF_Z4_EXPECTED_TIP` to the declared SHA before launch; a mismatch is `tip_mismatch`.

## What a session is

One process is one session. The WinUI app runs it. It is not `--z3a-l1-dev` and it does not write `artifacts/m2_02_*` or `%TEMP%\z3a_cabi_l1_dev`.

Flag (directory argument required):

```bat
MotionSoftDemo.exe --z4-l1-session <outdir>
```

Environment variable (same directory; used when the flag is absent):

```bat
set MF_Z4_L1_SESSION=<outdir>
```

`<outdir>` is the session directory. Create a new directory per process. The app creates it and writes only there.

G2 numbers are the locked gate, not a renumbering:

| Locked G2 | What the session checks |
|---|---|
| ① L1 sequence | `MotionSystem` create, one bad token file whose diagnostic contains a JSON Pointer (`/bogus`), `MotionTokens` load of `demo_motion.json`, `CreateAnimation`, `Play`, `Retarget` ≥ 5 covering a mid-play interrupt, a same-property retarget, and a cross-animation switch, then `Cancel`, then `Release` with motion continuing |
| ② Continuity | every measured `Retarget` continues from `Current`. `dynTol = max(2, abs(vel)*0.040+1)` DIP, no teleport onto the new target, frame step < 80 DIP, `jumpFails = 0` |
| ③ Reduced motion | in-flight animation snaps to the end; a `Retarget` while reduced motion is on lands on the target with velocity 0 |
| ④ Idle | after animations end, the frame source stops within ≤ 2 frames, then 0 frame callbacks for 5 s, and `IdleTransition` ≥ 1 |
| ⑤ Thread | `CrossThreadHits = 0` (creating UI thread only) |
| ⑥ ETW | the EventPipe nettrace contains all 6 E1 names |

E3 is that nettrace. Provider `FluidMotion-Motion`, GUID `8f3a2c1d-6b4e-4a91-9c7d-2e5f8a1b3c4d`, level Verbose so `FrameTick` is included. `logman` is not required for PASS.

Spring used by the session: response 0.32 s, damping 0.78 (`demo_motion.json` `springs.snappy`). That is not 0.3·T. `PresentStats` uses a 16.666 ms frame interval so the sixth event name exists. It is not a pacing offset.

`verdict.json` `status` is `PASS` only when ①–⑥ all pass and no INVALID reason is set. `FAIL` means the session ran and a criterion missed. `INVALID` means the session must not be counted.

## Results layout

```
z4/results_etw_l1_smoke_r1/<machine>/<session>/
  verdict.json
  session.log
  trace.nettrace
  env.json
  bad_tokens.json
```

`<machine>` is `K93N3P8` or `IJI1KJG`. `<session>` is a new name per process (`s1-unpackaged`, `s2-unpackaged`, `s3-packaged`, …).

`env.json` records machine, OS, packaged or unpackaged, package full name and package family name, process id, tip SHA, normalized `composition_sha256`, raw `composition_sha256_raw`, the expected SHA, `requested_outdir`, and `resolved_outdir` (the real directory the process wrote). `outdir` is the same path as `resolved_outdir`.

Do not commit this tree. `.gitignore` already excludes `z4/results_etw_l1_smoke_r1/`. Do not write `artifacts/m2_02_*` or `m3/results_*`.

Suggested outdirs, from the repo root:

```
z4\results_etw_l1_smoke_r1\K93N3P8\s1-unpackaged
z4\results_etw_l1_smoke_r1\K93N3P8\s2-unpackaged
z4\results_etw_l1_smoke_r1\K93N3P8\s3-packaged
z4\results_etw_l1_smoke_r1\IJI1KJG\s1-unpackaged
z4\results_etw_l1_smoke_r1\IJI1KJG\s2-unpackaged
z4\results_etw_l1_smoke_r1\IJI1KJG\s3-packaged
```

Unpackaged: the app writes exactly to the path you passed. `resolved_outdir` equals `requested_outdir`.

Packaged: the manifest does not disable filesystem virtualization (Win10 19045 MakeAppx rejects `windows.fileSystemWriteVirtualization`). The canonical files are written to package LocalState, which is a real directory the same user can read:

```
%LOCALAPPDATA%\Packages\<PackageFamilyName>\LocalState\z4\results_etw_l1_smoke_r1\<machine>\<session>\
```

`env.json` `resolved_outdir` and `session.log` line `RESOLVED_OUTDIR=` are that path. `dotnet-trace` is started by the app against its own pid and writes `trace.nettrace` into that same directory. Attach needs no admin. The trace path is the LocalState path, which the user can write.

The app also tries to copy the same files to `requested_outdir`. A packaged process may only see that copy through virtualization. The on-disk place for a profile path is then:

```
%LOCALAPPDATA%\Packages\<PackageFamilyName>\LocalCache\Local\<path under the user profile>
```

`env.json` `virtualized_mirror_guess` is that prediction. Do not collect the packaged session from the repo path or from LocalCache. Copy from `resolved_outdir` with an external PowerShell after the process exits (see Packaged collect below).

## Per-machine plan

Only these two machines. Do not add any other machine.

| Machine | OS | Processes | Mix |
|---|---|---|---|
| K93N3P8 | Windows 10 | ≥ 3 independent processes | ≥ 1 unpackaged and ≥ 1 packaged |
| IJI1KJG | Windows 11 | ≥ 3 independent processes | ≥ 1 unpackaged and ≥ 1 packaged |

Each process is started, runs one session, writes one directory, and exits (`CloseMainWindowSoon`). Do not launch the session twice in the same process. Do not pass `--z3a-l1-dev` or `MF_Z3A_L1_DEV=1` on a Z4 launch. The three suggested directories above are the minimum; extra sessions are allowed only as additional new processes and new directories.

## Prerequisites

1. Declared tip checked out, worktree clean.
2. .NET SDK that can build `net10.0-windows10.0.26100.0` (the unpackaged dogfood) and `net8.0` (the trace checker).
3. EventPipe collector, once per machine:

```bat
dotnet tool install --global dotnet-trace
```

`dotnet-trace` must be on PATH or at `%USERPROFILE%\.dotnet\tools\dotnet-trace.exe`. Override with `DOTNET_TRACE` set to the full exe path. The app starts the collector itself. If it is missing, `verdict.json` is `INVALID` (`eventpipe_not_started` / `missing_trace`).

4. MSVC x64 DLL, from the repo root, before either app build so the DLL is copied (and, for MSIX, packed):

```bat
cl /nologo /std:c++17 /EHsc /O2 /W3 /LD /I code\capi /I code /I code\explore code\capi\fm_capi.cpp /Fe:MotionNative\fluidmotion.dll
```

## Unpackaged build and run

Default. Do not pass `Z4Package` or `WindowsPackageType`.

```bat
dotnet build MotionSoftDemo\MotionSoftDemo.csproj -c Debug -p:Platform=x64
```

The exe is under:

```
MotionSoftDemo\bin\x64\Debug\net10.0-windows10.0.26100.0\win-x64\MotionSoftDemo.exe
```

Run one session (new process, new directory):

```bat
MotionSoftDemo\bin\x64\Debug\net10.0-windows10.0.26100.0\win-x64\MotionSoftDemo.exe --z4-l1-session z4\results_etw_l1_smoke_r1\K93N3P8\s1-unpackaged
```

The process stays up until `dotnet-trace` finishes its 50 s capture (`--duration 00:00:50`), then writes `verdict.json` and closes. Repeat from a new console for the next session. Unpackaged writes are not virtualized; `resolved_outdir` is the directory you passed.

## Packaged build, cert, install, run, collect

Placeholder logos are `MotionSoftDemo\Assets\StoreLogo.png` (50×50), `Square44x44Logo.png` (44×44), `Square150x150Logo.png` (150×150), `Wide310x150Logo.png` (310×150), and `SplashScreen.png` (620×300). Identity publisher is `CN=MotionSoftDemo Z4 Dev`. The manifest is `MotionSoftDemo\Package.appxmanifest`. `MinVersion` stays `10.0.17763.0`, which covers Win10 19045 and Win11. The manifest has no `desktop6` filesystem-virtualization extension; that category is not in the Win10 19045 MakeAppx allow-list (`windows.service`, `windows.mutablePackageDirectories`, `windows.customInstall`) and fails with C00CE169 / 0x80080204. The manifest is not used by the default unpackaged build.

`mspdbcmf.exe` converts PDBs for the Store symbol package (`.appxsym`). It is not used to compile, sign, install, or run the MSIX. The project sets `AppxSymbolPackageEnabled=false` for the MSIX build so a missing `mspdbcmf.exe` does not fail the package. Pass the same property on the command line as well.

Do not reuse the cert that failed APPX0107 (thumbprint `8FD7A7D4…89F2`). Create a new one. Subject must be exactly the manifest Publisher. EKU is code signing `1.3.6.1.5.5.7.3.3`, basic constraints are not a CA, the key is exportable, and `KeySpec Signature` uses the legacy CSP so Win10 19045 can import a pfx (APPX0105). The pfx lives outside the repo.

```powershell
$cert = New-SelfSignedCertificate -Type Custom -Subject "CN=MotionSoftDemo Z4 Dev" -KeyUsage DigitalSignature -KeyAlgorithm RSA -KeyLength 2048 -HashAlgorithm SHA256 -KeyExportPolicy Exportable -KeySpec Signature -Provider "Microsoft Enhanced RSA and AES Cryptographic Provider" -FriendlyName "MotionSoftDemo Z4 Dev" -CertStoreLocation "Cert:\CurrentUser\My" -NotAfter (Get-Date).AddYears(5) -TextExtension @("2.5.29.37={critical}{text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}false")
$pwd = ConvertTo-SecureString -String "z4-dev" -Force -AsPlainText
$certDir = Join-Path $env:USERPROFILE "z4_dev_cert"
New-Item -ItemType Directory -Force -Path $certDir | Out-Null
Export-PfxCertificate -Cert $cert -FilePath (Join-Path $certDir "Z4Dev.pfx") -Password $pwd
Import-PfxCertificate -FilePath (Join-Path $certDir "Z4Dev.pfx") -Password $pwd -CertStoreLocation "Cert:\CurrentUser\TrustedPeople"
$cert.Thumbprint
```

Primary build uses the thumbprint of that cert in `Cert:\CurrentUser\My`. `fluidmotion.dll` must already be at `MotionNative\fluidmotion.dll`. Do not pass a pfx on this command.

```powershell
$thumb = $cert.Thumbprint
dotnet build MotionSoftDemo\MotionSoftDemo.csproj -c Debug -p:Platform=x64 -p:Z4Package=true -p:AppxPackageSigningEnabled=true -p:PackageCertificateThumbprint=$thumb -p:AppxSymbolPackageEnabled=false
```

`-p:WindowsPackageType=MSIX` is the same switch as `-p:Z4Package=true`. Omit both for the unpackaged exe.

Pfx fallback, only if the thumbprint build cannot see the cert. The pfx is `%USERPROFILE%\z4_dev_cert\Z4Dev.pfx`, not a path inside the repo.

```bat
dotnet build MotionSoftDemo\MotionSoftDemo.csproj -c Debug -p:Platform=x64 -p:Z4Package=true -p:AppxPackageSigningEnabled=true -p:PackageCertificateKeyFile=%USERPROFILE%\z4_dev_cert\Z4Dev.pfx -p:PackageCertificatePassword=z4-dev -p:AppxSymbolPackageEnabled=false
```

Install (sideload) the signed msix printed by the build (under `MotionSoftDemo\AppPackages` or the bin `AppX` layout). Developer Mode must be on.

```powershell
Add-AppxPackage -Path <path-to-signed.msix>
```

The manifest registers the execution alias `MotionSoftDemo.exe`. Open a new console after install so the alias is on PATH. Launch one packaged session with the flag (the Start menu tile does not forward arguments):

```bat
MotionSoftDemo.exe --z4-l1-session %USERPROFILE%\motion_framework\z4\results_etw_l1_smoke_r1\K93N3P8\s3-packaged
```

Or:

```bat
set MF_Z4_L1_SESSION=%USERPROFILE%\motion_framework\z4\results_etw_l1_smoke_r1\K93N3P8\s3-packaged
MotionSoftDemo.exe
```

Wait until the process exits (the 50 s EventPipe capture). `dotnet-trace` 10.x attaches to that packaged pid as the same user, with no admin, and writes `trace.nettrace` under LocalState. Then collect into the repo from an external PowerShell (this window is not the packaged process):

```powershell
$envFile = Get-ChildItem "$env:LOCALAPPDATA\Packages" -Directory -Filter "MotionSoftDemo_*" | ForEach-Object {
  $p = Join-Path $_.FullName "LocalState\z4\results_etw_l1_smoke_r1\K93N3P8\s3-packaged\env.json"
  if (Test-Path $p) { $p }
} | Select-Object -First 1
$info = Get-Content -Raw $envFile | ConvertFrom-Json
$src = $info.resolved_outdir
$dst = Join-Path (Get-Location) "z4\results_etw_l1_smoke_r1\K93N3P8\s3-packaged"
New-Item -ItemType Directory -Force -Path $dst | Out-Null
Copy-Item -Path (Join-Path $src "*") -Destination $dst -Force
Get-Content (Join-Path $dst "env.json")
```

`$info.resolved_outdir` must be the LocalState directory, and the copied `env.json` must show `"packaged": true` and `"windows_package_type": "msix"`. `trace.nettrace` in `$dst` is the file to check. Use `IJI1KJG` and that machine's session name the same way. Do not change git config.

## EventPipe command

This is the command the session starts. Level 5 is Verbose.

```bat
dotnet-trace collect --process-id <pid> --providers FluidMotion-Motion:0xFFFFFFFFFFFFFFFF:5 --duration 00:00:50 --output <session>\trace.nettrace
```

`<pid>` is the MotionSoftDemo process (unpackaged or packaged). The app passes its own pid. `--output` is `<resolved_outdir>\trace.nettrace`: the requested directory when unpackaged, and the package LocalState directory when packaged. Override the duration with `MF_Z4_TRACE_DURATION` in `hh:mm:ss` form only if 50 s is not enough. Do not stop the process early; the nettrace is finished when `dotnet-trace` exits.

Check the trace (also done inside the session before `verdict.json` is written):

```bat
dotnet run --project tools\z4_check_trace -c Release -- <session>\trace.nettrace
```

Exit 0 means magic `Nettrace`, provider name, provider GUID, and all six names (`AnimationLifecycle`, `Retarget`, `FrameTick`, `PresentStats`, `IdleTransition`, `TokenLoad`). A text log from `--z3a-l1-dev` fails this check. Self-test, not a session:

```bat
dotnet run --project tools\z4_check_trace -c Release -- --self-test
```

## composition.h PRE / POST

Do this on the collection machine before the first build and again after the last session. Do not edit the file. Do not change `core.autocrlf`. PASS is the CRLF→LF normalized digest. With `core.autocrlf=true` the raw working-tree digest is `33d826c77d99c3feb545b79f16a5a3fc9f5ac200716f24759dfcbf8e60196e97` and that raw value is not a mismatch.

```powershell
$path = "code\composition.h"
$bytes = [System.IO.File]::ReadAllBytes((Resolve-Path $path))
$sha = [System.Security.Cryptography.SHA256]::Create()
$raw = ([System.BitConverter]::ToString($sha.ComputeHash($bytes))).Replace("-", "").ToLowerInvariant()
$norm = New-Object System.Collections.Generic.List[byte]
for ($i = 0; $i -lt $bytes.Length; $i++) {
  if ($bytes[$i] -eq 13) {
    $norm.Add(10)
    if (($i + 1) -lt $bytes.Length -and $bytes[$i + 1] -eq 10) { $i++ }
  } else { $norm.Add($bytes[$i]) }
}
$normalized = ([System.BitConverter]::ToString($sha.ComputeHash($norm.ToArray()))).Replace("-", "").ToLowerInvariant()
$sha.Dispose()
"raw        $raw"
"normalized $normalized"
```

PRE and POST `normalized` must both be:

```
a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418
```

Record both lines. The session does the same normalization when it can see `code\composition.h`. Otherwise it uses `composition_sha256` / `composition_sha256_raw` from `z4_build_info.txt` (the build stamp is also normalized). `env.json` stores both. A normalized digest other than the value above is `composition_h_mismatch` and **INVALID**. The git tip check is `git rev-parse`, not a file hash, so autocrlf does not apply to it.

## INVALID (do not count the session)

Any one of these makes the session INVALID:

1. Collected before the gate was LOCKED, or without the recorded `APPROVED Z4 r1`, or with the Z3a L1 build missing.
2. `composition.h` normalized PRE ≠ normalized POST, or either normalized digest differs from `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`. A raw autocrlf digest of `33d826c7…6e97` with that normalized digest is not INVALID.
3. Shadow initial velocity, or a tip that was not declared, or a tip that does not match the declared HEAD (`undeclared_tip`, `tip_mismatch`).
4. Treating PERF-CI HOLD as passed, including any claim that this run met a frame budget.
5. Missing artifacts (`verdict.json`, `session.log`, `trace.nettrace`, `env.json`) or a gate-body hash other than `acb89d1b0cb975e5d63ae2c6fc47adcbc74dddd959b63089b2546b692cadab14`.
6. A Z3a dev smoke (`--z3a-l1-dev` / `MF_Z3A_L1_DEV=1`, or a log under `%TEMP%\z3a_cabi_l1_dev`) submitted as a Z4 session. Passing both flags writes `dev_mode_instead_of_z4` and does not run the scenario.
7. The session entry ran twice in one process (`session_run_in_process_twice`).
8. EventPipe never started or `trace.nettrace` was not written.

`FAIL` is a real session whose G2 row missed. Do not re-run a FAIL to fish for a pass. A new process and a new directory are a new session only when the previous one was INVALID for an operational reason (collector missing, wrong tip) that has been corrected. Do not delete a FAIL to hide it.

## Not in this run

No merge to `main`. No new `fm_*` exports. No change to `fm_capi.h` / `fm_capi.cpp`. No `RiveView`, no `PointerFollow`, no WinRT Composition backend, no R-3 / #52 work, no 0.3·T default.
