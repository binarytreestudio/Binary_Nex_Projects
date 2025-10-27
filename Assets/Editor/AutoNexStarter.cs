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
    // 自動化 NexStarter 的所有步驟，在 Editor 啟動時執行一次。
    [InitializeOnLoad]
    public class AutoNexStarter
    {
        private const string CONFIG_KEY = "AutoNexStarter_Completed"; // 用來檢查是否已執行過

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

                // 步驟 1: 配置 UPM 認證 (使用你的 access token)
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

                // 標記為已完成
                EditorPrefs.SetBool(CONFIG_KEY, true);

                Debug.Log("AutoNexStarter: 配置完成！請重啟 Unity Editor 以確保變更生效。");
            }
            catch (Exception ex)
            {
                Debug.LogError($"AutoNexStarter: 配置失敗 - {ex.Message}\n{ex.StackTrace}");
                // 不標記為完成，讓下次重試
            }
        }

        // 步驟 1: 配置 .upmconfig.toml (從 NexStarter 複製)
        private static void ConfigureUpmConfig()
        {
            const string accessToken = "YOUR_ACCESS_TOKEN_HERE"; // 替換為你的 key
            Nex.Starter.NexStarter.UpmConfigWriter.Merge(accessToken);
            Debug.Log("AutoNexStarter: 已配置 UPM 認證 (.upmconfig.toml)。");
        }

        // 步驟 2: 配置 Scoped Registry (從 NexStarter 複製)
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
            Debug.Log("AutoNexStarter: 已配置 Scoped Registry。");
        }

        // 步驟 3: 添加 MDK 套件 (從 NexStarter 複製)
        private static void AddMdk()
        {
            var manager = Nex.Starter.NexStarter.PackageManager.Instance;
            manager.AddPackages("team.nex.mdk.body", "team.nex.nex-opencv-for-unity", "team.nex.ml-models");
            Debug.Log("AutoNexStarter: 已添加/更新 MDK 套件。");
        }

        // 步驟 4: 配置專案設定 (從 NexStarter 複製)
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

        // 步驟 5: (可選) 添加 Hand Pose (從 NexStarter 複製)
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

        // 步驟 6: 驗證 MDK (從 NexStarter 複製)
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

        // 輔助類 (從 NexStarter 複製必要的部分)
        [System.Serializable]
        private class MdkVerifier
        {
            public string version = "";
        }

        // 以下是從 NexStarter 複製的必要類別 (UpmConfigWriter 和 PackageManager)
        // ... (將 UpmConfigWriter 和 PackageManager 的完整代碼從 NexStarter.cs 複製到這裡)
        // 注意：為了完整性，你需要將 NexStarter.cs 中的 UpmConfigWriter、PackageManager、RegistrySpec 等類別複製到此腳本中。
        // 如果空間不足，可以將整個 NexStarter.cs 作為依賴，或合併成一個檔案。
    }
}