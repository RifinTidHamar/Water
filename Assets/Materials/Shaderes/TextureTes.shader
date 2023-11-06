Shader "Unlit/TextureTes"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _LogTex("log Tex", 2D) = "white"{}
        _LogColor("Log Color", Color) = (1, 0, 0, 0)
        _FireColor("Fire Color", Color) = (1, 0, 0, 0)
        _AshAngle("Ash angle", Range(0,1)) = 0.1
        _AshVec("Ash vec", Vector) = (1,1,1,1)
    }
    SubShader
    {
        Tags {}

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
            };

            sampler2D _MainTex;
            sampler2D _LogTex;
            float4 _MainTex_ST;
            half4 _LogColor;
            half4 _FireColor;
            float _AshAngle;
            float4 _AshVec; //= float3(0, 1, 0);

            v2f vert (appdata_base v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.normal = v.normal;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col, unTCol;
                col = unTCol = tex2D(_LogTex, i.uv);
                
                col = unTCol = clamp(((col - 0.5) * max(100, 0)) + 0.2, 0, 1);
                //col *= col * col * col;
                //if (dot(mul(unity_ObjectToWorld, _AshVec.xyz), i.normal) > _AshAngle)
                //{
                //    col = _LogColor;
                //}
                fixed4 fireCol;
                fireCol = tex2D(_MainTex, i.uv);
                fireCol *= fireCol * fireCol * fireCol * _FireColor;
                col = 0;
                col += fireCol * (1 - unTCol);
                            //col = 1 - col;
                col.a = 1;
                // apply fog
                return col;
            }
            ENDCG
        }
    }   
}
