#pragma pack_matrix(row_major)

cbuffer Constants : register(b0)
{
    float4x4 WorldViewProj;
    float4x4 World;
    float3 CameraPos;
    float Pad0;
    float3 LightDir;
    float Pad1;
    float4 MaterialColor;
};

struct PSInput
{
    float4 Position : SV_POSITION;
    float3 WorldPos : TEXCOORD0;
    float3 Normal : TEXCOORD1;
};

float4 main(PSInput input) : SV_TARGET
{
    float3 N = normalize(input.Normal);
    float3 L = normalize(-LightDir);
    float3 V = normalize(CameraPos - input.WorldPos);
    float3 H = normalize(L + V);

    // Ambient
    float3 ambient = 0.15 * MaterialColor.rgb;

    // Diffuse (Lambert)
    float NdotL = max(dot(N, L), 0.0);
    float3 diffuse = NdotL * MaterialColor.rgb;

    // Specular (Blinn-Phong)
    float NdotH = max(dot(N, H), 0.0);
    float spec = pow(NdotH, 32.0);
    float3 specular = spec * float3(0.3, 0.3, 0.3);

    // Back-face lighting (softer)
    float backNdotL = max(dot(-N, L), 0.0);
    float3 backDiffuse = 0.3 * backNdotL * MaterialColor.rgb;

    float3 color = ambient + diffuse + specular + backDiffuse;
    return float4(color, MaterialColor.a);
}
