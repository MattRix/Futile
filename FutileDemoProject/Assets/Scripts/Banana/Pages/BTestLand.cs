using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class BTestLandPage : BPage, FMultiTouchableInterface
{
	public BTestLandPage()
	{
		Futile.atlasManager.LoadAtlas("Atlases/GameAtlas");
	}
	
	override public void HandleAddedToStage()
	{
		Futile.touchManager.AddMultiTouchTarget(this);
		Futile.instance.SignalUpdate += HandleUpdate;
		base.HandleAddedToStage();	
	}
	
	override public void HandleRemovedFromStage()
	{
		Futile.touchManager.RemoveMultiTouchTarget(this);
		Futile.instance.SignalUpdate -= HandleUpdate;
		base.HandleRemovedFromStage();	
	}
	
	override public void Start()
	{
		
//		Vector2[] vertices = new Vector2[]
//		{
//			new Vector2(-50,0),
//			new Vector2(-50,50),
//			new Vector2(50,100),
//			new Vector2(90,-30),
//			new Vector2(70,-30),
//			new Vector2(-10,-80),
//		};
//		
		
	}

	public void HandleTweenComplete (AbstractTween tween)
	{
		((tween as Tween).target as FNode).RemoveFromContainer();
	}
	
	private GameObject _testGO;
	
	protected void HandleUpdate ()
	{
		if(Input.GetMouseButtonDown(0))
		{
			int count = 200;
			
			Vector2[] vertices = new Vector2[count];
			
			for(int v = 0; v<count; v++)
			{
				float angle = (float)v/(float)count * RXMath.DOUBLE_PI;
				float radius = RXRandom.Range(100.0f,200.0f);
				
				vertices[v] = new Vector2(Mathf.Cos (angle) * radius, -Mathf.Sin(angle) * radius);
			}
			
			FPPolygonalData polygonalData = new FPPolygonalData(vertices);
			
			if(_testGO != null) UnityEngine.Object.Destroy(_testGO);
			
			_testGO = new GameObject("TestGO");
			
			FPPolygonalCollider mesh2DCollider = _testGO.AddComponent<FPPolygonalCollider>() as FPPolygonalCollider;
			mesh2DCollider.Init(polygonalData);
			
			FPDebugRenderer.Create(_testGO, this, 0xFF0000,false);
		}
	}
	
	public void HandleMultiTouch(FTouch[] touches)
	{
	}
}

