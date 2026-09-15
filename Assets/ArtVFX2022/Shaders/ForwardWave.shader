Shader "ArtVFX2022/ForwardWave"
{
    Properties
    {
        [HDR] _BodyColor ("Body Color", Color) = (0.04,0.42,0.85,1)
        [HDR] _CrestColor ("Crest Color", Color) = (0.6,1,1,1)
        _Intensity ("Brightness", Range(0,6)) = 1.7
        _ScrollSpeed ("Surface Scroll Speed", Range(0,5)) = 1.5
        _Opacity ("Opacity", Range(0,1)) = 0.8
        [Toggle] _GroundWake ("Ground Wake", Float) = 0
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                float4 _BodyColor, _CrestColor;
                float _Intensity, _ScrollSpeed, _Opacity, _GroundWake;
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
                float2 uv = input.uv;
                float time = _Time.y*_ScrollSpeed;
                float edgeFade = smoothstep(0.0,0.12,uv.x)*
                    (1.0-smoothstep(0.88,1.0,uv.x));
                float ripple = sin(uv.x*71.0+sin(uv.x*23.0+time)*2.0-time*4.0);
                float crest = 1.0-smoothstep(0.025,0.17,
                    abs(uv.y-(0.76+ripple*0.025)));
                float streak = pow(saturate(sin(uv.x*83.0+uv.y*7.0-time*3.0)),12.0);
                float baseFade = smoothstep(0.0,0.15,uv.y);
                float lipFade = 1.0-smoothstep(0.93,1.0,uv.y);
                float details = saturate(crest+streak*0.25);
                float alpha = edgeFade*baseFade*lipFade*(0.38+crest*0.62);
                if (_GroundWake > 0.5)
                {
                    float lines = pow(saturate(sin(uv.y*35.0-time*9.0+ripple*0.3)),6.0);
                    details = lines;
                    alpha = edgeFade*sin(uv.y*PI)*(0.15+lines*0.5);
                }
                half3 rgb = lerp(_BodyColor.rgb,_CrestColor.rgb,details);
                return half4(rgb*input.color.rgb*_Intensity,
                    saturate(alpha*_Opacity*input.color.a));
            }
            ENDHLSL
        }
    }
}
