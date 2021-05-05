Shader "Custom/Sight"
{
	Properties//Variables
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_Intensity("Fade Amount", Range(0.0, 1.0)) = 0.0
		_Color("Fade to Color", Color) = (1, 1, 1, 1)
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		Lighting Off

		Pass
		{
			ZTest Always Cull Off ZWrite Off
			Fog{ Mode off }

			CGPROGRAM//Allows talk between two languages: shader lab and nvidia C for graphics.

			//\===========================================================================================
			//\ Function Defines - defines the name for the vertex and fragment functions
			//\===========================================================================================

			#pragma vertex vert_img
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest

			//\===========================================================================================
			//\ Includes
			//\===========================================================================================

			#include "UnityCG.cginc"//Built in shader functions.

			//\===========================================================================================
			//\ Structures - Can get data like - vertices's, normal, color, uv.
			//\===========================================================================================

			//\===========================================================================================
			//\ Imports - Re-import property from shader lab to nvidia cg
			//\===========================================================================================

			uniform sampler2D _MainTex;
			uniform float _Intensity;
			uniform fixed4 _Color;

			//\===========================================================================================
			//\ Vertex Function - Builds the object
			//\===========================================================================================

			//\===========================================================================================
			//\ Fragment Function - Color it in
			//\===========================================================================================

			fixed4 frag(v2f_img i) :COLOR
			{
				fixed4 texColor = tex2D(_MainTex, i.uv);
				return lerp(texColor, _Color, _Intensity);
			}

			ENDCG
		}
	}
}
