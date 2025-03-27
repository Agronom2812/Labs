using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Multithreading;

/// <summary>
/// Contains methods for solving functions.
/// </summary>
public sealed class FunctionSolver {

    private static readonly Semaphore s_semaphore = new Semaphore(2, 2);
    public event Action<double, long> CalculationCompleted;
    public event Action<int, int> ProgressChanged;

    /// <summary>
    /// Solving function using left rectangle method.
    /// </summary>
    /// <param name="lineOffset">offset for progress bars.</param>
    /// <remarks>
    /// Each iteration has a delay of 100,000 formal computations.
    /// Method using semaphore to allow only 2 threads at one time.
    /// </remarks>
    [SuppressMessage("ReSharper.DPA", "DPA0002: Excessive memory allocations in SOH", MessageId = "type: System.String; size: 696MB")]
    public void RectangleMethod(int lineOffset) {

        s_semaphore.WaitOne();

        try {
            const double left = 0.0, right = 1.0, step = 0.00000001;
            const long rectanglesCount = (long)((right - left) / step);
            double sum = 0.0;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            for (int i = 0; i < rectanglesCount; ++i) {
                double tempResult = left + i * step;
                sum += Math.Sin(tempResult);

                for (int j = 0; j < 10; j++) {
                    double formalResult = 2.0 * 2.0;
                }

                int progress = (int)((i + 1) * 100.0 / rectanglesCount);

                progress = Math.Clamp(progress, 0, 100);

                ProgressChanged?.Invoke(Environment.CurrentManagedThreadId, progress);
            }

            stopwatch.Stop();

            CalculationCompleted?.Invoke(sum * step, stopwatch.ElapsedTicks);
        }

        finally {
            s_semaphore.Release();
        }
    }
}
