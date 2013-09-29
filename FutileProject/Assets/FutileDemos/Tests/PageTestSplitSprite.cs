using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

public class PageTestSplitSprite : PageTest, FMultiTouchableInterface
{
	public PageTestSplitSprite()
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
	
	
	FSplitSprite _sprite;
	FLabel _label;
	HealthBar _bar;
	override public void Start()
	{
		ShowTitle("FSplitSprite\nClick anywhere to change splitRatio.");
		
		_sprite=new FSplitSprite("Banana");
		//_sprite.splitRatio=RXRandom.Float();
		_sprite.bottomColor=Color.white;
		_sprite.topColor=Color.gray;
		_sprite.scale=2f;
		_sprite.y=40f;
		AddChild(_sprite);
		_label=new FLabel(Config.fontFile,"splitRatio=\n"+_sprite.splitRatio);
		AddChild(_label);
		_label.scale=0.5f;
		_label.y=_sprite.y-_sprite.textureRect.height*0.5f*_sprite.scaleY;
		
		
		_bar=new HealthBar(2,2,50,8,0.5f);
		//_bar.alpha=1f;
		AddChild(_bar);
		_bar.y=_label.y-20-_label.textRect.height*_label.scaleY*0.5f;
		
		
		FLabel label=new FLabel(Config.fontFile,"HealthBar by Matt");
		AddChild(label);
		label.scale=0.5f;
		label.y=_bar.y-label.textRect.height*label.scaleY*0.5f-8;
		
		
		
		base.Start();
	}
	
	protected void ChangeSplitRatio(Vector2 pos) {
		float newSplitRatio=(pos.y+Futile.screen.halfHeight)/Futile.screen.height;
		_sprite.splitRatio=newSplitRatio;
		_bar.percentage=newSplitRatio;
		_label.text="splitRatio=\n"+_sprite.splitRatio;
	}
	
	public void HandleMultiTouch(FTouch[] touches)
	{
		foreach(FTouch touch in touches)
		{
			if (touch.phase == TouchPhase.Ended) {
				ChangeSplitRatio(touch.position);
			} else if (touch.phase == TouchPhase.Moved) {
				ChangeSplitRatio(touch.position);
			}
		}
	}
}