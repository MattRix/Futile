using UnityEngine;
using System.Collections;
using System;

public class BScorePage : BPage
{
	private FSprite _background;
	private BMonkey _monkey;
	private FButton _againButton;
	private FLabel _scoreLabel;
	private FLabel _bestScoreLabel;
	private int _frameCount = 0;
	private bool _isNewBestScore = false;
	
	public BScorePage()
	{
		
	}
	override public void HandleAddedToStage()
	{
		Futile.instance.SignalUpdate += HandleUpdate;
		Futile.screen.SignalResize += HandleResize;
		base.HandleAddedToStage();	
	}
	
	override public void HandleRemovedFromStage()
	{
		Futile.instance.SignalUpdate -= HandleUpdate;
		Futile.screen.SignalResize -= HandleResize;
		base.HandleRemovedFromStage();	
	}
	
	
	override public void Start()
	{
		_background = new FSprite("JungleBlurryBG.png");
		AddChild(_background);
		
		//this will scale the background up to fit the screen
		//but it won't let it shrink smaller than 100%

		_monkey = new BMonkey();
		AddChild(_monkey);
		_monkey.x = -5.0f;
		_monkey.y = -2.0f;
		
		_againButton = new FButton("YellowButton_normal.png", "YellowButton_over.png", "ClickSound");
		_againButton.AddLabel("Franchise","AGAIN?",new Color(0.45f,0.25f,0.0f,1.0f));
		
		AddChild(_againButton);
		_againButton.y = -110.0f;
		
		_againButton.SignalRelease += HandleAgainButtonRelease;
		
		_scoreLabel = new FLabel("Franchise", BMain.instance.score+" Bananas");
		AddChild(_scoreLabel);
		
		_scoreLabel.color = new Color(1.0f,0.9f,0.2f);
		_scoreLabel.y = 110.0f;
		
		if(BMain.instance.score > BMain.instance.bestScore)
		{
			BMain.instance.bestScore = BMain.instance.score;	
			_isNewBestScore = true;
		}
		else
		{
			_isNewBestScore = false;	
		}
		
		_bestScoreLabel = new FLabel("Franchise", "Best score: " + BMain.instance.bestScore+" Bananas");
		AddChild(_bestScoreLabel);
		_bestScoreLabel.scale = 0.5f;
		_bestScoreLabel.anchorX = 1.0f;
		_bestScoreLabel.anchorY = 0.0f;
		_bestScoreLabel.color = new Color(1.0f,0.9f,0.2f);
		
		
		
		_scoreLabel.scale = 0.0f;
		
		Go.to(_scoreLabel, 0.5f, new TweenConfig().
			setDelay(0.3f).
			floatProp("scale",1.0f).
			setEaseType(EaseType.BackOut));
		
		_monkey.scale = 0.0f;
		
		Go.to(_monkey, 0.5f, new TweenConfig().
			setDelay(0.1f).
			floatProp("scale",1.0f).
			setEaseType(EaseType.BackOut));
		
		
		_againButton.scale = 0.0f;
		
		Go.to(_againButton, 0.5f, new TweenConfig().
			setDelay(0.3f).
			floatProp("scale",1.0f).
			setEaseType(EaseType.BackOut));

		HandleResize(true); //force resize to position everything at the start
	}
	
	protected void HandleResize(bool wasOrientationChange)
	{
		//this will scale the background up to fit the screen
		//but it won't let it shrink smaller than 100%
		_background.scale = Math.Max (Math.Max(1.0f,Futile.screen.height/_background.textureRect.height),Futile.screen.width/_background.textureRect.width);
		 
		_bestScoreLabel.x = Futile.screen.halfWidth - 5;
		_bestScoreLabel.y = -Futile.screen.halfHeight + 5;
	}

	private void HandleAgainButtonRelease (FButton button)
	{
		BSoundPlayer.PlayRegularMusic();
		BMain.instance.GoToPage(BPageType.InGamePage); 
	}
	
	protected void HandleUpdate ()
	{
		if(_frameCount % 24 < 12) //make the score blink every 12 frames
		{
			_scoreLabel.color = new Color(1.0f,1.0f,0.5f);
			if(_isNewBestScore) _bestScoreLabel.color = new Color(1.0f,1.0f,0.5f);
		}
		else 
		{
			_scoreLabel.color = new Color(1.0f,0.9f,0.2f);	
			if(_isNewBestScore) _bestScoreLabel.color = new Color(1.0f,0.9f,0.2f);	
		}
		
		_frameCount++;
	}

}

