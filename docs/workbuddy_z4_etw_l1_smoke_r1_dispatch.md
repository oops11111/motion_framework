# workbuddy dispatch: Z4 ETW + L1 smoke r1 (tooling and collection runbook)

- **TICKET**: Z4 ETW + L1 smoke r1
- **BRANCH**: `claude/z3a-cabi-l1`
- **GATE**: `docs/z4_etw_l1_smoke_gate_r1_LOCKED.md`
- **GATE_BODY_SHA256**: `acb89d1b0cb975e5d63ae2c6fc47adcbc74dddd959b63089b2546b692cadab14`
- **APPROVED record**: `docs/z4_etw_l1_smoke_r1_approved.md` (pidan, phrase `APPROVED Z4 r1`). The LOCKED file body is not edited.
- **HOST**: MotionSoftDemo only. Not FluentHub, not Files.
- **PERF-CI**: HOLD. This is not a frame-budget run and not adopt.

## Declared collection tip

The declared collection tip is the final tip of the Z4 tooling push on `claude/z3a-cabi-l1`: the HEAD commit that contains this dispatch. The push report states that full SHA. Build that commit and no other. Do not collect from an ancestor, a descendant, or a dirty tree.

`MotionSoftDemo` stamps `git rev-parse HEAD` and the `code/composition.h` SHA-256 into `z4_build_info.txt` next to the exe. The session copies that tip into `env.json`. If `env.json` `tip` is empty or differs from the declared HEAD, the session is **INVALID**. Optional check: set `MF_Z4_EXPECTED_TIP` to the declared SHA before launch; a mismatch is `tip_mismatch`.

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

`env.json` records machine, OS, packaged or unpackaged, package full name when packaged, process id, tip SHA, `composition.h` SHA, the expected SHA, and the resolved outdir.

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

Those paths sit under `%USERPROFILE%\motion_framework\z4`, which the package manifest excludes from file-system write virtualization. A packaged session that uses a path outside that exclusion (and outside `%LOCALAPPDATA%\MotionSoftDemoZ4`) can be redirected to:

```
%LOCALAPPDATA%\Packages\<PackageFamilyName>\LocalCache\
```

`env.json` `package_full_name` is the `<PackageFamilyName>` source. If the files are not at the path you passed, look there before calling the session missing.

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

The process stays up until `dotnet-trace` finishes its 50 s capture (`--duration 00:00:50`), then writes `verdict.json` and closes. Repeat from a new console for the next session. Unpackaged writes are not virtualized; the directory you passed is the directory on disk.

## Packaged build, cert, install, run

Placeholder logos are `MotionSoftDemo\Assets\StoreLogo.png` (50×50), `Square44x44Logo.png` (44×44), `Square150x150Logo.png` (150×150), `Wide310x150Logo.png` (310×150), and `SplashScreen.png` (620×300). Identity publisher is `CN=MotionSoftDemo Z4 Dev`. The manifest is `MotionSoftDemo\Package.appxmanifest`. It is not used by the default unpackaged build.

Create a self-signed cert whose subject matches that publisher, trust it for sideload, and do not commit the pfx:

```powershell
$cert = New-SelfSignedCertificate -Type Custom -Subject "CN=MotionSoftDemo Z4 Dev" -KeyUsage DigitalSignature -FriendlyName "MotionSoftDemo Z4 Dev" -CertStoreLocation "Cert:\CurrentUser\My" -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}")
$pwd = ConvertTo-SecureString -String "z4-dev" -Force -AsPlainText
Export-PfxCertificate -Cert $cert -FilePath MotionSoftDemo\Z4Dev.pfx -Password $pwd
Import-PfxCertificate -FilePath MotionSoftDemo\Z4Dev.pfx -Password $pwd -CertStoreLocation "Cert:\CurrentUser\TrustedPeople"
$cert.Thumbprint
```

Build the MSIX. `fluidmotion.dll` must already be at `MotionNative\fluidmotion.dll`.

```bat
dotnet build MotionSoftDemo\MotionSoftDemo.csproj -c Debug -p:Platform=x64 -p:Z4Package=true -p:AppxPackageSigningEnabled=true -p:PackageCertificateKeyFile=MotionSoftDemo\Z4Dev.pfx -p:PackageCertificatePassword=z4-dev
```

`-p:WindowsPackageType=MSIX` is the same switch as `-p:Z4Package=true`. Omit both for the unpackaged exe.

Install (sideload) the signed msix printed by the build (under `MotionSoftDemo\AppPackages` or the bin `AppX` layout):

```powershell
Add-AppxPackage -Path <path-to-signed.msix>
```

The manifest registers the execution alias `MotionSoftDemo.exe`. Open a new console after install so the alias is on PATH. Launch one packaged session with the flag (the Start menu tile does not forward arguments):

```bat
MotionSoftDemo.exe --z4-l1-session %USERPROFILE%\motion_framework\z4\results_etw_l1_smoke_r1\K93N3P8\s3-packaged
```

Or, without putting the flag in the alias command:

```bat
set MF_Z4_L1_SESSION=%USERPROFILE%\motion_framework\z4\results_etw_l1_smoke_r1\K93N3P8\s3-packaged
MotionSoftDemo.exe
```

Use the same pattern on IJI1KJG with `IJI1KJG` in the path. Packaged output is the path you passed when it is under `%USERPROFILE%\motion_framework\z4` or `%LOCALAPPDATA%\MotionSoftDemoZ4`. Otherwise search `LocalCache` as above. `env.json` `windows_package_type` must be `msix` for the packaged session and `unpackaged` for the others.

## EventPipe command

This is the command the session starts. Level 5 is Verbose.

```bat
dotnet-trace collect --process-id <pid> --providers FluidMotion-Motion:0xFFFFFFFFFFFFFFFF:5 --duration 00:00:50 --output <session>\trace.nettrace
```

`<pid>` is the MotionSoftDemo process. The app passes its own pid and writes `<outdir>\trace.nettrace`. Override the duration with `MF_Z4_TRACE_DURATION` in `hh:mm:ss` form only if 50 s is not enough. Do not stop the process early; the nettrace is finished when `dotnet-trace` exits.

Check the trace (also done inside the session before `verdict.json` is written):

```bat
dotnet run --project tools\z4_check_trace -c Release -- <session>\trace.nettrace
```

Exit 0 means magic `Nettrace`, provider name, provider GUID, and all six names (`AnimationLifecycle`, `Retarget`, `FrameTick`, `PresentStats`, `IdleTransition`, `TokenLoad`). A text log from `--z3a-l1-dev` fails this check. Self-test, not a session:

```bat
dotnet run --project tools\z4_check_trace -c Release -- --self-test
```

## composition.h PRE / POST

Do this on the collection machine before the first build and again after the last session. Do not edit the file.

```powershell
Get-FileHash -Algorithm SHA256 -Path code\composition.h
```

PRE and POST must both be:

```
a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418
```

The session hashes the header when it can see `code\composition.h`, otherwise it uses the stamp in `z4_build_info.txt`, and writes the value to `env.json`. Anything other than the digest above is `composition_h_mismatch` and **INVALID**.

## INVALID (do not count the session)

Any one of these makes the session INVALID:

1. Collected before the gate was LOCKED, or without the recorded `APPROVED Z4 r1`, or with the Z3a L1 build missing.
2. `composition.h` PRE ≠ POST, or either side differs from `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`.
3. Shadow initial velocity, or a tip that was not declared, or a tip that does not match the declared HEAD (`undeclared_tip`, `tip_mismatch`).
4. Treating PERF-CI HOLD as passed, including any claim that this run met a frame budget.
5. Missing artifacts (`verdict.json`, `session.log`, `trace.nettrace`, `env.json`) or a gate-body hash other than `acb89d1b0cb975e5d63ae2c6fc47adcbc74dddd959b63089b2546b692cadab14`.
6. A Z3a dev smoke (`--z3a-l1-dev` / `MF_Z3A_L1_DEV=1`, or a log under `%TEMP%\z3a_cabi_l1_dev`) submitted as a Z4 session. Passing both flags writes `dev_mode_instead_of_z4` and does not run the scenario.
7. The session entry ran twice in one process (`session_run_in_process_twice`).
8. EventPipe never started or `trace.nettrace` was not written.

`FAIL` is a real session whose G2 row missed. Do not re-run a FAIL to fish for a pass. A new process and a new directory are a new session only when the previous one was INVALID for an operational reason (collector missing, wrong tip) that has been corrected. Do not delete a FAIL to hide it.

## Not in this run

No merge to `main`. No new `fm_*` exports. No change to `fm_capi.h` / `fm_capi.cpp`. No `RiveView`, no `PointerFollow`, no WinRT Composition backend, no R-3 / #52 work, no 0.3·T default.
