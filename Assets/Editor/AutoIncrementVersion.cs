using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class AutoIncrementVersion : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        // �����e������
        string currentVersion = PlayerSettings.bundleVersion;
        string[] versionParts = currentVersion.Split('.');

        // �T�O�������榡�� Major.Minor.Patch�]�Ҧp 1.0.0�^
        if (versionParts.Length != 3 || !int.TryParse(versionParts[2], out int patch))
        {
            Debug.LogError("�������榡���~�A�нT�O�榡�� Major.Minor.Patch�]�Ҧp 1.0.0�^");
            return;
        }

        // ���W patch ��
        patch++;
        string patchString = patch.ToString("D3");
        string newVersion = $"{versionParts[0]}.{versionParts[1]}.{patchString}";

        // ��s Unity ��������
        PlayerSettings.bundleVersion = newVersion;
        Debug.Log($"�������w��s���G{newVersion}");

        // �p�G�ݭn�w�� Android ��s bundleVersionCode
#if UNITY_ANDROID
        PlayerSettings.Android.bundleVersionCode++;
        Debug.Log($"Android Bundle Version Code �w��s���G{PlayerSettings.Android.bundleVersionCode}");
#endif
    }
}