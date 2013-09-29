using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FScrollContainer : FContainer, FSingleTouchableInterface
{
	protected FScrollTouchManager _touchManager;
	protected FContainer _contentContainer;
	
	protected float _width=0f,_height=0f;
	protected float _contentWidth=0f,_contentHeight=0f;
	protected Vector2 _scrollingSpeed=Vector2.zero;
	protected bool _isTouchDown=false,_oneMoreUpdate=false;
	protected Rect _hitRect;
	protected Vector2 _totalScroll=Vector2.zero;
	
	
	public delegate void MagnetReachDelegate(FScrollContainer scrollContainer,int magnetIdx);
	public event MagnetReachDelegate SignalMagnetReach;
	
	
	//magnets (pages)
	protected List<Vector2> _magnets;
	
	public FScrollContainer (float width, float height) : base()
	{
		_contentWidth=width;
		_contentHeight=height;
		
		_magnets=new List<Vector2>();
		ResetClosestMagnet();
		
		_touchManager=new FScrollTouchManager();
		_contentContainer=new FContainer();
		AddChild(_contentContainer);
		
		SetSize (width,height);
	}
	
	public List<Vector2> magnets { get { return _magnets; } }
	
	public FContainer contentContainer {
		get { return _contentContainer; }
	}
	
	public FTouchManager touchManager {
		get { return _touchManager; }
	}
	
	virtual public void SetContentSize(float width, float height) {
		_contentWidth=width;
		_contentHeight=height;
		CheckContentBorders();
	}
	
	virtual public void SetSize(float width, float height) {
		_width=width;
		_height=height;
		
		_hitRect.width = _width;
		_hitRect.height = _height;
		_hitRect.x = -0.5f*_width;
		_hitRect.y = -0.5f*_height;
		
		CheckContentBorders();
	}
	
	override public void HandleAddedToStage()
	{
		Futile.touchManager.AddSingleTouchTarget(this);
		Futile.instance.SignalUpdate += HandleUpdate;
		base.HandleAddedToStage();	
	}
	
	override public void HandleRemovedFromStage()
	{
		Futile.touchManager.RemoveSingleTouchTarget(this);
		Futile.instance.SignalUpdate -= HandleUpdate;
		base.HandleRemovedFromStage();
	}
	
	public void HandleUpdate() {
		if (_isTouchDown) {
			_touchManager.Update();
		} else {
			if (_oneMoreUpdate) {
				_touchManager.Update();
				_oneMoreUpdate=false;
			}
			if (_touchManager.isScrolling) {
				_scrollingSpeed*=0.92f;
				if (_scrollingSpeed.sqrMagnitude<4f) {
					if (!MagnetsMove()) {
						_scrollingSpeed=Vector2.zero;
						_touchManager.isScrolling=false;
						if(SignalMagnetReach != null) SignalMagnetReach(this,_closestMagnetIdx);
						ResetClosestMagnet();
					}
				} else {
					Move();
				}
			}
		}
	}
	
	virtual protected void CheckContentBorders() {
		if (_contentContainer.x-_contentWidth*0.5f>-_width*0.5f) {
			_contentContainer.x=-_width*0.5f+_contentWidth*0.5f;
		} else if (_contentContainer.x+_contentWidth*0.5f<_width*0.5f) {
			_contentContainer.x=_width*0.5f-_contentWidth*0.5f;
		}
		
		if (_contentContainer.y-_contentHeight*0.5f>-_height*0.5f) {
			_contentContainer.y=-_height*0.5f+_contentHeight*0.5f;
		} else if (_contentContainer.y+_contentHeight*0.5f<_height*0.5f) {
			_contentContainer.y=_height*0.5f-_contentHeight*0.5f;
		}
	}
	
	Vector2 _closestMagnet;
	int _closestMagnetIdx;
	protected void ResetClosestMagnet() {
		_closestMagnet=new Vector2(1000000000,1000000000);
		_closestMagnetIdx=-1;
	}
	virtual protected void SetClosestMagnet() {
		float sqDist=1000000000f;
		Vector2 contentContainerPos=_contentContainer.GetPosition();
		//foreach (Vector2 magnet in _magnets) {
		for (int i=0;i<_magnets.Count;i++) {
			Vector2 magnet=_magnets[i];
			Vector2 diff=contentContainerPos-magnet;
			if (diff.sqrMagnitude<sqDist) {
				sqDist=diff.sqrMagnitude;
				_closestMagnet=magnet;
				_closestMagnetIdx=i;
			}
		}
	}
	virtual protected bool MagnetsMove() {
		if (_magnets.Count>0) {
			//find closest magnet
			if (_closestMagnetIdx==-1) {
				SetClosestMagnet();
			}
			
			//move towards closest magnet
			/*
			Vector2 force=closestMagnet-contentContainerPos;
			force.Normalize();
			force/=(float)Math.Pow(sqDist,0.1f);
			_scrollingSpeed.x+=10*force.x;
			_scrollingSpeed.y+=10*force.y;
			Debug.Log("MagnetsMove closestMagnet="+closestMagnet+" force="+force);
			*/
			
			Vector2 contentContainerPos=_contentContainer.GetPosition();
			contentContainerPos=contentContainerPos+(_closestMagnet-contentContainerPos)*0.2f;
			
			Vector2 memoPos=_contentContainer.GetPosition();
			
			_contentContainer.SetPosition(contentContainerPos);
			CheckContentBorders();
			
			Vector2 speed=_contentContainer.GetPosition()-memoPos;
			
			return (speed.sqrMagnitude>0.1f);
		}
		return false;
	}
	
	virtual protected void Move() {
		Vector2 memoPos=_contentContainer.GetPosition();

		_contentContainer.x+=_scrollingSpeed.x;
		_contentContainer.y+=_scrollingSpeed.y;

		CheckContentBorders();
		
		_scrollingSpeed.x=_contentContainer.x-memoPos.x;
		_scrollingSpeed.y=_contentContainer.y-memoPos.y;
	}
	
	protected Vector2 _memo=Vector2.zero;
	//Memorize position of the content
	public void Memo() {
		_memo=new Vector2(_contentContainer.x,_contentContainer.y);
	}
	//Restore  position of the content to the one saved when calling Memo()
	public void Restore() {
		_contentContainer.x=_memo.x; _contentContainer.y=_memo.y;
		CheckContentBorders();
	}
	
	protected Tween _moveToAnim=null;
	//Translate content by v (animated if duration is >0)
	virtual public bool MoveContentBy(Vector2 v,float duration) {
		return MoveContentTo(new Vector2(_contentContainer.x+v.x,_contentContainer.y+v.y),duration);
	}
	//Move content to position v (animated if duration is >0)
	virtual public bool MoveContentTo(Vector2 v,float duration) {
		Memo();
		//Debug.Log ("v="+v);
		_contentContainer.x=v.x; _contentContainer.y=v.y;
		CheckContentBorders();
		v.x=_contentContainer.x; v.y=_contentContainer.y;
		Restore();
		
		if ((v.x!=_contentContainer.x)||(v.y!=_contentContainer.y)) {
			if (duration>0) {
				if (_moveToAnim!=null) {
					_moveToAnim.destroy();
				}
				//Debug.Log ("v="+v);
				TweenConfig config0=new TweenConfig().floatProp("x",v.x).floatProp("y",v.y).onComplete(HandleMoveToDone);
				config0.easeType=EaseType.ExpoOut;
				_moveToAnim = Go.to (_contentContainer,duration,config0);
			} else {
				_contentContainer.x=v.x; _contentContainer.y=v.y;
				CheckContentBorders();
			}
			return true;
		}
		return false;
	}
	virtual protected void HandleMoveToDone(AbstractTween tween) {
		_moveToAnim.destroy();
		_moveToAnim=null;
	}
	
	//Move content to make the zone rect visible (rect is in the local coordinates of the contentContainer
	virtual public bool MoveToMakeGlobalZoneVisible(Rect globRect,float duration) {
		Vector2 min=_contentContainer.GlobalToLocal(new Vector2(globRect.xMin,globRect.yMin));
		Vector2 max=_contentContainer.GlobalToLocal(new Vector2(globRect.xMax,globRect.yMax));
		
		return MoveToMakeContentZoneVisible(new Rect(min.x,min.y,max.x-min.x,max.y-min.y),duration);
	}
	virtual public bool MoveToMakeContentZoneVisible(Rect rect,float duration) {
		Vector2 min=_contentContainer.GlobalToLocal(this.LocalToGlobal(new Vector2(-_width*0.5f,-_height*0.5f)));
		Vector2 max=_contentContainer.GlobalToLocal(this.LocalToGlobal(new Vector2(_width*0.5f,_height*0.5f)));
		
		Rect visibleRect=new Rect(min.x,min.y,max.x-min.x,max.y-min.y);
		//bool intersect=Rect.CheckIntersect(visibleRect,rect);
		if (/*intersect &&*/ (rect.xMin>=visibleRect.xMin)&& (rect.yMin>=visibleRect.yMin)&&(rect.xMax<=visibleRect.xMax)&& (rect.yMax<=visibleRect.yMax)) {
			//do nothing rect is in visible rect
			return false;
		} else {
			Vector2 moveBy;
			if (rect.width>visibleRect.width) {
				moveBy.x=(float)(visibleRect.xMin-(rect.width-visibleRect.width)*0.5)-rect.xMin;
			} else {
				if (rect.xMin<=visibleRect.xMin) {
					moveBy.x=visibleRect.xMin-rect.xMin;
				} else {
					moveBy.x=visibleRect.xMax-rect.xMax;
				}
			}
			if (rect.height>visibleRect.height) {
				moveBy.y=(float)(visibleRect.yMin-(rect.height-visibleRect.height)*0.5)-rect.yMin;
			} else {
				if (rect.yMin<=visibleRect.yMin) {
					moveBy.y=visibleRect.yMin-rect.yMin;
				} else {
					moveBy.y=visibleRect.yMax-rect.yMax;
				}
			}
			return MoveContentTo(new Vector2(_contentContainer.x+moveBy.x,_contentContainer.y+moveBy.y), duration);
		}
	}
	

	virtual public bool HandleSingleTouchBegan(FTouch touch)
	{
		
		_scrollingSpeed=Vector2.zero;
		_isTouchDown = false;
		
		Vector2 touchPos = this.GetLocalTouchPosition(touch);
		if(_hitRect.Contains(touchPos))
		{
			_isTouchDown = true;
			_totalScroll=Vector2.zero;
			ResetClosestMagnet();
			return true;
		}
		
		return false;
	}
	
	virtual public void HandleSingleTouchMoved(FTouch touch)
	{
		if (_isTouchDown) {
			
			_scrollingSpeed=touch.deltaPosition;
			Move();
			if (!_touchManager.isScrolling) {
				//_totalScroll+=_scrollingSpeed;
				//better to add deltaPosition, if we move the finger to scroll and content doesn't scroll becasue we are on the border, we might click on a button not on purpose
				_totalScroll+=touch.deltaPosition;
				if (_totalScroll.sqrMagnitude>25f) { //set the touchManager in scroll mode after 5 pixels moved
					_touchManager.isScrolling=true;
				}
			}
		}
	}
	
	virtual public void HandleSingleTouchEnded(FTouch touch)
	{
		_isTouchDown=false;
		_oneMoreUpdate=true;
	}
	
	virtual public void HandleSingleTouchCanceled(FTouch touch)
	{
		_isTouchDown=false;
		_oneMoreUpdate=true;
	}
}

