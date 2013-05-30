using System;
using UnityEngine;

public class FNodeEnabler
{
	public FNodeEnabler ()
	{
	}
	
	virtual public void Connect()
	{
		
	}
	
	virtual public void Disconnect()
	{
		
	}
}

public class FNodeEnablerForUpdate : FNodeEnabler
{
	public Futile.FutileUpdateDelegate handleUpdateCallback;
	
	public FNodeEnablerForUpdate(Futile.FutileUpdateDelegate handleUpdateCallback)
	{
		this.handleUpdateCallback = handleUpdateCallback;	
	}
	
	override public void Connect()
	{
		Futile.instance.SignalUpdate += handleUpdateCallback;
	}
	
	override public void Disconnect()
	{
		Futile.instance.SignalUpdate -= handleUpdateCallback;
	}
}

public class FNodeEnablerForLateUpdate : FNodeEnabler
{
	public Futile.FutileUpdateDelegate handleUpdateCallback;
	public FNodeEnablerForLateUpdate(Futile.FutileUpdateDelegate handleUpdateCallback)
	{
		this.handleUpdateCallback = handleUpdateCallback;	
	}
	
	override public void Connect()
	{
		Futile.instance.SignalLateUpdate += handleUpdateCallback;
	}
	
	override public void Disconnect()
	{
		Futile.instance.SignalLateUpdate -= handleUpdateCallback;
	}
}

public class FNodeEnablerForFixedUpdate : FNodeEnabler
{
	public Futile.FutileUpdateDelegate handleUpdateCallback;
	
	public FNodeEnablerForFixedUpdate(Futile.FutileUpdateDelegate handleUpdateCallback)
	{
		this.handleUpdateCallback = handleUpdateCallback;	
	}
	
	override public void Connect()
	{
		Futile.instance.SignalFixedUpdate += handleUpdateCallback;
	}
	
	override public void Disconnect()
	{
		Futile.instance.SignalFixedUpdate -= handleUpdateCallback;
	}
}

public class FNodeEnablerForSingleTouch : FNodeEnabler
{
	public FSingleTouchableInterface singleTouchable;
	
	public FNodeEnablerForSingleTouch(FNode node)
	{
		singleTouchable = node as FSingleTouchableInterface;
		if(singleTouchable == null)
		{
			throw new FutileException("Trying to enable single touch on a node that doesn't implement FSingleTouchableInterface");	
		}
	}
	
	override public void Connect()
	{
		Futile.touchManager.AddSingleTouchTarget(singleTouchable);	
	}
	
	override public void Disconnect()
	{
		Futile.touchManager.RemoveSingleTouchTarget(singleTouchable);	
	}
}

public class FNodeEnablerForMultiTouch : FNodeEnabler
{
	public FMultiTouchableInterface multiTouchable;
	
	public FNodeEnablerForMultiTouch(FNode node)
	{
		multiTouchable = node as FMultiTouchableInterface;
		
		if(multiTouchable == null)
		{
			throw new FutileException("Trying to enable multi touch on a node that doesn't implement FMultiTouchableInterface");	
		}
	}
	
	override public void Connect()
	{
		Futile.touchManager.AddMultiTouchTarget(multiTouchable);	
	}
	
	override public void Disconnect()
	{
		Futile.touchManager.RemoveMultiTouchTarget(multiTouchable);	
	}
}

public class FNodeEnablerForResize : FNodeEnabler
{
	public FScreen.ScreenResizeDelegate handleResizeCallback;
	
	public FNodeEnablerForResize(FScreen.ScreenResizeDelegate handleResizeCallback)
	{
		this.handleResizeCallback = handleResizeCallback;	
	}
	
	override public void Connect()
	{
		Futile.screen.SignalResize += handleResizeCallback;
	}
	
	override public void Disconnect()
	{
		Futile.screen.SignalResize -= handleResizeCallback;
	}
}

public class FNodeEnablerForOrientationChange : FNodeEnabler
{
	public FScreen.ScreenOrientationChangeDelegate handleOrientationChangeCallback;
	
	public FNodeEnablerForOrientationChange(FScreen.ScreenOrientationChangeDelegate handleOrientationChangeCallback)
	{
		this.handleOrientationChangeCallback = handleOrientationChangeCallback;	
	}
	
	override public void Connect()
	{
		Futile.screen.SignalOrientationChange += handleOrientationChangeCallback;
	}
	
	override public void Disconnect()
	{
		Futile.screen.SignalOrientationChange -= handleOrientationChangeCallback;
	}
}


