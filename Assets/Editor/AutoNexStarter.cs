using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

#nullable enable

namespace Nex.Starter
{
    // �۰ʤ� NexStarter ���Ҧ��B�J�A�b Editor �Ұʮɰ���@���C
    [InitializeOnLoad]
    public class AutoNexStarter
    {
        private const string CONFIG_KEY = "AutoNexStarter_Completed"; // �Ψ��ˬd�O�_�w����L

        static AutoNexStarter()
        {
            // �u�b�����B��Υ������ɰ���
            if (EditorPrefs.HasKey(CONFIG_KEY) && EditorPrefs.GetBool(CONFIG_KEY, false))
            {
                Debug.Log("AutoNexStarter: �w�����t�m�A���L�۰ʤơC");
                return;
            }

            try
            {
                Debug.Log("AutoNexStarter: �}�l�۰ʰt�m Nex SDK...");

                // �B�J 1: �t�m UPM �{�� (�ϥΧA�� access token)
                ConfigureUpmConfig();

                // �B�J 2: �t�m Scoped Registry
                ConfigureScopedRegistry();

                // �B�J 3: �K�[/��s MDK �M��
                AddMdk();

                // �B�J 4: �t�m�M�׳]�w
                ConfigureProjectSettings();

                // �B�J 5: (�i��) �K�[ Hand Pose �䴩
                // AddHandPose(); // �������ѥH�ҥ�

                // �B�J 6: ���� MDK �w��
                VerifyMdkInstallation();

                // �аO���w����
                EditorPrefs.SetBool(CONFIG_KEY, true);

                Debug.Log("AutoNexStarter: �t�m�����I�Э��� Unity Editor �H�T�O�ܧ�ͮġC");
            }
            catch (Exception ex)
            {
                Debug.LogError($"AutoNexStarter: �t�m���� - {ex.Message}\n{ex.StackTrace}");
                // ���аO�������A���U������
            }
        }

        // �B�J 1: �t�m .upmconfig.toml (�q NexStarter �ƻs)
        private static void ConfigureUpmConfig()
        {
            const string accessToken = "YOUR_ACCESS_TOKEN_HERE"; // �������A�� key
            Nex.Starter.NexStarter.UpmConfigWriter.Merge(accessToken);
            Debug.Log("AutoNexStarter: �w�t�m UPM �{�� (.upmconfig.toml)�C");
        }

        // �B�J 2: �t�m Scoped Registry (�q NexStarter �ƻs)
        private static void ConfigureScopedRegistry()
        {
            var manager = Nex.Starter.NexStarter.PackageManager.Instance;
            manager.AddScopedRegistries(
                new Nex.Starter.NexStarter.PackageManager.RegistrySpec("Nex Packages", "https://packages.nex.inc",
                    new[]
                    {
                        "team.nex",
                        "com.cysharp.unitask",
                        "com.dbrizov.naughtyattributes",
                        "com.textus-games.serialized-reference-ui",
                        "net.tnrd.serializableinterface",
                    }));
            Debug.Log("AutoNexStarter: �w�t�m Scoped Registry�C");
        }

        // �B�J 3: �K�[ MDK �M�� (�q NexStarter �ƻs)
        private static void AddMdk()
        {
            var manager = Nex.Starter.NexStarter.PackageManager.Instance;
            manager.AddPackages("team.nex.mdk.body", "team.nex.nex-opencv-for-unity", "team.nex.ml-models");
            Debug.Log("AutoNexStarter: �w�K�[/��s MDK �M��C");
        }

        // �B�J 4: �t�m�M�׳]�w (�q NexStarter �ƻs)
        private static void ConfigureProjectSettings()
        {
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.OpenGLES3 });
            EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ETC2;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel30;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetApiCompatibilityLevel(NamedBuildTarget.Android, ApiCompatibilityLevel.NET_Standard);

            PlayerSettings.SetStackTraceLogType(LogType.Error, StackTraceLogType.Full);
            PlayerSettings.SetStackTraceLogType(LogType.Assert, StackTraceLogType.ScriptOnly);
            PlayerSettings.SetStackTraceLogType(LogType.Warning, StackTraceLogType.ScriptOnly);
            PlayerSettings.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            PlayerSettings.SetStackTraceLogType(LogType.Exception, StackTraceLogType.Full);

#if UNITY_6000_0_OR_NEWER
            PlayerSettings.Android.textureCompressionFormats = new[] { TextureCompressionFormat.ETC2 };
            PlayerSettings.Android.applicationEntry = AndroidApplicationEntry.Activity;
#endif

            Debug.Log("AutoNexStarter: �w�t�m�M�׳]�w�C");
        }

        // �B�J 5: (�i��) �K�[ Hand Pose (�q NexStarter �ƻs)
        private static void AddHandPose()
        {
            var manager = Nex.Starter.NexStarter.PackageManager.Instance;
            if (IsMinApiLevelReadyForHandPose())
            {
                manager.AddPackages("team.nex.mdk.hand");
            }
            else
            {
                manager.AddPackages("team.nex.min-playos-api-level@1.2.1", "team.nex.mdk.hand");
            }
            Debug.Log("AutoNexStarter: �w�K�[ Hand Pose �䴩�C");
        }

        private static bool IsMinApiLevelReadyForHandPose()
        {
            PackageInfo[] packages = PackageInfo.GetAllRegisteredPackages();
            var package = System.Array.Find(packages, p => p.name == "team.nex.min-playos-api-level");

            if (package == null) return false;

            var versionSplit = package.version.Split('.', 3);
            if (versionSplit.Length < 3) return false;

            if (!int.TryParse(versionSplit[0], out var major) || major < 1) return false;
            if (!int.TryParse(versionSplit[1], out var minor) || minor < 2) return false;
            if (!int.TryParse(versionSplit[2], out var patch) || patch < 1) return false;

            return true;
        }

        // �B�J 6: ���� MDK (�q NexStarter �ƻs)
        private static void VerifyMdkInstallation()
        {
            try
            {
                var asset = AssetDatabase.LoadAssetAtPath<TextAsset>("Packages/team.nex.mdk/Resources/mdk-info.json");
                if (asset == null)
                {
                    Debug.LogWarning("AutoNexStarter: ����� mdk-info.json�AMDK ���w�ˡC");
                    return;
                }

                var verifier = new MdkVerifier { version = "" };
                EditorJsonUtility.FromJsonOverwrite(asset.text, verifier);
                Debug.Log($"AutoNexStarter: MDK {verifier.version} �w�w�ˡC");
            }
            catch (Exception ex)
            {
                Debug.LogError($"AutoNexStarter: ���� MDK ���� - {ex.Message}");
            }
        }

        // ���U�� (�q NexStarter �ƻs���n������)
        [System.Serializable]
        private class MdkVerifier
        {
            public string version = "";
        }

        // �H�U�O�q NexStarter �ƻs�����n���O (UpmConfigWriter �M PackageManager)
        // ... (�N UpmConfigWriter �M PackageManager ������N�X�q NexStarter.cs �ƻs��o��)
        // �`�N�G���F����ʡA�A�ݭn�N NexStarter.cs ���� UpmConfigWriter�BPackageManager�BRegistrySpec �����O�ƻs�즹�}�����C
        // �p�G�Ŷ������A�i�H�N��� NexStarter.cs �@���̿�A�ΦX�֦��@���ɮסC
    }
}