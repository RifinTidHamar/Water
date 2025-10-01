Shader "Unlit/Fire"
{
    Properties
    {
        _FlameTex("Texture", 3D) = "white" {}
        _EdgeGradient("Edge Gradient", 2D) = "white" {}
        _SmallEdgeGrad("small Edge Grad", 2D) = "white" {}
        _Color("Color", Color) = (1, 0, 0, 0)
        _Speed("Speed", Range(0, 5)) = 0
        _Strength("Strength", Range(0, 2)) = 0
        _Test("Test", Range(0, 1)) = 0.5
        curCount("curCount", Int) = 1
        totCount("totCount", Int) = 1

    }
    SubShader
    {
        Tags {"Queue" = "Transparent" "RenderType" = "Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Assets/CsScripts/noiseSimplex.cginc"
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

            sampler3D _FlameTex;
            sampler2D _EdgeGradient;
            sampler2D _SmallEdgeGrad;
            float4 _Color;
            float _Test;
            float _Speed;
            float _Strength;
            int curCount;
            int totCount;
            static float trans = 1;

            v2f vert(appdata v)
            {
                v2f o;
                float3 worldVertex = mul(unity_ObjectToWorld, v.vertex).xyz;
                //v.vertex.yz += snoise(v.vertex.xz *  2 * (worldVertex.y * 10000) + _Time.z)/15;
                float2 uv = v.uv * v.uv;
                //v.vertex = mul(unity_ObjectToWorld, v.vertex);
                float time = _Time.y;
                v.vertex.x += snoise((worldVertex.xy - time) * _Speed) * uv.y * _Strength;
                //v.vertex.y += snoise((worldVertex.yz - time) * _Speed) * uv.y * _Strength * 2;
                v.vertex.z += snoise((worldVertex.zx - time) * _Speed) * uv.y * _Strength;
                o.vertex = UnityObjectToClipPos(v.vertex);
                //o.vertex = mul(UNITY_MATRIX_VP, v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float4 flameTex = tex3D(_FlameTex, float3(i.uv.x, i.uv.y, (float)curCount / (float) totCount));
                float4 smallEdgeGrad = tex2D(_SmallEdgeGrad, i.uv);
                float4 tempFlameTex = 1 - step(flameTex, 0.99);
                flameTex = clamp(((flameTex - 0.5f) * max(1.2, 0)) + 0.5f, 0, 1);
                float4 edgeGrad = tex2D(_EdgeGradient, i.uv);
                fixed4 col = flameTex * edgeGrad.r * trans * _Color * 2.5;
                col.rgb += tempFlameTex * edgeGrad.r * trans;// *_Test;
                float uvGrad = 1 - i.uv.y;
                col.rgb += float3(0, 0, 1) * (uvGrad - _Test) * tempFlameTex.b * smallEdgeGrad * trans;
                col.a = flameTex * edgeGrad.r * trans;
                return col;
            }
            ENDCG
        }
    }
}
