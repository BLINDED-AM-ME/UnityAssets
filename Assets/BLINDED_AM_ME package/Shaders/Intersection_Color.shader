
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

// this only works with Direct3D9 and openGL
// for windows go to Edit -> Project Settings -> Player -> other Settings
// uncheck the auto graphics API, then move it to the top of the list
// you can add one or all the openGL options


Shader "Custom/Unlit/Intersection_Color"
{
	Properties
	{

		_MainColor("Main Color", Color) = (1, 1, 1, 0.25)
		_IntersectionColor("Intersection Color", Color) = (1, 1, 1, 1) 
        _IntersectionMax("Intersection Max", Range(0.01,5)) = 1
        _IntersectionDamper("Intersection Damper", Range(0,1)) = 0


	}
	SubShader
	{
		Tags { "Queue"="Transparent" "RenderType"="Transparent"  }
        LOD 100

        // for use in other shaders
		// called with -> UsePass "Custom/Unlit/Intersection_Color/PASS"
        Pass
        {
	        Name "PASS"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
//            Cull Off //if you want to see back faces
 
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
			#pragma multi_compile_fog
			#include "UnityCG.cginc"

			sampler2D _CameraDepthTexture; //<- built-in

            half4 _MainColor;
            half4 _IntersectionColor;
            half  _IntersectionMax;
            half  _IntersectionDamper;


             struct appdata
			{
				float4 vertex : POSITION;
			};

            struct v2f
			{
				float4 screenPos : TEXCOORD0;
				float4 vertex : SV_POSITION;
				
				// fog
				UNITY_FOG_COORDS(1)
			};

            v2f vert (appdata v)
			{

				v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);           

                // fog
				UNITY_TRANSFER_FOG(o,o.vertex);

				return o;
			}
 

            fixed4 frag (v2f i) : SV_Target
			{

                float bufferZ = LinearEyeDepth (tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos)).r);
                float pixelZ = i.screenPos.z;
				float intersection_dist = sqrt(pow(bufferZ - pixelZ,2));

				half highlight_mask = max(0, sign(_IntersectionMax - intersection_dist));
				highlight_mask *= 1 - intersection_dist / _IntersectionMax * _IntersectionDamper;

				fixed4 col = _MainColor;

				highlight_mask *= _IntersectionColor.a;

				col *=  (1-highlight_mask);
				col +=  _IntersectionColor * highlight_mask;

				col.a = max(highlight_mask, col.a);

				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}


			ENDCG
		}
	}

	FallBack "Unlit/Color"
}
