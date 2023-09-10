Shader "Unlit/crowd"
{
    Properties
    {
        _Fella ("Texture", 2D) = "white" {}
    }
        SubShader
    {
        Tags { "LightMode" = "ForwardBase" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            #pragma target 4.5
            #include "UnityCG.cginc"    

            struct Person
            {
                uint stand;
                uint robes;
                uint headwear;
                float3 pos;
                float3 dir;
                float speed;
                float speedSaved;
                float height;
                float timeAtAshop;
                float finishedTimeAtShop;
                float timeSearching;
            };

            StructuredBuffer<Person> crowd;
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

            float3x3 rotate(float3 dir)
            {
                //angle between two vectos
                float dProd = dot(dir, float3(0, 0, 1));
                float absDProd = dot(length(dir), 1);
                float val = dProd / absDProd;
                float x = acos(val) * sign(dir);

                //rotation matrix
                float3x3 rot = {cos(x), 0 , sin(x),
                                0,      1,      0,
                                -sin(x), 0,  cos(x)};
                return rot;
            }

            v2f vert (appdata_full v, uint id : SV_InstanceID)
            {
                Person data = crowd[id];
                uint r = crowd[id].robes;
                uint hw = crowd[id].headwear;

                v2f o;
                float3 rotVert = mul(rotate(crowd[id].dir), v.vertex.xyz);
                float3 posit = data.pos + rotVert * crowd[id].height;
                posit.y += sin(_Time.z * crowd[id].speedSaved * sign(crowd[id].speed) * 4)/20;
                o.pos = mul(UNITY_MATRIX_VP, float4(posit, 1.0f));
                o.uv = v.texcoord;
                if (!(o.uv.y < 0.5 && o.uv.x > 0.5))
                    o.uv += 0.25 * carpPos[r];
                else
                    o.uv += 0.125 * headPos[hw];
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_Fella, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
