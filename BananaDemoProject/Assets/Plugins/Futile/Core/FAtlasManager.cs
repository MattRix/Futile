using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FAtlasManager
{
	private List<FAtlas> _atlases = new List<FAtlas>();
	
	private List<FAtlasElement> _allElements = new List<FAtlasElement>();
	
	private Dictionary<string, FAtlasElement> _allElementsByName = new Dictionary<string, FAtlasElement>();
	
	private List<FFont> _fonts = new List<FFont>();
	private Dictionary<string,FFont> _fontsByName = new Dictionary<string, FFont>();
	
	public FAtlasManager () //new DAtlasManager() called by FEngine
	{
		
	}
	
	//images and atlases are both treated as atlases
	private void LoadAtlasOrImage(string atlasPath, bool shouldLoadAsSingleImage)
	{
		FAtlas atlas = new FAtlas(atlasPath+FEngine.resourceSuffix, _atlases.Count, shouldLoadAsSingleImage);
		
		foreach(FAtlasElement element in atlas.elements)
		{
			element.indexInManager = _allElements.Count;
			element.atlas = atlas;
			element.atlasIndex = atlas.index;
			
			_allElements.Add(element);
			_allElementsByName.Add (element.name, element);
		}
		
		_atlases.Add(atlas); 
	}
	
	public void LoadAtlas(string atlasPath)
	{
		LoadAtlasOrImage(atlasPath,false);
	}
	
	public void LoadImage(string imagePath)
	{
		LoadAtlasOrImage(imagePath,true);
	}

	public FAtlasElement GetElementWithName (string elementName)
	{
		return _allElementsByName[elementName];
	}
	
	public FFont GetFontWithName(string fontName)
	{
		return _fontsByName[fontName];	
	}

	public void LoadFont (string name, string elementName, string configPath)
	{
		LoadFont (name,elementName,configPath,new FTextParams());
	}
	
	public void LoadFont (string name, string elementName, string configPath, FTextParams fontTextParams)
	{
		FAtlasElement element = _allElementsByName[elementName];
		FFont font = new FFont(name,element,configPath, fontTextParams);
		
		_fonts.Add(font);
		_fontsByName.Add (name, font);
	}
}


