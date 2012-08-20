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
	
	public void ActuallyLoadAtlasOrImage(string name, string imagePath, string dataPath)
	{
		int atlasCount = _atlases.Count;
		for(int a = 0; a<atlasCount; ++a)
		{
			if(_atlases[a].name == name) return; //don't load this atlas if we already have one with the same name
		}
		
		//if dataPath is empty, load it as a single image
		bool isSingleImage = (dataPath == "");
		
		FAtlas atlas = new FAtlas(name, imagePath, dataPath, _atlases.Count, isSingleImage);
		
		int elementCount = atlas.elements.Count;
		for(int e = 0; e<elementCount; ++e)
		{
			FAtlasElement element = atlas.elements[e];
			
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
		ActuallyLoadAtlasOrImage(atlasPath, atlasPath+Futile.resourceSuffix, atlasPath+Futile.resourceSuffix);
	}

	public void LoadImage(string imagePath)
	{
		ActuallyLoadAtlasOrImage(imagePath, imagePath+Futile.resourceSuffix,"");
	}
	
	public void ActuallyUnloadAtlasOrImage(string name)
	{
		int atlasCount = _atlases.Count;
		for(int a = 0; a<atlasCount; ++a)
		{
			FAtlas atlas = _atlases[a];
			
			if(atlas.name == name)
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
		ActuallyUnloadAtlasOrImage(atlasPath);
	}
	
	public void UnloadImage(string imagePath)
	{
		ActuallyUnloadAtlasOrImage(imagePath);	
	}

	public FAtlasElement GetElementWithName (string elementName)
	{
		if(_allElementsByName.ContainsKey(elementName))
		{
			return _allElementsByName[elementName];
		}
		throw new Exception("Couldn't find element named '"+elementName+"'");
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
		FAtlasElement element = GetElementWithName(elementName);
		FFont font = new FFont(name,element,configPath, fontTextParams);
	
		_fonts.Add(font);
		_fontsByName.Add (name, font);
	}
}


