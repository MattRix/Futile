using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum BScreenType
{
	None,
	TitleScreen,
	InGameScreen,
	ScoreScreen
}

public class BMain : MonoBehaviour
{	
	public static BMain instance;
	
	public int score = 0;
	public int bestScore = 0;
	
	private BScreenType _currentScreenType = BScreenType.None;
	private BScreen _currentScreen = null;
	
	private FStage _stage;
	
	private void Start()
	{
		instance = this; 
		
		Go.defaultEaseType = EaseType.Linear;
		Go.duplicatePropertyRule = DuplicatePropertyRuleType.RemoveRunningProperty;
		
		//Time.timeScale = 0.1f;
		
		FEngine.instance.Init (10,10);
		
		FEngine.atlasManager.LoadAtlas("Atlases/BananaLargeAtlas", false);
		FEngine.atlasManager.LoadAtlas("Atlases/BananaGameAtlas", false);
		FEngine.atlasManager.LoadFont("Franchise","FranchiseFontAtlas.png", "Atlases/FranchiseLarge", 0.8f,0.8f);
		
		_stage = FEngine.stage;
		
		BSoundPlayer.PlayRegularMusic();
		
		GoToScreen(BScreenType.TitleScreen);
	}
	
	public void GoToScreen (BScreenType screenType)
	{
		if(_currentScreenType == screenType) return; //we're already on the same screen, so don't bother
		
		BScreen screenToCreate = null;
		
		if(screenType == BScreenType.TitleScreen)
		{
			screenToCreate = new BTitleScreen();
		}
		else if (screenType == BScreenType.InGameScreen)
		{
			screenToCreate = new BInGameScreen();
		}  
		else if (screenType == BScreenType.ScoreScreen)
		{
			screenToCreate = new BScoreScreen();
		}
		
		if(screenToCreate != null) //destroy the old screen and create a new one
		{
			_currentScreenType = screenType;	
			
			if(_currentScreen != null)
			{
				_currentScreen.Destroy();
				_stage.RemoveChild(_currentScreen);
			}
			 
			_currentScreen = screenToCreate;
			_stage.AddChildAtIndex(_currentScreen,0);
			_currentScreen.Start();
		}
		
	}
	
	public void Update()
	{
		if(_currentScreen != null) _currentScreen.Advance();	
	}
}









