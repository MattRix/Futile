using UnityEngine;
using System;
using System.Collections.Generic;
using System.Timers;

public class RXWeak
{
	//allows you to add weak listeners, but currently only works with zero-argument Action callbacks
	//
	//syntax: 
	//			SomeEvent += RXWeak.Add(HandleEvent);
	//			SomeEvent -= RXWeak.Remove(HandleEvent);
	//

	private static List<RXWeakListener> _listeners;

	static RXWeak()
	{
		_listeners = new List<RXWeakListener>();

		//this will trigger CleanUp to be called every time the Garbage Collector is run
		RXGCTrigger.AddCallback(CleanUp);
	}

	//removes unused listeners periodically (and can be called manually if needed)
	//an unused listener is one where the target has already been garbage collected
	public static void CleanUp()
	{
		//reverse order so removals are easy
		for(int n = _listeners.Count-1; n>=0; n--)
		{
			if(!_listeners[n].weakRef.IsAlive)
			{
				_listeners.RemoveAt(n);
			}
		}
	}

	public static Action Add(Action callback)
	{
		//if we already have it, just use what we have (but increment timesAdded so we know how many Remove() calls are needed)
		for(int n = 0; n<_listeners.Count; n++)
		{
			Action dele = (_listeners[n].weakRef.Target as Action);

			if(dele == callback)
			{
				_listeners[n].timesAdded++;
				return _listeners[n].InnerCallback;
			}
		}

		//create a new listener
		RXWeakListener listener = new RXWeakListener();
		listener.weakRef = new WeakReference(callback);
		_listeners.Add(listener);
		return listener.InnerCallback;
	}

	public static Action Remove(Action callback)
	{
		for(int n = 0; n<_listeners.Count; n++)
		{
			Action dele = (_listeners[n].weakRef.Target as Action);

			if(dele == callback)
			{
				RXWeakListener listener = _listeners[n];

				listener.timesAdded --;

				if(listener.timesAdded <= 0)
				{
					_listeners.RemoveAt(n);
				}

				return listener.InnerCallback;
			}
		}
		return null; //this shouldn't ever really happen unless someone removes more listeners than they add
	}

	private class RXWeakListener
	{
		public int timesAdded = 1;
		public WeakReference weakRef;

		public void InnerCallback()
		{
			Action dele = (weakRef.Target as Action);

			if(dele != null)
			{
				dele();
			}
			else 
			{
				_listeners.Remove(this); //we don't have the target, so remove us
			}
		}
	}
}

