using MdCsg.Math;

namespace MdCsg.Tests.Math;

/// <summary>Batch 2: Vec2, Aabb, Segment, Ray extended tests (20 tests)</summary>
public class Vec2AabbSegmentRayTests
{
    // --- Vec2 ---

    [Fact]
    public void Vec2_Subtraction()
    {
        var a = new Vec2(4, 5);
        var b = new Vec2(1, 2);
        Assert.Equal(new Vec2(3, 3), a - b);
    }

    [Fact]
    public void Vec2_Negation()
    {
        var v = new Vec2(1, -2);
        Assert.Equal(new Vec2(-1, 2), -v);
    }

    [Fact]
    public void Vec2_ScalarMultiply()
    {
        var v = new Vec2(3, 4);
        Assert.Equal(new Vec2(6, 8), v * 2);
        Assert.Equal(new Vec2(6, 8), 2 * v);
    }

    [Fact]
    public void Vec2_Division()
    {
        var v = new Vec2(6, 8);
        Assert.Equal(new Vec2(3, 4), v / 2);
    }

    [Fact]
    public void Vec2_LengthSquared()
    {
        var v = new Vec2(3, 4);
        Assert.Equal(25, v.LengthSquared);
    }

    [Fact]
    public void Vec2_CrossProduct_Positive()
    {
        var a = new Vec2(1, 0);
        var b = new Vec2(0, 1);
        Assert.Equal(1, Vec2.Cross(a, b));
    }

    [Fact]
    public void Vec2_CrossProduct_Negative()
    {
        var a = new Vec2(0, 1);
        var b = new Vec2(1, 0);
        Assert.Equal(-1, Vec2.Cross(a, b));
    }

    [Fact]
    public void Vec2_ToString()
    {
        var v = new Vec2(1, 2);
        Assert.Equal("(1, 2)", v.ToString());
    }

    // --- Aabb ---

    [Fact]
    public void Aabb_Size()
    {
        var box = new Aabb(new Vec3(1, 2, 3), new Vec3(4, 6, 9));
        Assert.Equal(new Vec3(3, 4, 6), box.Size);
    }

    [Fact]
    public void Aabb_Center()
    {
        var box = new Aabb(new Vec3(0, 0, 0), new Vec3(2, 4, 6));
        Assert.Equal(new Vec3(1, 2, 3), box.Center);
    }

    [Fact]
    public void Aabb_Empty_HasNegativeSize()
    {
        var empty = Aabb.Empty;
        Assert.True(empty.Size.X < 0);
    }

    [Fact]
    public void Aabb_Expand_GrowsUniformly()
    {
        var box = new Aabb(new Vec3(1, 1, 1), new Vec3(2, 2, 2));
        var expanded = box.Expand(0.5);
        Assert.Equal(new Vec3(0.5, 0.5, 0.5), expanded.Min);
        Assert.Equal(new Vec3(2.5, 2.5, 2.5), expanded.Max);
    }

    [Fact]
    public void Aabb_FromPoint_MinEqualsMax()
    {
        var pt = new Vec3(3, 4, 5);
        var box = Aabb.FromPoint(pt);
        Assert.Equal(pt, box.Min);
        Assert.Equal(pt, box.Max);
    }

    [Fact]
    public void Aabb_ExpandToInclude()
    {
        var box = Aabb.FromPoint(new Vec3(0, 0, 0));
        box = box.ExpandToInclude(new Vec3(1, 1, 1));
        Assert.Equal(new Vec3(0, 0, 0), box.Min);
        Assert.Equal(new Vec3(1, 1, 1), box.Max);
    }

    [Fact]
    public void Aabb_Contains_BoundaryPoint()
    {
        var box = new Aabb(new Vec3(0, 0, 0), new Vec3(1, 1, 1));
        Assert.True(box.Contains(new Vec3(0, 0, 0)));
        Assert.True(box.Contains(new Vec3(1, 1, 1)));
        Assert.True(box.Contains(new Vec3(0.5, 0.5, 0.5)));
    }

    // --- Segment ---

    [Fact]
    public void Segment_Direction()
    {
        var seg = new Segment(new Vec3(1, 0, 0), new Vec3(4, 0, 0));
        Assert.Equal(new Vec3(3, 0, 0), seg.Direction);
    }

    [Fact]
    public void Segment_Length()
    {
        var seg = new Segment(new Vec3(0, 0, 0), new Vec3(3, 4, 0));
        Assert.Equal(5, seg.Length, 1e-15);
    }

    [Fact]
    public void Segment_Midpoint()
    {
        var seg = new Segment(new Vec3(0, 0, 0), new Vec3(2, 4, 6));
        Assert.Equal(new Vec3(1, 2, 3), seg.Midpoint);
    }

    // --- Ray ---

    [Fact]
    public void Ray_PointAt()
    {
        var ray = new Ray(new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        Assert.Equal(new Vec3(1, 3, 0), ray.PointAt(3));
    }

    [Fact]
    public void Segment_PointAt()
    {
        var seg = new Segment(new Vec3(0, 0, 0), new Vec3(10, 0, 0));
        Assert.Equal(new Vec3(5, 0, 0), seg.PointAt(0.5));
        Assert.Equal(new Vec3(0, 0, 0), seg.PointAt(0));
        Assert.Equal(new Vec3(10, 0, 0), seg.PointAt(1));
    }
}
