"""Fail closed on missing or incorrect native package inputs; never execute them."""
from pathlib import Path
import argparse
import hashlib
import json
import struct


def inspect_native(path, rid):
    if rid != "win-x64":
        raise ValueError("Only win-x64 is supported")
    if not path.is_file():
        raise ValueError("Native DLL is missing: " + str(path))
    data = path.read_bytes()
    if len(data) < 64 or data[:2] != b"MZ":
        raise ValueError("Native asset is not PE")
    offset = struct.unpack_from("<I", data, 0x3C)[0]
    if offset + 24 > len(data) or data[offset:offset + 4] != b"PE\0\0":
        raise ValueError("Invalid PE header")
    machine = struct.unpack_from("<H", data, offset + 4)[0]
    flags = struct.unpack_from("<H", data, offset + 22)[0]
    if machine != 0x8664 or not flags & 0x2000:
        raise ValueError("Expected an AMD64 DLL")
    return {
        "name": path.name,
        "rid": rid,
        "machine": "0x8664",
        "bytes": len(data),
        "sha256": hashlib.sha256(data).hexdigest(),
    }


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("file", type=Path)
    parser.add_argument("--rid", required=True)
    args = parser.parse_args()
    print(json.dumps(inspect_native(args.file, args.rid)))
