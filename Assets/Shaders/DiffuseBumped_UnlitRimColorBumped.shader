
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

Shader "Custom/DiffuseBumped_UnlitRimColorBumped"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}

		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0

		_RimColor ("Rim Color", Color) = (1,1,1,1)
	    _RimPower ("Rim Power", Range(0.5,8.0)) = 3.0

	    _BumpMap("Normal Map", 2D) = "bump" {}
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
		sampler2D _BumpMap;

		struct Input {
			float2 uv_MainTex;
			float2 uv_BumpMap;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Normal = UnpackNormal (tex2D (_BumpMap, IN.uv_BumpMap));
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG


		// unlit rim color for use in other shaders
		// called with -> UsePass "Custom/Diffuse_UnlitRimColorBumped/UNLIT_RIM_COLOR_BUMPED"
		Pass { 
			Name "UNLIT_RIM_COLOR_BUMPED"
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

			half4 _RimColor;
			half  _RimPower;

            sampler2D _BumpMap;
            float4    _BumpMap_ST;

			struct appdata {
				float2 uv : TEXCOORD0;
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				half4  tangent : TANGENT;
			};
			
			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;

				float3 worldViewDir : TEXCOORD1; 

				half3 tspace0 : TEXCOORD2; // tangent.x, bitangent.x, normal.x
                half3 tspace1 : TEXCOORD3; // tangent.y, bitangent.y, normal.y
                half3 tspace2 : TEXCOORD4; // tangent.z, bitangent.z, normal.z

                // fog
				UNITY_FOG_COORDS(5)
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


		        // bump
				o.uv = TRANSFORM_TEX(v.uv, _BumpMap);

				half3 worldNormal = UnityObjectToWorldNormal(v.normal);
                half3 wTangent = UnityObjectToWorldDir(v.tangent.xyz);
                // compute bitangent from cross product of normal and tangent
                half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
                half3 wBitangent = cross(worldNormal, wTangent) * tangentSign;
                // output the tangent space matrix
                o.tspace0 = half3(wTangent.x, wBitangent.x, worldNormal.x);
                o.tspace1 = half3(wTangent.y, wBitangent.y, worldNormal.y);
                o.tspace2 = half3(wTangent.z, wBitangent.z, worldNormal.z);

				return o;

			}

			fixed4 frag(v2f i) : SV_Target
			{

				// sample the normal map, and decode from the Unity encoding
                half3 tnormal = UnpackNormal(tex2D(_BumpMap, i.uv));

                // transform normal from tangent to world space
                half3 worldNormal;
                worldNormal.x = dot(i.tspace0, tnormal);
                worldNormal.y = dot(i.tspace1, tnormal);
                worldNormal.z = dot(i.tspace2, tnormal);

				half rim = 1.0 - saturate(dot (i.worldViewDir, worldNormal));

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
