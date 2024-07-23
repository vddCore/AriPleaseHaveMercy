namespace AriPleaseHaveMercy.Logic.Simulation;

using System.Drawing;
using System.Numerics;
using Chroma.Graphics;
using Color = Chroma.Graphics.Color;

public class World(Size size)
{
    private readonly List<Body> _bodies = [];
    
    public Size Size { get; } = size;
    public Vector2 Center => new(Size.Width / 2, Size.Height / 2);
    
    public IReadOnlyList<Body> Bodies => _bodies;

    public float Gravity { get; set; } = 6.674f;
    public float BounceDamping { get; set; } = 3.25f;
    public float TimeScale { get; set; } = 1f;

    public event EventHandler<Body>? BodyCollidedWithWall; 

    public void Draw(RenderContext context)
    {
        for (var i = 0; i < _bodies.Count; i++)
        {
            _bodies[i].Draw(context);
        }
    }

    public void Update(float delta)
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
        }
        
        for (var i = 0; i < _bodies.Count; i++)
        {
            _bodies[i].Move(delta * TimeScale);
        }
        
        for (var i = 0; i < _bodies.Count; i++)
        {   
            if (_bodies[i].CollidesWithWorldEdge(out var edges))
                _bodies[i].ResolveWallCollision(edges);
            
            for (var j = 0; j < _bodies.Count; j++)
            {
                if (j == i) continue;

                if (_bodies[i].CollidesWith(_bodies[j], out var depth)) 
                    _bodies[i].ResolveBodyCollision(_bodies[j], depth);
            }
        }
    }
    
    public Body CreateRandomBody()
    {
        var mass = Random.Shared.Next(50, 100);
        
        var b = new Body(this)
        {
            Position = new Vector2(
                Random.Shared.Next(20, Size.Width - 20),
                Random.Shared.Next(20, Size.Height - 20)
            ),
            Mass = mass,
            Radius = mass / 7,
            Color = new Color(
                (byte)Random.Shared.Next(0, 255),
                (byte)Random.Shared.Next(0, 255),
                (byte)Random.Shared.Next(0, 255),
                (byte)127
            )
        };

        _bodies.Add(b);
        return b;
    }

    public Body CreateBody()
    {
        var b = new Body(this);
        _bodies.Add(b);
        return b;
    }
}
