using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FAtlasManager
{
	static private int _nextAtlasIndex;
	
	private List<FAtlas> _atlases = new List<FAtlas>();
	
	private Dictionary<string, FAtlasElement> _allElementsByName = new Dictionary<string, FAtlasElement>();
	
	private List<FFont> _fonts = new List<FFont>();
	private Dictionary<string,FFont> _fontsByName = new Dictionary<string, FFont>();
	
	public FAtlasManager () //new DAtlasManager() called by Futile
	{
		
	}
	
	public FAtlas GetAtlasWithName(string name)
	{
		int atlasCount = _atlases.Count;
		for(int a = 0; a<atlasCount; ++a)
		{
			if(_atlases[a].name == name) return _atlases[a];
		}
		return null;
	}
	
	public bool DoesContainAtlas(string name)
	{
		int atlasCount = _atlases.Count;
		for(int a = 0; a<atlasCount; ++a)
		{
			if(_atlases[a].name == name) return true;
		}
		return false;
	}

	public void LoadAtlasFromTexture (string name, Texture texture)
	{
		if(DoesContainAtlas(name)) return; //we already have it, don't load it again
		
		FAtlas atlas = new FAtlas(name, texture, _nextAtlasIndex++);
		
		AddAtlas(atlas);
	}
	
	public void LoadAtlasFromTexture (string name, string dataPath, Texture texture)
	{
		if(DoesContainAtlas(name)) return; //we already have it, don't load it again
		
		FAtlas atlas = new FAtlas(name, dataPath, texture, _nextAtlasIndex++);
		
		AddAtlas(atlas);
	}
	
	public void ActuallyLoadAtlasOrImage(string name, string imagePath, string dataPath)
	{
		if(DoesContainAtlas(name)) return; //we already have it, don't load it again
		
		//if dataPath is empty, load it as a single image
		bool isSingleImage = (dataPath == "");
		
		FAtlas atlas = new FAtlas(name, imagePath, dataPath, _nextAtlasIndex++, isSingleImage);
		
		AddAtlas(atlas);
	}
	
	private void AddAtlas(FAtlas atlas)
	{
		int elementCount = atlas.elements.Count;
		for(int e = 0; e<elementCount; ++e)
		{
			FAtlasElement element = atlas.elements[e];
			
			element.atlas = atlas;
			element.atlasIndex = atlas.index;
			
			if(_allElementsByName.ContainsKey(element.name))
			{
                throw new FutileException("Duplicate element name '" + element.name +"' found! All element names must be unique!");	
			}
			else 
			{
				_allElementsByName.Add (element.name, element);
			}
		}
		
		_atlases.Add(atlas); 
	}
	
	public void LoadAtlas(string atlasPath)
	{
		if(DoesContainAtlas(atlasPath)) return; //we already have it, don't load it again
		
		string filePath = atlasPath+Futile.resourceSuffix+"_png";
		
		TextAsset imageBytes = Resources.Load (filePath, typeof(TextAsset)) as TextAsset;
		
		if(imageBytes != null) //do we have png bytes?
		{
			Texture2D texture = new Texture2D(0,0,TextureFormat.ARGB32,false);
			
			texture.LoadImage(imageBytes.bytes);
			
			Resources.UnloadAsset(imageBytes);
			
			LoadAtlasFromTexture(atlasPath,atlasPath+Futile.resourceSuffix, texture);
		}
		else //load it as a normal Unity image asset
		{
			ActuallyLoadAtlasOrImage(atlasPath, atlasPath+Futile.resourceSuffix, atlasPath+Futile.resourceSuffix);
		}
	}
	
	public void LoadImage(string imagePath)
	{
		if(DoesContainAtlas(imagePath)) return; //we already have it
		
		string filePath = imagePath+Futile.resourceSuffix+"_png";
		
		TextAsset imageBytes = Resources.Load (filePath, typeof(TextAsset)) as TextAsset;
		
		if(imageBytes != null) //do we have png bytes?
		{
			Texture2D texture = new Texture2D(0,0,TextureFormat.ARGB32,false);
			
			texture.LoadImage(imageBytes.bytes);
			
			Resources.UnloadAsset(imageBytes);
			
			LoadAtlasFromTexture(imagePath, texture);
		}
		else //load it as a normal Unity image asset
		{
			ActuallyLoadAtlasOrImage(imagePath, imagePath+Futile.resourceSuffix,"");
		}
	}
	
	public void ActuallyUnloadAtlasOrImage(string name)
	{
		bool wasAtlasRemoved = false;
		
		int atlasCount = _atlases.Count;
		
		for(int a = atlasCount-1; a>=0; a--) //reverse order so deletions ain't no thang
		{
			FAtlas atlas = _atlases[a];
			
			if(atlas.name == name)
			{
				int elementCount = atlas.elements.Count;
				
				for(int e = 0; e<elementCount; e++)
				{
					_allElementsByName.Remove(atlas.elements[e].name);	
				}
				
				atlas.Unload();
				_atlases.RemoveAt(a);
				
				wasAtlasRemoved = true;
			}
		}
		
		if(wasAtlasRemoved)
		{
			Futile.stage.renderer.Clear();
			Resources.UnloadUnusedAssets();
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
		if (_allElementsByName.ContainsKey(elementName))
        {
            return _allElementsByName [elementName];
        } 
        else
        {
            //Try to make an educated guess about what they were trying to load
            //First we get the last part of the path (the file name) and then we remove the extension
            //Then we check to see if that string is in any of our element names 
            //(perhaps they have the path wrong or are mistakenly using a .png extension)

            String lastChunk = null;

            if(elementName.Contains("\\"))
            {
                String[] chunks = elementName.Split('\\');
                lastChunk = chunks[chunks.Length-1];
            }
            else
            {
                String[] chunks = elementName.Split('/');
                lastChunk = chunks[chunks.Length-1];
            }

            String replacementName = null;

            if(lastChunk != null)
            {
                lastChunk = lastChunk.Split('.')[0]; //remove the extension

                foreach(KeyValuePair<String, FAtlasElement> pair in _allElementsByName)
                {
                    if(pair.Value.name.Contains(lastChunk))
                    {
                        replacementName = pair.Value.name;
                    }
                }
             }

            if(replacementName == null)
            {
                throw new FutileException("Couldn't find element named '" + elementName + "'. \nUse Futile.atlasManager.LogAllElementNames() to see a list of all loaded elements names");
            }
            else 
            {
                throw new FutileException("Couldn't find element named '" + elementName + "'. Did you mean '" + replacementName + "'? \nUse Futile.atlasManager.LogAllElementNames() to see a list of all loaded element names.");
            }
        }
	}
	
	public FFont GetFontWithName(string fontName)
	{
		if(_fontsByName.ContainsKey(fontName))
		{
            return _fontsByName[fontName];  
        }
        else 
        {
            throw new FutileException("Couldn't find font named '"+fontName+"'");
		}
	}

	public void LoadFont (string name, string elementName, string configPath, float offsetX, float offsetY)
	{
		LoadFont (name,elementName,configPath, offsetX, offsetY, new FTextParams());
	}
	
	public void LoadFont (string name, string elementName, string configPath, float offsetX, float offsetY, FTextParams textParams)
	{
		FAtlasElement element = GetElementWithName(elementName);
		FFont font = new FFont(name,element,configPath, offsetX, offsetY, textParams);
	
		_fonts.Add(font);
		_fontsByName.Add (name, font);
	}

	public void LogAllElementNames()
	{
		Debug.Log("Logging all element names:");

		foreach(KeyValuePair<String,FAtlasElement> pair in _allElementsByName)
		{
			Debug.Log("'"+pair.Value.name+"'");
		}
	}
}


