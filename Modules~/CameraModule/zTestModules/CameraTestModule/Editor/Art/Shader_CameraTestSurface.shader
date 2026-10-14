// Draws the Camera test scene's meshes under every render pipeline without a pipeline
// package. The last SubShader's one pass has no LightMode tag, which Built-in draws as it
// stands and HDRP draws as SRPDefaultUnlit. URP takes the first SubShader, the same pass beside
// URP's own depth passes: its depth prepass draws nothing else, and with depth priming on, a
// mesh missing from that prepass is never drawn. The first SubShader asks for URP, so a project
// without URP never compiles it. A light fixed where Unity's default Directional Light sits
// shades each face, so a cube still reads as a cube; nothing casts or receives a shadow.
Shader "Hidden/Modules/CameraTest/Surface"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)

        // Read by URP's depth passes, which cull back faces like the colour pass.
        [HideInInspector] _Cull ("Cull", Float) = 2
    }

    CGINCLUDE
    #include "UnityCG.cginc"

    // Towards a light rotated (50, -30, 0), the rotation Unity gives a new Directional Light.
    static const float3 TO_LIGHT = float3(0.321, 0.766, -0.557);

    fixed4 _Color;

    struct Attributes
    {
        float4 vertex : POSITION;
        float3 normal : NORMAL;
    };

    struct Varyings
    {
        float4 position : SV_POSITION;
        float3 normal : TEXCOORD0;
    };

    Varyings Vert(Attributes input)
    {
        Varyings output;
        output.position = UnityObjectToClipPos(input.vertex);
        output.normal = UnityObjectToWorldNormal(input.normal);
        return output;
    }

    fixed4 Frag(Varyings input) : SV_Target
    {
        float lambert = saturate(dot(normalize(input.normal), TO_LIGHT));
        return fixed4(_Color.rgb * (0.25 + 0.75 * lambert), 1);
    }
    ENDCG

    SubShader
    {
        PackageRequirements
        {
            "com.unity.render-pipelines.universal"
        }

        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" "Queue" = "Geometry" }

        UsePass "Universal Render Pipeline/Unlit/DEPTHONLY"
        UsePass "Universal Render Pipeline/Unlit/DEPTHNORMALSONLY"

        Pass
        {
            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            ENDCG
        }
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }

        Pass
        {
            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            ENDCG
        }
    }
}
