#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Keeps WebGL builds broadly compatible with static hosts that cannot configure
/// Content-Encoding headers for pre-compressed Unity build files.
/// </summary>
public sealed class WebGLBuildCompatibility : IPreprocessBuildWithReport
{
    public int callbackOrder => -1000;

    public void OnPreprocessBuild(BuildReport report)
    {
        if (report.summary.platform != BuildTarget.WebGL)
            return;

        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        Debug.Log("WebGL compatibility: compression disabled for static hosting.");
    }
}
#endif
