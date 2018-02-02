
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

// clips any pixel in an area defined by size, rotation, and position

Shader "Clip/Box" {
	Properties {
    
      _BoxSize ("Box Size (World Space)", Vector) = (1,1,1,1)
      _BoxRotation("Box Rotation (Euler xyz)", Vector) = (0,0,0,1)
      _Origin ("Origin (World Space)", Vector) = (0,0,0,0)
      _ConvertEmission ("Convert Emission", Range(0,1)) = 0.5
      _ConvertDistance ("Conversion Distance", float) = 0.1
      _Conversion ("Conversion (RGB)", 2D) = "white" {}
    
      _MainTex ("Main Texture", 2D) = "white" {}
      _BumpMap ("Bumpmap", 2D) = "bump" {}
      _Glossiness ("Smoothness", Range(0,1)) = 0.5
	  _Metallic ("Metallic", Range(0,1)) = 0.0
	  
    }
    SubShader {
      Tags { "RenderType" = "Opaque" }
      Cull Off
      CGPROGRAM
      // Physically based Standard lighting model, and enable shadows on all light types
	  #pragma surface surf Standard fullforwardshadows

	  // Use shader model 3.0 target, to get nicer looking lighting
	  #pragma target 3.0
		
      struct Input {
          float2 uv_MainTex;
          float2 uv_Conversion;
          float2 uv_BumpMap;
          float3 worldPos;
      };
      
      half _Glossiness;
	  half _Metallic;
	  half _ConvertDistance;
	  half _ConvertEmission;
      
      float3 _Origin;
      float3 _BoxSize;
      float3 _BoxRotation;
      
      sampler2D _MainTex;
      sampler2D _BumpMap;
      sampler2D _Conversion;
      
      void surf (Input IN, inout SurfaceOutputStandard o) {

	      // Rodrigues' rotation formula uses radians
		  // Vector3 newDir = Mathf.Cos(angle) * dir + Mathf.Sin(angle) * Vector3.Cross(axis, dir) + (1.0f - Mathf.Cos(angle)) * Vector3.Dot(axis,dir) * axis;

	  	  float3 dir = IN.worldPos - _Origin;
	  	  //dir = normalize(dir);
	  	  float3 rads = float3(radians(_BoxRotation.x), radians(_BoxRotation.y), radians(_BoxRotation.z));

		  // rotation euler order is z, x, then y like roll, pitch, and yaw
	  	  // z
	  	  dir = cos(rads.z) * dir + sin(rads.z) * cross(float3(0,0,1.0f), dir) + (1.0f - cos(rads.z)) * dot(float3(0,0,1.0f), dir) * float3(0,0,1.0f);
		  // x
		  dir = cos(rads.x) * dir + sin(rads.x) * cross(float3(1.0f,0,0), dir) + (1.0f - cos(rads.x)) * dot(float3(1.0f,0,0), dir) * float3(1.0f,0,0);
		  // y
		  dir = cos(rads.y) * dir + sin(rads.y) * cross(float3(0,1.0f,0), dir) + (1.0f - cos(rads.y)) * dot(float3(0,1.0f,0), dir) * float3(0,1.0f,0);
		  

      	  half3 dist = half3(
	      	  abs(dir.x), // no negatives
	      	  abs(dir.y), // no negatives
	      	  abs(dir.z)  // no negatives
      	  );

          // not good to use if statements in shaders, not sure about this EDIT: don't use this either
//        clip( 
//	      	(dist.x - _BoxSize.x * 0.5 < 0) &&
//	      	(dist.y - _BoxSize.y * 0.5 < 0) &&
//	      	(dist.z - _BoxSize.z * 0.5 < 0)
//        ? -1:1);
 		  // replaced

		  dist.x = dist.x - _BoxSize.x * 0.5;
		  dist.y = dist.y - _BoxSize.y * 0.5;
		  dist.z = dist.z - _BoxSize.z * 0.5;

      	  // all need to be less than size
      	  half t = min(1, dist.x); // if greater than zero don't clip
      	  t = max(t, dist.y);
      	  t = max(t, dist.z);

      	  clip(t); 
          
          // min = 0 // value = dist // max = _ConvertDistance
          float convert_mask = max(dist.x,max(dist.y, dist.z)) / _ConvertDistance;
		  convert_mask = clamp(convert_mask, 0, 1);
          
          fixed4 albedo = tex2D (_MainTex, IN.uv_MainTex);
          albedo *= convert_mask;
          
          fixed4 convert = tex2D (_Conversion, IN.uv_Conversion);
          convert *= 1.0 - convert_mask;
          
          o.Albedo = albedo.rgb + convert.rgb;
          o.Emission = convert.rgb * _ConvertEmission;
          o.Normal = UnpackNormal (tex2D (_BumpMap, IN.uv_BumpMap));
          o.Metallic = _Metallic;
		  o.Smoothness = _Glossiness;
		  o.Alpha = albedo.a + convert.a;
          
      }
      ENDCG
    } 
    Fallback "Diffuse"
  }