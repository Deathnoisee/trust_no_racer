Shader "Custom/SpriteOutline"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineWidth ("Outline Width", Range(0, 0.05)) = 0.005
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4 _OutlineColor;
            float _OutlineWidth;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR; // <-- this is what carries SpriteRenderer.color
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR; // <-- pass it through to the fragment stage
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color; // <-- forward the vertex color
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * i.color; // <-- use vertex color, not a fixed _Color property

                if (col.a < 0.99)
                {
                    float2 offsets[8] = {
                        float2(1,0), float2(-1,0), float2(0,1), float2(0,-1),
                        float2(1,1), float2(-1,1), float2(1,-1), float2(-1,-1)
                    };

                    for (int j = 0; j < 8; j++)
                    {
                        float2 uvOffset = i.uv + offsets[j] * _OutlineWidth;
                        fixed4 sample = tex2D(_MainTex, uvOffset);
                        if (sample.a > 0.5)
                        {
                            return _OutlineColor;
                        }
                    }
                }

                return col;
            }
            ENDCG
        }
    }
}