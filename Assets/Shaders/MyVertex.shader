Shader "Unlit/MyVertex"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 debug : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP(bool, _Selected)
            UNITY_INSTANCING_BUFFER_END(Props)

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                //get oriign in clip pos
                float3 localOrigin = float3(0,0,0);
                float4 originInClipPos = UnityObjectToClipPos(localOrigin);

                //get width and height in clip space
                float widthPercent = 0.01;
                float aspectRatio = _ScreenParams.y / _ScreenParams.x;
                float width = widthPercent * 2;
                //counter-act perspective divide to ensure constant screen space size
                width *= originInClipPos.w;
                float height = width / aspectRatio;
                
                o.vertex = originInClipPos + v.vertex * float4(width, -height, 0, 0);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                float2 diff = i.uv - float2(0.5, 0.5);
                float dist = length(diff);
                fixed4 col = 0;
                bool selected = UNITY_ACCESS_INSTANCED_PROP(Props, _Selected);
                if(selected)
                {
                    col.rgb = lerp(float3(1,1,1), float3(1,1,0), smoothstep(0.35, 0.25, dist));
                }
                col.a = smoothstep(0.5, 0.4, dist);
                return col;
            }
            ENDCG
        }
    }
}
