Shader "Hidden/DrawSelectionBox"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float4 _Box;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float d = dot(_Box, 1);
                if(d == 0)
                {
                    return col;
                }
                if(i.vertex.x < _Box.x || i.vertex.x > _Box.z || i.vertex.y < _Box.y || i.vertex.y > _Box.w)
                {
                    return col;
                }
                float weight = 2;
                float x1Dist = abs(i.vertex.x - _Box.x);
                float x2Dist = abs(i.vertex.x - _Box.z);
                float y1Dist = abs(i.vertex.y - _Box.y);
                float y2Dist = abs(i.vertex.y - _Box.w);
                float xDist = min(x1Dist, x2Dist);
                float yDist = min(y1Dist, y2Dist);
                float dist = min(xDist, yDist);
                col = lerp(col, fixed4(0, 1, 0, 1), smoothstep(weight, 0, dist));
                return col;
            }
            ENDCG
        }
    }
}
