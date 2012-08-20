using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//used for watching variables for debug reasons
//simply put whatever property you want to watch as a public var in RXWatchTarget
//and then call RXWatcher.GetTarget().someVariable = 6.0f;  

public class RXWatcher
{
	private static RXWatcher _instance;
	
	private GameObject _gameObject;
	
	public RXWatchTarget watchTarget;
	
	public RXWatcher ()
	{
		_gameObject = new GameObject("RXWatcher");
		watchTarget = _gameObject.AddComponent(typeof(RXWatchTarget)) as RXWatchTarget;
	}
	
	public static RXWatchTarget GetTarget()
	{
		if(_instance == null) _instance = new RXWatcher();
		
		return _instance.watchTarget;
	}
}

public class RXWatchTarget : MonoBehaviour
{
	public string stringA = "";
	public string stringB = "";
	public string stringC = "";
	
	public float floatA = 0;
	public float floatB = 0;
	public float floatC = 0;
	
	public int intA = 0;
	public int intB = 0;
	public int intC = 0;
	
	public RXWatchTarget ()
	{
		
	}
}
	



