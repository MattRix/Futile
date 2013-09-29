using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

public class PageTestDrawing : PageTest, FMultiTouchableInterface
{
	protected FDrawingSprite _draw=null;
	
	public PageTestDrawing()
	{
	}

	override public void HandleAddedToStage()
	{
		Futile.touchManager.AddMultiTouchTarget(this);
		base.HandleAddedToStage();	
	}
	
	override public void HandleRemovedFromStage()
	{
		Futile.touchManager.RemoveMultiTouchTarget(this);
		base.HandleRemovedFromStage();
	}
	
	
	override public void Start()
	{
		FLabel label=new FLabel(Config.fontFile,"FDrawingSprite demo\nDraw anywhere",Config.textParams);
		AddChild(label);
		base.Start();
	}

	public void HandleMultiTouch(FTouch[] touches)
	{
		foreach(FTouch touch in touches)
		{
			if (touch.phase == TouchPhase.Ended) {
				if (_draw!=null) {
					_draw.Flush();
				}
			} else if (touch.phase == TouchPhase.Began) {
				if (_draw!=null) {
					_draw.RemoveFromContainer();
					_draw=null;
				}
				_draw=new FDrawingSprite("Futile_White");
				AddChild(_draw);
		
				_draw.SetLineThickness(RXRandom.Range(4f,10f));
				Color color=RandomUtils.RandomColor();
				color.a=1f;
				_draw.SetLineColor(color);
				
				_draw.PushBorder (RXRandom.Range(2f,5f),RandomUtils.RandomColor(),RXRandom.Float()<0.5f?true:false);
				_draw.PushTopBorder (RXRandom.Range(2f,5f),RandomUtils.RandomColor(),RXRandom.Float()<0.5f?true:false);
				//_draw.PushBorder (4,new Color(1f,1f,0f,1f),false);
				if (RXRandom.Float()<0.5f) {
					_draw.PushBorder (RXRandom.Range(1f,4f),RandomUtils.RandomColor(),RXRandom.Float()<0.5f?true:false);
					if (RXRandom.Float()<0.5f) {
						_draw.PushBorder (RXRandom.Range(1f,4f),RandomUtils.RandomColor(),RXRandom.Float()<0.5f?true:false);
						if (RXRandom.Float()<0.5f) {
							_draw.PushBorder (RXRandom.Range(1f,4f),RandomUtils.RandomColor(),RXRandom.Float()<0.5f?true:false);
						}
					}
				}
				
				_draw.MoveTo(touch.position.x,touch.position.y);
			} else if (touch.phase == TouchPhase.Moved) {
				if (_draw==null) return;
				_draw.LineTo(touch.position.x,touch.position.y);
			}
		}
	}
}
