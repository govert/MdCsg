# .NET Framework 4.8 Port

MdCsg multi-targets both .NET 10 and .NET Framework 4.8 from a single codebase. The .NET 10 build uses all modern APIs with no compromises. The .NET 4.8 build uses polyfills and conditional compilation to maintain identical correctness.

## Build & Test

```bash
# Build both targets
dotnet build

# Test both targets
dotnet test

# Test a single target
dotnet test -f net10.0
dotnet test -f net48

# Run benchmarks (one target at a time)
dotnet run -c Release -f net10.0 --project benchmarks/MdCsg.Benchmarks/MdCsg.Benchmarks.csproj
dotnet run -c Release -f net48 --project benchmarks/MdCsg.Benchmarks/MdCsg.Benchmarks.csproj
```

## Compatibility Changes

### Global Usings

.NET 10 provides implicit global usings for `System`, `System.Collections.Generic`, `System.IO`, `System.Linq`, `System.Threading`, and `System.Threading.Tasks`. On .NET Framework 4.8 these are absent. Rather than adding `using` directives to every file, `Directory.Build.props` supplies them via `<Using>` items conditioned on `net48`.

### Generic Math Interfaces (Vec2, Vec3)

`IAdditionOperators`, `ISubtractionOperators`, `IUnaryNegationOperators`, and `IMultiplyOperators` exist only on .NET 7+. The interface declarations are wrapped in `#if NET7_0_OR_GREATER`. The operator implementations themselves (`operator +`, `operator -`, etc.) are unconditional — they work on all targets. Only the interface conformance is conditional.

```csharp
public readonly record struct Vec3(double X, double Y, double Z)
#if NET7_0_OR_GREATER
    : IAdditionOperators<Vec3, Vec3, Vec3>,
      ISubtractionOperators<Vec3, Vec3, Vec3>,
      IUnaryNegationOperators<Vec3, Vec3>,
      IMultiplyOperators<Vec3, double, Vec3>
#endif
```

### Fused Multiply-Add (MathUtil, ExpansionArithmetic)

`Math.FusedMultiplyAdd` was added in .NET Core 3.0. On .NET 10 it compiles to a single hardware FMA instruction. On .NET Framework 4.8, `MathUtil.Fma` falls back to `a * b + c` (two rounding steps instead of one).

For `ExpansionArithmetic.TwoProduct`, the fallback is more involved. The FMA path computes the exact error term of a floating-point product in one instruction:

```csharp
var e = Math.FusedMultiplyAdd(a, b, -p); // exact: a*b - p
```

The .NET Framework path uses the Dekker split (from Shewchuk's "Adaptive Precision Floating-Point Arithmetic"):

```csharp
var c = Splitter * a;       // Splitter = 2^27 + 1
var abig = c - a;
var ahi = c - abig;
var alo = a - ahi;
c = Splitter * b;
var bbig = c - b;
var bhi = c - bbig;
var blo = b - bhi;
var e = ((ahi * bhi - p) + ahi * blo + alo * bhi) + alo * blo;
```

Both paths produce the exact error term. The Dekker split is ~6 operations vs 1 for FMA, but `TwoProduct` is not a bottleneck — the benchmark shows identical throughput on both targets (762 ns vs 785 ns for 1000 calls).

### HashCode.Combine (Rational, MeshBuilder, SubTriangleAdjacency)

`HashCode.Combine` was added in .NET Core 2.1. On .NET Framework 4.8, the three call sites fall back to XOR-multiply hashing:

```csharp
#if NET
    return HashCode.Combine(x, y, z);
#else
    unchecked { return (x * 397L ^ y) * 397L ^ z; }
#endif
```

The hash quality difference does not affect correctness — these hashes are used for vertex welding and adjacency lookups where collisions cause linear probing, not incorrect results.

### KeyValuePair Deconstruction (SubTriangleAdjacency)

`foreach (var (key, value) in dictionary)` requires a `Deconstruct` extension method on `KeyValuePair` that was added in .NET Core 2.0. On .NET Framework 4.8 this is replaced with:

```csharp
foreach (var kvp in edgeMap)
{
    var tris = kvp.Value;
    // ...
}
```

### Array.Fill (PatchExtractor)

`Array.Fill` was added in .NET Core 2.0. Replaced with an equivalent for loop:

```csharp
for (int k = 0; k < n; k++) patchOf[k] = -1;
```

### NuGet Packages (net48 only)

| Package | Version | Purpose |
|---|---|---|
| PolySharp | 1.* | Compiler polyfills: `init`, `required`, `record struct`, `Index`, `Range`, `IsExternalInit`, `CallerArgumentExpression` |
| System.Memory | 4.5.5 | `Span<T>`, `ReadOnlySpan<T>`, `stackalloc` to Span conversion |
| System.Runtime.CompilerServices.Unsafe | 6.0.0 | Resolves transitive dependency version conflict between System.Memory and xUnit/BenchmarkDotNet |

### Binding Redirects (test and benchmark projects)

.NET Framework test and benchmark projects need auto-generated binding redirects for transitive NuGet dependencies:

```xml
<PropertyGroup Condition="'$(TargetFramework)' == 'net48'">
    <AutoGenerateBindingRedirects>true</AutoGenerateBindingRedirects>
    <GenerateBindingRedirectsOutputType>true</GenerateBindingRedirectsOutputType>
</PropertyGroup>
```

## Performance Comparison

Benchmarks run on AMD Ryzen 7 4800H, Windows 11, .NET SDK 10.0.200-preview.

| Benchmark | .NET 10.0 | .NET 4.8 | Ratio (4.8 / 10.0) |
|---|---:|---:|---:|
| CubeUnion | 108 us | 241 us | 2.2x slower |
| CubeIntersection | 92 us | 218 us | 2.4x slower |
| CubeDifference | 96 us | 228 us | 2.4x slower |
| CubeSphereIntersection | 9,453 us | 17,608 us | 1.9x slower |
| BvhBuild | 6.6 us | 23.4 us | 3.5x slower |
| MeshBuild | 2.9 us | 6.0 us | 2.0x slower |
| TwoProduct (1000 iterations) | 762 ns | 785 ns | 1.0x (same) |

| Benchmark | .NET 10.0 Allocated | .NET 4.8 Allocated | Ratio |
|---|---:|---:|---:|
| CubeUnion | 241 KB | 254 KB | 1.05x |
| CubeIntersection | 206 KB | 218 KB | 1.06x |
| CubeDifference | 220 KB | 232 KB | 1.05x |
| CubeSphereIntersection | 13,049 KB | 14,110 KB | 1.08x |
| BvhBuild | 22 KB | 23 KB | 1.02x |
| MeshBuild | 9 KB | 9 KB | 1.04x |
| TwoProduct | 0 B | 0 B | — |

### Analysis

**Throughput**: .NET 10 is 2–3.5x faster than .NET 4.8 on CSG operations. The gap is largest for BVH construction (3.5x) which benefits heavily from RyuJIT improvements in bounds-check elimination, loop unrolling, and struct promotion. The gap is smallest for pure arithmetic (TwoProduct at 1.0x) where both JITs emit essentially the same machine code.

**Memory**: Allocations are 2–8% higher on .NET 4.8. The difference comes from larger object headers (16 bytes on .NET Framework vs 12 bytes on 64-bit .NET) and less aggressive struct promotion.

**Correctness**: All 134 tests pass identically on both targets. The Dekker-split TwoProduct fallback produces bit-identical error terms to the FMA path for all IEEE 754 doubles.

## What Is NOT Conditional

The following features work identically on both targets with no `#if` guards:

- `readonly record struct` — polyfilled by PolySharp
- `init` properties — polyfilled by PolySharp
- `required` members — polyfilled by PolySharp
- `file-scoped namespaces` — C# 12 language feature, no runtime dependency
- `switch expressions` — C# 8+ language feature
- `pattern matching` — C# 7+ language feature
- `Span<T>` / `ReadOnlySpan<T>` — provided by System.Memory on net48
- `stackalloc` to `Span<T>` — works on net48 via System.Memory
- Range/index operators (`[..n]`, `[^1]`) — polyfilled by PolySharp, Span slicing handled by compiler
- Collection expressions (`[]`) — C# 12 compiler feature, generates `new List<T>()` / `Array.Empty<T>()`
- Value tuples — included in .NET Framework 4.7+
- `BigInteger` — in `System.Numerics` on both targets
