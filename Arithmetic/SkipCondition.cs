using System;

namespace ImageProcessingTools
{
    [Flags]
    public enum SkipCondition
    {
        None = 0,
        BlackPixel = 1 << 0,
        TransparentPixel = 1 << 1,
        WhitePixel = 1 << 2,
    }
}