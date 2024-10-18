using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public static class GISTerrainLoaderMathUtility
    {
        public const float EPSILON = 0.00001f;
        public static bool AreFloatsEqual(float a, float b)
        {
            float diff = a - b;

            float e = GISTerrainLoaderMathUtility.EPSILON;

            if (diff < e && diff > -e)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static float Remap(float value, float r1_low, float r1_high, float r2_low, float r2_high)
        {
            float remappedValue = r2_low + (value - r1_low) * ((r2_high - r2_low) / (r1_high - r1_low));

            return remappedValue;
        }
        public static int ClampListIndex(int index, int listSize)
        {
            index = ((index % listSize) + listSize) % listSize;

            return index;
        }
        public static float Det2(float x1, float x2, float y1, float y2)
        {
            return (x1 * y2 - y1 * x2);
        }
        public static double Det2(double x1, double x2, double y1, double y2)
        {
            return (x1 * y2 - y1 * x2);
        }
        public static float CalculateAngle(Vector3 from, Vector3 to)
        {
            return Quaternion.FromToRotation(from, to).eulerAngles.y;
        }
        public static float AddValueToAverage(float oldAverage, float valueToAdd, float count)
        {
            float newAverage = ((oldAverage * count) + valueToAdd) / (count + 1f);

            return newAverage;
        }
        public static int RoundValue(float value, float stepValue)
        {
            int roundedValue = (int)(Mathf.Round(value / stepValue) * stepValue);

            return roundedValue;
        }
    }
}
