using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

//DEFINITIONS:
//FTouch: a single temporary instance of a touch event, that exists for 1 frame
//FTouchSlot: a permanent reference to whatever is happening with the finger in that "slot"
//Touchable Interfaces: interfaces that define how a class/object should respond to touch events

public class FTouch //had to make a copy of Unity's Touch so I could make properties writeable for mouse touches
{
	public int fingerId;
	public Vector2 position;
	public Vector2 deltaPosition; //this is not accurate
	public float deltaTime;
	public int tapCount;
	public TouchPhase phase;
	public FTouchSlot slot;
}

public class FTouchSlot
{
	public int index;
	public FTouch touch;
	public bool doesHaveTouch = false;
	public FCapturedTouchableInterface touchable = null;

	public bool isUsedBySingleTouchable = true;

	public bool didJustBegin = false;
	public bool didJustMove = false;
	public bool didJustEnd = false;
	public bool didJustCancel = false;

	public bool wasArtificiallyCanceled = false;

	public FTouchSlot(int index)
	{
		this.index = index;
	}

	public void Cancel()
	{
		if(touchable != null)
		{
			if(isUsedBySingleTouchable)
			{
				(touchable as FSingleTouchableInterface).HandleSingleTouchCanceled(touch);
			}
			else 
			{
				(touchable as FSmartTouchableInterface).HandleSmartTouchCanceled(index, touch);
			}

			wasArtificiallyCanceled = true;
		}
	}

	public void Clear()
	{
		touchable = null;
		doesHaveTouch = false;
		
		didJustBegin = false;
		didJustEnd = false;
		didJustMove = false;
		didJustCancel = false;

		wasArtificiallyCanceled = false;
	}
}

public interface FCapturedTouchableInterface
{
	int touchPriority //FNodes have this defined by default
	{
		get;	
	}
}

public interface FSingleTouchableInterface : FCapturedTouchableInterface
{
	bool HandleSingleTouchBegan(FTouch touch);
	
	void HandleSingleTouchMoved(FTouch touch);

	void HandleSingleTouchEnded(FTouch touch);

	void HandleSingleTouchCanceled(FTouch touch);
}

public interface FMultiTouchableInterface
{
	void HandleMultiTouch(FTouch[] touches);
}

public interface FSmartTouchableInterface : FCapturedTouchableInterface
{
	bool HandleSmartTouchBegan(int touchIndex, FTouch touch);
	
	void HandleSmartTouchMoved(int touchIndex, FTouch touch);
	
	void HandleSmartTouchEnded(int touchIndex, FTouch touch);
	
	void HandleSmartTouchCanceled(int touchIndex, FTouch touch);
}

public class FTouchManager
{
	public const int SLOT_COUNT = 12;

	public static bool shouldMouseEmulateTouch = true;
	public static bool isEnabled = true;

	private List<FMultiTouchableInterface> _multiTouchables = new List<FMultiTouchableInterface>();
	private List<FCapturedTouchableInterface> _capturedTouchables = new List<FCapturedTouchableInterface>();

	private bool _needsPrioritySort = false;
	
	private Vector2 _previousMousePosition = new Vector2(0,0);

	private FTouchSlot[] _touchSlots;

	public FTouchManager ()
	{
		Input.multiTouchEnabled = true;
	
		//this just makes sure mouse emulation is off on iOS and Android
		//this may eventually cause problems on devices that support both mouse and touch
		
		#if UNITY_ANDROID
			shouldMouseEmulateTouch = false;
		#endif 
		
		#if UNITY_IPHONE
			shouldMouseEmulateTouch = false;
		#endif
		
		#if UNITY_EDITOR
			shouldMouseEmulateTouch = true;
		#endif

		_touchSlots = new FTouchSlot[SLOT_COUNT];

		for(int t = 0; t<SLOT_COUNT; t++)
		{
			_touchSlots[t] = new FTouchSlot(t);
		}
	}
	
	public bool DoesTheSingleTouchableExist()
	{
		return _touchSlots[0].doesHaveTouch;	
	}
	
	public void Update()
	{
		if (!isEnabled) return;

		if(_needsPrioritySort)
		{
			UpdatePrioritySorting();	
		}

		//create non-changeable temporary copies of the lists
		//this is so that there won't be problems if touchables are removed/added while being iterated through
		FMultiTouchableInterface[] tempMultiTouchables = _multiTouchables.ToArray();
		FCapturedTouchableInterface[] tempCapturedTouchables = _capturedTouchables.ToArray();

		float touchScale = 1.0f/Futile.displayScale;
		
		//the offsets account for the camera's 0,0 point (eg, center, bottom left, etc.)
		float offsetX = -Futile.screen.originX * Futile.screen.pixelWidth;
		float offsetY = -Futile.screen.originY * Futile.screen.pixelHeight;
		
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
		
		int capturedTouchableCount = tempCapturedTouchables.Length;

		//reset the touch slotIndexes so that each slot can pick the touch it needs
		for(int t = 0; t<touchCount; t++)
		{
			FTouch touch = touches[t];
			touch.slot = null; 
		}

		//match up slots that are currently active with the touches
		for(int s = 0; s<SLOT_COUNT; s++)
		{
			FTouchSlot slot = _touchSlots[s];

			if(slot.doesHaveTouch)
			{
				bool didFindMatchingTouch = false;

				for(int t = 0; t<touchCount; t++)
				{
					FTouch touch = touches[t];
					if(slot.touch.fingerId == touch.fingerId)
					{
						didFindMatchingTouch = true;
						slot.touch = touch;
						touch.slot = slot;
						break;
					}
				}

				if(!didFindMatchingTouch)
				{
					slot.doesHaveTouch = false;
					slot.touchable = null;
				}
			}
		}

		//fill any blank slots with the unclaimed touches
		for(int s = 0; s<SLOT_COUNT; s++)
		{
			FTouchSlot slot = _touchSlots[s];
			
			if(!slot.doesHaveTouch)
			{
				for(int t = 0; t<touchCount; t++)
				{
					FTouch touch = touches[t];
					if(touch.slot == null)
					{
						slot.touch = touch;
						slot.doesHaveTouch = true;
						touch.slot = slot;
						break;
					}
				}
			}

			if(slot.doesHaveTouch) //send the touch out to the slots that need it
			{
				if(slot.touch.phase == TouchPhase.Began)
				{
					slot.didJustBegin = true;
					slot.wasArtificiallyCanceled = false;

					for(int c = 0; c<capturedTouchableCount; c++)
					{
						FCapturedTouchableInterface capturedTouchable = tempCapturedTouchables[c];

						FSingleTouchableInterface singleTouchable = capturedTouchable as FSingleTouchableInterface;

						//the first touchable to return true becomes the active one
						if(slot.index == 0 && singleTouchable != null && singleTouchable.HandleSingleTouchBegan(slot.touch))
						{
							slot.isUsedBySingleTouchable = true;
							slot.touchable = capturedTouchable;
							break;
						}
						else 
						{
							FSmartTouchableInterface smartTouchable = capturedTouchable as FSmartTouchableInterface;
							if(smartTouchable != null && smartTouchable.HandleSmartTouchBegan(slot.index, slot.touch))
							{
								slot.isUsedBySingleTouchable = false;
								slot.touchable = capturedTouchable;
								break;
							}
						}
					}
				}
				else if(slot.touch.phase == TouchPhase.Canceled)
				{
					slot.didJustCancel = true;

					if(!slot.wasArtificiallyCanceled)
					{
						if(slot.touchable != null)
						{
							if(slot.isUsedBySingleTouchable)
							{
								(slot.touchable as FSingleTouchableInterface).HandleSingleTouchCanceled(slot.touch);
							}
							else 
							{
								(slot.touchable as FSmartTouchableInterface).HandleSmartTouchCanceled(slot.index, slot.touch);
							}
						}
					}

					//cleaned up in CleanUpEndedAndCanceledTouches() instead
					//slot.touchable = null;
					//slot.doesHaveTouch = false;
				}
				else if(slot.touch.phase == TouchPhase.Ended)
				{
					slot.didJustEnd = true;

					if(!slot.wasArtificiallyCanceled)
					{
						if(slot.touchable != null)
						{
							if(slot.isUsedBySingleTouchable)
							{
								(slot.touchable as FSingleTouchableInterface).HandleSingleTouchEnded(slot.touch);
							}
							else 
							{
								(slot.touchable as FSmartTouchableInterface).HandleSmartTouchEnded(slot.index, slot.touch);
							}
						}
					}

					//cleaned up in CleanUpEndedAndCanceledTouches() instead
					//slot.touchable = null;
					//slot.doesHaveTouch = false;
				}
				else if(slot.touch.phase == TouchPhase.Moved)
				{
					slot.didJustMove = true;

					if(!slot.wasArtificiallyCanceled)
					{
						if(slot.touchable != null)
						{
							if(slot.isUsedBySingleTouchable)
							{
								(slot.touchable as FSingleTouchableInterface).HandleSingleTouchMoved(slot.touch);
							}
							else 
							{
								(slot.touchable as FSmartTouchableInterface).HandleSmartTouchMoved(slot.index, slot.touch);
							}
						}
					}
				}
			}
			else //clear the slot here
			{
				slot.Clear();
			}
		}
		
		if(touchCount > 0)
		{
			int multiTouchableCount = tempMultiTouchables.Length;
			for(int m = 0; m<multiTouchableCount; m++)
			{
				tempMultiTouchables[m].HandleMultiTouch(touches);
			}	
		}
	}

	public void CleanUpEndedAndCanceledTouches()//called by Futile in LateUpdate
	{
		//clean up the touches that have been canceled or ended
		for(int t = 0; t<SLOT_COUNT; t++)
		{
			FTouchSlot slot = _touchSlots[t];
			if(slot.doesHaveTouch)
			{
				if(slot.wasArtificiallyCanceled && slot.touchable != null)
				{
					slot.touchable = null;
				}

				if(slot.touch.phase == TouchPhase.Moved)
				{
					slot.didJustMove = false;
				}
				else if(slot.touch.phase == TouchPhase.Began)
				{
					slot.didJustBegin = false;
				}
				else if(slot.touch.phase == TouchPhase.Ended)
				{
					slot.doesHaveTouch = false;
					slot.touchable = null;
					slot.didJustEnd = false;
				}
				else if(slot.touch.phase == TouchPhase.Canceled)
				{
					slot.doesHaveTouch = false;
					slot.touchable = null;
					slot.didJustCancel = false;
				}
			}
		}
	}

	public void HandleDepthChange ()
	{
		_needsPrioritySort = true;
	}

	private static int CapturablePriorityComparison(FCapturedTouchableInterface a, FCapturedTouchableInterface b) 
	{
		return b.touchPriority - a.touchPriority; //highest to lowest
	}

//	private static int TouchComparison(FTouch a, FTouch b) 
//	{
//		return a.fingerId - b.fingerId; //lowest to highest
//	}

	private void UpdatePrioritySorting()
	{
		_needsPrioritySort = false;
		_capturedTouchables.Sort(CapturablePriorityComparison);
	}

	public void AddSingleTouchTarget(FSingleTouchableInterface touchable)
	{
		if(!_capturedTouchables.Contains(touchable))
		{
			_capturedTouchables.Add(touchable);
			_needsPrioritySort = true;
		}
	}
	
	public void RemoveSingleTouchTarget(FSingleTouchableInterface touchable)
	{
		_capturedTouchables.Remove(touchable);
	}

	public void AddMultiTouchTarget(FMultiTouchableInterface touchable)
	{
		if(!_multiTouchables.Contains(touchable))
		{
			_multiTouchables.Add(touchable);
		}
	}
	
	public void RemoveMultiTouchTarget(FMultiTouchableInterface touchable)
	{
		_multiTouchables.Remove(touchable);
	}

	public void AddSmartTouchTarget(FSmartTouchableInterface touchable)
	{
		if(!_capturedTouchables.Contains(touchable))
		{
			_capturedTouchables.Add(touchable);
			_needsPrioritySort = true;
		}
	}
	
	public void RemoveSmartTouchTarget(FSmartTouchableInterface touchable)
	{
		_capturedTouchables.Remove(touchable);
	}
	


	public void LogAllListeners()
	{
		StringBuilder stringBuilder = new StringBuilder("MultiTouchables("+_multiTouchables.Count+"): ");

		for(int m = 0;m<_multiTouchables.Count;m++)
		{
			stringBuilder.Append(_multiTouchables[m]);
			if(m < _multiTouchables.Count - 1)
			{
				stringBuilder.Append(", ");
			}
		}

		Debug.Log(stringBuilder.ToString());

		stringBuilder = new StringBuilder("CapturedTouchables("+_capturedTouchables.Count+"): ");
		
		for(int s = 0;s<_capturedTouchables.Count;s++)
		{
			stringBuilder.Append(_capturedTouchables[s]);
			if(s < _capturedTouchables.Count - 1)
			{
				stringBuilder.Append(", ");
			}
		}
		
		Debug.Log(stringBuilder.ToString());
	}

	public FTouchSlot GetTouchSlot(int index)
	{
		if(index < 0 || index >= _touchSlots.Length) return null;
		return _touchSlots[index];
	}


}




