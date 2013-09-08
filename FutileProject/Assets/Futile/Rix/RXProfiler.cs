using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RXProfiler : MonoBehaviour
{
	static public Dictionary<Type,List<WeakReference>> instancesByType = new Dictionary<Type, List<WeakReference>>();

	static RXProfiler()
	{
		#if UNITY_EDITOR
		GameObject go = new GameObject("RXProfiler");
		go.AddComponent<RXProfiler>(); //for watching in the editor
		#endif
	}

	public void Update()
	{
		#if UNITY_EDITOR
			//update every second
			if(Time.frameCount % Application.targetFrameRate == 0)
			{
				RXProfiler.CheckInstanceCounts();
			}
		#endif
	}

	static private void CheckInstanceCounts()
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
		#if UNITY_EDITOR
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
		#endif
	}
}














