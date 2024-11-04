namespace ImageProcessingTools
{
    public static class CommonPipelines
    {
        public static readonly ImageProcessingPass[] Opening3 = {
            new(CommonKernels.Ones3, KernelOperation.Erode),
            new(CommonKernels.Ones3, KernelOperation.Dilate)
        };

        public static readonly ImageProcessingPass[] Closing3 = {
            new(CommonKernels.Ones3, KernelOperation.Dilate),
            new(CommonKernels.Ones3, KernelOperation.Erode)
        };
    }
}