using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum BPageType
{
	None,
	TitlePage,
	InGamePage,
	ScorePage
}

public class BMain : MonoBehaviour
{	
	public static BMain instance;
	
	public int score = 0;
	public int bestScore = 0;
	
	private BPageType _currentPageType = BPageType.None;
	private BPage _currentPage = null;
	
	private FStage _stage;
	
	private void Start()
	{
		instance = this; 
		
		Go.defaultEaseType = EaseType.Linear;
		Go.duplicatePropertyRule = DuplicatePropertyRuleType.RemoveRunningProperty;
		
		//Time.timeScale = 0.1f;
		
		bool isIPad = SystemInfo.deviceModel.Contains("iPad");
		
		bool shouldSupportPortraitUpsideDown = isIPad; //only support portrait upside-down on iPad
		
		FutileParams fparams = new FutileParams(true,true,true,shouldSupportPortraitUpsideDown);
		
		fparams.AddResolutionLevel(480.0f,	1.0f,	1.0f,	"_Scale1"); //iPhone
		fparams.AddResolutionLevel(960.0f,	2.0f,	2.0f,	"_Scale2"); //iPhone retina
		fparams.AddResolutionLevel(1024.0f,	2.0f,	2.0f,	"_Scale2"); //iPad
		fparams.AddResolutionLevel(1280.0f,	2.0f,	2.0f,	"_Scale2"); //Nexus 7
		fparams.AddResolutionLevel(2048.0f,	4.0f,	4.0f,	"_Scale4"); //iPad Retina
		
		fparams.origin = new Vector2(0.5f,0.5f);
		
		Futile.instance.Init (fparams);
		
		Futile.atlasManager.LoadAtlas("Atlases/BananaLargeAtlas");
		Futile.atlasManager.LoadAtlas("Atlases/BananaGameAtlas");
		
		Futile.atlasManager.LoadFont("Franchise","FranchiseFont"+Futile.resourceSuffix+".png", "Atlases/FranchiseFont"+Futile.resourceSuffix);
		
		_stage = Futile.stage;
		
		BSoundPlayer.PlayRegularMusic();
		
		GoToPage(BPageType.TitlePage);
	}

	public void GoToPage (BPageType pageType)
	{
		if(_currentPageType == pageType) return; //we're already on the same page, so don't bother doing anything
		
		BPage pageToCreate = null;
		
		if(pageType == BPageType.TitlePage)
		{
			pageToCreate = new BTitlePage();
		}
		else if (pageType == BPageType.InGamePage)
		{
			pageToCreate = new BInGamePage();
		}  
		else if (pageType == BPageType.ScorePage)
		{
			pageToCreate = new BScorePage();
		}
		
		if(pageToCreate != null) //destroy the old page and create a new one
		{
			_currentPageType = pageType;	
			
			if(_currentPage != null)
			{
				_stage.RemoveChild(_currentPage);
			}
			 
			_currentPage = pageToCreate;
			_stage.AddChild(_currentPage);
			_currentPage.Start();
		}
		
	}
	
}









