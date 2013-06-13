using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;

public class ExportUnityPackage : MonoBehaviour 
{
	[MenuItem ("Futile/Export both .unitypackages")]
	static public void ExportBoth()
	{
		ExportCore();
		ExportWithDemos();
	}

	//[MenuItem ("Futile/Export FutileCore.unitypackage")]
	static public void ExportCore()
	{
		Export("FutileCore",false);
	}

	//[MenuItem ("Futile/Export FutileWithDemos.unitypackage")]
	static public void ExportWithDemos()
	{
		Export("FutileWithDemos",true);
	}

	static public void Export(string fileName, bool shouldIncludeDemos)
	{
		List<String> paths = new List<String>();

		paths.Add("Assets/Futile");
		if(shouldIncludeDemos) paths.Add("Assets/FutileDemos");
		if(shouldIncludeDemos) paths.Add("Assets/FutileScene");

		AssetDatabase.ExportPackage(paths.ToArray(),"../../../reference/UnityPackages/"+fileName+".unitypackage", ExportPackageOptions.Recurse); 
	}


	[MenuItem ("Futile/Upload both .unitypackages")]
	static public void Upload()
	{
		Process uploadBat = new Process();
		uploadBat.StartInfo.FileName = "..\\..\\..\\reference\\UnityPackages\\upload.bat";
		UnityEngine.Debug.Log("FIL " + uploadBat.StartInfo.FileName);
		uploadBat.Start();
	}
}
