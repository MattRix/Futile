using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct FTouch //had to make a copy of Unity's Touch so I could make properties writeable for mouse touches
{
	public int fingerId;
	public Vector2 position;
	public Vector2 deltaPosition; //this is not accurate
	public float deltaTime;
	public int tapCount;
	public TouchPhase phase;
}

public interface FSingleTouchableInterface
{
	bool HandleSingleTouchBegan(FTouch touch);
	
	void HandleSingleTouchMoved(FTouch touch);

	void HandleSingleTouchEnded(FTouch touch);

	void HandleSingleTouchCanceled(FTouch touch);
	
	int touchPriority
	{
		get;	
	}
}

public interface FMultiTouchableInterface
{
	void HandleMultiTouch(FTouch[] touches);
}
	
public class FTouchManager
{
	public static bool shouldMouseEmulateTouch = true;
	
	private List<FSingleTouchableInterface> _singleTouchables = new List<FSingleTouchableInterface>();
	private List<FMultiTouchableInterface> _multiTouchables = new List<FMultiTouchableInterface>();
	
	private List<FSingleTouchableInterface> _singleTouchablesToAdd = new List<FSingleTouchableInterface>();
	private List<FSingleTouchableInterface> _singleTouchablesToRemove = new List<FSingleTouchableInterface>();
	private List<FMultiTouchableInterface> _multiTouchablesToAdd = new List<FMultiTouchableInterface>();
	private List<FMultiTouchableInterface> _multiTouchablesToRemove = new List<FMultiTouchableInterface>();
	
	private FSingleTouchableInterface _theSingleTouchable = null;
	
	private bool _isMouseDown;
	
	private bool _isUpdating = false;
	
	private Vector2 _previousMousePosition = new Vector2(0,0);

	public FTouchManager ()
	{
		Input.multiTouchEnabled = true;
	
		//this just makes sure mouse emulation is off on iOS and Android
		
		#if UNITY_ANDROID
			shouldMouseEmulateTouch = false;
		#endif 
		
		#if UNITY_IPHONE
			shouldMouseEmulateTouch = false;
		#endif
		
		#if UNITY_EDITOR
			shouldMouseEmulateTouch = true;
		#endif
		
	}
	
	public bool DoesTheSingleTouchableExist()
	{
		return (_theSingleTouchable != null);	
	}
	
	public void Update()
	{
		_isUpdating = true;
		
		float touchScale = 1.0f/Futile.displayScale;
		
		//the offsets account for the camera's 0,0 point (eg, center, bottom left, etc.)
		float offsetX = -Futile.instance.originX * Futile.pixelWidth;
		float offsetY = -Futile.instance.originY * Futile.pixelHeight;
		
		//Debug.Log ("Touch offset " + offsetX + " , " + offsetY);
		
		bool wasMouseTouch = false;
		FTouch mouseTouch = new FTouch();
		
		if(shouldMouseEmulateTouch)
		{
			mouseTouch.position = new Vector2((Input.mousePosition.x+offsetX)*touchScale, (Input.mousePosition.y+offsetY)*touchScale);
			
			mouseTouch.fingerId = 0;
			mouseTouch.tapCount = 1;
			mouseTouch.deltaTime = Time.deltaTime;
			
			
			
			if(Input.GetMouseButtonDown(0))
			{
				mouseTouch.deltaPosition = new Vector2(0,0);
				_previousMousePosition = mouseTouch.position;
				
				mouseTouch.phase = TouchPhase.Began;
				wasMouseTouch = true;
			}
			else if(Input.GetMouseButtonUp(0))
			{
				mouseTouch.deltaPosition = new Vector2(mouseTouch.position.x - _previousMousePosition.x, mouseTouch.position.y - _previousMousePosition.y);
				_previousMousePosition = mouseTouch.position;
				mouseTouch.phase = TouchPhase.Ended;	
				wasMouseTouch = true;
			}
			else if(Input.GetMouseButton(0))
			{
				mouseTouch.deltaPosition = new Vector2(mouseTouch.position.x - _previousMousePosition.x, mouseTouch.position.y - _previousMousePosition.y);
				_previousMousePosition = mouseTouch.position;
				
				mouseTouch.phase = TouchPhase.Moved;	
				wasMouseTouch = true;
			}
		}
		
		int touchCount = Input.touchCount;
		int offset = 0;
		
		if(wasMouseTouch) touchCount++;
		
		FTouch[] touches = new FTouch[touchCount];
		
		if(wasMouseTouch) 
		{
			touches[0] = mouseTouch;
			offset = 1;
		}
		
		for (int i = 0; i < Input.touchCount; ++i)
		{
			Touch sourceTouch = Input.GetTouch (i);
			FTouch resultTouch = new FTouch();
			
			resultTouch.deltaPosition = new Vector2(sourceTouch.deltaPosition.x*touchScale, sourceTouch.deltaPosition.y*touchScale);
			resultTouch.deltaTime = sourceTouch.deltaTime;
			resultTouch.fingerId = sourceTouch.fingerId+offset;
			resultTouch.phase = sourceTouch.phase;
			resultTouch.position = new Vector2((sourceTouch.position.x+offsetX)*touchScale, (sourceTouch.position.y+offsetY)*touchScale);
			resultTouch.tapCount = sourceTouch.tapCount;
			
			touches[i+offset] = resultTouch;
		}
		
		if(touches.Length > 0 )
		{
			foreach(FTouch touch in touches)
			{
				if(touch.fingerId == 0) // we only care about the first touch for the singleTouchables
				{
					if(touch.phase == TouchPhase.Began)
					{
						
						foreach(FSingleTouchableInterface singleTouchable in _singleTouchables)
						{
							if(singleTouchable.HandleSingleTouchBegan(touch)) //the first touchable to return true becomes theSingleTouchable
							{
								_theSingleTouchable = singleTouchable;
								break;
							}
						}
					}
					else if(touch.phase == TouchPhase.Ended)
					{
						if(_theSingleTouchable != null)
						{
							_theSingleTouchable.HandleSingleTouchEnded(touch);	
						}
						_theSingleTouchable = null;
					}
					else if(touch.phase == TouchPhase.Canceled)
					{
						if(_theSingleTouchable != null)
						{
							_theSingleTouchable.HandleSingleTouchCanceled(touch);	
						}
						_theSingleTouchable = null;
					}
					else //moved or stationary
					{
						if(_theSingleTouchable != null)
						{
							_theSingleTouchable.HandleSingleTouchMoved(touch);	
						}
					}
					
					break; //break out from the foreach, once we've found the first touch we don't care about the others
				}
			}
			
			foreach(FMultiTouchableInterface multiTouchable in _multiTouchables)
			{
				multiTouchable.HandleMultiTouch(touches);
			}
		}
		
		//now add or remove anything that was changed while we were looping through
		
		foreach(FSingleTouchableInterface singleTouchableToRemove in _singleTouchablesToRemove)
		{
			_singleTouchables.Remove(singleTouchableToRemove);	
		}
		
		foreach(FSingleTouchableInterface singleTouchableToAdd in _singleTouchablesToAdd)
		{
			_singleTouchables.Add(singleTouchableToAdd);	
		}
		
		foreach(FMultiTouchableInterface multiTouchableToRemove in _multiTouchablesToRemove)
		{
			_multiTouchables.Remove(multiTouchableToRemove);	
		}
		
		foreach(FMultiTouchableInterface multiTouchableToAdd in _multiTouchablesToAdd)
		{
			_multiTouchables.Add(multiTouchableToAdd);	
		}
		
		_singleTouchablesToRemove.Clear();
		_singleTouchablesToAdd.Clear();
		_multiTouchablesToRemove.Clear();
		_multiTouchablesToAdd.Clear();
		
		_isUpdating = false;
	}
	
	private static int PriorityComparison(FSingleTouchableInterface a, FSingleTouchableInterface b) 
	{
		return b.touchPriority - a.touchPriority;
	}

	public void UpdatePrioritySorting()
	{
		_singleTouchables.Sort(PriorityComparison);
	}
	
	public void AddSingleTouchTarget(FSingleTouchableInterface touchable)
	{
		if(_isUpdating)
		{
			if(!_singleTouchablesToAdd.Contains(touchable))
			{
				int index = _singleTouchablesToRemove.IndexOf(touchable);
				if(index != -1) _singleTouchablesToRemove.RemoveAt(index);
				_singleTouchablesToAdd.Add(touchable);
			}
		}
		else
		{
			if(!_singleTouchables.Contains(touchable))
			{
				_singleTouchables.Add(touchable);
			}
		}
		
	}
	
	public void AddMultiTouchTarget(FMultiTouchableInterface touchable)
	{
		if(_isUpdating)
		{
			if(!_multiTouchablesToAdd.Contains(touchable))
			{
				int index = _multiTouchablesToRemove.IndexOf(touchable);
				if(index != -1) _multiTouchablesToRemove.RemoveAt(index);
				_multiTouchablesToAdd.Add(touchable);
			}
		}
		else
		{
			if(!_multiTouchables.Contains(touchable))
			{
				_multiTouchables.Add(touchable);
			}
		}
	}
	
	public void RemoveSingleTouchTarget(FSingleTouchableInterface touchable)
	{
		if(_isUpdating)
		{
			if(!_singleTouchablesToRemove.Contains(touchable))
			{
				int index = _singleTouchablesToAdd.IndexOf(touchable);
				if(index != -1) _singleTouchablesToAdd.RemoveAt(index);
				_singleTouchablesToRemove.Add(touchable);
			}
		}
		else
		{
			_singleTouchables.Remove(touchable);
		}
	}
	
	public void RemoveMultiTouchTarget(FMultiTouchableInterface touchable)
	{
		if(_isUpdating)
		{
			if(!_multiTouchablesToRemove.Contains(touchable))
			{
				int index = _multiTouchablesToAdd.IndexOf(touchable);
				if(index != -1) _multiTouchablesToAdd.RemoveAt(index);
				_multiTouchablesToRemove.Add(touchable);
			}
		}
		else
		{
			_multiTouchables.Remove(touchable);
		}
	}
	
}
