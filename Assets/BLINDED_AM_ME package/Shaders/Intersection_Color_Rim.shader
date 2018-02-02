
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


Shader "Custom/Unlit/Intersection_Color_Rim"
{
	Properties
	{

		_MainColor("Main Color", Color) = (1, 1, 1, 0.25)
		_IntersectionColor("Intersection Color", Color) = (1, 1, 1, 1) 
        _IntersectionMax("Intersection Max", Range(0.01,5)) = 1 
        _IntersectionDamper("Intersection Damper", Range(0,1)) = 0.0

        // rim values
        _RimColor ("Rim Color", Color) = (1,1,1,1)
	    _RimPower ("Rim Power", Range(0.5,8.0)) = 3.0

	}
	SubShader
	{
		Tags { "Queue"="Transparent" "RenderType"="Transparent"  }
        LOD 100

        UsePass "Custom/Unlit/Intersection_Color/PASS"

		UsePass "Custom/Diffuse_UnlitRimColor/UNLIT_RIM_COLOR"
	}

	FallBack "Unlit/Color"
}


