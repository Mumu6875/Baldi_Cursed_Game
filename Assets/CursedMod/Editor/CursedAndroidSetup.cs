#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

[InitializeOnLoad]
public static class CursedAndroidSetup
{
    private const string SetupKey = "CursedBaldiAndroidSetup_v1";

    static CursedAndroidSetup()
    {
        EditorApplication.delayCall += ApplyOnce;
    }

    [MenuItem("Cursed Baldi/Apply Android Build Settings")]
    public static void ApplyAndroidSettings()
    {
        PlayerSettings.companyName = "Cursed Classroom Mods";
        PlayerSettings.productName = "Baldi Cursed Classroom";
        PlayerSettings.bundleVersion = "1.12.0";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.cursedclassroom.baldihorror");
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;
        PlayerSettings.Android.bundleVersionCode = 21;
        PlayerSettings.MTRendering = true;
        PlayerSettings.runInBackground = false;
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        EditorPrefs.SetBool(SetupKey, true);
        AssetDatabase.SaveAssets();
        Debug.Log("Cursed Baldi Android settings applied. Switch Build Target to Android, then Build APK.");
    }

    [MenuItem("Cursed Baldi/Build Android APK")]
    public static void BuildAndroidApk()
    {
        ApplyAndroidSettings();
        if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
        {
            EditorUtility.DisplayDialog("Android support missing", "Install Android Build Support, SDK/NDK and OpenJDK for Unity 2018.3.9f1 in Unity Hub.", "OK");
            return;
        }

        string output = EditorUtility.SaveFilePanel("Build Baldi Cursed Classroom APK", "", "BaldiCursedClassroom.apk", "apk");
        if (string.IsNullOrEmpty(output)) return;

        List<string> scenes = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled) scenes.Add(scene.path);
        }

        BuildPlayerOptions options = new BuildPlayerOptions();
        options.scenes = scenes.ToArray();
        options.locationPathName = output;
        options.target = BuildTarget.Android;
        options.options = BuildOptions.None;
        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result == BuildResult.Succeeded)
        {
            EditorUtility.RevealInFinder(output);
            Debug.Log("APK created: " + output);
        }
        else
        {
            Debug.LogError("APK build failed. Open Console for the first compiler or Android SDK error.");
        }
    }

    private static void ApplyOnce()
    {
        if (!EditorPrefs.GetBool(SetupKey, false))
        {
            ApplyAndroidSettings();
        }
    }
}

public sealed class CursedBuildValidation : IPreprocessBuildWithReport
{
    private const string WarningAssetPath = "Assets/Resources/CursedMod/PiracyWarningPhase1.jpg";
    private const string RulerAudioAssetPath = "Assets/Resources/CursedMod/BaldiRulerLoud.ogg";
    private const string HelpMeExitAssetPath = "Assets/Resources/CursedMod/HelpMeExitSign.png";
    private const string Phase2CompletionAssetPath = "Assets/Resources/CursedMod/Phase2Completion.png";
    private const string Phase3PasswordAssetPath = "Assets/Resources/CursedMod/Phase3Password.png";
    private const string Phase4FinalAssetPath = "Assets/Resources/CursedMod/Phase4Final.png";
    private static readonly string[] MobileButtonAssetPaths =
    {
        "Assets/Resources/CursedMod/MobileLookBackButton.png",
        "Assets/Resources/CursedMod/MobileRunButton.png"
    };

    public int callbackOrder { get { return -1000; } }

    public void OnPreprocessBuild(BuildReport report)
    {
        if (!File.Exists(WarningAssetPath))
        {
            throw new BuildFailedException("Required Phase 1 warning image file is missing: " + WarningAssetPath);
        }
        AssetDatabase.ImportAsset(WarningAssetPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        Texture2D warning = AssetDatabase.LoadAssetAtPath<Texture2D>(WarningAssetPath);
        if (warning == null)
        {
            throw new BuildFailedException("Required Phase 1 warning image is missing: " + WarningAssetPath);
        }
        if (warning.width < 1280 || warning.height < 720)
        {
            throw new BuildFailedException("Phase 1 warning image has an invalid resolution: " + warning.width + "x" + warning.height);
        }
        Debug.Log("Verified Phase 1 warning image: " + warning.width + "x" + warning.height);

        if (!File.Exists(HelpMeExitAssetPath))
        {
            throw new BuildFailedException("Required Phase 2 HELP ME exit sign is missing: " + HelpMeExitAssetPath);
        }
        AssetDatabase.ImportAsset(HelpMeExitAssetPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        Texture2D helpMeExit = AssetDatabase.LoadAssetAtPath<Texture2D>(HelpMeExitAssetPath);
        if (helpMeExit == null || helpMeExit.width != 128 || helpMeExit.height != 128)
        {
            throw new BuildFailedException("Phase 2 HELP ME exit sign must be exactly 128x128: " + HelpMeExitAssetPath);
        }
        Debug.Log("Verified Phase 2 HELP ME exit sign: " + helpMeExit.width + "x" + helpMeExit.height);

        if (!File.Exists(Phase2CompletionAssetPath))
        {
            throw new BuildFailedException("Required Phase 2 completion image is missing: " + Phase2CompletionAssetPath);
        }
        AssetDatabase.ImportAsset(Phase2CompletionAssetPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        Texture2D completion = AssetDatabase.LoadAssetAtPath<Texture2D>(Phase2CompletionAssetPath);
        if (completion == null || completion.width != 1672 || completion.height != 941)
        {
            throw new BuildFailedException("Phase 2 completion image must be exactly 1672x941: " + Phase2CompletionAssetPath);
        }
        Debug.Log("Verified Phase 2 completion image: " + completion.width + "x" + completion.height);

        if (!File.Exists(Phase3PasswordAssetPath))
        {
            throw new BuildFailedException("Required Phase 3 password image is missing: " + Phase3PasswordAssetPath);
        }
        AssetDatabase.ImportAsset(Phase3PasswordAssetPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        Texture2D phase3 = AssetDatabase.LoadAssetAtPath<Texture2D>(Phase3PasswordAssetPath);
        if (phase3 == null || phase3.width != 1672 || phase3.height != 941)
        {
            throw new BuildFailedException("Phase 3 password image must be exactly 1672x941: " + Phase3PasswordAssetPath);
        }
        Debug.Log("Verified Phase 3 password image: " + phase3.width + "x" + phase3.height);

        if (!File.Exists(Phase4FinalAssetPath))
        {
            throw new BuildFailedException("Required Phase 4 final image is missing: " + Phase4FinalAssetPath);
        }
        AssetDatabase.ImportAsset(Phase4FinalAssetPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        Texture2D phase4 = AssetDatabase.LoadAssetAtPath<Texture2D>(Phase4FinalAssetPath);
        if (phase4 == null || phase4.width != 1672 || phase4.height != 941)
        {
            throw new BuildFailedException("Phase 4 final image must be exactly 1672x941: " + Phase4FinalAssetPath);
        }
        Debug.Log("Verified Phase 4 final image: " + phase4.width + "x" + phase4.height);

        foreach (string buttonPath in MobileButtonAssetPaths)
        {
            if (!File.Exists(buttonPath))
            {
                throw new BuildFailedException("Required mobile button image is missing: " + buttonPath);
            }
            AssetDatabase.ImportAsset(buttonPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            Texture2D button = AssetDatabase.LoadAssetAtPath<Texture2D>(buttonPath);
            if (button == null)
            {
                throw new BuildFailedException("Mobile button image could not be imported: " + buttonPath);
            }

            // The icon-only controls are square RGBA textures. Validate useful
            // imported dimensions and a square aspect ratio.
            float aspect = (float)button.width / Mathf.Max(button.height, 1);
            if (button.width < 240 || button.height < 240 || aspect < 0.95f || aspect > 1.05f)
            {
                throw new BuildFailedException("Mobile button image has invalid imported dimensions: " + buttonPath + " (" + button.width + "x" + button.height + ")");
            }
            Debug.Log("Verified mobile button image: " + buttonPath + " (" + button.width + "x" + button.height + ")");
        }
        Debug.Log("Verified mobile Look Back and Run button images.");

        if (!File.Exists(RulerAudioAssetPath))
        {
            throw new BuildFailedException("Required replacement Baldi ruler sound is missing: " + RulerAudioAssetPath);
        }
        AssetDatabase.ImportAsset(RulerAudioAssetPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        AudioClip rulerAudio = AssetDatabase.LoadAssetAtPath<AudioClip>(RulerAudioAssetPath);
        if (rulerAudio == null || rulerAudio.length < 0.5f || rulerAudio.length > 1.0f || rulerAudio.channels != 1)
        {
            throw new BuildFailedException("Replacement Baldi ruler sound must be a 0.5-1.0 second mono AudioClip: " + RulerAudioAssetPath);
        }
        Debug.Log("Verified replacement Baldi ruler sound: " + rulerAudio.length.ToString("F2") + " seconds, " + rulerAudio.frequency + " Hz.");
    }
}
#endif
