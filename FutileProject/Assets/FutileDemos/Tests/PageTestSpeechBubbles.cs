using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

public class PageTestSpeechBubbles0 : PageTest, FMultiTouchableInterface
{
	public PageTestSpeechBubbles0()
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
	
	
	override public void Start()
	{
		FLabel label=new FLabel(Config.fontFile,"Click anywhere",Config.textParams);
		AddChild(label);
		base.Start();
	}
	
	protected FSpeechBubble _bubble=null;
	protected void TestSpeechBubbles(Vector2 touch) {
		if (_bubble!=null) {
			FSpeechBubbleManager.TransitionFadeOut(_bubble,0f);
		}
		FPseudoHtmlText text=new FPseudoHtmlText(Config.fontFile,"<style text-color='#111111'><style text-scale='0.5'>FSpeechBubble</style><br/><style text-scale='0.3'>Combined with a FPseudoHtmlText. How cool is that? <style text-color='#FF99FF'><br/><fsprite width='50' src='Monkey_0'/> With FSprites and FButtons inside, that's pretty cool! <fbutton src='YellowButton_normal' label='FButtons' scale='0.5' label-scale='0.5' color-down='#FF0000' action='MyMethodNameWithData' data='mybutttonid'/></style></style>",Config.textParams,200f,PseudoHtmlTextAlign.left,1f,this);
		//_bubble=FSpeechBubbleManager.Instance.Show(text,touch,10f);	
		FSpeechBubbleManager.Instance.defaultContainer=this;
		_bubble=FSpeechBubbleManager.Instance.Show(text,touch,10f);
		FSpeechBubbleManager.TransitionPop(_bubble);
		
		//_back.MoveToFront();
	}
	
	public void MyMethodNameWithData(object data) {
		Debug.Log("FButton clicked in speach bubble MyMethodNameWithData : "+data);
	}
	
	public void HandleMultiTouch(FTouch[] touches)
	{
		foreach(FTouch touch in touches)
		{
			if (touch.phase == TouchPhase.Ended) {
				TestSpeechBubbles(touch.position);
			}
		}
	}
}


public class PageTestSpeechBubbles1 : PageTest, FMultiTouchableInterface
{
	public PageTestSpeechBubbles1()
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
	
	
	override public void Start()
	{
		FLabel label=new FLabel(Config.fontFile,"Click anywhere\noutside the box",Config.textParams);
		label.color=Color.black;
		
		_bubble=new FSpeechBubble();
		_bubble.backgroundColor=Color.white;
		AddChild(_bubble);
		
		label.y+=3f;
		_bubble.AddChild(label);
		
		_bubble.SetSizeAndPointer(label.textRect.width+10f,label.textRect.height+40f,new Vector2(0,0),0f);

		base.Start();
	}
	
	protected FSpeechBubble _bubble=null;
	protected void TestSpeechBubbles(Vector2 touch) {
		_bubble.SetSizeAndPointer(_bubble.width,_bubble.height,touch,0f);
		//_back.MoveToFront();
	}
	
	public void HandleMultiTouch(FTouch[] touches)
	{
		foreach(FTouch touch in touches)
		{
			if (touch.phase == TouchPhase.Ended) {
				TestSpeechBubbles(touch.position);
			} else if (touch.phase == TouchPhase.Moved) {
				TestSpeechBubbles(touch.position);
			}
		}
	}
}