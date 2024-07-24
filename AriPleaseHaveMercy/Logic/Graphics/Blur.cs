namespace AriPleaseHaveMercy.Logic.Graphics;

using System.Numerics;
using Chroma.Graphics;
using Chroma.Graphics.Accelerated;

public class Blur
{
    private readonly Effect _effect;
    private Texture? _sourceTexture;

    public Texture? SourceTexture
    {
        get => _sourceTexture;
        set
        {
            _sourceTexture = value;

            if (_sourceTexture != null)
            {
                CurrentBuffer = new RenderTarget(_sourceTexture.Width, _sourceTexture.Height);
            }
            else
            {
                CurrentBuffer?.Dispose();
                CurrentBuffer = null;
            }
        }
    }

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

        for (var i = 0; i < Iterations + 1; i++)
        {
            if (i == 0)
            {
                context.RenderTo(CurrentBuffer, (c, _) =>
                {
                    c.Clear(Color.Transparent);
                    c.DrawTexture(SourceTexture, Vector2.Zero);
                });
            }
            else
            {
                context.RenderTo(CurrentBuffer, (c, _) =>
                {
                    _effect.Activate();
                    _effect.SetUniform("gauss_horizontal", Horizontal);
                    c.DrawTexture(CurrentBuffer, Vector2.Zero);
                });
            }

            Horizontal = !Horizontal;
        }

        Shader.Deactivate();
        context.DrawTexture(CurrentBuffer, Vector2.Zero);
    }
}