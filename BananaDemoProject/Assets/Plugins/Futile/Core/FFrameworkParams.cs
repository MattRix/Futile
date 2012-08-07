using System;
using UnityEngine;
using System.Collections.Generic;

public class FResolutionLevel
{
	public float maxLength;
	public float displayScale;
	public float contentScale;
	public float resourceScale;
	public string resourceSuffix;
}

public class FFrameworkParams
{
	public List<FResolutionLevel> resLevels = new List<FResolutionLevel>();
	
	public int startingQuadsPerLayer = 10;
	public int quadsPerLayerExpansion = 10;
	
	public Vector2 origin = new Vector2(0.5f,0.5f);

	public FResolutionLevel AddResolutionLevel (float maxLength, float displayScale, float contentScale, float resourceScale, string resourceSuffix)
	{
		FResolutionLevel resLevel = new FResolutionLevel();
		
		resLevel.maxLength = maxLength;
		resLevel.displayScale = displayScale;
		resLevel.contentScale = contentScale;
		resLevel.resourceScale = resourceScale;
		resLevel.resourceSuffix = resourceSuffix;
		
		bool wasAdded = false;
		
		//we've gotta have the resLevels sorted low to high by maxLength
		for(int r = 0; r<resLevels.Count; ++r)
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


