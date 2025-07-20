using System.Numerics;
using FlameFishLib.Units;
namespace FlameFishLib;

public class Transform
{
    public Vector2 translation;
    public Angle rotation;

    public float X {
        get => translation.X;
        set => translation.X = value;
    }

    public float Y {
        get => translation.Y;
        set => translation.Y = value;
    }

    public Transform(Vector2 translation, Angle rotation) {
        this.translation = translation;
        this.rotation = rotation;
    }

    public Transform() 
        : this(new Vector2(), new Angle()) {}

    public Transform(float x, float y, Angle rotation) 
        : this(new Vector2(x, y), rotation) {}

    public Transform(double x, double y, Angle rotation) 
        : this((float) x, (float) y, rotation) {}

    public override string ToString()
    {
        return $"<X: {this.X}, Y: {this.Y}, θ: {this.rotation.Degrees}°";
    }

    public Transform Add(Transform other) {
        return new(
            this.translation + other.translation,
            this.rotation + other.rotation
        );
    }

    public Transform Sub(Transform other) {
        return new(
            this.translation - other.translation,
            this.rotation - other.rotation
        );
    }

    public static Transform operator +(Transform a, Transform b) => a.Add(b);
    public static Transform operator -(Transform a, Transform b) => a.Sub(b);
    public static Transform operator +(Transform a, Vector2 b) => new(a.translation + b, a.rotation);
    public static Transform operator -(Transform a, Vector2 b) => new(a.translation - b, a.rotation);
}