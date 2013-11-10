using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;

//way faster than a normal rect
public class PRRect
{
	public int x;
	public int y;
	public int width;
	public int height;

	public PRRect Clone()
	{
		PRRect rect = new PRRect();
		rect.x = x;
		rect.y = y;
		rect.width = width;
		rect.height = height;
		return rect;
	}
}

public static class PRUtils
{
	//Unity's FileUtil.GetProjectRelativePath() only works for files INSIDE the project,
	//but this method works for ALL files+folders
	public static string GetTrueProjectRelativePath(string inputPath) 
	{
		string fullProjectPath = Directory.GetParent(Path.GetFullPath(Application.dataPath)).FullName;
		
		return PRUtils.GetRelativePath(fullProjectPath, Path.GetFullPath(inputPath));
	}

	//from http://mrpmorris.blogspot.ca/2007/05/convert-absolute-path-to-relative-path.html
	public static string GetRelativePath(string absolutePath, string relativeTo)
	{
		string[] absoluteDirectories = absolutePath.Split(Path.DirectorySeparatorChar);
		string[] relativeDirectories = relativeTo.Split(Path.DirectorySeparatorChar);
		
		//Get the shortest of the two paths
		int length = absoluteDirectories.Length < relativeDirectories.Length ? absoluteDirectories.Length : relativeDirectories.Length;
		
		//Use to determine where in the loop we exited
		int lastCommonRoot = -1;
		int index;
		
		//Find common root
		for (index = 0; index < length; index++)
			if (absoluteDirectories[index] == relativeDirectories[index])
				lastCommonRoot = index;
		else
			break;
		
		//If we didn't find a common prefix then use the absolute path of the second path instead
		if (lastCommonRoot == -1)
			return relativeTo;
		
		//Build up the relative path
		StringBuilder relativePath = new StringBuilder();
		
		//Add on the ..
		for (index = lastCommonRoot + 1; index < absoluteDirectories.Length; index++)
			if (absoluteDirectories[index].Length > 0)
				relativePath.Append("../");
		
		//Add on the folders
		for (index = lastCommonRoot + 1; index < relativeDirectories.Length - 1; index++)
			relativePath.Append(relativeDirectories[index] + "/");
		relativePath.Append(relativeDirectories[relativeDirectories.Length - 1]);
		
		return relativePath.ToString();
	}
}
