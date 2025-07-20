using System.Numerics;
using FlameFishLib.Units;

namespace FlameFishLib;

public static class MiscUtil {
    public static Vector2 VectorFromPolar(Angle angle, float magnitude) {
        return new Vector2(
            MathF.Cos((float) angle.Radians), 
            MathF.Sin((float) angle.Radians)
        ) * (float) magnitude;
    }
}