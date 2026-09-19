# Adaptive Icon Legacy-Look Specification

## Goal

Make the Android Adaptive Icon visually match the existing close-up Legacy/Round artwork as closely as Android's device-controlled mask permits.

## Approved design

- Keep Android icons Adaptive-only; do not add Legacy or Round entries.
- Flatten the transparent Baldi artwork over black into one opaque 432x432 Adaptive background.
- Add a dedicated fully transparent 432x432 Adaptive foreground.
- Preserve the existing legacy source artwork, package identifier, and unrelated project settings.
- Bind background first and the dedicated transparent foreground second for all six Android Adaptive Icon sizes.

## Acceptance criteria

- The background is 432x432, fully opaque, contains non-black Baldi pixels, and has black corners.
- The foreground is 432x432 and fully transparent.
- Exactly six Android icon entries exist at 432, 324, 216, 162, 108, and 81 pixels; all are kind 2.
- No Android Legacy or Round icon entries are introduced.
- All project verification scripts pass and the worktree is clean after commit.
