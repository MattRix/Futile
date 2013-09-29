using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;


public enum PageType
{
	None,
	PageTestMotionStreak,
	PageTestDelayedActions,
	PageTestBicolorSprite,
	PageTestSplitSprite,
	PageTestSpeechBubbles0,
	PageTestSpeechBubbles1,
	PageTestScrollContainer,
	PageTestDrawing,
}



public class PageTest : Page
{
	protected FButton _back;
	
	static public List<PageType> testPages;
	
	static PageTest() {
		testPages=new List<PageType>();
		
		foreach (PageType pageType in Enum.GetValues(typeof(PageType)))
		{
			if (pageType.ToString().StartsWith("PageTest")) {
				testPages.Add(pageType);
			}
		}
	}
	
	public PageTest()
	{
		
	}
	
	FLabel _titleLabel=null;
	FSprite _titleLabelBg=null;
	virtual public void ShowTitle(string title) {
		if (_titleLabel==null) {
			_titleLabelBg=new FSprite("Futile_White");
			AddChild(_titleLabelBg);
			_titleLabelBg.color=Color.black;
			_titleLabelBg.alpha=0.5f;
			_titleLabel=new FLabel(Config.fontFile,title,Config.textParams);
			AddChild(_titleLabel);
			_titleLabel.scale=0.75f;
		} else {
			_titleLabel.text=title;
		}
		_titleLabel.y=Futile.screen.halfHeight-_titleLabel.textRect.height*_titleLabel.scaleY*0.5f;
		_titleLabelBg.y=_titleLabel.y;
		_titleLabelBg.scaleX=(_titleLabel.textRect.width*_titleLabel.scaleX+10f)/_titleLabelBg.textureRect.width;
		_titleLabelBg.scaleY=(_titleLabel.textRect.height*_titleLabel.scaleY+10f)/_titleLabelBg.textureRect.height;
	}
	
	override public void Start()
	{
		_back=new FButton("Futile_White","Futile_White",null,null);
		_back.SetColors(new Color(0f,0f,1f),new Color(0f,0f,0.5f));
		_back.AddLabel(Config.fontFile,"Next",Color.white);
		_back.scaleX=6f;
		_back.label.scaleX=0.5f/_back.scaleX;
		_back.scaleY=2f;
		_back.label.scaleY=0.5f/_back.scaleY;

		
		_back.x=Futile.screen.halfWidth-_back.hitRect.width*_back.scaleX*0.5f;
		_back.y=-Futile.screen.halfHeight+_back.hitRect.height*_back.scaleY*0.5f;
		Futile.stage.AddChild(_back);
		
		_back.SignalRelease+=HandleNextButtonRelease;
	}
	
	private void HandleNextButtonRelease(FButton button)
	{
		//Debug.Log ("HandleNextButtonRelease");
		
		int i=0;
		for (;i<testPages.Count;i++) {
			if (Main.instance.currentPageType==testPages[i]) {
				break;
			}
		}
		i++; if (i>=testPages.Count) i=0;
		
		//show next test page
		Main.instance.FadeToPage(testPages[i]);
	}
	
	public void TempMessage(string text,float duration) {
		FLabel label=new FLabel(Config.fontFile,text,Config.textParams);
		label.color=Color.black;
		label.scale=0.5f;
		
		FSpeechBubble bubble=new FSpeechBubble();
		bubble.backgroundColor=Color.white;
		AddChild(bubble);
		
		label.y+=3f;
		bubble.AddChild(label);
		
		bubble.SetSizeAndPointer(label.textRect.width*label.scaleX+20f,label.textRect.height*label.scaleY+20f,new Vector2(0,0),0f);
		
		FSpeechBubbleManager.TransitionPop(bubble);
		FSpeechBubbleManager.TransitionFadeOut(bubble,duration);
	}
}
