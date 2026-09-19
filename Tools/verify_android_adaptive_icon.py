#!/usr/bin/env python3
"""Verify Android uses a flattened Legacy-look Adaptive Icon composition."""

from pathlib import Path
import re
import struct
import zlib


ROOT = Path(__file__).resolve().parents[1]
PLAYER_SETTINGS = ROOT / "ProjectSettings" / "ProjectSettings.asset"
BACKGROUND = (
    ROOT
    / "Assets"
    / "Texture2D"
    / "Miscellaneous"
    / "AdaptiveIcon"
    / "AdaptiveIconBackground.png"
)
FOREGROUND = BACKGROUND.with_name("AdaptiveIconForeground.png")

BACKGROUND_GUID = "2c8d8a7388d74b1e85b6ff72d64d8e27"
FOREGROUND_GUID = "b2163500ac0d4dc6ac643e7474b7eec7"
EXPECTED_SIZES = [432, 324, 216, 162, 108, 81]


def fail(message: str) -> None:
    raise SystemExit(f"FAIL: {message}")


def read_rgba_png(path: Path) -> tuple[int, int, list[tuple[int, int, int, int]]]:
    if not path.is_file():
        fail(f"missing PNG: {path.relative_to(ROOT)}")

    data = path.read_bytes()
    if data[:8] != bytes.fromhex("89504e470d0a1a0a"):
        fail(f"not a PNG: {path.relative_to(ROOT)}")

    width = height = None
    compressed = bytearray()
    offset = 8
    while offset < len(data):
        length = struct.unpack(">I", data[offset : offset + 4])[0]
        chunk_type = data[offset + 4 : offset + 8]
        chunk_data = data[offset + 8 : offset + 8 + length]
        offset += 12 + length
        if chunk_type == b"IHDR":
            width, height, depth, color_type, compression, filtering, interlace = (
                struct.unpack(">IIBBBBB", chunk_data)
            )
            if (depth, color_type, compression, filtering, interlace) != (8, 6, 0, 0, 0):
                fail(f"{path.name} must be a non-interlaced 8-bit RGBA PNG")
        elif chunk_type == b"IDAT":
            compressed.extend(chunk_data)
        elif chunk_type == b"IEND":
            break

    if width is None or height is None:
        fail(f"{path.name} has no IHDR chunk")

    raw = zlib.decompress(bytes(compressed))
    stride = width * 4
    expected_length = height * (stride + 1)
    if len(raw) != expected_length:
        fail(f"{path.name} has an unexpected decoded size")

    rows: list[bytearray] = []
    cursor = 0
    for _ in range(height):
        filter_type = raw[cursor]
        cursor += 1
        row = bytearray(raw[cursor : cursor + stride])
        cursor += stride
        previous = rows[-1] if rows else bytearray(stride)
        for index in range(stride):
            left = row[index - 4] if index >= 4 else 0
            above = previous[index]
            upper_left = previous[index - 4] if index >= 4 else 0
            if filter_type == 1:
                row[index] = (row[index] + left) & 0xFF
            elif filter_type == 2:
                row[index] = (row[index] + above) & 0xFF
            elif filter_type == 3:
                row[index] = (row[index] + ((left + above) // 2)) & 0xFF
            elif filter_type == 4:
                predictor = left + above - upper_left
                pa = abs(predictor - left)
                pb = abs(predictor - above)
                pc = abs(predictor - upper_left)
                nearest = left if pa <= pb and pa <= pc else above if pb <= pc else upper_left
                row[index] = (row[index] + nearest) & 0xFF
            elif filter_type != 0:
                fail(f"{path.name} uses unsupported PNG filter {filter_type}")
        rows.append(row)

    pixels = [
        tuple(row[index : index + 4])
        for row in rows
        for index in range(0, stride, 4)
    ]
    return width, height, pixels


background_width, background_height, background_pixels = read_rgba_png(BACKGROUND)
if (background_width, background_height) != (432, 432):
    fail(f"background must be 432x432, found {background_width}x{background_height}")
if any(alpha != 255 for _, _, _, alpha in background_pixels):
    fail("background must be fully opaque")
corners = [
    background_pixels[0],
    background_pixels[background_width - 1],
    background_pixels[-background_width],
    background_pixels[-1],
]
if any(pixel != (0, 0, 0, 255) for pixel in corners):
    fail("background corners must be opaque black")
if not any((red, green, blue) != (0, 0, 0) for red, green, blue, _ in background_pixels):
    fail("background must contain the flattened Baldi artwork, not only black")

foreground_width, foreground_height, foreground_pixels = read_rgba_png(FOREGROUND)
if (foreground_width, foreground_height) != (432, 432):
    fail(f"foreground must be 432x432, found {foreground_width}x{foreground_height}")
if any(alpha != 0 for _, _, _, alpha in foreground_pixels):
    fail("foreground must be fully transparent")

settings = PLAYER_SETTINGS.read_text(encoding="utf-8-sig")
try:
    android_icons = settings.split("  - m_BuildTarget: Android\n", 1)[1].split(
        "  - m_BuildTarget: iPhone\n", 1
    )[0]
except IndexError:
    fail("Android platform icon section is missing")

all_kinds = re.findall(r"      m_Kind: (\d+)\n", android_icons)
if all_kinds != ["2"] * len(EXPECTED_SIZES):
    fail(f"Android must contain only six Adaptive Icon entries, found kinds {all_kinds}")

entries = re.findall(
    r"    - m_Textures:\n"
    r"      - \{fileID: 2800000, guid: ([0-9a-f]{32}), type: 3\}\n"
    r"      - \{fileID: 2800000, guid: ([0-9a-f]{32}), type: 3\}\n"
    r"      m_Width: (\d+)\n"
    r"      m_Height: (\d+)\n"
    r"      m_Kind: (\d+)\n",
    android_icons,
)

if len(entries) != len(EXPECTED_SIZES):
    fail(f"expected 6 Adaptive Icon entries, found {len(entries)}")

for index, (first_guid, second_guid, width, height, kind) in enumerate(entries):
    expected_size = EXPECTED_SIZES[index]
    if (int(width), int(height), int(kind)) != (expected_size, expected_size, 2):
        fail(f"unexpected Android icon entry at index {index}: {width}x{height}, kind {kind}")
    if (first_guid, second_guid) != (BACKGROUND_GUID, FOREGROUND_GUID):
        fail(
            f"{expected_size}px must use the flattened background first and "
            "dedicated transparent foreground second"
        )

print("PASS: Android Adaptive Icon uses one opaque Legacy-look background layer")
