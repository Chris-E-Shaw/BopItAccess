"""Inspect installed Bop It UnityFS localization bundles without third-party tools.

The asset bundles contain Unity serialized string tables. This diagnostic
extracts readable text from their decompressed payloads; it does not alter the
game or make network requests.
"""

from __future__ import annotations

import argparse
import json
import re
import struct
from pathlib import Path


def lz4_block(data: bytes, expected_size: int) -> bytes:
    out = bytearray()
    offset = 0
    while offset < len(data):
        token = data[offset]
        offset += 1
        literal_length = token >> 4
        if literal_length == 15:
            while True:
                extra = data[offset]
                offset += 1
                literal_length += extra
                if extra != 255:
                    break
        out.extend(data[offset : offset + literal_length])
        offset += literal_length
        if offset == len(data):
            break
        distance = int.from_bytes(data[offset : offset + 2], "little")
        offset += 2
        if not distance or distance > len(out):
            raise ValueError("Invalid LZ4 match distance")
        match_length = token & 15
        if match_length == 15:
            while True:
                extra = data[offset]
                offset += 1
                match_length += extra
                if extra != 255:
                    break
        match_length += 4
        for _ in range(match_length):
            out.append(out[-distance])
    if len(out) != expected_size:
        raise ValueError(f"LZ4 size mismatch: {len(out)} != {expected_size}")
    return bytes(out)


def cstring(data: bytes, offset: int) -> tuple[str, int]:
    end = data.index(0, offset)
    return data[offset:end].decode("utf-8"), end + 1


def read_bundle(path: Path) -> bytes:
    data = path.read_bytes()
    signature, offset = cstring(data, 0)
    if signature != "UnityFS":
        raise ValueError("Expected UnityFS")
    version = struct.unpack_from(">I", data, offset)[0]
    offset += 4
    _, offset = cstring(data, offset)
    _, offset = cstring(data, offset)
    total_size, compressed_info_size, info_size, flags = struct.unpack_from(">QIII", data, offset)
    offset += 20
    if version >= 7:
        offset = (offset + 15) & ~15
    if total_size != len(data):
        raise ValueError("Bundle size mismatch")
    if flags & 0x80:
        info_compressed = data[-compressed_info_size:]
        block_offset = offset
    else:
        info_compressed = data[offset : offset + compressed_info_size]
        block_offset = offset + compressed_info_size
        if version >= 7:
            block_offset = (block_offset + 15) & ~15
    compression = flags & 0x3F
    if compression in (2, 3):
        info = lz4_block(info_compressed, info_size)
    elif compression == 0:
        info = info_compressed
    else:
        raise ValueError(f"Unsupported metadata compression {compression}")
    pointer = 16  # hash
    count = struct.unpack_from(">I", info, pointer)[0]
    pointer += 4
    result = bytearray()
    for _ in range(count):
        raw_size, stored_size, block_flags = struct.unpack_from(">IIH", info, pointer)
        pointer += 10
        stored = data[block_offset : block_offset + stored_size]
        block_offset += stored_size
        block_compression = block_flags & 0x3F
        if block_compression in (2, 3):
            result.extend(lz4_block(stored, raw_size))
        elif block_compression == 0:
            result.extend(stored)
        else:
            raise ValueError(f"Unsupported data compression {block_compression}")
    return bytes(result)


def strings(blob: bytes) -> list[str]:
    found = []
    for match in re.finditer(rb"[\x20-\x7e\xc2-\xf4][\x20-\x7e\x80-\xbf\xc2-\xf4]{3,}", blob):
        try:
            item = match.group().decode("utf-8").strip()
        except UnicodeDecodeError:
            continue
        if item and any(char.isalpha() for char in item):
            found.append(item)
    return list(dict.fromkeys(found))


def string_table(blob: bytes, table_name: str, code: str) -> dict[int, str]:
    """Read Unity Localization StringTable entries by shared numeric ID."""
    name = table_name.encode("utf-8")
    marker = len(name).to_bytes(4, "little") + name
    offset = blob.find(marker)
    if offset < 0:
        raise ValueError(f"Missing string table {table_name}")
    pointer = offset + len(marker)
    pointer = (pointer + 3) & ~3
    length = int.from_bytes(blob[pointer : pointer + 4], "little")
    pointer += 4
    locale = blob[pointer : pointer + length].decode("utf-8")
    if locale.lower() != code.lower():
        raise ValueError(f"Unexpected locale {locale} for {table_name}")
    pointer += length
    pointer = (pointer + 3) & ~3
    pointer += 12  # PPtr to shared table data
    pointer += 4  # MetadataCollection count (empty in game string tables)
    count = int.from_bytes(blob[pointer : pointer + 4], "little")
    pointer += 4
    if count > 10000:
        raise ValueError(f"Unexpected table size {count}")
    entries = {}
    for index in range(count):
        entry_id = int.from_bytes(blob[pointer : pointer + 8], "little")
        pointer += 8
        length = int.from_bytes(blob[pointer : pointer + 4], "little")
        pointer += 4
        try:
            value = blob[pointer : pointer + length].decode("utf-8")
        except UnicodeDecodeError as error:
            raise ValueError(f"Invalid table entry {index} at {pointer}, length {length}") from error
        pointer += length
        pointer = (pointer + 3) & ~3
        pointer += 4  # Entry MetadataCollection count
        entries[entry_id] = value
    return entries


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("bundle", type=Path)
    parser.add_argument("--table", metavar="LOCALE")
    args = parser.parse_args()
    blob = read_bundle(args.bundle)
    if args.table:
        print(json.dumps(string_table(blob, f"General_{args.table}", args.table), ensure_ascii=False, indent=2))
    else:
        for item in strings(blob):
            print(item)


if __name__ == "__main__":
    main()
