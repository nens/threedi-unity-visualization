using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public enum LeftOnRight
    {
        Left, On, Right
    }

    public static class GISTerrainLoaderGeometry
    {
        public static DVector2 CalculateCircleCenter(DVector2 a, DVector2 b, DVector2 c)
        {
            if (!IsPerpendicular(a, b, c))
            {
                return GetCircleCenter(a, b, c);
            }
            else if (!IsPerpendicular(a, c, b))
            {
                return GetCircleCenter(a, c, b);
            }
            else if (!IsPerpendicular(b, a, c))
            {
                return GetCircleCenter(b, a, c);
            }
            else if (!IsPerpendicular(b, c, a))
            {
                return GetCircleCenter(b, c, a);
            }
            else if (!IsPerpendicular(c, b, a))
            {
                return GetCircleCenter(c, b, a);
            }
            else if (!IsPerpendicular(c, a, b))
            {
                return GetCircleCenter(c, a, b);
            }
            else
            {
                Debug.LogWarning("Cant calculate circle center because all points are on same line");

                return new DVector2(-100f, -100f);
            }
        }
        private static DVector2 GetCircleCenter(DVector2 a, DVector2 b, DVector2 c)
        {
            double yDelta_a = b.y - a.y;
            double xDelta_a = b.x - a.x;
            double yDelta_b = c.y - b.y;
            double xDelta_b = c.x - b.x;

            float tolerance = 0.00001f;
            if (Mathf.Abs((float)xDelta_a) <= tolerance && Mathf.Abs((float)yDelta_b) <= tolerance)
            {
                double center_special_X = 0.5f * (b.x + c.x);
                double center_special_Y = 0.5f * (a.y + b.y);

                DVector2 center_special = new DVector2(center_special_X, center_special_Y);

                return center_special;
            }
            double ma = (b.y - a.y) / (b.x - a.x);
            double mb = (c.y - b.y) / (c.x - b.x);

            double centerX = (ma * mb * (a.y - c.y) + mb * (a.x + b.x) - ma * (b.x + c.x)) / (2 * (mb - ma));

            double centerY = (-1f / ma) * (centerX - (a.x + b.x) / 2f) + (a.y + b.y) / 2f;

            DVector2 center = new DVector2(centerX, centerY);

            return center;
        }
        private static bool IsPerpendicular(DVector2 p1, DVector2 p2, DVector2 p3)
        {
            double yDelta_a = p2.y - p1.y;
            double xDelta_a = p2.x - p1.x;
            double yDelta_b = p3.y - p2.y;
            double xDelta_b = p3.x - p2.x;

            float tolerance = 0.00001f;

            if (Mathf.Abs((float)xDelta_a) <= tolerance && Mathf.Abs((float)yDelta_b) <= tolerance)
            {
                return false;
            }
            
            if (Mathf.Abs((float)yDelta_a) <= tolerance)
            {
                return true;
            }
            else if (Mathf.Abs((float)yDelta_b) <= tolerance)
            {
                return true;
            }
            else if (Mathf.Abs((float)xDelta_a) <= tolerance)
            {
                return true;
            }
            else if (Mathf.Abs((float)xDelta_b) <= tolerance)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static bool IsTriangleOrientedClockwise(DVector2 p1, DVector2 p2, DVector2 p3)
        {
            bool isClockWise = true;

            double determinant = p1.x * p2.y + p3.x * p1.y + p2.x * p3.y - p1.x * p3.y - p3.x * p2.y - p2.x * p1.y;

            if (determinant > 0f)
            {
                isClockWise = false;
            }

            return isClockWise;
        }
        public static double GetPointInRelationToVectorValue(DVector2 a, DVector2 b, DVector2 p)
        {
            double x1 = a.x - p.x;
            double x2 = a.y - p.y;
            double y1 = b.x - p.x;
            double y2 = b.y - p.y;

            double determinant = GISTerrainLoaderMathUtility.Det2(x1, x2, y1, y2);

            return determinant;
        }

        public static bool IsPointLeftOfVector(DVector2 a, DVector2 b, DVector2 p)
        {
            double relationValue = GetPointInRelationToVectorValue(a, b, p);

            bool isToLeft = true;

            float epsilon = GISTerrainLoaderMathUtility.EPSILON;

            if (relationValue < 0f - epsilon)
            {
                isToLeft = false;
            }

            return isToLeft;
        }

        public static LeftOnRight IsPoint_Left_On_Right_OfVector(DVector2 a, DVector2 b, DVector2 p)
        {
            double relationValue = GetPointInRelationToVectorValue(a, b, p);

            float epsilon = GISTerrainLoaderMathUtility.EPSILON;

            if (relationValue < -epsilon)
            {
                return LeftOnRight.Right;
            }
            else if (relationValue > epsilon)
            {
                return LeftOnRight.Left;
            }
            else
            {
                return LeftOnRight.On;
            }
        }

        public static double DistanceFromPointToPlane(DVector2 planeNormal, DVector2 planePos, DVector2 pointPos)
        {
            double distance = DVector2.Dot(planeNormal, pointPos - planePos);

            return distance;
        }
        public static bool IsQuadrilateralConvex(DVector2 a, DVector2 b, DVector2 c, DVector2 d)
        {
            bool isConvex = false;

            bool abc = GISTerrainLoaderGeometry.IsTriangleOrientedClockwise(a, b, c);
            bool abd = GISTerrainLoaderGeometry.IsTriangleOrientedClockwise(a, b, d);
            bool bcd = GISTerrainLoaderGeometry.IsTriangleOrientedClockwise(b, c, d);
            bool cad = GISTerrainLoaderGeometry.IsTriangleOrientedClockwise(c, a, d);

            if (abc && abd && bcd & !cad)
            {
                isConvex = true;
            }
            else if (abc && abd && !bcd & cad)
            {
                isConvex = true;
            }
            else if (abc && !abd && bcd & cad)
            {
                isConvex = true;
            }
            else if (!abc && !abd && !bcd & cad)
            {
                isConvex = true;
            }
            else if (!abc && !abd && bcd & !cad)
            {
                isConvex = true;
            }
            else if (!abc && abd && !bcd & !cad)
            {
                isConvex = true;
            }


            return isConvex;
        }
        public static bool IsPointBetweenPoints(DVector2 a, DVector2 b, DVector2 p)
        {
            bool isBetween = false;

            DVector2 ab = b - a;

            DVector2 ap = p - a;

            if (DVector2.Dot(ab, ap) > 0f && DVector2.SqrMagnitude(ab) >= DVector2.SqrMagnitude(ap))
            {
                isBetween = true;
            }

            return isBetween;
        }
        public static DVector2 GetClosestPointOnLineSegment(DVector2 a, DVector2 b, DVector2 p)
        {
            DVector2 a_p = p - a;
            DVector2 a_b = b - a;

            double sqrMagnitudeAB = DVector2.SqrMagnitude(a_b);

            double ABAPproduct = DVector2.Dot(a_p, a_b);

            double distance = ABAPproduct / sqrMagnitudeAB;

            if (distance < 0)
            {
                return a;
            }
            else if (distance > 1)
            {
                return b;
            }
            else
            {
                return a + a_b * distance;
            }
        }
        public static GISTerrainLoaderTriangle2 GenerateSupertriangle(HashSet<DVector2> points)
        {
            GISTerrainLoaderAABB2 aabb = new GISTerrainLoaderAABB2(new List<DVector2>(points));

            DVector2 TL = new DVector2(aabb.minX, aabb.maxY);
            DVector2 TR = new DVector2(aabb.maxX, aabb.maxY);
            DVector2 BR = new DVector2(aabb.maxX, aabb.minY);

            DVector2 circleCenter = (TL + BR) * 0.5f;

            double circleRadius = DVector2.Magnitude(circleCenter - TR);

            double halfSideLenghth = circleRadius / Mathf.Tan(30f * Mathf.Deg2Rad);

            DVector2 t_B = new DVector2(circleCenter.x, circleCenter.y - circleRadius);

            DVector2 t_BL = new DVector2(t_B.x - halfSideLenghth, t_B.y);
            DVector2 t_BR = new DVector2(t_B.x + halfSideLenghth, t_B.y);

            double triangleHeight = halfSideLenghth * Mathf.Tan(60f * Mathf.Deg2Rad);

            DVector2 t_T = new DVector2(circleCenter.x, t_B.y + triangleHeight);


            GISTerrainLoaderTriangle2 superTriangle = new GISTerrainLoaderTriangle2(t_BR, t_BL, t_T);

            return superTriangle;
        }
        public static bool HasPassedWaypoint(DVector2 wp1, DVector2 wp2, DVector2 p)
        {
            DVector2 a = p - wp1;

            DVector2 b = wp2 - wp1;

            double progress = (a.x * b.x + a.y * b.y) / (b.x * b.x + b.y * b.y);

            if (progress > 1.0f + GISTerrainLoaderMathUtility.EPSILON)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
