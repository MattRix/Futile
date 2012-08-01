using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class TestFont : MonoBehaviour
{	
	public static TestFont instance;
	
	private FStage _stage;
	private FLabel _label;
	
	private void Start()
	{
		instance = this; 
		
		Go.defaultEaseType = EaseType.Linear;
		Go.duplicatePropertyRule = DuplicatePropertyRuleType.RemoveRunningProperty;
		
		FEngine.instance.Init (10,10);
		
		FEngine.atlasManager.LoadAtlas("Fonts/Quartermain", true);
		FEngine.atlasManager.LoadFont("Quartermain", "Fonts/Quartermain", "Fonts/Quartermain_Config", 1.0f, 1.0f);
		
		_stage = FEngine.stage;
		
		_label = new FLabel("Quartermain", "FOOBAR");
		if(_label != null)
		{
			_stage.AddChild(_label);
			_label.color = new Color(1.0f,1.0f,1.0f,1.0f); 	
		}
	}
	
	public void Update()
	{
	}
}









