using System;
using UnityEngine;

public class FShader
{
	static public FShader defaultShader;
	
	//shader types
	public static FShader Normal;
	public static FShader Additive;
	
	public static int nextShaderIndex = 0;
	
	public int index;
	public string name;
	public Shader shader;
	
	public FShader (string name, Shader shader, int index)
	{
		this.index = index;
		this.name = name;
		this.shader = shader; 
	}
	
	public static void Init() //called by Futile
	{
		Normal = new FShader("Normal", Shader.Find("Unlit Transparent Vertex Colored"), nextShaderIndex++);	
		Additive = new FShader("Additive", Shader.Find("Unlit Transparent Vertex Colored Additive"), nextShaderIndex++);	
		
		defaultShader = Normal;
	}
	
}


