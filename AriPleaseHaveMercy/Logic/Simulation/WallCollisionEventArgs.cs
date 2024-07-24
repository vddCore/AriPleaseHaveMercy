namespace AriPleaseHaveMercy.Logic.Simulation;

using System.Numerics;

public class WallCollisionEventArgs(Body body, WorldEdge edge, float penetration, Vector2 collisionPoint)
{
    public Body Body { get; } = body;
    public WorldEdge Edge { get; } = edge;
    public float Penetration { get; } = penetration;
    public Vector2 CollisionPoint { get; } = collisionPoint;
}