Shader "Space Graphics Toolkit/Flare"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1, 1, 1, 1)
		_ZTest("ZTest", Float) = 2 // 2 = LEqual
		_DstBlend("DstBlend", Float) = 1 // 1 = One
	}
	SubShader
	{
		Tags
		{
			"Queue"       = "Transparent"
			"RenderType"  = "Transparent"
			"PreviewType" = "Plane"
		}

		Pass
		{
			Blend One [_DstBlend]
			Cull Off
			ZWrite Off
			ZTest [_ZTest]

			CGPROGRAM
				#pragma vertex   Vert
				#pragma fragment Frag

				sampler2D _MainTex;
				float4    _Color;

				struct a2v
				{
					float4 vertex   : POSITION;
					float2 texcoord : TEXCOORD0;
				};

				struct v2f
				{
					float4 vertex   : SV_POSITION;
					float2 texcoord : TEXCOORD0;
				};

				struct f2g
				{
					float4 color : SV_TARGET;
				};

				void Vert(a2v i, out v2f o)
				{
					o.vertex   = UnityObjectToClipPos(i.vertex);
					o.texcoord = i.texcoord;
				}

				void Frag(v2f i, out f2g o)
				{
					float4 color = _Color;

					color.rgb *= color.a;

					o.color = tex2D(_MainTex, i.texcoord) * color;
				}
		ENDCG
		} // Pass
	} // SubShader
} // Shader