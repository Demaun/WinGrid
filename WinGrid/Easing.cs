using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinGridApp
{
    public static class Easing
    {
        public enum BoundType { Clamp, Loop, Oscilate, None, Default }
        public static BoundType defaultBoundType = BoundType.Clamp;

        private static void FixDefault(ref BoundType boundType)
        {
            if (defaultBoundType == BoundType.Default)
                defaultBoundType = BoundType.Clamp;
            if (boundType == BoundType.Default)
                boundType = defaultBoundType;
        }

        public static float Bound(float value, BoundType boundType = BoundType.Default)
        {
            if (boundType == BoundType.None) return value;
            if (value >= 0 && value <= 1) return value;

            FixDefault(ref boundType);

            float result = value;
            switch (boundType)
            {
                case BoundType.Clamp:
                    result = Clamp01(result);
                    break;

                case BoundType.Loop:
                    result %= 1;
                    if (result < 0)
                        result++;
                    break;

                case BoundType.Oscilate:
                    result = Math.Abs(result);
                    bool dir = Math.Floor(result) % 2 != 0;
                    result %= 1;
                    if (dir)
                        result = 1 - result;
                    break;
            }
            return result;
        }

        public static float SmoothStart(float value, float strength = 1, BoundType boundType = BoundType.Default)
        {
            value = Bound(value, (BoundType)boundType);
            bool reverse = false;
            if (strength < 0)
            {
                reverse = true;
                value = 1 - value;
            }

            strength = Math.Abs(strength);
            int count = (int)Math.Floor(strength);

            for (int i = 0; i < count; i++)
                value *= value;

            if (strength % 1 != 0)
                value = Lerp(value, value * value, strength % 1);

            if (reverse)
                return 1 - value;
            else
                return value;
        }

        public static float SmoothEnd(float value, float strength = 1, BoundType boundType = BoundType.Default)
        {
            value = Bound(value, boundType);
            if (strength < 0)
                return SmoothStart(value, Math.Abs(strength), boundType);
            else
                return 1 - SmoothStart(1 - value, strength, BoundType.None);
        }

        public static float SmoothStep(float value, float strength = 1, BoundType boundType = BoundType.Default)
        {
            value = Bound(value, boundType);
            if (strength < 0)
                return SharpStep(value, Math.Abs(strength), boundType);
            else
            {
                if (value <= .5f)
                    return SmoothStart(value * 2, strength, BoundType.None) * .5f;
                else
                    return SmoothEnd(value * 2 - 1, strength, BoundType.None) * .5f + .5f;
            }
        }

        public static float SharpStep(float value, float strength = 1, BoundType boundType = BoundType.Default)
        {
            value = Bound(value, boundType);
            if (strength < 0)
                return SmoothStep(value, Math.Abs(strength), boundType);
            else
            {
                if (value <= .5f)
                    return SmoothEnd(value * 2, strength, BoundType.None) * .5f;
                else
                    return SmoothStart(value * 2 - 1, strength, BoundType.None) * .5f + .5f;
            }
        }

        public static float Clamp01(float value)
        {
            return value > 1 ? 1 : value < 0 ? 0 : value;
        }

        public static float Lerp (float low, float high, float t)
        {
            return (1 - t) * low + t * high;
        }
    }
}
