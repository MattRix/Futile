using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Collections;

public class ExportUnityPackage : MonoBehaviour 
{
	[MenuItem ("Futile/ExportUnityPackage")]
	static public void Export()
	{
		List<String> paths = new List<String>();

		paths.Add("Futile/");
		paths.Add("Assets/FutileDemos/");
		paths.Add("Assets/FutileScene.scene");

		Debug.Log("V1 ");

		Debug.Log("SASEED " + AssetDatabase.GetAssetPath(Shader.Find("Futile/Basic")));

		//ExportPackageOptions.Recurse | ExportPackageOptions.IncludeLibraryAssets
		AssetDatabase.ExportPackage(paths.ToArray(),"TestThing.unitypackage", ExportPackageOptions.Recurse | ExportPackageOptions.Interactive); 
	}
}
