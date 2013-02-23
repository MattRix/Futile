using UnityEngine;
using System;

public class FWatcherLink_FNode : FWatcherLink
{
	public float x;
	public float y;
	
	public float scaleX;
	public float scaleY;
	
	public float rotation;
}

public class FWatcherLink_FLabel : FWatcherLink
{
	public string text;
	public float anchorX;
	public float anchorY;
	public Color color;
}

public class FWatcherLink_FSprite : FWatcherLink
{
	public float anchorX;
	public float anchorY;
	public Color color;
}

//public class FWatcherLink_FSprite : FWatcherLink
//{
//	public float anchorX;
//	public float anchorY;
//	
//	private float _anchorX;
//	private float _anchorY;
//	
//	public Color color;
//	
//	private Color _color;
//	
//	override protected void SetupTarget()
//	{
//		FSprite sprite = GetTarget() as FSprite;
//		
//		_anchorX = anchorX = sprite.anchorX;
//		_anchorY = anchorY = sprite.anchorY;
//		_color = color = sprite.color;
//	}
//	
//	override protected void UpdateTarget()
//	{
//		FSprite sprite = GetTarget() as FSprite;
//		
//		if (anchorX != _anchorX) sprite.anchorX = anchorX;
//		if (anchorY != _anchorY) sprite.anchorY = anchorY;
//		if (color != _color) sprite.color = color;
//
//		_anchorX = anchorX = sprite.anchorX;
//		_anchorY = anchorY = sprite.anchorY;
//		_color = color = sprite.color;
//	}
//	
//}




