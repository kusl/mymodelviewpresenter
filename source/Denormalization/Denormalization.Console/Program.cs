using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

public class DebugVsReleaseDenormalTest
{
    private const int WARMUP_ITERATIONS = 1000000;
    private const int TEST_ITERATIONS = 50000000; // Reduced for faster comparison

    public static void Main()
    {
        Console.WriteLine("Debug vs Release Denormal Performance Comparison");
        Console.WriteLine("==============================================");

        // Detect if we're running in Debug or Release
        bool isDebug = IsDebugBuild();
        Console.WriteLine($"Current build configuration: {(isDebug ? "DEBUG" : "RELEASE")}");

#if DEBUG
        Console.WriteLine("Compiled with DEBUG symbols");
#else
        Console.WriteLine("Compiled with RELEASE optimizations");
#endif

        Console.WriteLine($"Debugger attached: {Debugger.IsAttached}");
        Console.WriteLine($"JIT optimization enabled: {IsJitOptimized()}");
        Console.WriteLine();

        // Warm up
        WarmupJit();

        Console.WriteLine("==============================================");
        Console.WriteLine("PERFORMANCE TESTS");
        Console.WriteLine("==============================================");

        Console.WriteLine("\nFloat Performance:");
        TestFloatPerformance();

        Console.WriteLine("\nDouble Performance:");
        TestDoublePerformance();

        Console.WriteLine("\nDenormal Persistence Test:");
        TestDenormalPersistence();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void WarmupJit()
    {
        float f = 1.0f;
        double d = 1.0;

        for (int i = 0; i < WARMUP_ITERATIONS; i++)
        {
            f *= 1.000001f;
            d *= 1.000001;
        }

        Console.WriteLine($"JIT warmup completed");
    }

    private static void TestFloatPerformance()
    {
        // Normal arithmetic
        var normalTime = MeasureFloatOperation(1.0f, 1.000001f, "Normal Float");

        // Denormal that quickly underflows
        var quickUnderflowTime = MeasureFloatOperation(1E-37f, 0.1f, "Quick Underflow");

        // Sustained denormal
        var sustainedDenormalTime = MeasureFloatOperation(float.Epsilon, 0.999999f, "Sustained Denormal");

        Console.WriteLine($"  Denormal penalty: {(double)sustainedDenormalTime / normalTime:F1}x slower");
    }

    private static void TestDoublePerformance()
    {
        // Normal arithmetic
        var normalTime = MeasureDoubleOperation(1.0, 1.000001, "Normal Double");

        // Denormal that quickly underflows
        var quickUnderflowTime = MeasureDoubleOperation(1E-307, 0.1, "Quick Underflow");

        // Sustained denormal
        var sustainedDenormalTime = MeasureDoubleOperation(double.Epsilon, 0.999999, "Sustained Denormal");

        Console.WriteLine($"  Denormal penalty: {(double)sustainedDenormalTime / normalTime:F1}x slower");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static long MeasureFloatOperation(float initial, float multiplier, string testName)
    {
        // Force GC
        GC.Collect();
        GC.WaitForPendingFinalizers();

        var sw = Stopwatch.StartNew();
        float result = initial;

        for (int i = 0; i < TEST_ITERATIONS; i++)
        {
            result *= multiplier;
        }

        sw.Stop();

        bool isDenormal = IsDenormalized(result);
        Console.WriteLine($"  {testName,-18}: {sw.ElapsedMilliseconds,4} ms " +
                         $"(final: {result:E2}, denormal: {isDenormal})");

        // Prevent dead code elimination
        if (result == float.MaxValue) Console.WriteLine("Impossible");

        return sw.ElapsedMilliseconds;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static long MeasureDoubleOperation(double initial, double multiplier, string testName)
    {
        // Force GC
        GC.Collect();
        GC.WaitForPendingFinalizers();

        var sw = Stopwatch.StartNew();
        double result = initial;

        for (int i = 0; i < TEST_ITERATIONS; i++)
        {
            result *= multiplier;
        }

        sw.Stop();

        bool isDenormal = IsDenormalized(result);
        Console.WriteLine($"  {testName,-18}: {sw.ElapsedMilliseconds,4} ms " +
                         $"(final: {result:E2}, denormal: {isDenormal})");

        // Prevent dead code elimination
        if (result == double.MaxValue) Console.WriteLine("Impossible");

        return sw.ElapsedMilliseconds;
    }

    private static void TestDenormalPersistence()
    {
        Console.WriteLine("\nTesting how long numbers stay denormal:");

        // Test different multipliers to see optimization effects
        TestDenormalPersistenceFloat(0.9f, "Aggressive Decay");
        TestDenormalPersistenceFloat(0.99f, "Moderate Decay");
        TestDenormalPersistenceFloat(0.999999f, "Slow Decay");

        TestDenormalPersistenceDouble(0.9, "Aggressive Decay");
        TestDenormalPersistenceDouble(0.99, "Moderate Decay");
        TestDenormalPersistenceDouble(0.999999, "Slow Decay");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void TestDenormalPersistenceFloat(float multiplier, string testName)
    {
        var sw = Stopwatch.StartNew();
        float result = 1E-40f; // Start deep in denormal range
        int denormalCount = 0;

        for (int i = 0; i < TEST_ITERATIONS; i++)
        {
            result *= multiplier;
            if (IsDenormalized(result)) denormalCount++;
        }

        sw.Stop();

        double denormalPercentage = (double)denormalCount / TEST_ITERATIONS * 100;
        Console.WriteLine($"  Float {testName,-15}: {sw.ElapsedMilliseconds,4} ms " +
                         $"({denormalPercentage:F1}% denormal operations)");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void TestDenormalPersistenceDouble(double multiplier, string testName)
    {
        var sw = Stopwatch.StartNew();
        double result = 1E-320; // Start deep in denormal range
        int denormalCount = 0;

        for (int i = 0; i < TEST_ITERATIONS; i++)
        {
            result *= multiplier;
            if (IsDenormalized(result)) denormalCount++;
        }

        sw.Stop();

        double denormalPercentage = (double)denormalCount / TEST_ITERATIONS * 100;
        Console.WriteLine($"  Double {testName,-14}: {sw.ElapsedMilliseconds,4} ms " +
                         $"({denormalPercentage:F1}% denormal operations)");
    }

    private static bool IsDenormalized(float value)
    {
        if (float.IsNaN(value) || float.IsInfinity(value) || value == 0.0f)
            return false;
        return Math.Abs(value) < 1.175494E-38f;
    }

    private static bool IsDenormalized(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value) || value == 0.0)
            return false;
        return Math.Abs(value) < 2.225074E-308;
    }

    private static bool IsDebugBuild()
    {
#if DEBUG
        return true;
#else
        return false;
#endif
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool IsJitOptimized()
    {
        // This method will be optimized away in release builds if JIT optimization is enabled
        return !IsDebugBuild();
    }
}