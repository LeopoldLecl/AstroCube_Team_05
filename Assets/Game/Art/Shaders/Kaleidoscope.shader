﻿ Shader "UltraEffects/Kaleidoscope"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_SegmentCount("Segment Count", Float) = 4
        _Alpha ("Opacity", Float) = 1.0 // New property for opacity control
        _TEST_VAR1 ("Test Variable 1", Float) = -0.0153
        _TEST_VAR2 ("Test Variable 2", Float) = 1.44
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha // Enable transparency blending

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float _TEST_VAR1;
            float _TEST_VAR2;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                // o.uv = (v.uv-0.20f);
                // o.uv.x = o.uv.x*-1; 
                o.uv = (v.uv + _TEST_VAR1);
                o.uv = o.uv * _TEST_VAR2;
                return o;
            }

            sampler2D _MainTex;
			float _SegmentCount;
            float _Alpha; // New variable for opacity

            float4 frag (v2f i) : SV_Target
            {
				// Convert to polar coordinates.
				float2 shiftUV = i.uv - 0.5;
				float radius = sqrt(dot(shiftUV, shiftUV));
				float angle = atan2(shiftUV.y, shiftUV.x);

				// Calculate segment angle amount.
				float segmentAngle = UNITY_TWO_PI / _SegmentCount;

				// Calculate which segment this angle is in.
				angle -= segmentAngle * floor(angle / segmentAngle);

				// Each segment contains one reflection.
				angle = min(angle, segmentAngle - angle);

				// Convert back to UV coordinates.
				float2 uv = float2(cos(angle), sin(angle)) * radius + 0.3f;

				// Reflect outside the inner circle boundary.
				uv = max(min(uv, 2.0 - uv), -uv);

                // Sample the texture and apply opacity
                float4 color = tex2D(_MainTex, uv);
                color.a *= _Alpha; // Apply the opacity multiplier

				return color;
            }
            ENDCG
        }
    }
}
