namespace AriPleaseHaveMercy.Logic.Graphics;

using System.Numerics;
using Chroma.Graphics;
using Chroma.Graphics.Accelerated;

public class Blur
{
    private readonly Effect _effect;
    
    public Texture? SourceTexture { get; set; }
    public RenderTarget? CurrentBuffer { get; set; }

    public int Iterations { get; set; } = 5;
    public bool Horizontal { get; set; }

    public Blur(Effect effect)
    {
        _effect = effect;
    }
    
    public void Render(RenderContext context)
    {
        if (SourceTexture == null)
            return;
        
        CurrentBuffer?.Dispose();
        CurrentBuffer = new RenderTarget(SourceTexture.Width, SourceTexture.Height);
        
        context.RenderTo(CurrentBuffer, (c, t) =>
        {
            _effect.Activate();
            c.DrawTexture(SourceTexture, Vector2.Zero);
            Shader.Deactivate();
        });   
        
        for (var i = 0; i < Iterations - 1; i++)
        {
            context.RenderTo(CurrentBuffer, (c, _) =>
            {
                _effect.Activate();
                _effect.SetUniform("gauss_horizontal", Horizontal);
                c.DrawTexture(CurrentBuffer, Vector2.Zero);
                Shader.Deactivate();
            });

            Horizontal = !Horizontal;
        }

        context.DrawTexture(CurrentBuffer, Vector2.Zero);
    }
}