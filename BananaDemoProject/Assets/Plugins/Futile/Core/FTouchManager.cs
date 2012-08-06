using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct FTouch //had to make a copy of Unity's Touch so I could make properties writeable for mouse touches
{
	public int fingerId;
	public Vector2 position;
	public Vector2 deltaPosition;
	public float deltaTime;
	public int tapCount;
	public TouchPhase phase;
}

public interface FSingleTouchable
{
	bool HandleSingleTouchBegan(FTouch touch);
	
	void HandleSingleTouchMoved(FTouch touch);

	void HandleSingleTouchEnded(FTouch touch);

	void HandleSingleTouchCanceled(FTouch touch);
	
	int depth
	{
		get;	
	}
}

public interface FMultiTouchable
{
	void HandleMultiTouch(FTouch[] touches);
}

public class FTouchManager
{
	public static bool shouldMouseEmulateTouch = true;
	
	private List<FSingleTouchable> _singleTouchables = new List<FSingleTouchable>();
	private List<FMultiTouchable> _multiTouchables = new List<FMultiTouchable>();
	
	private List<FSingleTouchable> _singleTouchablesToAdd = new List<FSingleTouchable>();
	private List<FSingleTouchable> _singleTouchablesToRemove = new List<FSingleTouchable>();
	private List<FMultiTouchable> _multiTouchablesToAdd = new List<FMultiTouchable>();
	private List<FMultiTouchable> _multiTouchablesToRemove = new List<FMultiTouchable>();
	
	private FSingleTouchable _theSingleTouchable = null;
	
	private bool _isMouseDown;
	
	private bool _isUpdating = false;

	
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
		
		float touchScale = 1.0f/FEngine.displayScale;
		
		//the offsets account for the camera's 0,0 point (eg, center, bottom left, etc.)
		float offsetX = -FEngine.instance.originX * Screen.width;
		float offsetY = -FEngine.instance.originY * Screen.height;
		
		//Debug.Log ("Touch offset " + offsetX + " , " + offsetY);
		
		bool wasMouseTouch = false;
		FTouch mouseTouch = new FTouch();
		
		if(shouldMouseEmulateTouch)
		{
			mouseTouch.position = new Vector2((Input.mousePosition.x+offsetX)*touchScale, (Input.mousePosition.y+offsetY)*touchScale);
			mouseTouch.fingerId = 0;
			mouseTouch.tapCount = 1;
			mouseTouch.deltaPosition = new Vector2(0,0);
			mouseTouch.deltaTime = 0;
			
			if(Input.GetMouseButtonDown(0))
			{
				mouseTouch.phase = TouchPhase.Began;
				wasMouseTouch = true;
			}
			else if(Input.GetMouseButtonUp(0))
			{
				mouseTouch.phase = TouchPhase.Ended;	
				wasMouseTouch = true;
			}
			else if(Input.GetMouseButton(0))
			{
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
						
						foreach(FSingleTouchable singleTouchable in _singleTouchables)
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
			
			foreach(FMultiTouchable multiTouchable in _multiTouchables)
			{
				multiTouchable.HandleMultiTouch(touches);
			}
		}
		
		//now add or remove anything that was changed while we were looping through
		
		foreach(FSingleTouchable singleTouchableToRemove in _singleTouchablesToRemove)
		{
			_singleTouchables.Remove(singleTouchableToRemove);	
		}
		
		foreach(FSingleTouchable singleTouchableToAdd in _singleTouchablesToAdd)
		{
			_singleTouchables.Add(singleTouchableToAdd);	
		}
		
		foreach(FMultiTouchable multiTouchableToRemove in _multiTouchablesToRemove)
		{
			_multiTouchables.Remove(multiTouchableToRemove);	
		}
		
		foreach(FMultiTouchable multiTouchableToAdd in _multiTouchablesToAdd)
		{
			_multiTouchables.Add(multiTouchableToAdd);	
		}
		
		_singleTouchablesToRemove.Clear();
		_singleTouchablesToAdd.Clear();
		_multiTouchablesToRemove.Clear();
		_multiTouchablesToAdd.Clear();
		
		_isUpdating = false;
	}
	
	public void UpdateDepthSorting()
	{
		_singleTouchables.Sort(delegate(FSingleTouchable touchableA, FSingleTouchable touchableB) {return touchableB.depth - touchableA.depth;});
	}
	
	public void AddSingleTouchTarget(FSingleTouchable touchable)
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
	
	public void AddMultiTouchTarget(FMultiTouchable touchable)
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
	
	public void RemoveSingleTouchTarget(FSingleTouchable touchable)
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
	
	public void RemoveMultiTouchTarget(FMultiTouchable touchable)
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
