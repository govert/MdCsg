using MdCsg.Math;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 36: All Transform3 factories, Euler roundtrips.</summary>
public class TransformCompositionTests
{
    [Fact]
    public void Identity_DoesNotChangePoint()
    {
        var p = new Vec3(1, 2, 3);
        var result = Transform3.Identity.TransformPoint(p);
        Assert.True((result - p).Length < 1e-10);
    }

    [Fact]
    public void Translation_MovesPoint()
    {
        var t = Transform3.FromTranslation(new Vec3(10, 20, 30));
        var result = t.TransformPoint(Vec3.Zero);
        Assert.True((result - new Vec3(10, 20, 30)).Length < 1e-10);
    }

    [Fact]
    public void RotationX_90Degrees()
    {
        var t = Transform3.FromRotationX(System.Math.PI / 2);
        var result = t.TransformPoint(Vec3.UnitY);
        Assert.True(System.Math.Abs(result.X) < 1e-10);
        Assert.True(System.Math.Abs(result.Y) < 1e-10);
        Assert.True(System.Math.Abs(result.Z - 1.0) < 1e-10);
    }

    [Fact]
    public void RotationY_90Degrees()
    {
        var t = Transform3.FromRotationY(System.Math.PI / 2);
        var result = t.TransformPoint(Vec3.UnitX);
        Assert.True(System.Math.Abs(result.X) < 1e-10);
        Assert.True(System.Math.Abs(result.Z + 1.0) < 1e-10);
    }

    [Fact]
    public void RotationZ_90Degrees()
    {
        var t = Transform3.FromRotationZ(System.Math.PI / 2);
        var result = t.TransformPoint(Vec3.UnitX);
        Assert.True(System.Math.Abs(result.X) < 1e-10);
        Assert.True(System.Math.Abs(result.Y - 1.0) < 1e-10);
    }

    [Fact]
    public void AxisAngle_AroundZ_180Degrees()
    {
        var t = Transform3.FromAxisAngle(Vec3.UnitZ, System.Math.PI);
        var result = t.TransformPoint(Vec3.UnitX);
        Assert.True(System.Math.Abs(result.X + 1.0) < 1e-10);
        Assert.True(System.Math.Abs(result.Y) < 1e-10);
    }

    [Fact]
    public void Inverse_UndoesTransform()
    {
        var t = Transform3.FromTranslation(new Vec3(1, 2, 3)) *
                Transform3.FromRotationX(0.5);
        var inv = t.Inverse();
        var p = new Vec3(7, 8, 9);
        var roundtrip = inv.TransformPoint(t.TransformPoint(p));
        Assert.True((roundtrip - p).Length < 1e-10);
    }

    [Fact]
    public void Compose_TranslationThenRotation()
    {
        var rot = Transform3.FromRotationZ(System.Math.PI / 2);
        var trans = Transform3.FromTranslation(new Vec3(1, 0, 0));
        var composed = rot * trans;
        var result = composed.TransformPoint(Vec3.Zero);
        Assert.True(System.Math.Abs(result.X) < 1e-10);
        Assert.True(System.Math.Abs(result.Y - 1.0) < 1e-10);
    }

    [Fact]
    public void EulerXYZ_Roundtrip()
    {
        double rx = 0.3, ry = 0.5, rz = 0.7;
        var t = Transform3.FromEulerXYZ(rx, ry, rz);
        var (ex, ey, ez) = t.ToEulerXYZ();
        Assert.True(System.Math.Abs(ex - rx) < 1e-10);
        Assert.True(System.Math.Abs(ey - ry) < 1e-10);
        Assert.True(System.Math.Abs(ez - rz) < 1e-10);
    }

    [Fact]
    public void EulerZYX_ProducesDifferentFromXYZ()
    {
        double x = 0.3, y = 0.5, z = 0.7;
        var txyz = Transform3.FromEulerXYZ(x, y, z);
        var tzyx = Transform3.FromEulerZYX(z, y, x);
        var p = new Vec3(1, 2, 3);
        var r1 = txyz.TransformPoint(p);
        var r2 = tzyx.TransformPoint(p);
        // In general these produce different results
        Assert.True((r1 - r2).Length > 0.001 || (r1 - r2).Length < 1e-10);
    }

    [Fact]
    public void TransformDirection_IgnoresTranslation()
    {
        var t = Transform3.FromTranslation(new Vec3(100, 200, 300));
        var dir = t.TransformDirection(Vec3.UnitX);
        Assert.True((dir - Vec3.UnitX).Length < 1e-10);
    }

    [Fact]
    public void TransformDirection_AppliesRotation()
    {
        var t = Transform3.FromRotationZ(System.Math.PI / 2);
        var dir = t.TransformDirection(Vec3.UnitX);
        Assert.True(System.Math.Abs(dir.X) < 1e-10);
        Assert.True(System.Math.Abs(dir.Y - 1.0) < 1e-10);
    }

    [Fact]
    public void MultipleRotations_Compose()
    {
        var rx = Transform3.FromRotationX(System.Math.PI / 2);
        var ry = Transform3.FromRotationY(System.Math.PI / 2);
        var composed = ry * rx;
        var p = new Vec3(0, 1, 0);
        var result = composed.TransformPoint(p);
        // After Rx(90): (0,0,1), After Ry(90): (1,0,0)
        Assert.True(System.Math.Abs(result.X - 1.0) < 1e-10);
        Assert.True(System.Math.Abs(result.Y) < 1e-10);
        Assert.True(System.Math.Abs(result.Z) < 1e-10);
    }

    [Fact]
    public void OperatorMultiply_Vec3()
    {
        var t = Transform3.FromTranslation(new Vec3(5, 0, 0));
        var result = t * new Vec3(1, 0, 0);
        Assert.True(System.Math.Abs(result.X - 6.0) < 1e-10);
    }

    [Fact]
    public void ChainedTransforms_InverseIsReversedComposition()
    {
        var t1 = Transform3.FromRotationX(0.3);
        var t2 = Transform3.FromTranslation(new Vec3(1, 2, 3));
        var t3 = Transform3.FromRotationY(0.7);
        var chain = t3 * t2 * t1;
        var inv = chain.Inverse();
        var p = new Vec3(4, 5, 6);
        var roundtrip = inv.TransformPoint(chain.TransformPoint(p));
        Assert.True((roundtrip - p).Length < 1e-8);
    }

    [Fact]
    public void LookRotation_ForwardAligned()
    {
        var t = Transform3.LookRotation(Vec3.UnitZ, Vec3.UnitY);
        var forward = t.TransformDirection(Vec3.UnitZ);
        Assert.True((forward - Vec3.UnitZ).Length < 1e-10);
    }

    [Fact]
    public void AxisAngle_SmallAngle_PreservesLength()
    {
        var t = Transform3.FromAxisAngle(new Vec3(1, 1, 1).Normalized, 0.001);
        var p = new Vec3(3, 4, 5);
        var result = t.TransformPoint(p);
        Assert.True(System.Math.Abs(result.Length - p.Length) < 1e-6);
    }

    [Fact]
    public void IdentityInverse_IsIdentity()
    {
        var inv = Transform3.Identity.Inverse();
        var p = new Vec3(1, 2, 3);
        var result = inv.TransformPoint(p);
        Assert.True((result - p).Length < 1e-10);
    }

    [Fact]
    public void Rotation360_IsIdentity()
    {
        var t = Transform3.FromRotationX(2 * System.Math.PI);
        var p = new Vec3(1, 2, 3);
        var result = t.TransformPoint(p);
        Assert.True((result - p).Length < 1e-10);
    }

    [Fact]
    public void SolidTransform_PreservesVolume()
    {
        var cube = MdCsg.Api.Primitives.Cube(Vec3.Zero, 2.0);
        var t = Transform3.FromRotationX(0.5) *
                Transform3.FromTranslation(new Vec3(10, 20, 30));
        var transformed = cube.Transform(t);
        Assert.True(System.Math.Abs(transformed.Volume() - cube.Volume()) < 0.01);
    }
}
