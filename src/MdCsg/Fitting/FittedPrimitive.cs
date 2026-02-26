using MdCsg.Math;

namespace MdCsg.Fitting;

/// <summary>
/// Result of fitting a plane to a point cloud.
/// </summary>
/// <param name="Centroid">The centroid (mean) of the fitted points.</param>
/// <param name="Normal">The unit normal of the best-fit plane.</param>
/// <param name="RmsResidual">Root mean square distance of points from the fitted plane.</param>
public readonly record struct FittedPlane(Vec3 Centroid, Vec3 Normal, double RmsResidual);

/// <summary>
/// Result of fitting a sphere to a point cloud.
/// </summary>
/// <param name="Center">The center of the fitted sphere.</param>
/// <param name="Radius">The radius of the fitted sphere.</param>
/// <param name="RmsResidual">Root mean square distance of points from the fitted sphere surface.</param>
public readonly record struct FittedSphere(Vec3 Center, double Radius, double RmsResidual);

/// <summary>
/// Result of fitting a cylinder to a point cloud.
/// </summary>
/// <param name="BaseCenter">A point on the cylinder axis at the base.</param>
/// <param name="Axis">The unit direction of the cylinder axis.</param>
/// <param name="Radius">The radius of the fitted cylinder.</param>
/// <param name="Height">The axial extent of the point cloud on the cylinder.</param>
/// <param name="RmsResidual">Root mean square distance of points from the fitted cylinder surface.</param>
public readonly record struct FittedCylinder(Vec3 BaseCenter, Vec3 Axis, double Radius, double Height, double RmsResidual);

/// <summary>
/// Result of fitting a 2D line y = Slope*x + Intercept.
/// </summary>
/// <param name="Slope">The slope of the line.</param>
/// <param name="Intercept">The y-intercept.</param>
/// <param name="RmsResidual">Root mean square residual.</param>
public readonly record struct FittedLine2D(double Slope, double Intercept, double RmsResidual);

/// <summary>
/// Result of fitting a 3D line through a point cloud.
/// </summary>
/// <param name="Point">A point on the line (centroid).</param>
/// <param name="Direction">The unit direction of the line.</param>
/// <param name="RmsResidual">Root mean square distance from points to the line.</param>
public readonly record struct FittedLine3D(Vec3 Point, Vec3 Direction, double RmsResidual);

/// <summary>
/// Result of fitting a 1D quadratic y = A*x^2 + B*x + C.
/// </summary>
/// <param name="A">Quadratic coefficient.</param>
/// <param name="B">Linear coefficient.</param>
/// <param name="C">Constant term.</param>
/// <param name="RmsResidual">Root mean square residual.</param>
public readonly record struct FittedQuadratic1D(double A, double B, double C, double RmsResidual);

/// <summary>
/// Result of fitting a paraboloid z = A*x^2 + B*x*y + C*y^2 + D*x + E*y + F.
/// </summary>
/// <param name="A">x^2 coefficient.</param>
/// <param name="B">x*y coefficient.</param>
/// <param name="C">y^2 coefficient.</param>
/// <param name="D">x coefficient.</param>
/// <param name="E">y coefficient.</param>
/// <param name="F">Constant term.</param>
/// <param name="RmsResidual">Root mean square residual.</param>
public readonly record struct FittedParaboloid(double A, double B, double C, double D, double E, double F, double RmsResidual);

/// <summary>
/// Result of fitting a cone to a point cloud.
/// </summary>
/// <param name="Apex">The apex of the cone.</param>
/// <param name="Axis">The unit direction of the cone axis (from apex toward base).</param>
/// <param name="HalfAngle">The half-angle of the cone in radians.</param>
/// <param name="RmsResidual">Root mean square distance from points to the cone surface.</param>
public readonly record struct FittedCone(Vec3 Apex, Vec3 Axis, double HalfAngle, double RmsResidual);

/// <summary>
/// Result of a Nelder-Mead optimization.
/// </summary>
/// <param name="Solution">The parameter values at the minimum.</param>
/// <param name="Value">The objective function value at the minimum.</param>
/// <param name="Iterations">The number of iterations performed.</param>
public readonly record struct NelderMeadResult(double[] Solution, double Value, int Iterations);
