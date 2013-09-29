using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

public class PageTestBicolorSprite : PageTest, FMultiTouchableInterface
{
	public PageTestBicolorSprite()
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
	
	protected FBicolorSprite _bicolorSprite0,_bicolorSprite1;
	override public void Start()
	{
		_bicolorSprite0=new FBicolorSprite("Monkey_0"); //used as a reference
	    //_bicolorSprite0.bottomColor=RandomUtils.RandomColor();
	    //_bicolorSprite0.topColor=RandomUtils.RandomColor();
		AddChild(_bicolorSprite0);
		_bicolorSprite0.x=-_bicolorSprite0.textureRect.width*0.5f;
		
		_bicolorSprite1=new FBicolorSprite("Monkey_0");
	    _bicolorSprite1.bottomColor=RandomUtils.RandomColor();
	    _bicolorSprite1.topColor=RandomUtils.RandomColor();
		AddChild(_bicolorSprite1);
		_bicolorSprite1.x=_bicolorSprite1.textureRect.width*0.5f;
		
		ShowTitle("FBicolorSprite\nClick to fade to new colors");
		base.Start();
	}
	
	protected void Click(Vector2 touch) {
	    TweenConfig config;
		//config=new TweenConfig().colorProp("bottomColor",RandomUtils.RandomColor()).colorProp("topColor",RandomUtils.RandomColor());
	    //Go.to (_bicolorSprite0,0.25f,config);
		config=new TweenConfig().colorProp("bottomColor",RandomUtils.RandomColor()).colorProp("topColor",RandomUtils.RandomColor());
	    Go.to (_bicolorSprite1,0.25f,config);
	}

	public void HandleMultiTouch(FTouch[] touches)
	{
		foreach(FTouch touch in touches)
		{
			if (touch.phase == TouchPhase.Ended) {
				Click(touch.position);
			}
		}
	}
}

