/*     Unity GIS Tech 2020-2023      */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderPolygonMeshGenerator
    {
        private List<List<Vector3>> points;
        private GISTerrainLoaderMeshData md;

        private Vector3 _v1, _v2;
 
        private Vector3 _vert;

        public Rect TextureRect;
        private Quaternion _textureDirection;
        private Vector2[] _textureUvCoordinates;
        private Vector3 _vertexRelativePos;
        private Vector3 _firstVert;

        private float minx;
        private float miny;
        private float maxx;
        private float maxy;

        private static Vector3 Vector3Right = new Vector3(1, 0, 0);
        private float Customheight = 0;

        public GISTerrainLoaderPolygonMeshGenerator(List<List<Vector3>> m_SpacePoints, GISTerrainLoaderMeshData m_md, Rect m_TextureRect, float m_Customheight = 0)
        {
            TextureRect = m_TextureRect;
            points = m_SpacePoints;
            md = m_md;
            Customheight = m_Customheight;
        }
        public GISTerrainLoaderPolygonMeshGenerator(List<List<Vector3>> m_SpacePoints, GISTerrainLoaderMeshData m_md, float m_Customheight = 0)
        {
            points = m_SpacePoints;
            md = m_md;
            Customheight = m_Customheight;
        }
        public void Run()
        {
            var _counter = points.Count;
            var subset = new List<List<Vector3>>(_counter);
            GISTerrainLoaderMeshDataHolder flatData = null;
            List<int> result = null;
            var currentIndex = 0;
            int vertCount = 0, polygonVertexCount = 0;
            List<int> triList = null;
            List<Vector3> sub = null;
 
            for (int i = 0; i < _counter; i++)
            {
                sub = points[i];

                vertCount = md.Vertices.Count;

                if (IsClockwise(sub) && vertCount > 0)
                {
                    flatData = GISTerrainLoaderGeometryLib.Flatten(subset);
                    result = GISTerrainLoaderGeometryLib.Triangulate(flatData.Vertices, flatData.Holes, flatData.Dim);
                    polygonVertexCount = result.Count;
                    if (triList == null)
                    {
                        triList = new List<int>(polygonVertexCount);
                    }
                    else
                    {
                        triList.Capacity = triList.Count + polygonVertexCount;
                    }

                    for (int j = 0; j < polygonVertexCount; j++)
                    {
                        triList.Add(result[j] + currentIndex);
                    }

                    currentIndex = vertCount;

                    subset.Clear();
                }


                subset.Add(sub);

                polygonVertexCount = sub.Count;

                md.Vertices.Capacity = md.Vertices.Count + polygonVertexCount;
                md.Normals.Capacity = md.Normals.Count + polygonVertexCount;
                md.Edges.Capacity = md.Edges.Count + polygonVertexCount * 2;

                for (int j = 0; j < polygonVertexCount; j++)
                {
                    md.Edges.Add(vertCount + ((j + 1) % polygonVertexCount));
                    md.Edges.Add(vertCount + j);

                    var vertice = sub[j];

                    if (Customheight!=0)
                        vertice = new Vector3(sub[j].x, sub[j].y - Customheight, sub[j].z);
                    else
                    {
                        vertice = new Vector3(sub[j].x, sub[j].y, sub[j].z);
                        
                    }
                    md.Vertices.Add(vertice);
                    md.Tangents.Add(GISTerrainLoaderConstants.Vector3Forward);
                    md.Normals.Add(GISTerrainLoaderConstants.Vector3Up);

                }
            }

            flatData = GISTerrainLoaderGeometryLib.Flatten(subset);

            result = GISTerrainLoaderGeometryLib.Triangulate(flatData.Vertices, flatData.Holes, flatData.Dim);
            polygonVertexCount = result.Count;

            if (md.Vertices.Count < 2)
                return;

            minx = float.MaxValue;
            miny = float.MaxValue;
            maxx = float.MinValue;
            maxy = float.MinValue;

            _textureUvCoordinates = new Vector2[md.Vertices.Count];
            _textureDirection = Quaternion.FromToRotation((md.Vertices[0] - md.Vertices[1]), Vector3Right);
            _textureUvCoordinates[0] = new Vector2(0, 0);
            _firstVert = md.Vertices[0];

            for (int i = 1; i < md.Vertices.Count; i++)
            {
                _vert = md.Vertices[i];
                _vertexRelativePos = _vert - _firstVert;
                _vertexRelativePos = _textureDirection * _vertexRelativePos;
                _textureUvCoordinates[i] = new Vector2(_vertexRelativePos.x, _vertexRelativePos.z);
                if (_vertexRelativePos.x < minx)
                    minx = _vertexRelativePos.x;
                if (_vertexRelativePos.x > maxx)
                    maxx = _vertexRelativePos.x;
                if (_vertexRelativePos.z < miny)
                    miny = _vertexRelativePos.z;
                if (_vertexRelativePos.z > maxy)
                    maxy = _vertexRelativePos.z;
            }

            var width = maxx - minx;
            var height = maxy - miny;

            for (int i = 0; i < md.Vertices.Count; i++)
            {
                if (TextureRect != null)
                {
                    var pos = new Vector2(
                        (((_textureUvCoordinates[i].x - minx) / width) * TextureRect.width) + TextureRect.x,
                        (((_textureUvCoordinates[i].y - miny) / height) * TextureRect.height) + TextureRect.y);

                    md.UV[0].Add(pos);
                }
            }

            if (triList == null)
            {
                triList = new List<int>(polygonVertexCount);
            }
            else
            {
                triList.Capacity = triList.Count + polygonVertexCount;
            }

            for (int i = 0; i < polygonVertexCount; i++)
            {
                triList.Add(result[i] + currentIndex);
            }

            md.Triangles.Add(triList);


        }

        private bool IsClockwise(IList<Vector3> vertices)
        {
            double sum = 0.0;
            var _counter = vertices.Count;
            for (int i = 0; i < _counter; i++)
            {
                _v1 = vertices[i];
                _v2 = vertices[(i + 1) % _counter];
                sum += (_v2.x - _v1.x) * (_v2.z + _v1.z);
            }

            return sum > 0.0;
        }
    }

}