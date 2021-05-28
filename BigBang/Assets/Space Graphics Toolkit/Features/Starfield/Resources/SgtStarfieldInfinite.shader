Shader "Hidden/SgtStarfieldInfinite"
{
	Properties
	{
		_Color("Color", Color) = (1, 1, 1, 1)
		_MainTex("Main Tex", 2D) = "white" {}
		_Scale("Scale", Float) = 1
		_ScaleRecip("Scale Recip", Float) = 1
		_CameraRollAngle("Camera Roll Angle", Float) = 0

		_WrapSize("Wrap Size", Vector) = (0,0,0)
		_WrapScale("Wrap Scale", Vector) = (0,0,0)

		_StretchDirection("Stretch Direction", Vector) = (0,0,0)
		_StretchVector("Stretch Vector", Float) = 0

		_NearTex("Near Tex", 2D) = "white" {}
		_NearScale("Near Scale", Float) = 0

		_FarTex("Far Tex", 2D) = "white" {}
		_FarRadius("Far Radius", Float) = 0
		_FarScale("Far Scale", Float) = 0

		_PulseOffset("Pulse Offset", Float) = 1

		_SoftParticlesFactor("Soft Particles Factor", Float) = 1

		_SrcMode("Src Mode", Float) = 1 // 1 = One
		_DstMode("Dst Mode", Float) = 1 // 1 = One
		_ZWriteMode("ZWrite Mode", Float) = 8 // 8 = Always
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
			Blend [_SrcMode][_DstMode]
			ZWrite[_ZWriteMode]
			//ZTest[_ZTestMode]
			ZTest LEqual
			Cull Off

			CGPROGRAM
				#include "UnityCG.cginc"
				#pragma vertex Vert
				#pragma fragment Frag
				// Alpha Test
				#pragma multi_compile __ SGT_A
				// RGB Power
				#pragma multi_compile __ SGT_B
				// Stretch
				#pragma multi_compile __ SGT_C
				// Near
				#pragma multi_compile __ SGT_D
				// Far
				#pragma multi_compile __ SGT_E
				// Pulse (avoid using SGT_F)
				#pragma multi_compile __ LIGHT_1
				// Softness (avoid using SGT_G)
				#pragma multi_compile __ LIGHT_2

				float4    _Color;
				sampler2D _MainTex;
				float     _Scale;
				float     _ScaleRecip;
				float     _CameraRollAngle;

				float3 _WrapSize;
				float3 _WrapScale;

				float3 _StretchDirection;
				float3 _StretchVector;

				sampler2D _NearTex;
				float     _NearScale;

				sampler2D _FarTex;
				float     _FarRadius;
				float     _FarScale;

				float _PulseOffset;

				UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
				float _SoftParticlesFactor;

				struct a2v
				{
					float4 vertex    : POSITION;
					float4 color     : COLOR;
					float3 normal    : NORMAL; // xy = corner offset, z = angle
					float3 tangent   : TANGENT; // x = pulse offset, y = pulse speed, z = pulse scale
					float2 texcoord0 : TEXCOORD0; // uv
					float2 texcoord1 : TEXCOORD1; // x = radius, y = back or front [-0.5 .. 0.5]
				};

				struct v2f
				{
					float4 vertex    : SV_POSITION;
					float4 color     : COLOR;
					float2 texcoord0 : TEXCOORD0;
					float3 texcoord1 : TEXCOORD1; // mvpos
#if LIGHT_2 // Soft particles
					float4 projPos : TEXCOORD2;
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

				void Vert(a2v i, out v2f o)
				{
					float4 cameraO   = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1.0f)) * _ScaleRecip;
					float3 relativeO = i.vertex.xyz - cameraO.xyz;
					i.vertex.xyz = cameraO.xyz + (frac(relativeO * _WrapScale + 0.5f) - 0.5f) * _WrapSize;

					float radius = i.texcoord1.x * _Scale;
#if SGT_C // Stretch
					float4 vertexM  = mul(unity_ObjectToWorld, i.vertex);
					float3 up       = cross(_StretchDirection, normalize(vertexM.xyz - _WorldSpaceCameraPos));

					// Uncomment below if you want the stars to be stretched based on their size too
					vertexM.xyz += _StretchVector * i.texcoord1.y; // * radius;
					vertexM.xyz += up * i.normal.y * radius;

					o.vertex    = mul(UNITY_MATRIX_VP, vertexM);
					o.texcoord1 = mul(UNITY_MATRIX_V, vertexM);
#else
	#if LIGHT_1 // Pulse
					radius *= 1.0f + sin(i.tangent.x * 3.141592654f + _PulseOffset * i.tangent.y) * i.tangent.z;
	#endif
					float3 vertexMV = UnityObjectToViewPos(i.vertex);
					float4 cornerMV = float4(vertexMV, 1.0f);
					float  angle    = _CameraRollAngle + i.normal.z * 3.141592654f;

					i.normal.xy = Rotate(i.normal.xy, angle);

					cornerMV.xy += i.normal.xy * radius;

					o.vertex    = mul(UNITY_MATRIX_P, cornerMV);
					o.texcoord1 = cornerMV.xyz;
#endif
					o.color     = i.color * _Color;
					o.texcoord0 = i.texcoord0;
#if LIGHT_2 // Softness
					o.projPos = ComputeScreenPos(o.vertex);
					o.projPos.z = -UnityObjectToViewPos(i.vertex).z;
#endif
				}

				void Frag(v2f i, out f2g o)
				{
					float dist = length(i.texcoord1);
					o.color = tex2D(_MainTex, i.texcoord0);
#if SGT_B // RGB Power
					o.color.rgb = pow(o.color.rgb, float3(1.0f, 1.0f, 1.0f) + (1.0f - i.color.rgb) * 10.0f);
#else
					o.color *= i.color;
#endif
#if SGT_D // Near
					float2 near = dist * _NearScale;
					o.color *= tex2D(_NearTex, near);
#endif
#if SGT_E // Far
					float2 far = (_FarRadius - dist) * _FarScale;
					o.color *= tex2D(_FarTex, far);
#endif
#if LIGHT_2 // Softness
					float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
					float partZ  = i.projPos.z;

					o.color *= saturate(_SoftParticlesFactor * (sceneZ - partZ));
#endif
					o.color.a = saturate(o.color.a);
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