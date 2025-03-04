Shader "Custom/PortalShader"
{
    Properties
    {
        _MainTex ("360° Texture", 2D) = "white" {}
        _PortalMask ("Portal Mask", Range(0,1)) = 0.5
        _DepthFactor ("Depth Factor", Range(0.1, 10)) = 1.0
        _DistortionStrength ("Distortion Strength", Range(0, 1)) = 0.1
        _PortalEdgeSoftness ("Portal Edge Softness", Range(0, 1)) = 0.5
        _ColorTint ("Color Tint", Color) = (1,1,1,1)
        _GlowIntensity ("Glow Intensity", Range(0, 5)) = 1.0
        _NoiseTexture ("Noise Texture", 2D) = "white" {}
        _NoiseStrength ("Noise Strength", Range(0, 1)) = 0.2
        _RotationSpeed ("Rotation Speed", Range(-5, 5)) = 0.0
        _BackgroundFade ("Background Fade", Range(0, 1)) = 0.3
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata_t
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 viewDir : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float2 uv : TEXCOORD3;
            };
            
            sampler2D _MainTex;
            sampler2D _NoiseTexture;
            float4 _MainTex_ST;
            float _PortalMask;
            float _DepthFactor;
            float _DistortionStrength;
            float _PortalEdgeSoftness;
            float4 _ColorTint;
            float _GlowIntensity;
            float _NoiseStrength;
            float _RotationSpeed;
            float _BackgroundFade;
            
            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.viewDir = normalize(_WorldSpaceCameraPos - o.worldPos) * _DepthFactor;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Noise distortion
                float2 noiseOffset = tex2D(_NoiseTexture, i.uv + float2(_Time.y * _RotationSpeed, _Time.y * _RotationSpeed)).rg;
                float2 distortion = (noiseOffset - 0.5) * _NoiseStrength;
                float3 dir = normalize(i.viewDir + float3(distortion, 0));
                
                // Correct the direction for full 360° mapping
                float3 correctedDir = normalize(reflect(i.viewDir, i.normalDir));
                float2 sphericalUV = float2(0.5 + atan2(correctedDir.z, correctedDir.x) / (2 * 3.14159), 0.5 - asin(correctedDir.y) / 3.14159);
                fixed4 col = tex2D(_MainTex, sphericalUV) * _ColorTint;
                
                // Portal mask effect with edge softness
                float portalEffect = smoothstep(0.4, _PortalMask, dot(normalize(i.viewDir), i.normalDir));
                portalEffect = smoothstep(0.0, _PortalEdgeSoftness, portalEffect);
                
                // Background fade effect
                col.rgb = lerp(col.rgb, float3(0, 0, 0), _BackgroundFade);
                
                return lerp(fixed4(0,0,0,0), col, portalEffect);
            }
            
            ENDCG
        }
    }
}
