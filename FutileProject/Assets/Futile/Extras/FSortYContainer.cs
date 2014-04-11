using UnityEngine;
using System;
using System.Collections.Generic;

//useful for simple depth sorting based on y values (ex in simple isometric stuff)

public class FSortYContainer : FContainer
{
	public FSortYContainer()
	{
		ListenForAfterUpdate(HandleAfterUpdate);
	}
	
	void HandleAfterUpdate()
	{
		bool didChange = _childNodes.InsertionSort(CompareY);
		if(didChange)
		{
			_stage.HandleFacetsChanged();
		}
	}
	
	//sorts array in DESCENDING order because the higher y values should be earlier (aka further back)
	private static int CompareY(FNode a, FNode b) 
	{
		float delta = b.y-a.y;
		if(delta < 0) return -1;
		if(delta > 0) return 1;
		return 0;
	}
}
