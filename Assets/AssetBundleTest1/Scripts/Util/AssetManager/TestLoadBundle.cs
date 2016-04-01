using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TestLoadBundle : MonoBehaviour {
	IEnumerator Start() {
		AssetBundleManager.Init();
		AssetBundleLoadManifestOperation op = AssetBundleManager.LoadManifestAsset();
		yield return StartCoroutine(op);
		Debug.Log("load manifest finished!");

	}

	public void UnloadBundle(GameObject obj) {
		if (obj) {
			DestroyImmediate(obj, true);
		}
		AssetBundleManager.UnloadAssetBundle("cube.unity3d");
	}

	public void OnClickLoad() {
		StartCoroutine(LoadBundle());
	}

	IEnumerator LoadBundle() {
		AssetBundleLoadAssetOperation op2 = AssetBundleManager.LoadAssetAsync("cube.unity3d", "Cube", typeof(GameObject));
		yield return StartCoroutine(op2);

		GameObject cube = op2.GetAsset<GameObject>();
		if (cube != null) {
			GameObject.Instantiate(cube);
		}
	}



}
