using UnityEngine;
using System.Collections;
using System;

public class BTitleScreen : BScreen
{
	private FSprite _background;
	private FSprite _logo;
	private BLabelButton _startButton;
	private int _frameCount = 0;
	
	public BTitleScreen()
	{
		
	}
	
	override public void Start()
	{
		_background = new FSprite("JungleClearBG.png");
		AddChild(_background);
		
		_logo = new FSprite("MainLogo.png");
		AddChild(_logo);
		_logo.x = 0.0f;
		_logo.y = 15.0f;
		
		_startButton = new BLabelButton("START!");
		AddChild(_startButton);
		_startButton.x = FEngine.halfWidth-75.0f;
		_startButton.y = -FEngine.halfHeight+35.0f;
		
		_startButton.OnTap += HandleStartButtonTap;
		
		
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

	private void HandleStartButtonTap (object sender, EventArgs e)
	{
		BMain.instance.GoToScreen(BScreenType.InGameScreen);
	}
	
	override public void Advance ()
	{
		_logo.rotation = -5.0f +  RXMath.PingPong(_frameCount, 300) * 10.0f;
		
		if(_frameCount > 45) //wait until after the buildin
		{
			//make the banana rock back and forth
			_startButton.scale = 0.94f+ RXMath.PingPong(_frameCount, 90) * 0.06f;
		}
			
		_frameCount++;
	}
	
	override public void Destroy()
	{
		_background.RemoveFromContainer();
		_logo.RemoveFromContainer();
		_startButton.RemoveFromContainer();
	}
}

