namespace AriPleaseHaveMercy.Logic.Graphics;

using System.Numerics;
using AriPleaseHaveMercy.Logic.Simulation;
using Chroma.Graphics;
using Color = Chroma.Graphics.Color;

public class HitVisual(float ttl, Texture texture, Vector2 position, WorldEdge edge, float radius, Color color)
{
    public const int MaxTTL = 100;

    public float TTL { get; private set; } = ttl;
    private Texture _texture = texture;
    private Vector2 _position = position;

    private float _rotation = edge switch
    {
        WorldEdge.Bottom => 0,
        WorldEdge.Left => 90,
        WorldEdge.Top => 180,
        WorldEdge.Right => 270,
        _ => 0
    };

    private float _radius = radius;
    private Color _color = color;

    private float _mirror = edge == WorldEdge.Top ? -1 : 1;

    public void Update(float delta)
    {
        TTL -= 50 * delta;
    }

    public void Draw(RenderContext context)
    {
        _texture.ColorMask = Color.FromHSV(_color.Hue) with { A = (byte)(140 * (TTL / MaxTTL)) };

        var scale = (_radius * new Vector2(0.008f)) * (TTL / MaxTTL);

        if (edge == WorldEdge.Bottom)
        {
            context.DrawTexture(
                _texture,
                _position,
                scale,
                new Vector2(_texture.Width / 2, _texture.Height),
                0
            );
        }
        else if (edge == WorldEdge.Top)
        {
            context.DrawTexture(
                _texture,
                _position - new Vector2(0, 8),
                scale * -1,
                new Vector2(_texture.Width / 2, _texture.Height),
                0
            );
        }

        if (edge == WorldEdge.Left)
        {
            context.DrawTexture(
                _texture,
                _position,
                scale,
                new Vector2(_texture.Width / 2, _texture.Height),
                90
            );
        }
        else if (edge == WorldEdge.Right)
        {
            context.DrawTexture(
                _texture,
                _position,
                scale * -1,
                new Vector2(_texture.Width / 2, _texture.Height),
                90
            );
        }
    }
}