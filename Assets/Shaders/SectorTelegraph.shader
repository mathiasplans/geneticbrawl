Shader "Unlit/SectorTelegraph" {
    Properties {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Size ("SectorSize", float) = 0
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
            float _Size;
            float _Progress;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv - float2(0.4999, 0.4999);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                float dist = length(i.uv);
                float4 col = float4(0.0, 0.0, 0.0, 0.0);
                float sizeDist = acos(dot(normalize(i.uv), float2(0.0, 1.0)));
                if (dist < 0.5 && sizeDist < _Size) {
                    col.xyzw = _Color;

                    // Edge
                    if (dist > 0.485 || sizeDist > _Size - 0.035)
                        col.w *= 2;

                    // Progress
                    if (dist > 0.5 * _Progress)
                        col.xyz *= 1.25;
                }
                return col;
            }
            ENDCG
        }
    }
}
