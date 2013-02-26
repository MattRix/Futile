using UnityEngine;
using System;

public class FRadialWipeSprite : FSprite
{
	protected float _baseAngle;
	protected float _percentage;
	protected bool _isClockwise;
	protected Vector2[] _meshVertices = new Vector2[7];
	protected Vector2[] _uvVertices = new Vector2[7];
	
	public FRadialWipeSprite (string elementName, bool isClockwise, float baseAngle, float percentage) : base()
	{
		_isClockwise = isClockwise;
		_baseAngle = (baseAngle + 36000000.0f) % 360.0f;
		_percentage = Mathf.Clamp01(percentage);
		
		Init(FFacetType.Triangle, Futile.atlasManager.GetElementWithName(elementName),5);
		
		_isAlphaDirty = true; 
		
		UpdateLocalVertices(); 
	}

	private void CalculateTheRadialVertices ()
	{
		//TODO: A lot of these calculations could be offloaded to when the element (and maybe anchor?) changes. 
		
		float baseAngleToUse;
		
		if(_isClockwise)
		{
			baseAngleToUse = _baseAngle;
		}
		else 
		{
			baseAngleToUse = 360.0f-_baseAngle;
		}
		
		float startAngle = baseAngleToUse * RXMath.DTOR;
		float endAngle = startAngle + _percentage * RXMath.DOUBLE_PI;
		
		float halfWidth = _localRect.width*0.5f;
		float halfHeight = _localRect.height*0.5f;
		
		//corner 0 is the top right, the rest go clockwise from there
		Vector2 cornerTR = new Vector2(halfHeight, halfWidth);
		Vector2 cornerBR = new Vector2(-halfHeight, halfWidth);
		Vector2 cornerBL = new Vector2(-halfHeight, -halfWidth);
		Vector2 cornerTL = new Vector2(halfHeight, -halfWidth);
		
		float cornerAngleTR = -Mathf.Atan2(cornerTR.x,cornerTR.y) + RXMath.HALF_PI;
		float cornerAngleBR = -Mathf.Atan2(cornerBR.x,cornerBR.y) + RXMath.HALF_PI;
		float cornerAngleBL = -Mathf.Atan2(cornerBL.x,cornerBL.y) + RXMath.HALF_PI;
		float cornerAngleTL = -Mathf.Atan2(cornerTL.x,cornerTL.y) + RXMath.HALF_PI;
		
		cornerAngleTR = (cornerAngleTR + RXMath.DOUBLE_PI*10000.0f) % RXMath.DOUBLE_PI;
		cornerAngleBR = (cornerAngleBR + RXMath.DOUBLE_PI*10000.0f) % RXMath.DOUBLE_PI;
		cornerAngleBL = (cornerAngleBL + RXMath.DOUBLE_PI*10000.0f) % RXMath.DOUBLE_PI;
		cornerAngleTL = (cornerAngleTL + RXMath.DOUBLE_PI*10000.0f) % RXMath.DOUBLE_PI;
		
		float cornerAngle0;
		float cornerAngle1;
		float cornerAngle2;
		float cornerAngle3;
		
		if(startAngle < cornerAngleTR) //top right
		{
			cornerAngle0 = cornerAngleTR;	
			cornerAngle1 = cornerAngleBR;
			cornerAngle2 = cornerAngleBL;
			cornerAngle3 = cornerAngleTL;
		}
		else if(startAngle >= cornerAngleTR && startAngle < cornerAngleBR) //right
		{
			cornerAngle0 = cornerAngleBR;	
			cornerAngle1 = cornerAngleBL;
			cornerAngle2 = cornerAngleTL;
			cornerAngle3 = cornerAngleTR + RXMath.DOUBLE_PI;
		}
		else if(startAngle >= cornerAngleBR && startAngle < cornerAngleBL) //left
		{
			cornerAngle0 = cornerAngleBL;	
			cornerAngle1 = cornerAngleTL;
			cornerAngle2 = cornerAngleTR + RXMath.DOUBLE_PI;
			cornerAngle3 = cornerAngleBR + RXMath.DOUBLE_PI;
		}
		else if(startAngle >= cornerAngleBL && startAngle < cornerAngleTL)
		{
			cornerAngle0 = cornerAngleTL;	
			cornerAngle1 = cornerAngleTR + RXMath.DOUBLE_PI;
			cornerAngle2 = cornerAngleBR + RXMath.DOUBLE_PI;
			cornerAngle3 = cornerAngleBL + RXMath.DOUBLE_PI;
		}
		//else if(startAngle >= cornerAngleTL) 
		else //top left
		{
			cornerAngle0 = cornerAngleTR + RXMath.DOUBLE_PI;	
			cornerAngle1 = cornerAngleBR + RXMath.DOUBLE_PI;
			cornerAngle2 = cornerAngleBL + RXMath.DOUBLE_PI;
			cornerAngle3 = cornerAngleTL + RXMath.DOUBLE_PI;
		}
		
		float hugeRadius = 1000000.0f;
		 
		for(int v = 0; v<6; v++)
		{
			float angle = 0;
			 
			if(v<5)
			{
				angle = startAngle + ((endAngle - startAngle)/5.0f * v);
				
				if(v == 0)
				{
					//do nothing, 0 is gooood
				}
				else if(v == 1 && endAngle > cornerAngle0)
				{
					angle = cornerAngle0;
				}
				else if(v == 2 && endAngle > cornerAngle1)
				{
					angle = cornerAngle1;
				}
				else if(v == 3 && endAngle > cornerAngle2)
				{
					angle = cornerAngle2;
				}	
				else if(v == 4 && endAngle > cornerAngle3)
				{
					angle = cornerAngle3;
				}
				else if(endAngle > cornerAngle3)
				{
					angle = Mathf.Max (angle, cornerAngle3);
				}
				else if(endAngle > cornerAngle2)
				{
					angle = Mathf.Max (angle, cornerAngle2);
				}
				else if(endAngle > cornerAngle1)
				{
					angle = Mathf.Max (angle, cornerAngle1);	
				}
				else if(endAngle > cornerAngle0)
				{
					angle = Mathf.Max (angle, cornerAngle0);
				}
				
				
			}
			else 
			{
				angle = endAngle;		
			}
			
			angle = (angle + RXMath.DOUBLE_PI*10000.0f) % RXMath.DOUBLE_PI; 
			
			float compX = Mathf.Cos(-angle + RXMath.HALF_PI) * hugeRadius;
			float compY = Mathf.Sin(-angle + RXMath.HALF_PI) * hugeRadius;
			
			//snap the verts to the edge of the rect
			
			if(angle < cornerAngleTR) //top right
			{
				compX = compX * (halfHeight / compY);
				compY = halfHeight;
			}
			else if(angle >= cornerAngleTR && angle < cornerAngleBR) //right
			{
				compY = compY * (halfWidth / compX);
				compX = halfWidth;	
			}
			else if(angle >= cornerAngleBR && angle < cornerAngleBL) //bottom
			{
				compX = compX * (-halfHeight / compY);
				compY = -halfHeight;	
			}
			else if(angle >= cornerAngleBL && angle < cornerAngleTL) //left
			{
				compY = compY * (-halfWidth / compX);
				compX = -halfWidth;	
			}
			else if(angle >= cornerAngleTL) //top left
			{
				compX = compX * (halfHeight / compY);
				compY = halfHeight;
			} 
			
			if(!_isClockwise)
			{
				compX = -compX;
			}
			
			_meshVertices[v] = new Vector2(compX, compY);
		}
		
		_meshVertices[6] = new Vector2(0,0); //this last vert is actually the center vert
		
		//create uv vertices
		
		Rect uvRect = _element.uvRect;
		Vector2 uvCenter = uvRect.center;
		
		for(int v = 0; v<7; v++)
		{
			_uvVertices[v].x = uvCenter.x + _meshVertices[v].x / _localRect.width * uvRect.width;
			_uvVertices[v].y = uvCenter.y + _meshVertices[v].y / _localRect.height * uvRect.height;
		}
		
		//put mesh vertices in the correct position
		float offsetX = _localRect.center.x; 
		float offsetY = _localRect.center.y;
		
		for(int v = 0; v<7; v++)
		{
			_meshVertices[v].x += offsetX;
			_meshVertices[v].y += offsetY;			
		}
	}
		
	override public void PopulateRenderLayer()
	{
		if(_isOnStage && _firstFacetIndex != -1) 
		{
			_isMeshDirty = false;
			
			CalculateTheRadialVertices();
			
			//now do the actual population
			
			Vector3[] vertices = _renderLayer.vertices;
			Vector2[] uvs = _renderLayer.uvs;
			Color[] colors = _renderLayer.colors;	
			
			int vertexIndex0 = _firstFacetIndex*3;
			
			//set the colors
			for(int c = 0; c<15; c++)
			{
				colors[vertexIndex0 + c] = _alphaColor;	
			}
			
			//vertex 6 is the center vertex
			
			//set the uvs
			uvs[vertexIndex0] = _uvVertices[6];	
			uvs[vertexIndex0 + 1] = _uvVertices[0];	
			uvs[vertexIndex0 + 2] = _uvVertices[1];	
			
			uvs[vertexIndex0 + 3] = _uvVertices[6];	
			uvs[vertexIndex0 + 4] = _uvVertices[1];	
			uvs[vertexIndex0 + 5] = _uvVertices[2];	
			
			uvs[vertexIndex0 + 6] = _uvVertices[6];	
			uvs[vertexIndex0 + 7] = _uvVertices[2];	
			uvs[vertexIndex0 + 8] = _uvVertices[3];	
			
			uvs[vertexIndex0 + 9] = _uvVertices[6];	
			uvs[vertexIndex0 + 10] = _uvVertices[3];	
			uvs[vertexIndex0 + 11] = _uvVertices[4];	
			
			uvs[vertexIndex0 + 12] = _uvVertices[6];	
			uvs[vertexIndex0 + 13] = _uvVertices[4];	
			uvs[vertexIndex0 + 14] = _uvVertices[5];
			
			//set the mesh
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex0], _meshVertices[6],0);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex0 + 1], _meshVertices[0],0);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex0 + 2], _meshVertices[1],0);
			
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex0 + 3], _meshVertices[6],0);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex0 + 4], _meshVertices[1],0);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex0 + 5], _meshVertices[2],0);
			
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex0 + 6], _meshVertices[6],0);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex0 + 7], _meshVertices[2],0);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex0 + 8], _meshVertices[3],0);
			
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex0 + 9], _meshVertices[6],0);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex0 + 10], _meshVertices[3],0);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex0 + 11], _meshVertices[4],0);
			
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex0 + 12], _meshVertices[6],0);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex0 + 13], _meshVertices[4],0);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex0 + 14], _meshVertices[5],0);
			
			_renderLayer.HandleVertsChange();
		}
	}
	
	public float baseAngle 
	{
		get { return _baseAngle;}
		set 
		{ 
			value = (value + 36000000.0f) % 360.0f;
			
			if(_baseAngle != value)
			{
				_baseAngle = value; 
				_isMeshDirty = true; 
			}
		}
	}
	
	public float percentage 
	{
		get { return _percentage;}
		set 
		{ 
			value = Mathf.Max (0.0f, Mathf.Min(1.0f, value));
			if(_percentage != value)
			{
				_percentage = value; 
				_isMeshDirty = true; 
			}
		}
	}
	
	public bool isClockwise 
	{
		get { return _isClockwise;}
		set 
		{ 
			if(_isClockwise != value)
			{
				_isClockwise = value; 
				_isMeshDirty = true; 
			}
		}
	}

}
	
	

