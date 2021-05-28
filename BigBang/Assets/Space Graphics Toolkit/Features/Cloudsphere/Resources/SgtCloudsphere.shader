Shader "Hidden/SgtCloudsphere"
{
	Properties
	{
		_Color("Color", Color) = (1, 1, 1, 1)
		_MainTex("Main Tex", CUBE) = "white" {}
		_DepthTex("Depth Tex", 2D) = "white" {}

		_NearTex("Near Tex", 2D) = "white" {}
		_NearScale("Near Scale", Float) = 1

		_DetailTex("Detail Tex", 2D) = "white" {}
		_DetailScale("Detail Scale", Float) = 0.1
		_DetailTiling("Detail Tiling", Float) = 1

		_SoftParticlesFactor("Soft Particles Factor", Float) = 1

		_LightingTex("Lighting Tex", 2D) = "white" {}

		_Light1Color("Light 1 Color", Color) = (0,0,0)
		_Light1Scatter("Light 1 Scatter", Color) = (0,0,0)
		_Light1Position("Light 1 Position", Vector) = (0,0,0)
		_Light1Direction("Light 1 Direction", Vector) = (0,0,0)

		_Light2Color("Light 2 Color", Color) = (0,0,0)
		_Light2Scatter("Light 2 Scatter", Color) = (0,0,0)
		_Light2Position("Light 2 Position", Vector) = (0,0,0)
		_Light2Direction("Light 2 Direction", Vector) = (0,0,0)

		_Shadow1Texture("Shadow 1 Texture", 2D) = "white" {}
		_Shadow1Ratio("Shadow 1 Ratio", Float) = 1

		_Shadow2Texture("Shadow 2 Texture", 2D) = "white" {}
		_Shadow2Ratio("Shadow 2 Ratio", Float) = 1
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"RenderType" = "Transparent"
			"IgnoreProjector" = "True"
		}
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha, One One
			Cull Back
			Lighting Off
			ZWrite Off

			CGPROGRAM
			#include "SgtLight.cginc"
			#include "SgtShadow.cginc"
			#include "UnityCG.cginc"
			#pragma vertex Vert
			#pragma fragment Frag
			// Near
			#pragma multi_compile __ SGT_A
			// Detail
			#pragma multi_compile __ SGT_B
			// Softness
			#pragma multi_compile __ SGT_C
			// Lights
			#pragma multi_compile __ LIGHT_0 LIGHT_1 LIGHT_2
			// Shadows
			#pragma multi_compile __ SHADOW_1 SHADOW_2

			float4      _Color;
			samplerCUBE _MainTex;
			sampler2D   _DepthTex;

			sampler2D _NearTex;
			float     _NearScale;

			sampler2D _DetailTex;
			float     _DetailScale;
			float     _DetailTiling;

			UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
			float _SoftParticlesFactor;

			sampler2D _LightingTex;

			struct a2v
			{
				float4 vertex    : POSITION;
				float3 normal    : NORMAL;
				float2 texcoord  : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
			};

			struct v2f
			{
				float4 vertex    : SV_POSITION;
				float3 normal    : NORMAL;
				float2 texcoord0 : TEXCOORD0; // 0..1 depth, [polar]
				float3 texcoord1 : TEXCOORD1; // world camera to vert/frag
#if SGT_B // Detail
				float4 texcoord2 : TEXCOORD2; // detail UV
#endif
#if SGT_C // Softness
				float4 texcoord3 : TEXCOORD3; // screenPos
#endif
#if LIGHT_1 || LIGHT_2
				float2 texcoord4 : TEXCOORD4; // light 1 theta
	#if LIGHT_2
				float2 texcoord5 : TEXCOORD5; // light 2 theta
	#endif
#endif
#if SHADOW_1 || SHADOW_2
				float4 texcoord6 : TEXCOORD6; // world pos
#endif
			};

			struct f2g
			{
				float4 color : SV_TARGET;
			};

			float4 sample2(sampler2D tex, float4 coords, float polar)
			{
				float4 tex1 = tex2D(tex, coords.xy);
				float4 tex2 = tex2D(tex, coords.zw);

				return lerp(tex1, tex2, polar);
			}

			void Vert(a2v i, out v2f o)
			{
				float4 wVertex = mul(unity_ObjectToWorld, i.vertex);
				float3 wNormal = normalize(mul((float3x3)unity_ObjectToWorld, i.normal));

				o.vertex       = UnityObjectToClipPos(i.vertex);
				o.normal       = i.normal;
				o.texcoord1    = wVertex.xyz - _WorldSpaceCameraPos;
				o.texcoord0.xy = dot(wNormal, normalize(-o.texcoord1));
#if SGT_B // Detail
				o.texcoord0.y = saturate((abs(i.texcoord.y - 0.5f) - 0.2f) * 30.0f);
				o.texcoord2 = float4(i.texcoord.xy, i.texcoord1.xy) * _DetailTiling; o.texcoord2.x *= 2.0f;
#endif
#if SGT_C // Softness
				o.texcoord3 = ComputeScreenPos(o.vertex);
				o.texcoord3.z = -UnityObjectToViewPos(i.vertex).z;
#endif
#if LIGHT_1 || LIGHT_2
				o.texcoord4 = dot(wNormal, _Light1Direction) * 0.5f + 0.5f;
	#if LIGHT_2
				o.texcoord5 = dot(wNormal, _Light2Direction) * 0.5f + 0.5f;
	#endif
#endif
#if SHADOW_1 || SHADOW_2
				o.texcoord6 = wVertex;
#endif
			}

			void Frag(v2f i, out f2g o)
			{
				float4 depth = tex2D(_DepthTex, i.texcoord0.xx);
				float4 main = _Color * texCUBE(_MainTex, i.normal) * depth;
#if SGT_A // Near
				float2 near01 = length(i.texcoord1) * _NearScale;
				float  near   = tex2D(_NearTex, near01).a;
				main.a *= near;
#endif
#if SGT_B // Detail
				float4 detail = sample2(_DetailTex, i.texcoord2, i.texcoord0.y) - 0.5f;
				main.a -= (1.0f - main.a) * detail.a * _DetailScale * main.a;
#endif
#if SGT_C // Softness
				float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.texcoord3)));
				float partZ = i.texcoord3.z;

				main.a *= smoothstep(0.0f, 1.0f, _SoftParticlesFactor * (sceneZ - partZ));
#endif
				o.color = main;
#if LIGHT_0 || LIGHT_1 || LIGHT_2
				o.color.rgb *= UNITY_LIGHTMODEL_AMBIENT;
	#if LIGHT_1 || LIGHT_2
				float4 lighting = main * tex2D(_LightingTex, i.texcoord4) * _Light1Color;
		#if LIGHT_2
				lighting += main * tex2D(_LightingTex, i.texcoord5) * _Light2Color;
		#endif
		#if SHADOW_1 || SHADOW_2
				lighting *= ShadowColor(i.texcoord6);
		#endif
				o.color += lighting;
	#endif
#endif
			}
			ENDCG
		} // Pass
	} // SubShader
} // Shader