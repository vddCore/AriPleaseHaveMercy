namespace AriPleaseHaveMercy.Logic.Simulation;

using System.Numerics;
using Chroma.Graphics;

public class Body(World world)
{
    private Color _color = Color.Lime;
    
    public Vector2 Position;
    public Vector2 PreviousPosition;
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

        if (World.MaximumForce != null)
        {
            var sign = MathF.Sign(force);

            if (force > World.MaximumForce.Value)
                force = sign * World.MaximumForce.Value;
        }

        Jolt((by.Position - Position) * force);
    }

    public void Jolt(Vector2 vector)
        => Acceleration += vector;

    public void Accelerate(Vector2 vector)
    {
        Velocity.X += vector.X;
        Velocity.Y += vector.Y;

        if (World.MaximumBodyVelocityX != null)
        {
            if (MathF.Abs(Velocity.X) > World.MaximumBodyVelocityX)
                Velocity.X = World.MaximumBodyVelocityX.Value * MathF.Sign(Velocity.X);
        }
        
        if (World.MaximumBodyVelocityY != null)
        {
            if (MathF.Abs(Velocity.Y) > World.MaximumBodyVelocityY)
                Velocity.Y = World.MaximumBodyVelocityY.Value * MathF.Sign(Velocity.Y);
        }
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

    public void ResolveWallCollision(Vector2 collisionPoint, WorldEdge edges)
        => WallCollisionResolver?.Invoke(this, collisionPoint, edges);

    public bool CollidesWithWorldEdge(out Vector2 collisionPoint, out float penetration, out WorldEdge edges)
    {
        edges = WorldEdge.None;
        collisionPoint = Vector2.Zero;
        penetration = 0;

        if (Position.X - Radius <= 0)
        {
            edges |= WorldEdge.Left;

            var difference = Radius - Position.X;
            collisionPoint = Position with { X = Position.X + difference - Radius };
            penetration = MathF.Abs(Acceleration.X / World.BounceDamping);
        }

        if (Position.Y - Radius <= 0)
        {
            edges |= WorldEdge.Top;

            var difference = Radius - Position.Y;
            collisionPoint = Position with { Y = Position.Y + difference - Radius };
            penetration = MathF.Abs(Acceleration.Y / World.BounceDamping);
        }

        if (Position.X + Radius >= World.Size.Width)
        {
            edges |= WorldEdge.Right;

            var difference = Position.X + Radius - World.Size.Width;
            collisionPoint = Position with { X = Position.X - difference + Radius };
            penetration = MathF.Abs(Acceleration.X / World.BounceDamping);
        }

        if (Position.Y + Radius >= World.Size.Height)
        {
            edges |= WorldEdge.Bottom;
            
            var difference = Position.Y + Radius - World.Size.Height;
            collisionPoint = Position with { Y = Position.Y - difference + Radius };
            penetration = MathF.Abs(Acceleration.Y / World.BounceDamping);
        }

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