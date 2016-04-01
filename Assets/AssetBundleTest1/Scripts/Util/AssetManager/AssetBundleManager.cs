using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEditor;

public class LoadedAssetBundle
{
    public AssetBundle m_AssetBundle;
    public int m_Reference;
    public LoadedAssetBundle(AssetBundle assetBundle)
    {
        m_AssetBundle = assetBundle;
        m_Reference = 1;
    }
}

public class AssetBundleManager : MonoBehaviour
{
    public static AssetBundleManifest AssetBundleManifestObject
    {
        set { s_AssetBundleManifest = value; }
    }
    static string s_BundlesDir = "/AssetBundles/";
    static string s_PlatformDir = "";
    static string s_DownloadDir = "";
    static AssetBundleManifest s_AssetBundleManifest = null;
    static Dictionary<string, LoadedAssetBundle> s_LoadedAssetBundles = new Dictionary<string, LoadedAssetBundle>();
    static Dictionary<string, WWW> s_DownloadingWWWs = new Dictionary<string, WWW>();
    static Dictionary<string, string> s_DownloadingErrors = new Dictionary<string, string>();
    static List<AssetBundleLoadAssetOperation> s_DownloadingOps = new List<AssetBundleLoadAssetOperation>();
    static Dictionary<string, string[]> s_Dependencies = new Dictionary<string, string[]>();

    public static void Init()
    {
        s_PlatformDir =
#if UNITY_EDITOR
            GetPlatformFolderForAssetBundles(EditorUserBuildSettings.activeBuildTarget);
#else
            GetPlatformFolderForAssetBundles(Application.platform);
#endif
        s_DownloadDir = GetRelativePath() + s_BundlesDir + s_PlatformDir + "/";
    }

    public static AssetBundleLoadAssetOperation LoadAssetAsync(string assetBundleName, string assetName, System.Type type)
    {
        LoadAssetBundle(assetBundleName);
        AssetBundleLoadAssetOperation operation = new AssetBundleLoadCommonAsset(assetBundleName, assetName, type);
        s_DownloadingOps.Add(operation);
        return operation;
    }

    public static AssetBundleLoadManifestOperation LoadManifestAsset()
    {
        LoadAssetBundle(s_PlatformDir, true);
        AssetBundleLoadManifestOperation operation = new AssetBundleLoadManifestOperation(s_PlatformDir, "AssetBundleManifest", typeof(AssetBundleManifest));
        s_DownloadingOps.Add(operation);
        return operation;
    }

    public static void UnloadAssetBundle(string assetBundleName)
    {
        string error;
        LoadedAssetBundle bundle = GetLoadedAssetBundle(assetBundleName, out error);
        if (bundle == null)
        {
            return;
        }
        if (--bundle.m_Reference == 0)
        {
            bundle.m_AssetBundle.Unload(true);
            Debug.Log("unload a assset bundle " + assetBundleName);
            s_LoadedAssetBundles.Remove(assetBundleName);
        }
        string[] dependencies = null;
        if (!s_Dependencies.TryGetValue(assetBundleName, out dependencies))
        {
            return;
        }
        foreach (string item in dependencies)
        {
            UnloadAssetBundle(item);
        }
        if (bundle.m_Reference == 0)
        {
            s_Dependencies.Remove(assetBundleName);
        }
    }


    static void LoadAssetBundle(string assetBundleName, bool isManifestFile = false)
    {
        bool isExisted = CheckExists(assetBundleName);
        if (!isExisted && !isManifestFile)
        {
            LoadDependencies(assetBundleName);
        }
    }

    static bool CheckExists(string assetBundleName)
    {
        LoadedAssetBundle bundle = null;
        s_LoadedAssetBundles.TryGetValue(assetBundleName, out bundle);
        if (bundle != null)
        {
            bundle.m_Reference++;
            string[] dependencies = null;
            if(s_Dependencies.TryGetValue(assetBundleName, out dependencies))
            {
                foreach (string item in dependencies)
                {
                    CheckExists(item);
                }
            }
            return true;
        }
        if (s_DownloadingWWWs.ContainsKey(assetBundleName)) // be sure only one AssetBundle
        {
            return true;
        }
        WWW www = new WWW(s_DownloadDir + assetBundleName);
        s_DownloadingWWWs.Add(assetBundleName, www);
        return false;
    }

    static string GetRelativePath()
    {
        if (Application.isEditor)
            return "file://" + System.Environment.CurrentDirectory.Replace("\\", "/"); // Use the build output folder directly.
        else
            return "";
    }

    static void LoadDependencies(string assetBundleName)
    {
        if (s_AssetBundleManifest == null)
        {
            Debug.Log("manifest file is null");
            return;
        }
        string[] dependencies = s_AssetBundleManifest.GetAllDependencies(assetBundleName);
        if (dependencies.Length > 0)
        {
            s_Dependencies.Add(assetBundleName, dependencies);
            for (int i = 0; i < dependencies.Length; ++i)
            {
                LoadAssetBundle(dependencies[i], false);
            }
        }
    }

    public static LoadedAssetBundle GetLoadedAssetBundle(string assetBundleName, out string error)
    {
        if (s_DownloadingErrors.TryGetValue(assetBundleName, out error))
        {
            return null;
        }
        LoadedAssetBundle bundle = null;
        s_LoadedAssetBundles.TryGetValue(assetBundleName, out bundle);
        if (bundle == null)
        {
            return null;
        }
        string[] dependencies = null;
        if (!s_Dependencies.TryGetValue(assetBundleName, out dependencies))
        {
            return bundle;
        }
        foreach (string item in dependencies)
        {
            if (s_DownloadingErrors.TryGetValue(item, out error))
            {
                return null;
            }
            LoadedAssetBundle dependentBundle;
            s_LoadedAssetBundles.TryGetValue(item, out dependentBundle);
            if (dependentBundle == null)
            {
                return null;
            }
        }
        return bundle;
    }

    void Update()
    {
        RemoveFinishedWWWs();
        RemoveFinishedOps();
    }

    void RemoveFinishedWWWs()
    {
        List<string> keysToRemove = new List<string>();
        foreach (var item in s_DownloadingWWWs)
        {
            WWW www = item.Value;
            if (www.error != null)
            {
                Debug.LogError(www.error);
                s_DownloadingErrors.Add(item.Key, www.error);
                keysToRemove.Add(item.Key);
                continue;
            }
            if (www.isDone)
            {
                s_LoadedAssetBundles.Add(item.Key, new LoadedAssetBundle(www.assetBundle));
                keysToRemove.Add(item.Key);
            }
        }
        foreach (string item in keysToRemove)
        {
            WWW www = s_DownloadingWWWs[item];
            s_DownloadingWWWs.Remove(item);
            www.Dispose();
        }
    }

    void RemoveFinishedOps()
    {
        for (int i = 0; i < s_DownloadingOps.Count; )
        {
            if (!s_DownloadingOps[i].Update())
            {
                s_DownloadingOps.RemoveAt(i);
            }
            else
            {
                i++;
            }
        }
    }

    public static string GetPlatformFolderForAssetBundles(BuildTarget target)
    {
        switch (target)
        {
            case BuildTarget.Android:
                return "Android";
            case BuildTarget.iOS:
                return "iOS";
            case BuildTarget.WebPlayer:
                return "WebPlayer";
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                return "Windows";
            default:
                return null;
        }
    }
}
