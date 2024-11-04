using System;

namespace ImageProcessingTools
{
    [Flags]
    public enum ChannelMask
    {
        None = 0,
        A = 1,
        B = 2,
        G = 4,
        R = 8,
        RGBA = R | G | B | A,
        RGB = R | G | B,
    }
}