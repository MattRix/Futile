using UnityEngine;
using System;
using System.Collections.Generic;

public class RXSignal
{
	public delegate void NoArgumentDelegate();

	private List<RXSignalListener> _listeners;

	public RXSignal()
	{

	}

	public void Dispatch(int eventType)
	{
		if(_listeners == null) return; //we have no listeners so don't dispatch anything!

		for(int n = 0; n<_listeners.Count;n++)
		{
			RXSignalListener listener = _listeners[n];

			if(listener.eventType == eventType)
			{
				if(_listeners[n].isWeak)
				{
					object target = _listeners[n].weakRef.Target;
					if(target != null)
					{
						(target as NoArgumentDelegate).Invoke();
					}
					else //remove because it's null
					{
						_listeners.RemoveAt(n);
						n--;
					}
				}
				else
				{
					_listeners[n].strongDele.Invoke();
				}
			}
		}
	}

	public void AddStrongListener(int eventType, NoArgumentDelegate dele)
	{
		AddListener(eventType,dele,false);
	}

	public void AddWeakListener(int eventType, NoArgumentDelegate dele)
	{
		AddListener(eventType,dele,true);
	}

	public void AddListener(int eventType, NoArgumentDelegate dele, bool isWeak)
	{
		if(_listeners == null) _listeners = new List<RXSignalListener>();

		RXSignalListener listener = new RXSignalListener();

		listener.eventType = eventType;

		if(isWeak)
		{
			listener.isWeak = true;
			listener.weakRef = new WeakReference(dele);
		}
		else 
		{
			listener.isWeak = false;
			listener.strongDele = dele;
		}

		_listeners.Add(listener);
	}

	public void RemoveListener(int eventType, NoArgumentDelegate dele)
	{
		if(_listeners != null) 
		{
			for(int n = _listeners.Count-1; n>=0;n--)
			{
				if(_listeners[n].eventType == eventType)
				{
					if(_listeners[n].isWeak)
					{
						NoArgumentDelegate target = _listeners[n].weakRef.Target as NoArgumentDelegate;
						if(target != null && target == dele)
						{
							_listeners.RemoveAt(n);
						}

					}
					else if(_listeners[n].strongDele == dele)
					{
						_listeners.RemoveAt(n);
					}
				}
			}

		}
	}

	public void RemoveAllListeners()
	{
		if(_listeners != null)
		{
			_listeners.Clear();
		}
	}

	private class RXSignalListener
	{
		public bool isWeak;
		public WeakReference weakRef;
		public int eventType;
		public NoArgumentDelegate strongDele;
	}

}

