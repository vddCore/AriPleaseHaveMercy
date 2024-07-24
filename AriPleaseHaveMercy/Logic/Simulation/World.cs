namespace AriPleaseHaveMercy.Logic.Simulation;

using System.Drawing;
using System.Numerics;
using Chroma.Graphics;

public class World(Size size)
{
    private readonly List<Body> _bodies = [];
    
    public Size Size { get; } = size;
    public Vector2 Center => new(Size.Width / 2, Size.Height / 2);
    
    public IReadOnlyList<Body> Bodies => _bodies;

    public float Gravity { get; set; } = 6.674f;
    public float BounceDamping { get; set; } = 2.25f;
    public float TimeScale { get; set; } = 1f;
    public float? MaximumForce { get; set; } = 0.0753f;
    public float? MaximumBodyVelocityX { get; set; } = 0.199f;
    public float? MaximumBodyVelocityY { get; set; } = 0.199f;
    public bool IsCollisionDetectionEnabled { get; set; } = true;

    public event EventHandler<WallCollisionEventArgs>? BodyCollidedWithWall; 

    public void Draw(RenderContext context)
    {
        for (var i = 0; i < _bodies.Count; i++)
        {
            _bodies[i].Draw(context);
        }
    }

    public void Update(float delta)
    {
        var count = _bodies.Count;

        Parallel.For(0, count, i =>
        {
            var bodyA = _bodies[i];
            
            if (bodyA.CollidesWithWorldEdge(out var collisionPoint, out var penetration, out var edges))
            {
                BodyCollidedWithWall?.Invoke(this, new WallCollisionEventArgs(bodyA, edges, penetration, collisionPoint));
                bodyA.ResolveWallCollision(collisionPoint, edges);
            }
            
            for (var j = 0; j < count; j++)
            {
                if (j == i) continue;

                var bodyB = _bodies[j];
                bodyB.Affect(bodyA, Gravity);
                
                if (bodyB.CollidesWith(bodyA, out var depth))
                    bodyB.ResolveBodyCollision(bodyA, depth);
            }
        });

        Parallel.For(0, count, i =>
        {
            _bodies[i].Accelerate(_bodies[i].Acceleration * TimeScale);
        });

        Parallel.For(0, count, i =>
        {
            _bodies[i].Move(delta * TimeScale);
        });
    }

    public Body CreateBody(Action<Body> init)
    {
        var b = new Body(this);
        init(b);
        _bodies.Add(b);
        return b;
    }

    public void Reset()
    {
        _bodies.Clear();
        
        Gravity = 6.674f;
        BounceDamping = 3.25f;
        TimeScale = 1f;
    }
}
