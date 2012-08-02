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
	
	private string _resourceSuffix;
	
	public FAtlasManager (string resourceSuffix) //new DAtlasManager() called by FEngine
	{
		_resourceSuffix = resourceSuffix;
	}
	
	//images and atlases are both treated as atlases
	private void LoadAtlasOrImage(string atlasPath, bool shouldLoadAsSingleImage)
	{
		FAtlas atlas = new FAtlas(atlasPath+_resourceSuffix, _atlases.Count, shouldLoadAsSingleImage);
		
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

	public void LoadFont (string name, string elementName, string configPath, float defaultLineHeight, float defaultLetterSpacing)
	{
		FAtlasElement element = _allElementsByName[elementName];
		FFont font = new FFont(name,element,configPath, defaultLineHeight, defaultLetterSpacing);
		
		_fonts.Add(font);
		_fontsByName.Add (name, font);
	}
}


