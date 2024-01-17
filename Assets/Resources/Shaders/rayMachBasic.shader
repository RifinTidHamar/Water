Shader "Unlit/rayMarchBasic"
{
    Properties
    {
        _MainTex("3D tex", 3D) = "white"{}
        _StepSize("Step size", Range(0.0, 0.05)) = 0.5
        //_Amnt("Amount", Range(-1, 1)) = 1
        _TestX("test-x", Range(-2, 2)) = 1
        _TestY("test-y", Range(-2, 2)) = 1
        _TestZ("test-z", Range(-2, 2)) = 1
        _TestW("test-w", Range(-2, 2)) = 1

        _Alpha("alpha", Range(0, 1)) = 0.5
        _Color("Color", Color) = (1,0,0,0)
    }
        SubShader
        {
            Tags {"Queue" = "Transparent"}
            Blend One One
            Cull Off
            Pass
            {

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #include "UnityCG.cginc"
                #include "UnityLightingCommon.cginc" // for _LightColor0

                // Maximum amount of raymarching samples
                #define MAX_STEP_COUNT 150

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    float3 objectVertex : TEXCOORD1;
                    float3 rayDirection : TEXCOORD2;
                };

                sampler3D _MainTex;
                float _StepSize;
                float _TestX;
                float _TestY;
                float _TestZ;
                float _TestW;

                float _Amnt;
                float _Alpha;
                float4 _Color;
            

                v2f vert(appdata v)
                {
                    v2f o;
                    // Vertex in object space this will be the starting point of raymarching
                    //v.vertex.y *= 5;
                    o.objectVertex = v.vertex.xyz; // mul(unity_ObjectToWorld, v.vertex);// +float3(0, 0, 0.5);
                    //o.objectVertex.y /= 256;
                    //float4 newVert = float4(v.vertex.x, o.objectVertex.y, v.vertex.zw);
                    // Calculate vector from camera to vertex in world space
                    float3 worldVertex = o.objectVertex * 1.5; //*2 will need to be change to whatever object scale is
                    //o.objectVertex = worldVertex;
                    o.rayDirection = worldVertex - _WorldSpaceCameraPos;
                    //o.rayDirection.y /= 5;
                    o.vertex = mul(UNITY_MATRIX_VP, float4(worldVertex, 1));
                    return o;
                }

                float4 blendUnder(float4 color, float4 newColor)
                {
                    color.rgb += (1.0 - color.a) * newColor.a * newColor.rgb;
                    color.a += (1.0 - color.a) * newColor.a;
                    return color;
                }



                fixed4 frag(v2f i) : SV_Target
                {
                    float4 backCol = float4(0, 0, 0, 0);
                    //the ray orging is the linearly interpolated vertex position calculated in the vertex shader
                    float3 rayOrigin = mul(unity_ObjectToWorld, i.objectVertex ) / 1.5 + 0.5; //  + float3(_TestX, _TestY, _TestZ)+ 0.5;
                    //rayOrigin += _WorldSpaceCameraPos;
                    float3 worldRayDirection = normalize(i.rayDirection);// (unity_WorldToObject, float4(normalize(i.rayDirection), 1));
                     
                    for (int x = 0; x < MAX_STEP_COUNT; x++)
                    {
                        float4 frontCol = tex3D(_MainTex, rayOrigin);// / float4(1, _TestY, _TestZ, _TestW); // + float3(0.25, 0.333f, 0.25));
                        //frontCol = clamp(frontCol, 0, 1);
                        //blends the color at each step to porperly lWook through the object as a volume
                        float val = step(0, rayOrigin.y) * step(rayOrigin.y, 1) * step(0, rayOrigin.z) * step(0, rayOrigin.x) * step(rayOrigin.z, 1) * step(rayOrigin.x, 1);
                        
                        backCol.rgb += (1.0 - backCol.a) * frontCol.a * frontCol.rgb * _Alpha * val;
                        backCol.a += (1.0 - backCol.a) * frontCol.a *  _Alpha * val;
        
                        rayOrigin += _StepSize * worldRayDirection;
                    }
                    backCol.rgb *= _Color;
                    return backCol;
                }
                ENDCG
            }
        }
}