Shader "Hidden/SgtJovian"
{
	Properties
	{
		_Color("Color", Color) = (1, 1, 1, 1)
		_MainTex("Main Tex", CUBE) = "white" {}
		_DepthTex("Depth Tex", 2D) = "white" {}
		_Sky("Sky", Float) = 0

		_LightingTex("Lighting Tex", 2D) = "white" {}

		_ScatteringTex("Scattering Tex", 2D) = "white" {}
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

			CGPROGRAM
			#include "SgtLight.cginc"
			#include "SgtShadow.cginc"
			#pragma vertex Vert
			#pragma fragment Frag
			// Scattering
			#pragma multi_compile __ SGT_B
			// Lights
			#pragma multi_compile __ LIGHT_0 LIGHT_1 LIGHT_2
			// Shadows
			#pragma multi_compile __ SHADOW_1 SHADOW_2

			float4      _Color;
			samplerCUBE _MainTex;
			sampler2D   _DepthTex;
			float       _Sky;
			sampler2D   _LightingTex;
			sampler2D   _ScatteringTex;
			float4x4    _WorldToLocal;
			float4x4    _LocalToWorld;

			struct a2v
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex    : SV_POSITION;
				float2 texcoord0 : TEXCOORD0; // 0..1 depth
				float3 texcoord5 : TEXCOORD5; // near
#if LIGHT_1 || LIGHT_2
				float4 texcoord1 : TEXCOORD1; // xyz = light 1 to vertex/pixel, w = light 1 theta
	#if LIGHT_2
				float4 texcoord2 : TEXCOORD2; // xyz = light 2 to vertex/pixel, w = light 2 theta
	#endif
	#if SGT_B // Scattering
				float3 texcoord4 : TEXCOORD4; // camera to vertex/pixel
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

				o.vertex    = UnityObjectToClipPos(i.vertex);
				o.texcoord0 = length(near - far);
				o.texcoord5 = float3(-near.x, near.yz);
				//o.texcoord5 = near;
#if LIGHT_1 || LIGHT_2
				float3 nearD = normalize(near);

				o.texcoord1 = dot(nearD, _Light1Direction) * 0.5f + 0.5f;
	#if LIGHT_2
				o.texcoord2 = dot(nearD, _Light2Direction) * 0.5f + 0.5f;
	#endif
	#if SGT_B // Scattering
				o.texcoord4 = _WorldSpaceCameraPos - wPos.xyz;
				o.texcoord1.xyz = _Light1Position.xyz - _WorldSpaceCameraPos;
		#if LIGHT_2
				o.texcoord2.xyz = _Light2Position.xyz - _WorldSpaceCameraPos;
		#endif
	#endif
#endif
#if SHADOW_1 || SHADOW_2
				o.texcoord6 = mul(_LocalToWorld, near);
#endif
			}

			void Frag(v2f i, out f2g o)
			{
				float4 depth = tex2D(_DepthTex, i.texcoord0.xx); depth.a = saturate(depth.a + (1.0f - depth.a) * _Sky);
				float4 main  = _Color * texCUBE(_MainTex, i.texcoord5) * depth;
				o.color = main;
#if LIGHT_0 || LIGHT_1 || LIGHT_2
				o.color.rgb *= UNITY_LIGHTMODEL_AMBIENT;
	#if LIGHT_1 || LIGHT_2
				i.texcoord0 = i.texcoord1.ww;

				float4 lighting = main * tex2D(_LightingTex, i.texcoord0) * _Light1Color;
		#if SGT_B // Scattering
				i.texcoord4 = normalize(-i.texcoord4);

				i.texcoord0.y = dot(i.texcoord4, normalize(i.texcoord1.xyz)) * 0.5f + 0.5f;

				float4 scattering = tex2D(_ScatteringTex, i.texcoord0) * _Light1Scatter;
			#if LIGHT_2
				i.texcoord0.y = dot(i.texcoord4, normalize(i.texcoord2.xyz)) * 0.5f + 0.5f;

				scattering += tex2D(_ScatteringTex, i.texcoord0) * _Light2Scatter;
			#endif
				scattering *= o.color.a; // Fade scattering out according to optical depth
				scattering *= 1.0f - o.color.a;
				scattering *= saturate(1.0f - (o.color + lighting)); // Only scatter into remaining rgba

				lighting += scattering;
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
