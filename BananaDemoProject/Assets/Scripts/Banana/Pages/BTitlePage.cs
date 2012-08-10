using UnityEngine;
using System.Collections;
using System;

public class BTitlePage : BPage
{
	private FSprite _background;
	private FContainer _logoHolder;
	private FSprite _logo;
	private BLabelButton _startButton;
	private int _frameCount = 0;
	
	
	public BTitlePage()
	{
		
	}
	override public void HandleAddedToStage()
	{
		Futile.instance.SignalUpdate += HandleUpdate;
		Futile.instance.SignalResize += HandleResize;
		base.HandleAddedToStage();	
	}
	
	override public void HandleRemovedFromStage()
	{
		Futile.instance.SignalUpdate -= HandleUpdate;
		Futile.instance.SignalResize -= HandleResize;
		base.HandleRemovedFromStage();	
	}
	
	
	override public void Start()
	{
		_background = new FSprite("JungleClearBG.png");
		AddChild(_background);
		
		//this will scale the background up to fit the screen
		//but it won't let it shrink smaller than 100%
		
		_logoHolder = new FContainer();
		
		AddChild (_logoHolder);

		_logo = new FSprite("MainLogo.png");
		
		_logoHolder.AddChild(_logo);
		
		_startButton = new BLabelButton("START!");
		AddChild(_startButton);

		_startButton.SignalTap += HandleStartButtonTap;
		
		
		_logoHolder.scale = 0.0f;
		
		Go.to(_logoHolder, 0.5f, new TweenConfig().
			setDelay(0.1f).
			floatProp("scale",1.0f).
			setEaseType(EaseType.BackOut));
		
		
		_startButton.scale = 0.0f;
		
		Go.to(_startButton, 0.5f, new TweenConfig().
			setDelay(0.3f).
			floatProp("scale",1.0f).
			setEaseType(EaseType.BackOut));
		
		HandleResize(true); //force resize to position everything at the start
	}
	
	protected void HandleResize(bool wasOrientationChange)
	{
		//this will scale the background up to fit the screen
		//but it won't let it shrink smaller than 100%
		_background.scale = Math.Max (1.0f,Math.Max (Futile.height/_background.boundsRect.height,Futile.width/_background.boundsRect.width));
		
		_logoHolder.x = 0.0f;
		_logoHolder.y = 15.0f;
		
		_startButton.x = Futile.halfWidth-75.0f;
		_startButton.y = -Futile.halfHeight+35.0f;
		
		//scale the logo so it fits on the main screen 
		_logo.scale = Math.Min(1.0f,Futile.width/_logo.boundsRect.width);
		
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

