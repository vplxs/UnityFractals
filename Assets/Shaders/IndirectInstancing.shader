/*
Copyright(c)  2019 VASILEIOS PAPALEXOPOULOS.
Permission is granted to copy, distribute and/or modify this document
under the terms of the GNU Free Documentation License, Version 1.2

or any later version published by the Free Software Foundation;

with no Invariant Sections, no Front-Cover Texts, and no Back-Cover
Texts.  A copy of the license is included in the section entitled "GNU

Free Documentation License".
*/
Shader "Custom/IndirectInstancing"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        
		#pragma surface surf Standard addshadow
			#pragma multi_compile_instancing
			#pragma instancing_options procedural:setup
			#pragma target 4.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
		StructuredBuffer<float4> positionsBuffer;
		StructuredBuffer<float4> colorsBuffer;
#endif

		void setup()
		{
#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
			float4 pos = positionsBuffer[unity_InstanceID];
			unity_ObjectToWorld._11_21_31_41 = float4(pos.w, 0, 0, 0);
			unity_ObjectToWorld._12_22_32_42 = float4(0, pos.w, 0, 0);
			unity_ObjectToWorld._13_23_33_43 = float4(0, 0, pos.w, 0);
			unity_ObjectToWorld._14_24_34_44 = float4(pos.xyz, 1);
			unity_WorldToObject = unity_ObjectToWorld;
			unity_WorldToObject._14_24_34 *= -1;
			unity_WorldToObject._11_22_33 = 1.0f / unity_WorldToObject._11_22_33;
#endif
		}

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
			float4 col = 1.0f;
			float4 em = 0.0f;
#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
			col = colorsBuffer[unity_InstanceID];
			em = colorsBuffer[unity_InstanceID];
#else
			col = float4(1, 0, 0, 0.5f);
#endif
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * col;
            o.Albedo = c.rgb;
            o.Alpha = c.a;
			o.Emission = em;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
