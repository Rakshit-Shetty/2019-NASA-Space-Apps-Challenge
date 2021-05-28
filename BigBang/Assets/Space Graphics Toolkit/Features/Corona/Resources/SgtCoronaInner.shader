Shader "Hidden/SgtCoronaInner"
{
	Properties
	{
		_Color("Color", Color) = (1, 1, 1, 1)
		_DepthTex("Depth Tex", 2D) = "white" {}
		_HorizonLengthRecip("Horizon Length Recip", Float) = 0
		_InnerRatio("Inner Ratio", Float) = 0
		_InnerScale("Inner Scale", Float) = 1
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
			Cull Back
			Lighting Off
			Offset -0.1, 0
			ZWrite Off

			CGPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag

			float4    _Color;
			sampler2D _DepthTex;
			float     _HorizonLengthRecip;
			float     _InnerRatio;
			float     _InnerScale;
			float4x4  _WorldToLocal;

			struct a2v
			{
				float4 vertex   : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex    : SV_POSITION;
				float2 texcoord0 : TEXCOORD0; // 0..1 depth
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
				return -B - sqrt(max(D, 0.0f));
			}

			void Vert(a2v i, out v2f o)
			{
				float4 wPos = mul(unity_ObjectToWorld, i.vertex);
				float3 far  = mul(_WorldToLocal, wPos).xyz;
				float3 near = mul(_WorldToLocal, float4(_WorldSpaceCameraPos, 1.0f)).xyz;

				float3 nearFar = far - near;
				float3 dir     = normalize(nearFar);
				float  depthA  = length(nearFar);
				float  depthB  = GetOutsideDistance(near, dir);
				near += dir * max(depthB, 0.0f);
				float depth    = length(near - far);

				float alt01 = (length(far) - _InnerRatio) * _InnerScale;

				o.vertex = UnityObjectToClipPos(i.vertex);
				o.texcoord0.x = depth * _HorizonLengthRecip;
				o.texcoord0.y = 1.0f - alt01;
			}

			void Frag(v2f i, out f2g o)
			{
				float4 depth = tex2D(_DepthTex, i.texcoord0);
				float4 main = depth * _Color;

				//main.a *= tex2D(_DepthTex, i.texcoord0.zz).a;
				float alt = smoothstep(0.0f, 1.0f, saturate(i.texcoord0.y));
				main.a *= alt;

				o.color = main;
			}
			ENDCG
		} // Pass
	} // SubShader
} // Shader