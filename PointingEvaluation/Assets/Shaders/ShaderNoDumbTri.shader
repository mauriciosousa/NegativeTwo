Shader "Unlit/ShaderNoDumbTri"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_ShaderDistance ("ShaderDistance", Range(0, 1.0)) = 0.5
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			SamplerState g_samPoint
			{
				Filter = MIN_MAG_MIP_POINT;
				AddressU = Wrap;
				AddressV = Wrap;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _ShaderDistance;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);   
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			[maxvertexcount(3)]
			void geom(triangle v2f input[3], inout TriangleStream<v2f> OutputStream)
			{


				float lod = 0; // your lod level ranging from 0 to number of mipmap levels.
				float c0 = tex2Dlod(_MainTex, float4(input[0].uv, 0, lod)).a;
				float c1 = tex2Dlod(_MainTex, float4(input[1].uv, 0, lod)).a;
				float c2 = tex2Dlod(_MainTex, float4(input[2].uv, 0, lod)).a;

				/*if (distance(input[0].vertex, input[1].vertex) < _ShaderDistance & distance(input[0].vertex, input[2].vertex) < _ShaderDistance & distance(input[1].vertex, input[2].vertex) < _ShaderDistance
					& c0 != 0 & c1 != 0 & c2 != 0)*/
				{
					OutputStream.Append(input[0]);
					OutputStream.Append(input[1]);
					OutputStream.Append(input[2]);
				}
			}

			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
