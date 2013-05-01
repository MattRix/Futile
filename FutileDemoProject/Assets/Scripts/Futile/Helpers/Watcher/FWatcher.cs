using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR

using System.Reflection;

public class FWatcher
{
	private static FWatcher _instance;
	
	private GameObject _gameObject;
	private List<FWatcherLink> _watcherLinks = new List<FWatcherLink>();
	
	private FWatcher ()
	{
		_gameObject = new GameObject("FWatcher");
	}
	
	static public void Watch(object target)
	{
		if(target == null) return;

		Watch (target, false, null);
	}
	
	static public void Watch(object target, bool shouldShowNonPublic)
	{
		if(target == null) return;

		Watch (target, shouldShowNonPublic, null);
	}

	static public void Watch(object target, bool shouldShowNonPublic, string targetName)
	{
		if(target == null) return;

		if(targetName == null)
		{
			if(target is Type)
			{
				targetName = (target as Type).Name;
			}
			else
			{
				targetName = target.GetType().Name;
			}
		}

		if(_instance == null)
		{
			_instance = new FWatcher();
		}
		
		_instance.DoWatch(target, targetName, shouldShowNonPublic);
	}
	
	public void DoWatch(object target, string targetName, bool shouldShowNonPublic)
	{
		int watcherLinkCount = _watcherLinks.Count;
		for(int w = 0; w<watcherLinkCount; w++)
		{
			if(_watcherLinks[w].GetTarget() == target)
			{
				return; //we already have a watcher for this target!
			}
		}

		GameObject linkGO = new GameObject(targetName);

		linkGO.transform.parent = _gameObject.transform;

		FWatcherLink link = linkGO.AddComponent<FWatcherLink>();
		link.Init(target, shouldShowNonPublic);

		_watcherLinks.Add(link);
	}

	static public void RemoveWatcherLink(FWatcherLink watcherLink)
	{
		_instance.DoRemoveWatcherLink(watcherLink);
	}

	public void DoRemoveWatcherLink(FWatcherLink watcherLink)
	{
		int watcherLinkCount = _watcherLinks.Count;
		for(int w = 0; w<watcherLinkCount; w++)
		{
			if(_watcherLinks[w] == watcherLink)
			{
				_watcherLinks.RemoveAt(w);
				return;
			}
		}
	}

}

public class FWatcherLink : MonoBehaviour
{
	private WeakReference _targetRef;

	private bool _hasSetup = false;
	private bool _shouldShowNonPublicMembers = false;

	private List<FWatcherLinkMember> _members = new List<FWatcherLinkMember>();
	
	public void Init(object target, bool shouldShowNonPublic) 
	{
		_targetRef = new WeakReference(target);
		_shouldShowNonPublicMembers = shouldShowNonPublic;
	}
	
	private void SetupTarget()
	{
		object target = _targetRef.Target;
		Type targetType = _targetRef.Target.GetType();

		BindingFlags bindingFlags = BindingFlags.Public;

		if(target is Type)
		{
			bindingFlags |= BindingFlags.Static;
			targetType = target as Type;
		}
		else
		{
			bindingFlags |= BindingFlags.Instance;
		}

		if(_shouldShowNonPublicMembers)
		{
			bindingFlags |= BindingFlags.NonPublic;
		}

		FieldInfo[] fieldInfos = targetType.GetFields(bindingFlags);
		PropertyInfo[] propertyInfos = targetType.GetProperties(bindingFlags);

		for(int f = 0; f<fieldInfos.Length; f++)
		{
			FWatcherLinkMember member = new FWatcherLinkMember(this, fieldInfos[f]);

			if(member.CheckIfValid())
			{
				_members.Add(member);
			}
		}

		for(int p = 0; p<propertyInfos.Length; p++)
		{
			FWatcherLinkMember member = new FWatcherLinkMember(this, propertyInfos[p]);
			
			if(member.CheckIfValid())
			{
				_members.Add(member);
			}
		}
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
		}
	}

	public object GetTarget()
	{
		return _targetRef.Target;
	}

	public List<FWatcherLinkMember> members
	{
		get {return _members;}
	}
	
	private void Destroy()
	{
		UnityEngine.Object.Destroy(gameObject);
		FWatcher.RemoveWatcherLink(this);
	}


}

//this is used to wrap FieldInfo+PropertyInfo so they can be treated the exact same way
public class FWatcherLinkMember
{
	public string name;
	public Type memberType;
	public MemberInfo memberInfo;
	
	private FWatcherLink _link;
	private PropertyInfo _propertyInfo = null;
	private FieldInfo _fieldInfo = null;

	
	public FWatcherLinkMember(FWatcherLink link, MemberInfo memberInfo)
	{
		_link = link;
		this.memberInfo = memberInfo;

		_propertyInfo = memberInfo as PropertyInfo;
		
		if(_propertyInfo != null)
		{
			name = _propertyInfo.Name;

			memberType = _propertyInfo.PropertyType;
		}
		else //check if it's a field instead
		{
			_fieldInfo = memberInfo as FieldInfo;
			
			if(_fieldInfo != null)
			{
				name = _fieldInfo.Name;
				memberType = _fieldInfo.FieldType;
			}
		}
	}
	
	public bool CheckIfValid()
	{
		if(_propertyInfo != null)
		{
			if(_propertyInfo.CanWrite)
			{
				return true;
			}
		}
		else if(_fieldInfo != null)
		{
			return true;
		}
		
		return false;
	}
	
	public object GetValue()
	{
		if(_propertyInfo != null)
		{
			return _propertyInfo.GetValue(_link.GetTarget(), null);
		}
		else if(_fieldInfo != null)
		{
			return _fieldInfo.GetValue(_link.GetTarget());
		}
		
		return null;
	}
	
	public void SetValue(object newValue)
	{
		if(_propertyInfo != null)
		{
			_propertyInfo.SetValue(_link.GetTarget(), newValue, null);
		}
		else if(_fieldInfo != null)
		{
			_fieldInfo.SetValue(_link.GetTarget(), newValue);
		}
	}
}

#else

//do nothing if we're not in the editor

public class FWatcher
{
	static public void Watch(object target)
	{

	}
	
	static public void Watch(object target, bool shouldShowNonPublic)
	{

	}
	
	static public void Watch(object target, bool shouldShowNonPublic, string targetName)
	{

	}
}

public class FWatcherLink : MonoBehaviour
{

}


#endif