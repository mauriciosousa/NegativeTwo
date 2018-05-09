// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Custom/Depth Billboard"
{
	Properties 
	{
		_Size ("Size", Range(0, 3)) = 0.03 //patch size
		_ColorTex ("Texture", 2D) = "white" {}
		_DepthTex ("TextureD", 2D) = "white" {}
		_ShaderDistance ("ShaderDistance", Range(0, 1.0)) = 0.1
		//_PointDistanceThreshold("PointDistanceThreshold", Range(0, 2.0)) = 0.1
	}

	SubShader 
	{
		Pass
		{
			Tags { "RenderType"="Transparent" }
			
			Cull Off // render both back and front faces

			CGPROGRAM
				#pragma target 5.0
				#pragma vertex VS_Main
				#pragma fragment frag
				#pragma geometry geom
				#include "UnityCG.cginc" 

				// **************************************************************
				// Data structures												*
				// **************************************************************
				struct v2f
				{
					float4	pos		: POSITION;
					float4 color	: COLOR;
				};



				// **************************************************************
				// Vars															*
				// **************************************************************

				float _Size;
				sampler2D _ColorTex;				
				sampler2D _DepthTex; 
				int _TexScale;
				float4 _Color; 
				float _ShaderDistance;
				float _PointDistanceThreshold;


				//VARIAVEIS PARA DISTORCER O BRACINHO
				float4x4 _RightTransform;
				float3 _RightMidPoint;
				float _RightDistance;
				float3 _RightShoulder;
				float3 _RightElbow;
				float3 _RightWrist;
				float3 _RightHand;
				float3 _RightHandTip;
				int _RightPointing;

				float4x4 _LeftTransform;
				float3 _LeftMidPoint;
				float _LeftDistance;
				float3 _LeftShoulder;
				float3 _LeftElbow;
				float3 _LeftWrist;
				float3 _LeftHand;
				float3 _LeftHandTip;
				int _LeftPointing;

				float3 _VRHeadPos;
				float _HeadSize;
				// **************************************************************
				// Shader Programs												*
				// **************************************************************

				// Vertex Shader ------------------------------------------------
				v2f VS_Main(appdata_full v)
				{
					v2f output = (v2f)0;

					float4 c = tex2Dlod(_ColorTex,float4(v.vertex.x,v.vertex.y,0,0));
					float4 d = tex2Dlod(_DepthTex,float4(v.vertex.x,v.vertex.y,0,0));
					int dr = d.r*255;
					int dg = d.g*255;
					int db = d.b*255;
					int da = d.a*255;
					int dValue = (int)(db | (dg << 0x8) | (dr << 0x10) | (da << 0x18));
					float4 pos;
					//if(c.a == 0)
					//dValue = 2000;
					pos.z = dValue / 1000.0;
					int x = 512*v.vertex.x;
					int y = 424*v.vertex.y;
					float vertx = float(x);
					float verty = float(424-y);
					pos.x =  pos.z * (vertx - 255.5) / 351.001462;
					pos.y =  pos.z * (verty - 211.5) / 351.001462;
					pos.w = 1;	

					if(dValue == 0)		
						c.a = 0; 

						//erasing head
				

					float4 worldPos = mul(unity_ObjectToWorld, pos);
					

					if (distance(worldPos, float4(_VRHeadPos, 1.0f)) < _HeadSize)
						c.a = 0;


					float inc = 0.10f;
					float4 A;
					float4 B;
					int i;
					float4 p;

					// RIGHT ARM
					if (_RightPointing == 1)
					{
						// shoulder -> elbow
						A = float4(_RightShoulder, 1.0);
						B = float4(_RightElbow, 1.0);
						for (i = 1; i <= (distance(A, B) / inc); i++)
						{
							p = A + normalize(B - A) * i * inc;
							if (distance(worldPos, p) < _RightDistance)
							{
								pos = mul(unity_WorldToObject, mul(_RightTransform, worldPos));
								//c.r = 1; c.g = 0; c.b = 0;
								break;
							}
						}

						// elbow -> wrist
						A = float4(_RightElbow, 1.0);
						B = float4(_RightWrist, 1.0);
						for (i = 1; i <= (distance(A, B) / inc); i++)
						{
							p = A + normalize(B - A) * i * inc;
							if (distance(worldPos, p) < _RightDistance)
							{
								pos = mul(unity_WorldToObject, mul(_RightTransform, worldPos));
								//c.r = 1; c.g = 0; c.b = 0;
								break;
							}
						}

						// handz and tipz
						if (distance(worldPos, float4(_RightHand, 1.0)) < _RightDistance
							|| distance(worldPos, float4(_RightHandTip, 1.0)) < _RightDistance)
						{
							pos = mul(unity_WorldToObject, mul(_RightTransform, worldPos));
							//c.r = 0; c.g = 1; c.b = 0;
						}
					}

					// LEFT ARM
					if (_LeftPointing == 1)
					{
						// shoulder -> elbow
						A = float4(_LeftShoulder, 1.0);
						B = float4(_LeftElbow, 1.0);
						for (i = 1; i <= (distance(A, B) / inc); i++)
						{
							p = A + normalize(B - A) * i * inc;
							if (distance(worldPos, p) < _LeftDistance)
							{
								pos = mul(unity_WorldToObject, mul(_LeftTransform, worldPos));
								//c.r = 1; c.g = 0; c.b = 0;
								break;
							}
						}

						// elbow -> wrist
						A = float4(_LeftElbow, 1.0);
						B = float4(_LeftWrist, 1.0);
						for (i = 1; i <= (distance(A, B) / inc); i++)
						{
							p = A + normalize(B - A) * i * inc;
							if (distance(worldPos, p) < _LeftDistance)
							{
								pos = mul(unity_WorldToObject, mul(_LeftTransform, worldPos));
								//c.r = 1; c.g = 0; c.b = 0;
								break;
							}
						}

						// handz and tipz
						if (distance(worldPos, float4(_LeftHand, 1.0)) < _LeftDistance
							|| distance(worldPos, float4(_LeftHandTip, 1.0)) < _LeftDistance)
						{
							pos = mul(unity_WorldToObject, mul(_LeftTransform, worldPos));
							//c.r = 0; c.g = 1; c.b = 0;
						}
					}


					



					output.pos = pos;
					output.color = c;

					return output;
				}


			[maxvertexcount(3)]
			void geom(triangle v2f input[3], inout TriangleStream<v2f> OutputStream)
			{


				float lod = 0; // your lod level ranging from 0 to number of mipmap levels.
				float c0 = input[0].color.a;
				float c1 = input[1].color.a;
				float c2 = input[2].color.a;

				if (distance(input[0].pos, input[1].pos) < _ShaderDistance & distance(input[0].pos, input[2].pos) < _ShaderDistance & distance(input[1].pos, input[2].pos) < _ShaderDistance
					& c0 != 0 & c1 != 0 & c2 != 0)
				{
					v2f outV;
					outV.pos = UnityObjectToClipPos(input[0].pos);
					outV.color = input[0].color;
					OutputStream.Append(outV);
					outV.pos = UnityObjectToClipPos(input[1].pos);
					outV.color = input[1].color;
					OutputStream.Append(outV);
					outV.pos = UnityObjectToClipPos(input[2].pos);
					outV.color = input[2].color;
					OutputStream.Append(outV);	
				}
			}

			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = i.color;
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}

	
			
	

			ENDCG
		}
	} 
}
