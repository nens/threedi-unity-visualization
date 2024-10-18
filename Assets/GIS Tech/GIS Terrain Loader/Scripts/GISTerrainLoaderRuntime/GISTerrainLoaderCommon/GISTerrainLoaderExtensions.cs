/*     Unity GIS Tech 2020-2023      */

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
 
namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderExtensions
    {
  
        public static double GetDouble(string value, double defaultValue = 0)
        {
            double result;
            if (!double.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out result) &&
                !double.TryParse(value, NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out result) &&
                !double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
                result = defaultValue;
            return result;
        }
        public static double ConvertToDouble(string s)
        {
            char systemSeparator = Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencyDecimalSeparator[0];
            double result = 0;
            try
            {
                if (s != null)
                    if (!s.Contains(","))
                        result = double.Parse(s, CultureInfo.InvariantCulture);
                    else
                        result = Convert.ToDouble(s.Replace(".", systemSeparator.ToString()).Replace(",", systemSeparator.ToString()));
            }
            catch (Exception e)
            {
                try
                {
                    result = Convert.ToDouble(s);
                }
                catch
                {
                    try
                    {
                        result = Convert.ToDouble(s.Replace(",", ";").Replace(".", ",").Replace(";", "."));
                    }
                    catch
                    {
                        throw new Exception("Wrong string-to-double format  :" + e.Message);
                    }
                }
            }
            return result;
        }
        public static bool IsSubRegionIncluded(DVector2 FileUpperLeftCoordiante, DVector2 FileDownRightCoordiante, DVector2 SubRegionUpperLeftCoordiante, DVector2 SubRegionDownRightCoordiante)
        {
            bool Included = true;

            if (SubRegionUpperLeftCoordiante.x >= SubRegionDownRightCoordiante.x)
            {
                Debug.LogError("Down-Right Longitude must be greater than Top-Left Longitude");
                Included = false;
            }
            if (SubRegionUpperLeftCoordiante.y <= SubRegionDownRightCoordiante.y)
            {
                Debug.LogError("Top-Left Latitude must be greater than Bottom-Right Latitude");
                Included = false;
            }
            //-------
            if (SubRegionUpperLeftCoordiante.x < FileUpperLeftCoordiante.x)
            {
                Debug.LogError("Sub region Top-Left Longitude must be greater or equal than file Top-Left Longitude");
                Included = false;
            }

            if (SubRegionUpperLeftCoordiante.y > FileUpperLeftCoordiante.y)
            {
                Debug.LogError("Sub region Top-Left Latitude must be smaller or equal than file Top-Left Latitude");
                Included = false;
            }
            //-------
            if (SubRegionDownRightCoordiante.x > FileDownRightCoordiante.x)
            {
                Debug.LogError("Sub region Top-Left Longitude must be smaller or equal than file Top-Left Longitude");
                Included = false;
            }

            if (SubRegionDownRightCoordiante.y < FileDownRightCoordiante.y)
            {
                Debug.LogError("Sub region Down-Right Latitude must be greater or equal than file Top-Left Latitude");
                Included = false;
            }

            return Included;
        }
        public static Vector3 GetLocalLocation(GISTerrainLoaderFileData data, DVector2 point)
        {
            var rang_x = Math.Abs(Math.Abs(data.DRPoint_LatLon.x) - Math.Abs(data.TLPoint_LatLon.x));
            var rang_y = Math.Abs(Math.Abs(data.TLPoint_LatLon.y) - Math.Abs(data.DRPoint_LatLon.y));

            var rang_px = Math.Abs(Math.Abs(point.x) - Math.Abs(data.TLPoint_LatLon.x));
            var rang_py = Math.Abs(Math.Abs(data.TLPoint_LatLon.y) - Math.Abs(point.y));

            int localLat = (int)(rang_px * data.mapSize_col_x / rang_x);
            int localLon = (int)(rang_py * data.mapSize_row_y / rang_y);

            if (localLat > data.mapSize_col_x - 1) localLat = data.mapSize_col_x - 1;
            if (localLon > data.mapSize_row_y - 1) localLon = data.mapSize_row_y - 1;
            var elevation = data.floatheightData[localLat, localLon];

            return new Vector3(localLat, localLon, elevation);
        }
        public static short ToBigEndian(short value) => System.Net.IPAddress.HostToNetworkOrder(value);
        public static int ToBigEndian(int value) => System.Net.IPAddress.HostToNetworkOrder(value);
        public static long ToBigEndian(long value) => System.Net.IPAddress.HostToNetworkOrder(value);
        public static short FromBigEndian(short value) => System.Net.IPAddress.NetworkToHostOrder(value);
        public static int FromBigEndian(int value) => System.Net.IPAddress.NetworkToHostOrder(value);
        public static long FromBigEndian(long value) => System.Net.IPAddress.NetworkToHostOrder(value);
        public static ulong FromBigToLittle(int code)
        {
            ulong value;
            value = ((((ulong)(code) & (ulong)0x000000ffUL) << 24) | (((ulong)(code) & (ulong)0x0000ff00UL) << 8) | (((ulong)(code) & (ulong)0x00ff0000UL) >> 8) | (((ulong)(code) & (ulong)0xff000000UL) >> 24));
            return value;
        }
        public static byte[] FromLittleToBig(byte[] lbt)
        {
            byte[] bbt = new byte[4];
            bbt[0] = lbt[3];
            bbt[1] = lbt[2];
            bbt[2] = lbt[1];
            bbt[3] = lbt[0];
            return bbt;
        }
        public static long GetArraySize(Array arr)
        {
            return arr.LongLength * Marshal.SizeOf(arr.GetType().GetElementType());
        }
        public static Color HexToColor(string hex)
        {
            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            return new Color32(r, g, b, 255);
        }
        public static bool OnlyHexInString(string test)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(test, @"\A\b[0-9a-fA-F]+\b\Z");
        }
 
    }
    public static class cortineExtensions
    {
        public static IEnumerator AsIEnumerator(this Task task)
        {
            while (!task.IsCompleted)
            {
                Debug.Log(task.Status);
                yield return null;
            }

            if (task.IsFaulted)
            {
                throw task.Exception;
            }
        }
    }
        public static class TransformExtensions
    {
        /// <summary>
        /// Updates the local eulerAngles to a new vector3, if a value is omitted then the old value will be used.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public static void SetLocalEulerAngles(this Transform transform, float? x = null, float? y = null, float? z = null)
        {
            var vector = new Vector3();
            if (x != null) { vector.x = x.Value; } else { vector.x = transform.localEulerAngles.x; }
            if (y != null) { vector.y = y.Value; } else { vector.y = transform.localEulerAngles.y; }
            if (z != null) { vector.z = z.Value; } else { vector.z = transform.localEulerAngles.z; }
            transform.localEulerAngles = vector;
        }

        /// <summary>
        /// Updates the position to a new vector3, if a value is omitted then the old value will be used.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public static void SetPosition(this Transform transform, float? x = null, float? y = null, float? z = null)
        {
            var vector = new Vector3();
            if (x != null) { vector.x = x.Value; } else { vector.x = transform.position.x; }
            if (y != null) { vector.y = y.Value; } else { vector.y = transform.position.y; }
            if (z != null) { vector.z = z.Value; } else { vector.z = transform.position.z; }
            transform.position = vector;
        }

        public static void DestroyChildren(this Transform t)
        {
            bool isPlaying = Application.isPlaying;

            while (t.childCount != 0)
            {
                Transform child = t.GetChild(0);

                if (isPlaying)
                {
                    child.parent = null;
                    UnityEngine.Object.Destroy(child.gameObject);
                }
                else UnityEngine.Object.DestroyImmediate(child.gameObject);
            }
        }

        public static bool IsPrefab(this GameObject t)
        {
            bool isprefab = false;
#if UNITY_EDITOR

#if UNITY_2018_3_OR_NEWER
            isprefab = UnityEditor.PrefabUtility.IsPartOfAnyPrefab(t);

#else
	        isprefab = PrefabUtility.GetPrefabType(t) != PrefabType.None;
#endif
#endif
            return isprefab;
        }
        public static void UnpackPrefab(this GameObject t)
        {
#if UNITY_EDITOR
             UnityEditor.PrefabUtility.UnpackPrefabInstance(t,UnityEditor.PrefabUnpackMode.Completely,UnityEditor.InteractionMode.AutomatedAction);
 
#endif
        }
    }
    public static class GameObjectExtensions
    {
        public static void Destroy(this UnityEngine.Object obj, bool deleteAsset = false)
        {
            if (Application.isEditor && !Application.isPlaying)
            {
                GameObject.DestroyImmediate(obj, deleteAsset);
            }
            else
            {
                GameObject.Destroy(obj);
            }
        }
    }
    public struct GISTerrainLoaderMath
    {
        public const double PI = 3.141593d;
        public const double Infinity = double.PositiveInfinity;
        public const double NegativeInfinity = double.NegativeInfinity;
        public const double Deg2Rad = 0.01745329d;
        public const double Rad2Deg = 57.29578d;
        public const double Epsilon = 1.401298E-45d;

        public static float SlopeCal(float Z_Pos_x, float Z_Pos_y)
        {
            float Z_Pos = Z_Pos_x * Z_Pos_x + Z_Pos_y * Z_Pos_y;
            float Sq_Pos = SqrtCal(Z_Pos);

            return Mathf.Atan(Sq_Pos) * (180.0f / (float)Math.PI) / 90.0f;
        }
        public static float SqrtCal(float Value)
        {
            if (Value <= 0.0f) return 0.0f;
            return (float)Math.Sqrt(Value);
        }
        public static float SignOrZero(float v)
        {
            if (v == 0) return 0;
            return Math.Sign(v);
        }

        public static double Sin(double d)
        {
            return Math.Sin(d);
        }

        public static double Cos(double d)
        {
            return Math.Cos(d);
        }

        public static double Tan(double d)
        {
            return Math.Tan(d);
        }

        public static double Asin(double d)
        {
            return Math.Asin(d);
        }

        public static double Acos(double d)
        {
            return Math.Acos(d);
        }

        public static double Atan(double d)
        {
            return Math.Atan(d);
        }

        public static double Atan2(double y, double x)
        {
            return Math.Atan2(y, x);
        }

        public static double Sqrt(double d)
        {
            return Math.Sqrt(d);
        }

        public static double Abs(double d)
        {
            return Math.Abs(d);
        }

        public static int Abs(int value)
        {
            return Math.Abs(value);
        }

        public static double Min(double a, double b)
        {
            if (a < b)
                return a;
            else
                return b;
        }

        public static double Min(params double[] values)
        {
            int length = values.Length;
            if (length == 0)
                return 0.0d;
            double num = values[0];
            for (int index = 1; index < length; ++index)
            {
                if (values[index] < num)
                    num = values[index];
            }
            return num;
        }

        public static int Min(int a, int b)
        {
            if (a < b)
                return a;
            else
                return b;
        }

        public static int Min(params int[] values)
        {
            int length = values.Length;
            if (length == 0)
                return 0;
            int num = values[0];
            for (int index = 1; index < length; ++index)
            {
                if (values[index] < num)
                    num = values[index];
            }
            return num;
        }

        public static double Max(double a, double b)
        {
            if (a > b)
                return a;
            else
                return b;
        }

        public static double Max(params double[] values)
        {
            int length = values.Length;
            if (length == 0)
                return 0d;
            double num = values[0];
            for (int index = 1; index < length; ++index)
            {
                if ((double)values[index] > (double)num)
                    num = values[index];
            }
            return num;
        }

        public static int Max(int a, int b)
        {
            if (a > b)
                return a;
            else
                return b;
        }

        public static int Max(params int[] values)
        {
            int length = values.Length;
            if (length == 0)
                return 0;
            int num = values[0];
            for (int index = 1; index < length; ++index)
            {
                if (values[index] > num)
                    num = values[index];
            }
            return num;
        }

        public static double Pow(double d, double p)
        {
            return Math.Pow(d, p);
        }

        public static double Exp(double power)
        {
            return Math.Exp(power);
        }

        public static double Log(double d, double p)
        {
            return Math.Log(d, p);
        }

        public static double Log(double d)
        {
            return Math.Log(d);
        }

        public static double Log10(double d)
        {
            return Math.Log10(d);
        }

        public static double Ceil(double d)
        {
            return Math.Ceiling(d);
        }

        public static double Floor(double d)
        {
            return Math.Floor(d);
        }

        public static double Round(double d)
        {
            return Math.Round(d);
        }

        public static int CeilToInt(double d)
        {
            return (int)Math.Ceiling(d);
        }

        public static int FloorToInt(double d)
        {
            return (int)Math.Floor(d);
        }

        public static int RoundToInt(double d)
        {
            return (int)Math.Round(d);
        }

        public static double Sign(double d)
        {
            return d >= 0.0 ? 1d : -1d;
        }

        public static double Clamp(double value, double min, double max)
        {
            if (value < min)
                value = min;
            else if (value > max)
                value = max;
            return value;
        }

        public static int Clamp(int value, int min, int max)
        {
            if (value < min)
                value = min;
            else if (value > max)
                value = max;
            return value;
        }

        public static double Clamp01(double value)
        {
            if (value < 0.0)
                return 0.0d;
            if (value > 1.0)
                return 1d;
            else
                return value;
        }

        public static double Lerp(double from, double to, double t)
        {
            return from + (to - from) * GISTerrainLoaderMath.Clamp01(t);
        }

        public static double LerpAngle(double a, double b, double t)
        {
            double num = GISTerrainLoaderMath.Repeat(b - a, 360d);
            if (num > 180.0d)
                num -= 360d;
            return a + num * GISTerrainLoaderMath.Clamp01(t);
        }

        public static double MoveTowards(double current, double target, double maxDelta)
        {
            if (GISTerrainLoaderMath.Abs(target - current) <= maxDelta)
                return target;
            else
                return current + GISTerrainLoaderMath.Sign(target - current) * maxDelta;
        }

        public static double MoveTowardsAngle(double current, double target, double maxDelta)
        {
            target = current + GISTerrainLoaderMath.DeltaAngle(current, target);
            return GISTerrainLoaderMath.MoveTowards(current, target, maxDelta);
        }

        public static double SmoothStep(double from, double to, double t)
        {
            t = GISTerrainLoaderMath.Clamp01(t);
            t = (-2.0 * t * t * t + 3.0 * t * t);
            return to * t + from * (1.0 - t);
        }

        public static double Gamma(double value, double absmax, double gamma)
        {
            bool flag = false;
            if (value < 0.0)
                flag = true;
            double num1 = GISTerrainLoaderMath.Abs(value);
            if (num1 > absmax)
            {
                if (flag)
                    return -num1;
                else
                    return num1;
            }
            else
            {
                double num2 = GISTerrainLoaderMath.Pow(num1 / absmax, gamma) * absmax;
                if (flag)
                    return -num2;
                else
                    return num2;
            }
        }

        public static bool Approximately(double a, double b)
        {
            return GISTerrainLoaderMath.Abs(b - a) < GISTerrainLoaderMath.Max(1E-06d * GISTerrainLoaderMath.Max(GISTerrainLoaderMath.Abs(a), GISTerrainLoaderMath.Abs(b)), 1.121039E-44d);
        }

        public static double SmoothDamp(double current, double target, ref double currentVelocity, double smoothTime, double maxSpeed, double deltaTime)
        {
            smoothTime = GISTerrainLoaderMath.Max(0.0001d, smoothTime);
            double num1 = 2d / smoothTime;
            double num2 = num1 * deltaTime;
            double num3 = (1.0d / (1.0d + num2 + 0.479999989271164d * num2 * num2 + 0.234999999403954d * num2 * num2 * num2));
            double num4 = current - target;
            double num5 = target;
            double max = maxSpeed * smoothTime;
            double num6 = GISTerrainLoaderMath.Clamp(num4, -max, max);
            target = current - num6;
            double num7 = (currentVelocity + num1 * num6) * deltaTime;
            currentVelocity = (currentVelocity - num1 * num7) * num3;
            double num8 = target + (num6 + num7) * num3;
            if (num5 - current > 0.0 == num8 > num5)
            {
                num8 = num5;
                currentVelocity = (num8 - num5) / deltaTime;
            }
            return num8;
        }

        public static double SmoothDampAngle(double current, double target, ref double currentVelocity, double smoothTime, double maxSpeed, double deltaTime)
        {
            target = current + GISTerrainLoaderMath.DeltaAngle(current, target);
            return GISTerrainLoaderMath.SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
        }

        public static double Repeat(double t, double length)
        {
            return t - GISTerrainLoaderMath.Floor(t / length) * length;
        }

        public static double PingPong(double t, double length)
        {
            t = GISTerrainLoaderMath.Repeat(t, length * 2d);
            return length - GISTerrainLoaderMath.Abs(t - length);
        }

        public static double InverseLerp(double from, double to, double value)
        {
            if (from < to)
            {
                if (value < from)
                    return 0d;
                if (value > to)
                    return 1d;
                value -= from;
                value /= to - from;
                return value;
            }
            else
            {
                if (from <= to)
                    return 0d;
                if (value < to)
                    return 1d;
                if (value > from)
                    return 0d;
                else
                    return (1.0d - (value - to) / (from - to));
            }
        }

        public static double DeltaAngle(double current, double target)
        {
            double num = GISTerrainLoaderMath.Repeat(target - current, 360d);
            if (num > 180.0d)
                num -= 360d;
            return num;
        }

        internal static bool LineIntersection(DVector2 p1, DVector2 p2, DVector2 p3, DVector2 p4, ref DVector2 result)
        {
            double num1 = p2.x - p1.x;
            double num2 = p2.y - p1.y;
            double num3 = p4.x - p3.x;
            double num4 = p4.y - p3.y;
            double num5 = num1 * num4 - num2 * num3;
            if (num5 == 0.0d)
                return false;
            double num6 = p3.x - p1.x;
            double num7 = p3.y - p1.y;
            double num8 = (num6 * num4 - num7 * num3) / num5;
            result = new DVector2(p1.x + num8 * num1, p1.y + num8 * num2);
            return true;
        }

        internal static bool LineSegmentIntersection(DVector2 p1, DVector2 p2, DVector2 p3, DVector2 p4, ref DVector2 result)
        {
            double num1 = p2.x - p1.x;
            double num2 = p2.y - p1.y;
            double num3 = p4.x - p3.x;
            double num4 = p4.y - p3.y;
            double num5 = (num1 * num4 - num2 * num3);
            if (num5 == 0.0d)
                return false;
            double num6 = p3.x - p1.x;
            double num7 = p3.y - p1.y;
            double num8 = (num6 * num4 - num7 * num3) / num5;
            if (num8 < 0.0d || num8 > 1.0d)
                return false;
            double num9 = (num6 * num2 - num7 * num1) / num5;
            if (num9 < 0.0d || num9 > 1.0d)
                return false;
            result = new DVector2(p1.x + num8 * num1, p1.y + num8 * num2);
            return true;
        }
    }
    public static class GISTerrainLoaderEnumExtension
    {
        public static string GetDescription(this Enum e)
        {
            var attribute =
                e.GetType()
                    .GetTypeInfo()
                    .GetMember(e.ToString())
                    .FirstOrDefault(member => member.MemberType == MemberTypes.Field)
                    .GetCustomAttributes(typeof(DescriptionAttribute), false)
                    .SingleOrDefault()
                    as DescriptionAttribute;

            return attribute?.Description ?? e.ToString();
        }
        public static IEnumerable<Enum> GetEnumValues(this Enum e)
        {
            // Can't use type constraints on value types, so have to do check like this
            if (typeof(Enum).BaseType != typeof(Enum))
            {
                throw new ArgumentException("T must be of type System.Enum");
            }

            return Enum.GetValues(typeof(Enum)).Cast<Enum>();
        }
        public static bool Contains(this Enum e, string enu)
        {
            bool contains = false;

            var values = GetEnumValues(e);

            foreach (var val in values)
            {
                if (val.ToString() == enu)
                    contains = true;
            }

            return contains;
        }
    }

    [Serializable]
 
    public class DVector2
    {
        public static DVector2 Zero => new DVector2(0, 0);

        public double x;
        public double y;

        private static System.Random _random = new System.Random();

        public DVector2(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public void Reset()
        {
            x = 0;
            y = 0;
        }

        public void Normalize()
        {
            double length = Length();

            x /= length;
            y /= length;
        }

        public DVector2 Normalized()
        {
            return Clone() / Length();
        }
        public static DVector2 Normalize(DVector2 v)
        {
            double v_magnitude = Magnitude(v);

            DVector2 v_normalized = new DVector2(v.x / v_magnitude, v.y / v_magnitude);

            return v_normalized;
        }
        public static double Magnitude(DVector2 a)
        {
            double magnitude = Mathf.Sqrt((float)SqrMagnitude(a));

            return magnitude;
        }
        public static double SqrMagnitude(DVector2 a)
        {
            double sqrMagnitude = (a.x * a.x) + (a.y * a.y);

            return sqrMagnitude;
        }
        public static double SqrDistance(DVector2 a, DVector2 b)
        {
            double distance = SqrMagnitude(a - b);

            return distance;
        }

        public void Negate()
        {
            x = -x;
            y = -y;
        }

        public DVector2 Clone()
        {
            return new DVector2(x, y);
        }

        public static DVector2 operator +(DVector2 a, DVector2 b)
        {
            return new DVector2(a.x + b.x, a.y + b.y);
        }

        public static DVector2 operator -(DVector2 a, DVector2 b)
        {
            return new DVector2(a.x - b.x, a.y - b.y);
        }
        public static DVector2 operator -(DVector2 a)
        {
            return a * -1f;
        }
        public static DVector2 operator *(DVector2 a, double b)
        {
            return new DVector2(a.x * b, a.y * b);
        }
        public static DVector2 operator *(DVector2 a, float b)
        {
            return new DVector2(a.x * b, a.y * b);
        }
        public static DVector2 operator *(double b, DVector2 a)
        {
            return new DVector2(a.x * b, a.y * b);
        }
        public static DVector2 operator *(float b, DVector2 a)
        {
            return new DVector2(a.x * b, a.y * b);
        }
        public static DVector2 operator /(DVector2 a, DVector2 b)
        {
            return new DVector2(a.x / b.x, a.y / b.y);
        }

        public static DVector2 operator /(DVector2 a, double b)
        {
            return new DVector2(a.x / b, a.y / b);
        }

        public void Accumulate(DVector2 other)
        {
            x += other.x;
            y += other.y;
        }

        public DVector2 Divide(float scalar)
        {
            return new DVector2(x / scalar, y / scalar);
        }

        public DVector2 Divide(double scalar)
        {
            return new DVector2(x / scalar, y / scalar);
        }

        public double Dot(DVector2 v)
        {
            return x * v.x + y * v.y;
        }
        public static double Dot(DVector2 a, DVector2 b)
        {
            double dotProduct = (a.x * b.x) + (a.y * b.y);

            return dotProduct;
        }

        public double Cross(DVector2 v)
        {
            return x * v.y - y * v.x;
        }

        public double Length()
        {
            return Math.Sqrt(x * x + y * y);
        }

        public double LengthSquared()
        {
            return x * x + y * y;
        }

        public double Angle()
        {
            return Math.Atan2(y, x);
        }

        public static DVector2 Lerp(DVector2 from, DVector2 to, double t)
        {
            return new DVector2(from.x + t * (to.x - from.x),
                               from.y + t * (to.y - from.y));
        }

        public static DVector2 FromAngle(double angle)
        {
            return new DVector2(Math.Cos(angle), Math.Sin(angle));
        }

        public static double Distance(DVector2 v1, DVector2 v2)
        {
            return (v2 - v1).Length();
        }

        public static DVector2 RandomUnitVector()
        {
            double angle = _random.NextDouble() * Math.PI * 2;

            return new DVector2(Math.Cos(angle), Math.Sin(angle));
        }

        public override string ToString()
        {
            return "{" + Math.Round(x, 5) + "," + Math.Round(y, 5) + "}";
        }
        public float sqrMagnitude
        {
            get
            {
                return (float)(this.x * (double)this.x + this.y * (double)this.y);
            }
        }
        public Vector2Int ToIntVector()
        {
            return new Vector2Int((int)x, (int)y);
        }
    }
    [Serializable]
    public class DVector3
    {
        public double x;
        public double y;
        public double z;

        private const double radianTodegree = 180.0 / Math.PI;

        public DVector3(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public double? Measure
        {
            get { return m_Measure; }
            set { m_Measure = value; }
        }
        protected double? m_Measure = null;



        public void translate(double x, double y, double z)
        {

            this.x += x;
            this.y += y;
            this.z += z;
        }
        private void Scale(double scale)
        {
            this.x *= scale;
            this.y *= scale;
            this.z *= scale;
        }

        public void toDegree()
        {
            Scale(radianTodegree);
        }
        public string toString()
        {
            return this.x + " " + this.y + " " + this.z;

        }
        public DVector2 ToDVector2()
        {
            return new DVector2(this.x, this.y);
        }

    }

}