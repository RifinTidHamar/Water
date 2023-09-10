Shader "Unlit/TexelShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 tangent  : TANGENT;
            };
            
            struct texel
            {
                float2 uv;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 wVert : TEXCOORD2;
                float4 tangent : TEXCOORD1;
            };

            Texture2D _MainTex;
            SamplerState sampler_MainTex;
            RWStructuredBuffer<texel> tex;

            v2f vert (appdata v, uint vID : SV_VertexID)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.wVert = mul(unity_ObjectToWorld, v.vertex);
                o.uv = v.uv;
                //tex[vID].uv = v.uv;
                o.tangent = v.tangent;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // sample the texture

                fixed4 col = _MainTex.Sample(sampler_MainTex, i.uv);
                //col.rg = i.uv * abs(i.wVert);
                return col;
            }
            ENDCG
        }
    }
}
