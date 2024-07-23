namespace AriPleaseHaveMercy;

using System.Numerics;
using AriPleaseHaveMercy.Logic.Graphics;
using AriPleaseHaveMercy.Logic.Simulation;
using Chroma;
using Chroma.Commander;
using Chroma.ContentManagement;
using Chroma.Graphics;
using Chroma.Graphics.Accelerated;
using Chroma.Input;

public class AppCore() : Game(new(false, false, 8))
{
    private DebugConsole? _console;
    private World? _world;

    private Blur _blur = null!;
    private RenderTarget _target = null!;
    private Effect _effect = null!;

    [ConsoleVariable("phy_tscale", Description = "Controls how fast the simulation unfolds.")]
    public float TimeScale
    {
        get => _world!.TimeScale;
        set => _world!.TimeScale = value;
    }
    
    protected override void Initialize(IContentProvider content)
    {
        Window.Mode.SetWindowed(1200, 800, true);
        _console = new DebugConsole(Window);

        _target = new RenderTarget(Window.Size);
        _effect = content.Load<Effect>("shaders/gauss.glsl");
        _blur = new Blur(_effect)
        {
            SourceTexture = _target,
            Iterations = 8
        };

        RenderSettings.MultiSamplingEnabled = true;
        FixedTimeStepTarget = 165;

        _world = new World(Window.Size);
        _world.BodyCollidedWithWall += World_BodyCollidedWithWall;

        for (var i = 0; i < 150; i++)
            _world.CreateRandomBody();

        _console.RegisterInstanceEntities(this);
    }

    private void World_BodyCollidedWithWall(object? sender, Body e)
    {
        e.Color = new Color(
            (byte)Random.Shared.Next(0, 255),
            (byte)Random.Shared.Next(0, 255),
            (byte)Random.Shared.Next(0, 255),
            (byte)100
        );
    }

    protected override void Update(float delta)
        => _console?.Update(delta);

    protected override void FixedUpdate(float delta)
        => _world?.Update(delta);

    protected override void Draw(RenderContext context)
    {
        context.RenderTo(
            _target,
            (c, _) =>
            {
                c.Clear(Color.Transparent);
                _world?.Draw(c);
            }
        );

        _blur.Render(context);
        context.DrawTexture(_target, Vector2.Zero);
        
        _console?.Draw(context);
    }

    protected override void KeyPressed(KeyEventArgs e)
        => _console?.KeyPressed(e);

    protected override void TextInput(TextInputEventArgs e)
        => _console?.TextInput(e);

    protected override void WheelMoved(MouseWheelEventArgs e)
        => _console?.WheelMoved(e);
}