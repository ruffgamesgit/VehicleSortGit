using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public static class ByteBrewOnLoadPackageImportCredsHolder {
    public static ByteBrewSettings BBSettings = null;

    public static string AndroidEnabledPlayerPrefsKey = "BYTEBREW_ANDROID_ENABLED";
    public static string IOSEnabledPlayerPrefsKey = "BYTEBREW_IOS_ENABLED";

    public static string AndroidGameIDPlayerPrefsKey = "BYTEBREW_ANDROID_GAME_ID";
    public static string AndroidSDKKeyPlayerPrefsKey = "BYTEBREW_ANDROID_SDK_KEY";
    public static string IOSGameIDPlayerPrefsKey = "BYTEBREW_IOS_GAME_ID";
    public static string IOSSDKKeyPlayerPrefsKey = "BYTEBREW_IOS_SDK_KEY";

    public static bool AndroidEnabled => PlayerPrefs.GetInt(AndroidEnabledPlayerPrefsKey, 0) == 1;
    public static bool IOSEnabled => PlayerPrefs.GetInt(IOSEnabledPlayerPrefsKey, 0) == 1;

    public static string AndroidGameID => PlayerPrefs.GetString(AndroidGameIDPlayerPrefsKey, "");
    public static string AndroidGameKey => PlayerPrefs.GetString(AndroidSDKKeyPlayerPrefsKey, "");
    public static string IOSGameID => PlayerPrefs.GetString(IOSGameIDPlayerPrefsKey, "");
    public static string IOSGameKey => PlayerPrefs.GetString(IOSSDKKeyPlayerPrefsKey, "");

    static ByteBrewOnLoadPackageImportCredsHolder() {
        SetSDKKeysToPlayerPrefs();
    }

    public static void SetSDKKeysToPlayerPrefs() {
        if (BBSettings == null) {
            BBSettings = Resources.Load<ByteBrewSettings>("ByteBrewSettings");
        }

        if (BBSettings == null) {
            return;
        }

        if (BBSettings.androidEnabled) {
            PlayerPrefs.SetInt(AndroidEnabledPlayerPrefsKey, 1);
            PlayerPrefs.SetString(AndroidGameIDPlayerPrefsKey, BBSettings.androidGameID);
            PlayerPrefs.SetString(AndroidSDKKeyPlayerPrefsKey, BBSettings.androidSDKKey);
        }

        if (BBSettings.iosEnabled) {
            PlayerPrefs.SetInt(IOSEnabledPlayerPrefsKey, 1);
            PlayerPrefs.SetString(IOSGameIDPlayerPrefsKey, BBSettings.iosGameID);
            PlayerPrefs.SetString(IOSSDKKeyPlayerPrefsKey, BBSettings.iosSDKKey);
        }
    }
}

public class ByteBrewPackageImportPostProcessor : AssetPostprocessor {
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
        EditorApplication.delayCall += DelayedMethod;
    }

    private static void DelayedMethod() {
        EditorApplication.delayCall -= DelayedMethod;

        ByteBrewSettings bbSettings = Resources.Load<ByteBrewSettings>("ByteBrewSettings");

        if (bbSettings == null) {
            return;
        }

        if (ByteBrewOnLoadPackageImportCredsHolder.AndroidEnabled) {
            bbSettings.androidEnabled = true;
            bbSettings.androidGameID = ByteBrewOnLoadPackageImportCredsHolder.AndroidGameID;
            bbSettings.androidSDKKey = ByteBrewOnLoadPackageImportCredsHolder.AndroidGameKey;
        }

        if (ByteBrewOnLoadPackageImportCredsHolder.IOSEnabled) {
            bbSettings.iosEnabled = true;
            bbSettings.iosGameID = ByteBrewOnLoadPackageImportCredsHolder.IOSGameID;
            bbSettings.iosSDKKey = ByteBrewOnLoadPackageImportCredsHolder.IOSGameKey;
        }
    }
}
