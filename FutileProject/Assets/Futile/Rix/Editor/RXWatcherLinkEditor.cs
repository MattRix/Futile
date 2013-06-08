using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RXWatcherLink))]
public class RXWatcherLinkEditor : Editor
{
	public static Type FLOAT = typeof(float);
	public static Type INT = typeof(int);
	public static Type STRING = typeof(string);
	public static Type COLOR = typeof(Color);
	public static Type VECTOR2 = typeof(Vector2);

	public RXWatcherLink link = null;

	public void OnEnable()
	{
		link = target as RXWatcherLink;

		//Watcher objects in the inspector update every frame, but only when selected.

		Futile.instance.SignalUpdate += HandleSignalUpdate;
	}
	
	public void OnDisable()
	{
		Futile.instance.SignalUpdate -= HandleSignalUpdate;
	}
	
	private void HandleSignalUpdate ()
	{
		Repaint();
	}
	
	override public void OnInspectorGUI() 
	{
		GUILayout.Label(link.name, EditorStyles.boldLabel);
		
		EditorGUILayout.Separator();

		int memberCount = link.members.Count;

		for(int m = 0; m<memberCount; m++)
		{
			RXWatcherLinkMember member = link.members[m];

			object oldValue = member.GetValue();
			object newValue = null; 

			if(member.memberType == FLOAT)
			{
				newValue = EditorGUILayout.FloatField(member.name, (float)oldValue);
			}
			else if(member.memberType == INT)
			{
				newValue = EditorGUILayout.IntField(member.name, (int)oldValue);
			}
			else if(member.memberType == STRING)
			{
				newValue = EditorGUILayout.TextField(member.name, (string)oldValue);
			}
			else if(member.memberType == COLOR)
			{
				newValue = EditorGUILayout.ColorField(member.name, (Color)oldValue);
			}
			else if(member.memberType == VECTOR2)
			{
				newValue = EditorGUILayout.Vector2Field(member.name, (Vector2)oldValue);
			}

			if(newValue != null && !newValue.Equals(oldValue))
			{
				member.SetValue(newValue);
			}
		} 
	}
}


