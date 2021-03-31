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

		fparams.backgroundColor = Color.black;

		fparams.AddResolutionLevel(1000.0f, 1.0f, 1.0f, ""); 

		fparams.origin = new Vector2(0.5f, 0.5f);

		Futile.instance.Init(fparams);

		Futile.atlasManager.LoadImage("Box", false);
		Futile.atlasManager.LoadImage("simple_palette_wide_psd", false);
		Futile.atlasManager.LoadImage("floor3_m", false);

		StartDemo();
	}

	private void StartDemo()
	{
		Futile.stage.AddChild(demoContainer = new FContainer());

		var exampleSprite = new FSprite("floor3_m");
		exampleSprite.SetPosition(0,-64f);
		exampleSprite.shader = FancyColorSwapShader.TheShader;
		demoContainer.AddChild(exampleSprite);

		float boxWidth = 3f;
		float width = 256*boxWidth;
		int redIndex = 0;
		int greenIndex = 0;
		int blueIndex = 0;

		for(int i = 0; i<256; i++)
		{
			var box = new DemoBox(i);

			box.x = (-width/2f)+i*boxWidth + boxWidth/2f;

			box.onClick = ()=>
			{
				if(Input.GetKey(KeyCode.LeftShift)) greenIndex = box.index;
				else if(Input.GetKey(KeyCode.LeftControl)) blueIndex = box.index;
				else redIndex = box.index;
				
				exampleSprite.color = FancyColorSwapShader.GetColor(redIndex,greenIndex,blueIndex);

				Debug.Log($"Setting {redIndex},{greenIndex},{blueIndex} color is: {exampleSprite.color.r},{exampleSprite.color.g},{exampleSprite.color.b}");
			};

			demoContainer.AddChild(box);
		}

		var palSprite = new FSprite("simple_palette_wide_psd");
		palSprite.SetSize(width,32f);
		palSprite.SetPosition(0,-32f);
		demoContainer.AddChild(palSprite);
	}

	public class DemoBox : FContainer, FSingleTouchableInterface
	{
		public int index; 

		public FSprite sprite;

		public Action onClick;

		public DemoBox(int index)
		{
			this.index = index;

			sprite = new FSprite("Box");
			sprite.shader = FancyColorSwapShader.TheShader;
			sprite.SetSize(3f,32f);

			sprite.color = FancyColorSwapShader.GetColor(index,index,index);
			AddChild(sprite);

			//button = new FButton("Box");
			//button.alpha = 0;

			EnableSingleTouch();
		}

		public bool HandleSingleTouchBegan(FTouch touch)
		{
			if(sprite.localRect.Contains(sprite.GlobalToLocal(touch.position)))
			{
				Debug.Log($"clicking index: {index}");
				onClick?.Invoke();
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










