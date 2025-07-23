using System.Text.RegularExpressions;
using System;

namespace FlameFishLib.Units;

public readonly struct Angle {
    private readonly double rotationDegrees;

    // Properties are nice
    public double Degrees => rotationDegrees;
    public double Radians => DegreesToRadians(Degrees);

    private Angle(double degrees) {
        this.rotationDegrees = degrees;
    }

    public Angle() : this(0) {}

    // Using UpperCamelCase for methods feels wrong
    public static Angle FromDegrees(double degrees) {
        return new Angle(degrees);
    }

    public static Angle FromRadians(double radians) {
        return new Angle(RadiansToDegrees(radians));
    }

    public static double DegreesToRadians(double degrees) {
        return degrees * (Math.PI / 180);
    }

    public static double RadiansToDegrees(double radians) {
        return radians * (180 / Math.PI);
    }

    public Angle Add(Angle other)
    {
        return new Angle(rotationDegrees + other.rotationDegrees);
    }

    public Angle Subtract(Angle other)
    {
        return new Angle(rotationDegrees - other.rotationDegrees);
    }

    public Angle Multiply(double scalar)
    {
        return new Angle(rotationDegrees * scalar);
    }

    public Angle Divide(double denominator) {
        return new Angle(rotationDegrees / denominator);
    }

    public Angle Invert()
    {
        return new Angle(-rotationDegrees);
    }

    public Angle Normalize360()
    {
        double deg = rotationDegrees % 360;
        if (deg < 0) deg += 360;
        return new Angle(deg);
    }

    public Angle Normalize180()
    {
        double deg = Normalize360().rotationDegrees;
        if (deg > 180) deg -= 360;
        return new Angle(deg);
    }

    public Angle MinimalAngleTo(Angle target)
    {
        double diff = (target.rotationDegrees - rotationDegrees) % 360;
        if (diff < -180) diff += 360;
        if (diff > 180) diff -= 360;
        return FromDegrees(diff);
    }

    public Angle RotateTowards(Angle target, double maxDegrees)
    {
        double turn = MinimalAngleTo(target).Degrees;
        if (Math.Abs(turn) <= maxDegrees)
            return target;
        return new Angle(rotationDegrees + Math.Sign(turn) * maxDegrees);
    }

    // Yippee!
    public static Angle operator +(Angle a, Angle b) => a.Add(b);
    public static Angle operator -(Angle a, Angle b) => a.Subtract(b);
    public static Angle operator *(Angle a, double s) => a.Multiply(s);
    public static Angle operator /(Angle a, double d) => a.Divide(d);
}
