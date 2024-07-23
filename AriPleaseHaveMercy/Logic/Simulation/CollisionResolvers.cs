namespace AriPleaseHaveMercy.Logic.Simulation;

using System.Numerics;

public delegate void WallCollisionResolver(Body a, WorldEdge edge);
public delegate void BodyCollisionResolver(Body a, Body b, Vector2 collisionDepth); 

public partial class CollisionResolvers
{
    public static void FunkyBodyCollisionResolver(Body a, Body b, Vector2 depth)
    {
        var unit = (a.Position - b.Position) / a.DistanceTo(b) * depth.Length();
        
        a.Position += a.Radius * unit;
        b.Position -= b.Radius * unit;
        
        a.Acceleration += b.Mass * unit;
        b.Acceleration -= a.Mass * unit;
        
        a.Velocity += b.Mass * unit;
        b.Velocity -= a.Mass * unit;
        

    }
}