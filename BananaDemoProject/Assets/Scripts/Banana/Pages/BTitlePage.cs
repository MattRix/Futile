using UnityEngine;
using System.Collections;
using System;

public class BTitlePage : BPage
{
	private FSprite _background;
	private FSprite _logo;
	private BLabelButton _startButton;
	private int _frameCount = 0;
	
	
	public BTitlePage()
	{
		
	}
	override public void HandleAddedToStage()
	{
		Futile.instance.SignalUpdate += HandleUpdate;
		base.HandleAddedToStage();	
	}
	
	override public void HandleRemovedFromStage()
	{
		Futile.instance.SignalUpdate -= HandleUpdate;
		base.HandleRemovedFromStage();	
	}
	
	
	override public void Start()
	{
		_background = new FSprite("JungleClearBG.png");
		AddChild(_background);
		
		//this will scale the background up to fit the screen
		//but it won't let it shrink smaller than 100%
		_background.scale = Math.Max (Math.Max(1.0f,Futile.height/_background.height),Futile.width /_background.width);
		 
		_logo = new FSprite("MainLogo.png");
		AddChild(_logo);
		_logo.x = 0.0f;
		_logo.y = 15.0f;
		
		_startButton = new BLabelButton("START!");
		AddChild(_startButton);
		_startButton.x = Futile.halfWidth-75.0f;
		_startButton.y = -Futile.halfHeight+35.0f;
		
		_startButton.SignalTap += HandleStartButtonTap;
		
		
		_logo.scale = 0.0f;
		
		Go.to(_logo, 0.5f, new TweenConfig().
			setDelay(0.1f).
			floatProp("scale",1.0f).
			setEaseType(EaseType.BackOut));
		
		
		_startButton.scale = 0.0f;
		
		Go.to(_startButton, 0.5f, new TweenConfig().
			setDelay(0.3f).
			floatProp("scale",1.0f).
			setEaseType(EaseType.BackOut));
	}

	private void HandleStartButtonTap ()
	{
		BMain.instance.GoToPage(BPageType.InGamePage);
	}
	
	protected void HandleUpdate ()
	{
		_logo.rotation = -5.0f +  RXMath.PingPong(_frameCount, 300) * 10.0f;
		
		_frameCount++;
	}

}

