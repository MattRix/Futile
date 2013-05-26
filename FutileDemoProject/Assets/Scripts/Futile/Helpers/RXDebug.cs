using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Timers;

public static class RXDebug
{
	static private Dictionary<Type,List<WeakReference>> _instancesByType = new Dictionary<Type, List<WeakReference>>();
	static private Timer _timer;

	static RXDebug()
	{
		_timer = new Timer(1.0f);
		_timer.Elapsed += HandleTick;
		_timer.Start();
	}

	static void HandleTick(object sender, ElapsedEventArgs e)
	{
		CheckInstanceCounts();
	}

	static void CheckInstanceCounts()
	{
		foreach(KeyValuePair<Type, List<WeakReference>> typePair in _instancesByType)
		{
			int removalCount = 0;

			List<WeakReference> weakRefs = typePair.Value;

			for(int w = weakRefs.Count-1; w>=0; w--) //reversed so removals are easy
			{
				WeakReference weakRef = weakRefs[w];

				if(weakRef.Target == null)
				{
					removalCount++;
					weakRefs.RemoveAt(w);
				}
			}

			if(removalCount > 0)
			{
				Debug.Log("RXDebug: Removed " + removalCount + " instance" + (removalCount==1?"":"s") + " of [" + typePair.Key.Name + "]. There are now " + weakRefs.Count + " alive.");
			}
		}
	}

	static public void TrackLifeCycle(System.Object thing)
	{
		Type targetType = thing.GetType();

		List<WeakReference> weakRefs = null;

		if(_instancesByType.ContainsKey(targetType))
		{
			weakRefs = _instancesByType[targetType];

			int weakRefsCount = weakRefs.Count;

			for(int w = 0; w<weakRefsCount; w++)
			{
				if(weakRefs[w].Target == thing)
				{
					return; // we already have it!
				}
			}
		}
		else
		{
			weakRefs = new List<WeakReference>();
			_instancesByType.Add(targetType, weakRefs);
		}

		weakRefs.Add(new WeakReference(thing));

		Debug.Log ("RXDebug: Added an instance of [" + targetType.Name + "]. There are now " + weakRefs.Count + " alive.");
	}
}














