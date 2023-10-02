Shader "Unlit/crowdNoInstance"
{
    Properties
    {
        _Fella("Texture", 2D) = "white" {}
    }
        SubShader
    {
        Tags { "LightMode" = "ForwardBase" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5
            #include "UnityCG.cginc"    

            static int2 carpPos[] = {int2(0,0), int2(1,0), int2(2,0), int2(3,0),
                                     int2(0,-1), int2(1,-1), int2(2,-1), int2(3,-1),
                                     int2(0,-2), int2(1,-2),
                                     int2(0,-3), int2(1,-3)};

            static int2 headPos[] = { int2(0,0),  int2(1,0),  int2(2,0),  int2(3,0),
                                      int2(0,-1), int2(1,-1), int2(2,-1), int2(3,-1),
                                      int2(0,-2), int2(1,-2), int2(2,-2), int2(3,-2),
                                      int2(0,-3), int2(1,-3), int2(2,-3), int2(3,-3)};

            float hash11(float p)
            {
                p = frac(p * 0.1031);
                p *= p + 33.33;
                p *= p + p;
                p = frac(p);
                //p -= 0.5;
                return p;
            }

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
            sampler2D _Fella;

            v2f vert(appdata_full v)
            {
                v2f o;
                o.pos = mul(unity_ObjectToWorld, v.vertex);
                uint r = 4;
                uint hw = 3;
                o.pos = mul(UNITY_MATRIX_VP, o.pos);
                o.uv = v.texcoord;
                if (!(o.uv.y < 0.5 && o.uv.x > 0.5))
                    o.uv += 0.25 * carpPos[r];
                else
                    o.uv += 0.125 * headPos[hw];
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_Fella, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
