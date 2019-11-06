//Based on https://www.rosettacode.org/wiki/Mandelbrot_set
/*
Copyright(c)  2019 VASILEIOS PAPALEXOPOULOS.
Permission is granted to copy, distribute and/or modify this document
under the terms of the GNU Free Documentation License, Version 1.2

or any later version published by the Free Software Foundation;

with no Invariant Sections, no Front-Cover Texts, and no Back-Cover
Texts.  A copy of the license is included in the section entitled "GNU

Free Documentation License".
*/
Shader "Custom/MandelbrotSetUnlit"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Height("Height", Float) = 800
		_Width("Width", Float) = 800
			_MaxIter("MaxIter", Float) = 255
			_Zoom("Zoom", Float) = 1
			_Fn1 ("Fn1", Float) = 1
			_Fn2("Fn2", Float) = 2
			_Fn3("Fn3", Float) = 3
			_Dx ("Delta X", Float) = 0
			_Dy("Delta Y", Float) = 0
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
				float _Zmx1;
				float _Zmx2;
				float _Zmy1;
				float _Zmy2;
				float _Dx;
				float _Dy;
				float _Fn1;
				float _Fn2;
				float _Fn3;

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					UNITY_TRANSFER_FOG(o,o.vertex);
					return o;
				}

				fixed4 frag(v2f v) : SV_Target
				{
					_Zmx1 = (_Width / 4)*_Zoom;
					_Zmx2 = 2 * (1 / _Zoom);
					_Zmy1 = (_Height / 4)*_Zoom;
					_Zmy2 = 2 * (1/_Zoom);
					float i = v.uv.x*_Width;
					float j = v.uv.y*_Height;
					float x = (i + _Dx) / _Zmx1 - _Zmx2;
					float y = _Zmy2 - (j + _Dy) / _Zmy1;
					float zr = 0;
					float zi = 0; 
					float zr2 = 0;
					float zi2 = 0;
					float cr = x;
					float ci = y;
					float n = 1;

					while (n < _MaxIter && (zr2 + zi2) < 4)
					{
						zi2 = zi * zi;
						zr2 = zr * zr;
						zi = 2 * zi * zr + ci;
						zr = zr2 - zi2 + cr;
						n++;
					}
					float re = ((n*_Fn1) % 255) / 255.0;
					float gr = ((n*_Fn2) % 255) / 255.0;
					float bl = ((n*_Fn3) % 255) / 255.0;
					fixed4 col = fixed4(re, gr, bl, 1);
					// apply fog
					UNITY_APPLY_FOG(v.fogCoord, col);
					return col;
				}
				ENDCG
			}
		}
}
