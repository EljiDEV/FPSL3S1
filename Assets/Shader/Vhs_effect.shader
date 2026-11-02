Shader "Custom/URP_VHS_Effect"
{
    Properties
    {
        [HideInInspector] _BlitTexture("Texture", 2D) = "white" {}
        _Intensity ("Intensité de l'Aberration (Couleurs)", Range(0, 1)) = 0.5
        _ScanlineCount ("Nombre de Lignes", Float) = 300
        _ScanlineColor ("Couleur des Lignes", Color) = (0, 0, 0, 1) // Noir par défaut
        _ScanlineOpacity ("Opacité des Lignes", Range(0, 1)) = 0.3
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off Cull Off ZTest Always

        Pass
        {
            Name "VHSPostProcess"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D_X(_BlitTexture);
            SAMPLER(sampler_BlitTexture);
            
            float _Intensity;
            float _ScanlineCount;
            half4 _ScanlineColor;
            float _ScanlineOpacity;

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(uint vertexID : SV_VertexID)
            {
                Varyings output;
                
                float2 uv = float2((vertexID << 1) & 2, vertexID & 2);
                output.positionCS = float4(uv * 2.0 - 1.0, 0.0, 1.0);
                
                #if UNITY_UV_STARTS_AT_TOP
                uv.y = 1.0 - uv.y;
                #endif
                
                output.uv = uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;

                float offset = 0.01 * _Intensity;
                
                half r = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, float2(uv.x - offset, uv.y)).r;
                half g = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv).g;
                half b = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, float2(uv.x + offset, uv.y)).b;

                half4 col = half4(r, g, b, 1.0);
                
                float scanline = sin(uv.y * _ScanlineCount + _Time.y * 50.0);
                
                if(scanline < 0.0)
                {
                    col.rgb = lerp(col.rgb, _ScanlineColor.rgb, _ScanlineOpacity);
                }

                return col;
            }
            ENDHLSL
        }
    }
}