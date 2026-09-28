// Unity macro to declare a texture named _BlitTexture
TEXTURE2D_X(_BlitTexture);

void Outline_float(float2 uv, float2 texelSize, float width, out float alpha)
{
    alpha = 0;
    
    if (LOAD_TEXTURE2D_X_LOD(_BlitTexture, uint2(uv * _ScreenSize.xy), 0).r > 0)
    {
        return;
    }
    
    int radius = (int)width;
    
    for (int x = -radius; x <= radius; x++)
    {
        for (int y = -radius; y <= radius; y++)
        {
            if (x == 0 && y == 0)
            {
                continue;
            }
                
            float2 offset = float2(x, y) * texelSize;
            
            if (LOAD_TEXTURE2D_X_LOD(_BlitTexture, uint2((uv + offset) * _ScreenSize.xy), 0).r > 0)
            {
                alpha = 1;
                return;
            }
        }
    }
}