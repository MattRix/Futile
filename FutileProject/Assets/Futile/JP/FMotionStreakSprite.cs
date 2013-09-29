/* Author @jpsarda
 * A motion streak class.
 * 
 * Examples :
 * 
 * // A band with constant width and color
	motionStreak = new FMotionStreakSprite("sprite.png", // texture name
	20,  //Number of following quads
	x => 10.0f,  // width of the band function, x represents the offset in the band and 0<= x <=1
	x => new Color(1,1,1,1)  //color of the band function, x represents the offset in the band and 0<= x <=1
	);
 * 
 * // A band with thin ends and color changing (fading in the ends)
	motionStreak = new FMotionStreakSprite("sprite.png", // texture name
	20,  //Number of quads in the trail
	x => 10.0f*(float)Math.Sin (1.0f*x*Math.PI),  // width of the band function, x represents the offset in the band and 0<= x <=1
	x => new Color((float)Math.Sin (1.0f*x*Math.PI),1.0f,1.0f,(float)Math.Sin (1.0f*x*Math.PI))  //color of the band function, x represents the offset in the band and 0<= x <=1
	);
 * 
 * // A band with borders
	motionStreak = new FMotionStreakWithBorderSprite("sprite.png",10,x => 10.0f*(float)Math.Sin (1.0f*x*Math.PI),x => new Color(1,(float)Math.Sin (2.0f*x*Math.PI),1,(float)Math.Sin (1.0f*x*Math.PI)),  // same first parameters as in motion streak without borders
	x=>4.0f, // border width (contant here)
	x => new Color(0,0,0,0)  // border color (fading smoothly here)
	);
 * 
 * // Adding to stage
	AddChild(motionStreak);
 * 
 * // Pushing points
	motionStreak.PushPosition(position); //Vector2
 * 
 * Example :
 * 
 	public void HandleMultiTouch(FTouch[] touches)
	{
		foreach(FTouch touch in touches)
		{
			if(touch.phase == TouchPhase.Moved)
			{
				//we go reverse order so that if we remove a banana it doesn't matter
				//and also so that that we check from front to back
				Vector2 touchPos = motionStreak.GlobalToLocal(touch.position);
				motionStreak.PushPosition(touchPos);
			}
		}
	}
 * 
 * 
 */


using UnityEngine;
using System;


public class FMotionStreakElement
{
	public Vector2 bottomVertice,topVertice,centerVertice,direction;
	public Color color;
	public float width;
	//public float time;
	
	public Boolean valid;
	
	public FMotionStreakElement previous;
	
	public FMotionStreakElement(Vector2 position, FMotionStreakElement previousElement):base()
	{
		centerVertice=position;
		previous=previousElement;
		if (previousElement!=null) {
			Vector2 diff=centerVertice-previousElement.centerVertice;
			float dist = (float)Math.Sqrt(Vector2.SqrMagnitude(diff));
			if (dist>0.01f) {
				Vector2 normalizedDiff=diff.normalized;
				direction=new Vector2(-normalizedDiff.y,normalizedDiff.x);
				valid=true;
				ChangeWidth(1.0f);
			} else {
				if (previousElement.valid) {
					direction=previousElement.direction;
					valid=true;
					ChangeWidth(1.0f);
				} else {
					valid=false;
					width=0;
				}
			}
		} else {
			valid=false;
			width=0;
		}
	}
	
	virtual public void ChangeWidth(float newWidth) {
		if (valid) {
			Vector2 diff=direction*(newWidth*0.5f);
			topVertice=centerVertice+diff;
			bottomVertice=centerVertice-diff;
			width=newWidth;
		}
	}
	
	virtual public void ChangeSideWidths(float newBottomWidth,float newTopWidth) {
	}
	
	public void validWithElement(FMotionStreakElement element) {
		direction=element.direction;
		valid=true;
		ChangeWidth(1.0f);
	}
}

public class FMotionStreakSprite : FSprite
{
	protected Func<float,float> _widthFunction;
	protected Func<float,Color> _colorFunction;
	protected int _maxTrailQuads;
	
	//private Vector2[] _uvTrailVertices;
	protected FMotionStreakElement[] _trailElements;
	protected int _trailStartsIndex,_trailEndsIndex,_trailElementsCount;
	
	public FMotionStreakSprite (string elementName, int maxTrailQuads, Func<float,float> widthFunction,  Func<float,Color> colorFunction) : this(Futile.atlasManager.GetElementWithName(elementName), maxTrailQuads, widthFunction, colorFunction)
	{
	}
	
	public FMotionStreakSprite (FAtlasElement element, int maxTrailQuads, Func<float,float> widthFunction,  Func<float,Color> colorFunction) : base()
	{
		_widthFunction = widthFunction;
		_colorFunction = colorFunction;
		
		_maxTrailQuads = maxTrailQuads;
		
		Init(FFacetType.Quad, element,0); //this will call HandleElementChanged(), which will call Setup();
		
		_isAlphaDirty = true;
		
		UpdateLocalVertices();
	}
	
	override public void HandleElementChanged()
	{
		Setup();
	}

	virtual public void Setup ()
	{	
		_trailElements = new FMotionStreakElement[_maxTrailQuads+1];
		
		_trailStartsIndex=0;
		_trailEndsIndex=-1;
		_trailElementsCount=0;
		

		//_localVertices = new Vector2[_maxTrailQuads*4];
		//_uvTrailVertices = new Vector2[_sliceCount*4];
		
		_areLocalVerticesDirty = true;
		
		if(_numberOfFacetsNeeded != _maxTrailQuads)
		{
			_numberOfFacetsNeeded = _maxTrailQuads;
			if(_isOnStage) _stage.HandleFacetsChanged();
		}
	}
	
	override public void UpdateLocalVertices()
	{
		_areLocalVerticesDirty = false;
		//_isMeshDirty = true;
	} 
	
	override public void PopulateRenderLayer()
	{
		if(_isOnStage && _firstFacetIndex != -1) 
		{
			_isMeshDirty = false;
			
			int index=_trailStartsIndex;
			
			FMotionStreakElement previous=_trailElements[index];
			
			
			int vertexIndex0=_firstFacetIndex*4;
			for(int s = 1; s<_trailElementsCount; s++) {
				index++; if (index>=_trailElements.Length) index=0;
				FMotionStreakElement element=_trailElements[index];
				
				int vertexIndex1 = vertexIndex0 + 1;
				int vertexIndex2 = vertexIndex0 + 2;
				int vertexIndex3 = vertexIndex0 + 3;
				
				Vector3[] vertices = _renderLayer.vertices;
				Vector2[] uvs = _renderLayer.uvs;
				Color[] colors = _renderLayer.colors;
				
				_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex0], previous.topVertice,0);
				_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex1], previous.bottomVertice,0);
				_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex2], element.bottomVertice,0);
				_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex3], element.topVertice,0);
				
				uvs[vertexIndex0] = _element.uvTopLeft;
				uvs[vertexIndex1] = _element.uvTopRight;
				uvs[vertexIndex2] = _element.uvBottomRight;
				uvs[vertexIndex3] = _element.uvBottomLeft;
				
				colors[vertexIndex0] = previous.color;
				colors[vertexIndex1] = previous.color;
				colors[vertexIndex2] = element.color;
				colors[vertexIndex3] = element.color;
				
				previous=element;
				vertexIndex0+=4;
				
				_renderLayer.HandleVertsChange();
			}
		}
	}
	
	virtual public void UpdateElements() {
		int index=_trailStartsIndex;
		if (_trailElementsCount==1) {
			FMotionStreakElement element=_trailElements[index];
			element.ChangeWidth(_widthFunction(1.0f));
		} else {
			for(int i = 0; i<_trailElementsCount; i++) {
				FMotionStreakElement element=_trailElements[index];
				float indexRatio=(float)i/(float)(_trailElementsCount-1);
				element.ChangeWidth(_widthFunction(indexRatio));
				element.color=_colorFunction(indexRatio);
				
				index++; if (index>=_trailElements.Length) index=0;
			}
		}
	}
	
	virtual protected FMotionStreakElement NewElement(Vector2 localPosition, FMotionStreakElement previous) {
		return new FMotionStreakElement(localPosition,previous);
	}
	
	public Vector2 CurrentPosition() {
		if (_trailElementsCount==0) {
			return Vector2.zero;
		} else {
			return _trailElements[_trailEndsIndex].centerVertice;
		}
	}
	
	public void PushPosition(Vector2 localPosition)
	{
		FMotionStreakElement newElement;
		if (_trailElementsCount==0) {
			newElement=NewElement(localPosition,null);
		} else {
			FMotionStreakElement previous=_trailElements[_trailEndsIndex];
			newElement=NewElement(localPosition,previous);
			if (newElement.valid) {
				while (!previous.valid) {
					previous.validWithElement(newElement);
					previous=previous.previous;
					if (previous==null) break;
				}
			}
		}

		_trailEndsIndex++; if (_trailEndsIndex>=_trailElements.Length) _trailEndsIndex=0;
		_trailElements[_trailEndsIndex]=newElement;
		
		_trailElementsCount++;
		if (_trailElementsCount>_maxTrailQuads+1) {
			_trailElementsCount=_maxTrailQuads+1;
			_trailStartsIndex++; if (_trailStartsIndex>=_trailElements.Length) _trailStartsIndex=0;
		}
		
		UpdateElements();
		_isMeshDirty=true;
	}
}





public class FMotionStreakWithBorderElement : FMotionStreakElement
{
	public Vector2 bottom2Vertice,top2Vertice;
	public Color bottomColor,topColor;

	//public FMotionStreakElement previous;
	
	public FMotionStreakWithBorderElement(Vector2 position, FMotionStreakElement previousElement):base(position,previousElement)
	{
	}
	
	override public void ChangeWidth(float newWidth) {
		if (valid) {
			Vector2 diff=direction*(newWidth*0.5f);
			top2Vertice=topVertice=centerVertice+diff;
			bottom2Vertice=bottomVertice=centerVertice-diff;
			width=newWidth;
		}
	}
	
	override public void ChangeSideWidths(float newBottomWidth,float newTopWidth) {
		if (valid) {
			Vector2 diff=direction*(newTopWidth);
			top2Vertice=topVertice+diff;
			diff=direction*(newBottomWidth);
			bottom2Vertice=bottomVertice-diff;
		}
	}
}


public class FMotionStreakWithBorderSprite : FMotionStreakSprite
{
	protected Func<float,float> _bottomWidthFunction;
	protected Func<float,Color> _bottomColorFunction;
	protected Func<float,float> _topWidthFunction;
	protected Func<float,Color> _topColorFunction;

	public FMotionStreakWithBorderSprite (string elementName, int maxTrailQuads, Func<float,float> widthFunction,  Func<float,Color> colorFunction, Func<float,float> sideWidthFunction,  Func<float,Color> sideColorFunction) : this(elementName, maxTrailQuads, widthFunction, colorFunction, sideWidthFunction, sideColorFunction, sideWidthFunction, sideColorFunction)
	{
	}
	public FMotionStreakWithBorderSprite (string elementName, int maxTrailQuads, Func<float,float> widthFunction,  Func<float,Color> colorFunction, Func<float,float> bottomWidthFunction,  Func<float,Color> bottomColorFunction, Func<float,float> topWidthFunction,  Func<float,Color> topColorFunction) : this(Futile.atlasManager.GetElementWithName(elementName), maxTrailQuads, widthFunction, colorFunction, bottomWidthFunction, bottomColorFunction, topWidthFunction, topColorFunction)
	{
	}
	public FMotionStreakWithBorderSprite (FAtlasElement element, int maxTrailQuads, Func<float,float> widthFunction,  Func<float,Color> colorFunction, Func<float,float> sideWidthFunction,  Func<float,Color> sideColorFunction) : this(element, maxTrailQuads, widthFunction, colorFunction, sideWidthFunction, sideColorFunction, sideWidthFunction, sideColorFunction)
	{
	}
	public FMotionStreakWithBorderSprite (FAtlasElement element, int maxTrailQuads, Func<float,float> widthFunction,  Func<float,Color> colorFunction, Func<float,float> bottomWidthFunction,  Func<float,Color> bottomColorFunction, Func<float,float> topWidthFunction,  Func<float,Color> topColorFunction) : base(element, maxTrailQuads, widthFunction, colorFunction)
	{
		_bottomWidthFunction = bottomWidthFunction;
		_bottomColorFunction = bottomColorFunction;
		_topWidthFunction = topWidthFunction;
		_topColorFunction = topColorFunction;
	}

	override public void Setup ()
	{	
		_trailElements = new FMotionStreakElement[_maxTrailQuads+1];
		
		_trailStartsIndex=0;
		_trailEndsIndex=-1;
		_trailElementsCount=0;

		_areLocalVerticesDirty = true;
		
		if(_numberOfFacetsNeeded != _maxTrailQuads*3)
		{
			_numberOfFacetsNeeded = _maxTrailQuads*3;
			if(_isOnStage) _stage.HandleFacetsChanged();
		}
	}
	
	override public void UpdateLocalVertices()
	{
		_areLocalVerticesDirty = false;
		//_isMeshDirty = true;
	} 
	
	override public void PopulateRenderLayer()
	{
		if(_isOnStage && _firstFacetIndex != -1) 
		{
			_isMeshDirty = false;
			
			int index=_trailStartsIndex;
			FMotionStreakWithBorderElement previous=(FMotionStreakWithBorderElement)(_trailElements[index]);
			
			int i=0;
			
			int vertexIndex0=_firstFacetIndex*4;
			for(int s = 1; s<_trailElementsCount; s++) {
				index++; if (index>=_trailElements.Length) index=0;
				FMotionStreakWithBorderElement element=(FMotionStreakWithBorderElement)(_trailElements[index]);
				
				int vertexIndex1 = vertexIndex0 + 1;
				int vertexIndex2 = vertexIndex0 + 2;
				int vertexIndex3 = vertexIndex0 + 3;
				
				Vector3[] vertices = _renderLayer.vertices;
				Vector2[] uvs = _renderLayer.uvs;
				Color[] colors = _renderLayer.colors;

				_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex0], previous.topVertice,0);
				_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex1], previous.bottomVertice,0);
				_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex2], element.bottomVertice,0);
				_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex3], element.topVertice,0);
				
				uvs[vertexIndex0] = _element.uvTopLeft;
				uvs[vertexIndex1] = _element.uvTopRight;
				uvs[vertexIndex2] = _element.uvBottomRight;
				uvs[vertexIndex3] = _element.uvBottomLeft;
				
				colors[vertexIndex0] = previous.color;
				colors[vertexIndex1] = previous.color;
				colors[vertexIndex2] = element.color;
				colors[vertexIndex3] = element.color;
				
				previous=element;
				vertexIndex0+=4;
				
				_renderLayer.HandleVertsChange();
				i++;
			}
			
			
			
			
			index=_trailStartsIndex;
			previous=(FMotionStreakWithBorderElement)(_trailElements[index]);
			for(int s = 1; s<_trailElementsCount; s++) {
				index++; if (index>=_trailElements.Length) index=0;
				FMotionStreakWithBorderElement element=(FMotionStreakWithBorderElement)(_trailElements[index]);
				
				int vertexIndex1 = vertexIndex0 + 1;
				int vertexIndex2 = vertexIndex0 + 2;
				int vertexIndex3 = vertexIndex0 + 3;
				
				Vector3[] vertices = _renderLayer.vertices;
				Vector2[] uvs = _renderLayer.uvs;
				Color[] colors = _renderLayer.colors;
				
				_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex0], previous.top2Vertice,0);
				_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex1], previous.topVertice,0);
				_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex2], element.topVertice,0);
				_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex3], element.top2Vertice,0);
				
				uvs[vertexIndex0] = _element.uvTopLeft;
				uvs[vertexIndex1] = _element.uvTopRight;
				uvs[vertexIndex2] = _element.uvBottomRight;
				uvs[vertexIndex3] = _element.uvBottomLeft;
				
				colors[vertexIndex0] = previous.topColor;
				colors[vertexIndex1] = previous.color;
				colors[vertexIndex2] = element.color;
				colors[vertexIndex3] = element.topColor;
				
				previous=element;
				vertexIndex0+=4;
				
				_renderLayer.HandleVertsChange();
				i++;
			}
			
			
			
			index=_trailStartsIndex;
			previous=(FMotionStreakWithBorderElement)(_trailElements[index]);
			for(int s = 1; s<_trailElementsCount; s++) {
				index++; if (index>=_trailElements.Length) index=0;
				FMotionStreakWithBorderElement element=(FMotionStreakWithBorderElement)(_trailElements[index]);
				
				int vertexIndex1 = vertexIndex0 + 1;
				int vertexIndex2 = vertexIndex0 + 2;
				int vertexIndex3 = vertexIndex0 + 3;
				
				Vector3[] vertices = _renderLayer.vertices;
				Vector2[] uvs = _renderLayer.uvs;
				Color[] colors = _renderLayer.colors;

				_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex0], previous.bottomVertice,0);
				_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex1], previous.bottom2Vertice,0);
				_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex2], element.bottom2Vertice,0);
				_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex3], element.bottomVertice,0);
				
				uvs[vertexIndex0] = _element.uvTopLeft;
				uvs[vertexIndex1] = _element.uvTopRight;
				uvs[vertexIndex2] = _element.uvBottomRight;
				uvs[vertexIndex3] = _element.uvBottomLeft;
				
				colors[vertexIndex0] = previous.color;
				colors[vertexIndex1] = previous.bottomColor;
				colors[vertexIndex2] = element.bottomColor;
				colors[vertexIndex3] = element.color;
				
				previous=element;
				vertexIndex0+=4;
				
				_renderLayer.HandleVertsChange();
				i++;
			}
			
			
			
			
			
			
			
			Vector3 dummyVector3=new Vector3(1000000,100000,100000);
			Color dummyColor=new Color(0,0,0,0);
			for (;i<_numberOfFacetsNeeded;i++) {
				int vertexIndex1 = vertexIndex0 + 1;
				int vertexIndex2 = vertexIndex0 + 2;
				int vertexIndex3 = vertexIndex0 + 3;
				
				Vector3[] vertices = _renderLayer.vertices;
				Vector2[] uvs = _renderLayer.uvs;
				Color[] colors = _renderLayer.colors;

				vertices[vertexIndex0]=vertices[vertexIndex1]=vertices[vertexIndex2]=vertices[vertexIndex3]=dummyVector3;

				uvs[vertexIndex0] = uvs[vertexIndex1] = uvs[vertexIndex2] = uvs[vertexIndex3] = _element.uvBottomLeft;
				
				colors[vertexIndex0] = colors[vertexIndex1] = colors[vertexIndex2] = colors[vertexIndex3] = dummyColor;

				vertexIndex0+=4;
				
				_renderLayer.HandleVertsChange();
			}
			
			
			
		}
	}
	
	override public void UpdateElements() {
		int index=_trailStartsIndex;
		if (_trailElementsCount==1) {
			FMotionStreakElement element=_trailElements[index];
			element.ChangeWidth(_widthFunction(1.0f));
		} else {
			for(int i = 0; i<_trailElementsCount; i++) {
				FMotionStreakWithBorderElement element=(FMotionStreakWithBorderElement)(_trailElements[index]);
				float indexRatio=(float)i/(float)(_trailElementsCount-1);
				element.ChangeWidth(_widthFunction(indexRatio));
				element.color=_colorFunction(indexRatio);
				
				element.ChangeSideWidths(_bottomWidthFunction(indexRatio),_topWidthFunction(indexRatio));
				element.bottomColor=_bottomColorFunction(indexRatio);
				element.topColor=_topColorFunction(indexRatio);
				
				index++; if (index>=_trailElements.Length) index=0;
			}
		}
	}
	
	override protected FMotionStreakElement NewElement(Vector2 localPosition, FMotionStreakElement previous) {
		return new FMotionStreakWithBorderElement(localPosition,previous);
	}
}

