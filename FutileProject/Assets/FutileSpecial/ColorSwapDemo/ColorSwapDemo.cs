using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ColorSwapDemo : MonoBehaviour
{	
	FContainer demoContainer;

	private void Start()
	{
		Go.defaultEaseType = EaseType.Linear;
		Go.duplicatePropertyRule = DuplicatePropertyRuleType.RemoveRunningProperty;

		bool isIPad = SystemInfo.deviceModel.Contains("iPad");

		FutileParams fparams = new FutileParams(true, false, false, false);

		fparams.AddResolutionLevel(1000.0f, 1.0f, 1.0f, ""); 

		fparams.origin = new Vector2(0.5f, 0.5f);

		Futile.instance.Init(fparams);

		Futile.atlasManager.LoadImage("Box", false);
		Futile.atlasManager.LoadImage("simple_palette_wide_psd", false);

		StartDemo();
	}

	private void StartDemo()
	{
		Futile.stage.AddChild(demoContainer = new FContainer());

		int cols = 256;
		float boxWidth = 3f;
		float width = cols*boxWidth;

		for(int i = 0; i<256; i++)
		{
			int x = i % cols;
			int y = i / cols;

			var box = new DemoBox(x,y,i);

			box.x = (-width/2f)+x*boxWidth + boxWidth/2f;
			box.y = y*4;

			demoContainer.AddChild(box);
		}

		var palSprite = new FSprite("simple_palette_wide_psd");
		palSprite.SetSize(width,32f);
		palSprite.SetPosition(0,-32f);
		demoContainer.AddChild(palSprite);
	}

	public class DemoBox : FContainer, FSingleTouchableInterface
	{
		public int boxX;
		public int boxY;
		public int index; 

		public FSprite sprite;

		public DemoBox(int boxX, int boxY, int index)
		{
			this.boxX = boxX;
			this.boxY = boxY;
			this.index = index;

			sprite = new FSprite("Box");
			sprite.shader = FancyColorSwapShader.TheShader;
			sprite.SetSize(3f,32f);

			sprite.color = FancyColorSwapShader.GetColor(index,0,0);
			AddChild(sprite);

			//button = new FButton("Box");
			//button.alpha = 0;

			EnableSingleTouch();
		}

		public bool HandleSingleTouchBegan(FTouch touch)
		{
			if(sprite.localRect.Contains(sprite.GlobalToLocal(touch.position)))
			{
				Debug.Log($"clicking {boxX},{boxY} with index:{index}");
				return true;
			}

			return false;
		}

		public void HandleSingleTouchCanceled(FTouch touch)
		{
		}

		public void HandleSingleTouchEnded(FTouch touch)
		{
		}

		public void HandleSingleTouchMoved(FTouch touch)
		{
		}
	}
}










