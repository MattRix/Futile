using System;
using UnityEngine;
using System.Collections.Generic;

public class FEngineResolutionLevel
{
	public float maxLength;
	public float displayScale;
	public float contentScale;
	public float resourceScale;
	public string resourceSuffix;
}

public class FEngineParams
{
	public List<FEngineResolutionLevel> resLevels = new List<FEngineResolutionLevel>();
	
	public int startingQuadsPerLayer = 10;
	public int quadsPerLayerExpansion = 10;
	
	public Vector2 origin = new Vector2(0.5f,0.5f);

	public FEngineResolutionLevel AddResolutionLevel (float maxLength, float displayScale, float contentScale, float resourceScale, string resourceSuffix)
	{
		FEngineResolutionLevel resLevel = new FEngineResolutionLevel();
		
		resLevel.maxLength = maxLength;
		resLevel.displayScale = displayScale;
		resLevel.contentScale = contentScale;
		resLevel.resourceScale = resourceScale;
		resLevel.resourceSuffix = resourceSuffix;
		
		bool wasAdded = false;
		
		//we've gotta have the resLevels sorted low to high by maxLength
		for(int r = 0; r<resLevels.Count; r++)
		{
			if(resLevel.maxLength < resLevels[r].maxLength)
			{
				resLevels.Insert(r,resLevel);	
				wasAdded = true;
				break;
			}
		}
		
		if(!wasAdded)
		{
			resLevels.Add(resLevel);	
		}
		
		return resLevel;
	}

}


