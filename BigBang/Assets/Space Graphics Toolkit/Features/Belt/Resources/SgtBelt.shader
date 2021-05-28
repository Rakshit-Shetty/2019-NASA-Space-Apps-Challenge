Shader "Hidden/SgtBelt"
{
	Properties
	{
		_CameraRollAngle("Camera Roll Angle", Float) = 0
		_MainTex("Main Tex", 2D) = "white" {}
		_HeightTex("Height Tex", 2D) = "black" {}
		_Color("Color", Color) = (1, 1, 1, 1)
		_Scale("Scale", Float) = 1
		_Age("Age", Float) = 0

		_LightingTex("Lighting Tex", 2D) = "white" {}

		_SrcMode("Src Mode", Float) = 1 // 1 = One
		_DstMode("Dst Mode", Float) = 1 // 0 = Zero
		_ZWriteMode("ZWrite Mode", Float) = 4 // 4 = Less Equal
	}
	SubShader
	{
		Tags
		{
			"Queue"           = "Geometry"
			"RenderType"      = "TransparentCutout"
			"IgnoreProjector" = "True"
		}
		Pass
		{
			Blend[_SrcMode][_DstMode]
			ZWrite[_ZWriteMode]
			ZTest LEqual
			Cull Back

			CGPROGRAM
				#include "UnityCG.cginc"
				#include "SgtLight.cginc"
				#include "SgtShadow.cginc"
				#pragma vertex Vert
				#pragma fragment Frag
				// Alpha Test
				#pragma multi_compile __ SGT_A
				// PowerRGB
				#pragma multi_compile __ SGT_B
				// Lights
				#pragma multi_compile __ LIGHT_0 LIGHT_1 LIGHT_2
				// Shadows
				#pragma multi_compile __ SHADOW_1 SHADOW_2

				sampler2D _MainTex;
				sampler2D _HeightTex;
				float4    _Color;
				float     _Scale;
				float     _Age;
				float     _CameraRollAngle;

				sampler2D _LightingTex;

				struct a2v
				{
					float4 vertex    : POSITION; // x = orbit angle, y = orbit distance, z = orbit speed
					float4 color     : COLOR;
					float3 normal    : NORMAL;
					float2 tangent   : TANGENT; // x = angle, y = spin
					float2 texcoord0 : TEXCOORD0; // uv
					float2 texcoord1 : TEXCOORD1; // x = radius, y = height
				};

				struct v2f
				{
					float4 vertex    : SV_POSITION;
					float4 color     : COLOR;
					float2 texcoord0 : TEXCOORD0; // uv
					float3 texcoord1 : TEXCOORD1; // ray
					float3 texcoord2 : TEXCOORD2; // rayD
#if LIGHT_1 || LIGHT_2
					float3 texcoord3 : TEXCOORD3; // view vertex/pixel to light 1
	#if LIGHT_2
					float3 texcoord4 : TEXCOORD4; // view vertex/pixel to light 2
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

				float2 Rotate(float2 v, float a)
				{
					float s = sin(a);
					float c = cos(a);
					return float2(c * v.x - s * v.y, s * v.x + c * v.y);
				}

				float3 GetOutside(float3 ray, float3 rayD)
				{
					float B = dot(ray, rayD);
					float C = dot(ray, ray) - 1.0f;
					float D = B * B - C;
					return ray + rayD * max(-B - sqrt(max(D, 0.0f)), 0.0f);
				}

				void Vert(a2v i, out v2f o)
				{
					float orbitAngle    = i.vertex.x + i.vertex.z * _Age;
					float orbitDistance = i.vertex.y;
					float angle         = _CameraRollAngle + (i.tangent.x + i.tangent.y * _Age) * 3.141592654f;
					float radius        = i.texcoord1.x * _Scale;

					i.vertex.x = sin(orbitAngle) * orbitDistance;
					i.vertex.y = i.texcoord1.y;
					i.vertex.z = cos(orbitAngle) * orbitDistance;
					i.vertex.w = 1.0f;

					i.normal.xy = Rotate(i.normal.xy, angle);

					float4 wPos     = mul(unity_ObjectToWorld, i.vertex);
					float3 vertexMV = UnityObjectToViewPos(i.vertex);
					float4 cornerMV = float4(vertexMV, 1.0f);

					cornerMV.xyz += i.normal * radius;

					o.vertex    = mul(UNITY_MATRIX_P, cornerMV);
					o.color     = i.color * _Color;
					o.texcoord0 = i.texcoord0;
					o.texcoord1 = -vertexMV / radius;
					o.texcoord2 = cornerMV.xyz;
#if LIGHT_1 || LIGHT_2
					float3 light1v = mul(UNITY_MATRIX_V, _Light1Position).xyz;
					o.texcoord3 = normalize(light1v - vertexMV);
	#if LIGHT_2
					float3 light2v = mul(UNITY_MATRIX_V, _Light2Position).xyz;
					o.texcoord4 = normalize(light2v - vertexMV);
	#endif
#endif
#if SHADOW_1 || SHADOW_2
					o.texcoord6 = wPos;
#endif
				}

				void Frag(v2f i, out f2g o)
				{
					float4 main = tex2D(_MainTex, i.texcoord0);
#if SGT_B // PowerRGB
					o.color = main;
					o.color.rgb = pow(o.color.rgb, float3(1.0f, 1.0f, 1.0f) + (1.0f - i.color.rgb) * 10.0f);
#else
					main *= i.color;
					o.color = main;
#endif
#if LIGHT_0 || LIGHT_1 || LIGHT_2
					o.color.rgb *= UNITY_LIGHTMODEL_AMBIENT.rgb;
	#if LIGHT_1 || LIGHT_2
					float3 normal   = GetOutside(i.texcoord1, normalize(i.texcoord2));
					float2 theta    = dot(normal, i.texcoord3);
					float3 lighting = tex2D(_LightingTex, theta * 0.5f + 0.5f) * _Light1Color;
		#if LIGHT_2
					theta     = dot(normal, i.texcoord4);
					lighting += tex2D(_LightingTex, theta * 0.5f + 0.5f) * _Light2Color;
		#endif
		#if SHADOW_1 || SHADOW_2
					lighting *= ShadowColor(i.texcoord6);
		#endif
					o.color.rgb += lighting.rgb * main.rgb;
	#endif
#endif
#if SGT_A // Alpha Test
					if (o.color.a < 0.5f)
					{
						o.color.a = 0.0f; discard;
					}
					else
					{
						o.color.a = 1.0f;
					}
#endif
				}
			ENDCG
		} // Pass
	} // SubShader
} // Shader