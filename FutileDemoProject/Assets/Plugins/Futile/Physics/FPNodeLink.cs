using UnityEngine;
using System.Collections;
using System;

public class FPNodeLink : MonoBehaviour
{
	private FNode _node;
	private bool _shouldLinkRotation;
	private bool _shouldUseLocalPosition = false;
	
	public void Init(FNode node, bool shouldLinkRotation)
	{
		_node = node;	
		_shouldLinkRotation = shouldLinkRotation;
		Update();
	}
	
	public void Update() 
	{
		if (_node == null)
		{
			Debug.Log("_node is null for GameObject: " + gameObject.name);
		}
		
		if(_shouldUseLocalPosition)
		{
			_node.x = gameObject.transform.localPosition.x*FPhysics.METERS_TO_POINTS;
			_node.y = gameObject.transform.localPosition.y*FPhysics.METERS_TO_POINTS;
		}
		else 
		{
			_node.x = gameObject.transform.position.x*FPhysics.METERS_TO_POINTS;
			_node.y = gameObject.transform.position.y*FPhysics.METERS_TO_POINTS;
		}
		
		if(_shouldLinkRotation)
		{
			_node.rotation = -gameObject.transform.rotation.eulerAngles.z;
		}
	}
	
	public FNode node
	{
		get {return _node;}	
	}
	
	public bool shouldLinkRotation
	{
		get {return _shouldLinkRotation;}
		set 
		{
			if(_shouldLinkRotation != value)
			{
				_shouldLinkRotation = value;
				if(_shouldLinkRotation) Update();
			}
		}	
	}
	
	public bool shouldUseLocalPosition
	{
		get {return _shouldUseLocalPosition;}
		set 
		{
			if(_shouldUseLocalPosition != value)
			{
				_shouldUseLocalPosition = value;
				if(_shouldUseLocalPosition) Update();
			}
		}	
	}
}

