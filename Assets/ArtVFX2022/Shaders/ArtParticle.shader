Shader "ArtVFX2022/Particle"
{
    Properties
    {
        [HDR] _Tint ("Tint", Color) = (1,1,1,1)
        _Intensity ("Brightness", Range(0,6)) = 2
        [Enum(Soft,0,Flame,1,Ring,2)] _Style ("Shape", Float) = 0
        _Turbulence ("Flame Turbulence", Range(0,1)) = 0.45
        _RingWidth ("Ring Width", Range(0.01,0.3)) = 0.075
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Destination Blend", Float) = 1
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            Blend SrcAlpha [_DstBlend]
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                float4 _Tint;
                float _Intensity, _Style, _Turbulence, _RingWidth;
            CBUFFER_END
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };
            float Hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1,311.7))) * 43758.5453);
            }
            float Noise(float2 p)
            {
                float2 i = floor(p), f = frac(p);
                f = f*f*(3.0-2.0*f);
                return lerp(lerp(Hash(i), Hash(i+float2(1,0)),f.x),
                    lerp(Hash(i+float2(0,1)),Hash(i+1.0),f.x), f.y);
            }
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                float2 p = (input.uv-0.5)*2.0;
                float r = length(p);
                float mask = pow(saturate(1.0-r),1.5);
                if (_Style > 1.5)
                {
                    float aa = max(fwidth(r),0.001);
                    mask = 1.0-smoothstep(_RingWidth*0.2,
                        _RingWidth*0.5+aa,abs(r-0.82));
                }
                else if (_Style > 0.5)
                {
                    float noise = Noise(p*4.0 + float2(_Time.y*0.5,-_Time.y*2.5));
                    float shapedRadius = r + (noise-0.5)*_Turbulence;
                    mask = 1.0-smoothstep(0.35,0.85,shapedRadius);
                    mask *= lerp(0.6,1.0,noise);
                }
                mask *= 1.0-smoothstep(0.94,1.0,r);
                half4 color = input.color*_Tint;
                return half4(color.rgb*_Intensity,color.a*mask);
            }
            ENDHLSL
        }
    }
}
