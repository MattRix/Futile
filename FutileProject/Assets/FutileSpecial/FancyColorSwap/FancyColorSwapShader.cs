using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FancyColorSwapShader : FShader
{
	static public FShader TheShader = new FancyColorSwapShader();

	static FancyColorSwapShader()
	{
		Shader.SetGlobalTexture("_PaletteTex",Resources.Load<Texture2D>("simple_palette_wide"));
	}

	public FancyColorSwapShader() : base("FancyColorSwapShader", Shader.Find("Futile/FancyColorSwap"))
	{
	}

	static public Color GetColor(int palR, int palG, int palB)
	{
		return new Color((float)palR/255f,(float)palG/255f,(float)palB/255f);
	}
} 
