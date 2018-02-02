
//    MIT License
//    
//    Copyright (c) 2017 Dustin Whirle
//    
//    My Youtube stuff: https://www.youtube.com/playlist?list=PL-sp8pM7xzbVls1NovXqwgfBQiwhTA_Ya
//    
//    Permission is hereby granted, free of charge, to any person obtaining a copy
//    of this software and associated documentation files (the "Software"), to deal
//    in the Software without restriction, including without limitation the rights
//    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//    copies of the Software, and to permit persons to whom the Software is
//    furnished to do so, subject to the following conditions:
//    
//    The above copyright notice and this permission notice shall be included in all
//    copies or substantial portions of the Software.
//    
//    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//    SOFTWARE.

// this is so I can later call -> UsePass "Custom/Diffuse_Outline_Edges/OUTLINE_PASS"

Shader "Custom/Diffuse_Outline_Edges" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0

		_OutlineColor ("Outline Color", Color) = (0,0,0,1)
		_Outline ("Outline width", Range (.002, 0.03)) = .005
	}

	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows
		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG

		// Outline for other shaders
		// called with -> UsePass "Custom/Diffuse_Outline_Edges/OUTLINE_PASS"
		Pass {
			Name "OUTLINE_PASS"
			Tags { "LightMode" = "Always" } // LightMode Always: Always rendered; no lighting is applied.


//			Cull Back  // Don’t render polygons facing away from the viewer (default).
			Cull Front // Don’t render polygons facing towards the viewer. Used for turning objects inside-out.
//			Cull Off   // Disables culling - all faces are drawn. Used for special effects.

//			Controls whether pixels from this object are written to the depth buffer (default is On). 
			ZWrite On

//			How should depth testing be performed. 
//			Default is LEqual (draw objects in from or at the distance as existing objects; hide objects behind them).
//			Less | Greater | LEqual | GEqual | Equal | NotEqual | Always
//			ZTest Always 

//			ColorMask RGB

//			this will determine how the passes blend
			Blend SrcAlpha OneMinusSrcAlpha // Normal
//			Blend One One // Additive
//			Blend One OneMinusDstColor // Soft Additive
//			Blend DstColor Zero // Multiplicative
//			Blend DstColor SrcColor // 2x Multiplicative


			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct v2f {
				float4 pos : SV_POSITION;
				UNITY_FOG_COORDS(0)
				fixed4 color : COLOR;
			};
			
			float _Outline;
			half4 _OutlineColor;
			
			v2f vert(appdata v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);

				float3 norm = normalize(mul ((float3x3)UNITY_MATRIX_IT_MV, v.normal));
				float2 offset = TransformViewToProjection(norm.xy);

				o.pos.xy += offset * o.pos.z * _Outline;
				o.color = _OutlineColor;
				UNITY_TRANSFER_FOG(o,o.pos);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				UNITY_APPLY_FOG(i.fogCoord, i.color);
				return i.color;
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}
