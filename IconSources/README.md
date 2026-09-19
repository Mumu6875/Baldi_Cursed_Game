# Android Adaptive Icon source

`BaldiLegacyRoundGameIcon.png` is the decoded-pixel-identical 64x64
`Assets/Texture2D/Miscellaneous/GameIcon.png` from commit
`1f5b518ae523e60ae2941b31dda0933e487ad8d8`.

Install the tool dependency and regenerate the Adaptive Icon layers with:

```bash
python3 -m pip install -r Tools/requirements-icon.txt
python3 Tools/generate_android_adaptive_icon.py \
  --preview-directory Temp/AdaptiveIconPreviews
python3 Tools/verify_android_adaptive_icon.py
```

The source canvas is scaled with nearest-neighbor filtering to preserve its
pixel art. It intentionally uses almost all of the nominal 72dp mask viewport
so its launcher size stays close to the old Legacy/Round icon. Extreme OEM
masks or motion can crop a small part of the lower shirt edge; the generated
previews are approximations and a built APK must still be checked on-device.
