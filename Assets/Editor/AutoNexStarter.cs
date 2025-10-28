using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
using static Nex.Starter.NexStarter;
namespace Nex.Starter
{
    public class AutoNexStarterForceReload
    {
        private const string CONFIG_KEY = "AutoNexStarter_Completed_v3";

        [MenuItem("Nex/Force Reload Nex Starter")]
        private static void ForceReload()
        {
            if (EditorPrefs.HasKey(CONFIG_KEY))
            {
                EditorPrefs.DeleteKey(CONFIG_KEY);
                Debug.Log("AutoNexStarterForceReload: 已重置 AutoNexStarter 完成標記，程序將在下次編譯或啟動時重新運行。");
            }
            else
            {
                Debug.Log("AutoNexStarterForceReload: 完成標記不存在，無需重置。");
            }
        }
    }
    // 自動化 NexStarter 的所有步驟，並在完成後安裝 .unitypackage
    [InitializeOnLoad]
    public class AutoNexStarter
    {
        private const string CONFIG_KEY = "AutoNexStarter_Completed_v3"; // 用來檢查是否已執行過
        private const string PACKAGE_PATH = "Assets/NexPackages/2.PlaygroundSDKEssentials-20251016.unitypackage"; // 目標 .unitypackage 路徑

        static AutoNexStarter()
        {
            // 只在首次運行或未完成時執行
            if (EditorPrefs.HasKey(CONFIG_KEY) && EditorPrefs.GetBool(CONFIG_KEY, false))
            {
                Debug.Log("AutoNexStarter: 已完成配置，跳過自動化。");
                return;
            }

            try
            {
                Debug.Log("AutoNexStarter: 開始自動配置 Nex SDK...");

                // 步驟 1: 配置 UPM 認證
                ConfigureUpmConfig();

                // 步驟 2: 配置 Scoped Registry
                ConfigureScopedRegistry();

                // 步驟 3: 添加/更新 MDK 套件
                AddMdk();

                // 步驟 4: 配置專案設定
                ConfigureProjectSettings();

                // 步驟 5: (可選) 添加 Hand Pose 支援
                // AddHandPose(); // 取消註解以啟用

                // 步驟 6: 驗證 MDK 安裝
                VerifyMdkInstallation();

                // 步驟 7: 自動安裝 .unitypackage
                InstallUnityPackage();

                // 標記為已完成
                EditorPrefs.SetBool(CONFIG_KEY, true);

                Debug.Log("AutoNexStarter: 配置和 .unitypackage 安裝完成！請重啟 Unity Editor 以確保變更生效。");
            }
            catch (Exception ex)
            {
                Debug.LogError($"AutoNexStarter: 配置或安裝失敗 - {ex.Message}\n{ex.StackTrace}");
                // 不標記為完成，讓下次重試
            }
        }

        // 步驟 1: 配置 .upmconfig.toml
        private static void ConfigureUpmConfig()
        {
            const string accessToken = "VXxq0uuN5aoTb2N0A2HXKEtD1ru6K6H+I58QF1uk/ShjjqiGzOVR04yjvR4eKcGL"; // 替換為你的 key
            Nex.Starter.NexStarter.UpmConfigWriter.Merge(accessToken);
            Debug.Log("AutoNexStarter: 已配置 UPM 認證 (.upmconfig.toml)。");
        }

        // 步驟 2: 配置 Scoped Registry
        private static void ConfigureScopedRegistry()
        {
            var manager = PackageManager.Instance;
            manager.AddScopedRegistries(
                new PackageManager.RegistrySpec("Nex Packages", "https://packages.nex.inc",
                    new[]
                    {
                        "team.nex",
                        "com.cysharp.unitask",
                        "com.dbrizov.naughtyattributes",
                        "com.textus-games.serialized-reference-ui",
                        "net.tnrd.serializableinterface",
                    }));
            Debug.Log("AutoNexStarter: 已配置 Scoped Registry。");
        }

        // 步驟 3: 添加 MDK 套件
        private static void AddMdk()
        {
            var manager = PackageManager.Instance;
            manager.AddPackages("team.nex.mdk.body", "team.nex.nex-opencv-for-unity", "team.nex.ml-models");
            Debug.Log("AutoNexStarter: 已添加/更新 MDK 套件。");
        }

        // 步驟 4: 配置專案設定
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

            Debug.Log("AutoNexStarter: 已配置專案設定。");
        }

        // 步驟 5: (可選) 添加 Hand Pose
        private static void AddHandPose()
        {
            var manager = PackageManager.Instance;
            if (IsMinApiLevelReadyForHandPose())
            {
                manager.AddPackages("team.nex.mdk.hand");
            }
            else
            {
                manager.AddPackages("team.nex.min-playos-api-level@1.2.1", "team.nex.mdk.hand");
            }
            Debug.Log("AutoNexStarter: 已添加 Hand Pose 支援。");
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

        // 步驟 6: 驗證 MDK 安裝
        private static void VerifyMdkInstallation()
        {
            try
            {
                var asset = AssetDatabase.LoadAssetAtPath<TextAsset>("Packages/team.nex.mdk/Resources/mdk-info.json");
                if (asset == null)
                {
                    Debug.LogWarning("AutoNexStarter: 未找到 mdk-info.json，MDK 未安裝。");
                    return;
                }

                var verifier = new MdkVerifier { version = "" };
                EditorJsonUtility.FromJsonOverwrite(asset.text, verifier);
                Debug.Log($"AutoNexStarter: MDK {verifier.version} 已安裝。");
            }
            catch (Exception ex)
            {
                Debug.LogError($"AutoNexStarter: 驗證 MDK 失敗 - {ex.Message}");
            }
        }

        // 步驟 7: 自動安裝 .unitypackage
        private static void InstallUnityPackage()
        {
            if (File.Exists(PACKAGE_PATH))
            {
                AssetDatabase.ImportPackage(PACKAGE_PATH, true); // true 表示默認導入所有內容
                Debug.Log($"AutoNexStarter: 已安裝 .unitypackage: {PACKAGE_PATH}");
            }
            else
            {
                Debug.LogError($"AutoNexStarter: 找不到 .unitypackage 檔案: {PACKAGE_PATH}");
            }
        }

        // 輔助類
        [System.Serializable]
        private class MdkVerifier
        {
            public string version = "";
        }

        // 以下是從 NexStarter 複製的必要類別 (UpmConfigWriter 和 PackageManager)
        // ... (將 UpmConfigWriter 和 PackageManager 的完整代碼從 NexStarter.cs 複製到這裡)
        // 如果空間不足，可以將整個 NexStarter.cs 作為依賴，或合併成一個檔案。
    }
}