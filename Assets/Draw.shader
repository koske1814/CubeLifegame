Shader "Custom/Draw"
{
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass{
        CGPROGRAM
        #pragma target 5.0
        #pragma vertex vert_img
        #pragma fragment frag

        #include "UnityCG.cginc"
        StructuredBuffer<int> _Buf;
        int _Width;
        int _Height;

        inline int xyToIdx(int2 xy)
        {
            return xy.y * _Width  + xy.x;
        }

        fixed4 frag (v2f_img i) : SV_Target
            {
                int2 xy = int2(_Width,_Height) * i.uv;
                int data = _Buf[xyToIdx(xy)];
                return fixed4((data ? 1 : 0.5).xxxx);
            }
        ENDCG
        }
    }
}
