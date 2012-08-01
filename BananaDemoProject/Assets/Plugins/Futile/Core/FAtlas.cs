using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FAtlasElement
{
	public string name;
	
	public int indexInAtlas;
	public int indexInManager;

	public FAtlas atlas;
	public int atlasIndex;
	
	public Rect uvRect;
	public Vector2 uvTopLeft;
	public Vector2 uvTopRight;
	public Vector2 uvBottomRight;
	public Vector2 uvBottomLeft;
	
	public Rect sourceRect;
	public bool trimmed;
	public bool rotated;
}

public class FAtlas
{
	private string _atlasPath;
	private string _fullPath;
	
	private int _index;
	
	private List<FAtlasElement> _elements = new List<FAtlasElement>();
	
	private Dictionary<string, FAtlasElement> _elementsByName = new Dictionary<string, FAtlasElement>();	
	
	private Texture _texture;
	private Vector2 _textureSize;
	
	public FAtlas (string atlasPath, int index)
	{
		_atlasPath = atlasPath;
		_index = index;
		
		_fullPath = _atlasPath+"_Scale"+FEngine.scale;
		
		LoadTexture();
		LoadData();
	}
	
	private void LoadTexture()
	{
		_texture = (Texture) Resources.Load (_fullPath, typeof(Texture));
		 
		if(!_texture)
		{
			Debug.Log ("FEngine: Couldn't load the atlas texture from: " + _fullPath);	
		}
		
		_textureSize = new Vector2(_texture.width,_texture.height);
	}
	
	private void LoadData()
	{
		_elementsByName = new Dictionary<string, FAtlasElement>();
		
		TextAsset dataAsset = (TextAsset) Resources.Load (_fullPath, typeof(TextAsset));
		
		if(!dataAsset)
		{
			Debug.Log ("FEngine: Couldn't load the atlas data from: " + _fullPath + ", so loading it as a single image instead");
			
			LoadAsSingleImage();
			return;
		}
		
		Hashtable hash = dataAsset.text.hashtableFromJson();
		
		IDictionary frames = (IDictionary)hash["frames"];
		
		float scaleInverse = FEngine.scaleInverse;
		
		float uvOffsetX;
		float uvOffsetY;
		
		if(FEngine.isOpenGL)
		{
			uvOffsetX = 0.0f/_textureSize.x;;
			uvOffsetY = 0.0f/_textureSize.y;
		}
		else
		{
			uvOffsetX = 0.5f/_textureSize.x;;
			uvOffsetY = -0.5f/_textureSize.y;
		}
		
		int index = 0;
		
		foreach(DictionaryEntry item in frames)
		{
			FAtlasElement element = new FAtlasElement();
			 
			element.indexInAtlas = index++;
			element.name = (string) item.Key;
			
			IDictionary itemDict = (IDictionary)item.Value;
			
			IDictionary frame = (IDictionary)itemDict["frame"];
			float rectX = float.Parse(frame["x"].ToString());
			float rectY = float.Parse(frame["y"].ToString());
			float rectW = float.Parse(frame["w"].ToString());
			float rectH = float.Parse(frame["h"].ToString()); 
			
			
			Rect uvRect = new Rect
			(
				rectX/_textureSize.x + uvOffsetX,
				((_textureSize.y - rectY - rectH)/_textureSize.y)+uvOffsetY,
				rectW/_textureSize.x,
				rectH/_textureSize.y
			);
			
			element.uvRect = uvRect;
			
			element.uvTopLeft.Set(uvRect.xMin,uvRect.yMax);
			element.uvTopRight.Set(uvRect.xMax,uvRect.yMax);
			element.uvBottomRight.Set(uvRect.xMax,uvRect.yMin);
			element.uvBottomLeft.Set(uvRect.xMin,uvRect.yMin);
			
			//Debug.Log (element.name + " UVRECT ymax " + element.uvRect.yMax + " uvrect ymin " + element.uvRect.yMin);
					
			IDictionary sourceRect = (IDictionary)itemDict["spriteSourceSize"];
			rectX = float.Parse(sourceRect["x"].ToString()) * scaleInverse;
			rectY = float.Parse(sourceRect["y"].ToString()) * scaleInverse;
			rectW = float.Parse(sourceRect["w"].ToString()) * scaleInverse;
			rectH = float.Parse(sourceRect["h"].ToString()) * scaleInverse;
			
			element.sourceRect = new Rect(rectX,rectY,rectW,rectH);
			 
			element.trimmed = (bool)itemDict["trimmed"];
			element.rotated = (bool)itemDict["rotated"];
			
			_elements.Add (element);
			_elementsByName.Add(element.name, element);
		}
		
		Resources.UnloadAsset(dataAsset);
	}
	
	private void LoadAsSingleImage()
	{
		FAtlasElement element = new FAtlasElement();
		
		element.name = _atlasPath;
		element.indexInAtlas = 0;
		
		//TODO: may have to offset the rect slightly
		float scaleInverse = FEngine.scaleInverse;
		
		float uvOffsetX;
		float uvOffsetY;
		
		if(FEngine.isOpenGL)
		{
			uvOffsetX = 0.0f/_textureSize.x;;
			uvOffsetY = 0.0f/_textureSize.y;
		}
		else
		{
			uvOffsetX = 0.5f/_textureSize.x;;
			uvOffsetY = -0.5f/_textureSize.y;
		}
		
		Rect uvRect = new Rect(0.0f+uvOffsetX,0.0f+uvOffsetY,1.0f,1.0f);
		
		element.uvRect = uvRect;
		
		element.uvTopLeft.Set(uvRect.xMin,uvRect.yMax);
		element.uvTopRight.Set(uvRect.xMax,uvRect.yMax);
		element.uvBottomRight.Set(uvRect.xMax,uvRect.yMin);
		element.uvBottomLeft.Set(uvRect.xMin,uvRect.yMin);
		
		element.sourceRect = new Rect(0,0,_textureSize.x*scaleInverse,_textureSize.y*scaleInverse);
		
		element.trimmed = false;
		element.rotated = false;
		
		_elements.Add (element);
		_elementsByName.Add (element.name, element);
	}
	
	public List<FAtlasElement> elements
	{
		get {return _elements;}	
	}
	
	public int index
	{
		get {return _index;}	
	}
	
	public Texture texture
	{
		get {return _texture;}	
	}
	
	public Vector2 textureSize
	{
		get {return _textureSize;}	
	}
	
	public string atlasPath
	{
		get {return _atlasPath;}	
	}
}


