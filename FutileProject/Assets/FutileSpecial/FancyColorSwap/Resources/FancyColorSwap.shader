Shader "Futile/FancyColorSwap"
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

sampler2D _PaletteTex;

struct v2f {
    float4  pos : SV_POSITION;
    float2  uv : TEXCOORD0;
	half4 colorR : COLOR;
	half4 colorG : TEXCOORD1;
	half4 colorB : TANGENT;
};

v2f vert (appdata_full IN)
{
    v2f OUT;
    OUT.pos = UnityObjectToClipPos (IN.vertex);
    OUT.uv = TRANSFORM_TEX (IN.texcoord, _MainTex); 

	float4 color = IN.color;

	OUT.colorR = tex2Dlod(_PaletteTex,half4(color.r,0,0,0));
	OUT.colorG = tex2Dlod(_PaletteTex,half4(color.g,0,0,0));
	OUT.colorB = tex2Dlod(_PaletteTex,half4(color.b,0,0,0));

	OUT.colorR.a = IN.color.a; //put in the alpha so we can fade it in and out using the color's alpha still

    return OUT;
}

half4 frag (v2f IN) : COLOR
{
	half4 col = tex2D (_MainTex,IN.uv);

	//half3 result = col.r*IN.colorR.rgb + col.g*IN.colorG.rgb + col.b*IN.colorB.rgb;

	half3 result = col.r*IN.colorR.rgb;

	col.rgb = result * col.a; //multiplied alpha
	col.a *= IN.colorR.a; //use alpha from the red color (which was set from the original color)

	return col; 
}
ENDCG
				
				
				
			}
		} 
	}
}

