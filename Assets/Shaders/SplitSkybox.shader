Shader "Custom/SplitSkybox"
{
    Properties
    {
        _Cubemap ("Cubemap", CUBE) = "" {}
        _Rotation ("Rotation Angle", Range(0,360)) = 0
        _RotationAxis ("Rotation Axis", Vector) = (0,1,0,0)
        _SectionCenter ("Section Center", Vector) = (0,0,0,0)
        _SectionRadius ("Section Radius", Float) = 0.5
    }
    SubShader
    {
        Tags { "Queue"="Background" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float3 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float3 texcoord : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            samplerCUBE _Cubemap;
            float _Rotation;
            float3 _RotationAxis;
            float3 _SectionCenter;
            float _SectionRadius;

            // Fonction de rotation autour d'un axe arbitraire
            float3 RotatePoint(float3 p, float3 axis, float angle)
            {
                float s = sin(angle);
                float c = cos(angle);
                float oneMinusC = 1.0 - c;

                float3x3 rotationMatrix =
                {
                    c + axis.x * axis.x * oneMinusC,       axis.x * axis.y * oneMinusC - axis.z * s, axis.x * axis.z * oneMinusC + axis.y * s,
                    axis.y * axis.x * oneMinusC + axis.z * s, c + axis.y * axis.y * oneMinusC,       axis.y * axis.z * oneMinusC - axis.x * s,
                    axis.z * axis.x * oneMinusC - axis.y * s, axis.z * axis.y * oneMinusC + axis.x * s, c + axis.z * axis.z * oneMinusC
                };

                return mul(rotationMatrix, p);
            }

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;

                // Vérifier si le point est dans la section à tourner
                if (distance(v.texcoord, _SectionCenter) < _SectionRadius)
                {
                    float angleRad = radians(_Rotation);
                    o.texcoord = RotatePoint(o.texcoord, _RotationAxis, angleRad);
                }

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return texCUBE(_Cubemap, i.texcoord);
            }
            ENDCG
        }
    }
}
