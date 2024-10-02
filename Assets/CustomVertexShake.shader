Shader "Unlit/TransparentCloth"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}

        [Header(Wave 1)]
        _WaveAmplitude1("Amplitude", Range(0, 1)) = 0.025
        _WaveSpeed1("Speed", Range(-30, 30)) = 9.8
        _WaveFreq1("Freq", Range(0, 30)) = 8.5

        [Header(Wave 2)]
        _WaveAmplitude2("Amplitude", Range(0, 1)) = 0.0045
        _WaveSpeed2("Speed", Range(-30, 30)) = 15.4
        _WaveFreq2("Freq", Range(0, 30)) = 25.3

        [Header(Drop)]
        [Toggle] _DROP_Y("Enable Drop Y", Float) = 0
        _DropY("Drop Y", Range(0  , 1)) = 1

        [Header(Lighting)]
        _LightDirection("Light Direction", Vector) = (1.0, 3.0, 2.0, 0.0)
        _ShadowIntensity("Shadow Intensity", Range(0.0, 1.0)) = 0.5

        [HDR] _TintColor("Tint Color", Color) = (1.0, 1.0, 1.0, 1.0)

        [Header(Culling)]
        [Enum(UnityEngine.Rendering.CullMode)] _CullMode("Cull Mode", Float) = 2
    }

    SubShader
    {
        Tags{
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "LightMode" = "ForwardBase"
            "IgnoreProjector" = "True"
        }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            Lighting Off
            Fog{ Mode Off }

            Cull [_CullMode]
            ZWrite Off
            ZTest LEqual

            CGPROGRAM
            #include "UnityCG.cginc"

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile __ _DROP_Y_ON

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _WaveAmplitude1;
            float _WaveSpeed1;
            float _WaveFreq1;

            float _WaveAmplitude2;
            float _WaveSpeed2;
            float _WaveFreq2;

            float _DropY;

            half4 _LightColor0;
            half _ShadowIntensity;
            half4 _TintColor;

            #define _TIME (_Time.y)

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                half3 normal : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;

                float t = v.uv.x + v.uv.y;

                float t1 = _WaveFreq1 * t + _WaveSpeed1 * _TIME;

                float wave1 = _WaveAmplitude1 * sin(t1);

                float dWave1 = _WaveFreq1 * _WaveAmplitude1 * cos(t1);

                float t2 = _WaveFreq2 * t + _WaveSpeed2 * _TIME;
                float wave2 = _WaveAmplitude2 * sin(t2);
                float dWave2 = _WaveFreq2 * _WaveAmplitude2 * cos(t2);

                float fixTopScale = (1.0f - v.uv.y);

                float wave = fixTopScale * (wave1 + wave2);
                v.vertex += wave;

                float dWave = fixTopScale * (dWave1 + dWave2);
                float3 objNormal = normalize(float3(dWave, dWave, -1.0f));
                o.normal = mul((float3x3)unity_ObjectToWorld, objNormal);

                #if _DROP_Y_ON
                {
                    float a = 2.0f * _DropY - 1.0f;
                    float topY = 0.5f;
                    v.vertex.y = lerp(topY, v.vertex.y, pow(saturate(v.uv.y + a), 1.0f - a));
                }
                #endif

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                half4 col = tex2D(_MainTex, i.uv);
                col *= _TintColor * _LightColor0;

                half diffuse = saturate(dot(i.normal, _WorldSpaceLightPos0.xyz));

                half halfLambert = lerp(1.0, diffuse, _ShadowIntensity);
                col.rgb *= halfLambert;

                return col;
            }
            ENDCG
        }
    }

    Fallback "Unlit/Texture"
}
