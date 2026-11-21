"""Validate this delivery and execute the actual platform-independent source state.

The Windows Fake named-event tests run separately via --run-source-tests. These
checks do not claim that the WinUI host builds or receives OS broadcasts.
"""
from pathlib import Path
import argparse
import hashlib
import subprocess
import sys
import tempfile

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[2]
BASE = "e4cd696c10ad3d9a948c5650295be7fbbbc301cc"
PATCH = HERE / "fluenthub_f5_window_source.patch"


def git(*args):
    return subprocess.check_output(["git", "-C", str(ROOT), *args])


def extract_added_sources(destination):
    """Extract actual new-file patch bodies, rejecting unexpected paths/hunks."""
    blocks = PATCH.read_text(encoding="utf-8").split("--- ")
    names = set()
    for block in blocks[1:]:
        lines = block.splitlines()
        if lines[0] != "/dev/null":
            continue
        name = lines[1].removeprefix("+++ b/src/FluentHub/Views/")
        if name not in {"ReducedMotionState.cs", "FakeReducedMotionSource.cs", "WindowReducedMotionSource.cs"}:
            raise ValueError(f"unexpected new file {name}")
        body = lines[3:]
        if not lines[2].startswith("@@ -0,0 +1,") or any(not line.startswith("+") for line in body):
            raise ValueError(f"invalid new-file patch {name}")
        (destination / name).write_text("\n".join(line[1:] for line in body) + "\n", encoding="utf-8")
        names.add(name)
    assert len(names) == 3, names


def verify_manifest():
    expected = set()
    for line in (HERE / "SHA256SUMS").read_text(encoding="utf-8").splitlines():
        digest, name = line.split("  ", 1)
        path = (HERE / name).resolve()
        assert path.is_relative_to(HERE) and path.is_file(), name
        assert name not in expected, name
        assert hashlib.sha256(path.read_bytes()).hexdigest() == digest, name
        expected.add(name)
    actual = {p.relative_to(HERE).as_posix() for p in HERE.rglob("*") if p.is_file() and p.name != "SHA256SUMS"}
    assert expected == actual, (expected - actual, actual - expected)
    print(f"PASS evidence manifest entries={len(expected)}", flush=True)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--run-source-tests", action="store_true")
    args = parser.parse_args()
    verify_manifest()
    blob = git("rev-parse", "HEAD:code/composition.h").decode().strip()
    assert blob == "fd0603c17136c067e89c71e56aef310f2f7e63f4", blob
    normalized = (ROOT / "code/composition.h").read_bytes().replace(b"\r\n", b"\n")
    assert hashlib.sha256(normalized).hexdigest() == "a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418"
    changed = git("diff", "--name-only", BASE).decode().splitlines()
    changed += git("ls-files", "--others", "--exclude-standard").decode().splitlines()
    for path in changed:
        assert path.startswith(("z6/f5/codex/", ".github/workflows/z6-f5.yml")), path
        assert not path.endswith("_LOCKED.md"), path
    for file in HERE.rglob("*"):
        if file.is_file():
            assert file.stat().st_size < 5_000_000, str(file)
    # Let git parse the complete patch, including the incremental existing files.
    subprocess.run(["git", "apply", "--numstat", str(PATCH)], cwd=ROOT, check=True)
    print(f"PASS frozen surface; blob={blob}; patch syntax; bounded delivery scope", flush=True)
    work = ROOT / "m0" / "out"
    work.mkdir(parents=True, exist_ok=True)
    output = Path(tempfile.mkdtemp(prefix="fm-f5-tests-", dir=work))
    # Keep generated source/build output for diagnosis; m0/out is already ignored.
    extract_added_sources(output)
    if args.run_source_tests:
        if sys.platform != "win32":
            raise RuntimeError("actual Fake named-event tests require Windows")
        # Test only dependency-free host source. Do not build a subset of the FluentHub application.
        (output / "WindowReducedMotionSource.cs").unlink()
        (output / "SourceTests.cs").write_bytes((HERE / "SourceTests.cs").read_bytes())
        (output / "SourceTests.csproj").write_text(
            '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType>'
            '<TargetFramework>net8.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings>'
            '<Nullable>enable</Nullable><TreatWarningsAsErrors>true</TreatWarningsAsErrors>'
            '</PropertyGroup></Project>', encoding="utf-8")
        subprocess.run(["dotnet", "run", "--project", str(output / "SourceTests.csproj"), "-c", "Release"], check=True)
    print("PASS (dev-only); host build, GUI and real OS broadcasts are separate gates")


if __name__ == "__main__":
    main()
