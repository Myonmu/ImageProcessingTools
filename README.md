# ImageProcessingTools

A simple image processing library for Unity, does very basic things (convolution/filtering and arithmetic operations). Built for Edit-time use but compatible for runtime. However, runtime performance is not guaranteed in general. The library is used in production.

# Installation

Nothing special, just download and put the source code in the project (anywhere in the Assets folder). The library uses `Resources.Find` to load compute shaders, which isn't exactly a runtime-friendly solution. You may want to replace it with your project-specific resource loading mechanism if you wish to use it in runtime.

# Usage

You should be able to find the essentials accessible from `ImageProcessing` static class.

## Morphology

There are plenty of variants for morphology operations, categorized by what we are operating on (`Texture2D` or `RenderTexture`), whether the operation writes to a separate texture or is done "in-place" (write back to the source texture), and the number of operations to apply.

The naming of the methods follows this convention:

  Process{TypeOfResource}{UseComputeShader(CS)?}{MultiPass?}{Inplace? or NoAlloc?}

For example, manipulating a RenderTexture with multiple passes in-place would be `ProcessRenderTextureMultiPassInPlace`.

For most simple operations, you need to supply a `Kernel`. Common Kernels like Gaussian blur, edge detect, etc, can be found in `CommonKernels` static class.

An in-place Gaussian blur can be written as this:

```csharp
Texture2D tex = (...); // add texture loading or referencing here
ImageProcessing.ProcessTexture2DInPlace(tex, CommonKernels.GaussianBlur3);
```

Optionally, you could pass 3 other parameters:
- **KernelOperation** : what type of operation is performed, valid values are `Convolve`, `Dilate` and `Erode`. Default is `Convolve`. This parameter changes how shader calculates the result value, with `Convolve` summing `kernalValue * pixelValue`, `Dilate` propagates visible pixel colors, and `Erode` propagates empty pixels.
- **kernel offset (int)** : offset applied to non-center kernel cells. Default value is 1. When sampling, the effective pixel for a kernel cell at $(x,y)$ relative to the center of the kernel, would be $(x\*kernelOffset,y\*kernelOffset)$.
- **repeat times (int)** : how many times should the operation be executed. Default value is 1. You could supply a larger value for effects like multi-pass blur.

You could also use the overload that accepts an `ImageProcessingPass`, which packs these parameters into a single object. 

**Multi-pass** processing is similar to single-pass, except it only accepts an `IEnumerable<ImageProcessingPass>`. This is also called an *Morphology Processing Pipeline*. Multi-pass processing executes passes in order. It optionally takes a `Func<int,int>` for offset lookup. Multi-pass processing keeps an internal counter, indicating which pass we are processing, and the offset of the current pass is obtained via culling the offset lookup function `offsetFunc.Invoke(counter)`. If none is specified, the offset lookup function is defaulted to return 1 regardless of counter value. Note that a pass that is executed multiple times will increase the counter per-execution: if you have a pass that executes gaussian blur 3 times, and then followed by another pass that does a simple dilate, then when we are executing the dilate, the internal counter is 3.

Some common pipeline could be found in `CommonPipelines`, namely, *Closing* and *Openning* operations.

### Example Use Cases:

1. 5-pass Gaussian blur with increasing kernel offset:
```csharp
ImageProcessing.ProcessRenderTextureMultiPass( src , dest , new ImageProcessingPass[]{
        new ImageProcessingPass(CommonKernels.GaussianBlur3, KernelOperation.Convolve, 5)
    }, (i)=>{return i;});
```

2. Filling holes
```cshap
ImageProcessing.ProcessTexture2DMultiPassInPlace(tex, CommonPipelines.Closing);
```

3. Eliminating rogue pixels
```cshap
ImageProcessing.ProcessTexture2DMultiPassInPlace(tex, CommonPipelines.Opening);
```

## Arithmetic Operations
(WIP)



