#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.Collections.Generic;

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
        PlayerSettings.bundleVersion = "1.3.0";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.cursedclassroom.baldihorror");
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;
        PlayerSettings.Android.bundleVersionCode = 4;
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
#endif
