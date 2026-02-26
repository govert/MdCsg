using MdCsg.Math;

namespace MdCsg.Tests.Math;

public class Transform3Tests
{
    private const double Tol = 1e-10;

    private static void AssertVec3Near(Vec3 expected, Vec3 actual, double tol = Tol, string msg = "")
    {
        Assert.True(Vec3.DistanceSquared(expected, actual) < tol * tol,
            $"Expected {expected}, got {actual}. {msg}");
    }

    // ── Identity ─────────────────────────────────────────────────────

    [Fact]
    public void Identity_TransformPoint_Unchanged()
    {
        var p = new Vec3(3, 4, 5);
        AssertVec3Near(p, Transform3.Identity.TransformPoint(p));
    }

    [Fact]
    public void Identity_TransformDirection_Unchanged()
    {
        var d = new Vec3(1, 2, 3);
        AssertVec3Near(d, Transform3.Identity.TransformDirection(d));
    }

    // ── FromTranslation ──────────────────────────────────────────────

    [Fact]
    public void FromTranslation_ShiftsPoint()
    {
        var t = Transform3.FromTranslation(new Vec3(1, 2, 3));
        AssertVec3Near(new Vec3(4, 6, 8), t.TransformPoint(new Vec3(3, 4, 5)));
    }

    [Fact]
    public void FromTranslation_DirectionUnchanged()
    {
        var t = Transform3.FromTranslation(new Vec3(10, 20, 30));
        AssertVec3Near(new Vec3(1, 0, 0), t.TransformDirection(new Vec3(1, 0, 0)));
    }

    // ── FromRotationX/Y/Z ───────────────────────────────────────────

    [Fact]
    public void RotationX_90Degrees_YToZ()
    {
        var rot = Transform3.FromRotationX(System.Math.PI / 2);
        AssertVec3Near(Vec3.UnitZ, rot.TransformPoint(Vec3.UnitY));
    }

    [Fact]
    public void RotationY_90Degrees_ZToX()
    {
        var rot = Transform3.FromRotationY(System.Math.PI / 2);
        AssertVec3Near(Vec3.UnitX, rot.TransformPoint(Vec3.UnitZ));
    }

    [Fact]
    public void RotationZ_90Degrees_XToY()
    {
        var rot = Transform3.FromRotationZ(System.Math.PI / 2);
        AssertVec3Near(Vec3.UnitY, rot.TransformPoint(Vec3.UnitX));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(2.0)]
    [InlineData(3.14159)]
    public void RotationX_PreservesLength(double angle)
    {
        var rot = Transform3.FromRotationX(angle);
        var v = new Vec3(1, 2, 3);
        var rv = rot.TransformPoint(v);
        Assert.True(System.Math.Abs(rv.Length - v.Length) < Tol);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(2.0)]
    [InlineData(3.14159)]
    public void RotationY_PreservesLength(double angle)
    {
        var rot = Transform3.FromRotationY(angle);
        var v = new Vec3(1, 2, 3);
        var rv = rot.TransformPoint(v);
        Assert.True(System.Math.Abs(rv.Length - v.Length) < Tol);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(2.0)]
    [InlineData(3.14159)]
    public void RotationZ_PreservesLength(double angle)
    {
        var rot = Transform3.FromRotationZ(angle);
        var v = new Vec3(1, 2, 3);
        var rv = rot.TransformPoint(v);
        Assert.True(System.Math.Abs(rv.Length - v.Length) < Tol);
    }

    // ── FromAxisAngle ───────────────────────────────────────────────

    [Fact]
    public void AxisAngle_ZAxis90_XToY()
    {
        var rot = Transform3.FromAxisAngle(Vec3.UnitZ, System.Math.PI / 2);
        AssertVec3Near(Vec3.UnitY, rot.TransformPoint(Vec3.UnitX));
    }

    [Fact]
    public void AxisAngle_ArbitraryAxis_PreservesLength()
    {
        var axis = new Vec3(1, 1, 1);
        var rot = Transform3.FromAxisAngle(axis, 1.23);
        var v = new Vec3(2, 3, 4);
        Assert.True(System.Math.Abs(rot.TransformPoint(v).Length - v.Length) < Tol);
    }

    [Fact]
    public void AxisAngle_ZeroAngle_Identity()
    {
        var rot = Transform3.FromAxisAngle(Vec3.UnitX, 0);
        var v = new Vec3(1, 2, 3);
        AssertVec3Near(v, rot.TransformPoint(v));
    }

    [Fact]
    public void AxisAngle_360Degrees_ReturnsOriginal()
    {
        var axis = new Vec3(1, 2, 3);
        var rot = Transform3.FromAxisAngle(axis, 2 * System.Math.PI);
        var v = new Vec3(4, 5, 6);
        AssertVec3Near(v, rot.TransformPoint(v));
    }

    [Fact]
    public void AxisAngle_PointOnAxis_Unchanged()
    {
        var axis = new Vec3(1, 1, 1).Normalized;
        var rot = Transform3.FromAxisAngle(axis, 1.5);
        var v = axis * 5;
        AssertVec3Near(v, rot.TransformPoint(v));
    }

    // ── LookRotation ────────────────────────────────────────────────

    [Fact]
    public void LookRotation_ForwardZ_UpY_IsIdentity()
    {
        var rot = Transform3.LookRotation(Vec3.UnitZ, Vec3.UnitY);
        AssertVec3Near(Vec3.UnitX, rot.TransformPoint(Vec3.UnitX));
        AssertVec3Near(Vec3.UnitY, rot.TransformPoint(Vec3.UnitY));
        AssertVec3Near(Vec3.UnitZ, rot.TransformPoint(Vec3.UnitZ));
    }

    [Fact]
    public void LookRotation_ForwardX_UpY_RotatesCorrectly()
    {
        var rot = Transform3.LookRotation(Vec3.UnitX, Vec3.UnitY);
        // Z should map to X
        AssertVec3Near(Vec3.UnitX, rot.TransformPoint(Vec3.UnitZ));
    }

    // ── Euler roundtrips ────────────────────────────────────────────

    [Theory]
    [InlineData(0.1, 0.2, 0.3)]
    [InlineData(0.5, -0.3, 0.8)]
    [InlineData(0, 0, 0)]
    [InlineData(1.0, 0.0, 0.0)]
    [InlineData(0.0, 0.5, 0.0)]
    [InlineData(0.0, 0.0, 1.0)]
    public void EulerXYZ_Roundtrip(double x, double y, double z)
    {
        var t = Transform3.FromEulerXYZ(x, y, z);
        var (rx, ry, rz) = t.ToEulerXYZ();
        Assert.True(System.Math.Abs(rx - x) < 1e-8, $"X: expected {x}, got {rx}");
        Assert.True(System.Math.Abs(ry - y) < 1e-8, $"Y: expected {y}, got {ry}");
        Assert.True(System.Math.Abs(rz - z) < 1e-8, $"Z: expected {z}, got {rz}");
    }

    [Fact]
    public void EulerZYX_EquivalentToComposition()
    {
        double x = 0.3, y = 0.4, z = 0.5;
        var fromEuler = Transform3.FromEulerZYX(z, y, x);
        var manual = Transform3.FromRotationZ(z) * Transform3.FromRotationY(y) * Transform3.FromRotationX(x);
        var v = new Vec3(1, 2, 3);
        AssertVec3Near(manual.TransformPoint(v), fromEuler.TransformPoint(v));
    }

    // ── Compose / Inverse ──────────────────────────────────────────

    [Fact]
    public void Compose_TranslationAndRotation()
    {
        var t = Transform3.FromTranslation(new Vec3(1, 0, 0));
        var r = Transform3.FromRotationZ(System.Math.PI / 2);
        var combined = r * t; // translate then rotate
        var result = combined.TransformPoint(Vec3.Zero);
        // (0,0,0) -> translate to (1,0,0) -> rotate 90° -> (0,1,0)
        AssertVec3Near(new Vec3(0, 1, 0), result);
    }

    [Fact]
    public void Inverse_UndoesTransform()
    {
        var t = Transform3.FromAxisAngle(new Vec3(1, 2, 3), 0.7);
        t = new Transform3(t.Rotation, new Vec3(4, 5, 6));
        var inv = t.Inverse();
        var v = new Vec3(7, 8, 9);
        AssertVec3Near(v, inv.TransformPoint(t.TransformPoint(v)));
    }

    [Fact]
    public void Inverse_OfIdentity_IsIdentity()
    {
        var inv = Transform3.Identity.Inverse();
        var v = new Vec3(1, 2, 3);
        AssertVec3Near(v, inv.TransformPoint(v));
    }

    [Theory]
    [InlineData(0.5, 1.0, -0.3)]
    [InlineData(0.0, 0.0, 0.0)]
    [InlineData(2.0, -1.5, 0.7)]
    public void Inverse_Roundtrip_Various(double tx, double ty, double tz)
    {
        var rot = Transform3.FromAxisAngle(Vec3.UnitZ, 0.8);
        var t = new Transform3(rot.Rotation, new Vec3(tx, ty, tz));
        var roundtrip = t * t.Inverse();
        AssertVec3Near(Vec3.Zero, roundtrip.TransformPoint(Vec3.Zero));
        AssertVec3Near(Vec3.UnitX, roundtrip.TransformPoint(Vec3.UnitX));
    }

    // ── Operators ───────────────────────────────────────────────────

    [Fact]
    public void StarOperator_PointTransform()
    {
        var t = Transform3.FromTranslation(new Vec3(1, 2, 3));
        var p = new Vec3(0, 0, 0);
        AssertVec3Near(new Vec3(1, 2, 3), t * p);
    }

    [Fact]
    public void StarOperator_Composition_Associative()
    {
        var a = Transform3.FromRotationX(0.3);
        var b = Transform3.FromTranslation(new Vec3(1, 0, 0));
        var c = Transform3.FromRotationZ(0.5);
        var v = new Vec3(1, 2, 3);

        var ab_c = (a * b) * c;
        var a_bc = a * (b * c);
        AssertVec3Near(ab_c.TransformPoint(v), a_bc.TransformPoint(v));
    }

    // ── TransformDirection ──────────────────────────────────────────

    [Fact]
    public void TransformDirection_IgnoresTranslation()
    {
        var t = new Transform3(Mat3.Identity, new Vec3(100, 200, 300));
        AssertVec3Near(Vec3.UnitX, t.TransformDirection(Vec3.UnitX));
    }

    [Fact]
    public void TransformDirection_AppliesRotation()
    {
        var t = Transform3.FromRotationZ(System.Math.PI / 2);
        AssertVec3Near(Vec3.UnitY, t.TransformDirection(Vec3.UnitX));
    }
}
