using UnityEngine;
using System.Collections;
using System;

public class FScrollButton : FButton
{
	
	public FScrollButton (string upElementName, string downElementName, string overElementName, string clickSoundName) : base(upElementName, downElementName, overElementName, clickSoundName)		
	{
		DisableSingleTouch(); // Because we need the button already added to the stage to decide which touch manager to register with
	}
	
	override public void HandleAddedToStage()
	{
		base.HandleAddedToStage();	
		EnableScrollSingleTouch();
	}
	
	override public void HandleRemovedFromStage()
	{
		DisableScrollSingleTouch();
		base.HandleRemovedFromStage();
	}
	
	public void EnableScrollSingleTouch()
	{
		DisableSingleTouch();
		DisableScrollSingleTouch(); //clear any old ones first
		AddEnabler(new FNodeEnablerForScrollSingleTouch(this));
	}
	
	public void DisableScrollSingleTouch()
	{
		RemoveEnablerOfType(typeof(FNodeEnablerForScrollSingleTouch));
	}
}