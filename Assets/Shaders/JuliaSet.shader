//Based on https://rosettacode.org/wiki/Julia_set
/*
Copyright(c)  2019 VASILEIOS PAPALEXOPOULOS.
Permission is granted to copy, distribute and/or modify this document
under the terms of the GNU Free Documentation License, Version 1.2

or any later version published by the Free Software Foundation;

with no Invariant Sections, no Front-Cover Texts, and no Back-Cover
Texts.  A copy of the license is included in the section entitled "GNU

Free Documentation License".
*/
Shader "Custom/JuliaSetUnlit"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Height("Height", Float) = 800
		_Width("Width", Float) = 800
			_Zoom("Zoom", Float) = 1
			_MaxIter("MaxIter", Float) = 255
			_MoveX("Move X", Float) = 0
			_MoveY("Move Y", Float) = 0
			_CX ("CX", Float) = -0.7
			_CY("CY", Float) = 0.27015
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _Height;
			float _Width;
			float _Zoom;
			float _MaxIter;
			float _MoveX;
			float _MoveY;
			float _CX;
			float _CY;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float x = i.uv.x * _Width;
				float y = i.uv.y * _Height;

				float zx = 1.5* (x - _Width / 2) / (0.5 * _Zoom * _Width) + _MoveX;
				float zy = 1.5 *(y - _Height / 2) / (0.5 * _Zoom * _Height) + _MoveY;

				float iter = _MaxIter;
				float tmp = 0;
				while (zx * zx + zy * zy < 4 && iter>1)
				{
					tmp = zx * zx - zy * zy + _CX;
					zy = 2.0*zx*zy + _CY;
					zx = tmp;
					iter -= 1;
				}

				fixed4 col = fixed4(iter/255, 1-(iter/255), 0, 1);
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
