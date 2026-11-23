# CI coverage, as verified from the workflow files (MF-17)

Verified 2026-11-23 by reading the `on:` blocks of all 15 files under
`.github/workflows/`. This is a record of what triggers, not of runs.

## What runs on a push to main

Only two workflows trigger on `push` to `main`:

- `z6-integration-tests.yml` — "Z6 deterministic integration regression".
  Triggers: push to `codex/z6-integration-tests` or `main`, and PRs to `main`,
  no paths filter. Jobs: `policy-and-evidence` (runs
  `z6/integration/tests/verify.py`: frozen-source and evidence checks) and
  `native-and-managed` (Debug/Release, runs `z6/integration/tests/run_tests.py`:
  native tests, the `z3a_l1_devsmoke` console build, surface comparison).
  This is the only workflow that runs on every push to main.
- `everystage-component.yml` — "Formal internal component candidate".
  Triggers: push to `codex/everystage-component` or `main`, and PRs to `main`,
  paths-limited to `src/FluidMotion.L1/**`, `code/**`, and
  `MotionSoftDemo/MotionSystem.cs`. Job: `package-and-independent-consumer`
  (L1 pack via `tools/delivery/build_component.py` plus an independent
  consumer build). It runs on a main push only when one of those paths
  changed.

## The other 13 workflows (branch/PR evidence only)

Each triggers on push to its own `codex/*` branch and on PRs to `main` with a
narrow paths filter; none of them runs on a push to `main`:

- `z6-closure.yml` — push `codex/z6-closure`; PR paths `docs/z6_closed.md`.
- `z6-f2-repair.yml` — push `codex/z6-f2-repair`; PR paths `z6/f2/**`.
- `z6-f5.yml` — push `codex/z6-f5`; PR to `codex/z6-integration` or `main`.
- `z6-f9a-baseline.yml` — push `codex/z6-f9a-baseline`; PR paths
  `z6/f9a/baseline-codex/**`.
- `z6-f9a-deployment-preflight.yml` — push
  `codex/z6-f9a-deployment-preflight`; PR paths
  `z6/f9a/deployment-preflight/**`.
- `z6-f9a-devlayout.yml` — push `codex/z6-f9a-devlayout`; PR paths
  `z6/f9a/devlayout-codex/**`.
- `z6-f9a-fullsolution.yml` — push `codex/z6-f9a-fullsolution`; PR paths
  `z6/f9a/fullsolution-codex/**`.
- `z6-f9a-packagesconfig.yml` — push `codex/z6-f9a-packagesconfig`; PR paths
  `z6/f9a/packagesconfig-codex/**`.
- `z6-f9a-vsdeploy.yml` — push `codex/z6-f9a-vsdeploy`; PR paths
  `z6/f9a/vsdeploy-codex/**`.
- `z6-f9a.yml` — push `codex/z6-f9a`; PR paths `pilot/files/**`.
- `z6-f9b-pilot.yml` — push `codex/z6-f9b-pilot`; PR paths `z6/f9b/codex/**`.
- `z6-integration-docs.yml` — push `codex/z6-integration-docs`; PR paths
  `docs/z5_closed.md`.
- `z6-integration-pack.yml` — push `codex/z6-integration-pack`; PR paths
  `pilot/l1pack/README.md`.

## Gaps

- No workflow builds the WinUI demo app (`MotionSoftDemo.csproj`) on a push
  to `main`. The managed build in `z6-integration-tests` compiles only the
  `z3a_l1_devsmoke` console project (`MotionSystem.cs`, `MotionEtw.cs`,
  `Z3aL1DevSmoke.cs`). A green CI run therefore does NOT prove that
  `FmBridge.cs` or `MainPage.xaml.cs` compile; those need a Windows
  Visual Studio / solution build.
- No workflow builds the full solution on a push to `main`; the
  full-solution evidence workflow is PR-only.
- PERF-CI remains HOLD per `docs/z6_closed.md`.
