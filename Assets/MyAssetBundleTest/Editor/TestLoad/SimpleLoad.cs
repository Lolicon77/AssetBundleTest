using UnityEditor;
using UnityEngine;

public class SimpleLoad : MonoBehaviour {
	[MenuItem("Assets/SimpleLoad")]
	private static void Handle1() {
		AssetBundle bundle = AssetBundle.LoadFromFile(@"F:\Work\Test\TestAssetBundle\AssetBundles\77\myassets");

		foreach (var name in bundle.GetAllAssetNames())
			Debug.Log(name);

		GameObject cube = bundle.LoadAsset<GameObject>("Cube");
		var cubeIns = Instantiate(cube);

		Texture2D logo = bundle.LoadAsset<Texture2D>("UnityLogo");
		var renderer = cubeIns.GetComponent<Renderer>();
		renderer.sharedMaterial.mainTexture = logo;

		bundle.Unload(false);
	}

	//	[MenuItem("Assets/SimpleUnLoad")]
	//	private static void Handle2()
	//	{
	//		AssetBundle bundle = assetbu
	//	}
}
