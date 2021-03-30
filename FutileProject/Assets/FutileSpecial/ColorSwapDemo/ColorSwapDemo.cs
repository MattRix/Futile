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

		bool shouldSupportPortraitUpsideDown = isIPad; //only support portrait upside-down on iPad

		FutileParams fparams = new FutileParams(true, true, true, shouldSupportPortraitUpsideDown);

		fparams.AddResolutionLevel(480.0f, 1.0f, 1.0f, "_Scale1"); //iPhone
		fparams.AddResolutionLevel(960.0f, 2.0f, 2.0f, "_Scale2"); //iPhone retina
		fparams.AddResolutionLevel(1024.0f, 2.0f, 2.0f, "_Scale2"); //iPad
		fparams.AddResolutionLevel(1280.0f, 2.0f, 2.0f, "_Scale2"); //Nexus 7
		fparams.AddResolutionLevel(2048.0f, 4.0f, 4.0f, "_Scale4"); //iPad Retina

		fparams.origin = new Vector2(0.5f, 0.5f);

		Futile.instance.Init(fparams);

		Futile.atlasManager.LoadAtlas("Atlases/BananaLargeAtlas");
		Futile.atlasManager.LoadAtlas("Atlases/BananaGameAtlas");

		Futile.atlasManager.LoadFont("Franchise", "FranchiseFont" + Futile.resourceSuffix, "Atlases/FranchiseFont" + Futile.resourceSuffix, 0.0f, -4.0f);

		Futile.atlasManager.LoadImage("Box", false);

		StartDemo();
	}

	private void StartDemo()
	{
		Futile.stage.AddChild(demoContainer = new FContainer());

		for(int i = 0; i<256; i++)
		{
			int x = i % 16;
			int y = i / 16;

			var box = new DemoBox(x,y,i);

			box.x = x*8;
			box.y = y*8;

			demoContainer.AddChild(box);
		}
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
			sprite.SetSize(8f,8f);

			sprite.color = FancyColorSwapShader.GetColor(index,0,0);
			AddChild(sprite);

			EnableSingleTouch();
		}

		public bool HandleSingleTouchBegan(FTouch touch)
		{
			if(sprite.localRect.Contains(sprite.ScreenToLocal(touch.position)))
			{
				Debug.Log($"touching {boxX},{boxY} with index:{index}");
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










