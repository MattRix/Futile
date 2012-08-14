using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//parts of this were inspired by https://github.com/prime31/UIToolkit/blob/master/Assets/Plugins/UIToolkit/UIElements/UIText.cs

public class FLabel : FQuadNode
{
	protected FFont _font;
	protected string _fontName;
	protected string _text;
	
	protected Color _color = Color.white;
	protected Color _alphaColor = Color.white;
	
	protected FLetterQuadLine[] _letterQuadLines;
	
	protected bool _isMeshDirty = false;
	
	protected float _anchorX = 0.5f;
	protected float _anchorY = 0.5f;
	
	protected float _lineHeightDelta;
	protected float _letterSpacingDelta;
	
	protected bool _doesTextNeedUpdate = false;
	protected bool _doesLocalPositionNeedUpdate = false;
	
	protected Rect _boundsRect;
	
	protected FTextParams _textParams;
	
	public FLabel (string fontName, string text) : this(fontName, text, new FTextParams())
	{
	}
	
	public FLabel (string fontName, string text, FTextParams textParams) : base()
	{
		_fontName = fontName;
		_text = text;
		_font = Futile.atlasManager.GetFontWithName(_fontName);
		_textParams = textParams;
		 
		Init(_font.element, 0);
		
		CreateTextQuads();
	}
	
	public void CreateTextQuads()
	{
		_doesTextNeedUpdate = false;
		
		int oldQuadsNeeded = _numberOfQuadsNeeded;
		
		_letterQuadLines = _font.GetQuadInfoForText(_text,_textParams);
		
		_numberOfQuadsNeeded = 0;
		
		foreach(FLetterQuadLine line in _letterQuadLines)
		{
			_numberOfQuadsNeeded += line.quads.Length;
		}
		
		if(_isOnStage)
		{
			int delta = _numberOfQuadsNeeded - oldQuadsNeeded;
			
			if(delta != 0) //if the number of letter quads has changed, tell the stage
			{
				_stage.HandleQuadsChanged();
			}
		}
		
		UpdateLocalPosition(); //figures out the bounds and alignment, and sets the mesh dirty
	}
	
	public void UpdateLocalPosition()
	{
		_doesLocalPositionNeedUpdate = false;
		
		float minY = 100000000;
		float maxY = -100000000;
		
		float minX = 100000000;
		float maxX = -100000000;
		
		foreach(FLetterQuadLine line in _letterQuadLines)
		{
			minY = Math.Min (line.bounds.yMin,minY);
			maxY = Math.Max (line.bounds.yMax,maxY);
		}
		
		float offsetY = -(minY + ((maxY-minY)*_anchorY));
		
		foreach(FLetterQuadLine line in _letterQuadLines)
		{
			float offsetX = -line.bounds.width*_anchorX;
			
			minX = Math.Min (offsetX,minX);
			maxX = Math.Max (offsetX+line.bounds.width,maxX);
			
			foreach(FLetterQuad quad in line.quads)
			{
				quad.CalculateVectors(offsetX, offsetY);
			}
		}
		
		_boundsRect.x = minX;
		_boundsRect.y = minY+offsetY;
		_boundsRect.width = maxX-minX;
		_boundsRect.height = maxY-minY;
		
		_isMeshDirty = true; 
	}

	override public void Redraw(bool shouldForceDirty, bool shouldUpdateDepth)
	{
		bool wasMatrixDirty = _isMatrixDirty;
		bool wasAlphaDirty = _isAlphaDirty;
		
		UpdateDepthMatrixAlpha(shouldForceDirty, shouldUpdateDepth);
		
		if(_doesTextNeedUpdate)
		{
			CreateTextQuads();
		}
		
		if(shouldUpdateDepth)
		{
			UpdateQuads();
		}
		
		if(wasMatrixDirty || shouldForceDirty || shouldUpdateDepth)
		{
			_isMeshDirty = true;
		}
		
		if(wasAlphaDirty || shouldForceDirty)
		{
			_isMeshDirty = true;
			_alphaColor = _color.CloneWithMultipliedAlpha(_concatenatedAlpha);	
		}

		if(_doesLocalPositionNeedUpdate)
		{
			UpdateLocalPosition();	
		}
		
		if(_isMeshDirty)
		{
			PopulateRenderLayer();
		}
	}
	
	override public void PopulateRenderLayer()
	{
		if(_isOnStage && _firstQuadIndex != -1)
		{
			_isMeshDirty = false;
			
			Vector3[] vertices = _renderLayer.vertices;
			Vector2[] uvs = _renderLayer.uvs;
			Color[] colors = _renderLayer.colors;
			
			int vertexIndex = _firstQuadIndex*4;
			
			foreach(FLetterQuadLine line in _letterQuadLines)
			{
				foreach(FLetterQuad quad in line.quads)
				{
					_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex], quad.topLeft,0);
					_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex + 1], quad.topRight,0);
					_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex + 2], quad.bottomRight,0);
					_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex + 3], quad.bottomLeft,0);
					
					uvs[vertexIndex] = quad.charInfo.uvTopLeft;
					uvs[vertexIndex + 1] = quad.charInfo.uvTopRight;
					uvs[vertexIndex + 2] = quad.charInfo.uvBottomRight;
					uvs[vertexIndex + 3] = quad.charInfo.uvBottomLeft;
					
					colors[vertexIndex] = _alphaColor;
					colors[vertexIndex + 1] = _alphaColor;
					colors[vertexIndex + 2] = _alphaColor;
					colors[vertexIndex + 3] = _alphaColor;
					
					vertexIndex += 4;
				}
			}
			
			//TODO: maybe we can NOT call this when doing a depth populate, because we know it won't matter
			_renderLayer.HandleVertsChange();
		}
	}
	
	public string text
	{
		get {return _text;}
		set 
		{
			if(_text != value)
			{
				_text = value; 
				_doesTextNeedUpdate = true;
			}
		}
	}
	
	public float anchorX
	{
		get {return _anchorX;}
		set 
		{
			if(_anchorX != value)
			{
				_anchorX = value;
				_doesLocalPositionNeedUpdate = true;
			}
		}
	}

	public float anchorY
	{
		get {return _anchorY;}
		set 
		{
			if(_anchorY != value)
			{
				_anchorY = value;
				_doesLocalPositionNeedUpdate = true;
			}
		}
	}

	public float lineHeightDelta
	{
		get {return _lineHeightDelta;}
		set 
		{
			if(_lineHeightDelta != value)
			{
				_lineHeightDelta = value;
				_doesTextNeedUpdate = true;
			}
		}
	}

	public float letterSpacingDelta
	{
		get {return _letterSpacingDelta;}
		set 
		{
			if(_letterSpacingDelta != value)
			{
				_letterSpacingDelta = value;
				_doesTextNeedUpdate = true;
			}
		}
	}
	
	virtual public Color color 
	{
		get { return _color; }
		set 
		{ 
			if(_color != value)
			{
				_color = value; 
				_isAlphaDirty = true;
			}
		}
	}
	
	virtual public Rect boundsRect
	{
		get {return _boundsRect;}	
	}
	
	
}

