/*     Unity GIS Tech 2020-2024      */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderPointsFromMesh  
    {
        static MeshFilter filter;
        public static List<List<Vector3>> GetPointsFromMesh(MeshFilter m_filter)
        {
            List<List<Vector3>> points = new List<List<Vector3>>(); 

            filter = m_filter;
            var edges = BuildEdgesFromMesh(filter);
            var m_points = BuildColliderPaths(filter, edges);
 
            if (m_points.Count > 0)
            {
                for(int i = 0; i < m_points.Count; i++)
                {
                    if(m_points[i].Count > 4)
                    {
                        m_points[i].Add(m_points[i][0]);
                        points.Add(m_points[i]);
                    }
 
                }
            }
            return points;

        }
        static  Dictionary<GISTerrainLoaderEdge3D, int> BuildEdgesFromMesh(MeshFilter filter)
        {
            var mesh = filter.sharedMesh;

            if (mesh == null)
                return null;

            var verts = mesh.vertices;
            var tris = mesh.triangles;
            var edges = new Dictionary<GISTerrainLoaderEdge3D, int>();

            for (int i = 0; i < tris.Length - 2; i += 3)
            {

                var faceVert1 = verts[tris[i]];
                var faceVert2 = verts[tris[i + 1]];
                var faceVert3 = verts[tris[i + 2]];

                GISTerrainLoaderEdge3D[] faceEdges;
                faceEdges = new GISTerrainLoaderEdge3D[] {
                new GISTerrainLoaderEdge3D{ a = faceVert1, b = faceVert2 },
                new GISTerrainLoaderEdge3D{ a = faceVert2, b = faceVert3 },
                new GISTerrainLoaderEdge3D{ a = faceVert3, b = faceVert1 },
            };

                foreach (var edge in faceEdges)
                {
                    if (edges.ContainsKey(edge))
                        edges[edge]++;
                    else
                        edges[edge] = 1;
                }
            }

            return edges;
        }
        static List<List<Vector3>> BuildColliderPaths(MeshFilter filter, Dictionary<GISTerrainLoaderEdge3D, int> allEdges)
        {
            if (allEdges == null)
                return null;

            var outerEdges = GetOuterEdges(allEdges);

            var paths = new List<List<GISTerrainLoaderEdge3D>>();
            List<GISTerrainLoaderEdge3D> path = null;

            while (outerEdges.Count > 0)
            {

                if (path == null)
                {
                    path = new List<GISTerrainLoaderEdge3D>();
                    path.Add(outerEdges[0]);
                    paths.Add(path);

                    outerEdges.RemoveAt(0);
                }

                bool foundAtLeastOneEdge = false;

                int i = 0;
                while (i < outerEdges.Count)
                {
                    var edge = outerEdges[i];
                    bool removeEdgeFromOuter = false;

                    if (edge.b == path[0].a)
                    {
                        path.Insert(0, edge);
                        removeEdgeFromOuter = true;
                    }
                    else if (edge.a == path[path.Count - 1].b)
                    {
                        path.Add(edge);
                        removeEdgeFromOuter = true;
                    }

                    if (removeEdgeFromOuter)
                    {
                        foundAtLeastOneEdge = true;
                        outerEdges.RemoveAt(i);
                    }
                    else
                        i++;
                }

                //If we didn't find at least one edge, then the remaining outer edges must belong to a different path
                if (!foundAtLeastOneEdge)
                    path = null;

            }

            var cleanedPaths = new List<List<Vector3>>();

            foreach (var builtPath in paths)
            {
                var coords = new List<Vector3>();

                foreach (var edge in builtPath)
                {
                    coords.Add(edge.a);
                }
                cleanedPaths.Add(CoordinatesCleaned(coords));
            }


            return cleanedPaths;
        }
        static List<GISTerrainLoaderEdge3D> GetOuterEdges(Dictionary<GISTerrainLoaderEdge3D, int> allEdges)
        {
            var outerEdges = new List<GISTerrainLoaderEdge3D>();

            foreach (var edge in allEdges.Keys)
            {
                var numSharedFaces = allEdges[edge];
                if (numSharedFaces == 1)
                    outerEdges.Add(edge);
            }

            return outerEdges;
        }
        static bool CoordinatesFormLine(Vector3 a, Vector3 b, Vector3 c)
        {
            //If the area of a triangle created from three points is zero, they must be in a line.
            float area = a.x * (b.z - c.z) +
                b.x * (c.z - a.z) +
                    c.x * (a.z - b.z);

            return Mathf.Approximately(area, 0f);

        }
        static List<Vector3> CoordinatesCleaned(List<Vector3> coordinates)
        {
            List<Vector3> coordinatesCleaned = new List<Vector3>();
            coordinatesCleaned.Add(coordinates[0]);

            var lastAddedIndex = 0;

            for (int i = 1; i < coordinates.Count; i++)
            {

                var coordinate = coordinates[i];

                Vector2 lastAddedCoordinate = coordinates[lastAddedIndex];
                Vector2 nextCoordinate = (i + 1 >= coordinates.Count) ? coordinates[0] : coordinates[i + 1];

                if (!CoordinatesFormLine(lastAddedCoordinate, coordinate, nextCoordinate))
                {

                    coordinatesCleaned.Add(coordinate);
                    lastAddedIndex = i;

                }

            }

            return coordinatesCleaned;

        }
    }
}