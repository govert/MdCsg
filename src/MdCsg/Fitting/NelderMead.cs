namespace MdCsg.Fitting;

/// <summary>
/// Nelder-Mead (downhill simplex) derivative-free optimization.
/// </summary>
public static class NelderMead
{
    /// <summary>
    /// Minimizes an objective function using the Nelder-Mead simplex method.
    /// </summary>
    /// <param name="objective">The function to minimize.</param>
    /// <param name="initial">Initial parameter guess.</param>
    /// <param name="maxIter">Maximum number of iterations.</param>
    /// <param name="tol">Convergence tolerance on the simplex size.</param>
    /// <param name="initialStep">Optional per-dimension initial step sizes for simplex construction.</param>
    /// <returns>The optimization result.</returns>
    public static NelderMeadResult Minimize(Func<double[], double> objective, double[] initial, int maxIter = 1000, double tol = 1e-10, double[]? initialStep = null)
    {
        int n = initial.Length;
        int nv = n + 1; // number of simplex vertices

        // Build initial simplex
        var simplex = new double[nv][];
        var values = new double[nv];

        simplex[0] = (double[])initial.Clone();
        values[0] = objective(simplex[0]);

        for (int i = 0; i < n; i++)
        {
            simplex[i + 1] = (double[])initial.Clone();
            double step;
            if (initialStep != null)
            {
                step = initialStep[i];
            }
            else
            {
                step = System.Math.Abs(initial[i]) * 0.05;
                if (step < 1e-4) step = 0.00025;
            }
            simplex[i + 1][i] += step;
            values[i + 1] = objective(simplex[i + 1]);
        }

        const double alpha = 1.0;  // reflection
        const double gamma = 2.0;  // expansion
        const double rho = 0.5;    // contraction
        const double sigma = 0.5;  // shrink

        int iter;
        for (iter = 0; iter < maxIter; iter++)
        {
            // Order: find best, worst, second-worst
            int best = 0, worst = 0, secondWorst = 0;
            for (int i = 1; i < nv; i++)
            {
                if (values[i] < values[best]) best = i;
                if (values[i] > values[worst]) worst = i;
            }
            for (int i = 0; i < nv; i++)
            {
                if (i == worst) continue;
                if (secondWorst == worst || values[i] > values[secondWorst])
                    secondWorst = i;
            }

            // Check convergence: range of values
            double range = values[worst] - values[best];
            if (range < tol) break;

            // Centroid of all points except worst
            var centroid = new double[n];
            for (int i = 0; i < nv; i++)
            {
                if (i == worst) continue;
                for (int j = 0; j < n; j++)
                    centroid[j] += simplex[i][j];
            }
            for (int j = 0; j < n; j++)
                centroid[j] /= n;

            // Reflection
            var reflected = PointOp(centroid, simplex[worst], alpha, n);
            double fr = objective(reflected);

            if (fr < values[secondWorst] && fr >= values[best])
            {
                simplex[worst] = reflected;
                values[worst] = fr;
                continue;
            }

            if (fr < values[best])
            {
                // Expansion
                var expanded = PointOp(centroid, simplex[worst], gamma, n);
                double fe = objective(expanded);
                if (fe < fr)
                {
                    simplex[worst] = expanded;
                    values[worst] = fe;
                }
                else
                {
                    simplex[worst] = reflected;
                    values[worst] = fr;
                }
                continue;
            }

            // Contraction
            var contracted = new double[n];
            if (fr < values[worst])
            {
                // Outside contraction
                for (int j = 0; j < n; j++)
                    contracted[j] = centroid[j] + rho * (reflected[j] - centroid[j]);
            }
            else
            {
                // Inside contraction
                for (int j = 0; j < n; j++)
                    contracted[j] = centroid[j] + rho * (simplex[worst][j] - centroid[j]);
            }

            double fc = objective(contracted);
            if (fc < values[worst])
            {
                simplex[worst] = contracted;
                values[worst] = fc;
                continue;
            }

            // Shrink
            for (int i = 0; i < nv; i++)
            {
                if (i == best) continue;
                for (int j = 0; j < n; j++)
                    simplex[i][j] = simplex[best][j] + sigma * (simplex[i][j] - simplex[best][j]);
                values[i] = objective(simplex[i]);
            }
        }

        // Find best
        int bestIdx = 0;
        for (int i = 1; i < nv; i++)
            if (values[i] < values[bestIdx]) bestIdx = i;

        return new NelderMeadResult(simplex[bestIdx], values[bestIdx], iter);
    }

    private static double[] PointOp(double[] centroid, double[] worst, double coeff, int n)
    {
        // centroid + coeff * (centroid - worst)
        var result = new double[n];
        for (int j = 0; j < n; j++)
            result[j] = centroid[j] + coeff * (centroid[j] - worst[j]);
        return result;
    }
}
