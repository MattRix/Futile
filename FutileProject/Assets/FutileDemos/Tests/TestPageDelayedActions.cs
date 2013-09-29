using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

public class PageTestDelayedActions : PageTest, FMultiTouchableInterface
{
	public PageTestDelayedActions()
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
	
	protected List<FSprite> _sprites;
	override public void Start()
	{
		_sprites=new List<FSprite>();
		
		//Build the grid
		FSprite sprite=new FSprite("Monkey_0");
		float scale=0.25f;
		float width=sprite.textureRect.width*scale;
		float height=sprite.textureRect.height*scale;
		int rows=4;
		int columns=5;
		for(int i=0;i<columns;i++) {
			float x=-((float)(columns-1)*0.5f-(float)i)*width;
			for(int j=0;j<rows;j++) {
				float y=((float)(rows-1)*0.5f-(float)j)*height;
				sprite=new FSprite("Monkey_0");
				sprite.scale=scale;
				sprite.x=x; sprite.y=y;
				_sprites.Add (sprite);
				AddChild(sprite);
			}
		}

		ShowTitle("ListDelayedActions\nClick to start the chain");
		base.Start();
	}
	
	protected void Click(Vector2 touch) {
	    ListDelayedActions<FSprite> parse=new ListDelayedActions<FSprite>(_sprites);
		parse.Parse(HandleAListObjectAction,HandleAListEndOfActions,0.05f,0.01f,0.0f);
	}
		
	protected void HandleAListObjectAction(FSprite sprite)
	{
		sprite.rotation+=30f;
	}
	protected void HandleAListEndOfActions()
	{
		Debug.Log("All sprites parsed, we can go on");
		TempMessage("Done!",1f);
	}		

	public void HandleMultiTouch(FTouch[] touches)
	{
		foreach(FTouch touch in touches)
		{
			if (touch.phase == TouchPhase.Ended) {
				Click(touch.position);
			}
		}
	}
}

