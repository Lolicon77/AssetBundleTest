using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;

public class BuildAssetBundle
{
    const string m_outputDir = "AssetBundles";

    [MenuItem("UGameTools/BuildBundles/GenerateBundles")]
    static void BuildBundle()
    {
        string platformStr = AssetBundleManager.GetPlatformFolderForAssetBundles(EditorUserBuildSettings.activeBuildTarget);
		Debug.Log(platformStr);
        string outputPath = Path.Combine(m_outputDir, platformStr);
		Debug.Log(outputPath);
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }
        BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
    }
}
