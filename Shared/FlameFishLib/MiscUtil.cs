using System.Numerics;
using FlameFishLib.Units;
using Robocode.TankRoyale.BotApi;

namespace FlameFishLib;

public static class MiscUtil
{
    public static Vector2 VectorFromPolar(Angle angle, float magnitude)
    {
        return new Vector2(
            MathF.Cos((float)angle.Radians),
            MathF.Sin((float)angle.Radians)
        ) * (float)magnitude;
    }

    public static Transform TransformPosition(this BaseBot O)
    {
        return new Transform(O.X, O.Y, Angle.FromDegrees(O.Direction));
    }

    public static Vector2 VectorPosition(this BaseBot O)
    {
        return new Vector2((float)O.X, (float)O.Y);
    }
}