Shader "Hidden/SgtRing"
{
	Properties
	{
		_Color("Color", Color) = (1, 1, 1, 1)
		_MainTex("Main Tex", 2D) = "white" {}

		_DetailTex("Detail Tex", 2D) = "white" {}
		_DetailScale("Detail Scale", Vector) = (1,1,1)
		_DetailOffset("Detail Offset", Vector) = (0,0,0)
		_DetailTwist("Detail Twist", Float) = 0
		_DetailTwistBias("Detail Twist Bias", Float) = 0

		_NearTex("Near Tex", 2D) = "white" {}
		_NearScale("Near Scale", Float) = 1
			
		_LightingTex("Lighting Tex", 2D) = "white" {}

		_ScatteringMie("Scattering Mie", Float) = 10
	}
	SubShader
	{
		Tags
		{
			"Queue"           = "Transparent"
			"RenderType"      = "Transparent"
			"IgnoreProjector" = "True"
		}
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha, One One
			Cull Off
			Lighting Off
			ZWrite Off

			CGPROGRAM
			#include "SgtLight.cginc"
			#include "SgtShadow.cginc"
			#pragma vertex Vert
			#pragma fragment Frag
			float4    _Color;
			sampler2D _MainTex;

			sampler2D _DetailTex;
			float2    _DetailScale;
			float2    _DetailOffset;
			float     _DetailTwist;
			float     _DetailTwistBias;

			sampler2D _NearTex;
			float     _NearScale;

			sampler2D _LightingTex;

			float _ScatteringMie;
			// Scattering
			#pragma multi_compile __ SGT_A
			// Detail
			#pragma multi_compile __ SGT_B
			// Near
			#pragma multi_compile __ SGT_C
			// Lights
			#pragma multi_compile __ LIGHT_0 LIGHT_1 LIGHT_2
			// Shadows
			#pragma multi_compile __ SHADOW_1 SHADOW_2

			struct a2v
			{
				float4 vertex    : POSITION;
				float4 color     : COLOR;
				float2 texcoord0 : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
				float3 normal    : NORMAL;
			};

			struct v2f
			{
				float4 vertex    : SV_POSITION;
				float2 texcoord0 : TEXCOORD0; // x = 0..1 distance, y = inner/outer edge position
				float2 texcoord1 : TEXCOORD1;
				float3 texcoord2 : TEXCOORD2; // world camera to vert/frag
				float3 texcoord3 : TEXCOORD3; // local vert/frag
#if LIGHT_1 || LIGHT_2
				float3 texcoord4 : TEXCOORD4; // world vert/frag to light 1
	#if LIGHT_2
				float3 texcoord5 : TEXCOORD5; // world vert/frag to light 2
	#endif
#endif
#if SHADOW_1 || SHADOW_2
				float4 texcoord6 : TEXCOORD6; // world vert/frag
#endif
			};

			struct f2g
			{
				float4 color : SV_TARGET;
			};

			void Vert(a2v i, out v2f o)
			{
				float4 wVertex = mul(unity_ObjectToWorld, i.vertex);

				o.vertex    = UnityObjectToClipPos(i.vertex);
				o.texcoord0 = i.texcoord0;
				o.texcoord1 = i.texcoord1;
				o.texcoord2 = wVertex.xyz - _WorldSpaceCameraPos;
				o.texcoord3 = i.vertex.xyz;
#if LIGHT_1 || LIGHT_2
				o.texcoord4 = _Light1Position.xyz - wVertex.xyz;
	#if LIGHT_2
				o.texcoord5 = _Light2Position.xyz - wVertex.xyz;
	#endif
#endif
#if SHADOW_1 || SHADOW_2
				o.texcoord6 = wVertex;
#endif
			}

			void Frag(v2f i, out f2g o)
			{
				i.texcoord0.y = i.texcoord1.y / i.texcoord1.x;
				float4 main = _Color * tex2D(_MainTex, i.texcoord0);
#if SGT_B // Detail
				i.texcoord0.y += pow(i.texcoord0.x, _DetailTwistBias) * _DetailTwist;
				float4 detail = tex2D(_DetailTex, i.texcoord0 * _DetailScale + _DetailOffset);
				main.a *= detail.a;
#endif
#if SGT_C // Near
				float2 near01 = length(i.texcoord2) * _NearScale;
				float  near   = tex2D(_NearTex, near01).a;
				main.a *= near;
#endif
				o.color = main;
#if LIGHT_0 || LIGHT_1 || LIGHT_2
				o.color.rgb *= UNITY_LIGHTMODEL_AMBIENT.rgb;
	#if LIGHT_1 || LIGHT_2
				i.texcoord2 = normalize(i.texcoord2);

				float2 theta    = dot(i.texcoord2, normalize(i.texcoord4));
				float4 lighting = tex2D(_LightingTex, theta * 0.5f + 0.5f) * main * _Light1Color;
		#if SGT_A // Scattering
				float4 scattering = MiePhase2(theta.x, _ScatteringMie) * _Light1Scatter;
			#if LIGHT_2
				theta       = dot(i.texcoord2, normalize(i.texcoord5));
				scattering += MiePhase2(theta.x, _ScatteringMie) * _Light2Scatter;
			#endif
				scattering *= o.color.a * (1.0f - o.color.a); // Fade scattering out according to optical depth
				scattering *= main;
				scattering *= saturate(1.0f - (o.color + lighting)); // Only scatter into remaining rgba

				lighting += scattering;
		#endif
		#if SHADOW_1 || SHADOW_2
				lighting *= ShadowColor(i.texcoord6);
		#endif
				o.color += lighting;
				o.color.a = saturate(o.color.a);
	#endif
#endif
			}
			ENDCG
		} // Pass
	} // SubShader
} // Shader