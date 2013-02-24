using System;
using UnityEngine;

public class FPWorld : MonoBehaviour
{
	public static FPWorld instance;
	
	public static FPWorld Create(float metersToPointsRatio)
	{
		GameObject gameObject = new GameObject("FPWorld Root");	
		
		instance = gameObject.AddComponent<FPWorld>() as FPWorld;
		
		instance.Init(metersToPointsRatio);
		
		return instance;
	}

	protected void Init (float metersToPointsRatio)
	{
		FPhysics.METERS_TO_POINTS = metersToPointsRatio;
		FPhysics.POINTS_TO_METERS = 1.0f/metersToPointsRatio;
	}

}

