Shader "Unlit/Damagabel" {
    Properties {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Size ("Size", float) = 0
        _Health ("Health", Range(0.0, 1.0)) = 0.0
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
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
                UNITY_DEFINE_INSTANCED_PROP(float, _Size)
                UNITY_DEFINE_INSTANCED_PROP(float, _Health)
            UNITY_INSTANCING_BUFFER_END(Props)

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv - float2(0.4999, 0.4999);

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                UNITY_SETUP_INSTANCE_ID(i);

                float4 col = float4(0, 0, 0, 0);
                float l = length(i.uv);
                float s = UNITY_ACCESS_INSTANCED_PROP(Props, _Size);
                if (l < s) {
                    col = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);

                    if (l > s - 0.013) {
                        float prog = acos(-dot(normalize(i.uv), float2(0.0, 1.0)));
                        if (prog < UNITY_ACCESS_INSTANCED_PROP(Props, _Health) * 3.1415)
                            col.xyz *= 2;

                        else
                            col.xyz = float3(0, 0, 0);
                    }

                }
                return col;
            }
            ENDCG
        }
    }
}
