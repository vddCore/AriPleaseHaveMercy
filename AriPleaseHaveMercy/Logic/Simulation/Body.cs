namespace AriPleaseHaveMercy.Logic.Simulation;

using System.Numerics;
using Chroma.Graphics;



public class Body(World world)
{
    private Color _color = Color.Lime;
    
    public Vector2 Position;
    public Vector2 Acceleration;
    public Vector2 Velocity;
    public float Radius;
    public float Mass;

    public World World { get; } = world;

    public Color Color
    {
        get => _color;
        set
        {
            _color = value;
            StrokeColor = Color.FromHSV(_color.Hue, _color.Saturation);
        }
    }

    public Color StrokeColor { get; private set; }

    public float Density => Mass / (4 / 3f * MathF.PI * float.Pow(Radius, 3));

    public float? MaxForce { get; set; } = 0.0013f;
    public Vector2? MaxVelocity { get; set; } = new(0.099f);

    public bool IsStationary { get; set; }

    public BodyCollisionResolver? BodyCollisionResolver { get; set; } = CollisionResolvers.FunkyBodyCollisionResolver;
    public WallCollisionResolver? WallCollisionResolver { get; set; } = CollisionResolvers.DefaultWallBounceResponse;

    public void Draw(RenderContext context)
    {
        context.Circle(ShapeMode.Fill, Position, Radius, Color);
        RenderSettings.LineThickness = 1;
        context.Circle(ShapeMode.Stroke, Position, Radius, StrokeColor);
    }

    public void Affect(Body by, float gravity)
    {
        var force = gravity * (by.Mass * Mass) / MathF.Pow(by.DistanceTo(this), 2);

        if (MaxForce != null)
        {
            if (float.IsNaN(force))
                force = 0;

            var sign = MathF.Sign(force);

            if (force > MaxForce.Value)
                force = sign * MaxForce.Value;
        }

        Jolt((by.Position - Position) * force);
    }

    public void Jolt(Vector2 vector)
        => Acceleration += vector;

    public void Accelerate(Vector2 vector)
    {
        var (vx, vy) = (Velocity.X, Velocity.Y);
        vx += vector.X;
        vy += vector.Y;
        
        if (MaxVelocity != null)
        {
            if (MathF.Abs(vx) > MaxVelocity.Value.X)
                vx = MaxVelocity.Value.X * MathF.Sign(vx);

            if (MathF.Abs(vy) > MaxVelocity.Value.Y)
                vy = MaxVelocity.Value.Y * MathF.Sign(vy);
        }

        Velocity = new(vx, vy);
    }

    public void Move(float dt)
    {
        if (IsStationary)
            return;

        Position += (Velocity + Acceleration) * dt;
    }

    public float DistanceTo(Body other)
        => Vector2.DistanceSquared(Position, other.Position);

    public bool CollidesWith(Body other, out Vector2 depth)
    {
        depth = Vector2.Zero;

        var r = Radius + other.Radius;

        var distance = Vector2.Distance(Position, other.Position);

        if (r > distance)
        {
            var difference = r - distance;

            depth.X = MathF.Cos(difference);
            depth.Y = MathF.Sin(difference);
        }

        return depth != Vector2.Zero;
    }

    public void ResolveBodyCollision(Body b, Vector2 collisionDepth)
        => BodyCollisionResolver?.Invoke(this, b, collisionDepth);

    public void ResolveWallCollision(WorldEdge edges)
        => WallCollisionResolver?.Invoke(this, edges);

    public bool CollidesWithWorldEdge(out WorldEdge edges)
    {
        edges = WorldEdge.None;

        if (Position.X - Radius <= 0)
            edges |= WorldEdge.Left;

        if (Position.Y - Radius <= 0)
            edges |= WorldEdge.Top;

        if (Position.X + Radius >= World.Size.Width)
            edges |= WorldEdge.Right;

        if (Position.Y + Radius >= World.Size.Height)
            edges |= WorldEdge.Bottom;

        return edges != WorldEdge.None;
    }

    private Vector2 ClosestPointOnLine(Vector2 l1, Vector2 l2, Vector2 p)
    {
        var a1 = l2.Y - l1.Y;
        var b1 = l1.X - l2.X;
        var c1 = (l2.Y - l1.Y) * l1.X + (l1.X - l2.X) * l1.Y;
        var c2 = -b1 * p.X + a1 * p.Y;
        var det = a1 * a1 - -b1 * b1;

        if (det != 0)
        {
            return new Vector2(
                (a1 * c1 - b1 * c2) / det,
                (a1 * c2 - -b1 * c1) / det
            );
        }
        else
        {
            return p;
        }
    }
}