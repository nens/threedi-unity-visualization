using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public enum BooleanOperation { Intersection, Difference, ExclusiveOr, Union }

    public static class GISTerrainLoaderPolygonMathUtility
    {
        public static List<List<DVector2>> ClipPolygons(List<DVector2> polyVector2, List<DVector2> clipPolyVector2, BooleanOperation booleanOperation)
        {
            List<List<DVector2>> finalPoly = new List<List<DVector2>>();

            List<ClipVertex> poly = InitDataStructure(polyVector2);

            List<ClipVertex> clipPoly = InitDataStructure(clipPolyVector2);

            bool hasFoundIntersection = false;

            for (int i = 0; i < poly.Count; i++)
            {
                ClipVertex currentVertex = poly[i];

                int iPlusOne = GISTerrainLoaderMathUtility.ClampListIndex(i + 1, poly.Count);

                DVector2 a = poly[i].coordinate;

                DVector2 b = poly[iPlusOne].coordinate;

                for (int j = 0; j < clipPoly.Count; j++)
                {
                    int jPlusOne = GISTerrainLoaderMathUtility.ClampListIndex(j + 1, clipPoly.Count);

                    DVector2 c = clipPoly[j].coordinate;

                    DVector2 d = clipPoly[jPlusOne].coordinate;

                    if (GISTerrainLoaderMathIntersections.LineLine(a, b, c, d, true))
                    {
                        hasFoundIntersection = true;

                        DVector2 intersectionPoint2D = GISTerrainLoaderMathIntersections.GetLineLineIntersectionPoint(a, b, c, d);

                        ClipVertex vertexOnPolygon = InsertIntersectionVertex(a, b, intersectionPoint2D, currentVertex);

                        ClipVertex vertexOnClipPolygon = InsertIntersectionVertex(c, d, intersectionPoint2D, clipPoly[j]);

                        vertexOnPolygon.neighbor = vertexOnClipPolygon;

                        vertexOnClipPolygon.neighbor = vertexOnPolygon;
                    }
                }
            }

            if (hasFoundIntersection)
            {
                MarkEntryExit(poly, clipPolyVector2);

                MarkEntryExit(clipPoly, polyVector2);

                DebugEntryExit(poly);

                if (booleanOperation == BooleanOperation.Intersection)
                {
                    List<ClipVertex> intersectionVertices = GetClippedPolygon(poly, true);

                    AddPolygonToList(intersectionVertices, finalPoly, false);

                }
                else if (booleanOperation == BooleanOperation.Difference)
                {
                    List<ClipVertex> outsidePolyVertices = GetClippedPolygon(poly, false);

                    AddPolygonToList(outsidePolyVertices, finalPoly, true);
                }
                else if (booleanOperation == BooleanOperation.ExclusiveOr)
                {
                    List<ClipVertex> outsidePolyVertices = GetClippedPolygon(poly, false);

                    AddPolygonToList(outsidePolyVertices, finalPoly, true);

                    List<ClipVertex> outsideClipPolyVertices = GetClippedPolygon(clipPoly, false);

                    AddPolygonToList(outsideClipPolyVertices, finalPoly, true);
                }
                else if (booleanOperation == BooleanOperation.Union)
                {
                    List<ClipVertex> intersectionVertices = GetClippedPolygon(poly, true);

                    AddPolygonToList(intersectionVertices, finalPoly, false);

                    List<ClipVertex> outsidePolyVertices = GetClippedPolygon(poly, false);

                    AddPolygonToList(outsidePolyVertices, finalPoly, true);

                    List<ClipVertex> outsideClipPolyVertices = GetClippedPolygon(clipPoly, false);

                    AddPolygonToList(outsideClipPolyVertices, finalPoly, true);
                }
            }
            else
            {
                if (IsPolygonInsidePolygon(polyVector2, clipPolyVector2))
                {
                    List<List<DVector2>> polya = new List<List<DVector2>>();
                    polya.Add(polyVector2);
                    return polya;
                }
                else if (IsPolygonInsidePolygon(clipPolyVector2, polyVector2))
                {
                }
                else
                {
                }
            }

            return finalPoly;
        }

        private static void AddPolygonToList(List<ClipVertex> verticesToAdd, List<List<DVector2>> finalPoly, bool shouldReverse)
        {
            List<DVector2> thisPolyList = new List<DVector2>();

            finalPoly.Add(thisPolyList);

            for (int i = 0; i < verticesToAdd.Count; i++)
            {
                ClipVertex v = verticesToAdd[i];

                thisPolyList.Add(v.coordinate);

                if (v.nextPoly != null)
                {
                    if (shouldReverse)
                    {
                        thisPolyList.Reverse();
                    }

                    thisPolyList = new List<DVector2>();

                    finalPoly.Add(thisPolyList);
                }
            }
            if (shouldReverse)
            {
                finalPoly[finalPoly.Count - 1].Reverse();
            }
        }
        private static List<ClipVertex> GetClippedPolygon(List<ClipVertex> poly, bool getIntersectionPolygon)
        {
            List<ClipVertex> finalPolygon = new List<ClipVertex>();

            ResetVertices(poly);

            ClipVertex thisVertex = FindFirstEntryVertex(poly);

            ClipVertex firstVertex = thisVertex;

            finalPolygon.Add(thisVertex);

            thisVertex.isTakenByFinalPolygon = true;
            thisVertex.neighbor.isTakenByFinalPolygon = true;

            bool isMovingForward = getIntersectionPolygon ? true : false;

            thisVertex = getIntersectionPolygon ? thisVertex.next : thisVertex.prev;

            int safety = 0;

            while (true)
            {
                if (thisVertex.Equals(firstVertex) || (thisVertex.neighbor != null && thisVertex.neighbor.Equals(firstVertex)))
                {

                    ClipVertex nextVertex = FindFirstEntryVertex(poly);

                    if (nextVertex == null)
                    {
                        break;
                    }
                    else
                    {

                        finalPolygon[finalPolygon.Count - 1].nextPoly = nextVertex;

                        thisVertex = nextVertex;

                        firstVertex = nextVertex;

                        finalPolygon.Add(thisVertex);

                        thisVertex.isTakenByFinalPolygon = true;
                        thisVertex.neighbor.isTakenByFinalPolygon = true;

                        isMovingForward = getIntersectionPolygon ? true : false;

                        thisVertex = getIntersectionPolygon ? thisVertex.next : thisVertex.prev;
                    }
                }
                if (!thisVertex.isIntersection)
                {
                    finalPolygon.Add(thisVertex);

                    thisVertex = isMovingForward ? thisVertex.next : thisVertex.prev;
                }
                else
                {
                    thisVertex.isTakenByFinalPolygon = true;
                    thisVertex.neighbor.isTakenByFinalPolygon = true;

                    thisVertex = thisVertex.neighbor;

                    finalPolygon.Add(thisVertex);
                    if (getIntersectionPolygon)
                    {
                        isMovingForward = thisVertex.isEntry ? true : false;

                        thisVertex = thisVertex.isEntry ? thisVertex.next : thisVertex.prev;
                    }
                    else
                    {
                        isMovingForward = !isMovingForward;

                        thisVertex = isMovingForward ? thisVertex.next : thisVertex.prev;
                    }
                }

                safety += 1;

                if (safety > 100000)
                {
                    break;
                }
            }
            return finalPolygon;
        }

        private static void ResetVertices(List<ClipVertex> poly)
        {
            ClipVertex resetVertex = poly[0];

            int safety = 0;

            while (true)
            {
                resetVertex.isTakenByFinalPolygon = false;
                resetVertex.nextPoly = null;

                if (resetVertex.isIntersection)
                {
                    resetVertex.neighbor.isTakenByFinalPolygon = false;
                }

                resetVertex = resetVertex.next;

                if (resetVertex.Equals(poly[0]))
                {
                    break;
                }

                safety += 1;

                if (safety > 100000)
                {
                    break;
                }
            }
        }
        public static bool IsPolygonInsidePolygon(List<DVector2> polyOne, List<DVector2> polyTwo)
        {
            bool isInside = false;

            for (int i = 0; i < polyOne.Count; i++)
            {
                if (GISTerrainLoaderMathIntersections.PointPolygon(polyTwo, polyOne[i]))
                {
                    isInside = true;

                    break;
                }
            }

            return isInside;
        }

        private static ClipVertex FindFirstEntryVertex(List<ClipVertex> poly)
        {
            ClipVertex thisVertex = poly[0];

            ClipVertex firstVertex = thisVertex;

            int safety = 0;

            while (true)
            {
                if (thisVertex.isIntersection && thisVertex.isEntry && !thisVertex.isTakenByFinalPolygon)
                {
                    break;
                }

                thisVertex = thisVertex.next;

                if (thisVertex.Equals(firstVertex))
                {
                    thisVertex = null;

                    break;
                }

                safety += 1;

                if (safety > 100000)
                {
                    Debug.Log("Endless loop in find first entry vertex");

                    break;
                }
            }

            return thisVertex;
        }
        private static List<ClipVertex> InitDataStructure(List<DVector2> polyVector)
        {
            List<ClipVertex> poly = new List<ClipVertex>();

            for (int i = 0; i < polyVector.Count; i++)
            {
                poly.Add(new ClipVertex(polyVector[i]));
            }

            for (int i = 0; i < poly.Count; i++)
            {
                int iPlusOne = GISTerrainLoaderMathUtility.ClampListIndex(i + 1, poly.Count);
                int iMinusOne = GISTerrainLoaderMathUtility.ClampListIndex(i - 1, poly.Count);

                poly[i].next = poly[iPlusOne];
                poly[i].prev = poly[iMinusOne];
            }

            return poly;
        }
        private static ClipVertex InsertIntersectionVertex(DVector2 a, DVector2 b, DVector2 intersectionPoint, ClipVertex currentVertex)
        {
            double alpha = DVector2.SqrMagnitude(a - intersectionPoint) / DVector2.SqrMagnitude(a - b);

            ClipVertex intersectionVertex = new ClipVertex(intersectionPoint);

            intersectionVertex.isIntersection = true;
            intersectionVertex.alpha = (float)alpha;

            ClipVertex insertAfterThisVertex = currentVertex;

            int safety = 0;

            while (true)
            {
                if (insertAfterThisVertex.next.alpha > alpha || !insertAfterThisVertex.next.isIntersection)
                {
                    break;
                }

                insertAfterThisVertex = insertAfterThisVertex.next;

                safety += 1;

                if (safety > 100000)
                {
                    Debug.Log("Stuck in loop in insert intersection vertices");

                    break;
                }
            }

            intersectionVertex.next = insertAfterThisVertex.next;

            intersectionVertex.prev = insertAfterThisVertex;

            insertAfterThisVertex.next.prev = intersectionVertex;

            insertAfterThisVertex.next = intersectionVertex;

            return intersectionVertex;
        }

        private static void MarkEntryExit(List<ClipVertex> poly, List<DVector2> clipPolyVector)
        {
            bool isInside = GISTerrainLoaderMathIntersections.PointPolygon(clipPolyVector, poly[0].coordinate);

            ClipVertex currentVertex = poly[0];

            ClipVertex firstVertex = currentVertex;

            int safety = 0;

            while (true)
            {
                if (currentVertex.isIntersection)
                {
                    currentVertex.isEntry = isInside ? false : true;

                    isInside = !isInside;
                }

                currentVertex = currentVertex.next;

                if (currentVertex.Equals(firstVertex))
                {
                    break;
                }

                safety += 1;

                if (safety > 100000)
                {

                    break;
                }
            }
        }



        private static void DebugEntryExit(List<ClipVertex> polyList)
        {
            ClipVertex thisVertex = polyList[0];

            Gizmos.color = Color.green;

            float size = 0.02f;

            int safety = 0;

            while (true)
            {
                if (thisVertex.isIntersection)
                {
                    Gizmos.color = Color.yellow;

                    if (thisVertex.isEntry)
                    {
                        Gizmos.color = Color.red;
                    }
                    size += 0.005f;
                }

                thisVertex = thisVertex.next;

                if (thisVertex.Equals(polyList[0]))
                {
                    break;
                }


                safety += 1;

                if (safety > 100000)
                {
                    Debug.Log("Endless loop in debug entry exit");

                    break;
                }
            }
        }



        private static void InWhichOrderAreVerticesAdded(List<ClipVertex> polyList)
        {
            ClipVertex thisVertex = polyList[0];

            float size = 0.01f;

            int safety = 0;

            while (true)
            {
                Gizmos.color = Color.red;

                size += 0.01f;

                thisVertex = thisVertex.next;

                if (thisVertex.Equals(polyList[0]))
                {
                    break;
                }

                safety += 1;

                if (safety > 100000)
                {
                    Debug.Log("Endless loop in debug in which orders are vertices added");

                    break;
                }
            }
        }
    }

    public class ClipVertex
    {
        public DVector2 coordinate;

        public ClipVertex next;
        public ClipVertex prev;

        public ClipVertex nextPoly;

        public bool isIntersection = false;

        public bool isEntry;

        public ClipVertex neighbor;

        public float alpha = 0f;

        public bool isTakenByFinalPolygon;

        public ClipVertex(DVector2 coordinate)
        {
            this.coordinate = coordinate;
        }
    }
}