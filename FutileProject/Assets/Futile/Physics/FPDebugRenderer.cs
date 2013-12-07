using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class FPDebugRenderer : MonoBehaviour
{
	public static bool IS_ENABLED = true;
	
	public static FPDebugRenderer Create(GameObject targetGameObject, FContainer container, uint color, bool shouldUpdateColliders)
	{
		if(!IS_ENABLED) return null;
		
		FPDebugRenderer debugRenderer = targetGameObject.AddComponent<FPDebugRenderer>();
		debugRenderer.Init(container, color, shouldUpdateColliders);
		return debugRenderer;
	}
	
	private FContainer _container;
	private FContainer _drawContainer;
	
	private List<FNode> _nodes = new List<FNode>(1);
	
	private Color _color;
	
	public bool shouldUpdateColliders;
	
	public void Init(FContainer container, uint color, bool shouldUpdateColliders)
	{
		_container = container;	
		
		_container.AddChild(_drawContainer = new FContainer());
		
		_color = RXUtils.GetColorFromHex(color);
		
		this.shouldUpdateColliders = shouldUpdateColliders;
		
		Collider[] colliders = gameObject.GetComponents<Collider>();
		
		int colliderCount = colliders.Length;
		
		for(int c = 0; c<colliderCount; c++)
		{
			Collider collider = colliders[c];
			
			FNode newNode = null;
			
			if(collider is BoxCollider)
			{
				FSprite sprite = new FSprite("Debug/Square");
				sprite.color = _color;
				
				newNode = sprite;
			}
			else if(collider is SphereCollider)
			{
				FSprite sprite = new FSprite("Debug/Circle");
				sprite.color = _color;
				
				newNode = sprite;
			}
			
			if(newNode != null)
			{
				_drawContainer.AddChild(newNode);	
				_nodes.Add(newNode);
			}
		}
		
		FPPolygonalCollider mesh2D = gameObject.GetComponent<FPPolygonalCollider>();
		
		if(mesh2D != null)
		{
			FPDebugPolygonColliderView debugView = new FPDebugPolygonColliderView("Debug/Triangle", mesh2D);
			debugView.color = _color;
			
			_drawContainer.AddChild(debugView);	
			_nodes.Add(debugView);
		}
		
		Update();
		if(!shouldUpdateColliders) UpdateColliders(); //always update the colliders the first time
	}
	
	public void Update() 
	{
		_drawContainer.x = gameObject.transform.position.x*FPhysics.METERS_TO_POINTS;
		_drawContainer.y = gameObject.transform.position.y*FPhysics.METERS_TO_POINTS;
	
		_drawContainer.rotation = -gameObject.transform.rotation.eulerAngles.z;
		
		if(shouldUpdateColliders) UpdateColliders();
	}
	
	public void UpdateColliders()
	{
		//todo: recreate collider draw nodes	
		Collider[] colliders = gameObject.GetComponents<Collider>();
		
		int colliderCount = colliders.Length;
		
		for(int c = 0; c<colliderCount; c++)
		{
			Collider collider = colliders[c];
			
			if(collider is BoxCollider)
			{
				BoxCollider box = collider as BoxCollider;
					
				FSprite sprite = _nodes[c] as FSprite;
				sprite.width = box.size.x * FPhysics.METERS_TO_POINTS;
				sprite.height = box.size.y * FPhysics.METERS_TO_POINTS;
				sprite.x = box.center.x * FPhysics.METERS_TO_POINTS;
				sprite.y = box.center.y * FPhysics.METERS_TO_POINTS;
			}
			else if(collider is SphereCollider)
			{
				SphereCollider sphere = collider as SphereCollider;
				FSprite sprite = _nodes[c] as FSprite;
				sprite.width = sprite.height = sphere.radius * 2.0f * FPhysics.METERS_TO_POINTS;
				sprite.x = sphere.center.x * FPhysics.METERS_TO_POINTS;
				sprite.y = sphere.center.y * FPhysics.METERS_TO_POINTS;
			}
		}
	}
	
	public void OnDestroy()
	{
		_drawContainer.RemoveFromContainer();
	}

}

