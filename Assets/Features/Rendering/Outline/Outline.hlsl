// Unity macro to declare a texture named _BlitTexture
TEXTURE2D_X(_BlitTexture);

void OutlineHorizontal_float(float2 uv, float width, out float horizontal, out float mask)
{
    horizontal = 0;
   
    uint2 coords = uint2(uv * _ScreenSize.xy);
    
    mask = LOAD_TEXTURE2D_X_LOD(_BlitTexture, coords, 0).r;
    
    int radius = (int)width;
    
    for (int x = -radius; x <= radius; x++)
    {
        if (LOAD_TEXTURE2D_X_LOD(_BlitTexture, coords + int2(x, 0), 0).r > 0)
        {
            horizontal = 1;
            return;
        }
    }
}

void OutlineVertical_float(float2 uv, float width, out float alpha)
{
    alpha = 0;
    
    uint2 coords = uint2(uv * _ScreenSize.xy);
    
    float mask = LOAD_TEXTURE2D_X_LOD(_BlitTexture, coords, 0).g;
    
    int radius = (int)width;
    
    for (int y = -radius; y <= radius; y++)
    {
        if (LOAD_TEXTURE2D_X_LOD(_BlitTexture, coords + int2(0, y), 0).r > 0)
        {
            alpha = 1;
            break;
        }
    }
    
    alpha -= mask;
}