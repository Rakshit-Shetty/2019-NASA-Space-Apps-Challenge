Shader "Space Graphics Toolkit/Star"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_Brightness("Brightness", Float) = 1.0
		_TimeScale("Time Scale", Float) = 1.0

		_RimColor("Rim Color", Color) = (1,1,1,1)
		_RimPower("Rim Power", Float) = 2.0

		[NoScaleOffset]_NoiseTex("Noise Tex", 3D) = "black" {}
		_NoiseStep("Noise Step", Vector) = (23, 29, 31)
		_NoiseColor("Noise Color", Color) = (1,1,1,1)
		_NoiseStrength("Noise Strength", Float) = 0.1
		_NoiseTile("Noise Tile", Float) = 1.0
		_NoiseBias("Noise Bias", Float) = 0.5
		_NoiseSharpness("Noise Sharpen", Float) = 1.0
	}
	SubShader
	{
		Tags{ "RenderType" = "Opaque" }

		Pass
		{
			CGPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag

			#define NOISE_OCTAVES 8

			float3 _Color;
			float  _Brightness;
			float  _TimeScale;

			float3 _RimColor;
			float  _RimPower;

			sampler3D _NoiseTex;
			float4    _NoiseTex_TexelSize;
			float3    _NoiseStep;
			float3    _NoiseColor;
			float     _NoiseStrength;
			float     _NoiseTile;
			float     _NoiseBias;
			float     _NoiseSharpness;

			float Noise4D(float4 p)
			{
				float4 i     = floor(p);
				float4 f     = smoothstep(0.0, 1.0, frac(p));
				float3 pixel = i.xyz + f.xyz + i.w * _NoiseStep;
				float4 noise = tex3D(_NoiseTex, (pixel + 0.5) / _NoiseTex_TexelSize.w);

				return lerp(noise.x, noise.y, f.w);
			}

			struct a2v
			{
				float4 vertex    : POSITION;
				float4 normal    : NORMAL;
				float2 texcoord0 : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex      : SV_POSITION;
				float2 texcoord0   : TEXCOORD0;
				float3 localPos    : TEXCOORD1;
				float3 worldNormal : TEXCOORD2;
				float3 worldRefl   : TEXCOORD3;
			};

			struct f2g
			{
				float4 color : SV_TARGET;
			};

			void Vert(a2v i, out v2f o)
			{
				o.vertex      = UnityObjectToClipPos(i.vertex);
				o.texcoord0   = i.texcoord0;
				o.localPos    = i.vertex.xyz;
				o.worldNormal = mul((float3x3)unity_ObjectToWorld, i.normal);
				o.worldRefl   = mul(unity_ObjectToWorld, i.vertex).xyz - _WorldSpaceCameraPos;
			}

			void Frag(v2f i, out f2g o)
			{
				// Calc initial noise params
				float  str   = 1.0f;
				float4 pos   = float4(i.localPos * _NoiseTile, 0.0f);
				float  noise = 0.0f;

				// Loop through each octave and contribute
				for (int j = 0; j < NOISE_OCTAVES; j++)
				{
					pos.w += _Time.x * _TimeScale;
					noise += (Noise4D(pos) - _NoiseBias) * str;
					str   /= 2.0f;
					pos   *= 2.0f;
				}

				// Normalize vectors before use
				i.worldNormal = normalize(i.worldNormal);
				i.worldRefl   = normalize(i.worldRefl);

				// Find dot between normal and reflection vectors
				float nfDot = abs(dot(i.worldNormal, i.worldRefl));

				// Make the color a rim gradient
				float rim = 1.0f - pow(1.0f - nfDot, _RimPower);

				o.color.rgb = lerp(_RimColor, _Color, rim) * _Brightness - _NoiseColor * _NoiseStrength * noise;
				o.color.rgb = pow(o.color.rgb, _NoiseSharpness);
				o.color.a   = 1.0f;
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}