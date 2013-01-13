using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//used for watching variables for debug reasons
//simply put whatever property you want to watch as a public var in RXWatchTarget
//and then call RXWatcher.GetTarget().someVariable = 6.0f;  

#if UNITY_EDITOR

using System.Reflection;

public class FWatcher
{
	private static FWatcher _instance;
	
	private GameObject _gameObject;
	
	private List<FWatcherType> _watcherTypes;
	
	private FWatcher ()
	{
		_gameObject = new GameObject("FWatcher");
		_watcherTypes = new List<FWatcherType>();
		
		DoRegisterWatcherType(typeof(FNode), typeof(FWatcherLink_FNode));
		DoRegisterWatcherType(typeof(FSprite), typeof(FWatcherLink_FSprite));
		DoRegisterWatcherType(typeof(FLabel), typeof(FWatcherLink_FLabel));
	}
	
	static private void Init()
	{
		_instance = new FWatcher();
	}
	
	static public void Watch(object target)
	{
		Watch (target, target.GetType().Name, true);
	}
	
	static public void Watch(object target, string targetName)
	{
		Watch (target, targetName, true);
	}
	
	static public void Watch(object target, string targetName, bool shouldUseWeakReference)
	{
		if(_instance == null) Init();	
		
		_instance.DoWatch(target, targetName, shouldUseWeakReference);
	}
	
	public void DoWatch(object target, string targetName, bool shouldUseWeakReference)
	{
		Type targetType = target.GetType();
		
		int watcherTypeCount = _watcherTypes.Count;
		
		bool wasWatcherTypeFound = false;
		
		GameObject ownerGO = new GameObject(targetName);
		ownerGO.transform.parent = _gameObject.transform;
		
		//for(int w = watcherTypeCount-1; w >= 0; w--) //notice the reverse order, so we check against newer stuff first
		for(int w = 0; w < watcherTypeCount; w++)
		{
			FWatcherType watcherType = _watcherTypes[w];
			
			if(watcherType.targetType.IsAssignableFrom(targetType))
			{
				FWatcherLink link = ownerGO.AddComponent(watcherType.linkType) as FWatcherLink;
				link.Init(target, shouldUseWeakReference);
				wasWatcherTypeFound = true;
			}
		}
		
		if(!wasWatcherTypeFound)
		{
			ownerGO.transform.parent = null; //remove the gameobject
		}
	}
	
	static public void RegisterWatcherType(Type targetType, Type linkType)
	{
		if(_instance == null) Init();	
		
		_instance.DoRegisterWatcherType(targetType, linkType);
	}
	
	public void DoRegisterWatcherType(Type targetType, Type linkType)
	{
		int watcherTypeCount = _watcherTypes.Count;
		
		for(int w = 0; w < watcherTypeCount; w++)
		{
			if(_watcherTypes[w].targetType == targetType) 
			{
				//we already have a watcher for this type, so remove the old watcher
				_watcherTypes.RemoveAt(w);
				w--;
			}
			
		}
		
		FWatcherType newWatcherType = new FWatcherType(targetType, linkType);
		
		_watcherTypes.Add (newWatcherType);
	}
}

public class FWatcherType
{
	public Type targetType;
	public Type linkType;
	
	public FWatcherType(Type targetType, Type linkType)
	{
		this.targetType = targetType;
		this.linkType = linkType;
	}
}


public class FWatcherLink : MonoBehaviour
{
	private WeakReference _targetRef;
	private object _targetStrongRef;
	private FieldInfo[] _linkFields;
	private MemberInfo[] _targetMembers;
	private bool[] _isTargetMemberAProperty;
	private object[] _previousValues;
	private bool _hasSetup = false;
	
	public void Init(object target, bool shouldUseWeakReference)
	{
		_targetRef = new WeakReference(target);
		
		if(!shouldUseWeakReference)
		{
			_targetStrongRef = target; //now target will be retained forever... only really good for static classes
			_targetStrongRef.ToString(); //call something on it so the compiler doesn't give a warning about it being unused
		}
	}
	
	virtual protected void SetupTarget()
	{
		object target = _targetRef.Target;
		Type targetType = _targetRef.Target.GetType();
		
		_linkFields = this.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
		
		int linkFieldsCount = _linkFields.Length;
		_targetMembers = new MemberInfo[linkFieldsCount];
		_previousValues = new object[linkFieldsCount];
		_isTargetMemberAProperty = new bool[linkFieldsCount];
		
		for(int f = 0; f<linkFieldsCount; f++)
		{
			FieldInfo linkField = _linkFields[f];
			
			MemberInfo memberInfo = targetType.GetProperty(linkField.Name);
			
			if(memberInfo == null) //try seeing if it's a static field instead
			{
				memberInfo = targetType.GetProperty(linkField.Name, BindingFlags.Static);
			}	
			
			if(memberInfo == null)
			{
				memberInfo = targetType.GetField(linkField.Name);	
				_isTargetMemberAProperty[f] = false;
				_previousValues[f] = (memberInfo as FieldInfo).GetValue(target);
			}
			else 
			{
				_isTargetMemberAProperty[f] = true;	
				_previousValues[f] = (memberInfo as PropertyInfo).GetValue(target,null);
			}
			
			_targetMembers[f] = memberInfo;
			
			linkField.SetValue(this, _previousValues[f]);
		}
	}
	
	protected object GetTarget()
	{
		return _targetRef.Target;
	}
	
	public void Update()
	{
		if(_targetRef.Target == null)
		{
			Destroy();
		}
		else 
		{
			if(!_hasSetup)
			{
				_hasSetup = true;
				SetupTarget();
			}
			UpdateTarget();
		}
	}
	
	virtual protected void UpdateTarget()
	{
		object target = _targetRef.Target;
		
		int linkFieldsCount = _linkFields.Length;
		
		for(int f = 0; f<linkFieldsCount; f++)
		{
			FieldInfo linkField = _linkFields[f];
			
			object linkValue = linkField.GetValue(this);
			
			if(_isTargetMemberAProperty[f])
			{
				PropertyInfo targetProperty = _targetMembers[f] as PropertyInfo;
				
				if(linkValue != _previousValues[f])
				{
					targetProperty.SetValue(target,linkValue,null);	
				}
				
				_previousValues[f] = targetProperty.GetValue(target, null);
			}
			else 
			{
				FieldInfo targetField = _targetMembers[f] as FieldInfo;
				
				if(linkValue != _previousValues[f])
				{
					targetField.SetValue(target,linkValue);	
				}
				
				_previousValues[f] = targetField.GetValue(target);
			}
			
			linkField.SetValue(this, _previousValues[f]);
		}
	}
	
	private void Destroy()
	{
		UnityEngine.Object.Destroy(gameObject);
	}
}

#else

//do nothing if we're not in the editor

public class FWatcher
{

	static public void Watch(object target)
	{
	}
	
	static public void Watch(object target, string targetName)
	{
	}
	
	static public void Watch(object target, string targetName, bool shouldUseWeakReference)
	{
	}
	
	static public void RegisterWatcherType(Type targetType, Type linkType)
	{
		
	}
}

public class FWatcherLink : MonoBehaviour
{
	virtual protected void SetupTarget()
	{
		
	}

	virtual protected void UpdateTarget()
	{
		
	}
}


#endif