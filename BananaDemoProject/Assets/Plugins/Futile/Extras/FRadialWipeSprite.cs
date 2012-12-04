using UnityEngine;
using System;

public class FRadialWipeSprite : FSprite
{
	protected float _baseAngle;
	protected float _percentage;
	protected bool _isClockwise;
	protected Vector2[] _vertices = new Vector2[7];
	
	public FRadialWipeSprite (string elementName, bool isClockwise, float baseAngle, float percentage) : base()
	{
		_isClockwise = isClockwise;
		_baseAngle = (baseAngle + 36000000.0f) % 360.0f;
		_percentage = Mathf.Max (0.0f, Mathf.Min(1.0f, percentage));
		
		Init(FFacetType.Triangle, Futile.atlasManager.GetElementWithName(elementName),5);
		
		_isAlphaDirty = true; 
		
		UpdateLocalVertices(); 
	}

	private void CalculateTheRadialVertices ()
	{
		//TODO: A lot of these calculations could be offloaded to when the element (and maybe anchor?) changes. 
		float startAngle = _baseAngle * RXMath.DTOR;
		float endAngle = startAngle + _percentage * RXMath.DOUBLE_PI;
		
//		Debug.Log ("width " + _localRect.width);
//		Debug.Log ("height " + _localRect.height);
		
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
		
		float hugeRadius = 128.0f;
		
		//_vertices[0] = new Vector2(0,halfHeight);
		
		//Debug.Log(Math.Atan2(1,2)*RXMath.RTOD);
		
//		Debug.Log ("CORNER 0 " + cornerAngle0*RXMath.RTOD);
//		Debug.Log ("CORNER 1 " + cornerAngle1*RXMath.RTOD);
//		Debug.Log ("CORNER 2 " + cornerAngle2*RXMath.RTOD);
//		Debug.Log ("CORNER 3 " + cornerAngle3*RXMath.RTOD);
////		
//		Debug.Log ("START: " + startAngle*RXMath.RTOD);
//		Debug.Log ("END: " + endAngle*RXMath.RTOD);
//		
//		cornerAngle0 -= baseAngle;
//		cornerAngle1 -= baseAngle;
//		cornerAngle2 -= baseAngle;
//		cornerAngle3 -= baseAngle;
//		
//		cornerAngle0 = (cornerAngle0 + RXMath.DOUBLE_PI*10000.0f) % RXMath.DOUBLE_PI; 
//		cornerAngle1 = (cornerAngle1 + RXMath.DOUBLE_PI*10000.0f) % RXMath.DOUBLE_PI; 
//		cornerAngle2 = (cornerAngle2 + RXMath.DOUBLE_PI*10000.0f) % RXMath.DOUBLE_PI; 
//		cornerAngle3 = (cornerAngle3 + RXMath.DOUBLE_PI*10000.0f) % RXMath.DOUBLE_PI; 
//		
//		startAngle -= baseAngle;
//		endAngle -= baseAngle;
//		
//		startAngle = (startAngle + RXMath.DOUBLE_PI*10000.0f) % RXMath.DOUBLE_PI; 
//		endAngle = (endAngle + RXMath.DOUBLE_PI*10000.0f) % RXMath.DOUBLE_PI; 
		 
		for(int v = 0; v<6; v++)
		{
			float angle = 0;
			 
			if(v<5)
			{
				angle = startAngle + ((endAngle - startAngle)/5.0f * v);
				
				//angle -= baseAngle;
				
				//angle = (angle + RXMath.DOUBLE_PI*10000.0f) % RXMath.DOUBLE_PI; 
				
//				Debug.Log (v + " is " + (angle*RXMath.RTOD));
				
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
			
			float compX = Mathf.Cos(-angle + RXMath.HALF_PI) * hugeRadius;
			float compY = Mathf.Sin(-angle + RXMath.HALF_PI) * hugeRadius;
			
			angle = (angle + RXMath.DOUBLE_PI*10000.0f) % RXMath.DOUBLE_PI; 
			
			//snap the verts to the edge of the rect
			
			if(angle < cornerAngleTR) //top
			{
//				if(v == 1) Debug.Log (v + ": " + compX +","+compY);
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
			else if(angle >= cornerAngleTL) //top
			{
				compX = compX * (halfHeight / compY);
				compY = halfHeight;
			} 
			
			_vertices[v] = new Vector2(compX, compY);
			
//			Debug.Log ("" + v + ": point " + compX + "," + compY);
		}
		
		_vertices[6] = new Vector2(0,0); //THE LAST VERT HAS TO BE THE CENTER
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
			
			for(int i = 0; i<15; i++) //temporarily just use the top left as the UV
			{
				uvs[i] = _element.uvTopLeft;
				if(i/3 == 0)
				{
					colors[i] = new Color(1.0f,1.0f,0.0f,1.0f);
				}
				else if(i/3 == 1)
				{
					colors[i] = new Color(0.0f,0.8f,0.2f,1.0f);
				}
				else if(i/3 == 2)
				{
					colors[i] = new Color(1.0f,0.6f,0.4f,1.0f);
				}
				else if(i/3 == 3)
				{
					colors[i] = new Color(0.0f,0.4f,0.6f,1.0f);
				}
				else if(i/3 == 4)
				{
					colors[i] = new Color(1.0f,0.2f,0.8f,1.0f);
				}
				else if(i/3 == 5)
				{
					colors[i] = new Color(0.0f,1.0f,0.0f,1.0f);
				}
				
//				if(i%3 == 0)
//				{ 
//					colors[i] = Color.black;
//				}
				
				colors[i] = new Color(colors[i].r,colors[i].g,colors[i].b,0.8f);
			}
			
			int vertexIndex0 = _firstFacetIndex*3;
			
			Vector2 centerVectex = _vertices[6];
			
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex0], centerVectex,0);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex0 + 1], _vertices[0],0);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex0 + 2], _vertices[1],0);
			
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex0 + 3], centerVectex,0);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex0 + 4], _vertices[1],0);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex0 + 5], _vertices[2],0);
			
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex0 + 6], centerVectex,0);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex0 + 7], _vertices[2],0);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex0 + 8], _vertices[3],0);
			
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex0 + 9], centerVectex,0);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex0 + 10], _vertices[3],0);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex0 + 11], _vertices[4],0);
			
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex0 + 12], centerVectex,0);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex0 + 13], _vertices[4],0);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex0 + 14], _vertices[5],0);
			
			
//			int vertexIndex0 = _firstFacetIndex*4;
//			int vertexIndex1 = vertexIndex0 + 1;
//			int vertexIndex2 = vertexIndex0 + 2;
//			int vertexIndex3 = vertexIndex0 + 3;
//			
//			Vector3[] vertices = _renderLayer.vertices;
//			Vector2[] uvs = _renderLayer.uvs;
//			Color[] colors = _renderLayer.colors;
//			
//			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex0], _localVertices[0],0);
//			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex1], _localVertices[1],0);
//			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex2], _localVertices[2],0);
//			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex3], _localVertices[3],0);
//			
//			uvs[vertexIndex0] = _element.uvTopLeft;
//			uvs[vertexIndex1] = _element.uvTopRight;
//			uvs[vertexIndex2] = _element.uvBottomRight;
//			uvs[vertexIndex3] = _element.uvBottomLeft;
//			
//			colors[vertexIndex0] = _alphaColor;
//			colors[vertexIndex1] = _alphaColor;
//			colors[vertexIndex2] = _alphaColor;
//			colors[vertexIndex3] = _alphaColor;
			
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
	
	

