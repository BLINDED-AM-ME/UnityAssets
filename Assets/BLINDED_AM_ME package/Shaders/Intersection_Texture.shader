
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
// https://chrismflynn.wordpress.com/2012/09/06/fun-with-shaders-and-the-depth-buffer/

Shader "Custom/Unlit/Intersection_Texture"
{
	Properties
    {
     
        _IntersectionMax("Intersection Max", Range(0.01,5)) = 1 
        _IntersectionDamper("Intersection Damper", Range(0,1)) = 0.0

        _MainColor("Main Color", Color) = (1, 1, 1, 0.25)
	    _MainTex ("Texture", 2D) = "white" {}
	    _IntersectionColor("Intersection Color", Color) = (1, 1, 1, 1)
	    _IntersectionTex ("Intersection Texture", 2D) = "white" {}
	    
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType"="Transparent"  }
        LOD 100

        // for use in other shaders
		// called with -> UsePass "Custom/Unlit/Intersection_Texture/PASS"
        Pass
        {
        	name "PASS"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
//          Cull Off //if you want to see back faces
 
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
			#pragma multi_compile_fog

            #include "UnityCG.cginc"
 
            sampler2D _CameraDepthTexture;

            half4 _MainColor;
            half4 _IntersectionColor;

            half _IntersectionMax;
            half _IntersectionDamper;

            sampler2D _MainTex;
			float4 _MainTex_ST;

			sampler2D _IntersectionTex;
			float4 _IntersectionTex_ST;

            struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

            struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 screenPos : TEXCOORD1;
				float4 vertex : SV_POSITION;

				// fog
				UNITY_FOG_COORDS(2)

				float2 uv2 : TEXCOORD3;

			};

            v2f vert (appdata v)
			{

				v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv2 = TRANSFORM_TEX(v.uv, _IntersectionTex);
				
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


				fixed4 col    = tex2D(_MainTex, i.uv);
				fixed4 hColor = tex2D(_IntersectionTex, i.uv2);

				highlight_mask *= hColor.a * _IntersectionColor.a;

				col *= _MainColor * (1-highlight_mask);
				col += hColor * _IntersectionColor * highlight_mask;

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
