Shader "Hidden/SgtCoronaOuter"
{
	Properties
	{
		_Color("Color", Color) = (1, 1, 1, 1)
		_DepthTex("Depth Tex", 2D) = "white" {}
		_HorizonLengthRecip("Horizon Length Recip", Float) = 0
		_Sky("Sky", Float) = 0
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
			#pragma vertex Vert
			#pragma fragment Frag
			#include "UnityCG.cginc"
			// Softness
			#pragma multi_compile __ SGT_A

			float4    _Color;
			sampler2D _DepthTex;
			float     _HorizonLengthRecip;
			float     _Sky;
			float4x4  _WorldToLocal;

			UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
			float _SoftParticlesFactor;

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
				float3 dir = normalize(far - near);

				return near + dir * GetOutsideDistance(near, dir);
			}

			void Vert(a2v i, out v2f o)
			{
				float4 wPos = mul(unity_ObjectToWorld, i.vertex);
				float3 far = mul(_WorldToLocal, wPos).xyz;
				float3 near = GetNear(far);

				o.vertex       = UnityObjectToClipPos(i.vertex);
				o.texcoord0.xy = length(near - far) * _HorizonLengthRecip;
#if SGT_A // Softness
				o.texcoord1   = ComputeScreenPos(o.vertex);
				o.texcoord1.z = -UnityObjectToViewPos(i.vertex).z;
#endif
			}

			void Frag(v2f i, out f2g o)
			{
				float4 depth = tex2D(_DepthTex, i.texcoord0.xx); depth.a = saturate(depth.a + (1.0f - depth.a) * _Sky);
				float4 main = depth * _Color;

#if SGT_A // Softness
				float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.texcoord1)));
				float partZ  = i.texcoord1.z;

				main.a *= smoothstep(0.0f, 1.0f, _SoftParticlesFactor * abs(sceneZ - partZ));
#endif
				o.color = main;
			}
			ENDCG
		} // Pass
	} // SubShader
} // Shader