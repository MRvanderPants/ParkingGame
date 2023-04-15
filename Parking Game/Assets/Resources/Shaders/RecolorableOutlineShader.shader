Shader "Custom/RecolorableOutlineShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)
        _OutlineColor("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth("Outline Width", Range(0,0.1)) = 0.01
        _Recolor("Recolor", 2D) = "white" {}
        _Tint("Tint", Color) = (1, 1, 1, 1)
    }

        SubShader
        {
            Tags {"Queue" = "Transparent" "RenderType" = "Opaque"}
            LOD 100

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

                sampler2D _MainTex;
                float4 _MainTex_ST;
                float4 _Color;
                float4 _OutlineColor;
                float _OutlineWidth;

                sampler2D _Recolor;
                float4 _Recolor_ST;
                float4 _Tint;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    // Calculate the distance to the nearest edge
                    float2 d = fwidth(i.uv);
                    float edge = max(d.x, d.y);

                    // Calculate the color of the outline
                    fixed4 outline = _OutlineColor;

                    // Calculate the color of the object
                    fixed4 col = tex2D(_MainTex, i.uv);

                    // Mix the outline and object color based on the edge distance
                    fixed4 mixedColor = lerp(outline, col, smoothstep(_OutlineWidth - edge, _OutlineWidth + edge, 0.5));

                    // Apply the main color to the mixed color
                    mixedColor *= _Color;

                    // Apply the recolor texture and tint to the mixed color
                    fixed4 recolored = tex2D(_Recolor, i.uv);
                    mixedColor *= recolored;
                    mixedColor *= _Tint;

                    return mixedColor;
                }
                ENDCG
            }
        }
            FallBack "Diffuse"
}