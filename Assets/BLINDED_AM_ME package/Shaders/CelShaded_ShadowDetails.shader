
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

// It only looks good if it is affected by only one light source.
// this is uses lighting model -> https://docs.unity3d.com/Manual/SL-SurfaceShaderLightingExamples.html

Shader "Custom/CelShaded_ShadowDetails" {
	Properties {

		_OutlineColor ("Outline Color", Color) = (0,0,0,1)
		_Outline ("Outline width", Range (.002, 0.03)) = .005

		_Color ("Main Color", Color) = (0.5,0.5,0.5,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Ramp ("Toon Ramp (RGB)", 2D) = "gray" {} 

		_ShadowDetailsColor ("Shadow Details Color", Color) = (1,1,1,1)
		_ShadowDetailsMap ("Shadow Details Map(A)", 2D) = "black" {}
		

	}


	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf ToonRamp fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		struct SurfaceOutputCustom // turns out you can do this
		{
		    fixed3 Albedo;
		    fixed3 Normal;
		    fixed3 Emission;
		    half Specular;
		    fixed Gloss;
		    fixed Alpha;

		    half ShadowDetails; // <- new guy
		};

		struct Input {
			float2 uv_MainTex;
			float2 uv_ShadowDetailsMap;
		};


		sampler2D _MainTex;
		sampler2D _Ramp;
		sampler2D _ShadowDetailsMap;

		float4 _ShadowDetailsColor;
		float4 _Color;

		// in order

		// 1st
		void surf (Input IN, inout SurfaceOutputCustom o) {

			o.ShadowDetails = tex2D(_ShadowDetailsMap, IN.uv_ShadowDetailsMap).a;  // for 2nd

			half4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}

		// 2nd
		// custom lighting function that uses a texture ramp based
		// on angle between light direction and normal
		half4 LightingToonRamp (SurfaceOutputCustom o, half3 lightDir, half atten) // light attenuation
		{
			lightDir = normalize(lightDir);

			half d = dot (o.Normal, lightDir)*0.5 + 0.5;
			half3 ramp = tex2D (_Ramp, float2(d,0.5f)).rgb;
			half lumens = ramp * atten; // brightness
		
			half invert = max(0,sign(lumens)); // add emission to the darkside
			invert = 1-invert;              // lives up to the name

			half4 c;
			c.rgb = o.Albedo * _LightColor0.rgb * lumens;
			c.rgb += _ShadowDetailsColor.rgb * o.ShadowDetails * invert; // add it
			c.a = 0;

			return c;
		}


		ENDCG

		// for the outline
		UsePass "Custom/Diffuse_Outline_Edges/OUTLINE_PASS"

	} 
	
	Fallback "Diffuse"
}