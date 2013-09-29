using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Main : MonoBehaviour
{	
	public static Main instance;

	private PageType _currentPageType = PageType.None, _transitionPageType = PageType.None;
	private Page _currentPage = null;
	
	private FStage _stage;
	
	public Page currentPage { get { return _currentPage; } }
	public PageType currentPageType { get { return _currentPageType; } }
	
	private void Start()
	{
		instance = this; 
		
		Go.defaultEaseType = EaseType.Linear;
		Go.duplicatePropertyRule = DuplicatePropertyRuleType.RemoveRunningProperty;
		
		//Time.timeScale = 0.1f;
		
		//bool hasPro = UnityEditorInternal.InternalEditorUtility.HasPro();
		//Debug.Log("Unity Pro : "+hasPro);

		bool isIPad = SystemInfo.deviceModel.Contains("iPad");
		
		bool shouldSupportPortraitUpsideDown = isIPad; //only support portrait upside-down on iPad
		
		FutileParams fparams = new FutileParams(true,true,true,shouldSupportPortraitUpsideDown);
		
		fparams.AddResolutionLevel(480.0f,	1.0f,	1.0f,	"_Scale1"); //iPhone
		fparams.AddResolutionLevel(960.0f,	2.0f,	2.0f,	"_Scale2"); //iPhone retina
		fparams.AddResolutionLevel(1024.0f,	2.0f,	2.0f,	"_Scale2"); //iPad
		fparams.AddResolutionLevel(1280.0f,	2.0f,	2.0f,	"_Scale2"); //Nexus 7
		fparams.AddResolutionLevel(2048.0f,	4.0f,	4.0f,	"_Scale4"); //iPad Retina

		fparams.shouldLerpToNearestResolutionLevel=false;
		
		fparams.origin = new Vector2(0.5f,0.5f);
		
		Futile.instance.Init (fparams);
		
		Futile.atlasManager.LoadAtlas("Atlases/BananaGameAtlas");
		Futile.atlasManager.LoadFont(Config.fontFile,Config.fontFile+Futile.resourceSuffix, "Atlases/"+Config.fontFile+Futile.resourceSuffix, 0.0f,-4.0f);
		
		_stage = Futile.stage;
		
		FadeToPage(PageTest.testPages[0]);
	}
	
	//Change page with transition
	public void FadeToPage (PageType pageType) {
		FadeToPage(pageType,false);
	}
	public void FadeToPage (PageType pageType,bool force) {
		FadeToPage(pageType, Color.black, 1f,force);
		//FadeToPage(pageType, Color.blue, 2f);
	}
	public void FadeToPage (PageType pageType, Color color, float duration) {
		FadeToPage(pageType,color,duration,false);
	}
	public void FadeToPage (PageType pageType, Color color, float duration, bool force) {
		if(_currentPageType == pageType)  if (!force) return; //we're already on the same page, so don't bother doing anything
		
		_transitionPageType=pageType;
			
		FSprite fadeSprite=new FSprite("Futile_White");
		fadeSprite.scaleX=Futile.screen.width/fadeSprite.textureRect.width;
		fadeSprite.scaleY=Futile.screen.height/fadeSprite.textureRect.height;
		fadeSprite.color=color;
		fadeSprite.alpha=0f;
		_stage.AddChild(fadeSprite);
		
		TweenConfig config0=new TweenConfig().floatProp("alpha",1f).onComplete(MiddleTransition);
		
		config0.setEaseType(EaseType.Linear);
		//config0.setEaseType(EaseType.ExpoIn);
		//config0.setEaseType(EaseType.ElasticIn);
		Go.to (fadeSprite, duration*0.5f, config0);
	}
	protected void MiddleTransition(AbstractTween tween) {
		GoToPage(_transitionPageType,true);
		_transitionPageType = PageType.None;
		
		FSprite fadeSprite=(FSprite)(((Tween)tween).target);
		fadeSprite.RemoveFromContainer();
		_stage.AddChild(fadeSprite);
		
		TweenConfig config0=new TweenConfig().floatProp("alpha",0f).onComplete(FxHelper.Instance.RemoveFromContainer);
		config0.setEaseType(EaseType.Linear);
		//config0.setEaseType(EaseType.ExpoOut);
		//config0.setEaseType(EaseType.ElasticIn);
		Go.to(fadeSprite, tween.duration, config0);
	}
	
	//Change page without transition
	public void GoToPage (PageType pageType) {
		GoToPage (pageType,false);
	}
	public void GoToPage (PageType pageType,bool force) {
		if(_currentPageType == pageType) if (!force) return; //we're already on the same page, so don't bother doing anything
		
		Page pageToCreate = null;
		
		Debug.Log(pageType.ToString());
		
		Type type = Type.GetType(pageType.ToString()); 
		object o=Activator.CreateInstance(type);
		pageToCreate = (Page)o;
		
		//pageToCreate = (TPage)(System.Reflection.Assembly.GetExecutingAssembly().CreateInstance(pageType.ToString()));
		
		
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

	
	/*
	public void LateUpdate() {
        Boolean takeHiResShot = Input.GetKeyDown("k");
        if (takeHiResShot) {
			Debug.Log("takeHiResShot="+takeHiResShot);
            //FutileUtils.ScreenShot();
        }
    }
    */
}









