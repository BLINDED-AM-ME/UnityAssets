
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


// high - For world space positions and texture coordinates, use float precision.
// med  - For everything else (vectors, HDR colors, etc.), start with half precision. Increase only if necessary.
// low  - For very simple operations on texture data, use fixed precision.

// References
// https://docs.unity3d.com/Manual/SL-VertexFragmentShaderExamples.html
// https://docs.unity3d.com/Manual/SL-SurfaceShaderExamples.html

Shader "Custom/Diffuse_UnlitRimColor"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}

		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0

		_RimColor ("Rim Color", Color) = (1,1,1,1)
	    _RimPower ("Rim Power", Range(0.5,8.0)) = 3.0
	}


	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 200

		// this one is for the diffuse
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


		// unlit rim color for use in other shaders
		// called with -> UsePass "Custom/Diffuse_UnlitRimColor/UNLIT_RIM_COLOR"
		Pass { 
			Name "UNLIT_RIM_COLOR"
			Tags { "LightMode" = "Always" } // LightMode Always: Always rendered; no lighting is applied.

			// this will determine how the passes blend
			Blend SrcAlpha OneMinusSrcAlpha // Normal
			//Blend One One // Additive
			//Blend One OneMinusDstColor // Soft Additive
			//Blend DstColor Zero // Multiplicative
			//Blend DstColor SrcColor // 2x Multiplicative


			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog

			#include "UnityCG.cginc"

			// rim
			half4 _RimColor;
			half  _RimPower;

			struct appdata {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 worldNormal : TEXCOORD0; // rim
				float3 worldViewDir : TEXCOORD1; // rim

				// fog
				UNITY_FOG_COORDS(2)
			};


		    v2f vert (appdata v)
			{

				v2f o;
		        o.vertex = UnityObjectToClipPos(v.vertex);         
		        // fog
				UNITY_TRANSFER_FOG(o,o.vertex);

				// compute world space position of the vertex
		        float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
		         // compute world space view direction
		        o.worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
		        // world space normal
		        o.worldNormal = UnityObjectToWorldNormal(v.normal);

				return o;

			}

			fixed4 frag(v2f i) : SV_Target
			{
				half rim = 1.0 - saturate(dot (i.worldViewDir, i.worldNormal));

				rim = pow (rim, _RimPower);

				half4 col = half4(1,1,1,1);
				col -= col * rim;
	            col += _RimColor * rim;

	            col.a = rim * _RimColor.a;

				UNITY_APPLY_FOG(i.fogCoord, col);

				return col;
			}
			ENDCG
		}

	}
	FallBack "Diffuse" // <- why the **** does it need this for casting a shadow?
}
