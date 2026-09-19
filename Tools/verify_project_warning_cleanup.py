#!/usr/bin/env python3
"""Verify the project-owned warning fixes that do not require the Unity Editor."""

from pathlib import Path
import re
import sys


ROOT = Path(__file__).resolve().parents[1]
ASSETS = ROOT / "Assets"
SCHOOL_SCENE = ASSETS / "Scene" / "School.unity"
OCCLUSION_ASSET = ASSETS / "Scene" / "School" / "OcclusionCullingData.asset"
INPUT_MANAGER = (
    ASSETS
    / "Scripts"
    / "Core"
    / "UI"
    / "Settings"
    / "ControlMapper"
    / "InputManager.cs"
)


def fail(message: str) -> None:
    print(f"FAIL: {message}")
    raise SystemExit(1)


def verify_object_lookup_api() -> None:
    offenders = []
    for source in ASSETS.rglob("*.cs"):
        if "FindFirstObjectByType" in source.read_text(encoding="utf-8-sig"):
            offenders.append(source.relative_to(ROOT).as_posix())
    if offenders:
        fail("obsolete FindFirstObjectByType remains in " + ", ".join(offenders))
    print("PASS: project scripts use non-obsolete object lookup APIs")


def verify_runtime_dictionary_is_not_serialized() -> None:
    source = INPUT_MANAGER.read_text(encoding="utf-8-sig")
    pattern = re.compile(
        r"\[NonSerialized\]\s*"
        r"public Dictionary<InputAction, InputBinding> KeyboardMapping\s*=",
        re.MULTILINE,
    )
    if not pattern.search(source):
        fail("KeyboardMapping is runtime state but is not marked [NonSerialized]")
    print("PASS: KeyboardMapping is explicitly excluded from Unity serialization")


def verify_unsupported_occlusion_data_is_removed() -> None:
    scene = SCHOOL_SCENE.read_text(encoding="utf-8-sig")
    if not re.search(r"m_OcclusionCullingData:\s*\{fileID:\s*0\}", scene):
        fail("School scene still references baked Umbra occlusion data")
    if OCCLUSION_ASSET.exists() or OCCLUSION_ASSET.with_suffix(".asset.meta").exists():
        fail("unused School OcclusionCullingData asset or metadata still exists")
    print("PASS: unsupported School Umbra occlusion data is removed")


def main() -> None:
    verify_object_lookup_api()
    verify_runtime_dictionary_is_not_serialized()
    verify_unsupported_occlusion_data_is_removed()


if __name__ == "__main__":
    try:
        main()
    except (OSError, UnicodeError) as exc:
        print(f"ERROR: {exc}")
        sys.exit(2)
