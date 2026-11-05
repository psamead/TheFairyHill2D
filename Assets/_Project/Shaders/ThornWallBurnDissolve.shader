Shader "TheFairyHills/Thorn Wall Burn Dissolve"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _MaskTex ("Mask", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _BurnMask ("Burn Mask", 2D) = "white" {}
        [HideInInspector] _Color ("Tint", Color) = (1,1,1,1)
        [HideInInspector] _RendererColor ("Renderer Tint", Color) = (1,1,1,1)
        [HideInInspector] _AlphaTex ("External Alpha", 2D) = "white" {}
        [HideInInspector] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
        [MaterialToggle] _ZWrite ("ZWrite", Float) = 0
        _Burn ("Burn", Range(0,1)) = 0
        _EdgeWidth ("Fire Edge Width", Range(0.001,0.2)) = 0.06
        _CharWidth ("Char Width", Range(0,0.2)) = 0.03
        [HDR] _EdgeColor ("Fire Edge (Unlit)", Color) = (1,0.353,0.048,1)
        _CharColor ("Charred Edge", Color) = (0.227,0.055,0.031,1)
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        ZWrite [_ZWrite]

        Pass
        {
            Tags { "LightMode"="Universal2D" }
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
            #pragma vertex LitVertex
            #pragma fragment LitFragment
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/ShapeLightShared.hlsl"

            struct Attributes
            {
                COMMON_2D_INPUTS
                half4 color : COLOR;
                UNITY_SKINNED_VERTEX_INPUTS
            };

            struct Varyings
            {
                COMMON_2D_LIT_OUTPUTS
                half4 color : COLOR;
            };

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Lit2DCommon.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half4 _EdgeColor;
                half4 _CharColor;
                float _Burn;
                float _EdgeWidth;
                float _CharWidth;
            CBUFFER_END

            TEXTURE2D(_BurnMask);
            SAMPLER(sampler_BurnMask);

            Varyings LitVertex(Attributes input)
            {
                UNITY_SKINNED_VERTEX_COMPUTE(input);
                SetUpSpriteInstanceProperties();
                input.positionOS = UnityFlipSprite(input.positionOS, unity_SpriteProps.xy);
                Varyings output = CommonLitVertex(input);
                output.color = input.color * _Color * unity_SpriteColor;
                return output;
            }

            half4 LitFragment(Varyings input) : SV_Target
            {
                float mask = SAMPLE_TEXTURE2D(_BurnMask, sampler_BurnMask, input.uv).r;
                clip(mask - _Burn);

                half4 lit = CommonLitFragment(input, input.color);
                if (mask < _Burn + _EdgeWidth)
                    return half4(_EdgeColor.rgb, lit.a);

                if (_CharWidth > 0 && mask < _Burn + _EdgeWidth + _CharWidth)
                    return half4(_CharColor.rgb, lit.a);

                return lit;
            }
            ENDHLSL
        }

        Pass
        {
            Tags { "LightMode"="NormalsRendering" }
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
            #pragma vertex NormalsRenderingVertex
            #pragma fragment NormalsRenderingFragment

            struct Attributes
            {
                COMMON_2D_NORMALS_INPUTS
                float4 color : COLOR;
                UNITY_SKINNED_VERTEX_INPUTS
            };

            struct Varyings
            {
                COMMON_2D_NORMALS_OUTPUTS
                half4 color : COLOR;
            };

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Normals2DCommon.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half4 _EdgeColor;
                half4 _CharColor;
                float _Burn;
                float _EdgeWidth;
                float _CharWidth;
            CBUFFER_END

            TEXTURE2D(_BurnMask);
            SAMPLER(sampler_BurnMask);

            Varyings NormalsRenderingVertex(Attributes input)
            {
                UNITY_SKINNED_VERTEX_COMPUTE(input);
                SetUpSpriteInstanceProperties();
                input.positionOS = UnityFlipSprite(input.positionOS, unity_SpriteProps.xy);
                Varyings output = CommonNormalsVertex(input);
                output.color = input.color * _Color * unity_SpriteColor;
                return output;
            }

            half4 NormalsRenderingFragment(Varyings input) : SV_Target
            {
                float mask = SAMPLE_TEXTURE2D(_BurnMask, sampler_BurnMask, input.uv).r;
                clip(mask - _Burn);
                return CommonNormalsFragment(input, input.color);
            }
            ENDHLSL
        }

        Pass
        {
            Tags { "LightMode"="UniversalForward" "Queue"="Transparent" "RenderType"="Transparent" }
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
            #pragma vertex UnlitVertex
            #pragma fragment UnlitFragment

            struct Attributes
            {
                COMMON_2D_INPUTS
                half4 color : COLOR;
                UNITY_SKINNED_VERTEX_INPUTS
            };

            struct Varyings
            {
                COMMON_2D_OUTPUTS
                half4 color : COLOR;
            };

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/2DCommon.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half4 _EdgeColor;
                half4 _CharColor;
                float _Burn;
                float _EdgeWidth;
                float _CharWidth;
            CBUFFER_END

            TEXTURE2D(_BurnMask);
            SAMPLER(sampler_BurnMask);

            Varyings UnlitVertex(Attributes input)
            {
                UNITY_SKINNED_VERTEX_COMPUTE(input);
                SetUpSpriteInstanceProperties();
                input.positionOS = UnityFlipSprite(input.positionOS, unity_SpriteProps.xy);
                Varyings output = CommonUnlitVertex(input);
                output.color = input.color * _Color * unity_SpriteColor;
                return output;
            }

            half4 UnlitFragment(Varyings input) : SV_Target
            {
                half4 sprite = CommonUnlitFragment(input, input.color);
                float mask = SAMPLE_TEXTURE2D(_BurnMask, sampler_BurnMask, input.uv).r;
                clip(mask - _Burn);
                if (mask < _Burn + _EdgeWidth) return half4(_EdgeColor.rgb, sprite.a);
                if (_CharWidth > 0 && mask < _Burn + _EdgeWidth + _CharWidth)
                    return half4(_CharColor.rgb, sprite.a);
                return sprite;
            }
            ENDHLSL
        }
    }
}
