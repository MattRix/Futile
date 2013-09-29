using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

public class PageTestScrollContainer : PageTest
{
	public PageTestScrollContainer()
	{
	}
	
	
	override public void Start()
	{
		FLabel label;

		FScrollContainer scroll=new FScrollContainer(100,Futile.screen.height);
		scroll.SetContentSize(100,600);
		scroll.x=0;
		AddChild(scroll);
		FSprite scrollSprite;
		
		scroll.magnets.Add(new Vector2(0,-100));
		scroll.magnets.Add(new Vector2(0,100));

		
		scrollSprite=new FSprite("Futile_White");
		scrollSprite.color=new Color(1f,0f,0f);
		scrollSprite.scaleX=100f/scrollSprite.textureRect.width;
		scrollSprite.scaleY=150f/scrollSprite.textureRect.height;
		scrollSprite.y=-300f+75f;
		scroll.contentContainer.AddChild(scrollSprite);
		
		scrollSprite=new FSprite("Futile_White");
		scrollSprite.color=new Color(0f,1f,0f);
		scrollSprite.scaleX=100f/scrollSprite.textureRect.width;
		scrollSprite.scaleY=150f/scrollSprite.textureRect.height;
		scrollSprite.y=-300f+150f+75f;
		scroll.contentContainer.AddChild(scrollSprite);
		
		scrollSprite=new FSprite("Futile_White");
		scrollSprite.color=new Color(1f,0f,0f);
		scrollSprite.scaleX=100f/scrollSprite.textureRect.width;
		scrollSprite.scaleY=150f/scrollSprite.textureRect.height;
		scrollSprite.y=-300f+150f*2+75f;
		scroll.contentContainer.AddChild(scrollSprite);
		
		scrollSprite=new FSprite("Futile_White");
		scrollSprite.color=new Color(0f,1f,0f);
		scrollSprite.scaleX=100f/scrollSprite.textureRect.width;
		scrollSprite.scaleY=150f/scrollSprite.textureRect.height;
		scrollSprite.y=-300f+150f*3+75f;
		scroll.contentContainer.AddChild(scrollSprite);
		
		FScrollButton button=new FScrollButton("Futile_White", "Futile_White", null, null);
		button.SetColors(new Color(0f,0f,1f),new Color(0f,0f,0.5f));
		
		button.AddLabel(Config.fontFile,"BUTTON",Color.white);
		button.scale=5f;
		button.label.scale=0.1f;
		
		button.SignalRelease += HandleScrollButtonRelease;
		scroll.contentContainer.AddChild(button);

		
		foreach (Vector2 magnet in scroll.magnets) {
			label=new FLabel(Config.fontFile,"Magnet",Config.textParams);
			label.color=Color.black;
			label.y=magnet.y;
			scroll.contentContainer.AddChild(label);
		}

		
		ShowTitle("FScrollContainer with magnets");
		
		
		//scroll.MoveContentTo(new Vector2(20,100),3f);
		scroll.MoveToMakeContentZoneVisible(new Rect(20,200,1,1),1f);
		
		base.Start();
	}
	
	
	private void HandleScrollButtonRelease (FButton button)
	{
		Debug.Log ("SCROLL BUTTON CLICK");
		
		FPseudoHtmlText text=new FPseudoHtmlText(Config.fontFile,"<style text-color='#111111'>Click!</style>",Config.textParams,100f,PseudoHtmlTextAlign.center,1f,this);
		FSpeechBubble bubble=FSpeechBubbleManager.Instance.Show(text,text.width,text.height,FSpeechBubbleManager.Instance.defaultContentMarginX,FSpeechBubbleManager.Instance.defaultContentMarginY,button.LocalToGlobal(Vector2.zero),FSpeechBubbleManager.Instance.defaultPointerLength,10f,FSpeechBubbleManager.Instance.defaultBackgroundColor,this,FSpeechBubbleManager.Instance.defaultVisibleArea);
		FSpeechBubbleManager.TransitionPop(bubble);
		FSpeechBubbleManager.TransitionFadeOut(bubble,1f);
		
	}
}