using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/*
Perform delayed successive actions on a list of objects

Use :

protected void MyMethod()
{
	ListDelayedActions<FSprite> parse=new ListDelayedActions<FSprite>(parsed);
	parse.Parse(HandleAListObjectAction,HandleAListEndOfActions,0.1f,0.05f,0.2f);
}
	
protected void HandleAListObjectAction(FSprite sprite)
{
	MyUtils.FadeOut(sprite);
}
protected void HandleAListEndOfActions()
{
	Debug.Log("All sprites parsed, we can go on");
}		

*/

public class ListDelayedActions<T>
{	
	protected List<T> _objects;
	public static float DEFAULT_PARSE_DELAY=0.25f;
	public static float DEFAULT_PARSE_DELAY_VARIANCE=0.1f;
	
	System.Action<T> _action;
	System.Action _endAction;
	protected float _delay,_delayVariance;

	protected int _index;

	public ListDelayedActions(List<T> objects) {
		_objects=objects;
	}

	public void Parse(System.Action<T> action,System.Action endAction) {
		Parse (action,endAction,DEFAULT_PARSE_DELAY,DEFAULT_PARSE_DELAY_VARIANCE);
	}
	
	public void Parse(System.Action<T> action,System.Action endAction,float delay,float variance) {
		Parse(action,endAction,delay,variance,0f);
	}
	public void Parse(System.Action<T> action,System.Action endAction,float delay,float variance,float startingDelay) {
		_action=action;
		_endAction=endAction;
		_delay=delay;
		_delayVariance=variance;
		_index=0;
		if (startingDelay<=0f) {
			NextAction();
		} else {
			TweenConfig config0=new TweenConfig().floatProp("dummy",0).onComplete(HandleWaitComplete);
			Go.to (this, startingDelay, config0);
			//Futile.instance.StartDelayedCallback(NextAction,startingDelay);
		}
	}
	
	public float dummy { get { return 0;} set { } }
	
	protected void NextAction() {
		if (_index>=_objects.Count) {
			//Debug.Log("NextAction _endAction="+_endAction);
			_endAction();
		} else {
			//Debug.Log("NextAction _action="+_action+" _index="+_index);
			_action(_objects[_index]);
			
			_index++;
			
			TweenConfig config0=new TweenConfig().floatProp("dummy",0).onComplete(HandleWaitComplete);
			Go.to (this, _delay+RXRandom.Range(-1,1)*_delayVariance, config0);
			//Futile.instance.StartDelayedCallback(NextAction,_delay+RXRandom.Range(-1,1)*_delayVariance);
		}
	}

	protected void HandleWaitComplete(AbstractTween tween) { NextAction(); }
}


