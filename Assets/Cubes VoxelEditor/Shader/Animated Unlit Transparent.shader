// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/Transparent Animated" {
	Properties{
		_MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
		_ScrollSpeeds("Scroll Speeds", vector) = (0, 0, 0, 0)
		_Color("Main Color", Color) = (1,1,1,1)
	}

		SubShader{
			Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
			LOD 100

			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off

			Pass {
				CGPROGRAM
					#pragma vertex vert
					#pragma fragment frag
					#pragma multi_compile_fog

					#include "UnityCG.cginc"

					struct appdata_t {
						float4 vertex : POSITION;
						float2 texcoord : TEXCOORD0;
						float2 uv : TEXCOORD0;
					};

					struct v2f {
						float2 uv : TEXCOORD0;
						float4 vertex : SV_POSITION;
						half2 texcoord : TEXCOORD1;
						UNITY_FOG_COORDS(1)
					};

					sampler2D _MainTex;
					float4 _MainTex_ST;
					float4 _ScrollSpeeds;
					float4 _Color;

					v2f vert(appdata_t v)
					{
						v2f o;
						o.vertex = UnityObjectToClipPos(v.vertex);
						o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
						o.uv = TRANSFORM_TEX(v.uv, _MainTex);
						o.uv += _ScrollSpeeds * _Time.x;
						UNITY_TRANSFER_FOG(o,o.vertex);
						return o;
					}

					fixed4 frag(v2f i) : SV_Target
					{
						fixed4 col = tex2D(_MainTex, i.texcoord);
						UNITY_APPLY_FOG(i.fogCoord, col);
						col = col * _Color;
						return col;
					}
				ENDCG
			}
	}

}
