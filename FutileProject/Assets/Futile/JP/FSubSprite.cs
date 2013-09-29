using UnityEngine;
using System;


/*

Displays a sub area of a sprite. Supports trimmed atlas elements.
  
Examples :
// left half
FSubSprite subSprite=new FSubSprite("mysprite",new Rect(0f,0f,0.5f,1f));
AddChild(subSprite);

// right half
FSubSprite subSprite=new FSubSprite("mysprite",new Rect(0.5f,0f,0.5f,1f));
AddChild(subSprite);

// bottom half
FSubSprite subSprite=new FSubSprite("mysprite",new Rect(0f,0f,1f,0.5f));
AddChild(subSprite);

// top half
FSubSprite subSprite=new FSubSprite("mysprite",new Rect(0f,0.5f,1f,0.5f));
AddChild(subSprite);

// top/right corner
FSubSprite subSprite=new FSubSprite("mysprite",new Rect(0.5f,0.5f,0.5f,0.5f));
AddChild(subSprite);

// middle
FSubSprite subSprite=new FSubSprite("mysprite",new Rect(0.25f,0.25f,0.5f,0.5f));
AddChild(subSprite);
  
*/

public class FSubSprite : FSprite
{
	public FSubSprite (string elementName, Rect subRec) : base ()
	{
		
		//First transform the original atlas element into a new atlas element standing for the sub area
		//This part should be set in the FAtlasELement class in a method called GetSubElement(Rect subRect) for example
		
		FAtlasElement element = Futile.atlasManager.GetElementWithName(elementName).Clone();
		
		//Debug.Log ("a element.sourceRect="+element.sourceRect+" element.uvRect="+element.uvRect+" element.sourceSize="+element.sourceSize);

		Vector2 newSourceSize=new Vector2(element.sourceSize.x*subRec.width,element.sourceSize.y*subRec.height);

		Vector2 textureSize=element.atlas.textureSize;
		Rect sourceFrame=new Rect(element.uvRect.x*textureSize.x,element.uvRect.y*textureSize.y,element.uvRect.width*textureSize.x,element.uvRect.height*textureSize.y);
		
		Rect untrimmedSourceFrame=new Rect(sourceFrame.x-element.sourceRect.x*Futile.resourceScale,sourceFrame.y-element.sourceRect.y*Futile.resourceScale,element.sourceSize.x*Futile.resourceScale,element.sourceSize.y*Futile.resourceScale);
		Rect newUntrimmedSourceFrame = new Rect
		(
			untrimmedSourceFrame.x+untrimmedSourceFrame.width*subRec.x , 
			untrimmedSourceFrame.y+untrimmedSourceFrame.height*subRec.y ,
			untrimmedSourceFrame.width*subRec.width ,
			untrimmedSourceFrame.height*subRec.height
		);
		
		Rect trimmedSourceFrame = new Rect
		(
			untrimmedSourceFrame.x+element.sourceRect.x*Futile.resourceScale , 
			untrimmedSourceFrame.y+element.sourceRect.y*Futile.resourceScale ,
			element.sourceRect.width*Futile.resourceScale ,
			element.sourceRect.height*Futile.resourceScale
		);

		Rect newTrimmedSourceFrame=trimmedSourceFrame;
		//Debug.Log ("a untrimmedSourceFrame="+untrimmedSourceFrame+" trimmedSourceFrame="+trimmedSourceFrame);
		if (newTrimmedSourceFrame.xMin<newUntrimmedSourceFrame.xMin) newTrimmedSourceFrame.xMin=newUntrimmedSourceFrame.xMin;
		if (newTrimmedSourceFrame.yMin<newUntrimmedSourceFrame.yMin) newTrimmedSourceFrame.yMin=newUntrimmedSourceFrame.yMin;		
		if (newTrimmedSourceFrame.xMax>newUntrimmedSourceFrame.xMax) newTrimmedSourceFrame.xMax=newUntrimmedSourceFrame.xMax;
		if (newTrimmedSourceFrame.yMax>newUntrimmedSourceFrame.yMax) newTrimmedSourceFrame.yMax=newUntrimmedSourceFrame.yMax;
		//Debug.Log ("b newTrimmedSourceFrame="+newTrimmedSourceFrame);
		
		element.sourceRect=new Rect(
			(newTrimmedSourceFrame.x-newUntrimmedSourceFrame.x)*Futile.resourceScaleInverse,
			(newTrimmedSourceFrame.y-newUntrimmedSourceFrame.y)*Futile.resourceScaleInverse,
			newTrimmedSourceFrame.width*Futile.resourceScaleInverse,
			newTrimmedSourceFrame.height*Futile.resourceScaleInverse
		);
		
		element.uvRect = new Rect
		(
			newTrimmedSourceFrame.x/textureSize.x,
			newTrimmedSourceFrame.y/textureSize.y,
			newTrimmedSourceFrame.width/textureSize.x,
			newTrimmedSourceFrame.height/textureSize.y
		);

		element.uvTopLeft.Set(element.uvRect.xMin,element.uvRect.yMax);
		element.uvTopRight.Set(element.uvRect.xMax,element.uvRect.yMax);
		element.uvBottomRight.Set(element.uvRect.xMax,element.uvRect.yMin);
		element.uvBottomLeft.Set(element.uvRect.xMin,element.uvRect.yMin);

		element.sourceSize=newSourceSize;
		element.sourcePixelSize.Set(element.sourcePixelSize.x*subRec.width,element.sourcePixelSize.y*subRec.height);
		
		//Debug.Log ("b element.sourceRect="+element.sourceRect+" element.uvRect="+element.uvRect+" element.sourceSize="+element.sourceSize);
		
		
		//Classic FSprite constructor
		_localVertices = new Vector2[4];
		
		Init(FFacetType.Quad, element,1);
		
		_isAlphaDirty = true;
		
		UpdateLocalVertices();
	}
}

