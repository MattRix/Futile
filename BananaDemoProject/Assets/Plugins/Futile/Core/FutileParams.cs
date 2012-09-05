using System;
using UnityEngine;
using System.Collections.Generic;

public class FResolutionLevel
{
	public float maxLength;
	public float displayScale;
	public float resourceScale;
	public string resourceSuffix;
}

public class FutileParams
{
	public List<FResolutionLevel> resLevels = new List<FResolutionLevel>();
	
	public Vector2 origin = new Vector2(0.5f,0.5f);
	
	public int targetFrameRate = 60;
	
	public ScreenOrientation singleOrientation = ScreenOrientation.Unknown;
	
	public bool supportsLandscapeLeft;
	public bool supportsLandscapeRight;
	public bool supportsPortrait;
	public bool supportsPortraitUpsideDown;
	
	public Color backgroundColor = Color.black;
	
	public bool shouldLerpToNearestResolutionLevel = true;
	
	public FutileParams(bool supportsLandscapeLeft, bool supportsLandscapeRight, bool supportsPortrait, bool supportsPortraitUpsideDown)
	{
		this.supportsLandscapeLeft = supportsLandscapeLeft;
		this.supportsLandscapeRight = supportsLandscapeRight;
		this.supportsPortrait = supportsPortrait;
		this.supportsPortraitUpsideDown = supportsPortraitUpsideDown;
	}

	public FResolutionLevel AddResolutionLevel (float maxLength, float displayScale, float resourceScale, string resourceSuffix)
	{
		FResolutionLevel resLevel = new FResolutionLevel();
		
		resLevel.maxLength = maxLength;
		resLevel.displayScale = displayScale;
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


