/*     Unity GIS Tech 2020-2023      */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public class GISTerrainLoaderMeshData
    {
        public Vector3 CentrePosition;
        public List<int> Edges;
        public Vector2 MercatorCenter;
        public List<Vector3> Vertices;
        public List<Vector3> Normals;
        public List<Vector4> Tangents;
        public List<List<int>> Triangles;
        public List<List<Vector2>> UV;

        private OptionEnabDisab  AdapteElevation;
 
        public GISTerrainLoaderMeshData(OptionEnabDisab m_AdapteElevation = OptionEnabDisab.Disable)
        {
            Edges = new List<int>();
            Vertices = new List<Vector3>();
            Normals = new List<Vector3>();
            Tangents = new List<Vector4>();
            Triangles = new List<List<int>>();
            UV = new List<List<Vector2>>();
            UV.Add(new List<Vector2>());
            AdapteElevation = m_AdapteElevation;
        }
        public void GetCentrePoint(List<List<Vector3>> SpacePoints, Vector2 MinMaxHeight = new Vector2())
        {
            int _counter = SpacePoints.Count;

            int _SubCounter = 0;

            Vector3 _CentrePoint = Vector3.zero;

            int vertexIndex = 1;

            _CentrePoint = SpacePoints[0][0];

            for (int i = 0; i < _counter; i++)
            {
                _SubCounter = SpacePoints[i].Count;

                for (int j = 0; j < _SubCounter; j++)
                {
                    var vertice = new Vector3();

                    if (AdapteElevation == OptionEnabDisab.Disable)
                        vertice = new Vector3(SpacePoints[i][j].x, SpacePoints[i][j].y, SpacePoints[i][j].z);
                    else
                        vertice = new Vector3(SpacePoints[i][j].x, 0, SpacePoints[i][j].z);

                    _CentrePoint += vertice;

                    vertexIndex++;
                }

            }
            _CentrePoint /= vertexIndex;


            for (int i = 0; i < _counter; i++)
            {
                _SubCounter = SpacePoints[i].Count;

                for (int j = 0; j < _SubCounter; j++)
                {
                    var vertice = new Vector3();

                    if (AdapteElevation == OptionEnabDisab.Disable)
                        vertice = new Vector3(SpacePoints[i][j].x - _CentrePoint.x, SpacePoints[i][j].y, SpacePoints[i][j].z - _CentrePoint.z);
                    else
                        vertice = new Vector3(SpacePoints[i][j].x - _CentrePoint.x, MinMaxHeight.y - _CentrePoint.y, SpacePoints[i][j].z - _CentrePoint.z);
 
                    SpacePoints[i][j] = vertice;
 
                }

            }

            if (AdapteElevation == OptionEnabDisab.Disable)
                _CentrePoint.y = 0;


         CentrePosition = _CentrePoint;
        
        }

        internal void Clear()
        {
            Edges.Clear();
            Vertices.Clear();
            Normals.Clear();
            Tangents.Clear();

            foreach (var item in Triangles)
            {
                item.Clear();
            }
            foreach (var item in UV)
            {
                item.Clear();
            }
        }
    }
}

