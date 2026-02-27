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

float4 main() : SV_TARGET
{
    return MaterialColor;
}
