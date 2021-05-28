Shader "Hidden/SgtProminence"
{
	Properties
	{
		_MainTex("Main Tex", 2D) = "white" {}
		_Color("Color", Color) = (1, 1, 1, 1)
		_FadePower("Fade Power", Float) = 1
		_ClipPower("Clip Power", Float) = 1
		_WorldPosition("World Position", Vector) = (0, 0, 0)

		_DetailTex("Detail Tex", 2D) = "white" {}
		_DetailScale("Detail Scale", Vector) = (0, 0, 0)
		_DetailOffset("Detail Offset", Vector) = (0, 0, 0)
		_DetailStrength("Detail Strength", Float) = 0.1
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
			Blend One One
			Cull Off
			ZWrite Off
			
			CGPROGRAM
				#pragma vertex Vert
				#pragma fragment Frag
				// Fade Edges
				#pragma multi_compile __ SGT_A
				// Clip Near
				#pragma multi_compile __ SGT_B
				// Distort
				#pragma multi_compile __ SGT_C
				// Detail
				#pragma multi_compile __ SGT_D
				
				sampler2D _MainTex;
				float4    _Color;
				float3    _WorldPosition;
				float     _FadePower;
				float     _ClipPower;

				sampler2D _DistortTex;
				float2    _DistortScale;
				float2    _DistortOffset;
				float     _DistortStrength;

				sampler2D _DetailTex;
				float2    _DetailScale;
				float2    _DetailOffset;
				float     _DetailStrength;
				
				struct a2v
				{
					float4 vertex    : POSITION;
					float3 normal    : NORMAL;
					float2 texcoord0 : TEXCOORD0; // x = 0..1 distance, y = inner/outer edge position
					float  texcoord1 : TEXCOORD1; // inner/outer radius
				};
				
				struct v2f
				{
					float4 vertex    : SV_POSITION;
					float2 texcoord0 : TEXCOORD0;
					float  texcoord1 : TEXCOORD1;
#if SGT_A // Fade Edges
					float  texcoord2 : TEXCOORD2; // Edge fade
#endif
#if SGT_B // Clip Near
					float3 texcoord3 : TEXCOORD3; // Vertex/pixel to center direction
					float3 texcoord4 : TEXCOORD4; // Camera to center direction
#endif
				};
				
				struct f2g
				{
					fixed4 color : SV_TARGET;
				};
				
				void Vert(a2v i, out v2f o)
				{
					float4 vertM = mul(unity_ObjectToWorld, i.vertex);
					
					o.vertex    = UnityObjectToClipPos(i.vertex);
					o.texcoord0 = i.texcoord0;
					o.texcoord1 = i.texcoord1;
#if SGT_A // Fade Edges
					float3 cam2vertM = normalize(_WorldSpaceCameraPos - vertM.xyz);
					float3 normalM   = normalize(mul((float3x3)unity_ObjectToWorld, i.normal));
					
					o.texcoord2 = pow(abs(dot(cam2vertM, normalM)), _FadePower);
#endif
#if SGT_B // Clip Near
					o.texcoord3 = normalize(_WorldPosition - vertM.xyz);
					o.texcoord4 = normalize(_WorldPosition - _WorldSpaceCameraPos);
#endif
				}
				
				void Frag(v2f i, out f2g o)
				{
					float  scaledDist = i.texcoord0.x;
					float  scaledEdge = i.texcoord0.y / i.texcoord1;
					float2 uv         = float2(scaledDist, scaledEdge);

#if SGT_C // Distort
					float distort = tex2D(_DistortTex, uv * _DistortScale - _DistortOffset).a - 0.5f;
					uv.y += distort * _DistortStrength * uv.x;
#endif
					o.color = tex2D(_MainTex, uv) * _Color;
#if SGT_D // Detail
					float detail = tex2D(_DetailTex, uv * _DetailScale - _DetailOffset).a - 0.5f;
					o.color.rgb += saturate(detail * _DetailStrength) * o.color.rgb;
#endif
#if SGT_A // Fade Edges
					o.color *= i.texcoord2;
#endif
#if SGT_B // Clip Near
					float fadeNear = saturate(dot(i.texcoord3, i.texcoord4));
					
					o.color *= pow(1.0f - fadeNear, _ClipPower);
#endif
				}
			ENDCG
		} // Pass
	} // SubShader
} // Shader