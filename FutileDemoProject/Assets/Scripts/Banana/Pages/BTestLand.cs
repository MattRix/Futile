using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class BTestLandPage : BPage, FMultiTouchableInterface
{
	private FPWorld _world;
	
	public BTestLandPage()
	{
		Futile.atlasManager.LoadAtlas("Atlases/GameAtlas");
		
		ListenForUpdate(HandleUpdate);
		EnableMultiTouch();
		
		this.x = -Futile.screen.halfWidth;
		this.y = -Futile.screen.halfHeight;
	}
	
	override public void Start()
	{
		_world = FPWorld.Create(1.0f);
		
List<Vector2[]> polygons = new List<Vector2[]>();

Vector2[] vertices = new Vector2[46];
vertices[0] = new Vector2(20.5f,104f);
vertices[1] = new Vector2(35.5f,104.5f);
vertices[2] = new Vector2(49.5f,84f);
vertices[3] = new Vector2(55f,68.5f);
vertices[4] = new Vector2(62f,57f);
vertices[5] = new Vector2(80f,44.5f);
vertices[6] = new Vector2(110f,40f);
vertices[7] = new Vector2(135f,41f);
vertices[8] = new Vector2(154.5f,58.5f);
vertices[9] = new Vector2(175f,68.5f);
vertices[10] = new Vector2(195f,69.5f);
vertices[11] = new Vector2(216.5f,55.5f);
vertices[12] = new Vector2(232.5f,41.5f);
vertices[13] = new Vector2(253f,35f);
vertices[14] = new Vector2(272.5f,39.5f);
vertices[15] = new Vector2(289f,51f);
vertices[16] = new Vector2(309.5f,67.5f);
vertices[17] = new Vector2(321f,73f);
vertices[18] = new Vector2(337.5f,76f);
vertices[19] = new Vector2(354.5f,70f);
vertices[20] = new Vector2(366f,60.5f);
vertices[21] = new Vector2(370f,46f);
vertices[22] = new Vector2(366f,33.5f);
vertices[23] = new Vector2(348f,21f);
vertices[24] = new Vector2(332.5f,11f);
vertices[25] = new Vector2(290f,4.5f);
vertices[26] = new Vector2(228f,3f);
vertices[27] = new Vector2(194f,3.5f);
vertices[28] = new Vector2(173f,10f);
vertices[29] = new Vector2(156.5f,17f);
vertices[30] = new Vector2(144.5f,18.5f);
vertices[31] = new Vector2(120.5f,14.5f);
vertices[32] = new Vector2(101f,11.5f);
vertices[33] = new Vector2(86f,10f);
vertices[34] = new Vector2(68.5f,9.5f);
vertices[35] = new Vector2(41.5f,17.5f);
vertices[36] = new Vector2(29f,25.5f);
vertices[37] = new Vector2(25f,33.5f);
vertices[38] = new Vector2(16.5f,52f);
vertices[39] = new Vector2(8f,69f);
vertices[40] = new Vector2(4.5f,83.5f);
vertices[41] = new Vector2(5f,94f);
vertices[42] = new Vector2(7f,100.5f);
vertices[43] = new Vector2(11f,103.5f);
vertices[44] = new Vector2(14f,104.5f);
vertices[45] = new Vector2(15.5f,105f);
polygons.Add(vertices);


vertices = new Vector2[4];
vertices[0] = new Vector2(34f,142f);
vertices[1] = new Vector2(111.5f,91.5f);
vertices[2] = new Vector2(87f,75f);
vertices[3] = new Vector2(24.5f,133f);
polygons.Add(vertices);


vertices = new Vector2[15];
vertices[0] = new Vector2(105.5f,192f);
vertices[1] = new Vector2(123.5f,194.5f);
vertices[2] = new Vector2(132.5f,194.5f);
vertices[3] = new Vector2(143.5f,188f);
vertices[4] = new Vector2(146f,180f);
vertices[5] = new Vector2(149f,167.5f);
vertices[6] = new Vector2(142f,155f);
vertices[7] = new Vector2(129.5f,149.5f);
vertices[8] = new Vector2(112f,146.5f);
vertices[9] = new Vector2(97f,151.5f);
vertices[10] = new Vector2(93f,158f);
vertices[11] = new Vector2(89.5f,168.5f);
vertices[12] = new Vector2(89.5f,176f);
vertices[13] = new Vector2(92f,183.5f);
vertices[14] = new Vector2(99f,186.5f);
polygons.Add(vertices);


vertices = new Vector2[4];
vertices[0] = new Vector2(202.5f,151.5f);
vertices[1] = new Vector2(203f,186.5f);
vertices[2] = new Vector2(247.5f,192.5f);
vertices[3] = new Vector2(272f,161.5f);
polygons.Add(vertices);


vertices = new Vector2[16];
vertices[0] = new Vector2(181f,229.5f);
vertices[1] = new Vector2(183.5f,242.5f);
vertices[2] = new Vector2(207f,248f);
vertices[3] = new Vector2(216.5f,247f);
vertices[4] = new Vector2(228.5f,232f);
vertices[5] = new Vector2(229f,223f);
vertices[6] = new Vector2(221.5f,213.5f);
vertices[7] = new Vector2(211f,207.5f);
vertices[8] = new Vector2(197.5f,206.5f);
vertices[9] = new Vector2(184f,208.5f);
vertices[10] = new Vector2(173f,213f);
vertices[11] = new Vector2(162f,220.5f);
vertices[12] = new Vector2(155.5f,229.5f);
vertices[13] = new Vector2(152f,233f);
vertices[14] = new Vector2(152f,242.5f);
vertices[15] = new Vector2(154.5f,250.5f);
polygons.Add(vertices);


vertices = new Vector2[9];
vertices[0] = new Vector2(300f,185f);
vertices[1] = new Vector2(293.5f,233f);
vertices[2] = new Vector2(350f,240.5f);
vertices[3] = new Vector2(351.5f,140.5f);
vertices[4] = new Vector2(334.5f,138.5f);
vertices[5] = new Vector2(334.5f,177.5f);
vertices[6] = new Vector2(335f,221f);
vertices[7] = new Vector2(312.5f,221.5f);
vertices[8] = new Vector2(312.5f,190.5f);
polygons.Add(vertices);


vertices = new Vector2[17];
vertices[0] = new Vector2(171f,117f);
vertices[1] = new Vector2(174.5f,130f);
vertices[2] = new Vector2(211f,126f);
vertices[3] = new Vector2(261f,132f);
vertices[4] = new Vector2(289.5f,142f);
vertices[5] = new Vector2(284.5f,161f);
vertices[6] = new Vector2(299f,162f);
vertices[7] = new Vector2(306f,147.5f);
vertices[8] = new Vector2(303.5f,137f);
vertices[9] = new Vector2(284.5f,122f);
vertices[10] = new Vector2(261f,117.5f);
vertices[11] = new Vector2(247.5f,115.5f);
vertices[12] = new Vector2(230f,113.5f);
vertices[13] = new Vector2(218.5f,112.5f);
vertices[14] = new Vector2(203.5f,111.5f);
vertices[15] = new Vector2(193f,111f);
vertices[16] = new Vector2(184.5f,111f);
polygons.Add(vertices);
		
		for(int p = 0; p<polygons.Count; p++)
		{
			Vector2[] verts = polygons[p];
			
			bool shouldMove = p>2;
			
			FPPolygonalData polygonalData = new FPPolygonalData(verts, true);
			
			_testGO = new GameObject("TestGO");
		
			FPPolygonalCollider mesh2DCollider = _testGO.AddComponent<FPPolygonalCollider>() as FPPolygonalCollider;
			mesh2DCollider.Init(polygonalData);
		
			if(shouldMove)
			{
				_testGO.AddComponent<Rigidbody>();	
                _testGO.rigidbody.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
				_testGO.rigidbody.mass = 10.0f;
			}
			
			FPDebugRenderer.Create(_testGO, this, 0xFF0000,false);
		}
		
		
		
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
			/*
			int count = 200;
			
			Vector2[] vertices = new Vector2[count];
			
			for(int v = 0; v<count; v++)
			{
				float angle = (float)v/(float)count * RXMath.DOUBLE_PI;
				float radius = RXRandom.Range(100.0f,200.0f);
				
				vertices[v] = new Vector2(Mathf.Cos (angle) * radius, -Mathf.Sin(angle) * radius);
			}
			
			FPPolygonalData polygonalData = new FPPolygonalData(vertices, false);
			
			if(_testGO != null) UnityEngine.Object.Destroy(_testGO);
			
			_testGO = new GameObject("TestGO");
			
			FPPolygonalCollider mesh2DCollider = _testGO.AddComponent<FPPolygonalCollider>() as FPPolygonalCollider;
			mesh2DCollider.Init(polygonalData);
			
			FPDebugRenderer.Create(_testGO, this, 0xFF0000,false);
			*/
		}
	}
	
	public void HandleMultiTouch(FTouch[] touches)
	{
	}
}

