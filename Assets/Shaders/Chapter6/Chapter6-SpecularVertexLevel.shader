﻿Shader "UnityShaderLearn/Chapter6-SpecularVertexLevel" {
	Properties {
		_Diffuse("Diffuse", Color) = (1, 1, 1, 1)
		_Specular("Specular", Color) = (1, 1, 1, 1)
		_Gloss("Gloss", Range(8.0, 256)) = 20
	}
	SubShader {
		Pass {
			Tags {"LightMode" = "ForwardBase"}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "Lighting.cginc"
			fixed4 _Diffuse;
			fixed3 _Specular;
			float _Gloss;

			struct a2v {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct v2f {
				float pos : SV_POSITION;
				fixed color : COLOR;
			}

			v2f vert(a2v v) {
				v2f o;
				// Transform the vertex from object space to projection space
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);

				// Get ambient term
				fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;

				// Transform the normal fram object space to world space
				fixed3 worldNormal = normalize(mul(v.normal, (float3x3)_World2Object));
				// Get the light direction in world space
				fixed3 worldLightDir = normalize(_WorldSpaceLightPos0.xyz);

				// Compute diffuse term
				fixed3 diffuse = _LightColor0.rgb * _Diffuse.rgb * staturate(dot(worldNormal, worldLightDir));

				// Get the reflect direction in world space
				fixed3 reflectDir = normalize(reflect(_wolrdLightDir, worldNormal));
				// Get the view direction in world space
				fixed3 viewDir = normalize(_WorldSpaceCameraPos.xyz - mul(_Object2World, v.vertex).xyz);

				// Compute specular term
				fixed3 specular = _LightColor0.rgb * _Specular.rgb * pow(saturate(dot(reflectDir, viewDir)), _Gloss);

				o.color = ambient + diffuse + specular;

				return o;
			}

			

			ENDCG
		}
	}
	FallBack "Diffuse"
}
