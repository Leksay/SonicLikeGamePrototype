Shader "Custom/Unlit/Base"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        [Header(Culling)]
        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float) = 2 //"Back"
        [Enum(Off,0,On,1)]_ZWrite ("ZWrite", Float) = 1.0

        [Header(Stencil)]
		_Stencil ("Stencil ID [0;255]", Float) = 0
		_ReadMask ("ReadMask [0;255]", Int) = 255
		_WriteMask ("WriteMask [0;255]", Int) = 255
		[Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp ("Stencil Comparison", Int) = 3
		[Enum(UnityEngine.Rendering.StencilOp)] _StencilOp ("Stencil Operation", Int) = 0
		[Enum(UnityEngine.Rendering.StencilOp)] _StencilFail ("Stencil Fail", Int) = 0
		[Enum(UnityEngine.Rendering.StencilOp)] _StencilZFail ("Stencil ZFail", Int) = 0

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        Cull [_Cull]
        ZWrite [_ZWrite]
   		Stencil
		{
			Ref [_Stencil]
			ReadMask [_ReadMask]
			WriteMask [_WriteMask]
			Comp [_StencilComp]
			Pass [_StencilOp]
			Fail [_StencilFail]
			ZFail [_StencilZFail]
		}

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            //#pragma multi_compile_fog

            #include "UnityCG.cginc"
            //#include "UnityLightingCommon.cginc"

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                //float4 color : COLOR2; // vertex color
                float3 normal : NORMAL;
                float4 screenPos : TEXCOORD1;
                float3 viewD : TEXCOORD2;
                fixed4 world : TEXCOORD3;
                //fixed4 diff : COLOR0; // diffuse lighting color
                //fixed4 directional : COLOR1; // directional lighting color
                //UNITY_FOG_COORDS(1)
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata_full v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                //o.color = v.color; // vertex color
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.normal = UnityObjectToWorldNormal(v.normal);                
                o.screenPos = ComputeScreenPos(o.vertex);
                o.viewD = normalize(WorldSpaceViewDir(v.vertex));
                o.world = mul(unity_ObjectToWorld, v.vertex);
                
                // ==== освещение ====
                // dot product between normal and light direction for
                // directional (Lambert) lighting
                //half nl = max(0, dot(o.normal, _WorldSpaceLightPos0.xyz));
                // factor in the light color
                //o.directional = nl * _LightColor0;
                // ambient light
                //o.diff = unity_AmbientSky;
                
                //UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
            
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                //fixed4 col = tex2D(_MainTex, i.uv) * i.color; // vertex color
                
                // освещение фоновое
                //col.rgb *= i.diff;
                // освещение фоновое + направленное
                //col.rgb *= i.diff + i.directional;
                
                // apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);
                
                return col;
            }
            ENDCG
        }
    }
}
