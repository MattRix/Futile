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
	
	public FAtlasManager () //new DAtlasManager() called by Futile
	{
		
	}
	
	//images and atlases are both treated as atlases
	private void LoadAtlasOrImage(string atlasPath, bool shouldLoadAsSingleImage)
	{
		FAtlas atlas = new FAtlas(atlasPath+Futile.resourceSuffix, _atlases.Count, shouldLoadAsSingleImage);
		
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
	
	private void UnloadAtlasOrImage(string atlasPath)
	{
		string fullPath = atlasPath+Futile.resourceSuffix;
		
		for(int a = 0; a<_atlases.Count; a++)
		{
			FAtlas atlas = _atlases[a];
			
			if(atlas.atlasPath == fullPath)
			{
				for(int e = _allElements.Count-1; e>=0; e--)
				{
					FAtlasElement element = _allElements[e];
					
					if(element.atlas == atlas)
					{
						_allElements.RemoveAt(e);	
						_allElementsByName.Remove(element.name);
					}
				}
				
				atlas.Unload();
				_atlases.RemoveAt(a);
			}
		}
	}
	
	public void UnloadAtlas(string atlasPath)
	{
		UnloadAtlasOrImage(atlasPath);
	}
	
	public void UnloadImage(string imagePath)
	{
		UnloadAtlasOrImage(imagePath);	
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


