# Baldi Cursed Classroom — Unity Horror Mod

This is a real Unity 2018.3.9f1 project based on **Baldi's Basics Open Source Tool v5** (Classic 1.4.3 decompile). It modifies the original Baldi project rather than replacing it with a new imitation game.

Source page: https://pspleaffox.itch.io/baldi-open-source-classic-party

## Included

- Cursed Baldi runtime skin applied to the original Baldi sprite object
- Cursed You Can Think Pad full-screen skin behind the original functional controls
- Dark red fog, low ambient lighting, camera light flicker and proximity danger pulse
- More aggressive original Baldi navigation values
- Cursed Baldi game-over jumpscare
- Runtime touch joystick, swipe-to-look zone, Run, Grab, Use and Pause controls
- Android orientation, package ID, ARMv7 + ARM64 and minimum SDK setup

## Open and build an APK

1. Install Unity Hub and Unity **2018.3.9f1** with **Android Build Support**, Android SDK/NDK and OpenJDK.
2. Add this folder as an existing project in Unity Hub.
3. Wait for the first import to finish. The Android setup script runs automatically.
4. Open `Assets/Scene/MainMenu.unity` for testing.
5. Use `Cursed Baldi > Build Android APK` for the included one-click build command.

You can also use Unity's normal `File > Build Settings > Android > Switch Platform > Build` flow. All required scenes are already listed.

If Unity offers to upgrade the project, make a backup first. The source package recommends 2018.3.9f1; large upgrades can alter TextMesh Pro layout and old shaders.

## Non-commercial restriction and credits

The downloaded base identifies itself as a fan-made decompile. Its page states that Baldi, the characters, code, assets and music belong to mystman12/Basically Games and that the decompile may not be used commercially, including ads or in-app purchases. Credit **Mystman12 / Basically Games** in any distributed build.

Generated mod artwork is stored in `Assets/Resources/CursedMod/`. Mod runtime code is stored in `Assets/CursedMod/`.
