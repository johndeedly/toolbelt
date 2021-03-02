using System;

namespace toolbelt
{
    public class ProportionalRandom : Random
    {
        static readonly double maxByte = (double)byte.MaxValue + 1.0;

        static readonly double maxInt = (double)int.MaxValue + 1.0;

        protected override double Sample()
        {
            double sample = base.Sample();
            return sample * sample;
        }

        public override void NextBytes(byte[] buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (byte)(Sample() * maxByte);
            }
        }

        public override int Next()
        {
            return (int)(Sample() * maxInt);
        }

        public override int Next(int maxValue)
        {
            return (int)(Sample() * maxValue);
        }

        public override int Next(int minValue, int maxValue)
        {
            return (int)((Sample() * (maxValue - minValue)) + minValue);
        }
    }
}