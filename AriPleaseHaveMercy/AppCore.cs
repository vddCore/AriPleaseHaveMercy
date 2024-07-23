namespace AriPleaseHaveMercy;

using System.Numerics;
using AriPleaseHaveMercy.Logic;
using AriPleaseHaveMercy.Logic.Graphics;
using AriPleaseHaveMercy.Logic.Instrumentation;
using AriPleaseHaveMercy.Logic.Simulation;
using Chroma;
using Chroma.Commander;
using Chroma.Commander.Expressions;
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
    public float PhysicsTimeScale
    {
        get => _world!.TimeScale;
        set => _world!.TimeScale = value;
    }

    [ConsoleVariable("phy_maxvel_x", Description = "Controls the maximum X velocity bodies can reach.")]
    public float PhysicsMaximumBodyVelocityX
    {
        get => _world!.MaximumBodyVelocityX ?? 0;
        set => _world!.MaximumBodyVelocityX = value;
    }

    [ConsoleVariable("phy_maxvel_y", Description = "Controls the maximum X velocity bodies can reach.")]
    public float PhysicsMaximumBodyVelocityY
    {
        get => _world!.MaximumBodyVelocityY ?? float.NaN;
        set => _world!.MaximumBodyVelocityY = value;
    }

    [ConsoleVariable("phy_maxforce", Description = "Controls the maximum force that gravity can impose on bodies.")]
    public float PhysicsMaximumForce
    {
        get => _world!.MaximumForce ?? float.NaN;
        set => _world!.MaximumForce = value;
    }

    [ConsoleVariable("phy_gravity", Description = "Controls the gravitational constant.")]
    public float PhysicsGravity
    {
        get => _world!.Gravity;
        set => _world!.Gravity = value;
    }

    [ConsoleVariable("gen_min_mass", Description = "Controls the minimum mass of randomly generated bodies.")]
    public float GeneratorMinimumMass { get; set; } = 100;
    
    [ConsoleVariable("gen_max_mass", Description = "Controls the minimum mass of randomly generated bodies.")]
    public float GeneratorMaximumMass { get; set; } = 1000;

    [ConsoleVariable("gen_radius_factor", Description = "Controls the mass-based radius scaling factor for randomly generated bodies.")]
    public float GeneratorRadiusScaleFactor { get; set; } = 0.002f;

    [ConsoleVariable("gen_body_count", Description = "Controls how many bodies to generate on simulation reset.")]
    public int GeneratorBodyCount { get; set; } = 100;

    [ConsoleVariable("gfx_bloom_iterations", Description = "Controls intensity of bloom effect.")]
    public int GraphicsBloomIterations
    {
        get => _blur.Iterations;
        set => _blur.Iterations = value;
    }

    protected override void Initialize(IContentProvider content)
    {
        RenderSettings.MultiSamplingEnabled = true;
        RenderSettings.ShapeBlendingEnabled = true;

        Window.Mode.SetWindowed(1200, 800, true);
        _console = new MyDebugConsole(Window);
        _console.Theme.BackgroundColor = Color.HotPink with { A = 25 };
        _console.Theme.BorderColor = Color.Magenta;

        _console.ConsoleVariableChanged += Console_ConsoleVariableChanged;

        _target = new RenderTarget(Window.Size);
        _effect = content.Load<Effect>("shaders/gauss.glsl");
        _blur = new Blur(_effect)
        {
            SourceTexture = _target,
            Iterations = 8
        };

        FixedTimeStepTarget = 165;

        _world = new World(Window.Size);
        _world.BodyCollidedWithWall += World_BodyCollidedWithWall;

        _console.RegisterStaticEntities();
        _console.RegisterInstanceEntities(this);
    }

    protected override void Update(float delta)
        => _console?.Update(delta);

    protected override void FixedUpdate(float delta)
        => _world?.FixedUpdate(delta);

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

    private void InitializeBodyPropertiesRandom(Body b)
    {
        var mass = GeneratorMinimumMass + GeneratorMaximumMass * Random.Shared.NextSingle();

        b.Position = new Vector2(
            Random.Shared.Next(20, b.World.Size.Width - 20),
            Random.Shared.Next(20, b.World.Size.Height - 20)
        );

        b.Mass = mass;
        b.Radius = mass * GeneratorRadiusScaleFactor;

        b.Color = new Color(
            (byte)Random.Shared.Next(0, 255),
            (byte)Random.Shared.Next(0, 255),
            (byte)Random.Shared.Next(0, 255),
            (byte)127
        );
    }

    private void SpawnBodies()
    {
        for (var i = 0; i < GeneratorBodyCount; i++)
            _world!.CreateBody(InitializeBodyPropertiesRandom);
    }

    private void Reset()
    {
        _world?.Reset();
        SpawnBodies();
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

    private void Console_ConsoleVariableChanged(object? sender, ConsoleVariableEventArgs e)
    {
        switch (e.Variable.ManagedMemberName)
        {
            case nameof(GeneratorMaximumMass):
            case nameof(GeneratorMinimumMass):
            case nameof(GeneratorRadiusScaleFactor):
            case nameof(GeneratorBodyCount):
                Reset();
                break;
        }
    }

    [ConsoleCommand("sim_reset", Description = "Re-generates the simulation.")]
    private void ResetSimulation(DebugConsole _, params ExpressionValue[] __)
        => Reset();
}