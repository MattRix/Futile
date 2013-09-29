using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

public class PageTestMotionStreak : PageTest, FMultiTouchableInterface
{
	protected FMotionStreakSprite _draw=null;
	
	public PageTestMotionStreak()
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
		FLabel label=new FLabel(Config.fontFile,"FMotionStreakSprite demo\nDraw anywhere",Config.textParams);
		AddChild(label);
		base.Start();
	}

	public void HandleMultiTouch(FTouch[] touches)
	{
		foreach(FTouch touch in touches)
		{
			if (touch.phase == TouchPhase.Ended) {
				if (_draw!=null) {
					_draw.RemoveFromContainerAnimated();
					_draw=null;
				}
			} else if (touch.phase == TouchPhase.Began) {
				if (_draw==null) {
					float c0=RXRandom.Float();
					float c1=RXRandom.Float();
					float c2=RXRandom.Float();
					float c3=RXRandom.Float();
					float c4=RXRandom.Float();
					float c5=RXRandom.Float();
					_draw = new FMotionStreakWithBorderSprite("Futile_White", // texture name
						10+RXRandom.Int(20),  //Number of quads in the trail
						x => 15.0f*(float)Math.Sin (1.0f*x*x*x*Math.PI),  // width of the band function, x represents the offset in the band and 0<= x <=1
						x => new Color(c0,c1,c2,(float)Math.Sin (1.0f*x*Math.PI)),  //color of the band function, x represents the offset in the band and 0<= x <=1
						x => 10f*(float)Math.Sin (1.0f*x*Math.PI),
						x => new Color(c3,c4,c5,(float)Math.Sin (1.0f*x*Math.PI))
						);
					AddChild(_draw);
				}
				_draw.PushPosition(touch.position);
			} else if (touch.phase == TouchPhase.Moved) {
				if (_draw!=null) _draw.PushPosition(touch.position);
			}
		}
	}
}
