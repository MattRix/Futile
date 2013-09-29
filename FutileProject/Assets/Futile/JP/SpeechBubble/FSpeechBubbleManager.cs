using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;
using System.Reflection;

public class FSpeechBubbleManager
{
	static readonly FSpeechBubbleManager instance=new FSpeechBubbleManager();
	
	static FSpeechBubbleManager () {}

	FSpeechBubbleManager() {}

	public static FSpeechBubbleManager Instance { get { instance.Initialize(); return instance; } }
	
	protected bool _initialized=false;
	protected void Initialize() {
		if (!_initialized) {
			_initialized=true;
			HandleResize(false);
			Futile.screen.SignalResize += HandleResize;
		}
	}
	
	protected void HandleResize(bool wasOrientationChange)
	{
		_defaultContainer=Futile.stage;
		
		float screenMargin=5f;
		_defaultVisibleArea=new Rect(-Futile.screen.halfWidth+screenMargin,-Futile.screen.halfHeight+screenMargin,Futile.screen.width-screenMargin*2,Futile.screen.height-screenMargin*2);
	}
	
	
	protected Rect _defaultVisibleArea;
	protected FContainer _defaultContainer=null;
	protected float _defaultContentMarginX=5f,_defaultContentMarginY=5f;
	protected float _defaultPointerLength=10f;
	protected Color _defaultBackgroundColor=Color.white;
	
	public Rect defaultVisibleArea { get { return _defaultVisibleArea; } set { _defaultVisibleArea=value; } }
	public FContainer defaultContainer { get { return _defaultContainer; } set { _defaultContainer=value; } }
	public float defaultContentMarginX { get { return _defaultContentMarginX; } set { _defaultContentMarginX=value; } }
	public float defaultContentMarginY { get { return _defaultContentMarginY; } set { _defaultContentMarginY=value; } }
	public float defaultPointerLength { get { return _defaultPointerLength; } set { _defaultPointerLength=value; } }
	public Color defaultBackgroundColor { get { return _defaultBackgroundColor; } set { _defaultBackgroundColor=value; } }
	
	public FSpeechBubble Show(FPseudoHtmlText text,Vector2 point,float pointerMargin) {
		return Show(text,text.width,text.height,point,pointerMargin);
	}
	
	public FSpeechBubble Show(FNode node, float width, float height, Vector2 point, float pointerMargin) {
		return Show(node,width,height,_defaultContentMarginX,_defaultContentMarginY,point,_defaultPointerLength,pointerMargin,_defaultBackgroundColor,_defaultContainer,_defaultVisibleArea);
	}
	
	public FSpeechBubble Show(FNode node, float width, float height, float contentMarginX, float contentMarginY, Vector2 point, float pointerLength, float pointerMargin, Color backgroundColor, FContainer container, Rect visibleArea) {		
		FSpeechBubble bubble=new FSpeechBubble();
		bubble.backgroundColor=backgroundColor;
		if (node!=null) {
			if (node.container!=null) node.RemoveFromContainer();
			bubble.AddChild(node);
		}
		container.AddChild(bubble);

		//size fo the bubble with content margin taken into account
		float totalWidth=width+contentMarginX*2;
		float totalHeight=height+contentMarginY*2;
		
		float totalPointerLength=pointerLength+pointerMargin;
		float verticalFreeWidth=0,verticalFreeHeight=0;
		float horizontalFreeWidth=0,horizontalFreeHeight=0;
		
		//Try first the space on top/bottom or left/right
		//Determine which space is best suited
		
		if (point.x<visibleArea.center.x) {
			//more space on the right
			verticalFreeWidth=visibleArea.xMax-point.x-totalPointerLength;
			verticalFreeHeight=visibleArea.height;
		} else {
			//more space on the left
			verticalFreeWidth=point.x-visibleArea.xMin-totalPointerLength;
			verticalFreeHeight=visibleArea.height;
		}
		if (point.y<visibleArea.center.y) {
			//more space on the top
			horizontalFreeWidth=visibleArea.width;
			horizontalFreeHeight=visibleArea.yMax-point.y-totalPointerLength;
		} else {
			//more space on the bottom
			horizontalFreeWidth=visibleArea.width;
			horizontalFreeHeight=point.y-visibleArea.yMin-totalPointerLength;
		}
		
		float verticalRemainingSurface=-1,horizontalRemainingSurface=-1;
		float verticalHiddenSurface=0,horizontalHiddenSurface=0;
		bool verticalFit=false,horizontalFit=false;
		
		if ((totalWidth<=verticalFreeWidth)&&(totalHeight<=verticalFreeHeight)) {
			verticalFit=true;
		} else {
			if (totalWidth>verticalFreeWidth) {
				verticalHiddenSurface+=(totalWidth-verticalFreeWidth)*totalHeight;
			}
			if (totalHeight>verticalFreeHeight) {
				verticalHiddenSurface+=(totalHeight-verticalFreeHeight)*totalWidth;
			}
		}
		verticalRemainingSurface=verticalFreeWidth*verticalFreeHeight-totalWidth*totalHeight;
		
		if ((totalWidth<=horizontalFreeWidth)&&(totalHeight<=horizontalFreeHeight)) {
			horizontalFit=true;
		} else {
			if (totalWidth>horizontalFreeWidth) {
				horizontalHiddenSurface+=(totalWidth-horizontalFreeWidth)*totalHeight;
			}
			if (totalHeight>horizontalFreeHeight) {
				horizontalHiddenSurface+=(totalHeight-horizontalFreeHeight)*totalWidth;
			}
		}
		horizontalRemainingSurface=horizontalFreeWidth*horizontalFreeHeight-totalWidth*totalHeight;
		
		bool chooseHorizontal;
		if (verticalFit && horizontalFit) {
			if (verticalRemainingSurface>horizontalRemainingSurface) {
				//choose vertical
				chooseHorizontal=false;
			} else {
				//choose horizontal
				chooseHorizontal=true;
			}
		} else if (horizontalFit) {
			//choose horizontal
			chooseHorizontal=true;
		} else if (verticalFit) {
			//choose vertical
			chooseHorizontal=false;
		} else {
			if (verticalHiddenSurface<horizontalHiddenSurface) {
				//choose vertical
				chooseHorizontal=false;
			} else {
				//choose horizontal
				chooseHorizontal=true;
			}
		}
		
		if (chooseHorizontal) {
			//horizontal
			if (point.y<visibleArea.center.y) {
				//more space on the top
				bubble.y=point.y+totalPointerLength+totalHeight*0.5f;
			} else {
				//more space on the bottom
				bubble.y=point.y-totalPointerLength-totalHeight*0.5f;
			}
			bubble.x=point.x;
			if (bubble.x+totalWidth*0.5f>visibleArea.xMax) {
				bubble.x=visibleArea.xMax-totalWidth*0.5f;
			} else if (bubble.x-totalWidth*0.5f<visibleArea.xMin) {
				bubble.x=visibleArea.xMin+totalWidth*0.5f;
			}
		} else {
			//vertical
			if (point.x<visibleArea.center.x) {
				//more space on the right
				bubble.x=point.x+totalPointerLength+totalWidth*0.5f;
			} else {
				//more space on the left
				bubble.x=point.x-totalPointerLength-totalWidth*0.5f;
			}
			bubble.y=point.y;
			if (bubble.y+totalHeight*0.5f>visibleArea.yMax) {
				bubble.y=visibleArea.yMax-totalHeight*0.5f;
			} else if (bubble.y-totalHeight*0.5f<visibleArea.yMin) {
				bubble.y=visibleArea.yMin+totalHeight*0.5f;
			}
		}
		
		bubble.SetSizeAndPointer(totalWidth,totalHeight,point-bubble.GetPosition(),pointerMargin);
		
		//bubble.alpha=0.25f;

		return bubble;
	}
	
	static public void TransitionPop(FNode node) {
		node.scaleX=0;
		node.scaleY=0.1f;
		TweenConfig config0=new TweenConfig().floatProp("scaleX",1f);
		config0.easeType=EaseType.ElasticOut;
		Go.to(node,0.2f,config0);
		TweenConfig config1=new TweenConfig().floatProp("scaleY",1f);
		config1.easeType=EaseType.ElasticOut;
		//config1.delay=0.15f;
		Go.to(node,0.4f,config1);
	}
	
	static public void TransitionFadeOut(FNode node,float delay) {
		TweenConfig config=new TweenConfig().floatProp("alpha",0f).onComplete(FxHelper.Instance.RemoveFromContainer);
		config.easeType=EaseType.ExpoOut;
		config.delay=delay;
		Go.to (node,0.5f,config);
	}
}