Shader "ParryProject/Sprites/Item Rarity Outline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (1, 1, 1, 1)
        _OutlinePixels ("Outline Width (Pixels)", Range(0.5, 4)) = 1
        _Color ("Tint", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex SpriteVert
            #pragma fragment Frag
            #pragma target 2.0
            #include "UnitySprites.cginc"

            float4 _MainTex_TexelSize;
            fixed4 _OutlineColor;
            float _OutlinePixels;

            fixed4 Frag(v2f IN) : SV_Target
            {
                fixed4 sprite = SampleSpriteTexture(IN.texcoord) * IN.color;

                float2 offset = _MainTex_TexelSize.xy * _OutlinePixels;
                fixed outlineAlpha = 0;
                outlineAlpha = max(outlineAlpha, SampleSpriteTexture(IN.texcoord + float2( offset.x, 0)).a);
                outlineAlpha = max(outlineAlpha, SampleSpriteTexture(IN.texcoord + float2(-offset.x, 0)).a);
                outlineAlpha = max(outlineAlpha, SampleSpriteTexture(IN.texcoord + float2(0,  offset.y)).a);
                outlineAlpha = max(outlineAlpha, SampleSpriteTexture(IN.texcoord + float2(0, -offset.y)).a);
                outlineAlpha = max(outlineAlpha, SampleSpriteTexture(IN.texcoord + float2( offset.x,  offset.y)).a);
                outlineAlpha = max(outlineAlpha, SampleSpriteTexture(IN.texcoord + float2(-offset.x,  offset.y)).a);
                outlineAlpha = max(outlineAlpha, SampleSpriteTexture(IN.texcoord + float2( offset.x, -offset.y)).a);
                outlineAlpha = max(outlineAlpha, SampleSpriteTexture(IN.texcoord + float2(-offset.x, -offset.y)).a);

                if (sprite.a > 0.001)
                {
                    sprite.rgb *= sprite.a;
                    return sprite;
                }

                fixed4 outline = _OutlineColor * IN.color;
                outline.a *= outlineAlpha;
                outline.rgb *= outline.a;
                return outline;
            }
            ENDCG
        }
    }
}
