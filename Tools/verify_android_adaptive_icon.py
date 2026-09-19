#!/usr/bin/env python3
"""Verify Android uses the opaque background below the transparent Baldi layer."""

from pathlib import Path
import re


ROOT = Path(__file__).resolve().parents[1]
PLAYER_SETTINGS = ROOT / "ProjectSettings" / "ProjectSettings.asset"

BACKGROUND_GUID = "2c8d8a7388d74b1e85b6ff72d64d8e27"
FOREGROUND_GUID = "c95357becae9e284d9b5519cc4f6bf0f"
EXPECTED_SIZES = [432, 324, 216, 162, 108, 81]


def fail(message: str) -> None:
    raise SystemExit(f"FAIL: {message}")


settings = PLAYER_SETTINGS.read_text(encoding="utf-8-sig")
try:
    android_icons = settings.split("  - m_BuildTarget: Android\n", 1)[1].split(
        "  - m_BuildTarget: iPhone\n", 1
    )[0]
except IndexError:
    fail("Android platform icon section is missing")

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
            f"{expected_size}px layers are reversed: Unity expects background first "
            "and foreground second"
        )

print("PASS: all Android Adaptive Icon sizes use background then Baldi foreground")
