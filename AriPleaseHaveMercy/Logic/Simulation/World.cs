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
    public float BounceDamping { get; set; } = 3.25f;
    public float TimeScale { get; set; } = 1f;
    public float? MaximumForce { get; set; } = 0.0013f;
    public float? MaximumBodyVelocityX { get; set; } = 0.099f;
    public float? MaximumBodyVelocityY { get; set; } = 0.099f;

    public event EventHandler<Body>? BodyCollidedWithWall; 

    public void Draw(RenderContext context)
    {
        for (var i = 0; i < _bodies.Count; i++)
        {
            _bodies[i].Draw(context);
        }
    }

    public void FixedUpdate(float delta)
    {
        for (var i = 0; i < _bodies.Count; i++)
        {
            for (var j = 0; j < _bodies.Count; j++)
            {
                if (j == i) continue;
                
                _bodies[j].Affect(_bodies[i], Gravity * TimeScale);
            }    
        }
        
        for (var i = 0; i < _bodies.Count; i++)
        {
            _bodies[i].Accelerate(_bodies[i].Acceleration * TimeScale);
            _bodies[i].Move(delta * TimeScale);

        }
        
        for (var i = 0; i < _bodies.Count; i++)
        {
            if (_bodies[i].CollidesWithWorldEdge(out var edges))
            {
                BodyCollidedWithWall?.Invoke(this, _bodies[i]);
                _bodies[i].ResolveWallCollision(edges);
            }

            for (var j = 0; j < _bodies.Count; j++)
            {
                if (j == i) continue;

                if (_bodies[i].CollidesWith(_bodies[j], out var depth)) 
                    _bodies[i].ResolveBodyCollision(_bodies[j], depth);
            }
        }
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
