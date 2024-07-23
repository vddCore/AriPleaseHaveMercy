namespace AriPleaseHaveMercy.Logic.Simulation;

[Flags]
public enum WorldEdge : byte
{
    None = 0,
    Left = 1 << 0,
    Top = 1 << 1,
    Right = 1 << 2,
    Bottom = 1 << 3
}