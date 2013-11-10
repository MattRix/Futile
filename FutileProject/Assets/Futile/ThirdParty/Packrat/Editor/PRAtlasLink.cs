using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

//PR = FutileGenerator, the tool that makes atlases from source images
//PRAtlasLink - links a source images folder with an output atlas path
public class PRAtlasLink
{
	public string sourceFolderPath = "";
	public string atlasFilePath = "";
	
	public bool shouldGenerate = false;
	public bool shouldAutoGenerate = false;
	public bool shouldFoldout = false;
	public bool shouldAddSubfolders = true;
	public bool shouldUseBytes = false;

	public float scale = 1.0f;
	public int padding = 1;
	public int extrude = 0;
	public int trimPadding = 1;
	public bool shouldTrim = true;

	public PRAtlasLink(string sourceFolderPath, string atlasFilePath)
	{
		this.sourceFolderPath = sourceFolderPath;
		this.atlasFilePath = atlasFilePath;
	}
	
	public PRAtlasLink(Dictionary<string,object> dict)
	{
		dict.SetStringIfExists("sourceFolderPath", ref sourceFolderPath);
		dict.SetStringIfExists("atlasFilePath", ref atlasFilePath);
		dict.SetBoolIfExists("shouldGenerate", ref shouldGenerate);
		dict.SetBoolIfExists("shouldFoldout", ref shouldFoldout);
		dict.SetBoolIfExists("shouldAutoGenerate", ref shouldAutoGenerate);
		dict.SetBoolIfExists("shouldAddSubfolders", ref shouldAddSubfolders);
		dict.SetBoolIfExists("shouldUseBytes", ref shouldUseBytes);
		dict.SetFloatIfExists("scale", ref scale);
		dict.SetBoolIfExists("shouldTrim", ref shouldTrim);
		dict.SetIntIfExists("padding", ref padding);
		dict.SetIntIfExists("extrude", ref extrude);
		dict.SetIntIfExists("trimPadding", ref trimPadding);
	}
	
	public string GetJSONString ()
	{
		StringBuilder stringBuilder = new StringBuilder("{\n");
		
		stringBuilder.Append("\t\"sourceFolderPath\":\""+sourceFolderPath+"\",\n");
		stringBuilder.Append("\t\"atlasFilePath\":\""+atlasFilePath+"\",\n");
		stringBuilder.Append("\t\"shouldGenerate\":\""+shouldGenerate.ToString()+"\",\n");
		stringBuilder.Append("\t\"shouldFoldout\":\""+shouldFoldout.ToString()+"\",\n");
		stringBuilder.Append("\t\"shouldAutoGenerate\":\""+shouldAutoGenerate.ToString()+"\",\n");
		stringBuilder.Append("\t\"shouldAddSubfolders\":\""+shouldAddSubfolders.ToString()+"\",\n");
		stringBuilder.Append("\t\"shouldUseBytes\":\""+shouldUseBytes.ToString()+"\",\n");
		stringBuilder.Append("\t\"scale\":\""+scale.ToString()+"\"");
		stringBuilder.Append("\t\"shouldTrim\":\""+shouldTrim.ToString()+"\"");
		stringBuilder.Append("\t\"padding\":\""+padding.ToString()+"\"");
		stringBuilder.Append("\t\"extrude\":\""+extrude.ToString()+"\"");
		stringBuilder.Append("\t\"trimPadding\":\""+trimPadding.ToString()+"\"");
		
		stringBuilder.Append("\n}");
		
		return stringBuilder.ToString();
	}
	
	public PRAtlasLink GetDuplicate()
	{
		PRAtlasLink link = new PRAtlasLink(sourceFolderPath,atlasFilePath);
		link.shouldGenerate = shouldGenerate;
		link.shouldFoldout = shouldFoldout;
		link.shouldAutoGenerate = shouldAutoGenerate;
		link.shouldAddSubfolders = shouldAddSubfolders;
		link.shouldUseBytes = shouldUseBytes;
		link.scale = scale;
		link.shouldTrim = shouldTrim;
		link.padding = padding;
		link.extrude = extrude;
		link.trimPadding = trimPadding;
		return link;
	}
}
