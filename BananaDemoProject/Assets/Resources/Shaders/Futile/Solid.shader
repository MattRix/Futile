//from http://forum.unity3d.com/threads/68402-Making-a-2D-game-for-iPhone-iPad-and-need-better-performance

Shader "Futile/Solid" //Unlit
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	
	SubShader 
	{
		Pass 
		{
			SetTexture [_MainTex] 
			{
				Combine texture 
			}
		}
	}
}