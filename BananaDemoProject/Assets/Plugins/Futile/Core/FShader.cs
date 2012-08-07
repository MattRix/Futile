using System;
using UnityEngine;

public class FShader
{
	public static FShader Normal;
	public static FShader Additive;
	
	private static int _staticIndex = 0;
	
	public int index;
	public string name;
	public Shader shader;
	
	public FShader (string name, Shader shader)
	{
		this.index = _staticIndex++;
		this.name = name;
		this.shader = shader; 
	}
	
	public static void Init() //called by the FRenderer
	{
		Normal = new FShader("Normal", Shader.Find("Unlit Transparent Vertex Colored"));	
		Additive = new FShader("Additive", Shader.Find("Unlit Transparent Vertex Colored Additive"));	
	}
}


