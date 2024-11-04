namespace ImageProcessingTools
{
    public static class CommonKernels
    {
        public static readonly float[] Ones3Arr = { 1f, 1, 1, 1, 1, 1, 1, 1, 1 };

        public static readonly float[] Ones5Arr =
            { 1f, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };

        public static readonly float[] X3Arr = { 1f, 0, 1, 0, 1, 0, 1, 0, 1 };

        public static readonly float[] Cross3Arr = { 0f, 1, 0, 1, 1, 1, 0, 1, 0 };

        public static readonly Kernel GaussianBlur3 =
            new(new[] { 1 / 4f, 2 / 4f, 1 / 4f }, KernelCreationFlags.EinsteinSummation, 3);

        public static readonly Kernel GaussianBlur5 =
            new(new[] { 1 / 16f, 4 / 16f, 6 / 16f, 4 / 16f, 1 / 16f }, KernelCreationFlags.EinsteinSummation, 5);

        public static readonly Kernel BoxBlur3 =
            new(new[] { 1 / 9f, 1 / 9f, 1 / 9f, 1 / 9f, 1 / 9f, 1 / 9f, 1 / 9f, 1 / 9f, 1 / 9f },
                KernelCreationFlags.None, 3);

        public static readonly Kernel LaplacianEdgeDetect3 =
            new(new[] { 0f, 1, 0, 1, -4, 1, 0, 1, 0 }, KernelCreationFlags.None, 3);

        public static readonly Kernel BottomSobelEdgeDetect3 =
            new(new[] { -1f, -2, -1, 0, 0, 0, 1, 2, 1 }, KernelCreationFlags.None, 3);

        public static readonly Kernel TopSobelEdgeDetect3 =
            new(new[] { 1f, 2, 1, 0, 0, 0, -1, -2, -1 }, KernelCreationFlags.None, 3);

        public static readonly Kernel LeftSobelEdgeDetect3 =
            new(new[] { 1f, 0, -1, 2, 0, -2, 1, 0, -1 }, KernelCreationFlags.None, 3);

        public static readonly Kernel RightSobelEdgeDetect3 =
            new(new[] { -1f, 0, 1, -2, 0, 2, -1, 0, 1 }, KernelCreationFlags.None, 3);

        public static readonly Kernel BottomPrewittEdgeDetect3 =
            new(new[] { -1f, -1, -1, 0, 0, 0, 1, 1, 1 }, KernelCreationFlags.None, 3);

        public static readonly Kernel TopPrewittEdgeDetect3 =
            new(new[] { 1f, 1, 1, 0, 0, 0, -1, -1, -1 }, KernelCreationFlags.None, 3);

        public static readonly Kernel LeftPrewittEdgeDetect3 =
            new(new[] { 1f, 0, -1, 1, 0, -1, 1, 0, -1 }, KernelCreationFlags.None, 3);

        public static readonly Kernel RightPrewittEdgeDetect3 =
            new(new[] { -1f, 0, 1, -1, 0, 1, -1, 0, 1 }, KernelCreationFlags.None, 3);

        public static readonly Kernel Emboss3 = new(new[] { -2f, -1, 0, -1, 1, 1, 0, 1, 2 },
            KernelCreationFlags.None, 3);

        public static readonly Kernel SharpenStrengthOne3 =
            new(new[] { 0f, -1, 0, -1, 5, -1, 0, -1, 0 }, KernelCreationFlags.None, 3);

        public static readonly Kernel OutlineEdgeDetect3 =
            new(new[] { 1f, 1, 1, 1, -8, 1, 1, 1, 1 }, KernelCreationFlags.None, 3);

        public static readonly Kernel Ones3 = new(Ones3Arr, KernelCreationFlags.None, 3);
    }
}