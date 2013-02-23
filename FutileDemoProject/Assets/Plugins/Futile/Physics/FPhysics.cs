using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//A class for linking Unity physics and Futile
//Set METERS_TO_POINTS at a ratio that makes sense for your game

public class FPhysics
{
	public const float DEFAULT_Z_THICKNESS = 1.0f;
	public const float METERS_TO_POINTS = 64.0f;
	public const float POINTS_TO_METERS = 1.0f/METERS_TO_POINTS;
	
	public FPhysics ()
	{
	}
}

