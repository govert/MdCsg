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

struct VSInput
{
    float3 Position : POSITION;
    float3 Normal : NORMAL;
};

struct VSOutput
{
    float4 Position : SV_POSITION;
    float3 WorldPos : TEXCOORD0;
    float3 Normal : TEXCOORD1;
};

VSOutput main(VSInput input)
{
    VSOutput output;
    output.Position = mul(float4(input.Position, 1.0), WorldViewProj);
    output.WorldPos = mul(float4(input.Position, 1.0), World).xyz;
    output.Normal = mul(float4(input.Normal, 0.0), World).xyz;
    return output;
}
