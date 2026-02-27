using System;
using System.Numerics;

namespace MdCsg.Showcase;

/// <summary>Orbit camera around a target point.</summary>
internal class Camera
{
    public Vector3 Target = Vector3.Zero;
    public float Azimuth = 0.4f;
    public float Elevation = 0.3f;
    public float Distance = 5.0f;
    public float FieldOfView = MathF.PI / 4f;
    public float NearPlane = 0.01f;
    public float FarPlane = 1000f;

    public Vector3 Eye
    {
        get
        {
            var ce = MathF.Cos(Elevation);
            return Target + Distance * new Vector3(
                ce * MathF.Cos(Azimuth),
                MathF.Sin(Elevation),
                ce * MathF.Sin(Azimuth));
        }
    }

    public Matrix4x4 ViewMatrix => Matrix4x4.CreateLookAt(Eye, Target, Vector3.UnitY);

    public Matrix4x4 ProjectionMatrix(float aspectRatio)
        => Matrix4x4.CreatePerspectiveFieldOfView(FieldOfView, aspectRatio, NearPlane, FarPlane);

    public void Rotate(float dAzimuth, float dElevation)
    {
        Azimuth += dAzimuth;
        Elevation = System.Math.Clamp(Elevation + dElevation, -MathF.PI / 2f + 0.01f, MathF.PI / 2f - 0.01f);
    }

    public void Zoom(float delta)
    {
        Distance = MathF.Max(0.1f, Distance * (1f - delta * 0.1f));
    }

    public void Pan(float dx, float dy)
    {
        var v = ViewMatrix;
        var right = new Vector3(v.M11, v.M21, v.M31);
        var up = new Vector3(v.M12, v.M22, v.M32);
        Target += (right * dx + up * dy) * Distance * 0.002f;
    }

    public void FitBounds(Vector3 min, Vector3 max)
    {
        Target = (min + max) * 0.5f;
        Distance = (max - min).Length() * 1.2f;
    }
}
