using System;
using System.Runtime.InteropServices;

namespace toolbelt
{
    public static class Mathematics
    {
        public static float FastInverseSquareRoot(float x)
        {
            Span<float> tmp = new[] { x };
            Span<uint> integer = MemoryMarshal.Cast<float, uint>(tmp);
            // http[://]www[.]lomont[.]org/papers/2003/InvSqrt.pdf
            integer[0] = 0x5f375a86 - (integer[0] >> 1);
            tmp[0] = tmp[0] * (1.5f - x * 0.5f * tmp[0] * tmp[0]);
            return tmp[0];
        }

        public static double FastInverseSquareRoot(double x)
        {
            Span<double> tmp = new[] { x };
            Span<ulong> integer = MemoryMarshal.Cast<double, ulong>(tmp);
            // http[://]www[.]lomont[.]org/papers/2003/InvSqrt.pdf
            integer[0] = 0x5fe6ec85e7de30da - (integer[0] >> 1);
            tmp[0] = tmp[0] * (1.5 - x * 0.5 * tmp[0] * tmp[0]);
            return tmp[0];
        }

        public static float FastSquareRoot(float x)
        {
            return x * FastInverseSquareRoot(x);
        }

        public static double FastSquareRoot(double x)
        {
            return x * FastInverseSquareRoot(x);
        }
    }
}