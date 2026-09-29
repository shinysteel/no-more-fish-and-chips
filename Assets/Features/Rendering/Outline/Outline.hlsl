// Unity macro to declare a texture named _BlitTexture
TEXTURE2D_X(_BlitTexture);

void OutlineHorizontal_float(float2 uv, float width, out float horizontal, out float occlusion, out float mask)
{
    uint2 coords = uint2(uv * _ScreenSize.xy);
    
    horizontal = 0;
    occlusion = 0;
    mask = LOAD_TEXTURE2D_X_LOD(_BlitTexture, coords, 0).r;
    
    int radius = (int)width;
    
    for (int x = -radius; x <= radius; x++)
    {
        float4 sample = LOAD_TEXTURE2D_X_LOD(_BlitTexture, coords + int2(x, 0), 0);
        
        if (sample.r > 0)
        {
            horizontal = 1;
        }
        
        if (sample.g > 0)
        {
            occlusion = 1;
        }
        
        if (horizontal > 0 && occlusion > 0)
        {
            return;
        }
    }
}

void OutlineVertical_float(float2 uv, float width, out float alpha)
{   
    alpha = 0;
    
    int radius = (int)width;
    uint2 coords = uint2(uv * _ScreenSize.xy);
    
    float expandedMask = 0;
    float expandedOcclusion = 0;
    float mask = LOAD_TEXTURE2D_X_LOD(_BlitTexture, coords, 0).b;
    
    for (int y = -radius; y <= radius; y++)
    {
        float4 sample = LOAD_TEXTURE2D_X_LOD(_BlitTexture, coords + int2(0, y), 0);
        
        if (sample.r > 0)
        {
            expandedMask = 1;
        }
        
        if (sample.g > 0)
        {
            expandedOcclusion = 1;
        }
        
        if (expandedMask > 0 && expandedOcclusion > 0)
        {
            break;
        }
    }
    
    alpha = (expandedMask - mask) * expandedOcclusion;
}