using System.Runtime.CompilerServices;

namespace MdCsg.Arithmetic;

/// <summary>
/// Shewchuk-style expansion arithmetic for exact floating-point computation.
/// An "expansion" is an array of doubles whose sum equals the exact value,
/// with each component non-overlapping (|e[i]| ≤ ulp(e[i+1])/2).
/// </summary>
public static class ExpansionArithmetic
{
    /// <summary>Splitter for TwoProduct: 2^27 + 1.</summary>
    private const double Splitter = 134217729.0; // 2^27 + 1

    /// <summary>
    /// Exact sum of two doubles: a + b = s + e, where s is the float result and e is the error.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (double Sum, double Error) TwoSum(double a, double b)
    {
        var s = a + b;
        var bVirtual = s - a;
        var aVirtual = s - bVirtual;
        var bRoundoff = b - bVirtual;
        var aRoundoff = a - aVirtual;
        return (s, aRoundoff + bRoundoff);
    }

    /// <summary>
    /// Exact difference of two doubles: a - b = s + e.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (double Diff, double Error) TwoDiff(double a, double b)
    {
        var s = a - b;
        var bVirtual = a - s;
        var aVirtual = s + bVirtual;
        var bRoundoff = bVirtual - b;
        var aRoundoff = a - aVirtual;
        return (s, aRoundoff + bRoundoff);
    }

    /// <summary>
    /// Exact product of two doubles using FMA: a * b = p + e.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (double Product, double Error) TwoProduct(double a, double b)
    {
        var p = a * b;
#if NET
        var e = System.Math.FusedMultiplyAdd(a, b, -p);
#else
        // Dekker split: exact error term without FMA
        var c = Splitter * a;
        var abig = c - a;
        var ahi = c - abig;
        var alo = a - ahi;
        c = Splitter * b;
        var bbig = c - b;
        var bhi = c - bbig;
        var blo = b - bhi;
        var e = ((ahi * bhi - p) + ahi * blo + alo * bhi) + alo * blo;
#endif
        return (p, e);
    }

    /// <summary>
    /// Adds a single value to an expansion, producing a new expansion.
    /// The input expansion e[0..n-1] must be non-overlapping and sorted by magnitude.
    /// The result h[0..n] is also non-overlapping.
    /// </summary>
    public static int GrowExpansion(ReadOnlySpan<double> e, double b, Span<double> h)
    {
        var q = b;
        int hIndex = 0;
        for (int i = 0; i < e.Length; i++)
        {
            var (sum, err) = TwoSum(q, e[i]);
            q = sum;
            if (err != 0)
                h[hIndex++] = err;
        }
        h[hIndex++] = q;
        return hIndex;
    }

    /// <summary>
    /// Sums two expansions e and f into h. Returns the length of h.
    /// </summary>
    public static int ExpansionSum(ReadOnlySpan<double> e, ReadOnlySpan<double> f, Span<double> h)
    {
        // Merge-based expansion addition
        int hLen = 0;

        // Start with e
        if (e.Length > 0)
        {
            e.CopyTo(h);
            hLen = e.Length;
        }

        // Add each element of f
        Span<double> temp = stackalloc double[h.Length + 1];
        for (int i = 0; i < f.Length; i++)
        {
            int newLen = GrowExpansion(h[..hLen], f[i], temp);
            temp[..newLen].CopyTo(h);
            hLen = newLen;
        }

        return hLen;
    }

    /// <summary>
    /// Multiplies an expansion e by a scalar b, producing h. Returns the length of h.
    /// </summary>
    public static int ScaleExpansion(ReadOnlySpan<double> e, double b, Span<double> h)
    {
        if (e.Length == 0)
            return 0;

        var (product, error) = TwoProduct(e[0], b);
        int hLen = 0;
        if (error != 0)
            h[hLen++] = error;

        var q = product;

        for (int i = 1; i < e.Length; i++)
        {
            var (p, err) = TwoProduct(e[i], b);
            var (sum1, err1) = TwoSum(q, err);
            if (err1 != 0)
                h[hLen++] = err1;
            var (sum2, err2) = TwoSum(p, sum1);
            if (err2 != 0)
                h[hLen++] = err2;
            q = sum2;
        }
        h[hLen++] = q;
        return hLen;
    }

    /// <summary>
    /// Returns the sign of an expansion (-1, 0, or +1).
    /// The sign is determined by the most significant (last) non-zero component.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Sign(ReadOnlySpan<double> e)
    {
        for (int i = e.Length - 1; i >= 0; i--)
        {
            if (e[i] > 0) return 1;
            if (e[i] < 0) return -1;
        }
        return 0;
    }

    /// <summary>
    /// Compresses an expansion by eliminating zero components and merging overlapping terms.
    /// Returns the length of the compressed expansion.
    /// </summary>
    public static int Compress(ReadOnlySpan<double> e, Span<double> h)
    {
        if (e.Length == 0) return 0;

        // Bottom-up compression pass
        var q = e[^1];
        int bottom = e.Length - 1;
        Span<double> temp = stackalloc double[e.Length];

        for (int i = e.Length - 2; i >= 0; i--)
        {
            var (newQ, smallQ) = TwoSum(q, e[i]);
            if (smallQ != 0)
            {
                temp[bottom--] = newQ;
                q = smallQ;
            }
            else
            {
                q = newQ;
            }
        }
        temp[bottom] = q;

        // Top-down pass to finalize
        int hLen = 0;
        q = temp[bottom];
        for (int i = bottom + 1; i < e.Length; i++)
        {
            var (newQ, smallQ) = TwoSum(q, temp[i]);
            if (smallQ != 0)
                h[hLen++] = smallQ;
            q = newQ;
        }
        h[hLen++] = q;
        return hLen;
    }

    /// <summary>
    /// Negates all components of an expansion.
    /// </summary>
    public static void Negate(Span<double> e)
    {
        for (int i = 0; i < e.Length; i++)
            e[i] = -e[i];
    }
}
