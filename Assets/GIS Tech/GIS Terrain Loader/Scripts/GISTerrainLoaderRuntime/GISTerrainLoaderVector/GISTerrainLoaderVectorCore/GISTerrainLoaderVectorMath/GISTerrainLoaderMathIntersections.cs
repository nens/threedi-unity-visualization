using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public enum IntersectionCases
    {
        IsInside,
        IsOnEdge,
        NoIntersection
    }

    public static class GISTerrainLoaderMathIntersections
    {
        public static bool LineLine(DVector2 l1_p1, DVector2 l1_p2, DVector2 l2_p1, DVector2 l2_p2, bool shouldIncludeEndPoints)
        {
            float epsilon = GISTerrainLoaderMathUtility.EPSILON;

            bool isIntersecting = false;

            double denominator = (l2_p2.y - l2_p1.y) * (l1_p2.x - l1_p1.x) - (l2_p2.x - l2_p1.x) * (l1_p2.y - l1_p1.y);

            if (denominator != 0f)
            {
                double u_a = ((l2_p2.x - l2_p1.x) * (l1_p1.y - l2_p1.y) - (l2_p2.y - l2_p1.y) * (l1_p1.x - l2_p1.x)) / denominator;
                double u_b = ((l1_p2.x - l1_p1.x) * (l1_p1.y - l2_p1.y) - (l1_p2.y - l1_p1.y) * (l1_p1.x - l2_p1.x)) / denominator;

                if (shouldIncludeEndPoints)
                {
                    if (u_a >= 0f + epsilon && u_a <= 1f - epsilon && u_b >= 0f + epsilon && u_b <= 1f - epsilon)
                    {
                        isIntersecting = true;
                    }
                }
                else
                {
                    if (u_a > 0f + epsilon && u_a < 1f - epsilon && u_b > 0f + epsilon && u_b < 1f - epsilon)
                    {
                        isIntersecting = true;
                    }
                }

            }

            return isIntersecting;
        }
        public static DVector2 GetLineLineIntersectionPoint(DVector2 l1_p1, DVector2 l1_p2, DVector2 l2_p1, DVector2 l2_p2)
        {
            double denominator = (l2_p2.y - l2_p1.y) * (l1_p2.x - l1_p1.x) - (l2_p2.x - l2_p1.x) * (l1_p2.y - l1_p1.y);

            double u_a = ((l2_p2.x - l2_p1.x) * (l1_p1.y - l2_p1.y) - (l2_p2.y - l2_p1.y) * (l1_p1.x - l2_p1.x)) / denominator;

            DVector2 intersectionPoint = l1_p1 + u_a * (l1_p2 - l1_p1);

            return intersectionPoint;
        }
        public static bool RayPlane(DVector2 planePos, DVector2 planeNormal, DVector2 rayStart, DVector2 rayDir)
        {
            double epsilon = GISTerrainLoaderMathUtility.EPSILON;

            bool areIntersecting = false;

            double denominator = DVector2.Dot(planeNormal * -1f, rayDir);

            if (denominator > epsilon)
            {
                DVector2 vecBetween = planePos - rayStart;

                double t = DVector2.Dot(vecBetween, planeNormal * -1f) / denominator;

                if (t >= 0f)
                {
                    areIntersecting = true;
                }
            }

            return areIntersecting;
        }
        public static DVector2 GetRayPlaneIntersectionPoint(DVector2 planePos, DVector2 planeNormal, DVector2 rayStart, DVector2 rayDir)
        {
            DVector2 intersectionPoint = GetIntersectionCoordinate(planePos, planeNormal, rayStart, rayDir);

            return intersectionPoint;
        }
        private static DVector2 GetIntersectionCoordinate(DVector2 planePos, DVector2 planeNormal, DVector2 rayStart, DVector2 rayDir)
        {
            double denominator = DVector2.Dot(-planeNormal, rayDir);

            DVector2 vecBetween = planePos - rayStart;

            double t = DVector2.Dot(vecBetween, -planeNormal) / denominator;

            DVector2 intersectionPoint = rayStart + rayDir * t;

            return intersectionPoint;
        }
        public static bool LinePlane(DVector2 planePos, DVector2 planeNormal, DVector2 line_p1, DVector2 line_p2)
        {
            double epsilon = GISTerrainLoaderMathUtility.EPSILON;

            bool areIntersecting = false;

            DVector2 lineDir = DVector2.Normalize(line_p1 - line_p2);

            double denominator = DVector2.Dot(-planeNormal, lineDir);

            if (denominator > epsilon || denominator < -epsilon)
            {
                DVector2 vecBetween = planePos - line_p1;

                double t = DVector2.Dot(vecBetween, -planeNormal) / denominator;

                DVector2 intersectionPoint = line_p1 + lineDir * t;
                if (GISTerrainLoaderGeometry.IsPointBetweenPoints(line_p1, line_p2, intersectionPoint))
                {
                    areIntersecting = true;
                }
            }

            return areIntersecting;
        }
        public static DVector2 GetLinePlaneIntersectionPoint(DVector2 planePos, DVector2 planeNormal, DVector2 line_p1, DVector2 line_p2)
        {
            DVector2 lineDir = DVector2.Normalize(line_p1 - line_p2);

            DVector2 intersectionPoint = GetIntersectionCoordinate(planePos, planeNormal, line_p1, lineDir);

            return intersectionPoint;
        }
        public static bool PlanePlane(DVector2 planePos_1, DVector2 planeNormal_1, DVector2 planePos_2, DVector2 planeNormal_2)
        {
            bool areIntersecting = false;

            double dot = DVector2.Dot(planeNormal_1, planeNormal_2);
            float one = 1f - GISTerrainLoaderMathUtility.EPSILON;

            if (dot < one && dot > -one)
            {
                areIntersecting = true;
            }

            return areIntersecting;
        }

        public static DVector2 GetPlanePlaneIntersectionPoint(DVector2 planePos_1, DVector2 planeNormal_1, DVector2 planePos_2, DVector2 planeNormal_2)
        {
            DVector2 lineDir = DVector2.Normalize(new DVector2(planeNormal_2.y, -planeNormal_2.x));

            DVector2 intersectionPoint = GetIntersectionCoordinate(planePos_1, planeNormal_1, planePos_2, lineDir);

            return intersectionPoint;
        }
        public static bool PointTriangle(GISTerrainLoaderTriangle2 t, DVector2 p, bool includeBorder)
        {
            float epsilon = GISTerrainLoaderMathUtility.EPSILON;

            float zero = 0f - epsilon;
            float one = 1f + epsilon;

            double denominator = ((t.p2.y - t.p3.y) * (t.p1.x - t.p3.x) + (t.p3.x - t.p2.x) * (t.p1.y - t.p3.y));

            double a = ((t.p2.y - t.p3.y) * (p.x - t.p3.x) + (t.p3.x - t.p2.x) * (p.y - t.p3.y)) / denominator;
            double b = ((t.p3.y - t.p1.y) * (p.x - t.p3.x) + (t.p1.x - t.p3.x) * (p.y - t.p3.y)) / denominator;
            double c = 1 - a - b;

            bool isWithinTriangle = false;

            if (includeBorder)
            {
                if (a >= zero && a <= one && b >= zero && b <= one && c >= zero && c <= one)
                {
                    isWithinTriangle = true;
                }
            }
            else
            {
                if (a > zero && a < one && b > zero && b < one && c > zero && c < one)
                {
                    isWithinTriangle = true;
                }
            }

            return isWithinTriangle;
        }
        public static bool IsTriangleInsideTriangle(GISTerrainLoaderTriangle2 t1, GISTerrainLoaderTriangle2 t2)
        {
            bool isWithin = false;

            if (
                PointTriangle(t2, t1.p1, false) &&
                PointTriangle(t2, t1.p2, false) &&
                PointTriangle(t2, t1.p3, false))
            {
                isWithin = true;
            }

            return isWithin;
        }
        public static bool AABB_AABB_2D(GISTerrainLoaderAABB2 r1, GISTerrainLoaderAABB2 r2)
        {
            bool isIntersecting = true;

            if (r1.minX > r2.maxX)
            {
                isIntersecting = false;
            }
            else if (r2.minX > r1.maxX)
            {
                isIntersecting = false;
            }
            else if (r1.minY > r2.maxY)
            {
                isIntersecting = false;
            }
            else if (r2.minY > r1.maxY)
            {
                isIntersecting = false;
            }

            return isIntersecting;
        }
        public static IntersectionCases PointCircle(DVector2 a, DVector2 b, DVector2 c, DVector2 testPoint)
        {
            DVector2 circleCenter = GISTerrainLoaderGeometry.CalculateCircleCenter(a, b, c);

            double radiusSqr = DVector2.SqrDistance(a, circleCenter);

            double distPointCenterSqr = DVector2.SqrDistance(testPoint, circleCenter);

            if (distPointCenterSqr < radiusSqr - GISTerrainLoaderMathUtility.EPSILON * 2f)
            {
                return IntersectionCases.IsInside;
            }
            else if (distPointCenterSqr > radiusSqr + GISTerrainLoaderMathUtility.EPSILON * 2f)
            {
                return IntersectionCases.NoIntersection;
            }
            else
            {
                return IntersectionCases.IsOnEdge;
            }
        }
        public static bool PointPolygon(List<DVector2> polygonPoints, DVector2 point)
        {
            bool isInside = true;

            if (polygonPoints.Count > 0)
            {
                DVector2 maxXPosVertex = polygonPoints[0];

                for (int i = 1; i < polygonPoints.Count; i++)
                {
                    if (polygonPoints[i].x > maxXPosVertex.x)
                    {
                        maxXPosVertex = polygonPoints[i];
                    }
                }

                DVector2 pointOutside = maxXPosVertex + new DVector2(1f, 0.01f);

                DVector2 l1_p1 = point;
                DVector2 l1_p2 = pointOutside;

                int numberOfIntersections = 0;

                for (int i = 0; i < polygonPoints.Count; i++)
                {
                    DVector2 l2_p1 = polygonPoints[i];

                    int iPlusOne = GISTerrainLoaderMathUtility.ClampListIndex(i + 1, polygonPoints.Count);

                    DVector2 l2_p2 = polygonPoints[iPlusOne];

                    if (GISTerrainLoaderMathIntersections.LineLine(l1_p1, l1_p2, l2_p1, l2_p2, shouldIncludeEndPoints: true))
                    {
                        numberOfIntersections += 1;
                    }
                }

                if (numberOfIntersections == 0 || numberOfIntersections % 2 == 0)
                {
                    isInside = false;
                }

              
            }

            return isInside;
        }
        public static bool TriangleTriangle2D(GISTerrainLoaderTriangle2 t1, GISTerrainLoaderTriangle2 t2, bool do_AABB_test)
        {
            bool isIntersecting = false;

            if (do_AABB_test)
            {
                GISTerrainLoaderAABB2 r1 = new GISTerrainLoaderAABB2(t1.MinX(), t1.MaxX(), t1.MinY(), t1.MaxY());

                GISTerrainLoaderAABB2 r2 = new GISTerrainLoaderAABB2(t2.MinX(), t2.MaxX(), t2.MinY(), t2.MaxY());

                if (!AABB_AABB_2D(r1, r2))
                {
                    return false;
                }
            }
            if (
                LineLine(t1.p1, t1.p2, t2.p1, t2.p2, true) ||
                LineLine(t1.p1, t1.p2, t2.p2, t2.p3, true) ||
                LineLine(t1.p1, t1.p2, t2.p3, t2.p1, true)
            )
            {
                isIntersecting = true;
            }
            else if (
                LineLine(t1.p2, t1.p3, t2.p1, t2.p2, true) ||
                LineLine(t1.p2, t1.p3, t2.p2, t2.p3, true) ||
                LineLine(t1.p2, t1.p3, t2.p3, t2.p1, true)
            )
            {
                isIntersecting = true;
            }
            else if (
                LineLine(t1.p3, t1.p1, t2.p1, t2.p2, true) ||
                LineLine(t1.p3, t1.p1, t2.p2, t2.p3, true) ||
                LineLine(t1.p3, t1.p1, t2.p3, t2.p1, true)
            )
            {
                isIntersecting = true;
            }

            if (isIntersecting)
            {
                return isIntersecting;
            }
            if (PointTriangle(t2, t1.p1, true) || PointTriangle(t1, t2.p1, true))
            {
                isIntersecting = true;
            }


            return isIntersecting;
        }
    }
}
