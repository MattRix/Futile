using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//parts of this were inspired by https://github.com/prime31/UIToolkit/blob/master/Assets/Plugins/UIToolkit/UIElements/UIText.cs

public class FLabel : FFacetElementNode
{
	public Action SignalTextChange;

	public static float defaultAnchorX = 0.5f;
	public static float defaultAnchorY = 0.5f;
	
	protected FFont _font;
	protected string _fontName;
	protected string _text;
	
	protected Color _color = Futile.white;
	protected Color _alphaColor = Futile.white;
	
	protected FLetterQuadLine[] _letterQuadLines;
	
	protected bool _isMeshDirty = false;
	
	protected float _anchorX = defaultAnchorX;
	protected float _anchorY = defaultAnchorY;
	
	protected bool _doesTextNeedUpdate = false;
	protected bool _doesLocalPositionNeedUpdate = false;
	protected bool _doQuadsNeedUpdate = false;
	
	protected Rect _textRect;
	
	protected FTextParams _textParams;

	protected bool _shouldSnapToPixels = false;
	
	public FLabel (string fontName, string text) : this(fontName, text, new FTextParams())
	{
	}
	
	public FLabel (string fontName, string text, FTextParams textParams) : base()
	{
		_fontName = fontName;
		_text = text;
		_font = Futile.atlasManager.GetFontWithName(_fontName);
		_textParams = textParams;

		Init(FFacetType.Quad, _font.element, 0);

		CreateTextQuads();
	}

	public void CreateTextQuads()
	{
		_doesTextNeedUpdate = false;

		int oldFacetsNeeded = _numberOfFacetsNeeded;
		
		_letterQuadLines = _font.GetQuadInfoForText(_text,_textParams);
		
		_numberOfFacetsNeeded = 0;
		
		int lineCount = _letterQuadLines.Length;
		for(int i = 0; i< lineCount; i++)
		{
			_numberOfFacetsNeeded += _letterQuadLines[i].quads.Length;
		}
		
		if(_isOnStage)
		{
			int delta = _numberOfFacetsNeeded - oldFacetsNeeded;
			
			if(delta != 0) //if the number of letter quads has changed, tell the stage
			{
				_stage.HandleFacetsChanged();
			}
		}
		
		UpdateLocalPosition(); //figures out the bounds and alignment, and sets the mesh dirty
	}
	
	public void UpdateLocalPosition()
	{
		_doesLocalPositionNeedUpdate = false;
		
		float minY = float.MaxValue;
		float maxY = float.MinValue;
		
		float minX = float.MaxValue;
		float maxX = float.MinValue;
		
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
				line.quads[q].CalculateVectors(offsetX+_font.offsetX, offsetY+_font.offsetY);
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
		
		if(shouldUpdateDepth)
		{
			UpdateFacets();
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
		if(_isOnStage && _firstFacetIndex != -1)
		{
			_isMeshDirty = false;

			//check if the label is empty so we don't have to bother trying to draw anything
			if(_letterQuadLines.Length == 0 || _letterQuadLines[0].quads.Length == 0)
			{
				_renderLayer.HandleVertsChange();
				return; 
			}
			
			Vector3[] vertices = _renderLayer.vertices;
			Vector2[] uvs = _renderLayer.uvs;
			Color[] colors = _renderLayer.colors;
			
			int vertexIndex0 = _firstFacetIndex*4;
			int vertexIndex1 = vertexIndex0 + 1;
			int vertexIndex2 = vertexIndex0 + 2;
			int vertexIndex3 = vertexIndex0 + 3;

			Vector2 topLeft = _letterQuadLines[0].quads[0].topLeft;

			FMatrix matrixToUse = _concatenatedMatrix;

			if(_shouldSnapToPixels)
			{
				matrixToUse = matrixToUse.Clone();

				matrixToUse.tx += (Mathf.Round(topLeft.x * Futile.displayScale) * Futile.displayScaleInverse) - topLeft.x;
				matrixToUse.ty += (Mathf.Round(topLeft.y * Futile.displayScale) * Futile.displayScaleInverse) - topLeft.y;
			}
			
			int lineCount = _letterQuadLines.Length;
			for(int i = 0; i<lineCount; i++)
			{
				FLetterQuad[] quads = _letterQuadLines[i].quads;
				
				
				int quadCount = quads.Length;
				
				for(int q = 0; q<quadCount; q++)
				{
					FLetterQuad quad = quads[q];
					FCharInfo charInfo = quad.charInfo;
					
					matrixToUse.ApplyVector3FromLocalVector2(ref vertices[vertexIndex0], quad.topLeft,0);
					matrixToUse.ApplyVector3FromLocalVector2(ref vertices[vertexIndex1], quad.topRight,0);
					matrixToUse.ApplyVector3FromLocalVector2(ref vertices[vertexIndex2], quad.bottomRight,0);
					matrixToUse.ApplyVector3FromLocalVector2(ref vertices[vertexIndex3], quad.bottomLeft,0);
					
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

    public FLabelAlignment alignment
    {
        get 
        {
            if (_anchorX == 0.5f) return FLabelAlignment.Center;
            if (_anchorX == 0.0f) return FLabelAlignment.Left;
            if (_anchorX == 1.0f) return FLabelAlignment.Right;

            return FLabelAlignment.Custom;
        }
        set 
        {
            if (value == FLabelAlignment.Center) this.anchorX = 0.5f;
            else if (value == FLabelAlignment.Left) this.anchorX = 0.0f;
            else if (value == FLabelAlignment.Right) this.anchorX = 1.0f;
        }
    }
	
	virtual public string text
	{
		get {return _text;}
		set 
		{
			if(_text != value)
			{
				_text = value; 
				_doesTextNeedUpdate = true;
				CreateTextQuads(); //lazily creating the quads was causing too many issues, so just create them when .text is set
				if(SignalTextChange != null) SignalTextChange();
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
		get 
		{
			if(_doesTextNeedUpdate) CreateTextQuads();
			if(_doesLocalPositionNeedUpdate) UpdateLocalPosition();
			return _textRect;
		}	
	}

	public bool shouldSnapToPixels
	{
		get { return _shouldSnapToPixels; }
		set { _shouldSnapToPixels = value; }
	}

	[Obsolete("FLabel's boundsRect is obsolete, use textRect instead")]
	public Rect boundsRect
	{
		get {throw new NotSupportedException("boundsRect is obsolete! Use textRect instead");}
	}
	
	//for convenience
	public void SetAnchor(float newX, float newY)
	{
		this.anchorX = newX;
		this.anchorY = newY;
	}
	
	public void SetAnchor(Vector2 newAnchor)
	{
		this.anchorX = newAnchor.x;
		this.anchorY = newAnchor.y;
	}
	
	public Vector2 GetAnchor()
	{
		return new Vector2(_anchorX,_anchorY);	
	}
}

public enum FLabelAlignment
{
    Center,
    Left,
    Right,
    Custom
}



