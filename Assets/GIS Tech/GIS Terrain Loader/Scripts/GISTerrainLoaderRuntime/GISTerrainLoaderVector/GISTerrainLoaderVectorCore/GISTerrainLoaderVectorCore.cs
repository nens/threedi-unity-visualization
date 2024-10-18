/*     Unity GIS Tech 2020-2023     */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GISTech.GISTerrainLoader
{
    public abstract class GISTerrainLoaderGeoDataHolder
    {
        /// <summary>
        /// Get All Geo Filtred Data, Set a Container and Enable  LoadDataOutOfContainerBounds Form GISTerrainLoaderVectorParameters_SO
        /// To Load only data inside the container bounds
        /// </summary>
        /// <param name="name">FileName Or Any Reference name</param>
        /// <param name="container"></param>
        /// <returns></returns>
        public abstract GISTerrainLoaderGeoVectorData GetGeoFiltredData(string name="",GISTerrainContainer container=null);
    }

    public class GISTerrainLoaderGeoVectorData
    {
        public string Name;
        public List<GISTerrainLoaderPointGeoData> GeoPoints;
        public List<GISTerrainLoaderLineGeoData> GeoLines;
        public List<GISTerrainLoaderPolygonGeoData> GeoPolygons;
        public int EPSG;
 
        public GISTerrainLoaderGeoVectorData()
        {
            GeoPoints = new List<GISTerrainLoaderPointGeoData>();
            GeoPolygons = new List<GISTerrainLoaderPolygonGeoData>();
            GeoLines = new List<GISTerrainLoaderLineGeoData>();
        }
        public GISTerrainLoaderGeoVectorData(string name)
        {
            Name = name;
            GeoPoints = new List<GISTerrainLoaderPointGeoData>();
            GeoLines = new List<GISTerrainLoaderLineGeoData>();
            GeoPolygons = new List<GISTerrainLoaderPolygonGeoData>();
        }
        public static GISTerrainLoaderGeoVectorData GetGeoDataFromScene(GISTerrainContainer Container,CoordinatesSource coordinatesSource = CoordinatesSource.FromTerrain,bool storeZValue = false, RealWorldElevation ElevationMode = RealWorldElevation.Elevation)
        {
 
            GISTerrainLoaderGeoVectorData SceneGeoData = new GISTerrainLoaderGeoVectorData();

            GISTerrainLoaderVectorPoint[] GeoPoints = UnityEngine.Object.FindObjectsOfType<GISTerrainLoaderVectorPoint>();
            GISTerrainLoaderVectorLine[] GeoLines = UnityEngine.Object.FindObjectsOfType<GISTerrainLoaderVectorLine>();
            GISTerrainLoaderVectorPolygon[] GeoPolygones = UnityEngine.Object.FindObjectsOfType<GISTerrainLoaderVectorPolygon>();

            foreach (var GeoPoint in GeoPoints)
            {
                GeoPoint.UpdateGeoDataFrom(Container,coordinatesSource, storeZValue, ElevationMode);
                SceneGeoData.GeoPoints.Add(GeoPoint.PointGeoData);
            }

            foreach (var GeoLine in GeoLines)
            {
                GeoLine.UpdateGeoDataFrom(Container, coordinatesSource, storeZValue, ElevationMode);
                SceneGeoData.GeoLines.Add(GeoLine.LineGeoData);

            }

            foreach (var GeoPoly in GeoPolygones)
            {
                GeoPoly.UpdateGeoDataFrom(Container, coordinatesSource, storeZValue, ElevationMode);
                SceneGeoData.GeoPolygons.Add(GeoPoly.PolygonGeoData);

            }

            SceneGeoData.EPSG = Container.data.EPSG;

            return SceneGeoData;
        }
        public int GetGeoDataCount()
        {
            int count = 0;
            
            count += GeoPoints.Count;
            count += GeoLines.Count;
            count += GeoPolygons.Count;

            return count;
        }
        public static void AddVectordDataToObject(GameObject obj, VectorObjectType vectorObjectType, GISTerrainLoaderPointGeoData PointGeoData = null, GISTerrainLoaderLineGeoData LineGeoData = null, GISTerrainLoaderPolygonGeoData PolyGeoData = null)
        {
            switch (vectorObjectType)
            {
                case VectorObjectType.Point:
                    var VectorObjectDataPoint = obj.AddComponent<GISTerrainLoaderVectorPoint>();
                    VectorObjectDataPoint.PointGeoData = PointGeoData;

                    break;
                case VectorObjectType.Line:
                    var VectorObjectDataLine = obj.AddComponent<GISTerrainLoaderVectorLine>();
                    VectorObjectDataLine.LineGeoData = LineGeoData;
                    break;
                case VectorObjectType.Polygon:
                    var VectorObjectDataPoly = obj.AddComponent<GISTerrainLoaderVectorPolygon>();
                    VectorObjectDataPoly.PolygonGeoData = PolyGeoData;
                    break;
            }

        }
 
        /// <summary>
        /// Parse for Data all data by key and value
        /// </summary>
        /// <param name="VectorType">Specify the vector type to not research in all data</param>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        public GISTerrainLoaderGeoVectorData GetVectorDataByKeyValue(string Key,string Value, GeoVectorType vectorType= GeoVectorType.AllTypes)
        {
            GISTerrainLoaderGeoVectorData NewData = new GISTerrainLoaderGeoVectorData();
            NewData.Name = this.Name;
            NewData.EPSG = this.EPSG;

            switch (vectorType)
            {
                case GeoVectorType.Point:
                    foreach (var point in GeoPoints)
                    {
                        if (point.DataBase.Exists(x => x.Key == Key && x.Value == Value.Trim()))
                            NewData.GeoPoints.Add(point);
                    }
                    break;

                case GeoVectorType.Line:
                    foreach (var line in GeoLines)
                    {
                        if (line.DataBase.Exists(x => x.Key == Key && x.Value == Value.Trim()))
                            NewData.GeoLines.Add(line);
                    }
                    break;
                case GeoVectorType.Polygon:
                    foreach (var poly in GeoPolygons)
                    {
                        if (poly.DataBase.Exists(x => x.Key == Key && x.Value == Value.Trim()))
                            NewData.GeoPolygons.Add(poly);
                    }
                    break;
                case GeoVectorType.AllTypes:

                    foreach (var point in GeoPoints)
                    {
                        if (point.DataBase.Exists(x => x.Key == Key && x.Value == Value.Trim()))
                            NewData.GeoPoints.Add(point);
                    }
                    foreach (var line in GeoLines)
                    {
                        if (line.DataBase.Exists(x => x.Key == Key && x.Value == Value.Trim()))
                            NewData.GeoLines.Add(line);
                    }
                    foreach (var poly in GeoPolygons)
                    {
                        if (poly.DataBase.Exists(x => x.Key == Key && x.Value == Value.Trim()))
                            NewData.GeoPolygons.Add(poly);
                    }
                    break;
            }
             return NewData;
        }
        /// <summary>
        /// Filter GeoData and keep only data inside the container bounds
        /// </summary>
        /// <param name="name"></param>
        /// <param name="container"></param>
        /// <returns></returns>
        public GISTerrainLoaderGeoVectorData GetDataInContainerBounds(GISTerrainContainer container)
        {
            GISTerrainLoaderGeoVectorData NewGeoVectorData = new GISTerrainLoaderGeoVectorData();
            NewGeoVectorData.Name = this.Name;
            NewGeoVectorData.EPSG = this.EPSG;
 
            List<GISTerrainLoaderPointGeoData> m_GeoPoints = new List<GISTerrainLoaderPointGeoData>();
            List<GISTerrainLoaderLineGeoData> m_GeoLines = new List<GISTerrainLoaderLineGeoData>();
            List<GISTerrainLoaderPolygonGeoData> m_GeoPolygons = new List<GISTerrainLoaderPolygonGeoData>();


            if (this.GeoPoints.Count > 0)
            {
                foreach (var point in this.GeoPoints)
                {
                    if (container.IncludeRealWorldPoint(point.GeoPoint))
                        m_GeoPoints.Add(point);
                }
            }


            if (this.GeoLines.Count > 0)
            {
                foreach (var line in this.GeoLines)
                {
                    GISTerrainLoaderLineGeoData newLine = new GISTerrainLoaderLineGeoData();
                    newLine.ID = line.ID; 
                    newLine.Name = line.Name; 
                    newLine.Tag = line.Tag;
                    newLine.DataBase = line.DataBase;
 
                    for (int i = 0; i < line.GeoPoints.Count; i++)
                    {
                        var point = line.GeoPoints[i];
 
                        if (container.IncludeRealWorldPoint(point.GeoPoint))
                            newLine.GeoPoints.Add(point);
                    }
 
                    if (newLine.GeoPoints.Count > 0)
                        m_GeoLines.Add(newLine);
                }
            }


            if (this.GeoPolygons.Count > 0)
            {
                foreach (var polygon in this.GeoPolygons)
                {
                    GISTerrainLoaderPolygonGeoData newpolygon = new GISTerrainLoaderPolygonGeoData();
                    newpolygon.ID = polygon.ID;
                    newpolygon.Name = polygon.Name;
                    newpolygon.Tag = polygon.Tag;
                    newpolygon.DataBase = polygon.DataBase;
 
                    int PolyCounts = polygon.GeoPoints.Count;

                    for (int i = 0; i < PolyCounts; i++)
                    {
                        var SubPoints = polygon.GeoPoints[i];

                        var NewSubPoints = new List<GISTerrainLoaderPointGeoData>();

                        for (int s = 0; s < SubPoints.Count; s++)
                        {
                            var point = SubPoints[i];

                            if (container.IncludeRealWorldPoint(point.GeoPoint))
                                NewSubPoints.Add(point);
                        }

                        if (NewSubPoints.Count > 0)
                            newpolygon.GeoPoints.Add(NewSubPoints);
                    }
                    if (newpolygon.GeoPoints.Count > 0)
                        m_GeoPolygons.Add(newpolygon);
                }
            }

            NewGeoVectorData.GeoPoints = m_GeoPoints;
            NewGeoVectorData.GeoLines = m_GeoLines;
            NewGeoVectorData.GeoPolygons = m_GeoPolygons;

            return NewGeoVectorData;
        }

        /// <summary>
        /// Check if Geodata is not null or empty
        /// </summary>
        /// <returns></returns>
        public bool IsEmptyGeoData()
        {
            bool isEmpty = true;

            if (GeoPoints.Count > 0)
                isEmpty = false;
            if (GeoLines.Count > 0)
                isEmpty = false;
            if (GeoPolygons.Count > 0)
                isEmpty = false;

            return isEmpty;
        }
    }

    #region Point
    [Serializable]
    public class GISTerrainLoaderPointGeoData 
    {
        public string ID;
        public string Name;
        public string Tag;
        public DVector2 GeoPoint;
        public float Elevation;
        [SerializeField]
        public List<GISTerrainLoaderGeoDataBase> DataBase;

        public GISTerrainLoaderPointGeoData()
        {
            ID = "";
            Name = "";
            Tag = "";
            GeoPoint = new DVector2 (0,0);

            DataBase = new List<GISTerrainLoaderGeoDataBase>();
        }
        public GISTerrainLoaderPointGeoData(DVector2 point)
        {
            ID = "";
            Name = "";
            Tag = "";
            GeoPoint = new DVector2(point.x, point.y);

            DataBase = new List<GISTerrainLoaderGeoDataBase>();
        }
        public List<string> GetDataBaseKeys()
        {
            List<string> Keys = new List<string>();
            foreach (var x in DataBase)
            {
                Keys.Add(x.Key);
            }
            return Keys;
        }
        public List<string> GetDataBaseValues()
        {
            List<string> Values = new List<string>();
            foreach (var x in DataBase)
            {
                Values.Add(x.Key);
            }
            return Values;
        }
        public bool TryGetValue(string attribute, out string value)
        {
            value = "";

            bool Contains = false;

            GISTerrainLoaderGeoDataBase element = DataBase.Find(x => x.Key == attribute);

            if (element != null)
            {
                value = element.Value;
                Contains = true;
            }

            return Contains;
        }
    }
    #endregion
    #region Line
    [Serializable]
    public class GISTerrainLoaderLineGeoData
    {
        public string ID;
        public string Name;
        public string Tag;
        public List<GISTerrainLoaderPointGeoData> GeoPoints;
        [SerializeField]
        public List<GISTerrainLoaderGeoDataBase> DataBase;
        public GISTerrainLoaderLineGeoData()
        {
            ID = "";
            Name = "";
            Tag = "";
            GeoPoints = new List<GISTerrainLoaderPointGeoData>();

            DataBase = new List<GISTerrainLoaderGeoDataBase>();

        }
        public GISTerrainLoaderLineGeoData(string m_name, string m_tag, List<GISTerrainLoaderPointGeoData> m_GeoPoints)
        {
            Name = m_name;
            Tag = m_tag;
            GeoPoints = m_GeoPoints;
        }

        public List<string> GetDataBaseKeys()
        {
            List<string> Keys = new List<string>();
            foreach (var x in DataBase)
            {
                Keys.Add(x.Key);
            }
            return Keys;
        }
        public List<string> GetDataBaseValues()
        {
            List<string> Values = new List<string>();
            foreach (var x in DataBase)
            {
                Values.Add(x.Key);
            }
            return Values;
        }
        public bool TryGetValue(string attribute, out string value)
        {
            value = "";

            bool Contains = false;

            GISTerrainLoaderGeoDataBase element = DataBase.Find(x => x.Key == attribute);

            if (element != null)
            {
                value = element.Value;
                Contains = true;
            }

            return Contains;
        }
    }
    #endregion
    #region Polygon
    [Serializable]
    public class GISTerrainLoaderPolygonGeoData
    {
        public string ID;
        public string Name;
        public string Tag;
 
        public List<List<GISTerrainLoaderPointGeoData>> GeoPoints;
        [HideInInspector]
        public List<Role> Roles;
 
        [SerializeField]
        public List<GISTerrainLoaderGeoDataBase> DataBase;

        public GISTerrainLoaderPolygonGeoData()
        {
            ID = "";
            Name = "";
            Tag = "";
            GeoPoints = new List<List<GISTerrainLoaderPointGeoData>>(0);
            DataBase = new List<GISTerrainLoaderGeoDataBase>();
            Roles = new List<Role>();
        }

        public List<string> GetDataBaseKeys()
        {
            List<string> Keys = new List<string>();
            foreach (var x in DataBase)
            {
                Keys.Add(x.Key);
            }
            return Keys;
        }
        public List<string> GetDataBaseValues()
        {
            List<string> Values = new List<string>();
            foreach (var x in DataBase)
            {
                Values.Add(x.Key);
            }
            return Values;
        }
        public bool TryGetValue(string attribute, out string value)
        {
            value = "";

            bool Contains = false;

            GISTerrainLoaderGeoDataBase element = DataBase.Find(x => x.Key.Equals(attribute.Trim()));

            if (element != null)
            {
                value = element.Value;
                Contains = true;
            }

            return Contains;
        }
        public bool IsClockwise(List<GISTerrainLoaderPointGeoData> vertices)
        {
            GISTerrainLoaderPointGeoData _v1, _v2;

            double sum = 0.0;
            var _counter = vertices.Count;
            for (int i = 0; i < _counter; i++)
            {
                _v1 = vertices[i];
                _v2 = vertices[(i + 1) % _counter];
                sum += (_v2.GeoPoint.x - _v1.GeoPoint.x) * (_v2.GeoPoint.y + _v1.GeoPoint.y);
            }

            return sum > 0.0;
        }
        public bool IsClockwise(IList<Vector3> vertices)
        {
            Vector3 _v1, _v2;

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
        public Vector3[] OrderToClockwise(Vector3[] iPoints)
        {
            double delta = 0;

            for (int pt = 0; pt < iPoints.Length; pt++)
            {
                double deltaX = 0;
                double deltaY = 0;
                if (pt == iPoints.Length - 1)
                {
                    deltaX = (iPoints[0].x - iPoints[pt].x);
                    deltaY = (iPoints[0].z + iPoints[pt].z);
                }
                else
                {
                    deltaX = (iPoints[pt + 1].x - iPoints[pt].x);
                    deltaY = (iPoints[pt + 1].z + iPoints[pt].z);
                }

                delta += deltaX * deltaY;
            }

            if (delta > 0)
            {
                // clockwise
            }
            else
            {
                // counter-clockwise.
                Array.Reverse(iPoints);
            }

            return iPoints;
        }
        public Vector3[] CounterOrderToClockwise(Vector3[] iPoints)
        {
            double delta = 0;

            for (int pt = 0; pt < iPoints.Length; pt++)
            {
                double deltaX = 0;
                double deltaY = 0;
                if (pt == iPoints.Length - 1)
                {
                    deltaX = (iPoints[0].x - iPoints[pt].x);
                    deltaY = (iPoints[0].z + iPoints[pt].z);
                }
                else
                {
                    deltaX = (iPoints[pt + 1].x - iPoints[pt].x);
                    deltaY = (iPoints[pt + 1].z + iPoints[pt].z);
                }

                delta += deltaX * deltaY;
            }

            if (delta > 0)
            {
                // clockwise
                Array.Reverse(iPoints);
            }
            else
            {
                Array.Reverse(iPoints);
                // counter-clockwise.

            }

            return iPoints;
        }
        public Vector2[] CounterOrderToClockwise(Vector2[] iPoints)
        {
            double delta = 0;

            for (int pt = 0; pt < iPoints.Length; pt++)
            {
                double deltaX = 0;
                double deltaY = 0;
                if (pt == iPoints.Length - 1)
                {
                    deltaX = (iPoints[0].x - iPoints[pt].x);
                    deltaY = (iPoints[0].y + iPoints[pt].y);
                }
                else
                {
                    deltaX = (iPoints[pt + 1].x - iPoints[pt].x);
                    deltaY = (iPoints[pt + 1].y + iPoints[pt].y);
                }

                delta += deltaX * deltaY;
            }

            if (delta > 0)
            {
                // clockwise
                Array.Reverse(iPoints);
            }
            else
            {
                Array.Reverse(iPoints);
                // counter-clockwise.

            }

            return iPoints;
        }
        public List<GISTerrainLoaderPointGeoData> CounterOrderToClockwise(List<GISTerrainLoaderPointGeoData> iPoints)
        {
            double delta = 0;

            for (int pt = 0; pt < iPoints.Count; pt++)
            {
                double deltaX = 0;
                double deltaY = 0;
                if (pt == iPoints.Count - 1)
                {
                    deltaX = (iPoints[0].GeoPoint.x - iPoints[pt].GeoPoint.x);
                    deltaY = (iPoints[0].GeoPoint.y + iPoints[pt].GeoPoint.y);
                }
                else
                {
                    deltaX = (iPoints[pt + 1].GeoPoint.x - iPoints[pt].GeoPoint.x);
                    deltaY = (iPoints[pt + 1].GeoPoint.y + iPoints[pt].GeoPoint.y);
                }

                delta += deltaX * deltaY;
            }

            if (delta > 0)
            {
                // clockwise
                Array.Reverse(iPoints.ToArray());
                
            }
            else
            {
                Array.Reverse(iPoints.ToArray());
                // counter-clockwise.

            }

            return iPoints;
        }
        public Vector2 GetMinMaxHeight(List<List<Vector3>> vertices)
        {
            int counter = vertices.Count;
            float max = float.MinValue;
            float min = float.MaxValue;
            Vector2 MaxMin = new Vector2(max, min);

            for (int i = 0; i < counter; i++)
            {
                var sub = vertices[i];

                for (int j = 0; j < sub.Count; j++)
                {
                    if (vertices[i][j].y > max)
                        max = vertices[i][j].y;
                    else if (vertices[i][j].y < min)
                        min = vertices[i][j].y;
                }
                MaxMin.x = min;
                MaxMin.y = max;
            }

            return MaxMin;
        }


        /// <summary>
        /// Convert Real World Poly Coordinates to Unity Space Coordinates
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public List<List<Vector3>> GeoPointsToSpacePoints(GISTerrainContainer container)
        {
            List<List<Vector3>> SpacePoints = new List<List<Vector3>>();

            for (int i = 0; i < GeoPoints.Count; i++)
            {
                var sub = GeoPoints[i];

                List<Vector3> sub_SpacePoints = new List<Vector3>();

                for (int j = 0; j < sub.Count; j++)
                {
                    var geopoint = sub[j].GeoPoint;
                    var spacePoint = GISTerrainLoaderGeoConversion.RealWorldCoordinatesToUnityWorldSpace(container, geopoint);
                    sub_SpacePoints.Add(spacePoint);
                }
                SpacePoints.Add(sub_SpacePoints);
            }
            
            return SpacePoints; 
        }
 
        public List<DVector2> GetMinMaxBounds()
        {
            List<DVector2> bounds = new List<DVector2>();
 
            DVector2 TL_Point = new DVector2(180,-90);
            DVector2 DR_Point = new DVector2(-180, 90);
 
            if (GeoPoints.Count == 0) return bounds;

            for(int i=0; i< GeoPoints.Count;i++)
            {
                var sublist = GeoPoints[i];

                DVector2 Sub_TL_Point = new DVector2(180, -90);
                DVector2 Sub_DR_Point = new DVector2(-180, 90);
 
                foreach (var point in sublist)
                {
                    if (point != null)
                    {
 
                        if (point.GeoPoint.x < TL_Point.x)
                            Sub_TL_Point.x = point.GeoPoint.x;
                        if (point.GeoPoint.y > TL_Point.y)
                            Sub_TL_Point.y = point.GeoPoint.y;

                        if (point.GeoPoint.x > DR_Point.x)
                            Sub_DR_Point.x = point.GeoPoint.x;
                        if (point.GeoPoint.y < DR_Point.y)
                            Sub_DR_Point.y = point.GeoPoint.y;
  
                    }
 
                }

                if (Sub_TL_Point.x < TL_Point.x)
                    TL_Point.x = Sub_TL_Point.x;
                if (Sub_TL_Point.y > TL_Point.y)
                    TL_Point.y = Sub_TL_Point.y;

                if (Sub_DR_Point.x > DR_Point.x)
                    DR_Point.x = Sub_DR_Point.x;
                if (Sub_DR_Point.y < DR_Point.y)
                    DR_Point.y = Sub_DR_Point.y;
             }

            bounds.Add(TL_Point);
            bounds.Add(DR_Point);

            return bounds;
        }

        public void SortPolyRole()
        {
            GISTerrainLoaderPolygonGeoData PolygoneGeoData = this;
            List<int> RolIndex = new List<int>();

            List<List<GISTerrainLoaderPointGeoData>> GeoPoints = new List<List<GISTerrainLoaderPointGeoData>>();

            List<Role> Roles = new List<Role>();

            for (int i = 0; i < PolygoneGeoData.GeoPoints.Count; i++)
            {
                var Role = PolygoneGeoData.Roles[i];

                if (Role == Role.Outer)
                {
                    Roles.Add(Role);
                    GeoPoints.Add(PolygoneGeoData.GeoPoints[i]);
                }

            }

            for (int i = 0; i < PolygoneGeoData.GeoPoints.Count; i++)
            {
                var Role = PolygoneGeoData.Roles[i];

                if (Role == Role.Inner)
                {
                    Roles.Add(Role);
                    GeoPoints.Add(PolygoneGeoData.GeoPoints[i]);
                }

            }

            PolygoneGeoData.GeoPoints = GeoPoints;
            PolygoneGeoData.Roles = Roles;
        }

    }
    #endregion

    #region DataBase
    [Serializable]
    public class GISTerrainLoaderGeoDataBase
    {
        public string Key;
        public string Value;

        public GISTerrainLoaderGeoDataBase()
        {
            Key = "";
            Value = "";
        }
        public GISTerrainLoaderGeoDataBase(string m_Key, string m_Value)
        {
            Key = m_Key;
            Value = m_Value;
        }
    }
    #endregion
    public abstract class GISTerrainLoaderGeoVectorExt
    {
        public abstract void UpdateGeoDataFrom(CoordinatesSource coordinatesSource);

    }
    public enum Role
    {
        Inner = 0,
        Outer = 1,
    }

    public class GISTerrainLoaderVectorEntity
    {
        public GameObject GameObject;
        public MeshFilter MeshFilter;
        public Mesh Mesh;
        public MeshRenderer MeshRenderer;
        public Transform Transform;

        public GISTerrainLoaderVectorEntity (string ObjName = "",string MeshName = "MeshData")
        {
            var go = new GameObject(ObjName);
            var mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = new Mesh();
            mf.sharedMesh.name = MeshName;
            var mr = go.AddComponent<MeshRenderer>();

            GameObject = go;
            Transform = go.transform;
            MeshFilter = mf;
            MeshRenderer = mr;
            Mesh = mf.sharedMesh;
 
        }
        public void SetMaterial(List<Material> Materials)
        {
            var min = Math.Min(Materials.Count, MeshFilter.sharedMesh.subMeshCount);

            float renderMode = 0.0f;

            for (int i = 0; i < min; i++)
            {
                Materials[i].SetFloat("_Mode", renderMode);
            }
            MeshRenderer.materials = Materials.ToArray();
        }
        public void Apply(GISTerrainLoaderMeshData meshData, Transform Parent = null,float Yoffset =0, float UVScale = 0)
        {
            if (GameObject == null) return;
            int _counter = 0;
            Mesh.Clear();
            Mesh.subMeshCount = meshData.Triangles.Count;
            Mesh.SetVertices(meshData.Vertices);
            Mesh.SetNormals(meshData.Normals);

            if (meshData.Tangents.Count > 0) Mesh.SetTangents(meshData.Tangents);

            _counter = meshData.Triangles.Count;

            for (int f = 0; f < _counter; f++) Mesh.SetTriangles(meshData.Triangles[f], f);


            _counter = meshData.UV.Count;

            for (int f = 0; f < _counter; f++)
                Mesh.SetUVs(f, meshData.UV[f]);

            Transform.localPosition = meshData.CentrePosition;

            if (Parent)
                Transform.SetParent(Parent, true);

            if(Yoffset>0)
            Transform.transform.position += new Vector3(0, Yoffset, 0);

            if (UVScale>0)
                ApplyMeshUV(meshData, UVScale);
        }
        private void ApplyMeshUV(GISTerrainLoaderMeshData meshData,float UVScale=100)
        {
            Vector2[] uvs = new Vector2[meshData.Vertices.Count];

            for (int i = 0; i < uvs.Length; i++)
            {
                uvs[i] = new Vector2(meshData.Vertices[i].x / UVScale, meshData.Vertices[i].z / UVScale);
            }
            Mesh.uv = uvs;
        }
    }
    public struct GISTerrainLoaderEdge3D
    {

        public Vector3 a;
        public Vector3 b;

        public override bool Equals(object obj)
        {
            if (obj is GISTerrainLoaderEdge3D)
            {
                var edge = (GISTerrainLoaderEdge3D)obj;
                //An edge is equal regardless of which order it's points are in
                return (edge.a == a && edge.b == b) || (edge.b == a && edge.a == b);
            }

            return false;

        }

        public override int GetHashCode()
        {
            return a.GetHashCode() ^ b.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("[" + a.x + "," + a.z + "->" + b.x + "," + b.z + "]");
        }

    }
}