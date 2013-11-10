//from http://forum.unity3d.com/threads/68402-Making-a-2D-game-for-iPhone-iPad-and-need-better-performance
//pixelsnap code is from the Unity standard pixelsnap shader (Sprites/PixelSnap/AlphaBlended)

Shader "Futile/Basic_PixelSnap" //Unlit Transparent Vertex Colored
{
	Properties 
	{
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
	}
	
	SubShader 
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		ZWrite Off
		ZTest Always
		//Alphatest Greater 0
		Blend SrcAlpha OneMinusSrcAlpha 
		Fog { Mode Off }
		Lighting Off
		Cull Off //we can turn backface culling off because we know nothing will be facing backwards

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;

			struct appdata_t
			{
				float4 vertex   : POSITION;
				float2 texcoord : TEXCOORD0;
				float4 color	: COLOR;
			};

			struct v2f
			{
				float4 vertex	: POSITION;
				float2 texcoord : TEXCOORD0;
				float4 color	: COLOR;
			};

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = mul(UNITY_MATRIX_MVP, IN.vertex);
				OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);

				// Snapping params
				float hpcX = _ScreenParams.x * 0.5;
				float hpcY = _ScreenParams.y * 0.5;
				
				#ifdef UNITY_HALF_TEXEL_OFFSET
					float hpcOX = -0.5;
					float hpcOY = 0.5;
				#else
					float hpcOX = 0;
					float hpcOY = 0;
				#endif	
				
				// Snap
				float pos = floor((OUT.vertex.x / OUT.vertex.w) * hpcX + 0.5f) + hpcOX;
				OUT.vertex.x = pos / hpcX * OUT.vertex.w;

				pos = floor((OUT.vertex.y / OUT.vertex.w) * hpcY + 0.5f) + hpcOY;
				OUT.vertex.y = pos / hpcY * OUT.vertex.w;
				OUT.color = IN.color;

				return OUT;
			}

			fixed4 frag(v2f IN) : COLOR
			{
				return tex2D( _MainTex, IN.texcoord) * IN.color;
			}
			ENDCG
		}
	}
}
