using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Timers;

public class RXProfiler : MonoBehaviour
{
	static public Dictionary<Type,List<WeakReference>> instancesByType = new Dictionary<Type, List<WeakReference>>();
	static private Timer _timer;

	static RXProfiler()
	{
		GameObject go = new GameObject("RXProfiler");
		go.AddComponent<RXProfiler>(); //for watching in the editor

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
		foreach(KeyValuePair<Type, List<WeakReference>> typePair in instancesByType)
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
				//Debug.Log("RXProfiler: Removed " + removalCount + " instance" + (removalCount==1?"":"s") + " of [" + typePair.Key.Name + "]. There are now " + weakRefs.Count + " alive.");
			}
		}
	}

	static public void TrackLifeCycle(System.Object thing)
	{
		#if !UNITY_EDITOR
			return;
		#endif

		Type targetType = thing.GetType();

		List<WeakReference> weakRefs = null;

		if(instancesByType.ContainsKey(targetType))
		{
			weakRefs = instancesByType[targetType];

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
			instancesByType.Add(targetType, weakRefs);
		}

		weakRefs.Add(new WeakReference(thing));

		//Debug.Log ("RXProfiler: Added an instance of [" + targetType.Name + "]. There are now " + weakRefs.Count + " alive.");
	}
}














