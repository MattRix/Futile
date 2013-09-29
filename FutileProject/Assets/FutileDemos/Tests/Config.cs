using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Config
{
	public static string fontFile="FranchiseFont";
	public static FTextParams textParams;
	
	static Config()
    {         
        textParams = new FTextParams();
		textParams.kerningOffset=-3f;
		textParams.lineHeightOffset=-6f;
    } 
}









