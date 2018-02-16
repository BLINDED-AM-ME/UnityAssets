
// this just makes the uv = screen position for the _PortalTex

Shader "Unlit/Portal"
{
	Properties
	{
		_MainTex ("Main Texture (RGB)", 2D) = "white" {}
		_PortalTex ("Portal TargetTexture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100
 
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4    _MainTex_ST;

			sampler2D _PortalTex;
			float4    _PortalTex_ST;

			struct appdata
			{
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 screenPos : TEXCOORD2;
				float4 pos : SV_POSITION;
			};	

			v2f vert(appdata v)
			{
				v2f o;
				o.uv  = TRANSFORM_TEX(v.uv, _MainTex);
				o.pos = UnityObjectToClipPos (v.pos);
				o.screenPos = ComputeScreenPos (o.pos);

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return tex2D(_MainTex, i.uv) * tex2Dproj(_PortalTex, UNITY_PROJ_COORD(i.screenPos));
			}
			ENDCG
	    }
	}
}
