using UnityEngine;
using System;
using System.Collections.Generic;
using System.Timers;

//this class fires a callback every time the Garbage Collector runs
//usage: 	
//
//	RXGCTrigger.AddCallback(myAction); 
//	RXGCTrigger.RemoveCallback(myAction); 
//
public class RXGCTrigger
{
	private static List<WeakReference>weakRefs = new List<WeakReference>();
	
	public Action SignalGC;
	
	public WeakReference weakRef;
	public bool shouldStop = false;
	
	private RXGCTrigger(Action SignalGC)
	{
		this.SignalGC = SignalGC;
		weakRef = new WeakReference(this);
		weakRefs.Add(weakRef);
	}
	
	~RXGCTrigger()
	{
		weakRefs.Remove(weakRef);
		if(!shouldStop) 
		{
			new RXGCTrigger(SignalGC);
			if(SignalGC != null) SignalGC();
		}
	}
	
	public static void AddCallback(Action SignalGC)
	{
		for(int w = 0; w<weakRefs.Count; w++)
		{
			RXGCTrigger trigger = (weakRefs[w].Target as RXGCTrigger);
			if(trigger != null && trigger.SignalGC == SignalGC) return; //we already have it so don't add it again!
		}
		
		new RXGCTrigger(SignalGC);
	}
	
	public static void RemoveCallback(Action SignalGC)
	{ 
		for(int w = 0; w<weakRefs.Count; w++)
		{
			RXGCTrigger trigger = (weakRefs[w].Target as RXGCTrigger);
			if(trigger != null && trigger.SignalGC == SignalGC) 
			{
				trigger.shouldStop = true;
				weakRefs.RemoveAt(w);
				w--;
			}
		}
	}
}