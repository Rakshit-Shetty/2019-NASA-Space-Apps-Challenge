Shader "Hidden/SgtAtmosphereOuter"
{
	Properties
	{
		_Color("Color", Color) = (1, 1, 1, 1)
		_DepthTex("Depth Tex", 2D) = "white" {}
		_HorizonLengthRecip("Horizon Length Recip", Float) = 0
		_Sky("Sky", Float) = 0

		_SoftParticlesFactor("Soft Particles Factor", Float) = 1

		_LightingTex("Lighting Tex", 2D) = "white" {}
		_ScatteringTex("Scattering Tex", 2D) = "white" {}
		_ScatteringMie("Scattering Mie", Float) = 0.5
		_ScatteringRayleigh("Scattering Rayleigh", Float) = 0.5

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
			"Queue"           = "Transparent"
			"RenderType"      = "Transparent"
			"IgnoreProjector" = "True"
		}
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha, One One
			Cull Front
			Lighting Off
			ZWrite Off
			ZTest LEqual

			CGPROGRAM
			#include "SgtLight.cginc"
			#include "SgtShadow.cginc"
			#include "UnityCG.cginc"
			#pragma vertex Vert
			#pragma fragment Frag
			// Softness
			#pragma multi_compile __ SGT_A
			// Scattering
			#pragma multi_compile __ SGT_B
			// Lights
			#pragma multi_compile __ LIGHT_0 LIGHT_1 LIGHT_2
			// Shadows
			#pragma multi_compile __ SHADOW_1 SHADOW_2

			float4    _Color;
			sampler2D _DepthTex;
			float     _HorizonLengthRecip;
			float     _Sky;
			float4x4  _WorldToLocal;
			float4x4  _LocalToWorld;

			UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
			float _SoftParticlesFactor;

			sampler2D _LightingTex;

			sampler2D _ScatteringTex;
			float     _ScatteringMie;
			float     _ScatteringRayleigh;

			struct a2v
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex    : SV_POSITION;
				float2 texcoord0 : TEXCOORD0; // 0..1 depth, [eyeDepth]
#if SGT_A // Softness
				float4 texcoord1 : TEXCOORD1; // screenPos
#endif
#if LIGHT_1 || LIGHT_2
				float4 texcoord3: TEXCOORD3; // xyz = light 1 to vertex/pixel, w = light 1 theta
	#if LIGHT_2
				float4 texcoord4 : TEXCOORD4; // xyz = light 2 to vertex/pixel, w = light 2 theta
	#endif
	#if SGT_B // Scattering
				float3 texcoord5 : TEXCOORD5; // camera to vertex/pixel
	#endif
#endif
#if SHADOW_1 || SHADOW_2
				float4 texcoord6 : TEXCOORD6; // near world position
#endif
			};

			struct f2g
			{
				float4 color : SV_TARGET;
			};

			float GetOutsideDistance(float3 ray, float3 rayD)
			{
				float B = dot(ray, rayD);
				float C = dot(ray, ray) - 1.0f;
				float D = B * B - C;
				return max(-B - sqrt(max(D, 0.0f)), 0.0f);
			}

			float3 GetNear(float3 far)
			{
				float3 near = mul(_WorldToLocal, float4(_WorldSpaceCameraPos, 1.0f)).xyz;
				float3 dir  = normalize(far - near);

				return near + dir * GetOutsideDistance(near, dir);
			}

			void Vert(a2v i, out v2f o)
			{
				float4 wPos = mul(unity_ObjectToWorld, i.vertex);
				float3 far  = mul(_WorldToLocal, wPos).xyz;
				float3 near = GetNear(far);

				o.vertex       = UnityObjectToClipPos(i.vertex);
				o.texcoord0.xy = length(near - far) * _HorizonLengthRecip;
#if SGT_A // Softness
				o.texcoord1 = ComputeScreenPos(o.vertex);
				o.texcoord1.z = -UnityObjectToViewPos(i.vertex).z;
#endif
#if LIGHT_1 || LIGHT_2
				float3 nearD = normalize(near);

				o.texcoord3 = dot(nearD, _Light1Direction) * 0.5f + 0.5f;
	#if LIGHT_2
				o.texcoord4 = dot(nearD, _Light2Direction) * 0.5f + 0.5f;
	#endif
	#if SGT_B // Scattering
				o.texcoord5     = _WorldSpaceCameraPos - wPos.xyz;
				o.texcoord3.xyz = _Light1Position.xyz - _WorldSpaceCameraPos;
		#if LIGHT_2
				o.texcoord4.xyz = _Light2Position.xyz - _WorldSpaceCameraPos;
		#endif
	#endif
#endif
#if SHADOW_1 || SHADOW_2
				o.texcoord6 = mul(_LocalToWorld, wPos);
#endif
			}

			void Frag(v2f i, out f2g o)
			{
				float4 depth = tex2D(_DepthTex, i.texcoord0.xx); depth.a = saturate(depth.a + (1.0f - depth.a) * _Sky);
				float4 main  = depth * _Color;
#if SGT_A // Softness
				float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.texcoord1)));
				float partZ = i.texcoord1.z;

				main.a *= smoothstep(0.0f, 1.0f, _SoftParticlesFactor * abs(sceneZ - partZ));
#endif
				o.color = main;
#if LIGHT_0 || LIGHT_1 || LIGHT_2
				o.color.rgb *= UNITY_LIGHTMODEL_AMBIENT;
	#if LIGHT_1 || LIGHT_2
				i.texcoord0.x = i.texcoord3.w;

				float4 lighting = tex2D(_LightingTex, i.texcoord0.xx) * main * _Light1Color;
		#if SGT_B // Scattering
				i.texcoord5 = normalize(-i.texcoord5);

				float  angle      = dot(i.texcoord5, normalize(i.texcoord3.xyz));
				float  phase      = MieRayleighPhase(angle, _ScatteringMie, _ScatteringRayleigh);
				float4 scattering = tex2D(_ScatteringTex, i.texcoord0.xx) * _Light1Scatter * phase;
			#if LIGHT_2
				angle       = dot(i.texcoord5, normalize(i.texcoord4.xyz));
				phase       = MieRayleighPhase(angle, _ScatteringMie, _ScatteringRayleigh);
				scattering += tex2D(_ScatteringTex, i.texcoord0.xx) * _Light2Scatter * phase;
			#endif
				scattering *= o.color.a; // Fade scattering out according to optical depth
				scattering *= saturate(1.0f - (o.color + lighting)); // Only scatter into remaining rgba

				lighting += scattering;
		#endif
		#if SHADOW_1 || SHADOW_2
				lighting *= lerp(ShadowColor(i.texcoord6), 1.0f, o.color.a);
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