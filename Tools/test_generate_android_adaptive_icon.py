#!/usr/bin/env python3
"""Tests for the deterministic Legacy/Round to Adaptive Icon generator."""

from pathlib import Path
import sys
import tempfile
import unittest

from PIL import Image, ImageChops


ROOT = Path(__file__).resolve().parents[1]
TOOLS = ROOT / "Tools"
sys.path.insert(0, str(TOOLS))

from generate_android_adaptive_icon import (  # noqa: E402
    ADAPTIVE_SIZE,
    ART_CANVAS_SIZE,
    generate_adaptive_icon,
    rgba_images_equal,
)


SOURCE = ROOT / "IconSources" / "BaldiLegacyRoundGameIcon.png"


class AdaptiveIconGeneratorTests(unittest.TestCase):
    def test_rgba_comparison_detects_rgb_changes_when_alpha_is_unchanged(self):
        original = Image.new("RGBA", (1, 1), (10, 20, 30, 255))
        recolored = Image.new("RGBA", (1, 1), (11, 20, 30, 255))
        self.assertFalse(rgba_images_equal(original, recolored))

    def test_rejects_a_different_64x64_image(self):
        with tempfile.TemporaryDirectory() as temporary_directory:
            output = Path(temporary_directory)
            wrong_source = output / "wrong.png"
            Image.new("RGBA", (64, 64), (255, 0, 0, 255)).save(wrong_source)

            with self.assertRaisesRegex(ValueError, "original Legacy/Round icon"):
                generate_adaptive_icon(
                    source_path=wrong_source,
                    background_path=output / "background.png",
                    foreground_path=output / "foreground.png",
                )

    def test_generates_standard_two_layer_adaptive_icon_from_legacy_source(self):
        self.assertTrue(SOURCE.is_file(), "the original 64x64 Legacy/Round icon is required")
        source = Image.open(SOURCE).convert("RGBA")
        self.assertEqual(source.size, (64, 64))

        with tempfile.TemporaryDirectory() as temporary_directory:
            output = Path(temporary_directory)
            background_path = output / "AdaptiveIconBackground.png"
            foreground_path = output / "AdaptiveIconForeground.png"
            preview_directory = output / "previews"

            generate_adaptive_icon(
                source_path=SOURCE,
                background_path=background_path,
                foreground_path=foreground_path,
                preview_directory=preview_directory,
            )

            background = Image.open(background_path).convert("RGBA")
            foreground = Image.open(foreground_path).convert("RGBA")
            self.assertEqual(background.size, (ADAPTIVE_SIZE, ADAPTIVE_SIZE))
            self.assertEqual(foreground.size, (ADAPTIVE_SIZE, ADAPTIVE_SIZE))
            self.assertEqual(background.getextrema(), ((255, 255),) * 4)

            inset = (ADAPTIVE_SIZE - ART_CANVAS_SIZE) // 2
            safe_box = (inset, inset, inset + ART_CANVAS_SIZE, inset + ART_CANVAS_SIZE)
            expected_art = source.resize(
                (ART_CANVAS_SIZE, ART_CANVAS_SIZE), Image.Resampling.NEAREST
            )
            actual_art = foreground.crop(safe_box)
            self.assertTrue(rgba_images_equal(actual_art, expected_art))

            alpha = foreground.getchannel("A")
            outside = Image.new("L", foreground.size, 255)
            outside.paste(0, safe_box)
            self.assertIsNone(ImageChops.multiply(alpha, outside).getbbox())
            self.assertIsNotNone(alpha.getbbox())

            for name in ("circle.png", "rounded_square.png", "squircle.png"):
                preview = Image.open(preview_directory / name).convert("RGBA")
                self.assertEqual(preview.size, (ADAPTIVE_SIZE, ADAPTIVE_SIZE))
                self.assertEqual(preview.getpixel((0, 0))[3], 0)
                self.assertGreater(preview.getpixel((ADAPTIVE_SIZE // 2,) * 2)[3], 0)

                colored_pixels = Image.new("L", preview.size, 0)
                colored_data = colored_pixels.load()
                for y in range(ADAPTIVE_SIZE):
                    for x in range(ADAPTIVE_SIZE):
                        red, green, blue, alpha = preview.getpixel((x, y))
                        if alpha and (red, green, blue) != (255, 255, 255):
                            colored_data[x, y] = 255
                colored_box = colored_pixels.getbbox()
                self.assertIsNotNone(colored_box)
                self.assertGreaterEqual(colored_box[2] - colored_box[0], 300)
                self.assertGreaterEqual(colored_box[3] - colored_box[1], 380)


if __name__ == "__main__":
    unittest.main()
