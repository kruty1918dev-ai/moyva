Shader "Hidden/Moyva/Vegetation Billboard Bake"
{
    Properties { _BaseMap("Palette", 2D) = "white" {} }
    SubShader
    {
        Pass
        {
            Cull Off ZWrite On ZTest LEqual
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "UnityCG.cginc"
            sampler2D _BaseMap;
            struct Input { float4 vertex : POSITION; float3 normal : NORMAL; float2 uv : TEXCOORD0; };
            struct Output { float4 position : SV_POSITION; float2 uv : TEXCOORD0; float light : TEXCOORD1; };
            Output Vert(Input v)
            {
                Output o;
                o.position = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.light = 0.62 + 0.38 * saturate(dot(UnityObjectToWorldNormal(v.normal), normalize(float3(-0.4, 0.8, -0.5))));
                return o;
            }
            half4 Frag(Output i) : SV_Target { return half4(tex2D(_BaseMap, i.uv).rgb * i.light, 1); }
            ENDHLSL
        }
    }
}
