using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FAtlasElement
{
	public string name;
	
	public int indexInAtlas;

	public FAtlas atlas;
	public int atlasIndex;
	
	public Rect uvRect;
	public Vector2 uvTopLeft;
	public Vector2 uvTopRight;
	public Vector2 uvBottomRight;
	public Vector2 uvBottomLeft;
	
	public Rect sourceRect;
	public Vector2 sourceSize;
	public bool isTrimmed;
	//public bool isRotated;
	
	public FAtlasElement Clone()
	{
		FAtlasElement element = new FAtlasElement();
		
		element.name = name;
		
		element.indexInAtlas = indexInAtlas;
		
		element.atlas = atlas;
		element.atlasIndex = atlasIndex;
		
		element.uvRect = uvRect;
		element.uvTopLeft = uvTopLeft;
		element.uvTopRight = uvTopRight;
		element.uvBottomRight = uvBottomRight;
		element.uvBottomLeft = uvBottomLeft;
		
		element.sourceRect = sourceRect;
		element.sourceSize = sourceSize;
		element.isTrimmed = isTrimmed;
		
		return element;
	}
}

public class FAtlas
{
	private string _name;
	private string _imagePath;
	private string _dataPath;
	
	private int _index;
	
	private List<FAtlasElement> _elements = new List<FAtlasElement>();
	
	private Dictionary<string, FAtlasElement> _elementsByName = new Dictionary<string, FAtlasElement>();	
	
	private Texture _texture;
	private Vector2 _textureSize;
	
	private bool _isSingleImage;
	
	private bool _isTextureAnAsset = false;
	
	//TODO: allow users to pass a dictionary of pre-built atlas data if they want
	public FAtlas (string name, Texture texture, int index) //single image
	{
		_name = name;
		_imagePath = "";
		_dataPath = "";
		_index = index;
		
		_texture = texture;
		_textureSize = new Vector2(_texture.width,_texture.height);
		
		CreateAtlasFromSingleImage();
	}
	
	public FAtlas (string name, string dataPath, Texture texture, int index) //atlas with data path
	{
		_name = name;
		_imagePath = "";
		_dataPath = dataPath;
		_index = index;
		
		_texture = texture;
		_textureSize = new Vector2(_texture.width,_texture.height);
		
		_isSingleImage = false;
		LoadAtlasData();
	}
	
	public FAtlas (string name, string imagePath, string dataPath, int index, bool shouldLoadAsSingleImage)
	{
		_name = name;
		_imagePath = imagePath;
		_dataPath = dataPath;
		
		_index = index;
		
		LoadTexture();
		
		if(shouldLoadAsSingleImage)
		{
			_isSingleImage = true;
			CreateAtlasFromSingleImage();
		}
		else
		{
			_isSingleImage = false;
			LoadAtlasData();
		}
	}
	
	private void LoadTexture()
	{
		_texture = Resources.Load (_imagePath, typeof(Texture)) as Texture;
		 
		if(_texture == null)
		{
			throw new FutileException("Couldn't load the atlas texture from: " + _imagePath);	
		}
		
		_isTextureAnAsset = true;
		
		_textureSize = new Vector2(_texture.width,_texture.height);
	}
	
	private void LoadAtlasData()
	{
		TextAsset dataAsset = Resources.Load (_dataPath, typeof(TextAsset)) as TextAsset;
		
		if(dataAsset == null)
		{
			throw new FutileException("Couldn't load the atlas data from: " + _dataPath);
		}
		
		Dictionary<string,object> dict = dataAsset.text.dictionaryFromJson();
		
		if(dict == null)
		{
			throw new FutileException("The atlas at " + _dataPath + " was not a proper JSON file. Make sure to select \"Unity3D\" in TexturePacker.");
		}
		
		Dictionary<string,object> frames = (Dictionary<string,object>) dict["frames"];
		
		float scaleInverse = Futile.resourceScaleInverse;
		
		int index = 0;
		
		foreach(KeyValuePair<string,object> item in frames)
		{
			FAtlasElement element = new FAtlasElement();
			 
			element.indexInAtlas = index++;
			
			string name = (string) item.Key;
			
			if(Futile.shouldRemoveAtlasElementFileExtensions)
			{
				int extensionPosition = name.LastIndexOf(".");
				if (extensionPosition >= 0) name = name.Substring(0, extensionPosition);
			}

			element.name = name;
			
			IDictionary itemDict = (IDictionary)item.Value;
			
			element.isTrimmed = (bool)itemDict["trimmed"];
			
			if((bool)itemDict["rotated"]) 
			{
				throw new NotSupportedException("Futile no longer supports TexturePacker's \"rotated\" flag. Please disable it when creating the "+_dataPath+" atlas.");
			}
			
			IDictionary frame = (IDictionary)itemDict["frame"];
			
			float rectX = float.Parse(frame["x"].ToString());
			float rectY = float.Parse(frame["y"].ToString());
			float rectW = float.Parse(frame["w"].ToString());
			float rectH = float.Parse(frame["h"].ToString()); 
			
			Rect uvRect = new Rect
			(
				rectX/_textureSize.x,
				((_textureSize.y - rectY - rectH)/_textureSize.y),
				rectW/_textureSize.x,
				rectH/_textureSize.y
			);
				
			element.uvRect = uvRect;
		
			element.uvTopLeft.Set(uvRect.xMin,uvRect.yMax);
			element.uvTopRight.Set(uvRect.xMax,uvRect.yMax);
			element.uvBottomRight.Set(uvRect.xMax,uvRect.yMin);
			element.uvBottomLeft.Set(uvRect.xMin,uvRect.yMin);
			
			
			IDictionary sourceRect = (IDictionary)itemDict["spriteSourceSize"];

			rectX = float.Parse(sourceRect["x"].ToString()) * scaleInverse;
			rectY = float.Parse(sourceRect["y"].ToString()) * scaleInverse;
			rectW = float.Parse(sourceRect["w"].ToString()) * scaleInverse;
			rectH = float.Parse(sourceRect["h"].ToString()) * scaleInverse;
			
			element.sourceRect = new Rect(rectX,rectY,rectW,rectH);

			
			IDictionary sourceSize = (IDictionary)itemDict["sourceSize"];
			element.sourceSize.x = float.Parse(sourceSize["w"].ToString()) * scaleInverse;	
			element.sourceSize.y = float.Parse(sourceSize["h"].ToString()) * scaleInverse;	
			
			_elements.Add (element);
			_elementsByName.Add(element.name, element);
		}
		
		Resources.UnloadAsset(dataAsset);
	}
	
	private void CreateAtlasFromSingleImage()
	{
		FAtlasElement element = new FAtlasElement();
		
		element.name = _name;
		element.indexInAtlas = 0;
		
		//TODO: may have to offset the rect slightly
		float scaleInverse = Futile.resourceScaleInverse;
		
		Rect uvRect = new Rect(0.0f,0.0f,1.0f,1.0f);
		
		element.uvRect = uvRect;
		
		element.uvTopLeft.Set(uvRect.xMin,uvRect.yMax);
		element.uvTopRight.Set(uvRect.xMax,uvRect.yMax);
		element.uvBottomRight.Set(uvRect.xMax,uvRect.yMin);
		element.uvBottomLeft.Set(uvRect.xMin,uvRect.yMin);
		
		element.sourceRect = new Rect(0,0,_textureSize.x*scaleInverse,_textureSize.y*scaleInverse);
		
		element.sourceSize = new Vector2(_textureSize.x*scaleInverse,_textureSize.y*scaleInverse);
		element.isTrimmed = false;
		
		_elements.Add (element);
		_elementsByName.Add (element.name, element);
	}

	public void Unload ()
	{
		if(_isTextureAnAsset)
		{
			Resources.UnloadAsset(_texture);
		}
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
	
	public string name
	{
		get {return _name;}	
	}
	
	public string imagePath
	{
		get {return _imagePath;}	
	}
	
	public string dataPath
	{
		get {return _dataPath;}	
	}
	
	public bool isSingleImage
	{
		get {return _isSingleImage;}	
	}
}


