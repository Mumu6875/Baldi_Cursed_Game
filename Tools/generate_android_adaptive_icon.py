#!/usr/bin/env python3
"""Generate Android Adaptive Icon layers from the original Legacy/Round icon."""

from argparse import ArgumentParser
import hashlib
from pathlib import Path
from typing import Callable

from PIL import Image, ImageDraw


ROOT = Path(__file__).resolve().parents[1]
ADAPTIVE_SIZE = 432
ART_CANVAS_SIZE = 264
VISIBLE_MASK_SIZE = 288  # 72 / 108 of the layer canvas, enlarged by launchers.
LEGACY_RGBA_SHA256 = "6c67a95420d4bb1c0f05787bad4bfa4802097743f6779d4913d7ecdf0ae30f3b"
DEFAULT_SOURCE = ROOT / "IconSources" / "BaldiLegacyRoundGameIcon.png"
DEFAULT_BACKGROUND = (
    ROOT
    / "Assets"
    / "Texture2D"
    / "Miscellaneous"
    / "AdaptiveIcon"
    / "AdaptiveIconBackground.png"
)
DEFAULT_FOREGROUND = DEFAULT_BACKGROUND.with_name("AdaptiveIconForeground.png")


def rgba_images_equal(first: Image.Image, second: Image.Image) -> bool:
    """Compare every RGBA channel; ImageChops.getbbox() can ignore RGB-only changes."""
    first_rgba = first.convert("RGBA")
    second_rgba = second.convert("RGBA")
    return first_rgba.size == second_rgba.size and first_rgba.tobytes() == second_rgba.tobytes()


def _save(image: Image.Image, path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    image.save(path, format="PNG", optimize=True)


def _circle_mask() -> Image.Image:
    mask = Image.new("L", (ADAPTIVE_SIZE, ADAPTIVE_SIZE), 0)
    ImageDraw.Draw(mask).ellipse((0, 0, ADAPTIVE_SIZE - 1, ADAPTIVE_SIZE - 1), fill=255)
    return mask


def _rounded_square_mask() -> Image.Image:
    mask = Image.new("L", (ADAPTIVE_SIZE, ADAPTIVE_SIZE), 0)
    ImageDraw.Draw(mask).rounded_rectangle(
        (0, 0, ADAPTIVE_SIZE - 1, ADAPTIVE_SIZE - 1),
        radius=ADAPTIVE_SIZE // 5,
        fill=255,
    )
    return mask


def _squircle_mask() -> Image.Image:
    mask = Image.new("L", (ADAPTIVE_SIZE, ADAPTIVE_SIZE), 0)
    pixels = mask.load()
    center = (ADAPTIVE_SIZE - 1) / 2
    radius = center
    for y in range(ADAPTIVE_SIZE):
        normalized_y = abs((y - center) / radius)
        for x in range(ADAPTIVE_SIZE):
            normalized_x = abs((x - center) / radius)
            if normalized_x**4 + normalized_y**4 <= 1:
                pixels[x, y] = 255
    return mask


def _write_previews(
    background: Image.Image, foreground: Image.Image, preview_directory: Path
) -> None:
    composite = Image.alpha_composite(background, foreground)
    viewport_inset = (ADAPTIVE_SIZE - VISIBLE_MASK_SIZE) // 2
    viewport = composite.crop(
        (
            viewport_inset,
            viewport_inset,
            viewport_inset + VISIBLE_MASK_SIZE,
            viewport_inset + VISIBLE_MASK_SIZE,
        )
    ).resize((ADAPTIVE_SIZE, ADAPTIVE_SIZE), Image.Resampling.NEAREST)
    masks: dict[str, Callable[[], Image.Image]] = {
        "circle.png": _circle_mask,
        "rounded_square.png": _rounded_square_mask,
        "squircle.png": _squircle_mask,
    }
    for name, create_mask in masks.items():
        preview = viewport.copy()
        preview.putalpha(create_mask())
        _save(preview, preview_directory / name)


def generate_adaptive_icon(
    source_path: Path,
    background_path: Path,
    foreground_path: Path,
    preview_directory: Path | None = None,
) -> None:
    source = Image.open(source_path).convert("RGBA")
    if source.size != (64, 64):
        raise ValueError(f"Legacy/Round source must be 64x64, found {source.size}")
    if hashlib.sha256(source.tobytes()).hexdigest() != LEGACY_RGBA_SHA256:
        raise ValueError("source is not the original Legacy/Round icon")

    background = Image.new("RGBA", (ADAPTIVE_SIZE, ADAPTIVE_SIZE), (255, 255, 255, 255))
    foreground = Image.new("RGBA", (ADAPTIVE_SIZE, ADAPTIVE_SIZE), (0, 0, 0, 0))
    art = source.resize((ART_CANVAS_SIZE, ART_CANVAS_SIZE), Image.Resampling.NEAREST)
    inset = (ADAPTIVE_SIZE - ART_CANVAS_SIZE) // 2
    foreground.alpha_composite(art, (inset, inset))

    _save(background, background_path)
    _save(foreground, foreground_path)
    if preview_directory is not None:
        _write_previews(background, foreground, preview_directory)


def main() -> None:
    parser = ArgumentParser(description=__doc__)
    parser.add_argument("--source", type=Path, default=DEFAULT_SOURCE)
    parser.add_argument("--background", type=Path, default=DEFAULT_BACKGROUND)
    parser.add_argument("--foreground", type=Path, default=DEFAULT_FOREGROUND)
    parser.add_argument("--preview-directory", type=Path)
    arguments = parser.parse_args()
    generate_adaptive_icon(
        source_path=arguments.source,
        background_path=arguments.background,
        foreground_path=arguments.foreground,
        preview_directory=arguments.preview_directory,
    )


if __name__ == "__main__":
    main()
