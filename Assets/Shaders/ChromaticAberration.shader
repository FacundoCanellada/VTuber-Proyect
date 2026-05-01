Shader "Custom/ChromaticAberration"
{
    Properties
    {
        // Intensidad del efecto: valores mayores = más separación de canales de color
        _Intensity ("Intensidad", Range(0, 0.05)) = 0.005
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off
        Cull Off
        ZTest Always

        Pass
        {
            Name "ChromaticAberration"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _Intensity;

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord;

                // El desplazamiento crece desde el centro hacia los bordes
                float2 offset = (uv - 0.5) * _Intensity;

                // Cada canal de color se muestrea con un pequeño desplazamiento distinto
                half r = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv - offset).r;
                half g = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv).g;
                half b = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + offset).b;
                half a = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv).a;

                return half4(r, g, b, a);
            }
            ENDHLSL
        }
    }
}
