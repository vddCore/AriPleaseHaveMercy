namespace AriPleaseHaveMercy.Logic.Simulation;

using System.Numerics;

public partial class CollisionResolvers
{
    public static void DefaultBodyBounceResponse(Body a, Body b, Vector2 collisionDepth)
    {
        var sumOfRadii = a.Radius + b.Radius;

        var x = a.Position.X - b.Position.X;
        var y = a.Position.Y - b.Position.Y;

        var distanceSquared = x * x + y * y;
        var distance = MathF.Sqrt(distanceSquared);

        var separation = sumOfRadii - distance;
        var unit = (a.Position - b.Position) / MathF.Sqrt(distanceSquared);

        a.Position += unit * (separation / 2);
        b.Position -= unit * (separation / 2);

        a.Velocity -= unit * (b.Mass / b.World.BounceDamping);
        b.Velocity -= unit * (a.Mass / b.World.BounceDamping);
    }
    
    public static void DefaultWallBounceResponse(Body body, WorldEdge edges)
    {
        var world = body.World;
        
        if (edges.HasFlag(WorldEdge.Top))
        {
            var bounceBack = MathF.Abs(body.Velocity.Y) / world.BounceDamping;

            body.Velocity = body.Velocity with { Y = bounceBack };
            body.Position = body.Position with { Y = body.Radius + 1 };
            body.Acceleration = body.Acceleration with { Y = MathF.Abs(body.Acceleration.Y) / (world.BounceDamping * 2f) };
        }
        else if (edges.HasFlag(WorldEdge.Bottom))
        {
            var bounceBack = -body.Velocity.Y / world.BounceDamping;

            body.Velocity = body.Velocity with { Y = bounceBack };
            body.Position = body.Position with { Y = world.Size.Height - body.Radius - 1 };
            body.Acceleration = body.Acceleration with { Y = -body.Acceleration.Y / (world.BounceDamping * 2f) };
        }

        if (edges.HasFlag(WorldEdge.Left))
        {
            var bounceBack = MathF.Abs(body.Velocity.X) / world.BounceDamping;

            body.Velocity = body.Velocity with { X = bounceBack };
            body.Position = body.Position with { X = body.Radius + 1 };
            body.Acceleration = body.Acceleration with { X = MathF.Abs(body.Acceleration.X) / (world.BounceDamping * 2f) };
        }
        else if (edges.HasFlag(WorldEdge.Right))
        {
            var bounceBack = -body.Velocity.X / world.BounceDamping;

            body.Velocity = body.Velocity with { X = bounceBack };
            body.Position = body.Position with { X = world.Size.Width - body.Radius - 1 };
            body.Acceleration = body.Acceleration with { X = -body.Acceleration.X / (world.BounceDamping * 2f) };
        }
    }
}