// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Futile/ColorSwap"
{
	Properties 
	{
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
		_ColorR ("Red Replacement Color", Color) = (1,0,0,1)
		_ColorG ("Green Replacement Color", Color) = (0,1,0,1)
		_ColorB ("Blue Replacement Color", Color) = (0,0,1,1)
	}
	
	Category 
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		ZWrite Off
		//Alphatest Greater 0
		Blend SrcAlpha OneMinusSrcAlpha 
		Fog { Color(0,0,0,0) }
		Lighting Off
		Cull Off //we can turn backface culling off because we know nothing will be facing backwards

		SubShader   
		{
			Pass 
			{
				
CGPROGRAM
#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"

float4 _ColorR;
float4 _ColorG;
float4 _ColorB;
sampler2D _MainTex;
float4 _MainTex_ST;

struct v2f {
    float4  pos : SV_POSITION;
    float2  uv : TEXCOORD0;
};

v2f vert (appdata_base v)
{
    v2f o;
    o.pos = UnityObjectToClipPos (v.vertex);
    o.uv = TRANSFORM_TEX (v.texcoord, _MainTex);
    return o;
}

half4 frag (v2f IN) : COLOR
{
	fixed4 col = tex2D (_MainTex,IN.uv);

	fixed4 result = col.r*_ColorR + col.g*_ColorG + col.b *_ColorB;

	result.rgb *= col.a;
	result.a = col.a;

	return result;
}
ENDCG
				
				
				
			}
		} 
	}
}

