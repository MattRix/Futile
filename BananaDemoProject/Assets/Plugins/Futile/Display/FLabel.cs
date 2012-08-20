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
	
	protected Rect _textRect;
	
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
		
		int lineCount = _letterQuadLines.Length;
		for(int i = 0; i< lineCount; i++)
		{
			_numberOfQuadsNeeded += _letterQuadLines[i].quads.Length;
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
		
		int lineCount = _letterQuadLines.Length;
		for(int i = 0; i<lineCount; i++)
		{
			FLetterQuadLine line = _letterQuadLines[i];
			minY = Math.Min (line.bounds.yMin,minY);
			maxY = Math.Max (line.bounds.yMax,maxY);
		}
		
		float offsetY = -(minY + ((maxY-minY)*_anchorY));
		
		for(int i = 0; i<lineCount; i++)
		{
			FLetterQuadLine line = _letterQuadLines[i];
			float offsetX = -line.bounds.width*_anchorX;
			
			minX = Math.Min (offsetX,minX);
			maxX = Math.Max (offsetX+line.bounds.width,maxX);
			
			int quadCount = line.quads.Length;
			for(int q = 0; q< quadCount; q++)
			{
				line.quads[q].CalculateVectors(offsetX, offsetY);
			}
		}
		
		_textRect.x = minX;
		_textRect.y = minY+offsetY;
		_textRect.width = maxX-minX;
		_textRect.height = maxY-minY;
		
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
			
			int vertexIndex0 = _firstQuadIndex*4;
			int vertexIndex1 = vertexIndex0 + 1;
			int vertexIndex2 = vertexIndex0 + 2;
			int vertexIndex3 = vertexIndex0 + 3;
			
			int lineCount = _letterQuadLines.Length;
			for(int i = 0; i<lineCount; i++)
			{
				FLetterQuad[] quads = _letterQuadLines[i].quads;
				
				int quadCount = quads.Length;
				for(int q = 0; q<quadCount; q++)
				{
					FLetterQuad quad = quads[q];
					FCharInfo charInfo = quad.charInfo;
					
					_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex0], quad.topLeft,0);
					_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex1], quad.topRight,0);
					_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex2], quad.bottomRight,0);
					_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex3], quad.bottomLeft,0);
					
					uvs[vertexIndex0] = charInfo.uvTopLeft;
					uvs[vertexIndex1] = charInfo.uvTopRight;
					uvs[vertexIndex2] = charInfo.uvBottomRight;
					uvs[vertexIndex3] = charInfo.uvBottomLeft;
					
					colors[vertexIndex0] = _alphaColor;
					colors[vertexIndex1] = _alphaColor;
					colors[vertexIndex2] = _alphaColor;
					colors[vertexIndex3] = _alphaColor;
					
					vertexIndex0 += 4;
					vertexIndex1 += 4;
					vertexIndex2 += 4;
					vertexIndex3 += 4;
				}
			}
			
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
	
	virtual public Rect textRect
	{
		get {return _textRect;}	
	}
	
	[Obsolete("FLabel's boundsRect is obsolete, use textRect instead")]
	public Rect boundsRect
	{
		get {throw new NotSupportedException("boundsRect is obsolete! Use textRect instead");}
	}
	
	
}

