using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class AutoIncrementVersion : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        // 獲取當前版本號
        string currentVersion = PlayerSettings.bundleVersion;
        string[] versionParts = currentVersion.Split('.');

        // 確保版本號格式為 Major.Minor.Patch（例如 1.0.0）
        if (versionParts.Length != 3 || !int.TryParse(versionParts[2], out int patch))
        {
            Debug.LogError("版本號格式錯誤，請確保格式為 Major.Minor.Patch（例如 1.0.0）");
            return;
        }

        // 遞增 patch 號
        patch++;
        string patchString = patch.ToString("D3");
        string newVersion = $"{versionParts[0]}.{versionParts[1]}.{patchString}";

        // 更新 Unity 的版本號
        PlayerSettings.bundleVersion = newVersion;
        Debug.Log($"版本號已更新為：{newVersion}");

        // 如果需要針對 Android 更新 bundleVersionCode
#if UNITY_ANDROID
        PlayerSettings.Android.bundleVersionCode++;
        Debug.Log($"Android Bundle Version Code 已更新為：{PlayerSettings.Android.bundleVersionCode}");
#endif
    }
}