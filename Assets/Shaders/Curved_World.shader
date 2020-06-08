Shader "Javir/CurvedWorld" {
	Properties {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
        _Curvature("Curvature", Float) = 0.001
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Lambert vertex:vert addshadow

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
        uniform float _Curvature;

		struct Input {
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

        void vert(inout appdata_full v)
            {
                float4 worldSpace = mul(unity_ObjectToWorld, v.vertex);
                worldSpace.xyz -= _WorldSpaceCameraPos.xyz;
                worldSpace = float4(0.0f, (worldSpace.z * worldSpace.z) * - _Curvature, 0.0f,0.0f );

                v.vertex += mul(unity_WorldToObject, worldSpace);
            }

		void surf (Input IN, inout SurfaceOutput o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;         
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
