Shader "Hidden/SgtAccretion"
{
	Properties
	{
		_Color("Color", Color) = (1, 1, 1, 1)
		_MainTex("Main Tex", 2D) = "white" {}

		_NearTex("Near Tex", 2D) = "white" {}
		_NearScale("Near Scale", Float) = 1

		_DetailTex("Detail Tex", 2D) = "white" {}
		_DetailScale("Detail Scale", Vector) = (1,1,1)
		_DetailOffset("Detail Offset", Vector) = (0,0,0)
		_DetailTwist("Detail Twist", Float) = 0
		_DetailTwistBias("Detail Twist Bias", Float) = 0
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
			//Blend SrcAlpha OneMinusSrcAlpha, One One
			Blend One OneMinusSrcColor, One One
			Cull Off
			Lighting Off
			ZWrite Off

			CGPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag
			float4    _Color;
			sampler2D _MainTex;

			sampler2D _NearTex;
			float     _NearScale;

			sampler2D _DetailTex;
			float2    _DetailScale;
			float2    _DetailOffset;
			float     _DetailTwist;
			float     _DetailTwistBias;

			// Detail
			#pragma multi_compile __ SGT_B
			// Near
			#pragma multi_compile __ SGT_C

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
			}

			void Frag(v2f i, out f2g o)
			{
				i.texcoord0.y = i.texcoord1.y / i.texcoord1.x;
				float4 main = _Color * tex2D(_MainTex, i.texcoord0);
#if SGT_B // Detail
				i.texcoord0.y += pow(i.texcoord0.x, _DetailTwistBias) * _DetailTwist;
				float4 detail = tex2D(_DetailTex, i.texcoord0 * _DetailScale + _DetailOffset);
				main *= detail;
#endif
#if SGT_C // Near
				float2 near01 = length(i.texcoord2) * _NearScale;
				float  near = tex2D(_NearTex, near01).a;
				main.a *= near;
#endif
				main.rgb *= main.a;
				o.color = main;
			}
			ENDCG
		} // Pass
	} // SubShader
} // Shader