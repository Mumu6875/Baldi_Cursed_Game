#!/usr/bin/env python3
"""Verify the generated Legacy/Round-look Android Adaptive Icon."""

from pathlib import Path
import re
import tempfile

from PIL import Image, ImageChops

from generate_android_adaptive_icon import (
    ADAPTIVE_SIZE,
    ART_CANVAS_SIZE,
    DEFAULT_BACKGROUND,
    DEFAULT_FOREGROUND,
    DEFAULT_SOURCE,
    VISIBLE_MASK_SIZE,
    generate_adaptive_icon,
    rgba_images_equal,
)


ROOT = Path(__file__).resolve().parents[1]
PLAYER_SETTINGS = ROOT / "ProjectSettings" / "ProjectSettings.asset"
BACKGROUND_GUID = "2c8d8a7388d74b1e85b6ff72d64d8e27"
FOREGROUND_GUID = "b2163500ac0d4dc6ac643e7474b7eec7"
EXPECTED_SIZES = [432, 324, 216, 162, 108, 81]


def fail(message: str) -> None:
    raise SystemExit(f"FAIL: {message}")


if not DEFAULT_SOURCE.is_file():
    fail("original 64x64 Legacy/Round icon source is missing")

source = Image.open(DEFAULT_SOURCE).convert("RGBA")
background = Image.open(DEFAULT_BACKGROUND).convert("RGBA")
foreground = Image.open(DEFAULT_FOREGROUND).convert("RGBA")

if source.size != (64, 64):
    fail(f"Legacy/Round source must be 64x64, found {source.size}")
if background.size != (ADAPTIVE_SIZE, ADAPTIVE_SIZE):
    fail(f"background must be {ADAPTIVE_SIZE}x{ADAPTIVE_SIZE}")
if foreground.size != (ADAPTIVE_SIZE, ADAPTIVE_SIZE):
    fail(f"foreground must be {ADAPTIVE_SIZE}x{ADAPTIVE_SIZE}")
if background.getextrema() != ((255, 255),) * 4:
    fail("background must be fully opaque white")

inset = (ADAPTIVE_SIZE - ART_CANVAS_SIZE) // 2
art_box = (inset, inset, inset + ART_CANVAS_SIZE, inset + ART_CANVAS_SIZE)
expected_art = source.resize((ART_CANVAS_SIZE, ART_CANVAS_SIZE), Image.Resampling.NEAREST)
if not rgba_images_equal(foreground.crop(art_box), expected_art):
    fail("foreground does not contain the original Legacy/Round icon in its art canvas")

outside = Image.new("L", foreground.size, 255)
outside.paste(0, art_box)
if ImageChops.multiply(foreground.getchannel("A"), outside).getbbox() is not None:
    fail("foreground artwork extends outside the generated art canvas")

# Match Legacy/Round scale intentionally: only the lower shirt edge may be
# clipped by the nominal 72dp circular launcher mask. The 66dp guaranteed safe
# circle is smaller, so previews are approximations rather than device proof.
center = ADAPTIVE_SIZE / 2
visible_radius = VISIBLE_MASK_SIZE / 2
clipped_pixels = []
alpha = foreground.getchannel("A")
for y in range(ADAPTIVE_SIZE):
    for x in range(ADAPTIVE_SIZE):
        if alpha.getpixel((x, y)) and (
            (x + 0.5 - center) ** 2 + (y + 0.5 - center) ** 2
            > visible_radius**2
        ):
            clipped_pixels.append((x, y))
if len(clipped_pixels) > 400 or any(y < 330 for _, y in clipped_pixels):
    fail("nominal circular mask would clip more than the intended lower shirt edge")

with tempfile.TemporaryDirectory() as temporary_directory:
    generated_background = Path(temporary_directory) / "background.png"
    generated_foreground = Path(temporary_directory) / "foreground.png"
    generate_adaptive_icon(
        source_path=DEFAULT_SOURCE,
        background_path=generated_background,
        foreground_path=generated_foreground,
    )
    if not rgba_images_equal(Image.open(generated_background), background):
        fail("committed background differs from a fresh Python generation")
    if not rgba_images_equal(Image.open(generated_foreground), foreground):
        fail("committed foreground differs from a fresh Python generation")

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

for index, (background_guid, foreground_guid, width, height, kind) in enumerate(entries):
    expected_size = EXPECTED_SIZES[index]
    if (int(width), int(height), int(kind)) != (expected_size, expected_size, 2):
        fail(f"unexpected Android icon entry at index {index}: {width}x{height}, kind {kind}")
    if (background_guid, foreground_guid) != (BACKGROUND_GUID, FOREGROUND_GUID):
        fail(f"{expected_size}px must use background first and foreground second")

print("PASS: Python-generated Adaptive Icon matches the original Legacy/Round artwork")
