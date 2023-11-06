Shader "Custom/fireParticle"
{
    Properties
    {
    }
    SubShader
    {
        Tags {"Queue" = "Transparent -100" "RenderType" = "Transparent" "LightMode" = "UniversalForward"}

        Pass
        {
            Ztest Always
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            
            struct fireParticle
            {
                float s;
                float heat;
                float lum;
                float mass;
                float3 pos;
                float3 velocity;
            };

            struct vertex
            {
                float3 pos;
            };

            StructuredBuffer<fireParticle> parti;
            StructuredBuffer<vertex> verti;

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 lum : COLOR0;
            };

            v2f vert(uint vID : SV_VertexID, uint id : SV_InstanceID)
            {
                v2f o;
                int index = id * 6 + vID;
                float4 posi = mul(UNITY_MATRIX_VP, float4(parti[id].pos, 1));
                o.pos = posi + float4(verti[index].pos.x, verti[index].pos.y, 0, 0);
                o.lum = float4(0.9607843, 0.3682313, 0.1764705, 1);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = i.lum;
                return col;
            }
            ENDCG
        }
    }
}
