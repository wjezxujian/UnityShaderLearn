Shader "Custom/BumpedSpecular" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Main Tex", 2D) = "white" {}
		_BumpMap ("Normal Map", 2D) = "bump" {}
		_Specular ("Specular Color", Color) = (1, 1, 1, 1)
		_Gloss ("Gloss", Range(8.0, 256)) = 20
	}
	SubShader {
		Tags {"RenderType"="Opaque" "Queue"="Geometry"}

		Pass {
			Tags {"LightMode"="ForwardBase"}

			CGPROGRAM
			
			#pragma multi_complie_fwdbase

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "AutoLight.cginc"

			fixed4 _Color;
			sampler2D _MianTex;
			float4 _MainTex_ST;
			sampler2D _BumpMap;
			float4 _Specular;
			float _Gloss;

			struct a2v {
				float4 vertex : POSITION;
				float3 normal : TEXCOORD0;
				float4 tangent : TEXCOORD1;
				float4 texcoord : TEXCOORD2;
			};

			struct v2f {
				float4 pos : SV_POSITION;
				float4 uv : TEXCOORD0;
				float4 TtoW0 : TEXCOORD1;
				float4 TtoW1 : TEXCOORD2;
				float4 TtoW2 : TEXCOORD3;
				SHADOW_COORDS(4)
			};

			v2f vert(a2v v) {
				v2f o;

				

			}

			ENDCG
		}

		Pass {

		}
	}
	FallBack "Diffuse"
}
