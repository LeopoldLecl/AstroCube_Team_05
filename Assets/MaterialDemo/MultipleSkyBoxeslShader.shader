Shader "My/PortalShader"
{
    Properties
    {
        //0 - Main world (grab texture)
        //1 - Mix worlds
        //2 - Another world (_AnotherWorldTexture)
        _PortalFactor("Portal Factor", Range(-1.0, 1.0)) = 0
        [Toggle]_PortalOpening("Portal Opening", Float) = 1
        
        _LightingBorderSize("Lighting Border Size", Range(0, 0.1)) = 0.03
        _WorldColorFactor("World Color Factor", Range(0, 1)) = 0.7
        
        _AnotherWorldTexture("Another World Texture", 2D) = "white" {}
        _AnotherWorldPortalColor("Another World Portal Color", 2D) = "white" {}
        
        _SurfaceDistortion("Surface Distortion", 2D) = "white" {}
        _DistortionScale("Distortion Scale", Range(0.1, 3)) = 1
        _DistortionSize("Distortion Size", Range(0, 5)) = 3
        _DistortionSpeed("Distortion Speed", Range(0.01, 0.3)) = 0.05
        
        _PortalColors("Portal Colors", 2D) = "white" {}
        _PortalColorsWidthScale("Portal Colors Wid Scale", Float) = 0.0625
        
        _SurfaceNoise("Surface Noise", 2D) = "white" {}
        _NoiseSpeed("Noise Speed", Range(0.001, 0.03)) = 0.005
        _NoiseScale("Noise Scale", Range(0, 0.5)) = 0.1
        
        _PortalFrame("Portal Frame", 2D) = "black" {}
        _PortalFrameDistortionFactor("Frame Distortion factor", Range(1,20)) = 5
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" }
        GrabPass { "_BackgroundTexture" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            //Parameters
            float _PortalFactor;
            float _PortalOpening;
            float _LightingBorderSize;
            float _WorldColorFactor;
            float _PortalColorsWidthScale;
            float _DistortionScale;
            float _DistortionSize;
            float _DistortionSpeed;
            float _NoiseSpeed;
            float _NoiseScale;
            float _PortalFrameDistortionFactor;

            //Textures
            sampler2D _AnotherWorldTexture;
            sampler2D _AnotherWorldPortalColor;
            sampler2D _SurfaceDistortion;
            sampler2D _PortalColors;
            sampler2D _SurfaceNoise;
            sampler2D _PortalFrame;
            sampler2D _BackgroundTexture;
            
            //For TRANSFORM_TEX
            float4 _PortalFrame_ST;

            struct v2f
            {
                float4 grabPos : TEXCOORD0;
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD1;
                float2 noiseuv : TEXCOORD2;
                float2 distortuv : TEXCOORD3;
                float2 frameUV: TEXCOORD4;
            };
            
            struct appdata
            {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
            };

            v2f vert(appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.grabPos = ComputeGrabScreenPos(o.pos);
                o.uv = v.uv;
                o.distortuv = v.uv * _DistortionScale;
                o.frameUV = TRANSFORM_TEX(v.uv, _PortalFrame);
                
                o.noiseuv = v.uv * _NoiseScale;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                float2 offset1 = float2(_Time.y * _DistortionSpeed, _Time.y * _DistortionSpeed);
                float2 offset2 = -float2(
                    _Time.y * _DistortionSpeed * 1.1, 
                    _Time.y * _DistortionSpeed * 0.9) + float2(0.5, 0.5);
                half4 distortionValue1 = (tex2D(_SurfaceDistortion, i.distortuv + offset1) * 2 - 1);
                half4 distortionValue2 = (tex2D(_SurfaceDistortion, i.distortuv + offset2) * 2 - 1);

                half4 distortionValue = (distortionValue1 + distortionValue2) * 0.5;
                half4 distortionNoise = distortionValue * _DistortionSize;

                //Calculate color texture UV
                float2 colorUV1 = (i.uv * float2(_PortalColorsWidthScale, 1)) + float2(_Time.y * 0.015, 1);
                float2 colorUV2 = (i.uv * float2(_PortalColorsWidthScale, 1)) + float2(_Time.y * 0.015, 1);
                
                //Portal color
                float2 colorNoise = float2(distortionValue.x, distortionValue.y) * 0.03;
                half4 color1 = tex2D(_PortalColors, colorUV1 + colorNoise);
                half4 color2 = tex2D(_PortalColors, colorUV2 + colorNoise);
                half4 color = (color1 + color2) * 0.5;
                
                //Another world portal color
                half4 anotherColor1 = tex2D(_AnotherWorldPortalColor, colorUV1 + colorNoise);
                half4 anotherColor2 = tex2D(_AnotherWorldPortalColor, colorUV2 + colorNoise);
                half4 anotherColor = (anotherColor1 + anotherColor2) * 0.5;
        
                //Sample noise texture
                float2 noiseOffset1 = float2(_Time.y * _NoiseSpeed, _Time.y * _NoiseSpeed);
                float2 noiseOffset2 = -float2(
                   _Time.y * _NoiseSpeed * 1.1, _Time.y * _NoiseSpeed * 0.9) + float2(0.5, 0.5);
                half4 noiseValue1 = tex2D(_SurfaceNoise, i.noiseuv + noiseOffset1);
                half4 noiseValue2 = tex2D(_SurfaceNoise, i.noiseuv + noiseOffset2);
                half4 noiseValuePow = pow(noiseValue1 * noiseValue2, 2);
                half4 noiseValueSum = noiseValue1 + noiseValue2 - 1;

                //Another world texture
                half4 anotherWorldColor = tex2Dproj(
                    _AnotherWorldTexture, 
                    i.grabPos + half4(distortionNoise.x, distortionNoise.y, 0, 0));

                //Portal world border size calculation.
                half worldBorder = 0.0;
                half worldBorderSize = _LightingBorderSize;
                half worldBorderMin = worldBorder - worldBorderSize * 0.5;
                half worldBorderMax = worldBorder + worldBorderSize * 0.5;
                
                //Portal frame
                half4 portalFrame = tex2D(
                    _PortalFrame, i.frameUV + colorNoise * _PortalFrameDistortionFactor);
                half portalBorderFactor = portalFrame.x * (1 - abs(_PortalFactor));
                
                half worldFactor = noiseValueSum.x + _PortalFactor;
                if(_PortalOpening > 0.5)
                    worldFactor += portalBorderFactor;
                else
                    worldFactor -= portalBorderFactor;
                
                //Add portal border lighting
                half4 noiseLighting;
                if(worldFactor > worldBorderMin && worldFactor < worldBorderMax)
                    noiseLighting = half4(1,1,1,0);
                else
                    noiseLighting = half4(0,0,0,1);
                
                //Mix world colors
                half4 portalWorldColor;
                half4 portalColor;
                if(worldFactor < worldBorderMax)
                {
                    portalWorldColor = tex2Dproj(
                        _BackgroundTexture, 
                        i.grabPos + half4(distortionNoise.x, distortionNoise.y, 0, 0));
                    portalColor = color;
                }
                else
                {
                    portalWorldColor = tex2Dproj(
                        _AnotherWorldTexture, 
                        i.grabPos + half4(distortionNoise.x, distortionNoise.y, 0, 0));
                    portalColor = anotherColor;
                }
                
                //Calculate result color
                half bgColorFactor = clamp(_WorldColorFactor, 0, 1);
                
                return 
                    portalWorldColor * bgColorFactor + 
                    portalColor * (1 - bgColorFactor) + 
                    noiseLighting;
            }
            ENDCG
        }

    }
}