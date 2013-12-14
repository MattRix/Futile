
//Based on a brilliant idea by Matthew Wegner - https://twitter.com/mwegner/status/355147544818495488
//My implementation is super lazy with magic numbers everywhere! :D
//NOTE: only works in Unity 4

#if UNITY_EDITOR && !UNITY_3_5

using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.Reflection;

//Usage: put these above the variables where you want the divider
//[RXDivider]
//[RXDivider("My header")] 
//[RXDivider("My header", "My subtitle")] 

public class RXDivider : PropertyAttribute 
{
	public string header;
	public string subtitle;

	public RXDivider(string header, string subtitle)
	{
		this.header = header;
		this.subtitle = subtitle;
	}

	public RXDivider(string header)
	{
		this.header = header;
		this.subtitle = "";
	}

	public RXDivider()
	{
		this.header = "";
		this.subtitle = "";
	}
}

[CustomPropertyDrawer (typeof(RXDivider))]
public class RXDividerDrawer : PropertyDrawer 
{
	public override void OnGUI (Rect rect, SerializedProperty prop, GUIContent label) 
	{
		RXDivider att = attribute as RXDivider;

		float headerHeight = 10.0f;

		if(att.header != "")
		{
			headerHeight += 40.0f;
		}

		if(att.subtitle != "")
		{
			headerHeight += 16.0f;
		}

		rect.y += headerHeight;
		DrawDefaultProperty(rect,prop,label, true);
		rect.y -= headerHeight;

		if(att.header != "")
		{
			rect.y += 20.0f;

			//TITLE

			GUIStyle headerStyle = new GUIStyle (GUI.skin.label);
			headerStyle.fontSize = 15;
			headerStyle.fontStyle = FontStyle.Bold;

			EditorGUI.LabelField(rect,att.header,headerStyle);

			rect.y += 17.0f;
		}

		//SUBTITLE
		if(att.subtitle != "")
		{
			GUIStyle subtitleStyle = new GUIStyle (GUI.skin.label);
			subtitleStyle.fontSize = 10;
			//subtitleStyle.fontStyle = FontStyle.Italic;

			EditorGUI.LabelField(rect,att.subtitle,subtitleStyle);

			rect.y += 12.0f;
		}

		if(Event.current.type == EventType.Repaint) //draw the divider
		{
			rect.x = 14.0f;
			rect.y += 5.0f;
			rect.height = 1.0f;
			rect.width -= 14.0f;

			GUI.skin.box.Draw(rect,GUIContent.none,0);
		}
	}

	public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
	{
		RXDivider att = attribute as RXDivider;

		float headerHeight = 10.0f;

		if(att.header != "")
		{
			headerHeight += 40.0f;
		}

		if(att.subtitle != "")
		{
			headerHeight += 16.0f;
		}

		return base.GetPropertyHeight(prop, label) + headerHeight;
	}

	private void DrawDefaultProperty(Rect rect, SerializedProperty property, GUIContent label, bool includeChildren)
	{
		Dictionary<string, PropertyDrawer> dictionaryOfDrawers =
		typeof(PropertyDrawer).GetField("s_PropertyDrawers", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).GetValue(null) as Dictionary<string, PropertyDrawer>;

		if(dictionaryOfDrawers != null)
		{
			foreach (var entry in dictionaryOfDrawers)
			{
				if (entry.Value == this)
				{
					dictionaryOfDrawers[entry.Key] = null;
					EditorGUI.PropertyField(rect, property, label, true);
					dictionaryOfDrawers[entry.Key] = this;
					return;
				}

			}
		}

		EditorGUI.PropertyField(rect, property, label, true);
	}
}

#elif UNITY_3_5

using System;
using UnityEngine;

public class RXDivider 
{
	public string header;
	public string subtitle;

	public RXDivider(string header, string subtitle) {}
	public RXDivider(string header) {}
	public RXDivider() {}
}

#else

using System;
using UnityEngine;

public class RXDivider : PropertyAttribute 
{
	public string header;
	public string subtitle;

	public RXDivider(string header, string subtitle) {}
	public RXDivider(string header) {}
	public RXDivider() {}
}

#endif
