using System;
using System.Collections.Generic;
using UnityEngine;

public class RXScroller
{
	//note: these are variables in case you want to set them on a per-scroller basis
	
	public float MAX_DRAG_SPEED = 20.0f; //maximum drag speed in pixels-per-update
	public float EDGE_SQUISH = 60.0f; //how far to go past the end in pixels
	public float EDGE_SQUISH_RATIO = 0.55f; //keep below 1, it's the ratio of edge squish (0.55 is Apple's default) 
	public float EDGE_BOUNCE = 0.19f; //how much force to use to bounce back
	public float STRONG_FRICTION = 0.75f; //used to bring it to a stop quicker
	public float WEAK_FRICTION = 0.92f; //used when throwing at high speed
	public float SLOW_SPEED = 3.0f; //below this speed it will be brought to a stop quickly
	
	private bool _isDragging = false;
	
	private float _pos;
	private float _speed;
	
	private float _basePos;
	private float _baseTouchPos;
	
	private float _previousPos;
	
	private float _boundsMin;
	private float _boundsMax;

	private float _dragSpeed = 0;
	private bool _shouldDetermineSpeed = false;
	
	public RXScroller(float pos, float boundsMin, float boundsMax)
	{
		_pos = 0;
		_speed = 0;
		_basePos = 0;
		_baseTouchPos = 0;
		_previousPos = 0;
		_boundsMin = boundsMin;
		_boundsMax = boundsMax;
	}
	
	public void BeginDrag(float touchPos)
	{
		if(_isDragging) return;
		
		_isDragging = true;
		
		_baseTouchPos = touchPos;
		
		_basePos = _pos;
		_dragSpeed = 0;
	}

	public void EndDrag(float touchPos)
	{
		if(!_isDragging) return;
		
		_isDragging = false;
		
		UpdateDrag(touchPos);

		_shouldDetermineSpeed = true;
	}

	public void CancelDrag()
	{
		if(!_isDragging) return;
		
		_isDragging = false;
		
		_speed = 0;
		_dragSpeed = 0;
	}
	
	//returns true if it's still moving
	public bool Update()
	{
		_dragSpeed += (_pos-_previousPos - _dragSpeed) * 0.5f;

		if(_shouldDetermineSpeed)
		{
			_shouldDetermineSpeed = false;
			_speed = _dragSpeed;
		}

		_previousPos = _pos;
		
		if(_isDragging) return true;
		
		float diff = 0; //diff is the amount of movement needed to bring pos back in bounds
		
		if(_pos < _boundsMin)
		{
			diff = _boundsMin - _pos;
		}
		else if(_pos > _boundsMax)
		{
			diff = _boundsMax - _pos;
		}
		
		if(Mathf.Abs(_speed) > 0.01f || Mathf.Abs(diff) > 1.0f)
		{
			if(Mathf.Abs(_speed) < SLOW_SPEED || Mathf.Abs(diff) > 0.0f) //slow it down a lot if it's close to stopping or past the edge
			{
				_speed *= STRONG_FRICTION;
			}
			else
			{
				_speed *= WEAK_FRICTION;
			}
			
			_pos += _speed + diff * EDGE_BOUNCE;
			
			return true; //it's still moving
		}
		else //it's done moving, stahp!
		{
			_speed = 0.0f;
			
			//put it at the exact edge
			if(_pos < _boundsMin)
			{
				_pos = _boundsMin;
			}
			else if(_pos > _boundsMax)
			{
				_pos = _boundsMax;
			}
			
			return false; //it's not moving anymore!
		}
	}
	
	public void UpdateDrag(float touchPos)
	{
		float absolutePos = _basePos - (touchPos - _baseTouchPos);
		
		float diff = 0; //diff is the amount of movement needed to bring absolutePos back in bounds
		
		if(absolutePos < _boundsMin)
		{
			diff = _boundsMin - absolutePos;
			
			float result = (1.0f - (1.0f / ((diff * EDGE_SQUISH_RATIO / EDGE_SQUISH) + 1.0f))) * EDGE_SQUISH;
			
			_pos = _boundsMin - result;
		}
		else if(absolutePos > _boundsMax)
		{
			diff = absolutePos - _boundsMax;
			
			float result = (1.0f - (1.0f / ((diff * EDGE_SQUISH_RATIO / EDGE_SQUISH) + 1.0f))) * EDGE_SQUISH;
			
			_pos = _boundsMax + result;
		}
		else 
		{
			_pos = absolutePos;
		}
	}
	
	public void SetPos(float pos)
	{
		_pos = pos;
	}
	
	public void SetPos(float pos, float speed)
	{
		_pos = pos;
		_speed = speed;
	}
	
	public float GetPos()
	{
		return _pos;
	}
	
	public void SetSpeed(float speed)
	{
		_speed = speed;
	}
	
	public float GetSpeed()
	{
		return _speed;
	}
	
	public void SetBounds(float boundsMin, float boundsMax)
	{
		_boundsMin = boundsMin;
		_boundsMax = boundsMax;
	}
	
	public float GetDragDelta()
	{
		return _pos - _basePos;
	}
	
	public float GetDragDistance()
	{
		return Mathf.Abs(_pos - _basePos);
	}
	
	public bool isDragging
	{
		get {return _isDragging;}
	}

	public float boundsMin
	{
		get {return _boundsMin;}
	}

	public float boundsMax
	{
		get {return _boundsMax;}
	}
}
