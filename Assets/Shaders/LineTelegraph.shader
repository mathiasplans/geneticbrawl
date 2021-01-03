Shader "Unlit/LineTelegraph" {
    Properties {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Length ("Length", Range(0.0, 1.0)) = 1.0
        _Progress ("Progress", Range(0.0, 1.0)) = 0.0
    }

    SubShader {
        Tags { "Queue" = "Transparent" "RenderType"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float4 _Color;
            float _Length;
            float _Progress;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv - float2(0.4999, 0.4999);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                float4 col = float4(0.0, 0.0, 0.0, 0.0);
                if (i.uv.y > 0 && i.uv.y < _Length / 2) {
                    col = _Color;

                    // Edge
                    if ((i.uv.y < 0.002) || (i.uv.y > _Length / 2 - 0.002) || (i.uv.x > 0.475) || (i.uv.x < -0.475))
                        col.w *= 2;

                    // Progress
                    if (i.uv.y > (_Length / 2) * _Progress)
                        col.xyz *= 1.25;
                }

                return col;
            }
            ENDCG
        }
    }
}
