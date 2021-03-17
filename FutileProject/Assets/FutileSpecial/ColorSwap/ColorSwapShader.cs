using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorSwapShader : FShader
{
	private Color _colorR;
	private Color _colorG;
	private Color _colorB;
	
	public ColorSwapShader(Color? colorR = null, Color? colorG = null, Color? colorB = null) : base("ColorSwapShader", Shader.Find("Futile/ColorSwap"))
	{
		_colorR = colorR ?? Color.red;
		_colorG = colorG ?? Color.green;
		_colorB = colorB ?? Color.blue;

		needsApply = true; 
	}
	
	override public void Apply(Material mat)
	{
		mat.SetColor("_ColorR",_colorR);
		mat.SetColor("_ColorG",_colorG);
		mat.SetColor("_ColorB",_colorB);
	}
	
	public Color colorR
	{
		get => _colorR;
		set {if(_colorR != value) {_colorR = value; needsApply = true;}}
	}

	public Color colorG
	{
		get => _colorG;
		set {if(_colorG != value) {_colorG = value; needsApply = true;}}
	}

	public Color colorB
	{
		get => _colorB;
		set {if(_colorB != value) {_colorB = value; needsApply = true;}}
	}
}
