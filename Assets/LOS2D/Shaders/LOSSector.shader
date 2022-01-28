Shader "AillieoUtils/LOS2D/LOSSector"
{
    Properties
    {
        //[PerRendererData]
        _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip("Flip", Vector) = (1,1,1,1)
        //[PerRendererData]
        _AlphaTex("External Alpha", 2D) = "white" {}
        //[PerRendererData]
        _EnableExternalAlpha("Enable External Alpha", Float) = 0

        _UVBorder("UV Border", Vector) = (0, 0, 0, 0) // x=内侧径向边界宽度 y=外侧径向边界宽度 z=起点侧角度边界 w=终点侧角度边界
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM

            #pragma vertex SpriteVert
            #pragma fragment frag

            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"
            // #include "UnityCG.cginc"

            float4 _UVBorder;

            float2 getUVConsideringBorder(float2 uv)
            {
                float r_n = uv.x;
                float theta_n = uv.y;
                float r_max = 1;
                float theta_max = 1;

                float r = r_max * r_n;
                float theta = theta_max * theta_n;

                float r_b_1 = _UVBorder.x;
                float r_b_2 = r_max - _UVBorder.y;

                float r_v_1 = _UVBorder.x * r_max;
                float r_v_2 = r_max - _UVBorder.y * r_max;

                float theta_b_1 = _UVBorder.z;
                float theta_b_2 = theta_max - _UVBorder.w;

                float theta_v_1 = _UVBorder.z * theta_max;
                float theta_v_2 = theta_max - _UVBorder.w * theta_max;

                if (r < r_b_1)
                {
                    r = lerp(0, r_v_1, r / r_b_1);
                }
                else if (r > r_b_2)
                {
                    r = lerp(r_v_2, r_max, (r - r_b_2) / (r_max - r_b_2));
                }
                else
                {
                    r = lerp(r_v_1, r_v_2, (r - r_b_1) / (r_b_2 - r_b_1));
                }

                if (theta < theta_b_1)
                {
                    theta = lerp(0, theta_v_1, theta / theta_b_1);
                }
                else if (theta > theta_b_2)
                {
                    theta = lerp(theta_v_2, theta_max, (theta - theta_b_2) / (theta_max - theta_b_2));
                }
                else
                {
                    theta = lerp(theta_v_1, theta_v_2, (theta - theta_b_1) / (theta_b_2 - theta_b_1));
                }

                r_n = r / r_max;
                theta_n = theta / theta_max;

                return float2(theta_n, r_n);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv2 = getUVConsideringBorder(i.texcoord);
                fixed4 c = SampleSpriteTexture(uv2) * i.color;
                c.rgb *= c.a;

                //c = fixed4(uv2.x, uv2.x, uv2.x, 1);
                return c;
            }
            ENDCG
        }
    }
}
