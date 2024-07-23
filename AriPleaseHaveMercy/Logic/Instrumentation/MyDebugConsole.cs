namespace AriPleaseHaveMercy.Logic.Instrumentation;

using Chroma.Commander;
using Chroma.Diagnostics;
using Chroma.Graphics;
using Chroma.Graphics.TextRendering.TrueType;
using Chroma.Windowing;

public class MyDebugConsole : DebugConsole
{
    public MyDebugConsole(Window window, int maxLines = 20) 
        : base(window, maxLines)
    {
    }

    protected override void DrawBackdrop(RenderContext context)
    {
        base.DrawBackdrop(context);

        var label = $"FPS: {PerformanceCounter.FPS}";
        var measure = TrueTypeFont.Default.Measure(label);
        
        context.DrawString(
            label,
            Dimensions.Width - measure.Width - 8,
            8,
            Color.HotPink
        );
    }
}