# Adaptive Icon Legacy-Look Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the fragile two-visible-layer Adaptive Icon composition with one opaque Legacy-look background and a transparent foreground.

**Architecture:** Keep Unity's six Adaptive Icon records, but point them to two purpose-specific assets. The visible artwork is flattened into the background so Android launchers cannot independently inset Baldi; the required foreground remains transparent.

**Tech Stack:** Unity 6 project YAML, PNG assets, Python 3 regression verifier, ImageMagick diagnostics.

**Spec:** `docs/superpowers/specs/2026-09-19-adaptive-icon-legacy-look.md`

## Global Constraints

- Android icons remain Adaptive-only.
- Do not change the package identifier, Legacy/Round configuration, or unrelated assets.
- Preserve `GameIcon.png` as the source artwork.
- Do not claim device-perfect equivalence beyond Android's device-controlled outer mask.

## Review Focus

- A launcher must receive an opaque background rather than the current pure-black layer.
- A launcher must not independently shrink visible foreground artwork.
- Every configured density must use the same background/foreground pair.
- The new foreground Unity metadata must preserve transparency and nearest-neighbor filtering.
- No Legacy or Round Android icon records may appear.

---

### Task 1: Add a regression verifier for the approved composition

**Files:**
- Modify: `Tools/verify_android_adaptive_icon.py`

**Interfaces:**
- Consumes: PNG paths and GUIDs from the approved specification.
- Produces: a zero exit status only for the approved opaque-background/transparent-foreground composition.

- [ ] Extend the verifier to decode non-interlaced PNG pixels with Python's standard library.
- [ ] Assert the background is 432x432 RGBA, opaque, black in all four corners, and contains visible non-black pixels.
- [ ] Assert the dedicated foreground is 432x432 RGBA and fully transparent.
- [ ] Assert all six YAML entries use background first and the new foreground second.
- [ ] Run `python3 Tools/verify_android_adaptive_icon.py` and confirm it fails because the new composition does not exist.

### Task 2: Implement the single-visible-layer Adaptive Icon

**Files:**
- Modify: `Assets/Texture2D/Miscellaneous/AdaptiveIcon/AdaptiveIconBackground.png`
- Create: `Assets/Texture2D/Miscellaneous/AdaptiveIcon/AdaptiveIconForeground.png`
- Create: `Assets/Texture2D/Miscellaneous/AdaptiveIcon/AdaptiveIconForeground.png.meta`
- Modify: `ProjectSettings/ProjectSettings.asset`

**Interfaces:**
- Consumes: `Assets/Texture2D/Miscellaneous/GameIcon.png`.
- Produces: background GUID `2c8d8a7388d74b1e85b6ff72d64d8e27` and a new dedicated transparent foreground GUID.

- [ ] Composite `GameIcon.png` over opaque black without scaling or filtering.
- [ ] Create a fully transparent 432x432 foreground.
- [ ] Add Unity metadata matching the existing Android icon texture settings with mipmaps disabled.
- [ ] Replace the foreground GUID in all six Adaptive Icon records.
- [ ] Run `python3 Tools/verify_android_adaptive_icon.py` and confirm it passes.

### Task 3: Verify and publish

**Files:**
- Verify all changed files and all `Tools/verify_*.py` scripts.

**Interfaces:**
- Consumes: completed icon assets and configuration.
- Produces: a reviewed commit on GitHub `main`.

- [ ] Render diagnostic masked previews and inspect the flattened artwork.
- [ ] Run every `Tools/verify_*.py` script and `git diff --check`.
- [ ] Request an independent review of the complete diff.
- [ ] Resolve Critical or Important findings with RED-GREEN tests.
- [ ] Commit with a focused message, push without force, and verify the remote commit and files.
