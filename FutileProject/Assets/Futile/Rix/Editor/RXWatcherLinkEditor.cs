#if UNITY_EDITOR

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
	public static Type BOOL = typeof(bool);

	public RXWatcherLink link = null;

	public void OnEnable()
	{
		link = target as RXWatcherLink;

		EditorApplication.update += HandleSignalUpdate;
		//Watcher objects in the inspector update every frame, but only when selected.
	}
	
	public void OnDisable()
	{
		EditorApplication.update -= HandleSignalUpdate;
	}
	
	private void HandleSignalUpdate ()
	{
		Repaint();
	}
	
	override public void OnInspectorGUI() 
	{
		//the target has been GC'd, so do nothing
		if(link.GetTarget() == null) return;

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
			else if(member.memberType == BOOL)
			{
				newValue = EditorGUILayout.Toggle(member.name, (bool)oldValue);
			}

			if(newValue != null && !newValue.Equals(oldValue))
			{
				member.SetValue(newValue);
			}
		} 

        EditorGUILayout.Separator();

        GUILayout.Label("Transform Hierarchy", EditorStyles.boldLabel);

        var node = link.GetTarget() as FNode;

        while(node != null)
        {
            GUILayout.Label($"{node.GetType()}: x:{node.x} y:{node.y}", EditorStyles.miniLabel);
            GUILayout.Label($"scaleX:{node.scaleX} scaleY:{node.scaleY} rotation:{node.rotation}", EditorStyles.miniLabel);

            var spriteNode = node as FSprite;

            if(spriteNode != null)
            {
                GUILayout.Label($"localRect:{spriteNode.localRect} boundsRect::{spriteNode.textureRect}", EditorStyles.miniLabel);
                GUILayout.Label($"anchorX:{spriteNode.anchorX} anchorY:{spriteNode.anchorY}", EditorStyles.miniLabel);
                GUILayout.Label($"pixelAnchorX:{spriteNode.anchorX*spriteNode.textureRect.width} pixelAnchorY:{spriteNode.anchorY*spriteNode.textureRect.height}", EditorStyles.miniLabel);
            }

            EditorGUILayout.Separator();
            //display parent next
            node = node.container;
        }

        EditorGUILayout.Separator();

        
	}
}

#endif
