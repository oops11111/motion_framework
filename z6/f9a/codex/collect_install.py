"""Archive this one installer attempt; preserve originals and publish bounded sanitized evidence."""
from pathlib import Path
import hashlib
from datetime import datetime
import json
import os
import re
import shutil

ROOT = Path(__file__).resolve().parents[3]
ATTEMPT = ROOT / "m0/out/f9a-install-20261121T083326Z"
DEST = ROOT / "z6/f9a/codex/raw/install-20261121T083326Z"

def digest(data):
    return hashlib.sha256(data).hexdigest()

def sanitize(text):
    replacements = [(str(ROOT), "%WORKSPACE%"), (os.environ["TEMP"], "%TEMP%"), (os.environ["USERPROFILE"], "%USERPROFILE%"), (os.environ["ProgramFiles(x86)"], "%ProgramFiles(x86)%"), (os.environ["ProgramFiles"], "%ProgramFiles%"), (os.environ["ProgramData"], "%ProgramData%"), (os.environ["windir"], "%WINDIR%")]
    for path, token in replacements:
        for value in [path.replace("\\", "\\\\"), path, path.replace("\\", "/")]:
            text = text.replace(value, token)
    return re.sub(r"(?<![A-Za-z0-9])([A-Za-z]):[\\/]+", lambda m: "%DRIVE_" + m[1].upper() + "%/", text).replace("\r\n", "\n")

def decode(data):
    return data.decode("utf-16" if data.startswith((b"\xff\xfe", b"\xfe\xff")) else "utf-8-sig", errors="replace")

def main():
    DEST.mkdir(parents=True, exist_ok=False)
    originals = ATTEMPT / "original-dd-logs"
    originals.mkdir(exist_ok=False)
    pre = json.loads((ATTEMPT / "preflight.json").read_text(encoding="utf-8-sig"))
    old = {r["Name"] for r in pre["existing_dd_logs"]}
    inventory = []
    excerpts = []
    temp = Path(os.environ["TEMP"])
    logs = list(temp.glob("dd_*")) + list((temp / "windowssdk").glob("dd_setup_20261121163410_140*"))
    for log in sorted(logs):
        if not log.is_file() or log.name in old:
            continue
        # Only this attempt's date/time prefix; avoid unrelated pre-existing/background logs.
        stamp = re.search(r"20261121[0-9]{6}", log.name)
        if not stamp or not (datetime(2026, 11, 21, 16, 33, 26) <= datetime.strptime(stamp[0], "%Y%m%d%H%M%S") <= datetime.now()):
            continue
        data = log.read_bytes()
        relative = log.relative_to(temp)
        copy = originals / relative
        copy.parent.mkdir(parents=True, exist_ok=True)
        copy.write_bytes(data)
        row = {"name": relative.as_posix(), "bytes": len(data), "original_sha256": digest(data), "original_location": "%WORKSPACE%/m0/out/f9a-install-20261121T083326Z/original-dd-logs/" + relative.as_posix(), "last_write_utc_epoch": log.stat().st_mtime}
        text = sanitize(decode(data))
        primary = bool(re.fullmatch(r"dd_(?:installer(?:_elevated)?|setup)_\d{14}(?:_errors)?\.log", log.name))
        if primary:
            if len(text.encode("utf-8")) >= 4_500_000:
                text = text[:100_000] + "\n[BOUNDED EXCERPT; full original retained at indexed location]\n" + text[-100_000:]
                row["published"] = "bounded head/tail excerpt"
            else:
                row["published"] = "complete sanitized text"
            dest = DEST / log.name
            dest.write_text(text, encoding="utf-8", newline="\n")
            row["published_sha256"] = digest(dest.read_bytes())
        else:
            row["published"] = "inventory plus last3lines excerpt; complete original retained ignored"
            excerpts.append("## " + relative.as_posix() + "\n" + "\n".join(text.splitlines()[-3:]) + "\n")
        inventory.append(row)
    (DEST / "log-index.json").write_text(json.dumps(inventory, indent=2) + "\n", encoding="utf-8", newline="\n")
    (DEST / "package-log-excerpts.txt").write_text("Bounded last3lines of package logs; original SHA and complete ignored copy in log-index.json.\n\n" + "\n".join(excerpts), encoding="utf-8", newline="\n")
    for f in ATTEMPT.iterdir():
        if f.is_file() and f.suffix in (".json", ".jsonl"):
            (DEST / f.name).write_text(sanitize(f.read_text(encoding="utf-8-sig")), encoding="utf-8", newline="\n")
    print(json.dumps({"logs": len(inventory), "published_directory": DEST.relative_to(ROOT).as_posix(), "originals": originals.relative_to(ROOT).as_posix()}))

if __name__ == "__main__":
    main()
